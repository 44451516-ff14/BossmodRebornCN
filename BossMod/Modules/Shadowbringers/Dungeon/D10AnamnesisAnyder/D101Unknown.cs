namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D101Unknown;

public enum OID : uint
{
    Unknown1 = 0x2CD9, // R4.9
    Unknown2 = 0x2CDA, // R4.9
    SinisterBubble = 0x2CDB, // R1.5
    Clock = 0x1EAF6C, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/Unknown->player, no cast, single-target

    FetidFang1 = 19305, // Boss->player, 4.0s cast, single-target, tankbuster
    FetidFang2 = 19314, // Unknown->player, 4.0s cast, single-target
    NursedGrudge = 19309, // Boss->self, no cast, single-target
    Scrutiny = 20005, // Boss/Unknown->self, 13.0s cast, single-target
    Reflection = 19311, // Boss/Unknown->self, 1.2s cast, range 40 45-degree cone
    Explosion = 19310, // SinisterBubble->self, 14.0s cast, range 8 circle

    LuminousRay1 = 20006, // Boss->self, 5.0s cast, range 50 width 8 rect
    LuminousRay2 = 20007, // Unknown->self, 5.0s cast, range 50 width 8 rect

    Inscrutability1 = 19306, // Boss->self, 4.0s cast, range 40 circle
    Inscrutability2 = 19315, // Unknown->self, 4.0s cast, range 40 circle
    PlainWeirdness = 20043, // Boss/Unknown->self, 3.0s cast, single-target, damage up buff when partner boss dies

    EctoplasmicRayMarker1 = 19319, // Helper->player, no cast, single-target, line stack
    EctoplasmicRayVisual1 = 19322, // Unknown->self, 5.0s cast, single-target
    EctoplasmicRay1 = 19320, // Unknown->self, no cast, range 50 width 8 rect
    EctoplasmicRayMarker2 = 19312, // Helper->player, no cast, single-target
    EctoplasmicRayVisual2 = 19321, // Boss->self, 5.0s cast, single-target
    EctoplasmicRay2 = 19313, // Boss->self, no cast, range 50 width 8 rect

    Clearout1 = 19316, // Unknown->self, 3.0s cast, range 9 120-degree cone
    Clearout2 = 19307, // Boss->self, 3.0s cast, range 9 120-degree cone

    Setback1 = 19308, // Boss->self, 3.0s cast, range 9 120-degree cone
    Setback2 = 19317 // Unknown->self, 3.0s cast, range 9 120-degree cone
}

sealed class Inscrutability(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Inscrutability1, (uint)AID.Inscrutability2]);
sealed class FetidFang(BossModule module) : Components.SingleTargetDelayableCasts(module, [(uint)AID.FetidFang1, (uint)AID.FetidFang2]);

sealed class SetbackClearout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Setback1, (uint)AID.Setback2, (uint)AID.Clearout1, (uint)AID.Clearout2],
new AOEShapeCone(9f, 60f.Degrees()));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 8f);

sealed class LuminousRay(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LuminousRay1, (uint)AID.LuminousRay2], new AOEShapeRect(50f, 4f));

sealed class Reflection(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 22.5f.Degrees());
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var angle = state switch
        {
            0x00041000u => Angle.AnglesIntercardinals[3],
            0x00042000u => Angle.AnglesIntercardinals[2],
            0x00044000u => Angle.AnglesCardinals[1],
            _ => default
        };
        if (angle != default)
        {
            _aoe = [new(cone, actor.Position.Quantized(), angle, WorldState.FutureTime(14.4d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Reflection)
        {
            _aoe = [];
        }
    }
}

abstract class EctoplasmicRay(BossModule module, uint aid1, uint aid2) : Components.LineStack(module, aidMarker: aid1, aid2, 5.2d, markerIsFinalTarget: false);
sealed class EctoplasmicRay1(BossModule module) : EctoplasmicRay(module, (uint)AID.EctoplasmicRayMarker1, (uint)AID.EctoplasmicRay1);
sealed class EctoplasmicRay2(BossModule module) : EctoplasmicRay(module, (uint)AID.EctoplasmicRayMarker2, (uint)AID.EctoplasmicRay2);

sealed class D101UnknownStates : StateMachineBuilder
{
    public D101UnknownStates(D101Unknown module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Inscrutability>()
            .ActivateOnEnter<FetidFang>()
            .ActivateOnEnter<SetbackClearout>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<LuminousRay>()
            .ActivateOnEnter<Reflection>()
            .ActivateOnEnter<EctoplasmicRay1>()
            .ActivateOnEnter<EctoplasmicRay2>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.Unknown2?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Unknown1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714u, NameID = 9260u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Shadowbringers, SortOrder = 1)]
public sealed class D101Unknown(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public Actor? Unknown2;

    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-40f, 290f), 19.5f, 40)], [new Rectangle(new(-40f, 310.534f), 20f, 1.2f), new Rectangle(new(-40f, 269.5f), 20f, 2f)]);

    protected override void UpdateModule()
    {
        Unknown2 ??= GetActor((uint)OID.Unknown2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(Unknown2);
    }
}
