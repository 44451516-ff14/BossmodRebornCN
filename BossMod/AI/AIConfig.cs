namespace BossMod.AI;


[ConfigDisplay(Name = "AI 配置 (AI 处于非常实验阶段，请自行承担风险！)", Order = 7)]
sealed class AIConfig : ConfigNode
{
    [PropertyDisplay("在 DTR 条中显示状态")]
    public bool ShowDTR = false;

    [PropertyDisplay("显示 AI 界面")]
    public bool DrawUI = true;

    // [PropertyDisplay("将目标设置为焦点")]
    // public bool FocusTargetLeader = true;

    [PropertyDisplay("目标设置为焦点-Focus target master")]
    public bool FocusTargetMaster = false;

    [PropertyDisplay("将按键广播到其他窗口", tooltip: "Can cause hitching on some computers. Only enable it if it is actually needed! It is only useful for multiboxers.")]
    public bool BroadcastToSlaves = false;

    [PropertyDisplay("跟随小队位置")]
    public int FollowSlot = 0;

    [PropertyDisplay("禁止动作")]
    public bool ForbidActions = false;

    [PropertyDisplay("手动目标选择-Manual targeting")]
    public bool ManualTarget = false;

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


    [PropertyDisplay("跟随目标时期望位置(Any任意,Flank侧面,Rear背面,Front正面)")]
    // [PropertyCombo(["任何", "侧面", "后方", "前方"])]
    [PropertyCombo(["Any", "Flank", "Rear", "Front"])]
    public Positional DesiredPositional = Positional.Any;

    [PropertyDisplay("到插槽的最大距离")]
    public float MaxDistanceToSlot = 1;

    [PropertyDisplay("到目标的最大距离")]
    public float MaxDistanceToTarget = 2.6f;

    [PropertyDisplay("启用自动离开(AFK)模式", tooltip: "如果处于非战斗状态，则启用自动离开模式。在离开状态下，AI将不会自动旋转或选择目标")]
    public bool AutoAFK = false;


    [PropertyDisplay("启用非战斗离开(AFK)模式", tooltip: "非战斗状态下等待指定秒数后启用离开模式。任何移动将重置计时器，或在离开模式已激活时禁用该模式")]
    public float AFKModeTimer = 10;
    [PropertyDisplay("到碰撞箱的最小距离")]
    public float MinDistance = default;

    [PropertyDisplay("到禁止区域的偏好距离")]
    public float PreferredDistance = default;



    [PropertyDisplay("禁用障碍物地图加载-Disable loading obstacle maps", tooltip: "部分内容（如深层迷宫）可能需要启用此选项。")]
    public bool DisableObstacleMaps = false;

    [PropertyDisplay("移动决策延迟-Movement decision delay", tooltip: "谨慎修改此值并保持较低数值！过高可能导致无法及时应对某些机制。请注意根据不同内容调整此值。")]
    public double MoveDelay = 0;


    [PropertyDisplay("骑乘时保持静止-Idle while mounted")]
    public bool ForbidAIMovementMounted = false;

    [PropertyDisplay("将斜杠命令回显到聊天")]
    public bool EchoToChat = true;

    public string? AIAutorotPresetName;
}
