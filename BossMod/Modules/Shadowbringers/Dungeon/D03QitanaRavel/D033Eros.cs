namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D033Eros;

public enum OID : uint
{
    Boss = 0x27B1, //R=7.02
    PoisonVoidzone = 0x1E972C,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Jump = 15519, // Boss->location, no cast, single-target, visual

    Rend = 15513, // Boss->player, 4.0s cast, single-target, tankbuster
    HoundOutOfHeaven = 15514, // Boss->self, 5.0s cast, single-target
    HoundOutOfHeavenSuccess = 17079, // Boss->player, no cast, single-target, tether stretch success
    HoundOutOfHeavenFail = 17080, // Boss->player, no cast, single-target, tether stretch fail
    Glossolalia = 15515, // Boss->self, 3.0s cast, range 50 circle, raidwide
    ViperPoison = 15516, // Boss->self, 6.0s cast, single-target
    ViperPoisonPatterns = 15518, // Helper->location, 6.0s cast, range 6 circle
    ViperPoisonBait = 15517, // Helper->player, 6.0s cast, range 6 circle

    Inhale = 17168, // Boss->self, 4.0s cast, range 50 circle, attract 50 between centers

    HeavingBreathVisual = 16923, // Helper->self, 3.5s cast, range 42 width 30 rect
    HeavingBreath = 15520, // Boss->self, 3.5s cast, range 50 circle, knockback 35 forward

    ConfessionOfFaithVisual1 = 15524, // Boss->self, 5.0s cast, single-target, left/right breath
    ConfessionOfFaithVisual2 = 15521, // Boss->self, 5.0s cast, single-target, center breath
    ConfessionOfFaithLeft = 15526, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithRight = 15527, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithCenter = 15522, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithStack = 15525, // Helper->players, 5.8s cast, range 6 circle, stack
    ConfessionOfFaithSpread = 15523 // Helper->player, 5.8s cast, range 5 circle, spread
}

class HoundOutOfHeaven(BossModule module) : Components.StretchTetherDuo(module, 19f, 5.2f);
class ViperPoisonVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, ActionID.MakeSpell(AID.ViperPoisonPatterns), GetVoidzones, 0.8f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.PoisonVoidzone);
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
class ConfessionOfFaithStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithStack), 6f, 4, 4);
class ConfessionOfFaithSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithSpread), 5f);

abstract class Breath(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60f, 30f.Degrees()));
class ConfessionOfFaithCenter(BossModule module) : Breath(module, AID.ConfessionOfFaithCenter);
class ConfessionOfFaithLeft(BossModule module) : Breath(module, AID.ConfessionOfFaithLeft);
class ConfessionOfFaithRight(BossModule module) : Breath(module, AID.ConfessionOfFaithRight);

class ViperPoisonBait(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.ViperPoisonBait), new AOEShapeCircle(6f), true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(new(17f, -518f), new(17f, -558f), 13f));
    }
}

class Inhale(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.Inhale), 50f, kind: Kind.TowardsOrigin)
{
    private readonly ViperPoisonVoidzone _aoe = module.FindComponent<ViperPoisonVoidzone>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var component = _aoe.Sources(Module).ToList();
            var count = component.Count;
            var forbidden = new Func<WPos, float>[count];
            for (var i = 0; i < count; ++i)
                forbidden[i] = ShapeDistance.Rect(component[i].Position, Module.PrimaryActor.Rotation, 40f, default, 6f);
            if (forbidden.Length != 0)
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(Casters[0].CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return false;
    }
}

class HeavingBreath(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.HeavingBreath), 35f, kind: Kind.DirForward, stopAtWall: true)
{
    private readonly ViperPoisonVoidzone _aoe = module.FindComponent<ViperPoisonVoidzone>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Casters.Count != 0 ? Casters[0] : null;
        if (source != null)
        {
            var component = _aoe.Sources(Module).ToList();
            var count = component.Count;
            var forbidden = new Func<WPos, float>[count];
            for (var i = 0; i < count; ++i)
                forbidden[i] = ShapeDistance.Rect(component[i].Position, new WDir(0f, 1f), 40f, 40f, 6f);
            if (forbidden.Length != 0)
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        foreach (var aoe in _aoe.ActiveAOEs(slot, actor))
        {
            if (aoe.Check(pos))
                return true;
        }
        return false;
    }
}

class Glossolalia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Glossolalia));
class Rend(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Rend));

class D033ErosStates : StateMachineBuilder
{
    public D033ErosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ViperPoisonBait>()
            .ActivateOnEnter<ViperPoisonVoidzone>()
            .ActivateOnEnter<Rend>()
            .ActivateOnEnter<HoundOutOfHeaven>()
            .ActivateOnEnter<Glossolalia>()
            .ActivateOnEnter<ConfessionOfFaithCenter>()
            .ActivateOnEnter<ConfessionOfFaithRight>()
            .ActivateOnEnter<ConfessionOfFaithLeft>()
            .ActivateOnEnter<ConfessionOfFaithSpread>()
            .ActivateOnEnter<ConfessionOfFaithStack>()
            .ActivateOnEnter<HeavingBreath>()
            .ActivateOnEnter<Inhale>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8233)]
public class D033Eros(WorldState ws, Actor primary) : BossModule(ws, primary, new(17, -538), new ArenaBoundsRect(14.5f, 19.5f));
