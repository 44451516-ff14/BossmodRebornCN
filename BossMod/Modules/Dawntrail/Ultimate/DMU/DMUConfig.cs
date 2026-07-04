namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public class DMUConfig : ConfigNode {

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("Normal Graven Image 2")]
        GravenImage2Normal,

        [PropertyDisplay("Uptime Graven Image 2")]
        GravenImage2Uptime,
    }

    [PropertyDisplay("P1 Graven Image 2 strategy")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2Uptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("Modified Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky arrow CW box (Merry Go Round)")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing strategy")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 Graven Image 3 Static Spots")]
    public bool P1GravenImage3Static = true;

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow braindead strategy using markerless")]
        Meow_Markerless,

        [PropertyDisplay("EU meow braindead strategy using DN ZENITH markers")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("NA Kroxy-Rinon (341 Melee Flex)")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;
}
