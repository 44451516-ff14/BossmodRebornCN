using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using BossMod.Util;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace BossMod;

sealed class DebugTeleport
{
    private bool EnableNoClip;
    private float NoClipSpeed = 0.001f;
    private Vector3 inputCoordinates;

    public unsafe void Draw()
    {
        ImGui.BeginGroup();
        ImGui.Checkbox("No Clip", ref EnableNoClip);
        if (EnableNoClip)
        {
            Enable();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(150);
            ImGui.InputFloat("No Clip Speed", ref NoClipSpeed, 0.001f);
        }
        else
        {
            Disable();
        }
        ImGui.Separator();
        ImGui.EndGroup();
        ImGui.BeginGroup();
        ImGui.Text("Current Player Coordinates:");
        ImGui.Text("X: " + PlayerEx.Position.X.ToString("F3"));
        ImGui.Text("Y: " + PlayerEx.Position.Y.ToString("F3"));
        ImGui.Text("Z: " + PlayerEx.Position.Z.ToString("F3"));
        ImGui.EndGroup();
        ImGui.Separator();
        ImGui.BeginGroup();
        ImGui.Text("Enter Target Coordinates:");
        if (ImGui.Button("Set Position"))
        {
            SetPlayerPosition(inputCoordinates);
        }
        ImGui.SetNextItemWidth(150);
        ImGui.InputFloat("X Coordinate", ref inputCoordinates.X, 1f);
        ImGui.SetNextItemWidth(150);
        ImGui.InputFloat("Y Coordinate", ref inputCoordinates.Y, 1f);
        ImGui.SetNextItemWidth(150);
        ImGui.InputFloat("Z Coordinate", ref inputCoordinates.Z, 1f);
        ImGui.EndGroup();
    }

    private void SetPlayerPosition(Vector3 position)
    {
        try
        {
            if (Service.ClientState.LocalPlayer != null)
            {
                // Assuming PlayerEx.SetPosition accepts a Vector3
                PlayerEx.SetPosition = position;
                Service.Log($"Player position set to: X = {position.X}, Y = {position.Y}, Z = {position.Z}");
            }
            else
            {
                Service.Log("LocalPlayer is null. Unable to set position.");
            }
        }
        catch (Exception ex)
        {
            Service.Log($"An error occurred while setting position: {ex.Message}");
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
            if (Service.KeyState.GetRawValue(VirtualKey.SPACE) != 0 || Utils.IsKeyPressed(LimitedKeys.Space))
            {
                Service.KeyState.SetRawValue(VirtualKey.SPACE, 0);
                PlayerEx.SetPosition = (PlayerEx.Object.Position.X, PlayerEx.Object.Position.Y + NoClipSpeed, PlayerEx.Object.Position.Z).ToVector3();
            }
            if (Service.KeyState.GetRawValue(VirtualKey.LSHIFT) != 0 || Utils.IsKeyPressed(LimitedKeys.LeftShiftKey))
            {
                Service.KeyState.SetRawValue(VirtualKey.LSHIFT, 0);
                PlayerEx.SetPosition = (PlayerEx.Object.Position.X, PlayerEx.Object.Position.Y - NoClipSpeed, PlayerEx.Object.Position.Z).ToVector3();
            }
            if (Service.KeyState.GetRawValue(VirtualKey.W) != 0 || Utils.IsKeyPressed(LimitedKeys.W))
            {
                var newPoint = Utils.RotatePoint(PlayerEx.Object.Position.X, PlayerEx.Object.Position.Z, MathF.PI - PlayerEx.CameraEx->DirH, PlayerEx.Object.Position + new Vector3(0, 0, NoClipSpeed));
                Service.KeyState.SetRawValue(VirtualKey.W, 0);
                PlayerEx.SetPosition = newPoint;
            }
            if (Service.KeyState.GetRawValue(VirtualKey.S) != 0 || Utils.IsKeyPressed(LimitedKeys.S))
            {
                var newPoint = Utils.RotatePoint(PlayerEx.Object.Position.X, PlayerEx.Object.Position.Z, MathF.PI - PlayerEx.CameraEx->DirH, PlayerEx.Object.Position + new Vector3(0, 0, -NoClipSpeed));
                Service.KeyState.SetRawValue(VirtualKey.S, 0);
                PlayerEx.SetPosition = newPoint;
            }
            if (Service.KeyState.GetRawValue(VirtualKey.A) != 0 || Utils.IsKeyPressed(LimitedKeys.A))
            {
                var newPoint = Utils.RotatePoint(PlayerEx.Object.Position.X, PlayerEx.Object.Position.Z, MathF.PI - PlayerEx.CameraEx->DirH, PlayerEx.Object.Position + new Vector3(NoClipSpeed, 0, 0));
                Service.KeyState.SetRawValue(VirtualKey.A, 0);
                PlayerEx.SetPosition = newPoint;
            }
            if (Service.KeyState.GetRawValue(VirtualKey.D) != 0 || Utils.IsKeyPressed(LimitedKeys.D))
            {
                var newPoint = Utils.RotatePoint(PlayerEx.Object.Position.X, PlayerEx.Object.Position.Z, MathF.PI - PlayerEx.CameraEx->DirH, PlayerEx.Object.Position + new Vector3(-NoClipSpeed, 0, 0));
                Service.KeyState.SetRawValue(VirtualKey.D, 0);
                PlayerEx.SetPosition = newPoint;
            }
        }
    }
}

