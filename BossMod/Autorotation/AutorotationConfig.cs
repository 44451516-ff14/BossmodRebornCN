namespace BossMod.Autorotation;

[ConfigDisplay(Name = "自动循环", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("在游戏中显示 UI")]
    public bool ShowUI = false;

    public enum DtrStatus
    {
        [PropertyDisplay("禁用")]
        None,
        [PropertyDisplay("仅文本")]
        TextOnly,
        [PropertyDisplay("带图标")]
        Icon
    }

    [PropertyDisplay("在服务器信息条中显示自动循环预设")]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("隐藏 VBM 默认预设", tooltip: "如果您已创建了自己的预设且不再需要包含的默认预设，此选项将阻止它在自动旋转和预设编辑器窗口中显示。", since: "0.0.0.253")]
    public bool HideDefaultPreset = false;

    [PropertyDisplay("在游戏中显示位置提示", tooltip: "显示位置技能提示，指示移动到目标的侧面或后方")]
    public bool ShowPositionals = false;

    [PropertyDisplay("退出战斗时自动禁用自动循环")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("退出战斗时自动重新启用强制禁用的自动循环")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("提前拉怪阈值", tooltip: "如果有人在倒计时超过该值时与 Boss 进入战斗，则视为忍者拉怪，自动循环被强制禁用")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}