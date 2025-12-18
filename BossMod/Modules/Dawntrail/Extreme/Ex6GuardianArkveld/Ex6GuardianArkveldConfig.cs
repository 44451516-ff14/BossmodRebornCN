namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

[ConfigDisplay(Order = 0x150, Parent = typeof(DawntrailConfig))]
public class Ex6GuardianArkveldConfig : ConfigNode
{
    public enum LimitCutStrategy
    {
        [PropertyDisplay("在方位点轮流（1 西/东）")]
        Circle,
        [PropertyDisplay("偶数北，奇数南")]
        EvenNorth,
    }

    [PropertyDisplay("极限切割定位提示")]
    public LimitCutStrategy LimitCutHints = LimitCutStrategy.Circle;
}
