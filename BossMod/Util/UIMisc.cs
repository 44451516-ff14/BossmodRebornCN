using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public static class UIMisc
{
    public static bool Button(string label, float width, params (bool disabled, string reason)[] disabled)
    {
        var len = disabled.Length;
        var isDisabled = false;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var d = ref disabled[i];
            if (d.disabled)
            {
                isDisabled = true;
                break;
            }
        }
        using var disable = ImRaii.Disabled(isDisabled);
        var res = ImGui.Button(label, new(width, default));
        if (isDisabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tooltip = ImRaii.Tooltip();
            for (var i = 0; i < len; ++i)
            {
                ref readonly var d = ref disabled[i];
                if (d.disabled)
                {
                    ImGui.TextUnformatted(d.reason);
                }
            }
        }
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Button(string label, bool disabled, string reason, float width = default) => Button(label, width, (disabled, reason));

    // button that is disabled unless shift is held, useful for 'dangerous' operations like deletion
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DangerousButton(string label, float width = default) => Button(label, !ImGui.IsKeyDown(ImGuiKey.ModShift), "Hold shift", width);

    public static void TextUnderlined(Vector4 colour, string text)
    {
        var size = ImGui.CalcTextSize(text);
        var cur = ImGui.GetCursorScreenPos();
        cur.Y += size.Y;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        cur.X += size.X;
        ImGui.GetWindowDrawList().PathLineTo(cur);
        ImGui.GetWindowDrawList().PathStroke(ImGui.ColorConvertFloat4ToU32(colour));
        ImGui.TextColored(colour, text);
    }

    public static void Image(ISharedImmediateTexture? icon, Vector2 size)
    {
        var wrap = icon?.GetWrapOrDefault();
        if (wrap != null)
        {
            ImGui.Image(wrap.Handle, size);
        }
        else
        {
            ImGui.Dummy(size);
        }
    }

    public static bool ImageToggleButton(ISharedImmediateTexture? icon, Vector2 size, bool state, string text)
    {
        var cursor = ImGui.GetCursorPos();
        var padding = ImGui.GetStyle().FramePadding;
        ImGui.SetCursorPos(new(cursor.X + size.X + 2f * padding.X, cursor.Y + 0.5f * (size.Y - ImGui.GetFontSize())));
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(cursor);

        var wrap = icon?.GetWrapOrDefault();
        if (wrap != null)
        {
            Vector4 tintColor = state ? new(1f, 1f, 1f, 1f) : new(0.5f, 0.5f, 0.5f, 0.85f);
            return ImGui.ImageButton(wrap.Handle, size, Vector2.Zero, Vector2.One, 1, Vector4.Zero, tintColor);
        }
        else
        {
            return ImGui.Button($"##ImageToggle_{text}", size);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, string id) => IconButtonRaw($"{icon.ToIconString()}##{id}");
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon) => IconButtonRaw(icon.ToIconString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IconButtonRaw(string text)
    {
        using (ImRaii.PushFont(Service.IconFont))
            return ImGui.Button(text);
    }

    public static bool IconButtonWithText(FontAwesomeIcon icon, string text)
    {
        bool button;

        Vector2 iconSize;
        using (ImRaii.PushFont(Service.IconFont))
            iconSize = ImGui.CalcTextSize(icon.ToIconString());

        var textStr = text;
        if (textStr.Contains('#', StringComparison.Ordinal))
            textStr = textStr[..textStr.IndexOf('#', StringComparison.Ordinal)];

        var framePadding = ImGui.GetStyle().FramePadding;
        var iconPadding = 3 * ImGuiHelpers.GlobalScale;

        var cursor = ImGui.GetCursorScreenPos();

        using (ImRaii.PushId(text))
        {
            var textSize = ImGui.CalcTextSize(textStr);
            var width = iconSize.X + textSize.X + framePadding.X * 2 + iconPadding;
            var height = ImGui.GetFrameHeight();

            button = ImGui.Button(string.Empty, new Vector2(width, height));
        }

        var iconPos = cursor + framePadding;
        var textPos = new Vector2(iconPos.X + iconSize.X + iconPadding, cursor.Y + framePadding.Y);

        var dl = ImGui.GetWindowDrawList();

        using (ImRaii.PushFont(Service.IconFont))
            dl.AddText(iconPos, ImGui.GetColorU32(ImGuiCol.Text), icon.ToIconString());

        dl.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), textStr);

        return button;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void IconText(FontAwesomeIcon icon)
    {
        using var scope = ImRaii.PushFont(Service.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
    }

    public static void HelpMarker(Func<string> helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle)
    {
        IconText(icon);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            using var tooltip = ImRaii.Tooltip();
            using var wrap = ImRaii.TextWrapPos(ImGui.GetFontSize() * 35f);
            ImGui.TextUnformatted(helpText());
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void HelpMarker(string helpText, FontAwesomeIcon icon = FontAwesomeIcon.InfoCircle) => HelpMarker(() => helpText, icon);

    /// <summary>
    /// Draw rotated text at the current cursor position. The origin of rotation is the point on the left edge of the text's bounding box and at its vertical midpoint.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="radians"></param>
    public static unsafe void TextRotated(ImU8String text, float radians)
    {
        var dl = ImGui.GetWindowDrawList();
        var ts = ImGui.CalcTextSize(text, false);
        var startVert = dl.VtxCurrentIdx;
        var screenPos = ImGui.GetCursorScreenPos();
        dl.AddText(screenPos - new Vector2(0, ts.Y * 0.5f), ImGui.GetColorU32(ImGuiCol.Text), text);

        var endVert = dl.VtxCurrentIdx;

        for (var i = (int)startVert; i < endVert; ++i)
        {
            ref var vert = ref dl.Handle->VtxBuffer.Ref(i);
            vert.Pos = Rotate(vert.Pos - screenPos, radians) + screenPos;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector2 Rotate(Vector2 vec, float rad)
    {
        return new WDir(vec).Rotate(new Angle(rad)).ToVec2();
    }
}
