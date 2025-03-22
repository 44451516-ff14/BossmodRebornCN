namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D023Anima;

public enum OID : uint
{
    Boss = 0x33FD, // R=18.7
    LowerAnima = 0x3400, // R=18.7
    IronNail = 0x3401, // R=1.0
    LunarNail = 0x33FE, // R=1.0
    MegaGraviton = 0x33FF, // R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 25341, // Boss->player, no cast, single-target

    AetherialPull = 25345, // MegaGraviton->player, 8.0s cast, single-target, pull 30 between centers

    BoundlessPainPull = 26229, // Helper->self, no cast, range 60 circle, pull 60 between centers
    BoundlessPainVisual = 25347, // Boss->self, 8.0s cast, single-target, creates expanding AOE
    BoundlessPainFirst = 25348, // Helper->location, no cast, range 6 circle
    BoundlessPainRest = 25349, // Helper->location, no cast, range 6 circle

    CharnelClaw = 25357, // IronNail->self, 6.0s cast, range 40 width 5 rect

    CoffinScratchFirst = 25358, // Helper->location, 3.5s cast, range 3 circle
    CoffinScratchRest = 21239, // Helper->location, no cast, range 3 circle

    Imperatum = 25353, // Boss->self, 5.0s cast, range 60 circle, phase change
    ImperatumPull = 23929, // Helper->player, no cast, single-target, pull 60 between centers

    LunarNail = 25342, // Boss->self, 3.0s cast, single-target

    ObliviatingClaw1 = 25354, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClaw2 = 25355, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClawSpawnAOE = 25356, // IronNail->self, 6.0s cast, range 3 circle

    OblivionVisual = 25359, // LowerAnima->self, 6.0s cast, single-target
    OblivionStart = 23697, // Helper->location, no cast, range 60 circle
    OblivionLast = 23872, // Helper->location, no cast, range 60 circle

    MegaGraviton = 25344, // Boss->self, 5.0s cast, range 60 circle, tether mechanic
    GravitonSpark = 25346, // MegaGraviton->player, no cast, single-target, on touching the graviton

    PaterPatriaeVisual = 25350, // Boss->self, 3.5s cast, single-target
    PaterPatriaeAOE = 24168, // Helper->self, 3.5s cast, range 60 width 8 rect

    PhantomPainVisual = 21182, // Boss->self, 7.0s cast, single-target
    PhantomPain = 25343, // Helper->self, 7.0s cast, range 20 width 20 rect

    LowerAnimaVisual = 27228, // LowerAnima->self, no cast, single-target

    EruptingPainVisual = 25351, // Boss->self, 5.0s cast, single-target
    EruptingPain = 25352 // Helper->player, 5.0s cast, range 6 circle
}

public enum TetherID : uint
{
    AetherialPullGood = 17 // MegaGraviton->player
}

public enum IconID : uint
{
    ChasingAOE = 197 // player
}

class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x03)
        {
            if (state == 0x00020001)
                Arena.Center = D023Anima.LowerArenaCenter;
            else if (state == 0x00080004)
                Arena.Center = D023Anima.UpperArenaCenter;
        }
    }
}

class BoundlessPain(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(18f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BoundlessPainPull:
                _aoe = new(circle, Arena.Center);
                break;
            case (uint)AID.BoundlessPainFirst:
            case (uint)AID.BoundlessPainRest:
                if (++NumCasts == 20)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveAOEs(slot, actor).Length != 0)
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, Arena.Center + new WDir(default, 20f), 20f));
    }
}

class Gravitons(BossModule module) : Components.Voidzone(module, 1f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MegaGraviton);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class AetherialPull(BossModule module) : Components.StretchTetherDuo(module, 33f, 7.9f, tetherIDGood: (uint)TetherID.AetherialPullGood, knockbackImmunity: true);
class CoffinScratch(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(3f), ActionID.MakeSpell(AID.CoffinScratchFirst), ActionID.MakeSpell(AID.CoffinScratchRest), 6f, 1, 5, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(18.5f, default), Arena.Center + new WDir(-18.5f, default), 20f), Activation);
        else if (IsChaserTarget(actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(actor.Position, new WDir(1f, default), 40f, 40f, 3f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.OblivionVisual)
        {
            Chasers.Clear();
        }
    }
}

class PhantomPain(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PhantomPain), new AOEShapeRect(20f, 10f));
class PaterPatriaeAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PaterPatriaeAOE), new AOEShapeRect(60f, 4f));
class CharnelClaw(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CharnelClaw), new AOEShapeRect(40f, 2.5f), 5);
class ErruptingPain(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EruptingPain), 6f);
class ObliviatingClawSpawnAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ObliviatingClawSpawnAOE), 3f);
class Oblivion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OblivionVisual), "Raidwide x16");
class MegaGraviton(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MegaGraviton));

class D023AnimaStates : StateMachineBuilder
{
    public D023AnimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Gravitons>()
            .ActivateOnEnter<BoundlessPain>()
            .ActivateOnEnter<CoffinScratch>()
            .ActivateOnEnter<PhantomPain>()
            .ActivateOnEnter<AetherialPull>()
            .ActivateOnEnter<PaterPatriaeAOE>()
            .ActivateOnEnter<CharnelClaw>()
            .ActivateOnEnter<ErruptingPain>()
            .ActivateOnEnter<ObliviatingClawSpawnAOE>()
            .ActivateOnEnter<Oblivion>()
            .ActivateOnEnter<MegaGraviton>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10285)]
public class D023Anima(WorldState ws, Actor primary) : BossModule(ws, primary, UpperArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos UpperArenaCenter = new(default, -180f);
    public static readonly WPos LowerArenaCenter = new(default, -400f);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LowerAnima));
    }
}
