namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

[ConfigDisplay(Order = 0x120, Parent = typeof(DawntrailConfig))]
public sealed class M07SBruteAbombinatorConfig() : ConfigNode()
{
    [PropertyDisplay("Enable seed AOE prediction")]
    public bool EnableSeedPrediction = true;

    [PropertyDisplay("Enable lariat prediction")]
    public bool EnableLariatPrediction = true;
}
