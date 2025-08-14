namespace BossMod.Global.MaskedCarnivale.Stage29.Act1;

public enum OID : uint
{
    Boss = 0x2C5B, //R=3.0
    FireTornado = 0x2C5C, // R=4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Unknown = 18872, // Boss->self, no cast, single-target
    FluidSwing = 18689, // Boss->self/player, 4.0s cast, range 11 30-degree cone, interruptible, knockback 50, source forward
    SeaOfFlamesVisual = 18693, // Boss->self, 3.0s cast, single-target
    SeaOfFlames = 18694, // Helper->location, 3.0s cast, range 6 circle
    Pyretic = 18691, // Boss->self, 4.0s cast, range 80 circle, applies pyretic
    FireII = 18692, // Boss->location, 4.0s cast, range 5 circle
    PillarOfFlameVisual1 = 18695, // Boss->self, 3.0s cast, single-target
    PillarOfFlame1 = 18696, // Helper->location, 3.0s cast, range 8 circle
    PillarOfFlameVisual2 = 18894, // Boss->self, 6.0s cast, single-target
    PillarOfFlame2 = 18895, // Helper->location, 6.0s cast, range 8 circle
    Rush = 18690, // Boss->player, 5.0s cast, width 4 rect charge, does distance based damage, seems to scale all the way until the other side of the arena
    FlareStarVisual = 18697, // Boss->self, 5.0s cast, single-target
    FlareStar = 18698, // Helper->location, 5.0s cast, range 40 circle, distance based AOE, radius 10 seems to be a good compromise
    FireBlast = 18699 // FireTornado->self, 3.0s cast, range 70+R width 4 rect
}

public enum SID : uint
{
    Pyretic = 960 // Boss->player, extra=0x0
}

sealed class FluidSwing(BossModule module) : Components.CastInterruptHint(module, (uint)AID.FluidSwing);
sealed class FluidSwingKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FluidSwing, 50f, kind: Kind.DirForward);
sealed class SeaOfFlames(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeaOfFlames, 6f);
sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, 5f);
sealed class PillarOfFlame(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PillarOfFlame1, (uint)AID.PillarOfFlame2], 8f);
sealed class Rush(BossModule module) : Components.CastHint(module, (uint)AID.Rush, "GTFO from boss! (Distance based charge)");
sealed class FlareStar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlareStar, 10f);
sealed class FireBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireBlast, new AOEShapeRect(74f, 2f));
sealed class PyreticHint(BossModule module) : Components.CastHint(module, (uint)AID.Pyretic, "Pyretic, stop everything! Dodge the AOE after it runs out.");

sealed class Pyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Pyretic)
        {
            Array.Fill(PlayerStates, new(Requirement.Stay, Module.CastFinishAt(spell), 1));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Pyretic)
        {
            Array.Clear(PlayerStates);
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"For this act Exuviation and Diamondback are mandatory.\nBringing Flying Sardine, lightning and wind spells is higly recommended.");
    }
}

sealed class Stage29Act1States : StateMachineBuilder
{
    public Stage29Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<FluidSwingKnockback>()
            .ActivateOnEnter<SeaOfFlames>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<PillarOfFlame>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<FireBlast>()
            .ActivateOnEnter<Pyretic>()
            .ActivateOnEnter<PyreticHint>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 698, NameID = 9239, SortOrder = 1)]
public sealed class Stage29Act1 : BossModule
{
    public Stage29Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
