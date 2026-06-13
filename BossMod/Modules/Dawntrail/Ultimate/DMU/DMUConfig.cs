namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public class DMUConfig : ConfigNode {

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow braindead strategy using markerless")]
        Meow_Markerless,

        [PropertyDisplay("EU meow braindead strategy using DN ZENITH markers")]
        Meow_DN_ZENITH_Markers,
    }

    [PropertyDisplay("P2 Forsaken strategy")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;
}
