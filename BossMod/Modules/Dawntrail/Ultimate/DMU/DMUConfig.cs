namespace BossMod.Dawntrail.Ultimate.DMU;

[ConfigDisplay(Order = 0x400, Parent = typeof(DawntrailConfig))]
public sealed class DMUConfig : ConfigNode {
    [SectionStart("策略设置")]
    [PropertyDisplay("P1 刻印 1 处理策略")]
    public P1GravenImage1Strategy P1GravenImage1 = P1GravenImage1Strategy.GravenImage1LPDU;

    public enum P1GravenImage1Strategy {
        [PropertyDisplay("关闭提示")]
        GravenImage1None,

        [PropertyDisplay("LPDU")]
        GravenImage1LPDU,
    }

    [PropertyDisplay("P1 波形炮分组优先级", tooltip: "分组顺序由西向东")]
    [GroupDetails(["1", "2", "3", "4", "5", "6", "7", "8"])]
    [GroupPreset("HHTTMMRR", [3, 2, 1, 0, 4, 5, 6, 7])]
    public GroupAssignmentUnique P1WaveCannonAssignment = new() { Assignments = [3, 2, 1, 0, 4, 5, 6, 7] };

    [PropertyDisplay("P1 波形炮安全点提示", tooltip: "大致显示诱导站位")]
    public bool P1WaveCannonHints = false;

    public enum P1DoubleTroubleStrategy {
        [PropertyDisplay("关闭提示")]
        DoubleTroubleNone,

        [PropertyDisplay("LPDU")]
        DoubleTroubleLPDU
    }

    [PropertyDisplay("P1 Double Trouble 处理策略")]
    public P1DoubleTroubleStrategy P1DoubleTrouble = P1DoubleTroubleStrategy.DoubleTroubleLPDU;

    [PropertyDisplay("P1 Double Trouble 击退安全点提示", tooltip: "显示分摊大致站位，可能仍需微调")]
    public bool P1DoubleTroubleKnockbackHints = false;

    public enum P1GravenImage2Strategy {
        [PropertyDisplay("关闭提示")]
        GravenImage2None,

        [PropertyDisplay("LPDU 刻印 2 贪输出处理")]
        GravenImage2LPDUUptime,
    }

    [PropertyDisplay("P1 刻印 2 处理策略")]
    public P1GravenImage2Strategy P1GravenImage2 = P1GravenImage2Strategy.GravenImage2LPDUUptime;

    public enum P1TeleTrouncingStrategy {
        [PropertyDisplay("关闭提示")]
        TeleTrouncingNone,

        [PropertyDisplay("改版 Xolo")]
        Modified_Xolo,

        [PropertyDisplay("Freaky 顺时针箭头方块（旋转木马）")]
        Freaky_Arrow,
    }

    [PropertyDisplay("P1 TeleTrouncing 处理策略")]
    public P1TeleTrouncingStrategy P1TeleTrouncing = P1TeleTrouncingStrategy.Modified_Xolo;

    [PropertyDisplay("P1 TeleTrouncing 固定站位")]
    public bool P1TeleTrouncingStaticSpots = true;

    [PropertyDisplay("P1 刻印 3 处理策略")]
    public P1GravenImage3Strategy P1GravenImage3 = P1GravenImage3Strategy.GravenImage3LPDU;

    public enum P1GravenImage3Strategy {
        [PropertyDisplay("关闭提示")]
        GravenImage3None,

        [PropertyDisplay("LPDU")]
        GravenImage3LPDU,
    }

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

    [SectionStart("AI 设置")]
    [PropertyDisplay("P1 RevoltingRuinIII 始终以正北为基准？", tooltip: "仅供 AI 使用，目前尚未生效")]
    public bool P1RevoltingRuinIIIAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 RevoltingRuinIII 首次诱导", tooltip: "仅供 AI 使用，目前尚未生效")]
    [PropertyCombo("MT", "OT")]
    public bool P1RevoltingRuinIIIBait1OT = false;

    [PropertyDisplay("P1 HyperDrive 始终以正北为基准？", tooltip: "仅供 AI 使用，目前尚未生效")]
    public bool P1HyperDriveAlwaysAroundTrueNorth = true;

    [PropertyDisplay("P1 HyperDrive 首次诱导", tooltip: "仅供 AI 使用，目前尚未生效")]
    [PropertyCombo("MT", "OT")]
    public bool P1HyperDriveBait1OT = false;

    [SectionStart("调试设置")]
    [PropertyDisplay("P1 刻印 1 击退额外提示", tooltip: "仅用于调试：若北侧安全，H1 和 R1 仍需移动较远，其他职责只需稍作调整")]
    public bool P1GravenImage1KnockbackAdditionalHints = false;
}
