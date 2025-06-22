namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

[ConfigDisplay(Order = 0x020, Parent = typeof(DawntrailConfig))]
sealed class Ch01CloudOfDarknessConfig() : ConfigNode()
{
    [PropertyDisplay("Show occupied tiles on radar", tooltip: "Required for AI to not step on occupied tiles.")]
    public bool ShowOccupiedTiles = true;
}
