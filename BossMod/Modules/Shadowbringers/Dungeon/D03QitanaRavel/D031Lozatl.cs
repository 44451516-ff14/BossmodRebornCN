namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D031Lozatl;

public enum OID : uint
{
    Boss = 0x27AF, //R=4.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Stonefist = 15497, // Boss->player, 4.0s cast, single-target
    SunToss = 15498, // Boss->location, 3.0s cast, range 5 circle
    LozatlsScorn = 15499, // Boss->self, 3.0s cast, range 40 circle
    RonkanLightRight = 15500, // Helper->self, no cast, range 60 width 20 rect
    RonkanLightLeft = 15725, // Helper->self, no cast, range 60 width 20 rect
    HeatUp = 15502, // Boss->self, 3.0s cast, single-target
    HeatUp2 = 15501, // Boss->self, 3.0s cast, single-target
    LozatlsFury1 = 15504, // Boss->self, 4.0s cast, range 60 width 20 rect
    LozatlsFury2 = 15503 // Boss->self, 4.0s cast, range 60 width 20 rect
}

[SkipLocalsInit]
sealed class LozatlsFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LozatlsFury1, (uint)AID.LozatlsFury2], new AOEShapeRect(60f, 10f));
[SkipLocalsInit]
sealed class Stonefist(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Stonefist);
[SkipLocalsInit]
sealed class LozatlsScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.LozatlsScorn);
[SkipLocalsInit]
sealed class SunToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunToss, 5f);

[SkipLocalsInit]
sealed class RonkanLight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rect = new(60f, 20f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u)
        {
            var rot = actor.Position.X == 10f ? 90f.Degrees() : -90f.Degrees();
            var pos = D031Lozatl.ArenaCenter;
            _aoe = [new(rect, D031Lozatl.ArenaCenter, rot, WorldState.FutureTime(8d), shapeDistance: rect.Distance(pos, rot))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RonkanLightLeft or (uint)AID.RonkanLightRight)
        {
            _aoe = [];
        }
    }
}

[SkipLocalsInit]
sealed class D031LozatlStates : StateMachineBuilder
{
    public D031LozatlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LozatlsFury>()
            .ActivateOnEnter<Stonefist>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<RonkanLight>()
            .ActivateOnEnter<LozatlsScorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651u, NameID = 8231u)]
[SkipLocalsInit]
public sealed class D031Lozatl(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(default, 315f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi40th, 40)],
    [new Rectangle(new(default, 335.1f), 20f, 2f), new Rectangle(new(default, 294.5f), 20f, 2f)]);
}
