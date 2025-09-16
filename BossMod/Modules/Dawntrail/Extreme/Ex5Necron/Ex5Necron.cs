namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class Wipe(BossModule module) : BossComponent(module)
{
    public bool Wiped;
    public override bool KeepOnPhaseChange => true;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000029)
        {
            Wiped = true;
        }
    }
}

sealed class Intermission(BossModule module) : BossComponent(module)
{
    public bool Started;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (param2 == default && updateID == 0x8000000C)
        {
            Started = true;
        }
    }
}

sealed class BlueShockwave(BossModule module) : Components.TankSwap(module, (uint)AID.BlueShockwaveVisual1, (uint)AID.BlueShockwave, (uint)AID.BlueShockwave, 1.2d, 5.2d, new AOEShapeCone(100f, 50f.Degrees()));
sealed class ChokingGrasp(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChokingGraspAOE1, (uint)AID.ChokingGraspAOE2], Rect)
{
    public static readonly AOEShapeRect Rect = new(24f, 3f);
}

sealed class CircleOfLives(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleOfLives, new AOEShapeDonut(3f, 50f), 1);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Necron, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1062u, NameID = 14093u, PlanLevel = 100, Category = BossModuleInfo.Category.Extreme, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class Ex5Necron(WorldState ws, Actor primary) : Trial.T05Necron.Necron(ws, primary);
