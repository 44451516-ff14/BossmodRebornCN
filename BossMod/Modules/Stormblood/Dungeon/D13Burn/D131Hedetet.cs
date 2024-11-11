namespace BossMod.Stormblood.Dungeon.D13TheBurn.D131Hedetet;

public enum OID : uint
{
    Boss = 0x2419, // R4.2
    DimCrystal = 0x241A, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport1 = 33212, // Boss->location, no cast, single-target
    Teleport2 = 12694, // Boss->location, no cast, single-target

    CrystalNeedle = 12691, // Boss->player, 3.0s cast, single-target
    Hailfire = 12692, // Boss->self/players, 6.0s cast, range 40+R width 4 rect
    ShardstrikeVisual = 12693, // Boss->self, 5.0s cast, single-target
    Shardstrike = 12697, // Helper->players, no cast, range 5 circle
    Shardfall = 12689, // Boss->self, 5.0s cast, range 40 circle
    ResonantFrequency = 12696, // DimCrystal->self, 3.0s cast, range 6 circle
    Dissonance = 12690, // Boss->self, 5.0s cast, range 5-40 donut
    CrystallineFracture = 12695 // DimCrystal->self, 3.0s cast, range 3 circle
}

public enum IconID : uint
{
    Tankbuster = 381, // player
    Prey = 2, // player
    Spreadmarker = 96 // player
}

class Shardstrike(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.Shardstrike), 5, 5.8f);
class ShardstrikeCrystals(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Shardstrike _st = module.FindComponent<Shardstrike>()!;
    private static readonly AOEShapeCircle circle = new(6.6f); // for non players hitbox must not be clipped

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_st.ActiveSpreadTargets.Contains(actor))
            foreach (var c in Module.Enemies(OID.DimCrystal).Where(x => !x.IsDead))
                yield return new(circle, c.Position, Color: Colors.FutureVulnerable);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any())
            hints.Add("Avoid clipping crystals!");
    }
}

class Hailfire(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _target;
    private const string Hint = "Hide behind crystal!";
    private const float Length = 44.2f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_target != null)
        {
            var boss = Module.PrimaryActor;
            List<RectangleSE> rects = [];
            foreach (var c in Module.Enemies(OID.DimCrystal).Where(x => !x.IsDead))
            {
                var dir = boss.DirectionTo(c);
                rects.Add(new(c.Position + 0.1f * dir, c.Position + Length * dir, 1.6f));
            }
            yield return _target == actor
                ? new(new AOEShapeCustom(rects, InvertForbiddenZone: true), Arena.Center, Color: Colors.SafeFromAOE)
                : new(new AOEShapeCustom([new RectangleSE(boss.Position, boss.Position + Length * boss.DirectionTo(_target), 2)], rects), Arena.Center);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hailfire)
            _target = WorldState.Actors.Find(spell.TargetID);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hailfire)
            _target = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == actor)
        {
            if (!ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position)))
                hints.Add(Hint);
            else
                hints.Add(Hint, false);
        }
        else
            base.AddHints(slot, actor, hints);
    }
}

class CrystalNeedle(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CrystalNeedle));
class Shardfall(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.Shardfall), 40)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.DimCrystal).Where(e => !e.IsDead);
}
class CrystallineFracture(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrystallineFracture), new AOEShapeCircle(3));
class ResonantFrequency(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ResonantFrequency), new AOEShapeCircle(6));
class Dissonance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Dissonance), new AOEShapeDonut(5, 40));

class D131HedetetStates : StateMachineBuilder
{
    public D131HedetetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shardstrike>()
            .ActivateOnEnter<ShardstrikeCrystals>()
            .ActivateOnEnter<Hailfire>()
            .ActivateOnEnter<CrystalNeedle>()
            .ActivateOnEnter<Shardfall>()
            .ActivateOnEnter<CrystallineFracture>()
            .ActivateOnEnter<ResonantFrequency>()
            .ActivateOnEnter<Dissonance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7667)]
public class D131Hedetet(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(174, 178), 19.5f)], [new Rectangle(new(174, 197.6f), 20, 1), new Rectangle(new(174, 158.3f), 20, 1)]);
}
