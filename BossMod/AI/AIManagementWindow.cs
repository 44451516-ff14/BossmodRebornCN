using ImGuiNET;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI: off{_windowID}";
    private const string _windowID = "###AI debug window";
    private static readonly string[] positionals = Enum.GetNames<Positional>();

    public AIManagementWindow(AIManager manager) : base(_windowID, false, new(100f, 100f))
    {
        WindowName = _title;
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
        var configModified = false;

        ImGui.TextUnformatted($"Navi={_manager.Controller.NaviTargetPos}");

        configModified |= ImGui.Checkbox("禁用技能", ref _config.ForbidActions);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("禁用移动-Forbid movement", ref _config.ForbidMovement);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("骑乘时保持静止-Idle while mounted", ref _config.ForbidAIMovementMounted);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("战斗中跟随", ref _config.FollowDuringCombat);
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("BOSS模块激活时跟随", ref _config.FollowDuringActiveBossModule);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("脱战时跟随", ref _config.FollowOutOfCombat);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("跟随目标", ref _config.FollowTarget);
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("手动目标选择", ref _config.ManualTarget);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("禁用障碍物地图加载", ref _config.DisableObstacleMaps);

        ImGui.Text("跟随队伍成员位置");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        if (ImRaii.Combo("##Leader", _manager.Beh == null ? "<idle>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<unknown>"))
        {
            if (ImGui.Selectable("<idle>", _manager.Beh == null))
                _manager.SwitchToIdle();
            foreach (var (i, p) in _manager.WorldState.Party.WithSlot(true))
            {
                if (ImGui.Selectable(p.Name, _manager.MasterSlot == i))
                {
                    _manager.SwitchToFollow(i);
                    _config.FollowSlot = i;
                    configModified = true;
                }
            }
            ImGui.EndCombo();
        }
        ImGui.Separator();
        ImGui.Text("期望站位-Desired positional");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var positionalIndex = (int)_config.DesiredPositional;
        if (ImGui.Combo("##DesiredPositional", ref positionalIndex, positionals, 4))
        {
            _config.DesiredPositional = (Positional)positionalIndex;
            configModified = true;
        }
        ImGui.SameLine();
        ImGui.Text("最大距离 - 至目标");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100);
        var maxDistanceTargetStr = _config.MaxDistanceToTarget.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToTarget", ref maxDistanceTargetStr, 64u))
        {
            maxDistanceTargetStr = maxDistanceTargetStr.Replace(',', '.');
            if (float.TryParse(maxDistanceTargetStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToTarget = maxDistance;
                configModified = true;
            }
        }
        ImGui.SameLine();
        ImGui.Text("- to slots");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var maxDistanceSlotStr = _config.MaxDistanceToSlot.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToSlot", ref maxDistanceSlotStr, 64u))
        {
            maxDistanceSlotStr = maxDistanceSlotStr.Replace(',', '.');
            if (float.TryParse(maxDistanceSlotStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToSlot = maxDistance;
                configModified = true;
            }
        }

        ImGui.Text("移动决策延迟-Movement decision delay");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var movementDelayStr = _config.MoveDelay.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MovementDelay", ref movementDelayStr, 64u))
        {
            movementDelayStr = movementDelayStr.Replace(',', '.');
            if (float.TryParse(movementDelayStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var delay))
            {
                _config.MoveDelay = delay;
                configModified = true;
            }
        }
        ImGui.SameLine();
        ImGui.Text("Autorotation AI preset");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250f);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        var aipreset = _config.AIAutorotPresetName;
        var presets = _manager.Autorot.Database.Presets.VisiblePresets;

        var count = presets.Count;
        List<string> presetNames = new(count + 1);
        for (var i = 0; i < count; ++i)
        {
            presetNames.Add(presets[i].Name);
        }

        if (aipreset != null)
            presetNames.Add("Deactivate");
        var countnames = presetNames.Count;
        var selectedIndex = presetNames.IndexOf(aipreset ?? "");

        if (ImGui.Combo("##AI preset", ref selectedIndex, [.. presetNames], countnames))
        {
            if (selectedIndex == countnames - 1 && aipreset != null)
            {
                _manager.SetAIPreset(null);
                configModified = true;
                selectedIndex = -1;
            }
            else if (selectedIndex >= 0 && selectedIndex < count)
            {
                var selectedPreset = presets[selectedIndex];
                _manager.SetAIPreset(selectedPreset);
                configModified = true;
            }
        }
        if (configModified)
            _config.Modified.Fire();
    }

    public override void OnClose() => SetVisible(false);

    public void UpdateTitle()
    {
        var masterSlot = _manager?.MasterSlot ?? -1;
        var masterName = _manager?.Autorot?.WorldState?.Party[masterSlot]?.Name ?? "unknown";
        var masterSlotNumber = masterSlot != -1 ? (masterSlot + 1).ToString() : "N/A";

        WindowName = $"AI: {(_manager?.Beh != null ? "on" : "off")}, master={masterName}[{masterSlotNumber}]{_windowID}";
    }
}
