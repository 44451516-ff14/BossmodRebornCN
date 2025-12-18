namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

[ConfigDisplay(Order = 0x130, Parent = typeof(DawntrailConfig))]
public sealed class M08SHowlingBladeConfig() : ConfigNode()
{
    [PropertyDisplay("显示平台编号")]
    public bool ShowPlatformNumbers = true;

    [PropertyDisplay("平台编号颜色：")]
    public Color[] PlatformNumberColors = [new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff), new(0xffffffff)];

    [PropertyDisplay("平台编号字体大小")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float PlatformNumberFontSize = 22;

    public enum ReignStrategy
    {
        [PropertyDisplay("显示当前职责的两个安全点")]
        Any,
        [PropertyDisplay("假设从竞技场中心看向BOSS时 G1 左，G2 右")]
        Standard,
        [PropertyDisplay("假设从竞技场中心看向BOSS时 G1 右，G2 左")]
        Inverse,
        [PropertyDisplay("无")]
        Disabled
    }

    [PropertyDisplay("革命/显赫统治定位提示")]
    public ReignStrategy ReignHints = ReignStrategy.Standard;

    [PropertyDisplay("显示孤狼哀歌中 Rinon/毒友塔位置")]
    public bool LoneWolfsLamentHints = true;
}
