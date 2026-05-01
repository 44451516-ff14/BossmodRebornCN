using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Globalization;

namespace BossMod.AI;

sealed class AIManagementWindow : UIWindow
{
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManager _manager;
    private readonly EventSubscriptions _subscriptions;
    private const string _title = $"AI：关闭{_windowID}";
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

        ImGui.TextUnformatted($"导航目标={_manager.Controller.NaviTargetPos}");

        configModified |= ImGui.Checkbox("禁用技能", ref _config.ForbidActions);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("禁用移动", ref _config.ForbidMovement);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("骑乘时禁用AI移动", ref _config.ForbidAIMovementMounted);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("战斗中跟随", ref _config.FollowDuringCombat);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("需要启用此项才能跟随目标。");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("BOSS模块激活时跟随", ref _config.FollowDuringActiveBossModule);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("需要启用此项，才能在 Boss 模块激活时跟随目标。");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("脱战时跟随", ref _config.FollowOutOfCombat);
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("跟随目标", ref _config.FollowTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("按下方距离设置跟随目标。\n需要同时启用“战斗中跟随”和“BOSS模块激活时跟随”。");
            ImGui.EndTooltip();
        }
        ImGui.Spacing();
        configModified |= ImGui.Checkbox("允许手动选目标", ref _config.ManualTarget);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("AI 自动循环启用时允许手动选择目标。");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        configModified |= ImGui.Checkbox("禁用障碍物地图加载", ref _config.DisableObstacleMaps);

        ImGui.Text("跟随队伍成员位置");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        if (ImRaii.Combo("##Leader", _manager.Beh == null ? "<待机>" : _manager.WorldState.Party[_manager.MasterSlot]?.Name ?? "<未知>"))
        {
            if (ImGui.Selectable("<待机>", _manager.Beh == null))
            {
                _manager.SwitchToIdle();
            }
            var party = _manager.WorldState.Party.WithSlot(true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var slot = p.Item1;
                if (ImGui.Selectable(p.Item2.Name, _manager.MasterSlot == slot))
                {
                    _manager.SwitchToFollow(slot);
                    _config.FollowSlot = slot;
                    configModified = true;
                }
            }
            ImGui.EndCombo();
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("选择要跟随的玩家以启用 AI。通常选择自己。");
            ImGui.EndTooltip();
        }
        ImGui.Separator();
        ImGui.Text("攻击身位");
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
        if (ImGui.InputText("##MaxDistanceToTarget", ref maxDistanceTargetStr, 64))
        {
            maxDistanceTargetStr = maxDistanceTargetStr.Replace(',', '.');
            if (float.TryParse(maxDistanceTargetStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToTarget = maxDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("与目标保持的最大距离，单位为星码。");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("- 至队友");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var maxDistanceSlotStr = _config.MaxDistanceToSlot.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MaxDistanceToSlot", ref maxDistanceSlotStr, 64))
        {
            maxDistanceSlotStr = maxDistanceSlotStr.Replace(',', '.');
            if (float.TryParse(maxDistanceSlotStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxDistance))
            {
                _config.MaxDistanceToSlot = maxDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("与被跟随队友保持的最大距离，单位为星码。");
            ImGui.EndTooltip();
        }
        ImGui.Text("最小距离");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var minDistanceStr = _config.MinDistance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MinDistance", ref minDistanceStr, 64))
        {
            minDistanceStr = minDistanceStr.Replace(',', '.');
            if (float.TryParse(minDistanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var minDistance))
            {
                _config.MinDistance = minDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("与目标碰撞箱保持的距离，单位为星码。");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("与危险区的偏好距离");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var prefDistanceStr = _config.PreferredDistance.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##PrefDistance", ref prefDistanceStr, 64))
        {
            prefDistanceStr = prefDistanceStr.Replace(',', '.');
            if (float.TryParse(prefDistanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var prefDistance))
            {
                _config.PreferredDistance = prefDistance;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("与危险区域保持的距离，单位为星码。");
            ImGui.EndTooltip();
        }
        ImGui.Text("移动决策延迟");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(100f);
        var movementDelayStr = _config.MoveDelay.ToString(CultureInfo.InvariantCulture);
        if (ImGui.InputText("##MovementDelay", ref movementDelayStr, 64))
        {
            movementDelayStr = movementDelayStr.Replace(',', '.');
            if (float.TryParse(movementDelayStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var delay))
            {
                _config.MoveDelay = delay;
                configModified = true;
            }
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("做出移动决策后开始移动前的最短等待时间。\n不同副本需求不同，避免设置过高。");
            ImGui.EndTooltip();
        }
        ImGui.SameLine();
        ImGui.Text("自动循环 AI 预设");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250f);
        ImGui.SetNextWindowSizeConstraints(default, new Vector2(float.MaxValue, ImGui.GetTextLineHeightWithSpacing() * 50f));
        var aipreset = _config.AIAutorotPresetName;
        var presets = _manager.Autorot.Database.Presets.AllPresets;

        var count = presets.Count;
        List<string> presetNames = new(count + 1);
        for (var i = 0; i < count; ++i)
        {
            presetNames.Add(presets[i].Name);
        }

        if (aipreset != null)
            presetNames.Add("停用");
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
        var masterName = _manager?.Autorot?.WorldState?.Party[masterSlot]?.Name ?? "未知";
        var masterSlotNumber = masterSlot != -1 ? (masterSlot + 1).ToString() : "N/A";

        WindowName = $"AI：{(_manager?.Beh != null ? "开启" : "关闭")}，领袖={masterName}[{masterSlotNumber}]{_windowID}";
    }
}
