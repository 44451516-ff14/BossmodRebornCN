namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

[ConfigDisplay(Order = 0x333, Parent = typeof(EndwalkerConfig))]
public sealed class C033SStaticeConfig() : ConfigNode()
{
    [PropertyDisplay("Darts 2: supports relative west, dd relative east")]
    public bool Fireworks2Invert = false;
}
