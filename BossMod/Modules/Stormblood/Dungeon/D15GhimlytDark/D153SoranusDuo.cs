namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D153SoranusDuo;

public enum OID : uint
{
    Boss = 0x25B0, // R0.6, seems to be random if Julia or Annia is first?
    JuliaQuoSoranus = 0x25AF, // R0.6
    SoranusDuo1 = 0x25B5, // R1.0
    SoranusDuo2 = 0x25B1, // R0.0
    CeruleumTank = 0x25B2, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // JuliaQuoSoranus->player, no cast, single-target
    TeleportAnnia = 14095, // Boss->location, no cast, single-target
    TeleportJulia = 14094, // JuliaQuoSoranus->location, no cast, single-target

    Heirsbane = 14123, // JuliaQuoSoranus->player, 3.0s cast, single-target, single target dmg similar to tankbuster
    DeltaTrance = 14122, // Boss->player, 4.0s cast, single-target, tankbuster
    Innocence = 14121, // JuliaQuoSoranus->player, 4.0s cast, single-target, tankbuster

    ArtificialPlasmaAnnia = 14120, // Boss->self, 3.0s cast, range 40+R circle, raidwide
    ArtificialPlasmaJulia = 14119, // JuliaQuoSoranus->self, 3.0s cast, range 40+R circle

    TheOrder1 = 14099, // JuliaQuoSoranus->self, no cast, single-target
    TheOrder2 = 14100, // JuliaQuoSoranus->self, 5.0s cast, single-target
    TheOrder3 = 14778, // JuliaQuoSoranus->self, 3.0s cast, single-target
    OrderToBombard = 14096, // Boss->self, 5.0s cast, single-target

    Crossbones = 15488, // SoranusDuo1->player, 5.0s cast, width 4 rect charge, knockback 15, away from source
    AngrySalamander = 14124, // Boss->self, 3.0s cast, range 45+R width 6 rect
    Bombardment = 14097, // Helper->location, 7.0s cast, range 10 circle

    Quaternity1 = 14131, // SoranusDuo1->self, 3.0s cast, range 40+R width 4 rect
    Quaternity2 = 14729, // SoranusDuo1->self, 3.0s cast, range 25+R width 4 rect
    StunningSweep = 14098, // Boss->self, 4.0s cast, range 6+R circle

    CrosshatchVisual = 14113, // SoranusDuo2->self, no cast, single-target

    CrosshatchTelegraph1 = 14411, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrosshatchTelegraph2 = 14412, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrosshatchTelegraph3 = 14413, // Helper->self, 3.5s cast, range 29+R width 4 rect
    CrosshatchTelegraph4 = 14115, // Helper->self, 3.5s cast, range 20+R width 4 rect
    CrosshatchTelegraph5 = 14116, // Helper->self, 3.5s cast, range 40+R width 4 rect
    CrosshatchTelegraph6 = 14697, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrosshatchTelegraph7 = 14698, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrosshatchTelegraph8 = 14699, // Helper->self, 3.5s cast, range 40+R width 4 rect

    Crosshatch = 14114, // Helper->self, no cast, range 40+R width 4 rect
    CrosshatchVisualJulia = 14118, // JuliaQuoSoranus->self, no cast, single-target
    CrosshatchVisualAnnia = 14117, // Boss->self, no cast, single-target

    CommenceAirStrike = 14102, // JuliaQuoSoranus->self, 3.0s cast, single-target
    AglaeaBite = 14103, // Boss->CeruleumTank, no cast, single-target, kicks a single ceruleum tank, knockback 8, away from source
    Roundhouse = 14104, // Boss->self, no cast, range 6+R circle, kicks multiple ceruleum tanks, knockback 8, away from source
    HeirsbaneCeruleum = 14105, // JuliaQuoSoranus->CeruleumTank, 3.5s cast, single-target, ignites first ceruleum tank
    Burst = 14106, // CeruleumTank->self, 2.0s cast, range 10+R circle

    OrderToFire = 14125, // Boss->self, 3.0s cast, single-target
    MissileImpact = 14126, // Helper->location, 3.0s cast, range 6 circle

    OrderToSupport = 14107, // Boss->self, 3.0s cast, single-target
    CoveringFire = 14108, // Helper->player, 5.0s cast, range 8 circle, spread

    ArtificialBoostJulia = 14127, // JuliaQuoSoranus->self, 3.0s cast, single-target
    ArtificialBoostAnnia = 14128, // Boss->self, 3.0s cast, single-target
    ImperialAuthorityJulia = 14129, // JuliaQuoSoranus->self, 40.0s cast, range 80 circle, enrage
    ImperialAuthorityAnnia = 14130, // Boss->self, 40.0s cast, range 80 circle, enrage
}

class Innocence(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Innocence);
class DeltaTrance(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DeltaTrance);
class Heirsbane(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Heirsbane, "Single target damage");

class ArtificialPlasma(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ArtificialPlasmaAnnia, (uint)AID.ArtificialPlasmaJulia]);

class Crossbones(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Crossbones, 2f);

class CrossbonesKB(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private DateTime _activation;
    private Actor? _caster;
    private readonly Bombardment _aoe = module.FindComponent<Bombardment>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _caster != null ? new Knockback[1] { new(_caster.Position, 15f, _activation) } : [];

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Crossbones)
        {
            _activation = Module.CastFinishAt(spell);
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Crossbones)
            _caster = null;
    }
}

class AngrySalamander(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AngrySalamander, new AOEShapeRect(45.6f, 3f));
class Quaternity1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Quaternity1, new AOEShapeRect(41f, 2f));
class Quaternity2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Quaternity2, new AOEShapeRect(26f, 2f));
class StunningSweep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StunningSweep, 6.6f);
class Bombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombardment, 10f);
class MagitekMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MissileImpact, 6f);
class CoveringFire(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CoveringFire, 8f);

class CeruleumTanks(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly AOEShapeCircle circleRoundhouse = new(6.6f);
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly WPos[] origins = [new(370.992f, -277.028f), new(362.514f, -273.49f), new(379.485f, -273.49f), new(358.999f, -265.034f),
    new(382.986f, -265.034f), new(379.485f, -256.52f), new(362.514f, -256.52f), new(370.992f, -253.01f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count == 9 ? 8 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        if (count > 2)
        {
            var maxDanger = count == 9 ? 4 : count == 8 ? 3 : 2;
            var maxDangerAdj = maxDanger - 1;
            var color = Colors.Danger;
            for (var i = 0; i < count; ++i)
            {
                if (i <= maxDangerAdj)
                {
                    aoes[i].Color = color;
                }
            }
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (_aoes.Count == 0 && actor.OID == (uint)OID.CeruleumTank)
        {
            _aoes.Add(new(circleRoundhouse, new WPos(370.992f, -265.034f).Quantized(), default, WorldState.FutureTime(3.1d)));
            for (var i = 0; i < 8; ++i)
            {
                var activation = i switch
                {
                    0 => 5.8d,
                    1 or 2 => 8.3d,
                    3 or 4 => 10.7d,
                    5 or 6 => 13.2d,
                    7 => 15.7d,
                    _ => default
                };
                _aoes.Add(new(circle, origins[i].Quantized(), default, WorldState.FutureTime(activation)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Roundhouse or (uint)AID.Burst)
            _aoes.RemoveAt(0);
    }
}

class Crosshatch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rect = new(40.5f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
            aoes[0].Color = Colors.Danger;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CrosshatchTelegraph1:
            case (uint)AID.CrosshatchTelegraph2:
            case (uint)AID.CrosshatchTelegraph3:
            case (uint)AID.CrosshatchTelegraph4:
            case (uint)AID.CrosshatchTelegraph5:
            case (uint)AID.CrosshatchTelegraph6:
            case (uint)AID.CrosshatchTelegraph7:
            case (uint)AID.CrosshatchTelegraph8:
                _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, _aoes.Count == 0 ? Module.CastFinishAt(spell, 2.1d) : _aoes[0].Activation.AddSeconds(_aoes.Count * 0.5d)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Crosshatch)
            _aoes.RemoveAt(0);
    }
}

class ImperialAuthority(BossModule module) : Components.CastHints(module, [(uint)AID.ImperialAuthorityAnnia, (uint)AID.ImperialAuthorityJulia], "Enrage!", true);

class D153SoranusDuoStates : StateMachineBuilder
{
    public D153SoranusDuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<DeltaTrance>()
            .ActivateOnEnter<Heirsbane>()
            .ActivateOnEnter<ArtificialPlasma>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<Crossbones>()
            .ActivateOnEnter<CrossbonesKB>()
            .ActivateOnEnter<AngrySalamander>()
            .ActivateOnEnter<Quaternity1>()
            .ActivateOnEnter<Quaternity2>()
            .ActivateOnEnter<StunningSweep>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<CoveringFire>()
            .ActivateOnEnter<CeruleumTanks>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<ImperialAuthority>()
            .Raw.Update = () =>
            {
                var julia = module.Enemies((uint)OID.JuliaQuoSoranus);
                var count = julia.Count;
                return module.PrimaryActor.IsDeadOrDestroyed && (count == 0 || count != 0 && julia[0].IsDeadOrDestroyed);
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7861, SortOrder = 5)]
public class D153SoranusDuo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(371f, -265f), 19.5f, 24)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.JuliaQuoSoranus));
    }

    protected override bool CheckPull()
    {
        var julia = Enemies((uint)OID.JuliaQuoSoranus);
        return PrimaryActor.InCombat || julia.Count != 0 && julia[0].InCombat;
    }
}
