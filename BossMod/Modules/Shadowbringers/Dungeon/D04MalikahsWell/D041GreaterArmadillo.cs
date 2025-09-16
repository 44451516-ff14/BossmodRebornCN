namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D041GreaterArmadillo;

public enum OID : uint
{
    Boss = 0x2679, // R=4.0
    MorningStar = 0x267E, // R=1.68
    PackArmadillo = 0x2683, // R=2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // PackArmadillo->player, no cast, single-target

    StoneFlail = 15589, // Boss->player, 4.5s cast, single-target
    FallingRock = 15594, // Helper->location, 3.0s cast, range 4 circle
    HeadToss = 15590, // Boss->player, 5.0s cast, range 6 circle
    RightRoundVisual = 15591, // Boss->MorningStar, 2.5s cast, single-target
    RightRound = 15592, // Helper->self, no cast, range 9 circle, knockback 20, away from source
    FlailSmash = 15593, // Boss->location, 3.0s cast, range 40 circle, distance based
    Earthshake = 15929, // Helper->self, 3.5s cast, range 10-20 donut
    Rehydration = 16776 // PackArmadillo->self, 5.0s cast, single-target
}

class StoneFlail(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StoneFlail);
class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
class FlailSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlailSmash, 10f);
class HeadToss(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HeadToss, 6f, 4, 4);
class Earthshake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthshake, new AOEShapeDonut(10f, 20f));
class Rehydration(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Rehydration, showNameInHint: true);

class RightRound(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RightRoundVisual)
        {
            // approximation of the mechanic with a capsule since the jump seems to behave quite unpredictable for long distances to the morning star
            var len = (spell.LocXZ - caster.Position).Length();
            var maxLen = len > 10f ? 10f : len;
            _aoe = [new(new AOEShapeCapsule(9f, maxLen), spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 0.9d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RightRound)
        {
            _aoe = [];
        }
    }
}

class D041GreaterArmadilloStates : StateMachineBuilder
{
    public D041GreaterArmadilloStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RightRound>()
            .ActivateOnEnter<StoneFlail>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<FlailSmash>()
            .ActivateOnEnter<HeadToss>()
            .ActivateOnEnter<Earthshake>()
            .ActivateOnEnter<Rehydration>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8252)]
public class D041GreaterArmadillo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(278f, 204f), 19.5f, 40)], [new Rectangle(new(278f, 223.594f), 20f, 1f)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.PackArmadillo => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PackArmadillo));
    }
}
