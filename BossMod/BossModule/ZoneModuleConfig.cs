namespace BossMod;

[ConfigDisplay(Name = "全职自动化?-Full duty automation", Order = 6)]
public sealed class ZoneModuleConfig : ConfigNode
{
    [PropertyDisplay("加载区域模块所需的最低完成度")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("启用自动执行任务战斗/单人任务")]
    public bool EnableQuestBattles = false;

    [PropertyDisplay("在游戏世界中绘制路径点")]
    public bool ShowWaypoints = false;

    [PropertyDisplay("使用冲刺技能进行导航（Smudge、Elusive Jump, etc）")]
    public bool UseDash = false;

    [PropertyDisplay("显示xan调试UI")]
    public bool ShowXanDebugger = false;

    [PropertyDisplay ("锁定区域模块窗口移动及鼠标交互")]
    public bool Lock = false;

    [PropertyDisplay ("使区域模块窗口透明", tooltip: "移除区域模块窗口周围的黑色边框，若将雷达移至其他显示器则此功能无效")]
    public bool TransparentMode = false;
}
