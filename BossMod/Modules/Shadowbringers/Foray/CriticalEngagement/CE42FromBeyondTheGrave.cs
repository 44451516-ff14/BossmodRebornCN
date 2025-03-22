namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE42FromBeyondTheGrave;

public enum OID : uint
{
    Boss = 0x2E35, // R8.250, x1
    Deathwall = 0x2EE8, // R0.500, x1
    ShockSphere = 0x3232, // R1.000, spawn during fight
    WarWraith = 0x3233, // R1.800, spawn during fight
    HernaisTheTenacious = 0x3234, // R0.500, spawn during fight
    DyunbuTheAccursed = 0x3235, // R0.500, spawn during fight
    LlofiiTheForthright = 0x3236, // R0.500, x1
    Monoceros = 0x3237, // R1.800, x1
    PurifyingLight = 0x1EB173, // R0.500, EventObj type, spawn during fight
    LivingCorpseSpawn = 0x1EB07A, // R0.500, EventObj type, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackBoss = 24692, // Boss->player, no cast, single-target
    AutoAttackHernais = 6497, // HernaisTheTenacious->player, no cast, single-target
    AutoAttackWraith = 6498, // WarWraith->player, no cast, single-target

    DevourSoul = 24093, // Boss->player, 5.0s cast, single-target, tankbuster
    Blight = 24094, // Boss->self, 5.0s cast, single-target, visual
    BlightAOE = 24095, // Helper->self, 5.0s cast, ???, raidwide
    GallowsMarch = 24096, // Boss->self, 3.0s cast, single-target, visual (applies doom and forced march)
    LivingCorpse = 24097, // Boss->self, 3.0s cast, single-target, visual
    ChainMagick = 24098, // Boss->self, 3.0s cast, single-target, applies dualcast for next soul purge
    SoulPurgeCircle = 24099, // Boss->self, 5.0s cast, range 10 circle
    SoulPurgeCircleDual = 24100, // Boss->self, no cast, range 10 circle
    SoulPurgeDonut = 24101, // Boss->self, 5.0s cast, range 10-30 donut
    SoulPurgeDonutDual = 24102, // Boss->self, no cast, range 10-30 donut
    CrimsonBlade = 24103, // HernaisTheTenacious->self, 8.0s cast, range 50 180-degree cone aoe
    BloodCyclone = 24104, // HernaisTheTenacious->self, 3.0s cast, range 5 circle
    Aethertide = 24105, // DyunbuTheAccursed->self, 8.0s cast, single-target, visual
    AethertideAOE = 24106, // Helper->players, 8.0s cast, range 8 circle spread
    MarchingBreath = 24107, // DyunbuTheAccursed->self, 8.0s cast, interruptible, heals all allies by 20% of max health (raidwide)
    TacticalStone = 24108, // DyunbuTheAccursed->player, 2.5s cast, single-target, autoattack
    TacticalAero = 24109, // DyunbuTheAccursed->self, 3.0s cast, range 40 width 8 rect
    Enrage = 24110, // DyunbuTheAccursed->self, 3.0s cast, applies Dmg up and haste to self
    EntropicFlame = 24111, // WarWraith->self, 4.0s cast, range 60 width 8 rect
    DarkFlare = 24112, // WarWraith->location, 5.0s cast, range 8 circle
    SoulSacrifice = 24113, // WarWraith->Boss, 6.0s cast, interruptible, WarWraith sacrifices to give Dmg Up to Boss

    DeadlyToxin = 24699, // Deathwall->self, no cast, range 25-30 donut, deathwall
    Shock = 24114, // ShockSphere->self, no cast, range 7 circle aoe around sphere

    AutoAttackMonoceros = 871, // Monoceros->Boss, no cast, single-target
    PurifyingLight = 24115, // Monoceros->location, 11.0s cast, range 12 circle, visual
    PurifyingLightAOE = 24116, // Helper->location, no cast, range 12 circle, cleanse doom
    Ruin = 24119, // LlofiiTheForthright->Boss, 2.5s cast, single-target, autoattack
    Cleanse = 24969, // LlofiiTheForthright->location, 5.0s cast, range 6 circle, damages boss
    SoothingGlimmer = 24970 // LlofiiTheForthright->self, 2.5s cast, single-target, heal
}

public enum SID : uint
{
    ForwardMarch = 2161, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForcedMarch = 1257 // Boss->player, extra=0x2/0x1/0x8/0x4
}

class DevourSoul(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DevourSoul));
class Blight(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Blight));

class GallowsMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly PurifyingLight _aoe = module.FindComponent<PurifyingLight>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (!aoes[i].Check(pos))
                return true;
        }
        return false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.CastInfo?.IsSpell(AID.GallowsMarch) ?? false)
            hints.Add("Apply doom & march debuffs");
    }
}

class ShockSphere(BossModule module) : Components.Voidzone(module, 7f, GetSpheres)
{
    private static List<Actor> GetSpheres(BossModule module) => module.Enemies((uint)OID.ShockSphere);
}

class SoulPurge(BossModule module) : Components.GenericAOEs(module)
{
    private bool _dualcast;
    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeCircle _shapeCircle = new(10);
    private static readonly AOEShapeDonut _shapeDonut = new(10, 30);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChainMagick:
                _dualcast = true;
                break;
            case (uint)AID.SoulPurgeCircle:
                AddAOEs(_shapeCircle, _shapeDonut);
                break;
            case (uint)AID.SoulPurgeDonut:
                AddAOEs(_shapeDonut, _shapeCircle);
                break;
        }
        void AddAOEs(AOEShape main, AOEShape dual)
        {
            _aoes.Add(new(main, spell.LocXZ, default, Module.CastFinishAt(spell)));
            if (_dualcast)
            {
                _aoes.Add(new(dual, spell.LocXZ, default, Module.CastFinishAt(spell, 2.1f)));
                _dualcast = false;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SoulPurgeCircle or (uint)AID.SoulPurgeCircleDual or (uint)AID.SoulPurgeDonut or (uint)AID.SoulPurgeDonutDual)
            _aoes.RemoveAt(0);
    }
}

class CrimsonBlade(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CrimsonBlade), new AOEShapeCone(50f, 90f.Degrees()));
class BloodCyclone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BloodCyclone), 5f);
class Aethertide(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AethertideAOE), 8f);
class MarchingBreath(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.MarchingBreath), showNameInHint: true); // heals all allies by 20% of max health (raidwide)
class TacticalAero(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TacticalAero), new AOEShapeRect(40f, 4f));
class EntropicFlame(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(60f, 4f));
class DarkFlare(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DarkFlare), 8f);
class SoulSacrifice(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.SoulSacrifice), showNameInHint: true); // WarWraith sacrifices itself to give boss a damage buff

class PurifyingLight : Components.SimpleAOEs
{
    public PurifyingLight(BossModule module) : base(module, ActionID.MakeSpell(AID.PurifyingLight), 12)
    {
        Color = Colors.SafeFromAOE;
        Risky = false;
    }
}

class CE42FromBeyondTheGraveStates : StateMachineBuilder
{
    public CE42FromBeyondTheGraveStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DevourSoul>()
            .ActivateOnEnter<Blight>()
            .ActivateOnEnter<GallowsMarch>()
            .ActivateOnEnter<ShockSphere>()
            .ActivateOnEnter<SoulPurge>()
            .ActivateOnEnter<CrimsonBlade>()
            .ActivateOnEnter<BloodCyclone>()
            .ActivateOnEnter<Aethertide>()
            .ActivateOnEnter<MarchingBreath>()
            .ActivateOnEnter<TacticalAero>()
            .ActivateOnEnter<EntropicFlame>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<SoulSacrifice>()
            .ActivateOnEnter<PurifyingLight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 20)] // bnpcname=9931
public class CE42FromBeyondTheGrave(WorldState ws, Actor primary) : BossModule(ws, primary, new(-60, 800), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.WarWraith));
        Arena.Actors(Enemies(OID.HernaisTheTenacious));
        Arena.Actors(Enemies(OID.DyunbuTheAccursed));
    }
}
