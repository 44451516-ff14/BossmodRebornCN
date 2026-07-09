namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public class DMUConfig : ConfigNode {

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("P1 刻印 2 常规处理")]
        GravenImage2Normal,

        [PropertyDisplay("P1 刻印 2 贪输出处理")]
        GravenImage2Uptime,
    }

    [PropertyDisplay("P1 刻印 2 处理策略")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2Uptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("改版 Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky 顺时针箭头方块（旋转木马）")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing 处理策略")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 刻印 3 固定站位")]
    public bool P1GravenImage3Static = true;

    public enum P2ForsakenStrategy {
        [PropertyDisplay("EU meow 无脑打法（无标点）")]
        Meow_Markerless,

        [PropertyDisplay("EU meow 无脑打法（DN ZENITH 标点）")]
        Meow_DN_ZENITH_Markers,

        [PropertyDisplay("NA Kroxy-Rinon（341 近战机动）")]
        Kroxy_Rinon_Melee_Flex,
    }

    [PropertyDisplay("P2 Forsaken 处理策略")]
    public P2ForsakenStrategy P2Forsaken = P2ForsakenStrategy.Meow_Markerless;
}
