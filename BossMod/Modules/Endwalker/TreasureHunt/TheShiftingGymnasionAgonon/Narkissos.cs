namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.Narkissos;

public enum OID : uint
{
    Boss = 0x3D48, //R=8.0
    GymnasiouLampas = 0x3D4D, //R=2.001, bonus loot adds
    GymnasiouLyssa = 0x3D4E, //R=3.75, bonus loot adds
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GymnasiouLyssa->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target

    FetchingFulgence = 32332, // Boss->self, 4.0s cast, range 40 circle, gaze, vegetal vapors
    PotentPerfume = 32333, // BossHelper->location, 4.0s cast, range 8 circle, high damage, vegetal vapours
    Lash = 32330, // Boss->player, 5.0s cast, single-target, tankbuster
    SapShowerVisual = 32335, // Boss->self, no cast, single-target
    SapShower = 32336, // BossHelper->location, 6.5s cast, range 8 circle, high damage, vegetal vapours
    ExtensibleTendrils = 32339, // Boss->self, 5.0s cast, range 25 width 6 cross

    RockHard = 32340, // BossHelper->player, 5.5s cast, range 6 circle
    HeavySmash = 32317, // 3D4E->location, 3.0s cast, range 6 circle
    BeguilingGas = 32331, // Boss->self, 5.0s cast, range 40 circle, temporary misdirection
    Brainstorm = 32334, // Boss->self, 5.0s cast, range 40 circle, forced march debuffs
    PutridBreath = 32338 // Boss->self, 4.0s cast, range 25 90-degree cone
}

public enum SID : uint
{
    VegetalVapours = 3467, // Boss/Helper->player, extra=0x2162 (description: Overcome and quite unable to act.)
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x2/0x4/0x8
    RightFace = 1961, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959, // Boss->player, extra=0x0
    LeftFace = 1960 // Boss->player, extra=0x0
}

class Brainstorm(BossModule module) : Components.StatusDrivenForcedMarch(module, 2f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly SapShower _aoe = module.FindComponent<SapShower>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.Casters.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}

class FetchingFulgence(BossModule module) : Components.CastGaze(module, (uint)AID.FetchingFulgence);
class Lash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Lash);
class PotentPerfume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PotentPerfume, 8f);

class SapShowerTendrilsHint(BossModule module) : BossComponent(module)
{
    private int NumCasts;
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SapShower)
        {
            active = true;
            ++NumCasts;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SapShower)
            active = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (active)
        {
            if (NumCasts is <= 4 and > 0)
                hints.Add("Circles resolve before cross");
            else if (NumCasts > 4)
                hints.Add("Circles resolve before cross, aim forced march into cross");
        }
    }
}

class SapShower : Components.SimpleAOEs
{
    public SapShower(BossModule module) : base(module, (uint)AID.SapShower, 8f)
    {
        Color = Colors.Danger;
    }
}

class ExtensibleTendrils(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExtensibleTendrils, new AOEShapeCross(25f, 3f));
class PutridBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PutridBreath, new AOEShapeCone(25f, 45f.Degrees()));
class RockHard(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RockHard, 6f);
class BeguilingGasTM(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.BeguilingGas);
class BeguilingGas(BossModule module) : Components.RaidwideCast(module, (uint)AID.BeguilingGas);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class NarkissosStates : StateMachineBuilder
{
    public NarkissosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SapShower>()
            .ActivateOnEnter<Brainstorm>()
            .ActivateOnEnter<Lash>()
            .ActivateOnEnter<FetchingFulgence>()
            .ActivateOnEnter<PotentPerfume>()
            .ActivateOnEnter<SapShowerTendrilsHint>()
            .ActivateOnEnter<ExtensibleTendrils>()
            .ActivateOnEnter<PutridBreath>()
            .ActivateOnEnter<RockHard>()
            .ActivateOnEnter<BeguilingGas>()
            .ActivateOnEnter<BeguilingGasTM>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Narkissos.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12029)]
public class Narkissos(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasiouLampas, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GymnasiouLampas => 2,
                (uint)OID.GymnasiouLyssa => 1,
                _ => 0
            };
        }
    }
}
