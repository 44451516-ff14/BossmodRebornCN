namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V20ShishuChochin;

[ConfigDisplay(Order = 0x100, Parent = typeof(EndwalkerConfig))]
public sealed class V20ShishuChochinConfig() : ConfigNode()
{
    [PropertyDisplay("启用路径12灯笼AI")]
    public bool P12LanternAI = false;
}
