namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD80ProtoKaliya;

public enum OID : uint
{
    Boss = 0x3D18, // R5.0
    WeaponsDrone = 0x3D19, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31421, // Boss->players, no cast, range 6+R 90-degree cone

    AetheromagnetismKB = 31431, // Helper->player, no cast, single-target, knockback 10, away from source
    AetheromagnetismPull = 31430, // Helper->player, no cast, single-target, pull 10, between centers
    AutoCannons = 31432, // WeaponsDrone->self, 4.0s cast, range 41+R width 5 rect
    Barofield = 31427, // Boss->self, 3.0s cast, single-target

    CentralizedNerveGasVisual = 31423, // Boss->self, 4.5s cast, range 25+R 120-degree cone
    CentralizedNerveGas = 32933, // Helper->self, 5.3s cast, range 25+R 120-degree cone
    LeftwardNerveGasVisual = 31424, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    LeftwardNerveGas = 32934, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    RightwardNerveGasVisual = 31425, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    RightwardNerveGas = 32935, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    NerveGasRingVisual = 31426, // Boss->self, 5.0s cast, range 8-30 donut
    NerveGasRing = 32930, // Helper->self, 7.2s cast, range 8-30 donut
    Resonance = 31422, // Boss->player, 5.0s cast, range 12 90-degree cone, tankbuster

    NanosporeJet = 31429 // Boss->self, 5.0s cast, range 100 circle
}

public enum SID : uint
{
    Barofield = 3420, // none->Boss, extra=0x0, damage taken when near boss
    NegativeChargePlayer = 3419, // none->player, extra=0x0
    PositiveChargePlayer = 3418, // none->player, extra=0x0
    NegativeChargeDrone = 3417, // none->WeaponsDrone, extra=0x0
    PositiveChargeDrone = 3416 // none->WeaponsDrone, extra=0x0
}

public enum TetherID : uint
{
    Magnetism = 38
}

class Magnetism(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Knockback?[] _sources = new Knockback?[4];
    private readonly NerveGasRingAndAutoCannons _aoe1 = module.FindComponent<NerveGasRingAndAutoCannons>()!;
    private readonly Barofield _aoe2 = module.FindComponent<Barofield>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_sources[slot] is Knockback source)
        {
            var count = _aoe1.AOEs.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoe1.AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Shape == NerveGasRingAndAutoCannons.donut)
                {
                    return new Span<Knockback>([source]);
                }
            }
        }
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe1.AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe1.AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }

        return _aoe2.AOE != null && _aoe2.AOE.Value.Check(pos) || !Module.InBounds(pos);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        bool IsPull(ref Actor target)
        => source.FindStatus((uint)SID.NegativeChargeDrone) != null && target.FindStatus((uint)SID.PositiveChargePlayer) != null ||
        source.FindStatus((uint)SID.PositiveChargeDrone) != null && target.FindStatus((uint)SID.NegativeChargePlayer) != null;

        bool IsKnockback(ref Actor target)
        => source.FindStatus((uint)SID.NegativeChargeDrone) != null && target.FindStatus((uint)SID.NegativeChargePlayer) != null ||
        source.FindStatus((uint)SID.PositiveChargeDrone) != null && target.FindStatus((uint)SID.PositiveChargePlayer) != null;

        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            if (IsPull(ref target))
                AddSource(false);
            else if (IsKnockback(ref target))
                AddSource(true);

            void AddSource(bool isKnockback) => _sources[Raid.FindSlot(target.InstanceID)] = new(source.Position, 10f, WorldState.FutureTime(10d), kind: isKnockback ? Kind.AwayFromOrigin : Kind.TowardsOrigin, ignoreImmunes: true);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            _sources[Raid.FindSlot(target.InstanceID)] = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_sources[slot] is Knockback source)
        {
            var attract = source.Kind == Kind.TowardsOrigin;
            var pos = Module.PrimaryActor.Position;
            var barofield = ShapeDistance.Circle(pos, 5f);
            var arena = ShapeDistance.InvertedCircle(pos, 8f);
            var cannons = Module.Enemies((uint)OID.WeaponsDrone);
            var count = cannons.Count;
            var forbiddenRects = new Func<WPos, float>[count];
            for (var i = 0; i < count; ++i)
            {
                var c = cannons[i];
                forbiddenRects[i] = ShapeDistance.Rect(c.Position, c.Rotation, 43f, default, 2.5f);
            }
            var all = ShapeDistance.Union((Func<WPos, float>[])[barofield, arena, .. forbiddenRects]);

            hints.AddForbiddenZone(p =>
            {
                var dir = (p - source.Origin).Normalized();
                var kb = attract ? -dir : dir;

                // prevent KB through death zone in center
                if (Intersect.RayCircle(p, kb, pos, 5f) < 99f)
                    return 0f;

                return all(p + kb * 10f);
            }, source.Activation);
        }
    }
}

class Barofield(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f);
    public AOEInstance? AOE;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (AOE == null && spell.Action.ID is (uint)AID.Barofield or (uint)AID.NanosporeJet)
            AOE = new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
            AOE = null;
    }
}

class NerveGasRingAndAutoCannons(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCross cross = new(20f, 2.5f);
    private static readonly AOEShapeCross rect = new(43f, 2.5f);
    public static readonly AOEShapeDonut donut = new(8f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => AOEs.Add(new(shape, spell.LocXZ, shape == rect ? spell.Rotation : default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        if (spell.Action.ID == (uint)AID.NerveGasRing)
        {
            AddAOE(donut);
            AddAOE(cross);
        }
        else if (spell.Action.ID == (uint)AID.AutoCannons)
        {
            AddAOE(rect);
            var count = AOEs.Count;
            for (var i = 0; i < count; ++i)
            {
                if (AOEs[i].Shape == cross)
                {
                    AOEs.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.NerveGasRing or (uint)AID.AutoCannons)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (AOEs[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

class NerveGas(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftwardNerveGas, (uint)AID.RightwardNerveGas], new AOEShapeCone(25.5f, 90f.Degrees()));

class CentralizedNerveGas(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentralizedNerveGas, new AOEShapeCone(25.5f, 60f.Degrees()));

class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(11f, 45f.Degrees()))
{
    private readonly Barofield _aoe = module.FindComponent<Barofield>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOE == null)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_aoe.AOE == null)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class Resonance(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Resonance, new AOEShapeCone(12f, 45f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class DD80ProtoKaliyaStates : StateMachineBuilder
{
    public DD80ProtoKaliyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Barofield>()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Resonance>()
            .ActivateOnEnter<NerveGasRingAndAutoCannons>()
            .ActivateOnEnter<NerveGas>()
            .ActivateOnEnter<CentralizedNerveGas>()
            .ActivateOnEnter<Magnetism>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, legendoficeman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 904, NameID = 12247)]
public class DD80ProtoKaliya(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(20f));
