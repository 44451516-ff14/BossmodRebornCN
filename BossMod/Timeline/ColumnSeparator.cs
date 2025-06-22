using ImGuiNET;

namespace BossMod;

public sealed class ColumnSeparator : Timeline.Column
{
    public uint Color;

    public ColumnSeparator(Timeline timeline, uint color = 0, float width = 1)
        : base(timeline)
    {
        Width = width;
        Color = color == 0 ? Colors.TextColor1 : color;
    }

    public override void Draw()
    {
        var trackMin = Timeline.ColumnCoordsToScreenCoords(0, Timeline.MinVisibleTime);
        var trackMax = Timeline.ColumnCoordsToScreenCoords(Width, Timeline.MaxVisibleTime);
        ImGui.GetWindowDrawList().AddRectFilled(trackMin, trackMax, Color);
    }
}
