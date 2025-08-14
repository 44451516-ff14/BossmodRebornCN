using BossMod.Autorotation;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using Lumina.Excel.Sheets;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class ReplayManagementWindow : UIWindow
{
    private readonly WorldState _ws;
    private DirectoryInfo _logDir;
    private static readonly ReplayManagementConfig _config = Service.Config.Get<ReplayManagementConfig>();
    private readonly ReplayManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private readonly BossModuleManager _bmm;
    private ReplayRecorder? _recorder;
    private string _message = "";
    private bool _recordingManual; // recording was started manually, and so should not be stopped automatically
    private bool _recordingDuty; // recording was started automatically because we've entered duty
    private int _recordingActiveModules; // recording was started automatically, because we've activated N modules
    private FileDialog? _folderDialog;
    private string _lastErrorMessage = "";
    private DalamudLinkPayload? _startLinkPayload;
    private DalamudLinkPayload? _uploadLinkPayload;
    private DalamudLinkPayload? _disableAlertLinkPayload;

    private const string _windowID = "###Replay recorder";

    public ReplayManagementWindow(WorldState ws, BossModuleManager bmm, RotationDatabase rotationDB, DirectoryInfo logDir) : base(_windowID, false, new(300, 200))
    {
        _ws = ws;
        _logDir = logDir;
        _manager = new(rotationDB, logDir.FullName);
        _bmm = bmm;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() =>
            {
                IsOpen = _config.ShowUI;
                UpdateLogDirectory();
            }),
            _ws.CurrentZoneChanged.Subscribe(op => OnZoneChange(op.CFCID)),
            _bmm.ModuleActivated.Subscribe(OnModuleActivation),
            _bmm.ModuleDeactivated.Subscribe(OnModuleDeactivation)
        );
        if (!OnZoneChange(_ws.CurrentCFCID))
            UpdateTitle();

        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _recorder?.Dispose();
        _subscriptions.Dispose();
        _manager.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.ShowUI != vis)
        {
            _config.ShowUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void PreOpenCheck()
    {
        _manager.Update();
    }

    public override void Draw()
    {
        if (ImGui.Button(!IsRecording() ? "Start recording" : "Stop recording"))
        {
            if (!IsRecording())
            {
                _recordingManual = true;
                StartRecording("");
            }
            else
            {
                StopRecording();
            }
        }
        ImGui.SameLine();
        if (ImGui.Button("Select replay folder"))
        {
            _folderDialog ??= new FileDialog("select_replay_folder", "Select replay folder", "", _config.ReplayFolder, "", "", 1, false, ImGuiFileDialogFlags.SelectOnly);
            _folderDialog.Show();
        }

        if (_folderDialog?.Draw() ?? false)
        {
            if (_folderDialog.GetIsOk())
            {
                _config.ReplayFolder = _folderDialog.GetResults().FirstOrDefault() ?? "";
                _config.Modified.Fire();
            }
            _folderDialog.Hide();
            _folderDialog = null;
        }
        if (_recorder != null)
        {
            ImGui.InputText("###msg", ref _message, 1024);
            ImGui.SameLine();
            if (ImGui.Button("Add log marker") && _message.Length > 0)
            {
                _ws.Execute(new WorldState.OpUserMarker(_message));
                _message = "";
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Open replay folder") && _logDir != null)
            _lastErrorMessage = OpenDirectory(_logDir);

        if (_lastErrorMessage.Length > 0)
        {
            ImGui.SameLine();
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor3);
            ImGui.TextUnformatted(_lastErrorMessage);
        }

        ImGui.Separator();
        _manager.Draw();
    }

    public bool IsRecording() => _recorder != null;

    public override void OnClose()
    {
        SetVisible(false);
    }

    private void UpdateTitle() => WindowName = $"Replay recording: {(_recorder != null ? "in progress..." : "idle")}{_windowID}";

    public bool ShouldAutoRecord => _config.AutoRecord && (_config.AutoARR || !Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.DutyRecorderPlayback]);

    private bool OnZoneChange(uint cfcId)
    {
        if (_config.ImportantDutyAlert && IsImportantDuty(cfcId) && !ShouldAutoRecord)
        {
            _startLinkPayload ??= Service.PluginInterface.AddChatLinkHandler(0, (id, str) =>
            {
                if (id == 0)
                {
                    StartRecording("");
                    Service.ChatGui.Print("[BMR] Replay recording started");
                }
            });
            _disableAlertLinkPayload ??= Service.PluginInterface.AddChatLinkHandler(2, (id, str) =>
            {
                if (id == 2)
                {
                    _config.ImportantDutyAlert = false;
                    _config.Modified.Fire();
                    Service.ChatGui.Print("[BMR] Important duty alert disabled");
                }
            });
            var alertPayload =
                new TextPayload("[BMR] This duty does not yet have a complete module. Recording and uploading a replay will help enable module creation. ");
            var linkTextPayload = new TextPayload("[Start replay recording]");
            var disableTextPayload = new TextPayload("[Permanently disable these alerts]");

            var seString = new SeStringBuilder()
                .Add(alertPayload)
                .Add(_startLinkPayload)
                .Add(linkTextPayload)
                .Add(RawPayload.LinkTerminator)
                .AddText(" ")
                .Add(_disableAlertLinkPayload)
                .Add(disableTextPayload)
                .Add(RawPayload.LinkTerminator)
                .Build();
            Service.ChatGui.Print(seString);
        }
        if (!ShouldAutoRecord || _recordingManual)
            return false; // don't care

        var isDuty = cfcId != 0;
        if (_recordingDuty == isDuty)
            return false; // don't care
        _recordingDuty = isDuty;

        if (isDuty && !IsRecording())
        {
            StartRecording("");
            return true;
        }

        if (!isDuty && _recordingActiveModules <= 0 && IsRecording())
        {
            StopRecording();
            return true;
        }

        return false;
    }

    private static readonly Dictionary<uint, bool> StaticDutyImportance = GenerateStaticDutyMap();

    private static Dictionary<uint, bool> GenerateStaticDutyMap()
    {
        var map = new Dictionary<uint, bool>();

        uint[] alwaysImportantDuties = [280u, 539u, 694u, 788u, 908u, 1006u, 700u, 736u, 779u, 878u, 879u, 946u, 947u, 979u, 980u,
        801u, 807u, 809u, 811u, 873u, 877u, 881u, 884u, 937u, 939u, 941u, 943u];
        for (var i = 0; i < 27; ++i)
        {
            map[alwaysImportantDuties[i]] = true;
        }

        // ignored duties
        uint[] alwaysIgnoredDuties = [0u, 650u, 127u, 130u, 195u, 756u, 180u, 701u, 473u, 721u];
        for (var i = 0; i < 10; ++i)
        {
            map[alwaysIgnoredDuties[i]] = false;
        }

        static void AddRange(Dictionary<uint, bool> dict, uint start, uint end)
        {
            for (var i = start; i <= end; ++i)
            {
                dict[i] = false;
            }
        }

        AddRange(map, 197, 199); // lord of verminion
        AddRange(map, 481, 535); // chocobo races
        AddRange(map, 552, 579); // lord of verminion
        AddRange(map, 599, 608); // hidden gorge + leap of faith
        AddRange(map, 640, 645); // air force one + mahjong
        AddRange(map, 705, 713); // leap of faith
        AddRange(map, 730, 734); // ocean fishing
        AddRange(map, 766, 775); // mahjong + ocean fishing
        AddRange(map, 835, 843); // crystalline conflict
        AddRange(map, 847, 864); // crystalline conflict
        AddRange(map, 912, 923); // crystalline conflict
        AddRange(map, 927, 935); // leap of faith
        AddRange(map, 952, 957); // ocean fishing
        AddRange(map, 967, 978); // crystalline conflict
        AddRange(map, 1012, 1014); // tutorial

        // check modules for WIP and non existing
        foreach (var module in BossModuleRegistry.RegisteredModules.Values)
        {
            if (module.Maturity != BossModuleInfo.Maturity.WIP)
            {
                var id = module.GroupID;
                if (!map.ContainsKey(id))
                {
                    map[id] = false;
                }
            }
        }

        return map;
    }

    private bool IsImportantDuty(uint cfcId)
    {
        if (StaticDutyImportance.TryGetValue(cfcId, out var isImportant))
        {
            return isImportant;
        }
        return true;
    }

    private void OnModuleActivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual)
            return; // don't care

        ++_recordingActiveModules;
        if (!IsRecording())
            StartRecording($"{m.GetType().Name}-");
    }

    private void OnModuleDeactivation(BossModule m)
    {
        if (!ShouldAutoRecord || _recordingManual || _recordingActiveModules <= 0)
            return; // don't care

        --_recordingActiveModules;
        if (_recordingActiveModules <= 0 && !_recordingDuty && IsRecording())
            StopRecording();
    }

    public void StartRecording(string prefix)
    {
        if (IsRecording())
            return; // already recording

        // if there are too many replays, delete oldest
        if (_config.MaxReplays > 0)
        {
            try
            {
                var replayFolder = new DirectoryInfo(_config.ReplayFolder);
                var replays = replayFolder.GetFiles();
                replays.Sort((a, b) => a.LastWriteTime.CompareTo(b.LastWriteTime));
                foreach (var f in replays.Take(replays.Length - _config.MaxReplays))
                    f.Delete();
            }
            catch (Exception ex)
            {
                Service.Log($"Failed to delete old replays: {ex}");
            }
        }

        try
        {
            var replayFolder = string.IsNullOrEmpty(_config.ReplayFolder) ? _logDir : new DirectoryInfo(_config.ReplayFolder);
            _recorder = new(_ws, _config.WorldLogFormat, true, replayFolder, prefix + GetPrefix());
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to start recording: {ex}");
        }

        UpdateTitle();
    }

    public void StopRecording()
    {
        if (_config.ImportantDutyAlert && IsImportantDuty(_recorder?.CFCID ?? 0))
        {
            var path = _recorder?.LogPath;
            _uploadLinkPayload ??= Service.PluginInterface.AddChatLinkHandler(1, (id, str) =>
            {
                if (id == 1)
                {
                    Task.Run(() =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "https://forms.gle/z6czgekaEnFBtgbB6",
                            UseShellExecute = true
                        });
                    });
                    Service.ChatGui.Print($"[BMR] The path to your replay is: {path}");
                }
            });
            var alertPayload =
                new TextPayload(
                    "[BMR] You recorded a duty without a complete module. Uploading this replay helps with module development. ");
            var linkTextPayload = new TextPayload("[Upload the replay]");
            var seString = new SeStringBuilder().Add(alertPayload).Add(_uploadLinkPayload).Add(linkTextPayload).Add(RawPayload.LinkTerminator).Build();
            Service.ChatGui.Print(seString);
        }
        _recordingManual = false;
        _recordingDuty = false;
        _recordingActiveModules = 0;
        _recorder?.Dispose();
        _recorder = null;
        UpdateTitle();
    }

    public void UpdateLogDirectory()
    {
        var newLogDir = string.IsNullOrEmpty(_config.ReplayFolder) ? _logDir : new DirectoryInfo(_config.ReplayFolder);
        _logDir = newLogDir;
        _manager.SetLogDirectory(_logDir.FullName);
    }

    private unsafe string GetPrefix()
    {
        string? prefix = null;
        if (_ws.CurrentCFCID != default)
            prefix = Service.LuminaRow<ContentFinderCondition>(_ws.CurrentCFCID)?.Name.ToString();
        if (_ws.CurrentZone != default)
            prefix ??= Service.LuminaRow<TerritoryType>(_ws.CurrentZone)?.PlaceName.ValueNullable?.NameNoArticle.ToString();
        prefix ??= "World";
        prefix = Utils.StringToIdentifier(prefix);

        var player = _ws.Party.Player();
        if (player != null)
        {
            prefix += "_" + player.Class + player.Level + "_";

            var nameParts = player.Name.Split(' ');
            List<string> shortenedParts = [];
            var nameLen = nameParts.Length;
            for (var i = 0; i < nameLen; ++i)
            {
                ref var part = ref nameParts[i];
                var len = Math.Min(2, part.Length);
                shortenedParts.Add(part[..len]);
            }

            prefix += string.Join("_", shortenedParts);
        }


        var cf = FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder.Instance();
        if (cf->IsUnrestrictedParty)
            prefix += "_U";
        if (cf->IsLevelSync)
            prefix += "_LS";
        if (cf->IsMinimalIL)
            prefix += "_MI";
        if (cf->IsSilenceEcho)
            prefix += "_NE";

        return prefix;
    }

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"Directory '{dir}' not found.";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"Failed to open folder '{dir}', open it manually.";
        }
    }
}
