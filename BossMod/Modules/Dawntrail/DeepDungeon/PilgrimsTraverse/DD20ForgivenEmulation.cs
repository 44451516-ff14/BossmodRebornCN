namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD20ForgivenEmulation;

public enum OID : uint
{
    ForgivenEmulation = 0x485C, // R9.9
    Root = 0x1EBE4B, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45130, // ForgivenEmulation->player, no cast, single-target

    TouchdownVisual = 43454, // ForgivenEmulation->self, 14.5s cast, single-target
    Touchdown = 43455, // Helper->self, 14.0s cast, range 30 circle, knockback 10, away from source
    Burst1 = 43456, // Helper->self, 2.0s cast, range 11 circle
    Burst2 = 43457, // Helper->self, 3.3s cast, range 11 circle
    Burst3 = 43458, // Helper->self, 4.6s cast, range 11 circle
    Burst4 = 43459, // Helper->self, 5.9s cast, range 11 circle
    BareRootPlantingVisual = 43460, // ForgivenEmulation->self, 5.0s cast, single-target
    BareRootPlanting = 43461, // Helper->location, 3.0s cast, range 6 circle
    DensePlantingVisual = 43463, // ForgivenEmulation->self, 5.0s cast, single-target
    DensePlanting = 43464, // Helper->location, 3.0s cast, range 6 circle
    WoodsEmbrace = 43462 // Helper->self, no cast, range 3 width 6 cross
}

public enum SID : uint
{
    Touchdown = 2056, // none->ForgivenEmulation, extra=0x396/0x397/0x398/0x395
    AreaOfInfluenceUp = 1909 // none->Helper, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9
}

public enum IconID : uint
{
    BareRootPlanting = 23 // player->self
}

[SkipLocalsInit]
sealed class DenseBareRootPlanting(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DensePlanting, (uint)AID.BareRootPlanting], 6f);

[SkipLocalsInit]
sealed class Touchdown(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Touchdown, 10f)
{
    private readonly Burst _aoe = module.FindComponent<Burst>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        var max = len > 3 ? 3 : len;
        for (var i = 0; i < max; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var source = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(source.Origin, 4f), source.Activation);
        }
    }
}

[SkipLocalsInit]
sealed class Burst(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(4);
    private readonly AOEShapeCircle circle = new(11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID != (uint)SID.Touchdown)
        {
            return;
        }
        WPos pos = status.Extra switch
        {
            0x395 => new(-293.65f, -306.365f), // .65 must be a typo from the developers, TODO: check in future if it has been fixed
            0x396 => new(-306.365f, -306.365f),
            0x397 => new(-293.365f, -293.365f),
            0x398 => new(-306.365f, -293.365f),
            _ => default
        };
        if (pos != default)
        {
            var count = AOEs.Count;
            AOEs.Add(new(circle, pos.Quantized(), default, count == 0 ? WorldState.FutureTime(14.7d) : AOEs.Ref(count - 1).Activation.AddSeconds(1.3d), shapeDistance: circle.Distance(pos, default)));
            if (count == 1)
            {
                AOEs.Ref(0).Color = Colors.Danger;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = AOEs.Count;
        if (count != 0 && spell.Action.ID is (uint)AID.Burst1 or (uint)AID.Burst2 or (uint)AID.Burst3 or (uint)AID.Burst4)
        {
            AOEs.RemoveAt(0);
            if (count > 2)
            {
                AOEs.Ref(0).Color = Colors.Danger;
            }
        }
    }
}

[SkipLocalsInit]
sealed class BareRootPlantingBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly AOEShapeCircle circle = new(6f);
    private readonly AOEShapeCross cross = new(30f, 3f); // 3 base range + 9 * 3 from status effect

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BareRootPlanting)
        {
            var activation = WorldState.FutureTime(5.2d);
            var primary = Module.PrimaryActor;
            AddBait(circle);
            AddBait(cross, Angle.AnglesCardinals[1]);
            AddBait(cross, Angle.AnglesIntercardinals[1]);
            void AddBait(AOEShape shape, Angle rotation = default) => CurrentBaits.Add(new(primary, actor, shape, activation, default, rotation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BareRootPlanting)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 13.5f), CurrentBaits.Ref(0).Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsBaitTarget(actor))
        {
            hints.Add("Drop baited AOEs at arena border!");
        }
    }
}

[SkipLocalsInit]
sealed class WoodsEmbrace(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly List<SDCross> sdfs = new(2);
    private readonly AOEShapeCross cross = new(30f, 3f);  // 3 base range + 9 * 3 from status effect

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BareRootPlanting)
        {
            var pos = spell.LocXZ;
            var act = Module.CastFinishAt(spell, 4.4d);
            AddAOE(pos, Angle.AnglesCardinals[1], act);
            AddAOE(pos, Angle.AnglesIntercardinals[1], act);
            void AddAOE(WPos position, Angle rotation, DateTime activation) => _aoes.Add(new(cross, position, rotation, activation, shapeDistance: cross.Distance(position, rotation)));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AreaOfInfluenceUp)
        {
            sdfs.Clear();
            var r = 3f + 3f * status.Extra;
            var pos = _aoes.Count != 0 ? _aoes.Ref(0).Origin : actor.Position.Quantized();
            AddSDF(pos, r, Angle.AnglesCardinals[1]);
            AddSDF(pos, r, Angle.AnglesIntercardinals[1]);
            void AddSDF(WPos position, float range, Angle rotation) => sdfs.Add(new SDCross(position, rotation, range, 3f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WoodsEmbrace)
        {
            if (++NumCasts == 30)
            {
                _aoes.Clear();
                sdfs.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (sdfs.Count != 0)
        {
            for (var i = 0; i < 2; ++i)
            {
                hints.TemporaryObstacles.Add(sdfs[i]);
            }
        }
    }
}

[SkipLocalsInit]
sealed class DD20ForgivenEmulationStates : StateMachineBuilder
{
    public DD20ForgivenEmulationStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BareRootPlantingBait>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DenseBareRootPlanting>()
            .ActivateOnEnter<WoodsEmbrace>()
            .ActivateOnEnter<Touchdown>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DD20ForgivenEmulationStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.ForgivenEmulation,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1033u,
NameID = 13973u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD20ForgivenEmulation(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsCustom([new Polygon(arenaCenter, 14.709f, 72)]))
{
    private static readonly WPos arenaCenter = new(-300f, -300f);
}
