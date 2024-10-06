using BossMod.Autorotation;
using Dalamud.Common;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.IO;
using System.Reflection;

namespace BossMod;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "BossMod Reborn";

    private ICommandManager CommandManager { get; init; }

    private readonly RotationDatabase _rotationDB;
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly BossModuleManager _bossmod;
    private readonly AIHintsBuilder _hintsBuilder;
    private readonly MovementOverride _movementOverride;
    private readonly ActionManagerEx _amex;
    private readonly WorldStateGameSync _wsSync;
    private readonly RotationModuleManager _rotation;
    private readonly AI.AIManager _ai;
    private readonly AI.Broadcast _broadcast;
    private readonly IPCProvider _ipc;
    private readonly DTRProvider _dtr;
    private TimeSpan _prevUpdateTime;

    // windows
    private readonly ConfigUI _configUI; // TODO: should be a proper window!
    private readonly BossModuleMainWindow _wndBossmod;
    private readonly BossModuleHintsWindow _wndBossmodHints;
    private readonly ReplayManagementWindow _wndReplay;
    private readonly UIRotationWindow _wndRotation;
    private readonly MainDebugWindow _wndDebug;

    public unsafe Plugin(IDalamudPluginInterface dalamud, ICommandManager commandManager, ISigScanner sigScanner, IDataManager dataManager)
    {
        if (!dalamud.ConfigDirectory.Exists)
            dalamud.ConfigDirectory.Create();
        var dalamudRoot = dalamud.GetType().Assembly.
                GetType("Dalamud.Service`1", true)!.MakeGenericType(dalamud.GetType().Assembly.GetType("Dalamud.Dalamud", true)!).
                GetMethod("Get")!.Invoke(null, BindingFlags.Default, null, [], null);
        var dalamudStartInfo = dalamudRoot?.GetType().GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(dalamudRoot) as DalamudStartInfo;
        var gameVersion = dalamudStartInfo?.GameVersion?.ToString() ?? "unknown";
        InteropGenerator.Runtime.Resolver.GetInstance.Setup(sigScanner.SearchBase, gameVersion, new(dalamud.ConfigDirectory.FullName + "/cs.json"));
        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        InteropGenerator.Runtime.Resolver.GetInstance.Resolve();

        dalamud.Create<Service>();
        Service.LogHandler = (string msg) => Service.Logger.Debug(msg);
        Service.LuminaGameData = dataManager.GameData;
        Service.WindowSystem = new("bmr");
        //Service.Device = pluginInterface.UiBuilder.Device;
        Service.Condition.ConditionChange += OnConditionChanged;
        MultiboxUnlock.Exec();
        Network.IDScramble.Initialize();
        Camera.Instance = new();

        Service.Config.Initialize();
        Service.Config.LoadFromFile(dalamud.ConfigFile);
        Service.Config.Modified.Subscribe(() => Service.Config.SaveToFile(dalamud.ConfigFile));

        CommandManager = commandManager;
        CommandManager.AddHandler("/bmr", new CommandInfo(OnCommand) { HelpMessage = "Show boss mod settings UI" });
        CommandManager.AddHandler("/vbm", new CommandInfo(OnCommand) { ShowInHelp = false });

        ActionDefinitions.Instance.UnlockCheck = QuestUnlocked; // ensure action definitions are initialized and set unlock check functor (we don't really store the quest progress in clientstate, for now at least)

        var qpf = (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        _rotationDB = new(new(dalamud.ConfigDirectory.FullName + "/autorot"), new(dalamud.AssemblyLocation.DirectoryName! + "/DefaultRotationPresets.json"));
        _ws = new(qpf, gameVersion);
        _hints = new();
        _bossmod = new(_ws);
        _hintsBuilder = new(_ws, _bossmod);
        _movementOverride = new();
        _amex = new(_ws, _hints, _movementOverride);
        _wsSync = new(_ws, _amex);
        _rotation = new(_rotationDB, _bossmod, _hints);
        _ai = new(_rotation, _amex, _movementOverride);
        _broadcast = new();
        _ipc = new(_rotation, _amex, _movementOverride, _ai);
        _dtr = new(_rotation, _ai);
        _wndBossmod = new(_bossmod);
        _wndBossmodHints = new(_bossmod);
        var config = Service.Config.Get<ReplayManagementConfig>();
        var replayDir = string.IsNullOrEmpty(config.ReplayFolder) ? dalamud.ConfigDirectory.FullName + "/replays" : config.ReplayFolder;
        _wndReplay = new ReplayManagementWindow(_ws, _rotationDB, new DirectoryInfo(replayDir));
        _configUI = new(Service.Config, _ws, new DirectoryInfo(replayDir), _rotationDB);
        config.Modified.ExecuteAndSubscribe(() => _wndReplay.UpdateLogDirectory());
        _wndRotation = new(_rotation, _amex, () => OpenConfigUI("Autorotatiion presets"));
        _wndDebug = new(_ws, _rotation, _amex, _hintsBuilder, dalamud);

        dalamud.UiBuilder.DisableAutomaticUiHide = true;
        dalamud.UiBuilder.Draw += DrawUI;
        dalamud.UiBuilder.OpenConfigUi += () => OpenConfigUI();

        _ = new ConfigChangelogWindow();
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChanged;
        _wndDebug.Dispose();
        _wndRotation.Dispose();
        _wndReplay.Dispose();
        _wndBossmodHints.Dispose();
        _wndBossmod.Dispose();
        _configUI.Dispose();
        _ipc.Dispose();
        _ai.Dispose();
        _rotation.Dispose();
        _wsSync.Dispose();
        _amex.Dispose();
        _movementOverride.Dispose();
        _hintsBuilder.Dispose();
        _bossmod.Dispose();
        _dtr.Dispose();
        ActionDefinitions.Instance.Dispose();
        CommandManager.RemoveHandler("/bmr");
        CommandManager.RemoveHandler("/vbm");
    }

    private void OnCommand(string cmd, string args)
    {
        Service.Log($"OnCommand: {cmd} {args}");
        var split = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0)
        {
            OpenConfigUI();
            return;
        }

        switch (split[0].ToUpperInvariant())
        {
            case "D":
                _wndDebug.IsOpen = true;
                _wndDebug.BringToFront();
                break;
            case "CFG":
                var output = Service.Config.ConsoleCommand(new ArraySegment<string>(split, 1, split.Length - 1));
                foreach (var msg in output)
                    Service.ChatGui.Print(msg);
                break;
            case "GC":
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                break;
            case "R":
                HandleReplayCommand(split);
                break;
            case "AR":
                ParseAutorotationCommands(split);
                break;
            case "RESETCOLORS":
                ResetColors();
                break;
            case "RESTOREROTATION":
                ToggleRestoreRotation();
                break;
            case "TOGGLEANTICHEAT":
                ToggleAnticheat();
                break;
        }
    }

    private bool HandleReplayCommand(string[] messageData)
    {
        if (messageData.Length == 1)
            _wndReplay.SetVisible(!_wndReplay.IsOpen);
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _wndReplay.StartRecording();
                    break;
                case "OFF":
                    _wndReplay.StopRecording();
                    break;
                default:
                    Service.ChatGui.Print($"[BMR] Unknown replay command: {messageData[1]}");
                    break;
            }
        }
        return false;
    }

    private static void ResetColors()
    {
        var defaultConfig = ColorConfig.DefaultConfig;
        var currentConfig = Service.Config.Get<ColorConfig>();

        currentConfig.PlannerBackground = defaultConfig.PlannerBackground;
        currentConfig.PlannerBackgroundHighlight = defaultConfig.PlannerBackgroundHighlight;
        currentConfig.PlannerCooldown = defaultConfig.PlannerCooldown;
        currentConfig.PlannerFallback = defaultConfig.PlannerFallback;
        currentConfig.PlannerEffect = defaultConfig.PlannerEffect;
        currentConfig.PlannerWindow = defaultConfig.PlannerWindow;
        currentConfig.ArenaBackground = defaultConfig.ArenaBackground;
        currentConfig.ArenaBorder = defaultConfig.ArenaBorder;
        currentConfig.ArenaAOE = defaultConfig.ArenaAOE;
        currentConfig.ArenaSafeFromAOE = defaultConfig.ArenaSafeFromAOE;
        currentConfig.ArenaDanger = defaultConfig.ArenaDanger;
        currentConfig.ArenaSafe = defaultConfig.ArenaSafe;
        currentConfig.ArenaTrap = defaultConfig.ArenaTrap;
        currentConfig.ArenaPC = defaultConfig.ArenaPC;
        currentConfig.ArenaEnemy = defaultConfig.ArenaEnemy;
        currentConfig.ArenaObject = defaultConfig.ArenaObject;
        currentConfig.ArenaPlayerInteresting = defaultConfig.ArenaPlayerInteresting;
        currentConfig.ArenaPlayerGeneric = defaultConfig.ArenaPlayerGeneric;
        currentConfig.ArenaVulnerable = defaultConfig.ArenaVulnerable;
        currentConfig.ArenaFutureVulnerable = defaultConfig.ArenaFutureVulnerable;
        currentConfig.ArenaMeleeRangeIndicator = defaultConfig.ArenaMeleeRangeIndicator;
        currentConfig.ArenaOther = defaultConfig.ArenaOther;
        currentConfig.Shadows = defaultConfig.Shadows;
        currentConfig.WaymarkA = defaultConfig.WaymarkA;
        currentConfig.WaymarkB = defaultConfig.WaymarkB;
        currentConfig.WaymarkC = defaultConfig.WaymarkC;
        currentConfig.WaymarkD = defaultConfig.WaymarkD;
        currentConfig.Waymark1 = defaultConfig.Waymark1;
        currentConfig.Waymark2 = defaultConfig.Waymark2;
        currentConfig.Waymark3 = defaultConfig.Waymark3;
        currentConfig.Waymark4 = defaultConfig.Waymark4;
        currentConfig.ButtonPushColor = defaultConfig.ButtonPushColor;
        currentConfig.TextColors = defaultConfig.TextColors;
        currentConfig.PositionalColors = defaultConfig.PositionalColors;
        currentConfig.ArenaPlayerGenericTank = defaultConfig.ArenaPlayerGenericTank;
        currentConfig.ArenaPlayerGenericHealer = defaultConfig.ArenaPlayerGenericHealer;
        currentConfig.ArenaPlayerGenericPhysRanged = defaultConfig.ArenaPlayerGenericPhysRanged;
        currentConfig.ArenaPlayerGenericCaster = defaultConfig.ArenaPlayerGenericCaster;
        currentConfig.ArenaPlayerGenericMelee = defaultConfig.ArenaPlayerGenericMelee;
        currentConfig.ArenaPlayerGenericFocus = defaultConfig.ArenaPlayerGenericFocus;
        currentConfig.Modified.Fire();
        Service.Log("Colors have been reset to default values.");
    }

    private static bool ToggleAnticheat()
    {
        var config = Service.Config.Get<ActionTweaksConfig>();
        config.ActivateAnticheat = !config.ActivateAnticheat;
        config.Modified.Fire();
        Service.Log($"The animation lock anticheat is now {(config.ActivateAnticheat ? "enabled" : "disabled")}");
        return true;
    }

    private static bool ToggleRestoreRotation()
    {
        var config = Service.Config.Get<ActionTweaksConfig>();
        config.RestoreRotation = !config.RestoreRotation;
        config.Modified.Fire();
        Service.Log($"Restore character orientation after action use is now {(config.RestoreRotation ? "enabled" : "disabled")}");
        return true;
    }

    private void OpenConfigUI(string showTab = "")
    {
        _configUI.ShowTab(showTab);
        _ = new UISimpleWindow("BossModReborn", _configUI.Draw, true, new(300, 300));
    }

    private void DrawUI()
    {
        var tsStart = DateTime.Now;

        _dtr.Update();
        Camera.Instance?.Update();
        _wsSync.Update(_prevUpdateTime);
        _bossmod.Update();
        _hintsBuilder.Update(_hints, PartyState.PlayerSlot);
        _amex.QueueManualActions();
        var userPreventingCast = _movementOverride.IsMoveRequested() && !_amex.Config.PreventMovingWhileCasting;
        _rotation.Update(_amex.AnimationLockDelayEstimate, userPreventingCast ? 0 : _ai.ForceMovementIn, _movementOverride.IsMoving());
        _ai.Update();
        _broadcast.Update();
        _amex.FinishActionGather();
        ExecuteHints();

        var uiHidden = Service.GameGui.GameUiHidden || Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.WatchingCutscene];
        if (!uiHidden)
        {
            Service.WindowSystem?.Draw();
        }

        Camera.Instance?.DrawWorldPrimitives();
        _prevUpdateTime = DateTime.Now - tsStart;
    }

    private unsafe bool QuestUnlocked(uint link)
    {
        // see ActionManager.IsActionUnlocked
        var gameMain = FFXIVClientStructs.FFXIV.Client.Game.GameMain.Instance();
        return link == 0
            || Service.LuminaRow<Lumina.Excel.GeneratedSheets.TerritoryType>(gameMain->CurrentTerritoryTypeId)?.TerritoryIntendedUse == 31 // deep dungeons check is hardcoded in game
            || FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(link);
    }

    private unsafe void ExecuteHints()
    {
        _movementOverride.DesiredDirection = _hints.ForcedMovement;
        // update forced target, if needed (TODO: move outside maybe?)
        if (_hints.ForcedTarget != null)
        {
            var obj = _hints.ForcedTarget.SpawnIndex >= 0 ? FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[_hints.ForcedTarget.SpawnIndex].Value : null;
            if (obj != null && obj->EntityId != _hints.ForcedTarget.InstanceID)
                Service.Log($"[ExecHints] Unexpected new target: expected {_hints.ForcedTarget.InstanceID:X} at #{_hints.ForcedTarget.SpawnIndex}, but found {obj->EntityId:X}");
            FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->Target = obj;
        }
        foreach (var s in _hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : 0xE0000000);
            Service.Log($"[ExecHints] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }
    }

    private void ParseAutorotationCommands(string[] cmd)
    {
        switch (cmd.Length > 1 ? cmd[1].ToUpperInvariant() : "")
        {
            case "CLEAR":
                Service.Log($"Console: clearing autorotation preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = null;
                break;
            case "DISABLE":
                Service.Log($"Console: force-disabling from preset '{_rotation.Preset?.Name ?? "<n/a>"}'");
                _rotation.Preset = RotationModuleManager.ForceDisable;
                break;
            case "SET":
                if (cmd.Length <= 2)
                    PrintAutorotationHelp();
                else
                    ParseAutorotationSetCommand(cmd[2], false);
                break;
            case "TOGGLE":
                ParseAutorotationSetCommand(cmd.Length > 2 ? cmd[2] : "", true);
                break;
            default:
                PrintAutorotationHelp();
                break;
        }
    }

    private void ParseAutorotationSetCommand(string presetName, bool toggle)
    {
        var preset = presetName.Length > 0 ? _rotation.Database.Presets.Presets.FirstOrDefault(p => p.Name == presetName) : RotationModuleManager.ForceDisable;
        if (preset != null)
        {
            var newPreset = toggle && _rotation.Preset == preset ? null : preset;
            Service.Log($"Console: {(toggle ? "toggle" : "set")} changes preset from '{_rotation.Preset?.Name ?? "<n/a>"}' to '{newPreset?.Name ?? "<n/a>"}'");
            _rotation.Preset = newPreset;
        }
        else
        {
            Service.ChatGui.PrintError($"Failed to find preset '{presetName}'");
        }
    }

    private static void PrintAutorotationHelp()
    {
        Service.ChatGui.Print("Autorotation commands:");
        Service.ChatGui.Print("* /vbm ar clear - clear current preset; autorotation will do nothing unless plan is active");
        Service.ChatGui.Print("* /vbm ar disable - force disable autorotation; no actions will be executed automatically even if plan is active");
        Service.ChatGui.Print("* /vbm ar set Preset - start executing specified preset");
        Service.ChatGui.Print("* /vbm ar toggle - force disable autorotation if not already; otherwise clear overrides");
        Service.ChatGui.Print("* /vbm ar toggle Preset - start executing specified preset unless it's already active; clear otherwise");
    }

    private static void OnConditionChanged(ConditionFlag flag, bool value)
    {
        Service.Log($"Condition change: {flag}={value}");
    }
}
