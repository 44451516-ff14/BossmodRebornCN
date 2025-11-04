namespace BossMod.Autorotation;

[ConfigDisplay(Name = "自动循环配置", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("显示游戏内界面")]
    public bool ShowUI = false;

    public enum DtrStatus
    {
        [PropertyDisplay("禁用")]
        None,
        [PropertyDisplay("仅文字")]
        TextOnly,
        [PropertyDisplay("带图标")]
        Icon
    }

    [PropertyDisplay("在服务器信息栏显示当前预设")]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("隐藏VBM默认预设", tooltip: "创建自定义预设后，启用此选项可隐藏内置的默认预设")]
    public bool HideDefaultPreset = false;

    [PropertyDisplay("显示定位指引标记", tooltip: "显示定位技能提示，指示应移动至目标侧面或背面")]
    public bool ShowPositionals = false;


    [PropertyDisplay("脱战后自动禁用自动循环")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("脱战后自动重置强制禁用状态")]
    public bool ClearPresetOnDeath = true;

    [PropertyDisplay ("若引诱陷阱触发则自动禁用自动循环", tooltip: "仅适用于深层地下城")]
    public bool ClearPresetOnLuring = false;

    [PropertyDisplay ("脱离战斗时自动重新启用被强制禁用的自动循环")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("提前开怪判定阈值", tooltip: "当队伍成员在倒计时剩余时间超过此值时进入战斗，将被判定为提前开怪并强制禁用自动循环")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}
