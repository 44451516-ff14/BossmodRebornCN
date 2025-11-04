namespace BossMod.Stormblood.Quest.MSQ.BestServedWithColdSteel;

public enum OID : uint
{
    Boss = 0x1A52, // R2.1
    Grynewaht = 0x1A53,
    Helper = 0x233C
}

public enum AID : uint
{
    CermetPile = 8117, // 1A52->self, 3.0fs cast, range 4$1fR width 6 rect
    Firebomb = 8495, // Boss->location, 3.0fs cast, range 4 circle
    OpenFire1 = 8121, // 19D9->location, 3.0fs cast, range 6 circle
    AugmentedSuffering = 8492, // Boss->self, 3.5fs cast, range $1fR circle
    AugmentedUprising = 8493, // Boss->self, 3.0fs cast, range $1fR 120-degree cone
    SelfDetonate = 8122, // 1A56->self, no cast, range 6 circle
    SelfDetonate1 = 9169 // Boss->self, 60.0s cast, range 100 circle
}

public enum TetherID : uint
{
    Mine = 54 // 1A56->player
}

class AugmentedUprising(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedUprising, new AOEShapeCone(8.5f, 60.Degrees()));
class AugmentedSuffering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedSuffering, 6.5f);
class OpenFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OpenFire1, 6);

class CermetPile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CermetPile, new AOEShapeRect(42.1f, 3f));
class Firebomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebomb, 4);

class MagitekTurret(BossModule module) : Components.GenericAOEs(module, (uint)AID.SelfDetonate)
{
    private static readonly AOEShapeCircle circle = new(6f);

    class Mine(Actor source, Actor target, WPos sourcePosLastFrame, DateTime tethered)
    {
        public Actor source = source;
        public Actor target = target;
        public WPos sourcePosLastFrame = sourcePosLastFrame;
        public DateTime tethered = tethered;

        public float DistanceLeft(WorldState ws)
        {
            var elapsed = (float)(ws.CurrentTime - tethered).TotalSeconds;
            // approximation, turret starts moving after about 3.7s on average, but 4 is a nice round number
            return Math.Clamp(12 - elapsed, 0, 8) * 3;
        }
    }

    private readonly List<Mine> Mines = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Mines.Count;
        if (count == 0)
            return [];
        var aoes = new List<AOEInstance>();
        for (var i = 0; i < count; ++i)
        {
            var m = Mines[i];
            if (m.target == actor)
            {
                var mineToPlayer = m.target.Position - m.source.Position;
                var distance = m.DistanceLeft(WorldState);
                var projectedExplosion = mineToPlayer.LengthSq() > distance * distance
                    ? (m.target.Position - m.source.Position).Normalized() * distance
                    // offset danger zone slightly toward mine so that AI can dodge
                    // if centered on player it doesn't know which direction to go
                    : mineToPlayer * 0.9f;
                aoes.Add(new(circle, m.source.Position + projectedExplosion, default, activation: m.tethered.AddSeconds(12d)));
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Mine && WorldState.Actors.Find(tether.Target) is Actor target)
            Mines.Add(new(source, target, source.Position, WorldState.CurrentTime));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SelfDetonate)
        {
            var count = Mines.Count;
            for (var i = 0; i < count; ++i)
            {
                var m = Mines[i];
                if (m.source == caster)
                {
                    Mines.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        var count = Mines.Count;
        for (var i = 0; i < count; ++i)
        {
            var m = Mines[i];
            if (m.source == actor)
            {
                Mines.RemoveAt(i);
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Mines.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var m = Mines[i];
            if (m.target == pc)
            {
                Arena.AddLine(m.source.Position, pc.Position);
                return;
            }
        }
    }
}

class MagitekSelfDetonate(BossModule module) : Components.CastCounter(module, (uint)AID.SelfDetonate1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            NumCasts++;
    }
}

class MagitekVanguardIPrototypeStates : StateMachineBuilder
{
    private readonly MagitekVanguardIPrototype _module;

    private float BossHPRatio => (float)_module.PrimaryActor.HPMP.CurHP / _module.PrimaryActor.HPMP.MaxHP;

    public MagitekVanguardIPrototypeStates(MagitekVanguardIPrototype module) : base(module)
    {
        _module = module;
        DeathPhase(0, P1);
    }

    private void P1(uint id)
    {
        Condition(id, 300, () => BossHPRatio < 0.9f, "Adds 1")
            .ActivateOnEnter<CermetPile>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<OpenFire>()
            .ActivateOnEnter<AugmentedUprising>();

        Condition(id + 2, 300, () => BossHPRatio < 0.75f, "Adds 2");
        Condition(id + 4, 300, () => BossHPRatio < 0.65f, "Turrets").ActivateOnEnter<MagitekTurret>();
        Condition(id + 6, 300, () => BossHPRatio < 0.55f, "Adds 3");
        Condition(id + 8, 300, () => BossHPRatio < 0.4f, "Cutscene").ActivateOnEnter<MagitekSelfDetonate>();
        ComponentCondition<MagitekSelfDetonate>(id + 10, 18, m => m.NumCasts > 0);
        CastEnd(id + 12, 60, "Self-detonate").SetHint(StateMachine.StateHint.DowntimeStart);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67989, NameID = 5650)]
public class MagitekVanguardIPrototype(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-487.40f, -230.79f), new(-487.56f, -188.08f), new(-478.75f, -181.25f), new(-439.37f, -183.46f), new(-457.85f, -211.90f), new(-461.13f, -228.75f)];
    public static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            if (h.Actor.OID == 0x1A52)
                h.Priority = 1;
            else if (h.Actor.TargetID == actor.InstanceID)
                h.Priority = 2;
            else
                h.Priority = 0;
        }
    }
}
