namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD70ForgivenZeal;

public enum OID : uint
{
    ForgivenZeal = 0x484C, // R7.0
    HolySphere = 0x484D, // R0.7
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // ForgivenZeal->player, no cast, single-target
    Teleport = 43416, // ForgivenZeal->location, no cast, single-target

    ZealousGlowerVisual1 = 43411, // ForgivenZeal->self, 3.0s cast, single-target, far to near
    ZealousGlower1 = 43412, // Helper->self, 4.0s cast, range 5 width 10 rect
    ZealousGlower2 = 43413, // Helper->self, 4.5s cast, range 5 width 10 rect
    ZealousGlower3 = 43414, // Helper->self, 5.0s cast, range 5 width 10 rect
    ZealousGlower4 = 43415, // Helper->self, 5.5s cast, range 5 width 10 rect
    ZealousGlowerVisual2 = 43406, // ForgivenZeal->self, 3.0s cast, single-target, near to far
    ZealousGlower5 = 43407, // Helper->self, 4.0s cast, range 5 width 10 rect
    ZealousGlower6 = 43408, // Helper->self, 4.5s cast, range 5 width 10 rect
    ZealousGlower7 = 43409, // Helper->self, 5.0s cast, range 5 width 10 rect
    ZealousGlower8 = 43410, // Helper->self, 5.5s cast, range 5 width 10 rect

    BrutalHalo = 43417, // HolySphere->self, 2.0s cast, range 3-20 donut
    ArdorousEyeVisual1 = 43418, // ForgivenZeal->self, 6.0s cast, single-target, cw
    ArdorousEye1 = 43419, // Helper->self, 7.5s cast, range 5-10 90-degree donut sector
    ArdorousEye2 = 43420, // Helper->self, 7.8s cast, range 5-10 90-degree donut sector
    ArdorousEye3 = 43421, // Helper->self, 8.1s cast, range 5-10 90-degree donut sector
    ArdorousEye4 = 43422, // Helper->self, 8.4s cast, range 5-10 90-degree donut sector
    ArdorousEyeVisual2 = 43423, // ForgivenZeal->self, 6.0s cast, single-target, ccw
    ArdorousEye5 = 43424, // Helper->self, 7.5s cast, range 5-10 90-degree donut sector
    ArdorousEye6 = 43425, // Helper->self, 7.8s cast, range 5-10 90-degree donut sector
    ArdorousEye7 = 43426, // Helper->self, 8.1s cast, range 5-10 90-degree donut sector
    ArdorousEye8 = 43427, // Helper->self, 8.4s cast, range 5-10 90-degree donut sector

    TwothousandMinaSwingVisual = 43428, // ForgivenZeal->self, 6.0s cast, single-target
    TwothousandMinaSwing = 43429, // Helper->self, 7.0+1,0s cast, range 8 circle

    DisorientingGroanVisual = 43430, // ForgivenZeal->self, 6.0s cast, single-target
    DisorientingGroan = 43431, // Helper->self, 7.0+1,0s cast, range 30 circle, knockback 7, away from source

    OctupleSwipeTelegraph = 43437, // Helper->self, 1.0s cast, range 40 90-degree cone
    OctupleSwipeVisual = 43432, // ForgivenZeal->self, 10.0s cast, single-target
    OctupleSwipe1 = 43434, // ForgivenZeal->self, no cast, range 40 90-degree cone
    OctupleSwipe2 = 43436, // ForgivenZeal->self, no cast, range 40 90-degree cone
    OctupleSwipe3 = 43435, // ForgivenZeal->self, no cast, range 40 90-degree cone
    OctupleSwipe4 = 43433 // ForgivenZeal->self, no cast, range 40 90-degree cone
}

[SkipLocalsInit]
sealed class BrutalHalo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(8);
    private readonly AOEShapeDonut donut = new(3f, 20f);
    private DateTime activation;
    private ZealousGlower? _aoe1;
    private ArdorousEye? _aoe2;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.HolySphere)
        {
            var pos = actor.Position.Quantized();
            var count = AOEs.Count;
            AOEs.Add(new(donut, pos, actorID: actor.InstanceID, activation: count == 0 ? activation : AOEs.Ref(count - 1).Activation.AddSeconds(2.5d), shapeDistance: donut.Distance(pos, default)));

            // actors spawn about 0.4s apart. Sort by instance ID to ensure correct order if player is lagging
            SortHelpers.SortAOEsByActorID(AOEs);
        }
    }

    public override void Update()
    {
        if (AOEs.Count != 0)
        {
            _aoe1 ??= Module.FindComponent<ZealousGlower>()!;
            _aoe2 ??= Module.FindComponent<ArdorousEye>()!;
            AOEs.Ref(0).Color = _aoe1.Casters.Count != 0 || _aoe2.Casters.Count != 0 ? Colors.Danger : default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ArdorousEyeVisual1:
            case (uint)AID.ArdorousEyeVisual2:
                activation = Module.CastFinishAt(spell, 6.7d);
                break;
            case (uint)AID.ZealousGlowerVisual1:
            case (uint)AID.ZealousGlowerVisual2:
                activation = Module.CastFinishAt(spell, 8.4d);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID == (uint)AID.BrutalHalo)
        {
            AOEs.RemoveAt(0);
        }
    }
}

[SkipLocalsInit]
sealed class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly AOEShapeCone cone = new(40f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OctupleSwipeTelegraph)
        {
            var rot = spell.Rotation;
            var loc = spell.LocXZ;
            var count = _aoes.Count;
            _aoes.Add(new(cone, loc, rot, count == 0 ? Module.CastFinishAt(spell, 8.2d) : _aoes.Ref(0).Activation.AddSeconds(count * 2d), shapeDistance: cone.Distance(loc, rot)));
            if (count == 1)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (count != 0 && spell.Action.ID is (uint)AID.OctupleSwipe1 or (uint)AID.OctupleSwipe2 or (uint)AID.OctupleSwipe3 or (uint)AID.OctupleSwipe4)
        {
            _aoes.RemoveAt(0);
            if (count > 1)
            {
                _aoes.Ref(0).Color = Colors.Danger;
            }
        }
    }
}

[SkipLocalsInit]
sealed class ZealousGlower : Components.SimpleAOEGroups
{
    public ZealousGlower(BossModule module) : base(module, [(uint)AID.ZealousGlower1, (uint)AID.ZealousGlower2,(uint)AID.ZealousGlower3, (uint)AID.ZealousGlower4,
    (uint)AID.ZealousGlower5, (uint)AID.ZealousGlower6, (uint)AID.ZealousGlower7, (uint)AID.ZealousGlower8], new AOEShapeRect(5f, 5f), expectedNumCasters: 4)
    {
        MaxDangerColor = 1;
    }
}

[SkipLocalsInit]
sealed class ArdorousEye : Components.SimpleAOEGroups
{
    public ArdorousEye(BossModule module) : base(module, [(uint)AID.ArdorousEye1, (uint)AID.ArdorousEye2,(uint)AID.ArdorousEye3, (uint)AID.ArdorousEye4,
    (uint)AID.ArdorousEye5, (uint)AID.ArdorousEye6, (uint)AID.ArdorousEye7, (uint)AID.ArdorousEye8], new AOEShapeDonutSector(5f, 10f, 45f.Degrees()), expectedNumCasters: 4)
    {
        MaxDangerColor = 1;
    }
}

[SkipLocalsInit]
sealed class DisorientingGroan(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DisorientingGroan, 7f)
{
    private readonly BrutalHalo _aoe = module.FindComponent<BrutalHalo>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && _aoe.AOEs.Count == 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 2.5f), c.Activation);
        }
    }
}

[SkipLocalsInit]
sealed class TwothousandMinaSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwothousandMinaSwing, 8f);

[SkipLocalsInit]
sealed class DD70ForgivenZealStates : StateMachineBuilder
{
    public DD70ForgivenZealStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OctupleSwipe>()
            .ActivateOnEnter<TwothousandMinaSwing>()
            .ActivateOnEnter<BrutalHalo>()
            .ActivateOnEnter<ZealousGlower>()
            .ActivateOnEnter<ArdorousEye>()
            .ActivateOnEnter<DisorientingGroan>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DD70ForgivenZealStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.ForgivenZeal,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1038u,
NameID = 13971u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD70ForgivenZeal(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsCustom([new Polygon(arenaCenter, 10.0015f, 72)]))
{
    private static readonly WPos arenaCenter = new(-300f, -300f);
}
