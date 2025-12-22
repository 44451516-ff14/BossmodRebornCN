namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D111AllSeeingEye;

public enum OID : uint
{
    AllSeeingEye = 0x4A8A, // R2.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // AllSeeingEye->player, no cast, single-target

    EyesOnMe = 45569, // AllSeeingEye->self, 5.0s cast, range 33 circle
    BlusteringBlink = 45570, // AllSeeingEye->self, 3.0s cast, range 55 width 10 rect
    VoidMatterVisual = 45567, // AllSeeingEye->self, 7.5+0,5s cast, single-target
    VoidMatter = 45568 // Helper->location, 8.0s cast, range 10 circle
}

[SkipLocalsInit]
sealed class BlusteringBlink(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlusteringBlink, new AOEShapeRect(55f, 5f));

[SkipLocalsInit]
sealed class VoidMatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidMatter, 10f, 6);

[SkipLocalsInit]
sealed class EyesOnMe(BossModule module) : Components.RaidwideCast(module, (uint)AID.EyesOnMe);

[SkipLocalsInit]
sealed class D111AllSeeingEyeStates : StateMachineBuilder
{
    public D111AllSeeingEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlusteringBlink>()
            .ActivateOnEnter<VoidMatter>()
            .ActivateOnEnter<EyesOnMe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(D111AllSeeingEyeStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.AllSeeingEye,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.RealmReborn,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 13u,
NameID = 1397u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D111AllSeeingEye : BossModule
{
    public D111AllSeeingEye(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D111AllSeeingEye(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(48f, 78f), 19.5f, 64)], [new Rectangle(new(62.43887f, 92.40965f), 8f, 1.25f, 45.855f.Degrees())]);
        return (arena.Center, arena);
    }
}
