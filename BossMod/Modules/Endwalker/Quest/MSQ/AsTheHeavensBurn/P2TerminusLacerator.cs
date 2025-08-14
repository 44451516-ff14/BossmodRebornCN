using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.MSQ.AsTheHeavensBurn.P2TerminusLacerator;

public enum OID : uint
{
    Boss = 0x35EC, // R6.0
    Meteorite = 0x35ED, // R2.4
    MeteoriteHelper = 0x1EB291, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    TheBlackDeath = 27010, // Boss->self, no cast, range 25 120-degree cone

    BlackStarVisual = 27011, // Boss->self, 5.0s cast, single-target
    BlackStar = 27012, // Helper->location, 6.0s cast, range 40 circle
    DeadlyImpact = 27014, // Helper->location, 7.0s cast, range 10 circle
    Burst = 27021, // Helper->location, 7.5s cast, range 5 circle

    DeadlyImpactVisual1 = 27013, // Boss->self, 4.0s cast, single-target
    DeadlyImpactVisual2 = 27020, // Boss->self, 4.0s cast, single-target
    DeadlyImpactMeteoriteVisual = 27023, // Boss->self, 6.0s cast, single-target
    DeadlyImpact1 = 27025, // Meteorite->self, 5.0s cast, range 20 circle
    DeadlyImpact2 = 27024, // Helper->location, 6.0s cast, range 20 circle
    CosmicKiss = 27027, // Helper->location, no cast, range 40 circle

    Explosion = 27026 // Meteorite->self, 3.0s cast, range 6 circle
}

class TheBlackDeath(BossModule module) : Components.Cleave(module, (uint)AID.TheBlackDeath, new AOEShapeCone(25f, 60f.Degrees()), [(uint)OID.Boss], activeWhileCasting: false);
class Burst(BossModule module) : Components.CastTowers(module, (uint)AID.Burst, 5f);
class DeadlyImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyImpact, 10f, 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackStar);
class DeadlyImpact1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyImpact1, 8f);
class DeadlyImpact2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyImpact2, 10f);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 6f);
class Meteor(BossModule module) : Components.GenericLineOfSightAOE(module, default, 40f, safeInsideHitbox: false)
{
    private readonly List<(Actor, DateTime)> casters = new(4);
    private readonly List<Actor> meteors = new(4);

    private void Refresh()
    {
        if (meteors.Count != 0)
        {
            List<(WPos Position, float HitboxRadius)> activemeteors = new(4);
            for (var i = 0; i < meteors.Count; ++i)
            {
                var m = meteors[i];
                activemeteors.Add((m.Position, m.HitboxRadius));
            }

            Modify(casters[0].Item1.Position, activemeteors, casters[0].Item2);

            Safezones.Clear();
            AddSafezone(NextExplosion, default);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MeteoriteHelper)
        {
            casters.Add((actor, WorldState.FutureTime(11.7d)));
            Refresh();
        }
        else if (actor.OID == (uint)OID.Meteorite)
            meteors.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (casters.Count != 0 && spell.Action.ID == (uint)AID.CosmicKiss)
        {
            for (var i = 0; i < meteors.Count; ++i)
            {
                var meteor = meteors[i];
                if (meteor.Position.AlmostEqual(caster.Position, 10f))
                {
                    meteors.Remove(meteor);
                    break;
                }
            }
            casters.RemoveAt(0);
            Refresh();
            if (casters.Count == 0)
                Safezones.Clear();
        }
    }
}

class AutoAlisaie(BossModule module) : QuestBattle.RotationModule<AlisaieAI>(module);

class TerminusLaceratorStates : StateMachineBuilder
{
    public TerminusLaceratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheBlackDeath>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>()
            .ActivateOnEnter<DeadlyImpact1>()
            .ActivateOnEnter<DeadlyImpact2>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<AutoAlisaie>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10933)]
public class TerminusLacerator(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(new(-260.28f, 80.73f), 19.5f, 20)]);
}
