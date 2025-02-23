﻿namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D013Apollyon;

public enum OID : uint
{
    Boss = 0x4165, // R7.0
    IhuykatumuOcelot = 0x4166, // R3.57
    IhuykatumuPuma = 0x4167, // R2.52
    IhuykatumuSandworm1 = 0x4169, // R3.3
    IhuykatumuSandworm2 = 0x4168, // R3.3
    Whirlwind = 0x416C, // R1.0
    LightningVoidzone = 0x1EBA21, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // IhuykatumuOcelot/IhuykatumuPuma/IhuykatumuSandworm2/IhuykatumuSandworm1->Boss, no cast, single-target

    RazorZephyr = 36340, // Boss->self, 4.0s cast, range 50 width 12 rect

    Blade = 36347, // Boss->player, 4.5s cast, single-target

    Teleport = 36344, // Boss->location, no cast, single-target
    HighWind = 36341, // Boss->self, 5.0s cast, range 60 circle
    Devour = 36342, // Boss->self, no cast, single-target
    SwarmingLocust = 36343, // Boss->self, 3.0s cast, single-target

    BladesOfFamineVisual = 36345, // Boss->self, 2.2+0.8s cast, single-target
    BladesOfFamine = 36346, // Helper->self, 3.0s cast, range 50 width 12 rect

    LevinsickleVisual = 36348, // Boss->self, 4.5+0.5s cast, single-target
    Levinsickle = 36350, // Helper->location, 5.0s cast, range 4 circle

    LevinsickleSpark = 36349, // Helper->location, 5.0s cast, range 4 circle

    WingOfLightning = 36351, // Helper->self, 8.0s cast, range 40 45-degree cone

    ThunderIIIVisual = 36352, // Boss->self, 4.0+1.0s cast, single-target
    ThunderIII = 36353, // Helper->player, 5.0s cast, range 6 circle, spread

    SandwormVisual = 36354, // IhuykatumuSandworm1/IhuykatumuSandworm2->self, no cast, single-target

    BladeVisual = 36356, // Boss->player, 4.5s cast, single-target
    BladeTB = 36357, // Helper->player, 5.0s cast, range 6 circle

    WindSickle = 36358, // Helper->self, 4.0s cast, range 5-60 donut
    RazorStorm = 36355, // Boss->self, 5.0s cast, range 40 width 40 rect
    Windwhistle = 36359, // Boss->self, 4.0s cast, single-target
    CuttingWind = 36360, // Helper->self, no cast, range 72 width 8 rect
    BitingWind = 36761 // Helper->player, no cast, single-target
}

class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 4.5f, GetWhirlwind, 5f)
{
    private static List<Actor> GetWhirlwind(BossModule module) => module.Enemies((uint)OID.Whirlwind);
}
class Blade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Blade));
class HighWind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HighWind));

abstract class Blades(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50f, 6f));
class RazorZephyr(BossModule module) : Blades(module, AID.RazorZephyr);
class BladesOfFamine(BossModule module) : Blades(module, AID.BladesOfFamine);

class Levinsickle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Levinsickle), 4f);
class LevinsickleSpark(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 4f, ActionID.MakeSpell(AID.LevinsickleSpark), GetVoidzones, 0.7f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.LightningVoidzone);
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
class WingOfLightning(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WingOfLightning), new AOEShapeCone(40f, 22.5f.Degrees()), 8);

class ThunderIII2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderIII), 6f);
class BladeTB(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.BladeTB), new AOEShapeCircle(6f), true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count != 0)
            hints.Add("Tankbuster cleave");
    }
}

class WindSickle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WindSickle), new AOEShapeDonut(5f, 60f));
class RazorStorm(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.RazorStorm), new AOEShapeRect(40f, 20f));

class CuttingWind(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeRect rect = new(36f, 4f, 36f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
            aoes[i] = _aoes[i];
        return aoes;
    }

    private static readonly double[] delays = [8.6d, 16.7d, 24.7d];
    private static readonly Angle[] angles = [89.999f.Degrees(), 44.998f.Degrees(), 134.999f.Degrees(), -0.003f.Degrees()];

    public override void OnActorCreated(Actor actor)
    {
        void AddWhirlwind(WPos[] pos)
        {
            for (var i = 0; i < 3; ++i)
            {
                var delay = WorldState.FutureTime(delays[i]);
                var posi = WPos.ClampToGrid(pos[i]);
                for (var j = 0; j < 4; ++j)
                    _aoes.Add(new(rect, posi, angles[j], delay));
            }
        }
        if (actor.OID == (uint)OID.Whirlwind)
            if (actor.Position.AlmostEqual(new WPos(-121f, 279f), 1f))
                AddWhirlwind([new(-102.935f, 274.357f), new(-108.935f, 262.224f), new(-105.733f, 252.340f)]); // SW whirlwind
            else
                AddWhirlwind([new(-111.688f, 253.942f), new(-102.276f, 264.313f), new(-108.922f, 276.528f)]); // NW whirlwind
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.CuttingWind)
            _aoes.RemoveAt(0);
    }
}

class D013ApollyonStates : StateMachineBuilder
{
    public D013ApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<HighWind>()
            .ActivateOnEnter<BladesOfFamine>()
            .ActivateOnEnter<Levinsickle>()
            .ActivateOnEnter<LevinsickleSpark>()
            .ActivateOnEnter<WingOfLightning>()
            .ActivateOnEnter<ThunderIII2>()
            .ActivateOnEnter<BladeTB>()
            .ActivateOnEnter<WindSickle>()
            .ActivateOnEnter<RazorStorm>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<CuttingWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12711)]
public class D013Apollyon(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(new(-107f, 265f), 19.5f, 32)], [new Rectangle(new(-107f, 285.75f), 20f, 2f)]);
}
