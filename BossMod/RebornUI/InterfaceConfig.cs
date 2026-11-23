using RebornMaterial;

namespace BossMod.RebornUI;

[ConfigDisplay(Name = "界面", Order = -2, Tags = ["theme", "accent", "scale", "size", "material", "animation", "motion"])]
public sealed class InterfaceConfig : ConfigNode, IM3Settings
{
    [SectionStart("主题")]
    [PropertyDisplay("强调色", tooltip: "整个界面的配色由此颜色生成。可选择预设色，或点击最后一个色块自定义。")]
    public Color AccentColor = Color.FromRGBA(0x5BA7B7FF);

    [PropertyDisplay("文字大小", tooltip: "在 Dalamud 字体大小的基础上，缩放主题窗口中的文字。")]
    [PropertySlider(0.75f, 2f)]
    public float TextScale = 1f;

    [PropertyDisplay("控件大小", tooltip: "缩放按钮、开关、输入框等控件。")]
    [PropertySlider(0.75f, 2f)]
    public float ElementScale = 1f;

    [PropertyDisplay("控件间距", tooltip: "缩放控件周围及控件之间的留白，不改变控件本身大小。")]
    [PropertySlider(0.5f, 2f)]
    public float PaddingScale = 1f;

    [PropertyDisplay("动画时长", tooltip: "调整主题窗口动画的时长；设为 0 可关闭动画。")]
    [PropertySlider(0f, 2f)]
    public float MotionScale = 1f;

    [PropertyDisplay("紧凑布局", tooltip: "缩小留白和行高，让屏幕显示更多设置。")]
    public bool CompactSettings = false;

    [PropertyDisplay("直接显示说明", tooltip: "在设置名称下方显示说明。关闭后，将鼠标悬停在设置上即可查看。")]
    public bool InlineDescriptions = true;

    [PropertyDisplay("侧栏仅显示图标", tooltip: "将侧栏收窄为一列图标。将鼠标悬停在图标上即可查看名称。")]
    public bool IconSidebar = false;

    Vector4 IM3Settings.UiAccentColor
    {
        get => AccentColor.ToFloat4();
        set => AccentColor = Color.FromFloat4(value);
    }

    float IM3Settings.UiTextScale
    {
        get => TextScale;
        set => TextScale = value;
    }

    float IM3Settings.UiElementScale
    {
        get => ElementScale;
        set => ElementScale = value;
    }

    float IM3Settings.UiPaddingScale
    {
        get => PaddingScale;
        set => PaddingScale = value;
    }

    float IM3Settings.UiMotionScale
    {
        get => MotionScale;
        set => MotionScale = value;
    }

    bool IM3Settings.UiIconSidebar
    {
        get => IconSidebar;
        set => IconSidebar = value;
    }

    bool IM3Settings.UiDescriptionTooltips => !InlineDescriptions;
}
