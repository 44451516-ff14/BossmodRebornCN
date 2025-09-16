using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using CSGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace BossMod;

sealed class DebugTeleport
{
    private bool EnableNoClip;
    private float NoClipSpeed = 0.0001f;
    private Vector3 inputCoordinates;

    public unsafe void Draw()
    {
        ImGui.BeginGroup();
        ImGui.Checkbox("No Clip", ref EnableNoClip);
        if (EnableNoClip)
        {
            Enable();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150f);
            ImGui.InputFloat("No Clip Speed", ref NoClipSpeed, 0.0001f, default, "%.4f");
        }
        else
        {
            Disable();
        }
        var localPlayer = Service.ClientState.LocalPlayer;
        var pos = localPlayer != null ? localPlayer.Position : Vector3.Zero;
        ImGui.Separator();
        ImGui.EndGroup();
        ImGui.BeginGroup();
        ImGui.Text("Current Player Coordinates:");
        ImGui.Text("X: " + pos.X.ToString("F3"));
        ImGui.Text("Y: " + pos.Y.ToString("F3"));
        ImGui.Text("Z: " + pos.Z.ToString("F3"));
        ImGui.EndGroup();
        ImGui.Separator();
        ImGui.BeginGroup();
        ImGui.Text("Enter Target Coordinates:");
        if (ImGui.Button("Set Position"))
        {
            SetPlayerPosition(inputCoordinates);
        }
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("X Coordinate", ref inputCoordinates.X, 1f);
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("Y Coordinate", ref inputCoordinates.Y, 1f);
        ImGui.SetNextItemWidth(150f);
        ImGui.InputFloat("Z Coordinate", ref inputCoordinates.Z, 1f);
        ImGui.EndGroup();
    }

    private unsafe void SetPlayerPosition(Vector3 position)
    {
        var p = Service.ClientState.LocalPlayer;
        if (p != null)
        {
            var obj = (CSGameObject*)p.Address;
            obj->SetPosition(position.X, position.Y, position.Z);
        }
    }

    private void Enable()
    {
        Service.Framework.Update += OnUpdate;
    }

    private void Disable()
    {
        Service.Framework.Update -= OnUpdate;
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        if (EnableNoClip && !Framework.Instance()->WindowInactive)
        {
            var p = Service.ClientState.LocalPlayer;
            if (p == null)
            {
                return;
            }
            var obj = (CSGameObject*)p.Address;
            var cameraDirH = CameraManager.Instance()->GetActiveCamera()->DirH;

            if (IsKeyPressed(32)) // space to go up
            {
                Service.KeyState.SetRawValue(VirtualKey.SPACE, 0);
                var pos = p.Position;
                obj->SetPosition(pos.X, pos.Y + NoClipSpeed, pos.Z);
            }
            else if (IsKeyPressed(160)) // left shift to go down
            {
                Service.KeyState.SetRawValue(VirtualKey.LSHIFT, 0);
                var pos = p.Position;
                obj->SetPosition(pos.X, pos.Y - NoClipSpeed, pos.Z);
            }
            if (IsKeyPressed(87)) // W to go forward
            {
                Service.KeyState.SetRawValue(VirtualKey.W, 0);
                var pos = p.Position;
                var newPos = RotatePoint(pos.X, pos.Z, MathF.PI - cameraDirH, pos + new Vector3(0f, 0f, NoClipSpeed));
                SetPosition(ref newPos);
            }
            else if (IsKeyPressed(83)) // S to go backwards
            {
                Service.KeyState.SetRawValue(VirtualKey.S, 0);
                var pos = p.Position;
                var newPos = RotatePoint(pos.X, pos.Z, MathF.PI - cameraDirH, pos + new Vector3(0f, 0f, -NoClipSpeed));
                SetPosition(ref newPos);
            }
            if (IsKeyPressed(65)) // A to go left
            {
                Service.KeyState.SetRawValue(VirtualKey.A, 0);
                var pos = p.Position;
                var newPos = RotatePoint(pos.X, pos.Z, MathF.PI - cameraDirH, pos + new Vector3(NoClipSpeed, 0f, 0f));
                SetPosition(ref newPos);
            }
            else if (IsKeyPressed(68)) // D to go right
            {
                Service.KeyState.SetRawValue(VirtualKey.D, 0);
                var pos = p.Position;
                var newPos = RotatePoint(pos.X, pos.Z, MathF.PI - cameraDirH, pos + new Vector3(-NoClipSpeed, 0f, 0f));
                SetPosition(ref newPos);
            }

            void SetPosition(ref Vector3 pos) => obj->SetPosition(pos.X, pos.Y, pos.Z);
            static Vector3 RotatePoint(float cx, float cy, float angle, Vector3 p)
            {
                if (angle == default)
                {
                    return p;
                }
                var (sin, cos) = MathF.SinCos(angle);

                p.X -= cx;
                p.Z -= cy;

                var xnew = p.X * cos - p.Z * sin;
                var ynew = p.X * sin + p.Z * cos;

                p.X = xnew + cx;
                p.Z = ynew + cy;
                return p;
            }
            static bool IsKeyPressed(int key)
            {
                static bool IsBitSet(short b, int pos) => (b & (1 << pos)) != 0;
                return key != 0 && IsBitSet(PInvoke.User32.GetAsyncKeyState(key), 15);
            }
        }
    }
}
