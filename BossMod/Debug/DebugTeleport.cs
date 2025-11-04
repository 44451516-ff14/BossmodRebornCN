using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace BossMod;

sealed class DebugTeleport : IDisposable
{
    private bool _enableNoClip;
    private bool _subscribed;
    private float _noClipSpeed = 6.0f; // units per second
    private Vector3 _inputCoordinates;

    public unsafe void Draw()
    {
        if (!_subscribed)
        {
            Service.Framework.Update += OnUpdate;
            _subscribed = true;
        }

        ImGui.BeginGroup();

        ImGui.Checkbox("No clip", ref _enableNoClip);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(180f);
        ImGui.InputFloat("No clip speed (yalms/s)", ref _noClipSpeed, 0.1f, 1f, "%.3f");
        _noClipSpeed = Math.Clamp(_noClipSpeed, 0.001f, 1000f);

        var localPlayer = Service.ClientState.LocalPlayer;
        var pos = localPlayer != null ? localPlayer.Position : Vector3.Zero;
        ImGui.Separator();
        ImGui.EndGroup();

        ImGui.BeginGroup();
        ImGui.Text("Current player coordinates:");
        ImGui.Text($"X: {pos.X:F3}");
        ImGui.Text($"Y: {pos.Y:F3}");
        ImGui.Text($"Z: {pos.Z:F3}");
        ImGui.EndGroup();

        ImGui.Separator();

        ImGui.BeginGroup();
        ImGui.Text("Enter target coordinates:");
        if (ImGui.Button("Set position"))
        {
            SetPlayerPosition(_inputCoordinates);
        }
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("X coordinate", ref _inputCoordinates.X, 1f);
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("Y coordinate", ref _inputCoordinates.Y, 1f);
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("Z coordinate", ref _inputCoordinates.Z, 1f);
        ImGui.EndGroup();
    }

    public void Dispose()
    {
        if (_subscribed)
        {
            Service.Framework.Update -= OnUpdate;
            _subscribed = false;
        }
    }

    private unsafe void SetPlayerPosition(Vector3 position)
    {
        var p = Service.ClientState.LocalPlayer;
        if (p == null)
        {
            return;
        }
        var obj = (CSGameObject*)p.Address;
        obj->SetPosition(position.X, position.Y, position.Z);
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (!_enableNoClip || Framework.Instance()->WindowInactive)
        {
            return;
        }

        var p = Service.ClientState.LocalPlayer;
        if (p == null)
        {
            return;
        }

        var obj = (CSGameObject*)p.Address;
        var pos = p.Position;

        // Δtime from ImGui (same thread), clamped to avoid zeros
        var dt = Math.Max(1e-4f, ImGui.GetIO().DeltaTime);
        var step = _noClipSpeed * dt;

        // camera yaw
        var camera = CameraManager.Instance()->GetActiveCamera();
        var yaw = MathF.PI - camera->DirH;
        var (s, c) = MathF.SinCos(yaw);

        // Build camera-space basis
        // forward = rotate (0,0,1) by yaw -> (-sin, 0, cos)
        // right   = rotate (1,0,0) by yaw -> ( cos, 0, sin)
        var forward = new Vector3(-s, 0f, c);
        var right = new Vector3(c, 0f, s);

        // poll keys once; allow diagonals
        var up = IsKeyPressed(32);
        var down = IsKeyPressed(160);
        var w = IsKeyPressed(87);
        var sKey = IsKeyPressed(83);
        var a = IsKeyPressed(65);
        var d = IsKeyPressed(68);

        // cancel game input only for pressed keys
        if (up)
        {
            Service.KeyState.SetRawValue(VirtualKey.SPACE, 0);
        }
        if (down)
        {
            Service.KeyState.SetRawValue(VirtualKey.LSHIFT, 0);
        }
        if (w)
        {
            Service.KeyState.SetRawValue(VirtualKey.W, 0);
        }
        if (sKey)
        {
            Service.KeyState.SetRawValue(VirtualKey.S, 0);
        }
        if (a)
        {
            Service.KeyState.SetRawValue(VirtualKey.A, 0);
        }
        if (d)
        {
            Service.KeyState.SetRawValue(VirtualKey.D, 0);
        }

        // accumulate movement
        var move = Vector3.Zero;
        if (up)
        {
            move.Y += step;
        }
        if (down)
        {
            move.Y -= step;
        }
        if (w)
        {
            move += forward * step; // forward
        }
        if (sKey)
        {
            move -= forward * step; // backwards
        }
        if (a)
        {
            move += right * step; // left
        }
        if (d)
        {
            move -= right * step; // right
        }

        if (move != Vector3.Zero)
        {
            var newPos = pos + move;
            obj->SetPosition(newPos.X, newPos.Y, newPos.Z);
        }

        static bool IsKeyPressed(int vk)
        {
            static bool IsBitSet(short b, int bit) => (b & (1 << bit)) != 0;
            return vk != 0 && IsBitSet(PInvoke.User32.GetAsyncKeyState(vk), 15);
        }
    }
}