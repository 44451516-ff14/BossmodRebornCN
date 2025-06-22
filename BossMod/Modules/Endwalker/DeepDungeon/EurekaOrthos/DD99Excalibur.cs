namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD99Excalibur;

public enum OID : uint
{
    Boss = 0x3CFC, // R5.1
    Caliburnus = 0x3CFD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31355, // Boss->player, no cast, single-target
    Teleport = 31326, // Boss->location, no cast, single-target

    ParadoxumVisual = 31353, // Boss->self, 5.0s cast, single-target
    Paradoxum = 31354, // Helper->self, no cast, range 100 circle

    Caliburni = 31333, // Boss->self, 5.0+0,5s cast, single-target
    Steelstrike1 = 31334, // Caliburnus->location, 2.0s cast, width 4 rect charge
    Steelstrike2 = 31335, // Caliburnus->location, 2.0s cast, width 4 rect charge
    ThermalDivideVisual1 = 31349, // Boss->self, 5.0+2,0s cast, single-target, ice left, fire right
    ThermalDivideVisual2 = 31350, // Boss->self, 5.0+2,0s cast, single-target, fire left, ice right
    ThermalDivide1 = 32701, // Helper->self, 6.7s cast, range 40 width 8 rect
    ThermalDivide2 = 31351, // Helper->self, no cast, range 40 width 40 rect

    CallDimensionBlade = 31338, // Boss->self, 4.0s cast, single-target
    SolidSoulsCaliber = 31327, // Boss->self, 6.0s cast, range 10 circle
    EmptySoulsCaliber = 31328, // Boss->self, 6.0s cast, range 5-40 donut
    VacuumSlash = 31342, // Helper->self, 7.0s cast, range 80 45-degree cone
    Flameforge = 31329, // Boss->self, 3.0s cast, single-target
    Frostforge = 31330, // Boss->self, 3.0s cast, single-target
    SteelFlame = 31336, // Caliburnus->location, 2.0s cast, width 4 rect charge
    SteelFrost = 31337, // Caliburnus->location, 2.0s cast, width 4 rect charge

    AbyssalSlash1 = 31339, // Helper->self, 7.0s cast, range 2-7 180-degree donut segment
    AbyssalSlash2 = 31340, // Helper->self, 7.0s cast, range 7-12 180-degree donut segment
    AbyssalSlash3 = 31341, // Helper->self, 7.0s cast, range 17-22 180-degree donut segment

    FlamesRevelation = 31331, // Helper->self, no cast, range 60 circle
    FrostRevelation = 31332, // Helper->self, no cast, range 60 circle

    ExflammeusVisual = 31343, // Boss->self, 4.0s cast, single-target
    Exflammeus = 31344, // Helper->self, 5.0s cast, range 8 circle

    Exglacialis = 31345, // Boss->self, 4.0s cast, single-target
    IceShoot = 31346, // Helper->self, 5.0s cast, range 6 circle
    IceBloomCircle = 31347, // Helper->self, 4.0s cast, range 6 circle
    IceBloomCross = 31348 // Helper->self, no cast, range 40 width 5 cross
}

public enum SID : uint
{
    SoulOfIce = 3411, // Helper->player, extra=0x0
    SoulOfFire = 3410, // Helper->player, extra=0x0
    CaliburnusElement = 2552 // none->Caliburnus, extra=0x219/0x21A (0x21A = frost, 0x219 = fire)
}

class Steelstrike(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20f, 2f);
    private static readonly AOEShapeRect rect2 = new(20f, 2f, 20f);
    private readonly List<AOEInstance> _aoes = new(15);
    private readonly List<Angle> angles = new(15);
    private readonly List<AOEInstance> swordsFire = new(5);
    private readonly List<AOEInstance> swordsIce = new(5);
    private static readonly Angle[] offsets = [default, 120f.Degrees(), 240f.Degrees()];
    private static readonly Angle a20 = 20f.Degrees();
    private static readonly Angle a10 = 10f.Degrees();
    private static readonly Angle a45 = 45f.Degrees();
    private bool? nextRaidwide; // null = none, false = fire, true = ice

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        if (nextRaidwide == null)
            return CollectionsMarshal.AsSpan(_aoes);

        var fire = actor.FindStatus((uint)SID.SoulOfFire) != null;
        var ice = actor.FindStatus((uint)SID.SoulOfIce) != null;

        return ice && (bool)nextRaidwide ? GetSafeAOEs(swordsFire)
            : fire && !(bool)nextRaidwide ? GetSafeAOEs(swordsIce) : CollectionsMarshal.AsSpan(_aoes);
    }

    private AOEInstance[] GetSafeAOEs(List<AOEInstance> swordAOEs)
    {
        var count = _aoes.Count;
        var aoes = new AOEInstance[count];

        var firstSwordAOE = swordAOEs[0];
        var lastSwordAOE = swordAOEs[^1];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe == firstSwordAOE || aoe == lastSwordAOE)
                continue;
            aoes[index++] = _aoes[i];
        }

        aoes[count - 1] = firstSwordAOE with { Color = Colors.SafeFromAOE, Risky = false };
        aoes[count - 2] = lastSwordAOE with { Color = Colors.SafeFromAOE, Risky = false };

        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Caliburni:
                AddAOEs(spell.Rotation);
                ++NumCasts;
                break;
            case (uint)AID.ThermalDivide1:
            case (uint)AID.ParadoxumVisual:
                var count = angles.Count;
                for (var i = 0; i < count; ++i)
                    _aoes.Add(new(rect2, Arena.Center, angles[i], Module.CastFinishAt(spell, 3.4f)));
                break;
            case (uint)AID.Flameforge:
                nextRaidwide = false;
                break;
            case (uint)AID.Frostforge:
                nextRaidwide = true;
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (nextRaidwide != null)
            hints.Add((bool)!nextRaidwide ? "Next raidwide: Fire" : "Next raidwide: Ice");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (nextRaidwide != null)
        {
            var fire = actor.FindStatus((uint)SID.SoulOfFire) != null;
            var ice = actor.FindStatus((uint)SID.SoulOfIce) != null;
            if (ice && (bool)nextRaidwide)
                hints.Add("Get hit by a fire blade!");
            else if (fire && !(bool)nextRaidwide)
                hints.Add("Get hit by a single ice blade!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CaliburnusElement)
        {
            AddSwordAOE(actor, status.Extra == 0x219 ? swordsFire : swordsIce);
            if (swordsFire.Count == 5 && swordsIce.Count == 5)
            {
                swordsFire.Sort((a, b) => a.Rotation.Rad.CompareTo(b.Rotation.Rad));
                swordsIce.Sort((a, b) => a.Rotation.Rad.CompareTo(b.Rotation.Rad));
            }
            void AddSwordAOE(Actor actor, List<AOEInstance> swordAOEs)
            {
                var count = _aoes.Count;
                for (var i = 0; i < count; ++i)
                {
                    var aoe = _aoes[i];
                    if (aoe.Rotation.AlmostEqual(actor.Rotation, Angle.DegToRad))
                    {
                        swordAOEs.Add(aoe);
                        break;
                    }
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Steelstrike1:
                _aoes.Clear();
                break;
            case (uint)AID.Steelstrike2:
            case (uint)AID.SteelFlame:
            case (uint)AID.SteelFrost:
                _aoes.Clear();
                angles.Clear();
                swordsFire.Clear();
                swordsIce.Clear();
                break;
        }
    }

    private void AddAOEs(Angle rotation)
    {
        var numAOEs = NumCasts == 0 ? 10 : 5;
        var startAngle = NumCasts == 0 ? -a45 : -a20;

        for (var i = 0; i < numAOEs; ++i)
        {
            var rot = rotation + startAngle + i * a10;
            if (NumCasts == 0)
                AddSingleAOE(rot);
            else
            {
                for (var j = 0; j < 3; ++j)
                    AddSingleAOE(rot + offsets[j]);
            }
        }
        void AddSingleAOE(Angle rot)
        {
            _aoes.Add(new(rect, Arena.Center, rot));
            angles.Add(rot);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlamesRevelation or (uint)AID.FrostRevelation)
            nextRaidwide = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count == 0)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        var forbidden = new Func<WPos, float>[2];
        var index = 0;
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Color == Colors.SafeFromAOE)
            {
                forbidden[index++] = ShapeDistance.InvertedRect(aoe.Origin, aoe.Rotation, 20f, 20f, 2f);
            }
        }
        if (index == 2)
            hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), _aoes[0].Activation);
    }
}

class ThermalDivideSides(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle a90 = 90f.Degrees();
    private static readonly AOEShapeCone cone = new(40f, a90);
    private bool? pattern; // null = none, false = ice left, fire right, true = fire left, ice right
    private Angle rotation;
    private WDir offset;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (pattern != null)
        {
            var fire = actor.FindStatus((uint)SID.SoulOfFire) != null;
            var ice = actor.FindStatus((uint)SID.SoulOfIce) != null;

            return (bool)pattern ? HandleAOEs(fire) : HandleAOEs(ice);
        }
        else
            return [];
    }

    private AOEInstance[] HandleAOEs(bool condition)
    {
        var pos1 = Arena.Center + offset;
        var pos2 = Arena.Center - offset;

        var check = condition ? 1 : -1;
        var aoes = new AOEInstance[2];
        aoes[0] = new(cone, condition ? pos1 : pos2, rotation + check * a90, activation);
        aoes[1] = new(cone with { InvertForbiddenZone = true }, condition ? pos2 : pos1, rotation - check * a90, activation, Colors.SafeFromAOE);
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ThermalDivideVisual1 or (uint)AID.ThermalDivideVisual2)
        {
            pattern = spell.Action.ID == (uint)AID.ThermalDivideVisual2;
            rotation = spell.Rotation;
            offset = 4f * (spell.Rotation + a90).ToDirection();
            activation = Module.CastFinishAt(spell, 1.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ThermalDivide2)
            pattern = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (pattern == null)
            return;
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        var isRisky = true;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Color == Colors.SafeFromAOE && aoe.Check(actor.Position))
            {
                isRisky = false;
                break;
            }
        }
        hints.Add("Go to safe side!", isRisky);
    }
}

class IceBloomCross(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeCross cross = new(40f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 6 ? 6 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IceShoot)
        {
            var pos = spell.LocXZ;
            var activation = Module.CastFinishAt(spell, 5.2f);
            _aoes.Add(new(cross, pos, Angle.AnglesIntercardinals[1], activation));
            _aoes.Add(new(cross, pos, Angle.AnglesCardinals[1], activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.IceBloomCross)
            _aoes.RemoveAt(0);
    }
}

class AbyssalSlash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSlash1, new AOEShapeDonutSector(2f, 7f, 90f.Degrees()));
class AbyssalSlash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSlash2, new AOEShapeDonutSector(7f, 12f, 90f.Degrees()));
class AbyssalSlash3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalSlash3, new AOEShapeDonutSector(17f, 22f, 90f.Degrees()));
class VacuumSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumSlash, new AOEShapeCone(80f, 22.5f.Degrees()));
class ThermalDivide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalDivide1, new AOEShapeRect(40f, 4f));
class Exflammeus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exflammeus, 8f);
class IceShoot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceShoot, 6f);
class IceBloomCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceBloomCircle, 6f);
class EmptySoulsCaliber(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptySoulsCaliber, new AOEShapeDonut(5f, 40f));
class SolidSoulsCaliber(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SolidSoulsCaliber, 10f);

class DD99ExcaliburStates : StateMachineBuilder
{
    public DD99ExcaliburStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermalDivideSides>()
            .ActivateOnEnter<AbyssalSlash1>()
            .ActivateOnEnter<AbyssalSlash2>()
            .ActivateOnEnter<AbyssalSlash3>()
            .ActivateOnEnter<VacuumSlash>()
            .ActivateOnEnter<ThermalDivide>()
            .ActivateOnEnter<IceShoot>()
            .ActivateOnEnter<IceBloomCircle>()
            .ActivateOnEnter<Exflammeus>()
            .ActivateOnEnter<EmptySoulsCaliber>()
            .ActivateOnEnter<SolidSoulsCaliber>()
            .ActivateOnEnter<Steelstrike>()
            .ActivateOnEnter<IceBloomCross>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 906, NameID = 12100)]
public class DD99Excalibur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600f, -300f), new ArenaBoundsCircle(19.5f));
