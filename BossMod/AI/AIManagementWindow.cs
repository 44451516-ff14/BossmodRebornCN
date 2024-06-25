﻿using ImGuiNET;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private readonly AIConfig _config;
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100, 100))
    {
        WindowName = _title;
        _config = Service.Config.Get<AIConfig>();
        _manager = manager;
        _subscriptions = new
        (
            _config.Modified.ExecuteAndSubscribe(() => IsOpen = _config.DrawUI)
        );
        RespectCloseHotkey = false;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public void SetVisible(bool vis)
    {
        if (_config.DrawUI != vis)
        {
            _config.DrawUI = vis;
            _config.Modified.Fire();
        }
    }

    public override void Draw()
    {
        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos} / {_manager.Controller.NaviTargetRot}{(_manager.Controller.ForceFacing ? " forced" : "")}");
        _manager.Beh?.DrawDebug();
        if (ImGui.Button("AI on/update followed slot"))
        {
            _manager.SwitchToFollow(_config.FollowSlot);
            _config.Enabled = true;
            _config.Modified.Fire();
        }
        ImGui.SameLine();
        if (ImGui.Button("AI off"))
            _manager.SwitchToIdle();
        ImGui.Text("Follow party slot");
        ImGui.SameLine();
        var partyMemberNames = new List<string>();
        for (var i = 0; i < 8; i++)
        {
            var member = _manager.Autorot.WorldState.Party[i];
            if (member != null)
                partyMemberNames.Add(member.Name);
            else
                partyMemberNames.Add($"Slot {i + 1}");
        }
        var partyMemberNamesArray = partyMemberNames.ToArray();

        if (ImGui.Combo("##FollowPartySlot", ref _config.FollowSlot, partyMemberNamesArray, partyMemberNamesArray.Length))
            _config.Modified.Fire();
        ImGui.Text("Desired positional");
        ImGui.SameLine();
        var positionalOptions = Enum.GetNames(typeof(Positional));
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionalOptions, positionalOptions.Length))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            _config.Modified.Fire();
        }
        using (var presetCombo = ImRaii.Combo("AI preset", _manager.AiPreset?.Name ?? ""))
        {
            if (presetCombo)
            {
                foreach (var p in _manager.Autorot.Database.Presets.Presets)
                {
                    if (ImGui.Selectable(p.Name, p == _manager.AiPreset))
                    {
                        _manager.AiPreset = p;
                        if (_manager.Beh != null)
                            _manager.Beh.AIPreset = p;
                    }
                }
            }
        }
    }

    public override void OnClose() => SetVisible(false);

    public void UpdateTitle() => WindowName = $"AI: {(_manager.Beh != null ? "on" : "off")}, master={_manager.Autorot.WorldState.Party[_manager.MasterSlot]?.Name}{_windowID}";
}
