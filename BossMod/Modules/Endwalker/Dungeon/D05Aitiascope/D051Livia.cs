namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D051Livia;

public enum OID : uint
{
    Boss = 0x3469, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24771, // Boss->player, no cast, single-target

    AglaeaBite = 25673, // Boss->self/player, 5.0s cast, range 9 90-degree cone, tankbuster 

    AglaeaClimb1 = 25666, // Boss->self, 7.0s cast, single-target
    AglaeaClimb2 = 25667, // Boss->self, 7.0s cast, single-target
    AglaeaClimbAOE = 25668, // Helper->self, 7.0s cast, range 20 90-degree cone

    AglaeaShotVisual = 25669, // Boss->self, 3.0s cast, single-target
    AglaeaShot1 = 25670, // 346A->location, 3.0s cast, range 20 width 4 rect
    AglaeaShot2 = 25671, // 346A->location, 1.0s cast, range 40 width 4 rect

    Disparagement = 25674, // Boss->self, 5.0s cast, range 40 120-degree cone

    Frustration = 25672, // Boss->self, 5.0s cast, range 40 circle, raidwide

    IgnisAmoris = 25676, // Helper->location, 4.0s cast, range 6 circle
    IgnisOdi = 25677, // Helper->players, 5.0s cast, range 6 circle

    OdiEtAmo = 25675 // Boss->self, 3.0s cast, single-target
}

class AglaeaBite(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.AglaeaBite), new AOEShapeCone(9, 60.Degrees()), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class AglaeaShot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 3);
    private readonly List<Actor> casters = [];
    private DateTime activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            foreach (var a in _aoes)
                yield return new(rect, a.Origin, a.Rotation, a.Activation);
        else if ((activation - WorldState.CurrentTime).TotalSeconds < 5)
            foreach (var c in casters)
                yield return new(rect, c.Position, c.Rotation, activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AglaeaShot1)
            _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AglaeaShot1)
        {
            activation = WorldState.FutureTime(10);
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
                casters.Add(caster);
            }
        }
        else if ((AID)spell.Action.ID == AID.AglaeaShot2)
            casters.Remove(caster);
    }
}

class AglaeaClimbAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AglaeaClimbAOE), new AOEShapeCone(20, 45.Degrees()));
class Disparagement(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Disparagement), new AOEShapeCone(40, 60.Degrees()));

class IgnisOdi(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.IgnisOdi), 6, 4, 4);
class IgnisAmoris(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.IgnisAmoris), 6);
class Frustration(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Frustration));

class D051LiviaStates : StateMachineBuilder
{
    public D051LiviaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AglaeaShot>()
            .ActivateOnEnter<AglaeaClimbAOE>()
            .ActivateOnEnter<Disparagement>()
            .ActivateOnEnter<IgnisOdi>()
            .ActivateOnEnter<IgnisAmoris>()
            .ActivateOnEnter<Frustration>()
            .ActivateOnEnter<AglaeaBite>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10290)]
public class D051Livia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-6, 471), 19.5f / MathF.Cos(MathF.PI / 36), 36)], [new Rectangle(new(-6, 491.8f), 20, 2)]);
}
