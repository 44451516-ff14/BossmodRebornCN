namespace BossMod.AI;


[ConfigDisplay(Name = "AI 配置 (AI 处于非常实验阶段，请自行承担风险！)", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("在 DTR 条中显示状态")]
    public bool ShowDTR = false;

    [PropertyDisplay("显示 AI 界面")]
    public bool DrawUI = true;

    [PropertyDisplay("将目标设置为焦点")]
    public bool FocusTargetLeader = true;

    [PropertyDisplay("将按键广播到其他窗口")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("跟随小队位置")]
    public int FollowSlot = 0;

    [PropertyDisplay("禁止动作")]
    public bool ForbidActions = false;

    [PropertyDisplay("禁止移动")]
    public bool ForbidMovement = false;

    [PropertyDisplay("战斗中跟随")]
    public bool FollowDuringCombat = false;

    [PropertyDisplay("在主动 Boss 模块期间跟随")]
    public bool FollowDuringActiveBossModule = false;

    [PropertyDisplay("战斗外跟随")]
    public bool FollowOutOfCombat = false;

    [PropertyDisplay("跟随目标")]
    public bool FollowTarget = false;

    [PropertyDisplay("跟随目标时期望位置")]
    [PropertyCombo(["任何", "侧面", "后方", "前方"])]
    public Positional DesiredPositional = Positional.Any;

    [PropertyDisplay("到插槽的最大距离")]
    public float MaxDistanceToSlot = 1;

    [PropertyDisplay("到目标的最大距离")]
    public float MaxDistanceToTarget = 2.6f;


    public string? AIAutorotPresetName;
}
