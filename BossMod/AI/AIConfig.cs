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

    [PropertyDisplay("启用自动离开(AFK)模式", tooltip: "如果处于非战斗状态，则启用自动离开模式。在离开状态下，AI将不会自动旋转或选择目标")]
    public bool AutoAFK = false;

    [PropertyDisplay("启用非战斗离开(AFK)模式", tooltip: "非战斗状态下等待指定秒数后启用离开模式。任何移动将重置计时器，或在离开模式已激活时禁用该模式")]
    public float AFKModeTimer = 10;

    public string? AIAutorotPresetName;
}
