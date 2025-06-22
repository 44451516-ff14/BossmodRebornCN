namespace BossMod.Heavensward.Quest.WarringTriad.ABloodyReunion;

public enum OID : uint
{
    Boss = 0x161E,
    MagitekTurretI = 0x161F, // R0.6
    MagitekTurretII = 0x1620, // R0.6
    TerminusEst = 0x1621, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    MagitekSlug = 6026, // Boss->self, 2.5s cast, range 60+R width 4 rect
    AetherochemicalGrenado = 6031, // MagitekTurretII->location, 3.0s cast, range 8 circle
    SelfDetonate = 6032, // MagitekTurretI/MagitekTurretII->self, 5.0s cast, range 40+R circle
    MagitekSpread = 6027, // Boss->self, 3.0s cast, range 20+R 240-degree cone
}

class MagitekSlug(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSlug, new AOEShapeRect(60f, 2f));
class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalGrenado, 8f);
class SelfDetonate(BossModule module) : Components.CastHint(module, (uint)AID.SelfDetonate, "Kill turret before detonation!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            if (h.Actor.CastInfo?.Action.ID == WatchedAction)
                h.Priority = 5;
        }
    }
}
class MagitekSpread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSpread, new AOEShapeCone(20.55f, 120f.Degrees()));

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 173, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(230f, 79f), 20.256f, 24)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies([(uint)OID.MagitekTurretI, (uint)OID.MagitekTurretII]));
        Arena.Actor(PrimaryActor);
    }
}
