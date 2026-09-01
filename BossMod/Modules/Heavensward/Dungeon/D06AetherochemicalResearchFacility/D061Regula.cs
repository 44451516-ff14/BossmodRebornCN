namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D061Regula;

public enum OID : uint
{
    Boss = 0xE97, // R1.65
    ClockworkHunter = 0xF5C, // R1.25
    MagitekTurretI = 0xE98, // R0.6
    MagitekTurretII = 0xE99, // R0.6
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherochemicalGrenado = 4322, // MagitekTurretII->location, 3.0s cast, range 8 circle
    AetherochemicalLaser = 4321, // MagitekTurretI->player, 3.0s cast, range 50 width 4 rect
    Bastardbluss = 4314, // Boss->player, no cast, single-target, tankbuster + stun

    Judgment = 4317, // Boss->player, no cast, single-target
    JudgmentAOE = 4318, // Helper->self, 3.0s cast, range 8 circle

    SelfDetonate = 4323, // MagitekTurretI/MagitekTurretII->self, 5.0s cast, range 40+R circle

    MagitekSlug = 4315, // Boss->self, 2.5s cast, range 60+R width 4 rect
    MagitekSpread = 4316, // Boss->self, 4.5s cast, range 30+R 240-degree cone, knockback 20, away from source
    MagitekTurret = 4320, // Boss->self, no cast, single-target
    Quickstep = 4319 // Boss->location, no cast, single-target
}

public enum TetherID : uint
{
    BaitAway = 17 // MagitekTurretI->player
}

sealed class SelfDetonate(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDetonate);
sealed class AetherochemicalGrenado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalGrenado, 8f);
sealed class AetherochemicalLaser(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50f, 2f), (uint)TetherID.BaitAway, (uint)AID.AetherochemicalLaser);
sealed class AetherochemicalLaserAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherochemicalLaser, new AOEShapeRect(50f, 2f));

sealed class JudgmentAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JudgmentAOE, 8f);
sealed class MagitekSlug(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSlug, new AOEShapeRect(61.65f, 2f));
sealed class MagitekSpread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSpread, new AOEShapeCone(31.65f, 120f.Degrees()));

sealed class D061RegulaStates : StateMachineBuilder
{
    public D061RegulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<AetherochemicalLaser>()
            .ActivateOnEnter<AetherochemicalLaserAOE>()
            .ActivateOnEnter<JudgmentAOE>()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38u, NameID = 3818u, SortOrder = 4)]
public sealed class D061Regula : BossModule
{
    public D061Regula(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D061Regula(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        PolygonCustom[] poly = [new PolygonCustom([new(-89.11f, -301.95f), new(-93.32f, -310.37f), new(-106.99f, -318.19f), new(-115.01f, -318.19f),
            new(-128.64f, -310.32f), new(-132.28f, -303.07f), new(-132.29f, -287.96f), new(-128.64f, -280.69f), new(-115.04f, -272.8f), new(-107.04f, -272.8f),
            new(-93.35f, -280.7f), new(-89.11f, -289.1f)])];
        var arena = new ArenaBoundsCustom(poly);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MagitekTurretI));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MagitekTurretI => 1,
                _ => 0
            };
        }
    }

    public override bool ShouldPrioritizeAllEnemies => true;
}
