namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D053Inferno;

public enum OID : uint
{
    Boss = 0x1AAE, // R4.5
    TwelfthLegionDeathClaw = 0x1AB0, // R1.0
    TwelfthLegionPacker = 0x1AAF, // R2.16
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    KetuCut = 8326, // Boss->self, no cast, single-target
    RahuCut = 8327, // Boss->self, no cast, single-target

    KetuSlash1 = 7974, // Boss->player, 2.0s cast, single-target, tankbuster
    KetuSlash2 = 8331, // Boss->player, 2.0s cast, single-target
    KetuSlash3 = 8332, // Boss->player, 2.0s cast, single-target

    RahuBlaster1 = 7977, // Boss->location, 3.0s cast, range 40+R width 6 rect
    RahuBlaster2 = 8334, // Boss->location, 2.0s cast, range 40+R width 6 rect
    RahuBlaster3 = 8335, // Boss->location, 2.0s cast, range 40+R width 6 rect

    KetuRahu = 7973, // Boss->self, 4.0s cast, single-target
    KetuCutter = 7975, // Helper->self, 4.0s cast, range 20+R 20-degree cone
    RahuRay = 7978, // Helper->player, no cast, range 10 circle

    KetuWave = 7976, // Helper->location, 4.0s cast, range 10 circle
    RahuComet1 = 7979, // Helper->location, 3.5s cast, range 40 circle, damage fall off AOE
    RahuComet2 = 8328, // Helper->location, 3.5s cast, range 40 circle, knockback 5 away from source
    RahuComet3 = 8329, // Helper->location, 3.5s cast, range 40 circle, knockback 10 away from source

    QuickCharge = 8487, // TwelfthLegionPacker->self, 15.0s cast, single-target
    DeathGrip = 8486 // TwelfthLegionDeathClaw->player, no cast, single-target, stuns player until is claw destroyed
}

public enum TetherID : uint
{
    Claw = 1 // TwelfthLegionDeathClaw->player
}

public enum IconID : uint
{
    Spreadmarker = 74, // player
}

sealed class ClawTether(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.Claw, 10f, needToKite: true);
sealed class RahuRay(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.RahuRay, 10f, 4.1f);
sealed class KetuSlash(BossModule module) : Components.SingleTargetCasts(module, [(uint)AID.KetuSlash1, (uint)AID.KetuSlash2, (uint)AID.KetuSlash3]);
sealed class KetuCutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KetuCutter, new AOEShapeCone(20.5f, 10f.Degrees()));
sealed class KetuWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KetuWave, 10f);

sealed class RahuBlaster(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RahuBlaster1, (uint)AID.RahuBlaster2, (uint)AID.RahuBlaster3], new AOEShapeRect(44.5f, 3f));

sealed class RahuComet(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RahuComet1, (uint)AID.RahuComet2, (uint)AID.RahuComet3], 15f);

abstract class RahuCometKB(BossModule module, uint aid, float distance) : Components.SimpleKnockbacks(module, aid, distance, stopAtWall: true)
{
    private readonly KetuWave _aoe1 = module.FindComponent<KetuWave>()!;
    private readonly KetuCutter _aoe2 = module.FindComponent<KetuCutter>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && _aoe1.Casters.Count == 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var center = Arena.Center;
            hints.AddForbiddenZone(new SDInvertedCone(center, 20f, Angle.FromDirection(center - c.Origin), 20f.Degrees()), c.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count1 = _aoe1.Casters.Count;
        var aoes1 = CollectionsMarshal.AsSpan(_aoe1.Casters);
        for (var i = 0; i < count1; ++i)
        {
            if (aoes1[i].Check(pos))
            {
                return true;
            }
        }
        var count2 = _aoe2.Casters.Count;
        var aoes2 = CollectionsMarshal.AsSpan(_aoe2.Casters);
        for (var i = 0; i < count2; ++i)
        {
            if (aoes2[i].Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}

sealed class RahuCometKB1(BossModule module) : RahuCometKB(module, (uint)AID.RahuComet2, 5f);
sealed class RahuCometKB2(BossModule module) : RahuCometKB(module, (uint)AID.RahuComet3, 10f);

sealed class D053InfernoStates : StateMachineBuilder
{
    public D053InfernoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KetuWave>()
            .ActivateOnEnter<ClawTether>()
            .ActivateOnEnter<RahuComet>()
            .ActivateOnEnter<RahuBlaster>()
            .ActivateOnEnter<KetuSlash>()
            .ActivateOnEnter<KetuCutter>()
            .ActivateOnEnter<RahuRay>()
            .ActivateOnEnter<RahuCometKB1>()
            .ActivateOnEnter<RahuCometKB2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242u, NameID = 6268u)]
public sealed class D053Inferno : BossModule
{
    public D053Inferno(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D053Inferno(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Circle(new(282.5f, -27.25f), 19.51f)], [new Rectangle(new(277.157f, -7.933f), 20, 1.25f, -17.532f.Degrees())]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.TwelfthLegionPacker));
        Arena.Actors(Enemies((uint)OID.TwelfthLegionDeathClaw));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.TwelfthLegionDeathClaw => 2,
                (uint)OID.TwelfthLegionPacker => 1,
                _ => 0
            };
        }
    }
}
