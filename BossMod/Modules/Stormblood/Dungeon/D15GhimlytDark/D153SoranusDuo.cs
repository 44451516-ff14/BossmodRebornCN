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
    CrosshatchTelegraph1 = 14115, // Helper->self, 3.5s cast, range 20+R width 4 rect
    CrosshatchTelegraph2 = 14411, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrosshatchTelegraph3 = 14412, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrosshatchTelegraph4 = 14116, // Helper->self, 3.5s cast, range 40+R width 4 rect
    CrosshatchTelegraph5 = 14698, // Helper->self, 3.5s cast, range 39+R width 4 rect
    CrosshatchTelegraph6 = 14697, // Helper->self, 3.5s cast, range 35+R width 4 rect
    CrosshatchTelegraph7 = 14413, // Helper->self, 3.5s cast, range 29+R width 4 rect
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

class Innocence(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Innocence));
class DeltaTrance(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.DeltaTrance));
class Heirsbane(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Heirsbane), "Single target damage");

class ArtificialPlasmaAnnia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArtificialPlasmaAnnia));
class ArtificialPlasmaJulia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArtificialPlasmaJulia));

class Crossbones(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.Crossbones), 2);
class CrossbonesKB(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private DateTime _activation;
    private Actor? _caster;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_caster != null)
            yield return new(_caster.Position, 15, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Crossbones)
        {
            _activation = Module.CastFinishAt(spell);
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Crossbones)
            _caster = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<Bombardment>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class AngrySalamander(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AngrySalamander), new AOEShapeRect(45.6f, 3));
class Quaternity1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Quaternity1), new AOEShapeRect(41, 2));
class Quaternity2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Quaternity2), new AOEShapeRect(23, 2, 3));
class StunningSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StunningSweep), new AOEShapeCircle(6.6f));
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Bombardment), 10);
class MagitekMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MissileImpact), 6);
class CoveringFire(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.CoveringFire), 8);

class CeruleumTanks(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly AOEShapeCircle circleRoundhouse = new(6.6f);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos[] origins = [new(370.992f, -277.028f), new(362.514f, -273.49f), new(379.485f, -273.49f), new(358.999f, -265.034f),
    new(382.986f, -265.034f), new(379.485f, -256.52f), new(362.514f, -256.52f), new(370.992f, -253.01f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count == 9 ? 4 : _aoes.Count == 8 ? 3 : 2;
        return _aoes.Skip(count).Take(4).Concat(_aoes.Take(count).Select(a => a with { Color = Colors.Danger }));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.CeruleumTank && _aoes.Count == 0)
        {
            for (var i = 0; i < origins.Length; ++i)
            {
                float activation;
                if (_aoes.Count == 0)
                    activation = 8.8f;
                else
                {
                    var set = (_aoes.Count + 1) * 0.5f;
                    activation = 8.8f + 2.4f * set;
                }
                _aoes.Add(new(circle, origins[i], default, WorldState.FutureTime(activation)));
            }
            _aoes.Add(new(circleRoundhouse, new(370.992f, -265.034f), default, WorldState.FutureTime(3.1f)));
            _aoes.SortBy(x => x.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.Roundhouse or AID.Burst)
            _aoes.RemoveAt(0);
    }
}

class Crosshatch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40.5f, 2);
    private static readonly HashSet<AID> telegraphs = [AID.CrosshatchTelegraph1, AID.CrosshatchTelegraph2, AID.CrosshatchTelegraph3, AID.CrosshatchTelegraph4,
    AID.CrosshatchTelegraph5, AID.CrosshatchTelegraph6, AID.CrosshatchTelegraph7, AID.CrosshatchTelegraph8];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            yield return _aoes[0] with { Color = Colors.Danger };
            for (var i = 1; i < count; ++i)
                yield return _aoes[i];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (telegraphs.Contains((AID)spell.Action.ID))
            _aoes.Add(new(rect, caster.Position, spell.Rotation, _aoes.Count == 0 ? Module.CastFinishAt(spell, 2.1f) : _aoes[0].Activation.AddSeconds(_aoes.Count * 0.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.Crosshatch)
            _aoes.RemoveAt(0);
    }
}

abstract class ImperialAuthority(BossModule module, AID aid) : Components.CastHint(module, ActionID.MakeSpell(aid), "Enrage!", true);
class ImperialAuthorityAnnia(BossModule module) : ImperialAuthority(module, AID.ImperialAuthorityAnnia);
class ImperialAuthorityJulia(BossModule module) : ImperialAuthority(module, AID.ImperialAuthorityJulia);

class D153SoranusDuoStates : StateMachineBuilder
{
    public D153SoranusDuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Innocence>()
            .ActivateOnEnter<DeltaTrance>()
            .ActivateOnEnter<Heirsbane>()
            .ActivateOnEnter<ArtificialPlasmaAnnia>()
            .ActivateOnEnter<ArtificialPlasmaJulia>()
            .ActivateOnEnter<Crossbones>()
            .ActivateOnEnter<CrossbonesKB>()
            .ActivateOnEnter<AngrySalamander>()
            .ActivateOnEnter<Quaternity1>()
            .ActivateOnEnter<Quaternity2>()
            .ActivateOnEnter<StunningSweep>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<CoveringFire>()
            .ActivateOnEnter<CeruleumTanks>()
            .ActivateOnEnter<Crosshatch>()
            .ActivateOnEnter<ImperialAuthorityAnnia>()
            .ActivateOnEnter<ImperialAuthorityJulia>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7861, SortOrder = 5)]
public class D153SoranusDuo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(371, -265), 19.5f, 24)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.JuliaQuoSoranus).Concat([PrimaryActor]));
    }

    protected override bool CheckPull() => Enemies(OID.JuliaQuoSoranus).Concat([PrimaryActor]).Any(x => x.InCombat);
}
