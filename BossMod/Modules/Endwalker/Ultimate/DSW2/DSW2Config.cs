namespace BossMod.Endwalker.Ultimate.DSW2;

[ConfigDisplay(Order = 0x201, Parent = typeof(EndwalkerConfig))]
public sealed class DSW2Config() : ConfigNode()
{
    [PropertyDisplay("P2 圣域（充能）：队伍分配")]
    [GroupDetails(["West/Across", "East/Behind"])]
    [GroupPreset("Default light parties", [0, 1, 0, 1, 0, 1, 0, 1])]
    [GroupPreset("Inverted light parties", [1, 0, 1, 0, 1, 0, 1, 0])]
    public GroupAssignmentLightParties P2SanctityGroups = GroupAssignmentLightParties.DefaultLightParties();

    [PropertyDisplay("P2 圣域（充能）：队伍相对于暗黑骑士（对面/背后）而非绝对（西/东）")]
    public bool P2SanctityRelative = false;

    [PropertyDisplay("P2 圣域（充能）：负责平衡队伍的职责（如果未设置，则与职责搭档交换）")]
    public Role P2SanctitySwapRole;

    [PropertyDisplay("P2 圣域（陨石）：需要时自动使用防击退")]
    public bool P2Sanctity2AutomaticAntiKB = true;

    [PropertyDisplay("P2 圣域（陨石）：配对分配")]
    [GroupDetails(["North", "East", "South", "West"])]
    [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 E, H2/M2 W", [0, 2, 1, 3, 1, 3, 0, 2])]
    [GroupPreset("MT/R1 N, OT/R2 S, H1/M1 W, H2/M2 E", [0, 2, 3, 1, 3, 1, 0, 2])]
    public GroupAssignmentDDSupportPairs P2Sanctity2Pairs = GroupAssignmentDDSupportPairs.DefaultOneMeleePerPair();

    public enum P2PreyCardinals
    {
        [PropertyDisplay("始终 N/S")]
        AlwaysNS,

        [PropertyDisplay("始终 E/W")]
        AlwaysEW,

        [PropertyDisplay("N/S，除非两个猎物都从 E & W 开始")]
        PreferNS,

        [PropertyDisplay("E/W，除非两个猎物都从 N & S 开始")]
        PreferEW,
    }

    [PropertyDisplay("P2 圣域（陨石）：猎物目标的偏好方位")]
    public P2PreyCardinals P2Sanctity2PreyCardinals;

    [PropertyDisplay("P2 圣域（陨石）：即使对于120度模式也强制使用偏好方位（交换更简单，但移动更复杂）")]
    public bool P2Sanctity2ForcePreferredPrey = true;

    public enum P2PreySwapDirection
    {
        [PropertyDisplay("所有猎物职责顺时针旋转")]
        RotateCW,

        [PropertyDisplay("所有猎物职责逆时针旋转")]
        RotateCCW,

        [PropertyDisplay("配对：N <-> E, S <-> W")]
        PairsNE,

        [PropertyDisplay("配对：N <-> W, S <-> E")]
        PairsNW,
    }

    [PropertyDisplay("P2 圣域（陨石）：如果两个猎物目标都在错误方位则交换方向")]
    public P2PreySwapDirection P2Sanctity2SwapDirection;

    [PropertyDisplay("P2 圣域（陨石）：猎物职责的偏好外塔")]
    [PropertyCombo("CCW (如果面向外侧则为最左)", "CW (如果面向外侧则为最右)")]
    public bool P2Sanctity2PreferCWTowerAsPrey = true;

    public enum P2OuterTowers
    {
        [PropertyDisplay("不尝试分配外塔")]
        None,

        [PropertyDisplay("始终使用偏好方向")]
        AlwaysPreferred,

        [PropertyDisplay("如果角度更好，猎物目标都使用共同相反方向；没有猎物目标的象限中的玩家仍使用偏好方向")]
        SynchronizedTargets,

        [PropertyDisplay("如果角度更好，猎物目标都使用共同相反方向；所有象限中的玩家使用相同方向")]
        SynchronizedRole,

        [PropertyDisplay("猎物目标使用任何给出最佳角度的方向；没有猎物目标的象限中的玩家仍使用偏好方向")]
        Individual
    }

    [PropertyDisplay("P2 圣域（陨石）：外塔分配策略")]
    public P2OuterTowers P2Sanctity2OuterTowers = P2OuterTowers.Individual;

    public enum P2InnerTowers
    {
        [PropertyDisplay("不尝试分配内塔")]
        None,

        [PropertyDisplay("分配最近的无歧义内塔")]
        Closest,

        [PropertyDisplay("分配第一个未被更近的人分配的顺时针内塔")]
        CW,
    }

    [PropertyDisplay("P2 圣域（陨石）：内塔分配策略")]
    public P2InnerTowers P2Sanctity2InnerTowers = P2InnerTowers.CW;

    [PropertyDisplay("P2 圣域（陨石）：非猎物职责第二个塔的间方位")]
    [PropertyCombo("CCW", "CW")]
    public bool P2Sanctity2NonPreyTowerCW = false;

    [PropertyDisplay("P3 恩典俯冲：向西看箭头而非向东（因此前箭头占据E位置，后箭头占据W位置）")]
    public bool P3DiveFromGraceLookWest = false;

    [PropertyDisplay("P3 enumeration towers: assignments")]
    [GroupDetails(["NW Flex", "NE Flex", "SE Flex", "SW Flex", "NW Stay", "NE Stay", "SE Stay", "SW Stay"])]
    [GroupPreset("LPDU", [1, 3, 6, 0, 2, 4, 5, 7])]
    [GroupPreset("LPDU but CCW", [0, 2, 5, 7, 1, 3, 4, 6])]
    [GroupPreset("NA", [1, 3, 4, 6, 0, 2, 5, 7])]
    public GroupAssignmentUnique P3DarkdragonDiveCounterGroups = GroupAssignmentUnique.Default();

    [PropertyDisplay("P3 enumeration towers: prefer flexing to CCW tower (rather than to CW)")]
    public bool P3DarkdragonDiveCounterPreferCCWFlex = false;

    public enum P6MortalVow
    {
        [PropertyDisplay("不假设任何顺序")]
        None,

        [PropertyDisplay("LPDU: MT->OT->M1 (M2作为后备)->R1")]
        TanksMeleeR1,

        [PropertyDisplay("LPDU: MT->OT->M1 (M2作为后备)->R2")]
        TanksMeleeR2,
    }

    [PropertyDisplay("P6 凡人誓约传递顺序")]
    public P6MortalVow P6MortalVowOrder = P6MortalVow.None;
}
