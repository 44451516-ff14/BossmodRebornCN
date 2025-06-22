namespace BossMod;


[ConfigDisplay(Name = "BOSS模块和雷达", Order = 1)]

public sealed class BossModuleConfig : ConfigNode
{
    // BOSS模块设置
    [PropertyDisplay("模块加载的最低完成度", tooltip: "部分模块带有\"WIP\"（开发中）状态，修改此设置可强制加载这些模块")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("允许模块自动使用技能", tooltip: "例如：在击退发生前自动使用防击退技能")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("显示测试雷达和提示窗口", tooltip: "用于在不进入BOSS战的情况下配置雷达和提示窗口", separator: true)]
    public bool ShowDemo = false;

    // 雷达窗口设置
    [PropertyDisplay("启用雷达")]
    public bool Enable = true;

    [PropertyDisplay("锁定雷达和提示窗口的位置及鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay("透明雷达窗口背景", tooltip: "移除雷达周围的黑色背景（跨显示器时可能失效）")]
    public bool TrishaMode = true;

    [PropertyDisplay("为雷达竞技场添加不透明背景")]
    public bool OpaqueArenaBackground = true;

    [PropertyDisplay("显示雷达标记的轮廓和阴影")]
    public bool ShowOutlinesAndShadows = true;

    [PropertyDisplay("雷达竞技场缩放比例", tooltip: "雷达窗口中竞技场的显示比例")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1;
    
    
    [PropertyDisplay("雷达元素厚度[thickness]缩放因子", tooltip: "全局缩放雷达元素厚度[thickness]的厚度")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ThicknessScale = 1;

    [PropertyDisplay("根据镜头方向旋转雷达")]
    public bool RotateArena = true;

    [PropertyDisplay("当禁用旋转时镜像翻转180°")]
    public bool FlipArena = false;

    [PropertyDisplay("为雷达旋转预留额外空间", tooltip: "开启旋转功能时，可为雷达两侧预留空间以防止边缘裁剪，适应战斗中的镜头旋转或方向标识需求")]
    [PropertySlider(1, 2, Speed = 0.1f, Logarithmic = true)]
    public float SlackForRotations = 1.5f;

    [PropertyDisplay("显示雷达竞技场边框")]
    public bool ShowBorder = true;

    [PropertyDisplay("玩家处于危险区域时改变边框颜色", tooltip: "当可能受到机制攻击时，边框会从白色变为红色")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("在雷达上显示方位名称")]
    public bool ShowCardinals = false;

    [PropertyDisplay("方位名称字体大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float CardinalsFontSize = 17;

    [PropertyDisplay("标记字体大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float WaymarkFontSize = 22;

    [PropertyDisplay("角色三角标记缩放比例")]
    [PropertySlider(0.1f, 10, Speed = 0.1f)]
    public float ActorScale = 1;

    [PropertyDisplay("在雷达上显示标记")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("始终显示所有存活队友")]
    public bool ShowIrrelevantPlayers = false;

    [PropertyDisplay("根据职责着色未染色玩家标记")]
    public bool ColorPlayersBasedOnRole = false;

    [PropertyDisplay("始终显示焦点目标队友", separator: true)]
    public bool ShowFocusTargetPlayer = false;

    // 提示窗口设置
    [PropertyDisplay("在独立窗口显示文本提示", tooltip: "将提示窗口与雷达分离以便独立布局")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("独立提示窗口透明化")]
    public bool HintsInSeparateWindowTransparent = false;

    [PropertyDisplay("显示机制序列和计时提示")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("显示全屏机制提示")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("显示玩家专属提示和警告", separator: true)]
    public bool ShowPlayerHints = true;

    // 其他设置
    [PropertyDisplay("在游戏场景中显示移动指引", tooltip: "使用频率较低，可在游戏场景中显示箭头指示特定机制的移动方向")]
    public bool ShowWorldArrows = false;

    [PropertyDisplay("显示近战范围指示器")]
    public bool ShowMeleeRangeIndicator = false;
}