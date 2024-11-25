﻿using BossMod.AI;
using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod;

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct PlayerMoveControllerFlyInput
{
    [FieldOffset(0x0)] public float Forward;
    [FieldOffset(0x4)] public float Left;
    [FieldOffset(0x8)] public float Up;
    [FieldOffset(0xC)] public float Turn;
    [FieldOffset(0x10)] public float u10;
    [FieldOffset(0x14)] public byte DirMode;
    [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
}

public sealed unsafe class MovementOverride : IDisposable
{
    public Vector3? DesiredDirection;

    private float UserMoveLeft;
    private float UserMoveUp;
    private float ActualMoveLeft;
    private float ActualMoveUp;
#pragma warning disable IDE0032
    private readonly ActionTweaksConfig _tweaksConfig = Service.Config.Get<ActionTweaksConfig>();
    private bool _movementBlocked;
#pragma warning restore IDE0032
    private bool? _forcedControlState;
    private bool _legacyMode;

    public bool IsMoving() => ActualMoveLeft != 0 || ActualMoveUp != 0;
    public bool IsMoveRequested() => UserMoveLeft != 0 || UserMoveUp != 0;

    public bool IsForceUnblocked() => _tweaksConfig.MoveEscapeHatch switch
    {
        ActionTweaksConfig.ModifierKey.Ctrl => ImGui.GetIO().KeyCtrl,
        ActionTweaksConfig.ModifierKey.Alt => ImGui.GetIO().KeyAlt,
        ActionTweaksConfig.ModifierKey.Shift => ImGui.GetIO().KeyShift,
        ActionTweaksConfig.ModifierKey.M12 => UIInputData.Instance()->UIFilteredCursorInputs.MouseButtonHeldFlags.HasFlag(MouseButtonFlags.LBUTTON | MouseButtonFlags.RBUTTON),
        _ => false,
    };

    public bool MovementBlocked
    {
        get => _movementBlocked && !IsForceUnblocked();
        set => _movementBlocked = value;
    }

    public static readonly float* ForcedMovementDirection = (float*)Service.SigScanner.GetStaticAddressFromSig("F3 0F 11 0D ?? ?? ?? ?? 48 85 DB");

    private delegate bool RMIWalkIsInputEnabled(void* self);
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled1;
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled2;

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    private readonly HookAddress<RMIWalkDelegate> _rmiWalkHook;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    private readonly HookAddress<RMIFlyDelegate> _rmiFlyHook;

    // input source flags: 1 = kb/mouse, 2 = gamepad
    private delegate byte MoveControlIsInputActiveDelegate(void* self, byte inputSourceFlags);
    private readonly HookAddress<MoveControlIsInputActiveDelegate> _mcIsInputActiveHook;

    public MovementOverride()
    {
        var rmiWalkIsInputEnabled1Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C");
        var rmiWalkIsInputEnabled2Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F");
        Service.Log($"RMIWalkIsInputEnabled1 address: 0x{rmiWalkIsInputEnabled1Addr:X}");
        Service.Log($"RMIWalkIsInputEnabled2 address: 0x{rmiWalkIsInputEnabled2Addr:X}");
        _rmiWalkIsInputEnabled1 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled1Addr);
        _rmiWalkIsInputEnabled2 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled2Addr);

        _rmiWalkHook = new("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D", RMIWalkDetour);
        _rmiFlyHook = new("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8", RMIFlyDetour);
        _mcIsInputActiveHook = new("E8 ?? ?? ?? ?? 84 C0 74 09 84 DB 74 1A", MCIsInputActiveDetour);

        Service.GameConfig.UiControlChanged += OnConfigChanged;
        UpdateLegacyMode();
    }

    public void Dispose()
    {
        Service.GameConfig.UiControlChanged -= OnConfigChanged;
        MovementBlocked = false;
        UserMoveLeft = 0;
        UserMoveUp = 0;
        ActualMoveLeft = 0;
        ActualMoveUp = 0;
        _mcIsInputActiveHook.Dispose();
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _forcedControlState = null;
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);
        UserMoveLeft = *sumLeft;
        UserMoveUp = *sumForward;

        // TODO: this allows AI mode to move even if movement is "blocked", is this the right behavior? AI mode should try to avoid moving while casting anyway...
        if (_movementBlocked)
        {
            *sumLeft = 0;
            *sumForward = 0;
        }

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        var movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2(self); //&& !_movementBlocked
        if (movementAllowed && *sumLeft == 0 && *sumForward == 0 && DirectionToDestination(false) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            *sumLeft = dir.X;
            *sumForward = dir.Z;
        }

        if ((AIManager.Instance?.Beh != null || _tweaksConfig.MisdirectionThreshold < 180) && PlayerHasMisdirection())
        {
            var threshold = AIManager.Instance?.Beh != null ? 20 : _tweaksConfig.MisdirectionThreshold;
            if (!movementAllowed)
            {
                // we are already moving, see whether we need to force stop it
                // unfortunately, the base implementation would not sample the input if movement is disabled - force it
                float realLeft = 0, realForward = 0, realTurn = 0;
                byte realStrafe = 0, realUnk = 0;
                if (!MovementBlocked)
                    _rmiWalkHook.Original(self, &realLeft, &realForward, &realTurn, &realStrafe, &realUnk, 1);
                var desiredRelDir = realLeft != 0 || realForward != 0 ? Angle.FromDirection(new(realLeft, realForward)) : DirectionToDestination(false)?.h;
                _forcedControlState = desiredRelDir != null && (desiredRelDir.Value + ForwardMovementDirection() - ForcedMovementDirection->Radians()).Normalized().Abs().Deg <= threshold;
            }
            else if (*sumLeft != 0 || *sumForward != 0)
            {
                var currentDir = Angle.FromDirection(new(*sumLeft, *sumForward)) + ForwardMovementDirection();
                var dirDelta = currentDir - ForcedMovementDirection->Radians();
                _forcedControlState = dirDelta.Normalized().Abs().Deg <= threshold;
                if (!_forcedControlState.Value)
                {
                    // forbid movement for now
                    *sumLeft = *sumForward = 0;
                }
            }
            // else: movement is allowed (so we're not already moving), but we don't want to move anywhere, do nothing
        }

        ActualMoveLeft = *sumLeft;
        ActualMoveUp = *sumForward;
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _forcedControlState = null;
        _rmiFlyHook.Original(self, result);
        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if (result->Forward == 0 && result->Left == 0 && result->Up == 0 && DirectionToDestination(true) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Z;
            result->Left = dir.X;
            result->Up = relDir.Value.v.Rad;
        }
    }

    private byte MCIsInputActiveDetour(void* self, byte inputSourceFlags)
    {
        return _forcedControlState != null ? (byte)(_forcedControlState.Value ? 1 : 0) : _mcIsInputActiveHook.Original(self, inputSourceFlags);
    }

    private (Angle h, Angle v)? DirectionToDestination(bool allowVertical)
    {
        if (DesiredDirection == null || DesiredDirection.Value == default)
            return null;

        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player == null)
            return null;

        var dxz = new WDir(DesiredDirection.Value.X, DesiredDirection.Value.Z);
        var dirH = Angle.FromDirection(dxz);
        var dirV = allowVertical ? Angle.FromDirection(new(DesiredDirection.Value.Y, dxz.Length())) : default;
        return (dirH - ForwardMovementDirection(), dirV);
    }

    private Angle ForwardMovementDirection() => _legacyMode ? Camera.Instance!.CameraAzimuth.Radians() + 180.Degrees() : GameObjectManager.Instance()->Objects.IndexSorted[0].Value->Rotation.Radians();

    private bool PlayerHasMisdirection()
    {
        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var sm = player != null && player->IsCharacter() ? player->GetStatusManager() : null;
        if (sm == null)
            return false;
        for (var i = 0; i < sm->NumValidStatuses; ++i)
            if (sm->Status[i].StatusId is 1422 or 2936 or 3694 or 3909)
                return true;
        return false;
    }

    private void OnConfigChanged(object? sender, ConfigChangeEvent evt) => UpdateLegacyMode();
    private void UpdateLegacyMode()
    {
        _legacyMode = Service.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1;
        Service.Log($"Legacy mode is now {(_legacyMode ? "enabled" : "disabled")}");
    }
}
