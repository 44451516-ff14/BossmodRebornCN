using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    public static AIManager? Instance;
    public readonly RotationModuleManager Autorot;
    public readonly AIController Controller;
    private readonly AIConfig _config;
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
        _config = Service.Config.Get<AIConfig>();
        Service.ChatGui.ChatMessage += OnChatMessage;
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
        Service.CommandManager.AddHandler("/vbmai", new Dalamud.Game.Command.CommandInfo(OnCommand) { ShowInHelp = false });
    }

    public void SetAIPreset(Preset? p)
    {
        AiPreset = p;
        if (Beh != null)
            Beh.AIPreset = p;
    }

    public void Dispose()
    {
        SwitchToIdle();
        _wndAI.Dispose();
        Service.ChatGui.ChatMessage -= OnChatMessage;
        Service.CommandManager.RemoveHandler("/bmrai");
        Service.CommandManager.RemoveHandler("/vbmai");
    }

    public void Update()
    {
        if (!WorldState.Party.Members[MasterSlot].IsValid())
            SwitchToIdle();

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];
        if (Beh != null && player != null && master != null && !WorldState.Party.Members[PartyState.PlayerSlot].InCutscene)
            Beh.Execute(player, master);
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
        Beh = new AIBehaviour(Controller, Autorot, AiPreset);
        _wndAI.UpdateTitle();
    }

    private unsafe int FindPartyMemberSlotFromSender(SeString sender)
    {
        if (sender.Payloads.FirstOrDefault() is not PlayerPayload source)
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

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Beh == null || type != XivChatType.Party)
            return;

        var messagePrefix = message.Payloads.FirstOrDefault() as TextPayload;
        if (messagePrefix?.Text == null)
            return;

        var messageText = messagePrefix.Text;
        if (!messageText.StartsWith("bmrai ", StringComparison.OrdinalIgnoreCase) && !messageText.StartsWith("vbmai ", StringComparison.OrdinalIgnoreCase))
            return;

        var messageData = messagePrefix.Text.Split(' ');
        if (messageData.Length < 2)
            return;

        switch (messageData[1])
        {
            case "follow":
                var master = FindPartyMemberSlotFromSender(sender);
                if (master >= 0)
                    SwitchToFollow(master);
                break;
            case "cancel":
                SwitchToIdle();
                break;
            default:
                Service.ChatGui.Print($"[AI] Unknown command: {messageData[1]}");
                break;
        }
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
                configModified = EnableConfig(true);
                break;
            case "OFF":
                configModified = EnableConfig(false);
                break;
            case "TOGGLE":
                configModified = ToggleConfig();
                break;
            case "TARGETMASTER":
                configModified = ToggleFocusTargetLeader();
                break;
            case "FOLLOW":
                configModified = HandleFollowCommand(messageData);
                break;
            case "UI":
                configModified = ToggleDebugMenu();
                break;
            case "FORBIDACTIONS":
                configModified = ToggleForbidActions(messageData);
                break;
            case "FORDBIDMOVEMENT":
                configModified = ToggleForbidMovement(messageData);
                break;
            case "FOLLOWOUTOFCOMBAT":
                configModified = ToggleFollowOutOfCombat(messageData);
                break;
            case "FOLLOWCOMBAT":
                configModified = ToggleFollowCombat(messageData);
                break;
            case "FOLLOWMODULE":
                configModified = ToggleFollowModule(messageData);
                break;
            case "FOLLOWTARGET":
                configModified = ToggleFollowTarget(messageData);
                break;
            case "POSITIONAL":
                configModified = HandlePositionalCommand(messageData);
                break;
            case "MAXDISTANCETARGET":
                configModified = HandleMaxDistanceTargetCommand(messageData);
                break;
            case "MAXDISTANCESLOT":
                configModified = HandleMaxDistanceSlotCommand(messageData);
                break;
            case "SETPRESETNAME":
                if (cmd.Length <= 2)
                    Service.Log("Specify an AI autorotation preset name.");
                else
                    ParseAIAutorotationSetCommand(messageData);
                break;
            default:
                Service.ChatGui.Print($"[AI] Unknown command: {messageData[0]}");
                break;
        }

        if (configModified)
            _config.Modified.Fire();
    }

    private bool EnableConfig(bool enable)
    {
        if (enable)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
        return true;
    }

    private bool ToggleConfig()
    {
        if (Beh == null)
            SwitchToFollow(_config.FollowSlot);
        else
            SwitchToIdle();
        return true;
    }

    private bool ToggleFocusTargetLeader()
    {
        _config.FocusTargetLeader = !_config.FocusTargetLeader;
        return true;
    }

    private bool HandleFollowCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.ChatGui.Print("[AI] Missing follow target.");
            return false;
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
            {
                Service.ChatGui.Print($"[AI] Unknown party member: {string.Join(" ", messageData.Skip(1))}");
                return false;
            }
        }
        return true;
    }

    private bool ToggleDebugMenu()
    {
        _config.DrawUI = !_config.DrawUI;
        Service.Log($"[AI] AI menu is now {(_config.DrawUI ? "enabled" : "disabled")}");
        return true;
    }

    private bool ToggleForbidActions(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown forbid actions command: {messageData[1]}");
                    return _config.ForbidActions;
            }
        }
        Service.Log($"[AI] Forbid actions is now {(_config.ForbidActions ? "enabled" : "disabled")}");
        return _config.ForbidActions;
    }

    private bool ToggleForbidMovement(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown forbid movement command: {messageData[1]}");
                    return _config.ForbidMovement;
            }
        }
        Service.Log($"[AI] Forbid movement is now {(_config.ForbidMovement ? "enabled" : "disabled")}");
        return _config.ForbidMovement;
    }

    private bool ToggleFollowOutOfCombat(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown follow out of combat command: {messageData[1]}");
                    return _config.FollowOutOfCombat;
            }
        }
        Service.Log($"[AI] Follow out of combat is now {(_config.FollowOutOfCombat ? "enabled" : "disabled")}");
        return _config.FollowOutOfCombat;
    }

    private bool ToggleFollowCombat(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown follow during combat command: {messageData[1]}");
                    return _config.FollowDuringCombat;
            }
        }
        Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
        Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
        return _config.FollowDuringCombat;
    }

    private bool ToggleFollowModule(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown follow during active boss module command: {messageData[1]}");
                    return _config.FollowDuringActiveBossModule;
            }
        }
        Service.Log($"[AI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
        Service.Log($"[AI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
        return _config.FollowDuringActiveBossModule;
    }

    private bool ToggleFollowTarget(string[] messageData)
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
                    Service.ChatGui.Print($"[AI] Unknown follow target command: {messageData[1]}");
                    return _config.FollowTarget;
            }
        }
        Service.Log($"[AI] Following targets is now {(_config.FollowTarget ? "enabled" : "disabled")}");
        return _config.FollowTarget;
    }

    private bool HandlePositionalCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.ChatGui.Print("[AI] Missing positional type.");
            return false;
        }
        SetPositional(messageData[1]);
        return true;
    }

    private void SetPositional(string positional)
    {
        switch (positional.ToUpperInvariant())
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
                Service.ChatGui.Print($"[AI] Unknown positional: {positional}");
                return;
        }
        Service.Log($"[AI] Desired positional set to {_config.DesiredPositional}");
    }

    private bool HandleMaxDistanceTargetCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.ChatGui.Print("[AI] Missing distance value.");
            return false;
        }
        var distanceStr = messageData[1].Replace(',', '.');
        if (!float.TryParse(distanceStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance))
        {
            Service.ChatGui.Print("[AI] Invalid distance value.");
            return false;
        }
        _config.MaxDistanceToTarget = distance;
        Service.Log($"[AI] Max distance to target set to {distance}");
        return true;
    }

    private bool HandleMaxDistanceSlotCommand(string[] messageData)
    {
        if (messageData.Length < 2)
        {
            Service.ChatGui.Print("[AI] Missing distance value.");
            return false;
        }
        var distanceStr = messageData[1].Replace(',', '.');
        if (!float.TryParse(distanceStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance))
        {
            Service.ChatGui.Print("[AI] Invalid distance value.");
            return false;
        }
        _config.MaxDistanceToSlot = distance;
        Service.Log($"[AI] Max distance to slot set to {distance}");
        return true;
    }

    private int FindPartyMemberByName(string name)
    {
        for (var i = 0; i < 8; i++)
        {
            var member = Autorot.WorldState.Party[i];
            if (member != null && member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    private void ParseAIAutorotationSetCommand(string[] presetName)
    {
        if (presetName.Length < 2)
        {
            Service.Log("No valid preset name provided.");
            return;
        }

        var userInput = string.Join(" ", presetName.Skip(1)).Trim();
        if (userInput == "null" || string.IsNullOrWhiteSpace(userInput))
        {
            SetAIPreset(null);
            Autorot.Preset = null;
            _config.AIAutorotPresetName = null;
            Service.Log("Disabled AI autorotation preset.");
            return;
        }

        var normalizedInput = userInput.ToUpperInvariant();
        var preset = Autorot.Database.Presets.VisiblePresets
            .FirstOrDefault(p => p.Name.Trim().Equals(normalizedInput, StringComparison.OrdinalIgnoreCase))
            ?? RotationModuleManager.ForceDisable;

        if (preset != null)
        {
            Service.Log($"Console: Changed preset from '{Beh?.AIPreset?.Name ?? "<n/a>"}' to '{preset?.Name ?? "<n/a>"}'");
            SetAIPreset(preset);
            _config.AIAutorotPresetName = preset?.Name;
        }
        else
        {
            Service.ChatGui.PrintError($"Failed to find preset '{userInput}'");
        }
    }
}
