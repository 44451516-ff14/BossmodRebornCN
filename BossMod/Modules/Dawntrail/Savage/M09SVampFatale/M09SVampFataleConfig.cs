namespace BossMod.Dawntrail.Savage.M09SVampFatale;

[ConfigDisplay(Order = 0x140, Parent = typeof(DawntrailConfig))]
public sealed class M09SVampFataleConfig : ConfigNode
{
    //[PropertyDisplay("Enable force targetting adds", tooltip: "Prioritizes Deadly Doornail; melee move to flails once puddle is too big")]
    //public bool EnableForcedTarget = false;

    [PropertyDisplay("Show static Aetherletting positions (NA / Hector)", tooltip: "If party roles are configured, only show player's spot")]
    public bool ShowStaticAetherletting = false;

    [PropertyDisplay("Enable AI static Aetherletting spread positions (NA / Hector)", tooltip: "Will not work unless party roles are configured")]
    public bool EnableStaticAetherlettingPuddle = false;

    [PropertyDisplay("Show Hell In A Cell tower order (North CW / Hector)", tooltip: "If party roles are configured, only show own tower")]
    public bool ShowTowerOrder = false;

    [PropertyDisplay("Enable AI Hell In A Cell tower order (North CW / Hector)", tooltip: "Will not work unless party roles are configured")]
    public bool EnableTowerOrder = false;

    //[PropertyDisplay("Show Ultrasonic Amp/Spread bait positions (NA / Hector)")]
    //public bool ShowUltrasonicBait = false;

    [PropertyDisplay("Enable AI Undead Deathmatch (LP1 N/W)", tooltip: "Will not work unless party roles are configured")]
    public bool EnableDeathmatch = false;
}
