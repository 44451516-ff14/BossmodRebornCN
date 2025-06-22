namespace BossMod.RealmReborn.Dungeon.D29TheKeeperoftheLake.D292MagitekGunship;

public enum OID : uint
{
    Boss = 0xE68, // R6.0
    FireVoidzone = 0x1E98CA, // R0.5
    SixthCohortSecutor = 0xE71, // R0.5
    SixthCohortSignifer = 0xE72, // R0.5
    SixthCohortLaquearius = 0xE6F, // R0.5
    SixthCohortEques = 0xE70, // R0.5
    SixthCohortVanguard = 0xE69, // R2.8
    Helper = 0xE7B
}

public enum AID : uint
{
    AutoAttack = 870, // SixthCohortLaquearius/Boss/SixthCohortVanguard->player, no cast, single-target
    AutoAttack2 = 872, // SixthCohortSecutor->player, no cast, single-target
    AutoAttack3 = 871, // SixthCohortEques->player, no cast, single-target

    FlameThrowerFirst = 3389, // Boss->self, 3.5s cast, range 12+R 120-degree cone
    FlameThrowerRest = 3390, // Helper->self, no cast, range 17+R 120-degree cone
    GarleanFire = 3411, // Helper->location, no cast, range 8 circle
    Thunder = 968, // SixthCohortSignifer->player, 1.0s cast, single-target
    CarpetBombVisual = 3391, // Boss->location, 1.0s cast, single-target
    CarpetBomb = 3392, // Helper->location, 3.0s cast, range 5 circle
    DrillCannons = 1433, // SixthCohortVanguard->self, 2.5s cast, range 30+R width 5 rect
    Blitz = 3393, // Boss->location, no cast, width 4 rect charge, knockback 10, dir forward
    Overcharge = 1435 // SixthCohortVanguard->self, 2.5s cast, range 8+R 120-degree cone
}

class GarleanFire(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.GarleanFire, GetVoidzones, 0.2f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class DrillCannons(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DrillCannons, new AOEShapeRect(32.8f, 2.5f));
class CarpetBomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CarpetBomb, 5f);
class Overcharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overcharge, new AOEShapeCone(10.8f, 60f.Degrees()));

class Flamethrower(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(18f, 60f.Degrees());
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameThrowerFirst)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlameThrowerFirst:
            case (uint)AID.FlameThrowerRest:
                if (++NumCasts == 6)
                {
                    _aoe = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class D292MagitekGunshipStates : StateMachineBuilder
{
    public D292MagitekGunshipStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CarpetBomb>()
            .ActivateOnEnter<GarleanFire>()
            .ActivateOnEnter<DrillCannons>()
            .ActivateOnEnter<Overcharge>()
            .ActivateOnEnter<Flamethrower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3373)]
public class D292MagitekGunship(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(16.14f, -167.84f), new(20.01f, -165.87f), new(20.41f, -165.51f), new(24.38f, -161.54f), new(24.66f, -161.04f),
    new(27.06f, -156.32f), new(27.23f, -155.85f), new(28.13f, -150.15f), new(28.10f, -149.58f), new(27.27f, -144.35f),
    new(27.15f, -143.83f), new(24.42f, -138.47f), new(20.15f, -134.19f), new(19.69f, -133.93f), new(15.16f, -131.62f),
    new(2.11f, -131.61f), new(1.64f, -131.79f), new(-2.70f, -134.00f), new(-3.12f, -134.31f), new(-6.95f, -138.14f),
    new(-7.35f, -138.62f), new(-9.99f, -143.81f), new(-10.12f, -144.36f), new(-10.94f, -149.51f), new(-11.00f, -150.06f),
    new(-10.07f, -155.93f), new(-9.85f, -156.45f), new(-7.45f, -161.16f), new(-7.17f, -161.58f), new(-3.28f, -165.47f),
    new(-2.84f, -165.86f), new(1.22f, -167.76f), new(15.79f, -167.88f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    private static readonly uint[] trash = [(uint)OID.SixthCohortEques, (uint)OID.SixthCohortLaquearius, (uint)OID.SixthCohortSecutor, (uint)OID.SixthCohortSignifer, (uint)OID.SixthCohortVanguard];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(trash));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }
}
