namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V20ShishuChochin;

[ConfigDisplay(Order = 0x100, Parent = typeof(EndwalkerConfig))]
public sealed class V20ShishuChochinConfig() : ConfigNode()
{
    [PropertyDisplay("Enable path 12 lantern AI")]
    public bool P12LanternAI = false;
}
