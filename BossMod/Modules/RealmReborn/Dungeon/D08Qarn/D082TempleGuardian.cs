namespace BossMod.RealmReborn.Dungeon.D08Qarn.D082TempleGuardian;

public enum OID : uint
{
    Boss = 0x477C, // R2.2
    GolemSoulstone = 0x477D // R2.2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    BoulderClap = 42234, // Boss->self, 2.5s cast, range 12+R 120-degree cone
    TrueGrit = 42235, // Boss->self, 3.0s cast, range 12+R 120-degree cone
    Rockslide = 42236, // Boss->self, 2.5s cast, range 14+R width 8 rect
    StoneSkull = 42237, // Boss->player, no cast, single-target
    Obliterate = 42238 // Boss->self, 2.0s cast, range 60 circle
}

sealed class BoulderClapTrueGrit(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BoulderClap, (uint)AID.TrueGrit], new AOEShapeCone(14.2f, 60.Degrees()));
sealed class Rockslide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rockslide, new AOEShapeRect(16.2f, 4f));
sealed class Obliterate(BossModule module) : Components.RaidwideCast(module, (uint)AID.Obliterate);

sealed class D082TempleGuardianStates : StateMachineBuilder
{
    public D082TempleGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoulderClapTrueGrit>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Obliterate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1569)]
public sealed class D082TempleGuardian(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsComplex arena = new([new PolygonCustom([new(66.5f, -33.7f), new(58.6f, -25), new(51.4f, -22.5f),
    new(39.3f, -16.5f), new(36.6f, -5), new(39.3f, 5.7f), new(41.1f, 16),
    new(56.5f, 14.8f), new(63.6f, 7.1f), new(64.7f, 3.3f), new(70.3f, -3.9f), new(72.6f, -33.3f)])]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GolemSoulstone => 1,
                _ => 0
            };
        }
    }
}
