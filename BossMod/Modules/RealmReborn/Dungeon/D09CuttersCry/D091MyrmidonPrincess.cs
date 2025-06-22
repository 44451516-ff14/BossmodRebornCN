namespace BossMod.RealmReborn.Dungeon.D09CuttersCry.D091MyrmidonPrincess;
// TODO: Revist when it gets duty support to finish.
public enum OID : uint
{
    // Boss
    Boss = 0x646, // Myrimidon Princess

    // Trash
    MyrmidonMarshal = 0x647,
    MyrmidonSoldier = 0x649
}

public enum AID : uint
{
    // Boss
    AutoAttack = 870, // Boss->player, no cast, single target
    MandibleBite = 1109, // Boss->self, 2.5s cast, range 9.0 90-degree cone
    Silence = 307, // Boss->player, 2.5s cast, random single-target
    TrapJaws = 523, // Boss->player, no cast, single target
    Haste = 744, // Boss->enemy, 2.5s cast, single target

    // Trash
    AutoAttackTrash = 870, // Trash->player, no cast, single target
    FormicPheromones = 1110 // MyrmidonMarshal->Boss, 4.5s cast, single target
}

class MandibleBite(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MandibleBite, new AOEShapeCone(9f, 45f.Degrees()));

class D091MyrmidonPrincessStates : StateMachineBuilder
{
    public D091MyrmidonPrincessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MandibleBite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12, NameID = 1585)]
public class D091MyrmidonPrincess(WorldState ws, Actor primary) : BossModule(ws, primary, new(-20f, 200f), new ArenaBoundsCircle(35f))
{
    private static readonly uint[] trash = [(uint)OID.MyrmidonMarshal, (uint)OID.MyrmidonSoldier];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MyrmidonMarshal => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(trash));
    }
}
