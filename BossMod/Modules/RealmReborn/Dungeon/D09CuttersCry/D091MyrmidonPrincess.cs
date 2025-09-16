namespace BossMod.RealmReborn.Dungeon.D09CuttersCry.D091MyrmidonPrincess;

public enum OID : uint
{
    MyrmidonPrincess = 0x48FB, // R3.0
    MyrmidonSoldier = 0x48FE, // R1.05
    MyrmidonGuard = 0x48FD, // R1.95
    MyrmidonMarshal = 0x48FC, // R2.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // MyrmidonPrincess/MyrmidonSoldier/MyrmidonGuard/MyrmidonMarshal->player, no cast, single-target

    StoneIIVisual = 44226, // MyrmidonPrincess->self, 2.3+0,7s cast, single-target
    StoneII = 44227, // Helper->location, 3.0s cast, range 5 circle
    Silence = 44228, // MyrmidonPrincess->player, 5.0s cast, single-target, interrupt
    MandibleBite = 44229, // MyrmidonPrincess->self, 2.5s cast, range 6+R 90-degree cone
    TrapJaws = 44231, // MyrmidonSoldier/MyrmidonGuard->player, no cast, single-target
    Protection = 44230 // MyrmidonMarshal->MyrmidonPrincess, no cast, single-target
}

sealed class MandibleBite(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MandibleBite, new AOEShapeCone(9f, 45f.Degrees()));
sealed class StoneII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StoneII, 5f);
sealed class Silence(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Silence, showNameInHint: true);

sealed class D091MyrmidonPrincessStates : StateMachineBuilder
{
    public D091MyrmidonPrincessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StoneII>()
            .ActivateOnEnter<Silence>()
            .ActivateOnEnter<MandibleBite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), Chuggalo", PrimaryActorOID = (uint)OID.MyrmidonPrincess, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12u, NameID = 1585u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.RealmReborn, SortOrder = 1)]
public sealed class D091MyrmidonPrincess(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly uint[] trash = [(uint)OID.MyrmidonMarshal, (uint)OID.MyrmidonSoldier, (uint)OID.MyrmidonGuard];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(-29.24f, 177.93f), new(-11.23f, 190.65f),
    new(-10.72f, 190.55f), new(-10.20f, 190.18f), new(-9.60f, 189.97f),
    new(-8.97f, 189.85f), new(-7.67f, 190.15f), new(-7.09f, 190.56f), new(-4.98f, 197.15f), new(-4.77f, 197.73f),
    new(-4.16f, 198.93f), new(-3.91f, 200.31f), new(-3.93f, 203.00f), new(-4.25f, 204.28f), new(-5.73f, 206.57f),
    new(-5.81f, 207.12f), new(-5.62f, 208.37f), new(-5.90f, 208.81f), new(-15.81f, 216.56f), new(-16.20f, 217.00f),
    new(-16.23f, 217.66f), new(-16.18f, 218.16f), new(-20.58f, 220.47f), new(-21.09f, 220.88f), new(-21.33f, 221.45f),
    new(-21.78f, 221.97f), new(-22.43f, 222.27f), new(-22.80f, 222.72f), new(-23.06f, 223.15f), new(-23.56f, 223.24f),
    new(-24.25f, 223.20f), new(-24.84f, 223.24f), new(-26.83f, 224.89f), new(-27.53f, 224.88f), new(-28.77f, 225.05f),
    new(-29.39f, 224.90f), new(-30.04f, 224.59f), new(-30.57f, 224.16f), new(-32.23f, 223.05f), new(-32.84f, 223.01f),
    new(-33.48f, 223.13f), new(-34.17f, 223.15f), new(-35.55f, 223.01f), new(-49.30f, 206.21f), new(-49.42f, 205.62f),
    new(-50.09f, 204.52f), new(-50.31f, 203.86f), new(-50.49f, 202.56f), new(-50.39f, 199.94f), new(-50.27f, 199.33f),
    new(-48.16f, 195.99f), new(-48.42f, 195.53f), new(-48.43f, 194.87f), new(-43.38f, 185.97f), new(-42.85f, 185.55f),
    new(-41.67f, 184.94f), new(-40.48f, 184.47f), new(-39.83f, 184.31f), new(-37.77f, 184.20f), new(-35.23f, 184.54f),
    new(-34.67f, 184.99f), new(-34.30f, 185.57f), new(-33.12f, 184.63f), new(-33.03f, 182.80f), new(-32.69f, 182.18f),
    new(-32.18f, 181.78f), new(-31.99f, 180.41f), new(-31.72f, 179.75f), new(-31.16f, 179.32f), new(-30.63f, 178.97f),
    new(-29.74f, 178.02f), new(-29.24f, 177.93f)])]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MyrmidonMarshal => 2,
                (uint)OID.MyrmidonPrincess => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }
}
