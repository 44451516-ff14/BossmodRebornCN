using RebornMaterial;

namespace BossMod.RebornUI;

[ConfigDisplay(Name = "界面", Order = -2, Tags = ["theme", "accent", "scale", "size", "material", "animation", "motion"])]
public sealed class InterfaceConfig : ConfigNode, IM3Settings
{
    [SectionStart("主题")]
    [PropertyDisplay("强调色", tooltip: "BossMod Reborn 界面中的其他颜色均由此颜色生成。若选择灰色或黑色，则会改用默认颜色。")]
    public Color AccentColor = Color.FromRGBA(0x5BA7B7FF);

    [SectionStart("尺寸")]
    [PropertyDisplay("文字大小", tooltip: "在 Dalamud 字体大小的基础上，缩放主题窗口中的文字。")]
    [PropertySlider(0.5f, 3f)]
    public float TextScale = 1f;

    [PropertyDisplay("控件大小", tooltip: "缩放按钮、开关、输入框等控件。")]
    [PropertySlider(0.5f, 2.5f)]
    public float ElementScale = 1f;

    [PropertyDisplay("控件间距", tooltip: "缩放控件周围及控件之间的留白，不改变控件本身大小。")]
    [PropertySlider(0f, 3f)]
    public float PaddingScale = 1f;

    [PropertyDisplay("动画时长", tooltip: "调整主题窗口动画的时长；设为 0 可关闭动画。")]
    [PropertySlider(0f, 3f)]
    public float MotionScale = 1f;

    [PropertyDisplay("紧凑设置窗口", tooltip: "缩小设置窗口中的留白，以便在屏幕上显示更多内容。")]
    public bool CompactSettings = false;

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
}
