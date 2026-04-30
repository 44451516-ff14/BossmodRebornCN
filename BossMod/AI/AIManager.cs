using BossMod.Autorotation;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    public static AIManager? Instance;
    public readonly RotationModuleManager Autorot;
    public readonly AIController Controller;
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManagementWindow _wndAI;
    public int MasterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    public AIBehaviour? Beh;
    public Preset? AiPreset;

    public WorldState WorldState => Autorot.Bossmods.WorldState;
    public float ForceMovementIn => Beh?.ForceMovementIn ?? float.MaxValue;
    public string GetAIPreset => AiPreset?.Name ?? string.Empty;

    public AIManager(RotationModuleManager autorot, ActionManagerEx amex, MovementOverride movement)
    {
        Instance = this;
        _wndAI = new AIManagementWindow(this);
        Autorot = autorot;
        Controller = new(autorot.WorldState, amex, movement);
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
    }

    public void SetAIPreset(Preset? p)
    {
        AiPreset = p;
        _config.AIAutorotPresetName = p?.Name;
        Beh?.AIPreset = p;
    }

    public void Dispose()
    {
        SwitchToIdle();
        _wndAI.Dispose();
        Service.CommandManager.RemoveHandler("/bmrai");
        Instance = null;
    }

    public void Update()
    {
        if (!WorldState.Party.Members[MasterSlot].IsValid())
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];
        if (Beh != null && player != null && master != null && !WorldState.Party.Members[PartyState.PlayerSlot].InCutscene)
            _ = Beh.Execute(player, master);
        else
            Controller.Clear();
        Controller.Update(player, Autorot.Hints, WorldState.CurrentTime);
    }

    public void SwitchToIdle()
    {
        Beh?.Dispose();
        Beh = null;
        MasterSlot = PartyState.PlayerSlot;
        Autorot.Preset = null;
        Controller.Clear();
        _wndAI.UpdateTitle();
    }

    public void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        MasterSlot = WorldState.Party[masterSlot]?.Name == null ? 0 : masterSlot;
        var allpresets = Autorot.Database.Presets.AllPresets;
        var count = allpresets.Count;
        Preset? preset = null;
        for (var i = 0; i < count; ++i)
        {
            var p = allpresets[i];
            if (p.Name == _config.AIAutorotPresetName)
            {
                preset = p;
                break;
            }
        }
        Beh = new AIBehaviour(Controller, Autorot, preset);
        _wndAI.UpdateTitle();
    }

    private unsafe int FindPartyMemberSlotFromSender(SeString sender)
    {
        var sources = sender.Payloads.Count != 0 ? sender.Payloads[0] : null;
        if (sources is not PlayerPayload source)
            return -1;
        var group = GroupManager.Instance()->GetGroup();
        var slot = -1;
        for (var i = 0; i < group->MemberCount; ++i)
        {
            if (group->PartyMembers[i].HomeWorld == source.World.RowId && group->PartyMembers[i].NameString == source.PlayerName)
            {
                slot = i;
                break;
            }
        }
        return slot >= 0 ? Array.FindIndex(WorldState.Party.Members, m => m.ContentId == group->PartyMembers[slot].ContentId) : -1;
    }

    private void OnCommand(string cmd, string message)
    {
        var messageData = message.Split(' ');
        if (messageData.Length == 0)
            return;

        var configModified = false;

        switch (messageData[0].ToUpperInvariant())
        {
            case "ON":
                EnableConfig(true);
                break;
            case "OFF":
                EnableConfig(false);
                break;
            case "TOGGLE":
                ToggleConfig();
                break;
            case "TARGETMASTER":
                configModified = ToggleFocusTargetMaster();
                break;
            case "FOLLOW":
                var cfgFollowSlot = _config.FollowSlot;
                HandleFollowCommand(messageData);
                configModified = cfgFollowSlot != _config.FollowSlot;
                break;
            case "UI":
                configModified = ToggleDebugMenu();
                break;
            case "FORBIDACTIONS":
                var cfgForbidActions = _config.ForbidActions;
                ToggleForbidActions(messageData);
                configModified = cfgForbidActions != _config.ForbidActions;
                break;
            case "FORBIDMOVEMENT":
                var cfgForbidMovement = _config.ForbidMovement;
                ToggleForbidMovement(messageData);
                configModified = cfgForbidMovement != _config.ForbidMovement;
                break;
            case "IDLEWHILEMOUNTED":
                var cfgMountedIdle = _config.ForbidAIMovementMounted;
                ToggleIdleWhileMounted(messageData);
                configModified = cfgMountedIdle != _config.ForbidAIMovementMounted;
                break;
            case "FOLLOWOUTOFCOMBAT":
                var cfgFollowOOC = _config.FollowOutOfCombat;
                ToggleFollowOutOfCombat(messageData);
                configModified = cfgFollowOOC != _config.FollowOutOfCombat;
                break;
            case "FOLLOWCOMBAT":
                var cfgFollowIC = _config.FollowDuringCombat;
                ToggleFollowCombat(messageData);
                configModified = cfgFollowIC != _config.FollowDuringCombat;
                break;
            case "FOLLOWMODULE":
                var cfgFollowM = _config.FollowDuringActiveBossModule;
                ToggleFollowModule(messageData);
                configModified = cfgFollowM != _config.FollowDuringActiveBossModule;
                break;
            case "FOLLOWTARGET":
                var cfgFollowT = _config.FollowTarget;
                ToggleFollowTarget(messageData);
                configModified = cfgFollowT != _config.FollowTarget;
                break;
            case "OBSTACLEMAPS":
                var cfgOM = _config.DisableObstacleMaps;
                ToggleObstacleMaps(messageData);
                configModified = cfgOM != _config.DisableObstacleMaps;
                break;

            case "POSITIONAL":
                var cfgPositional = _config.DesiredPositional;
                HandlePositionalCommand(messageData);
                configModified = cfgPositional != _config.DesiredPositional;
                break;
            case "MAXDISTANCETARGET":
                var cfgMDT = _config.MaxDistanceToTarget;
                HandleMaxDistanceTargetCommand(messageData);
                configModified = cfgMDT != _config.MaxDistanceToTarget;
                break;
            case "MAXDISTANCESLOT":
                var cfgMDS = _config.MaxDistanceToSlot;
                HandleMaxDistanceSlotCommand(messageData);
                configModified = cfgMDS != _config.MaxDistanceToSlot;
                break;
            case "MINDISTANCE":
                var cfgMinDT = _config.MinDistance;
                HandleMinDistanceCommand(messageData);
                configModified = cfgMinDT != _config.MinDistance;
                break;
            case "PREFDISTANCE":
                var cfgPrefDS = _config.PreferredDistance;
                HandlePrefDistanceCommand(messageData);
                configModified = cfgPrefDS != _config.PreferredDistance;
                break;
            case "MOVEDELAY":
                var cfgMD = _config.MoveDelay;
                HandleMoveDelayCommand(messageData);
                configModified = cfgMD != _config.MoveDelay;
                break;
            case "SETPRESETNAME":
                if (cmd.Length <= 2)
                {
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print("请指定一个 AI 自动循环预设名称。");
                    }
                    return;
                }
                else
                {
                    var cfgARPreset = _config.AIAutorotPresetName;
                    ParseAIAutorotationSetCommand(messageData);
                    configModified = cfgARPreset != _config.AIAutorotPresetName;
                }
                break;
            default:
                if (_config.EchoToChat)
                {
                    Service.ChatGui.Print($"[BMRAI] 未知命令：{messageData[0]}");
                }
                return;
        }

        if (configModified)
            _config.Modified.Fire();
    }

    private void EnableConfig(bool enable)
    {
        if (enable)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
    }

    private void ToggleConfig()
    {
        if (Beh == null)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
    }

    private bool ToggleFocusTargetMaster()
    {
        _config.FocusTargetMaster = !_config.FocusTargetMaster;
        return true;
    }

    private void ToggleObstacleMaps(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.DisableObstacleMaps = !_config.DisableObstacleMaps;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.DisableObstacleMaps = false;
                    break;
                case "OFF":
                    _config.DisableObstacleMaps = true;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知障碍地图命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 障碍地图现在已{(_config.DisableObstacleMaps ? "禁用" : "启用")}");
        }
    }

    private void ToggleIdleWhileMounted(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.ForbidAIMovementMounted = !_config.ForbidAIMovementMounted;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.ForbidAIMovementMounted = true;
                    break;
                case "OFF":
                    _config.ForbidAIMovementMounted = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知骑乘待机命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 骑乘时待机现在已{(_config.ForbidAIMovementMounted ? "启用" : "禁用")}");
        }
    }

    private void HandleFollowCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少跟随目标。");
            return;
        }

        if (messageData[1].StartsWith("Slot", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(messageData[1].AsSpan(4), out var slot) && slot >= 1 && slot <= 8)
        {
            SwitchToFollow(slot - 1);
            _config.FollowSlot = slot - 1;
        }
        else
        {
            var memberIndex = FindPartyMemberByName(string.Join(" ", messageData.Skip(1)));
            if (memberIndex >= 0)
            {
                SwitchToFollow(memberIndex);
                _config.FollowSlot = memberIndex;
            }
            else
                    if (_config.EchoToChat)
            {
                Service.ChatGui.Print($"[BMRAI] 未知队员：{string.Join(" ", messageData.Skip(1))}");
            }
        }
    }

    private bool ToggleDebugMenu()
    {
        _config.DrawUI = !_config.DrawUI;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] AI 菜单现在已{(_config.DrawUI ? "启用" : "禁用")}");
        }
        return true;
    }

    private void ToggleForbidActions(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.ForbidActions = !_config.ForbidActions;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.ForbidActions = true;
                    break;
                case "OFF":
                    _config.ForbidActions = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知禁止动作命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 禁止动作现在已{(_config.ForbidActions ? "启用" : "禁用")}");
        }
    }

    private void ToggleForbidMovement(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.ForbidMovement = !_config.ForbidMovement;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.ForbidMovement = true;
                    break;
                case "OFF":
                    _config.ForbidMovement = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知禁止移动命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 禁止移动现在已{(_config.ForbidMovement ? "启用" : "禁用")}");
        }
    }

    private void ToggleFollowOutOfCombat(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.FollowOutOfCombat = !_config.FollowOutOfCombat;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowOutOfCombat = true;
                    break;
                case "OFF":
                    _config.FollowOutOfCombat = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知脱战跟随命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 脱战跟随现在已{(_config.FollowOutOfCombat ? "启用" : "禁用")}");
        }
    }

    private void ToggleFollowCombat(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            if (_config.FollowDuringCombat)
            {
                _config.FollowDuringCombat = false;
                _config.FollowDuringActiveBossModule = false;
            }
            else
                _config.FollowDuringCombat = true;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowDuringCombat = true;
                    break;
                case "OFF":
                    _config.FollowDuringCombat = false;
                    _config.FollowDuringActiveBossModule = false;
                    break;
                default:
                    Service.ChatGui.Print($"[BMRAI] 未知战斗中跟随命令：{messageData[1]}");
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 战斗中跟随现在已{(_config.FollowDuringCombat ? "启用" : "禁用")}");
            Service.ChatGui.Print($"[BMRAI] Boss 模块激活时跟随现在已{(_config.FollowDuringActiveBossModule ? "启用" : "禁用")}");
        }
    }

    private void ToggleFollowModule(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.FollowDuringActiveBossModule = !_config.FollowDuringActiveBossModule;
            if (!_config.FollowDuringCombat)
                _config.FollowDuringCombat = true;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowDuringActiveBossModule = true;
                    _config.FollowDuringCombat = true;
                    break;
                case "OFF":
                    _config.FollowDuringActiveBossModule = false;
                    break;
                default:
                    Service.ChatGui.Print($"[BMRAI] 未知 Boss 模块激活时跟随命令：{messageData[1]}");
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Boss 模块激活时跟随现在已{(_config.FollowDuringActiveBossModule ? "启用" : "禁用")}");
            Service.ChatGui.Print($"[BMRAI] 战斗中跟随现在已{(_config.FollowDuringCombat ? "启用" : "禁用")}");
        }
    }

    private void ToggleFollowTarget(string[] messageData)
    {
        if (messageData.Length == 1)
            _config.FollowTarget = !_config.FollowTarget;
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowTarget = true;
                    break;
                case "OFF":
                    _config.FollowTarget = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] 未知跟随目标命令：{messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 跟随目标现在已{(_config.FollowTarget ? "启用" : "禁用")}");
        }
    }

    private void HandlePositionalCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少身位类型。");
            return;
        }

        var msg = messageData[1];
        switch (msg.ToUpperInvariant())
        {
            case "ANY":
                _config.DesiredPositional = Positional.Any;
                break;
            case "FLANK":
                _config.DesiredPositional = Positional.Flank;
                break;
            case "REAR":
                _config.DesiredPositional = Positional.Rear;
                break;
            case "FRONT":
                _config.DesiredPositional = Positional.Front;
                break;
            default:
                if (_config.EchoToChat)
                {
                    Service.ChatGui.Print($"[BMRAI] 未知身位：{msg}");
                }
                return;
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 期望身位已设为 {_config.DesiredPositional}");
        }
    }

    private void HandleMaxDistanceTargetCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少距离数值。");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 无效的距离数值。");
            return;
        }

        _config.MaxDistanceToTarget = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 到目标的最大距离已设为 {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandleMaxDistanceSlotCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少距离数值。");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 无效的距离数值。");
            return;
        }

        _config.MaxDistanceToSlot = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 到队伍槽位的最大距离已设为 {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandleMinDistanceCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少距离数值。");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 无效的距离数值。");
            return;
        }

        _config.MinDistance = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 到队伍槽位的最小距离已设为 {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandlePrefDistanceCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少距离数值。");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 无效的距离数值。");
            return;
        }

        _config.PreferredDistance = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 到队伍槽位的偏好距离已设为 {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }
    }

    private void HandleMoveDelayCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 缺少延迟数值。");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var delay) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] 无效的延迟数值。");
            return;
        }

        _config.MoveDelay = delay;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] 移动延迟已设为 {delay.ToString(System.Globalization.CultureInfo.InvariantCulture)}s");
        }
    }

    private int FindPartyMemberByName(string name)
    {
        for (var i = 0; i < 8; ++i)
        {
            var member = Autorot.WorldState.Party[i];
            if (member != null && member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private void ParseAIAutorotationSetCommand(string[] presetName)
    {
        if (presetName.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("未提供有效的预设名称。");
            return;
        }

        var userInput = string.Join(" ", presetName.Skip(1)).Trim();
        if (userInput == "null" || string.IsNullOrWhiteSpace(userInput))
        {
            SetAIPreset(null);
            Autorot.Preset = null;
            if (_config.EchoToChat)
            {
                Service.ChatGui.Print("已禁用 AI 自动循环预设。");
            }
            return;
        }

        var normalizedInput = userInput.ToUpperInvariant();
        var preset = Autorot.Database.Presets.AllPresets
            .FirstOrDefault(p => p.Name.Trim().Equals(normalizedInput, StringComparison.OrdinalIgnoreCase))
            ?? RotationModuleManager.ForceDisable;

        if (preset != null)
        {
            if (_config.EchoToChat)
            {
                Service.ChatGui.Print($"预设已从 '{Beh?.AIPreset?.Name ?? "<n/a>"}' 切换为 '{preset?.Name ?? "<n/a>"}'");
            }
            SetAIPreset(preset);
        }
        else if (_config.EchoToChat)
        {
            Service.ChatGui.PrintError($"找不到预设 '{userInput}'");
        }
    }
}
