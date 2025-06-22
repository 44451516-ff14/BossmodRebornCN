namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

[ConfigDisplay(Order = 0x100, Parent = typeof(DawntrailConfig))]
public sealed class M05SDancingGreenConfig() : ConfigNode()
{
    [PropertyDisplay("Draw moving exaflare pattern", tooltip: "If disabled the full Ride the Waves exaflare pattern is drawn from the beginning. Otherwise it will move like in normal mode.")]
    public bool MovingExaflares = true;

    [PropertyDisplay("Show all spotlight positions of same order", tooltip: "If enabled all spotlight positions are shown that match your own order.")]
    public bool ShowFromSameOrder = true;
    [PropertyDisplay("Show all spotlight positions of different order", tooltip: "If enabled all spotlight positions are shown that do not match your own order.")]
    public bool ShowFromDifferentOrder = false;

    [PropertyDisplay("Time left before spotlights get drawn")]
    [PropertySlider(0.1f, 34, Speed = 1)]
    public float SpotlightTimer = 34;
}
