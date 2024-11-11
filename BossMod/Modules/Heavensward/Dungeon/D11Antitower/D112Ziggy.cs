namespace BossMod.Heavensward.Dungeon.D11Antitower.D112Ziggy;

public enum OID : uint
{
    Boss = 0x3D82, // R2.700, x1
    Stardust = 0x3D83 // R2.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 31836, // Boss->location, no cast, single-target

    GyratingGlare = 31835, // Boss->self, 5.0s cast, range 40 circle
    ShinySummoning = 31831, // Boss->self, no cast, single-target
    MysticLight = 31838, // Stardust->self, 6.0s cast, range 12 circle
    JitteringGlare = 31832, // Boss->self, 3.0s cast, range 40 30-degree cone
    JitteringJounceVisual = 31833, // Boss->self, 6.0s cast, single-target
    JitteringJounce = 31840, // Boss->players/Stardust, no cast, width 6 rect charge
    DeepFracture = 31839, // Stardust->self, 4.0s cast, range 11 circle
    JitteringJab = 31837 // Boss->player, 5.0s cast, single-target, takbuster
}

public enum TetherID : uint
{
    JitteringJounce = 2 // Boss->player/Stardust
}

class GyratingGlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GyratingGlare));
class MysticLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MysticLight), new AOEShapeCircle(12));
class DeepFracture(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeepFracture), new AOEShapeCircle(11));
class JitteringGlare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JitteringGlare), new AOEShapeCone(40, 15.Degrees()));
class JitteringJab(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.JitteringJab));

class JitteringJounceBait(BossModule module) : Components.BaitAwayChargeTether(module, 3, 0, ActionID.MakeSpell(AID.JitteringJounce), tetherIDBad: (uint)TetherID.JitteringJounce)
{
    public Actor? Target;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(source, tether);
        Target ??= WorldState.Actors.Find(tether.Target);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.JitteringJounce)
            Target = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

class JitteringJounceLOS(BossModule module) : Components.GenericAOEs(module)
{
    private readonly JitteringJounceBait _bait = module.FindComponent<JitteringJounceBait>()!;
    private static readonly Angle halfAngle = Angle.Asin(1.5f / 11);
    private const string hint = "Hide behind meteor!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_bait.Target == actor)
        {
            List<DonutSegmentHA> cones = [];
            foreach (var c in Module.Enemies(OID.Stardust).Where(x => !x.IsDead))
            {
                cones.Add(new(D112Ziggy.ArenaCenter, 11.1f, 20, Angle.FromDirection(Module.PrimaryActor.DirectionTo(c)), halfAngle));
            }
            yield return new(new AOEShapeCustom(cones, InvertForbiddenZone: true), Arena.Center, Color: Colors.SafeFromAOE);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_bait.Target == actor)
        {
            if (!ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position)))
                hints.Add(hint);
            else
                hints.Add(hint, false);
        }
    }
}

class D112ZiggyStates : StateMachineBuilder
{
    public D112ZiggyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GyratingGlare>()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<DeepFracture>()
            .ActivateOnEnter<JitteringGlare>()
            .ActivateOnEnter<JitteringJab>()
            .ActivateOnEnter<JitteringJounceBait>()
            .ActivateOnEnter<JitteringJounceLOS>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4808)]
public class D112Ziggy(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(185.8f, 137.5f);
    private static readonly ArenaBoundsComplex arena = new([new Polygon(ArenaCenter, 20.06f / MathF.Cos(MathF.PI / 36), 36)], [new Rectangle(new(166, 138), 20, 1.1f, 90.Degrees()),
    new Rectangle(new(207, 137), 20, 2.6f, 90.Degrees())]);
}
