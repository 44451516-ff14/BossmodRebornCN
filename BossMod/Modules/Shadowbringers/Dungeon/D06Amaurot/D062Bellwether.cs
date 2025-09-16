namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D062Bellwether;

public enum OID : uint
{
    Boss = 0x27B8, // R=6.0-12.0
    TerminusFlesher = 0x27B9, // R=3.3
    TerminusCrier = 0x27BD, // R=1.0
    TerminusSprinter = 0x2879, // R=1.96
    TerminusDetonator = 0x27BA, // R=2.0
    TerminusShriver = 0x27BC, // R=1.35
    TerminusRoiler = 0x27BB, // R=1.2
    TerminusBeholder = 0x27BE, // R=1.32
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/TerminusSprinter/TerminusCrier->player, no cast, single-target
    AutoAttack2 = 6499, // TerminusFlesher/TerminusBeholder->player, no cast, single-target
    AutoAttack3 = 18013, // TerminusDetonator->player, no cast, single-target

    ShrillShriek = 15567, // Boss->self, 3.0s cast, range 50 circle
    Aetherspike = 15571, // TerminusSprinter->self, 4.0s cast, range 40 width 8 rect
    SelfDestruct = 15570, // TerminusDetonator->self, no cast, range 6 circle
    IllWill = 15573, // TerminusRoiler->player, no cast, single-target
    SicklyFlame = 15588, // TerminusShriver->player, 2.0s cast, single-target
    Comet = 15572, // 18D6->location, 4.0s cast, range 4 circle
    SicklyInferno = 16765, // TerminusShriver->location, 3.0s cast, range 5 circle
    Burst = 15569, // Boss->self, no cast, range 50 circle, raidwide on boss death
    BurstEnrage = 15568, // Boss->self, 45.0s cast, single-target, enrage cast
    ExplosionEnrage = 15919 // Boss->self, no cast, range 50 circle, enrage
}

sealed class ShrillShriek(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShrillShriek);
sealed class Aetherspike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aetherspike, new AOEShapeRect(40f, 4f));
sealed class Comet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Comet, 4f);
sealed class SicklyInferno(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SicklyInferno, 5f);
sealed class Burst(BossModule module) : Components.CastHint(module, (uint)AID.BurstEnrage, "Enrage!", true);

sealed class D062BellwetherStates : StateMachineBuilder
{
    public D062BellwetherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShrillShriek>()
            .ActivateOnEnter<Aetherspike>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<SicklyInferno>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8202)]
public sealed class D062Bellwether(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(60f, -361f), 19.5f, 40)], [new Rectangle(new(60.024f, -340.815f), 20f, 1.25f)]);
    private static readonly uint[] trash = [(uint)OID.TerminusRoiler, (uint)OID.TerminusShriver, (uint)OID.TerminusFlesher, (uint)OID.TerminusDetonator,
    (uint)OID.TerminusBeholder, (uint)OID.TerminusCrier, (uint)OID.TerminusSprinter];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, trash);
    }
}
