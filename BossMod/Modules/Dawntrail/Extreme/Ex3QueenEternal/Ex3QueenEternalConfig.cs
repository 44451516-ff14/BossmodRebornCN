namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

[ConfigDisplay(Order = 0x010, Parent = typeof(DawntrailConfig))]
sealed class Ex3QueenEternalConfig() : ConfigNode()
{
    [PropertyDisplay("Absolute Authority: ignore flares, stack together")]
    public bool AbsoluteAuthorityIgnoreFlares = true;

    [PropertyDisplay("Fixed bridge spots for West/East tethers", tooltip: "West/East stretch tethers without crossing is typically used on EU/NA partyfinder, the other option is usually used by JP")]
    [PropertyCombo("No crossing", "Cross (JP)")]
    public bool SideTethersCrossStrategy = false;
}
