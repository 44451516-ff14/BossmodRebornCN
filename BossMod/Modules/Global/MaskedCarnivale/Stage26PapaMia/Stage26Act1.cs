namespace BossMod.Global.MaskedCarnivale.Stage26.Act1;

public enum OID : uint
{
    Boss = 0x2C84, //R=2.55
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6498, // Boss->player, no cast, single-target

    AlternatePlumage = 18686, // Boss->self, 3.0s cast, single-target, armor up, needs dispel
    RuffledFeathers = 18685, // Boss->player, no cast, single-target
    Gust = 18687, // Boss->location, 2.5s cast, range 3 circle
    CaberToss = 18688 // Boss->player, 5.0s cast, single-target, interrupt or wipe
}

public enum SID : uint
{
    VulnerabilityDown = 63, // Boss->Boss, extra=0x0
    Windburn = 269 // Boss->player, extra=0x0
}

sealed class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 3f);
sealed class AlternatePlumage(BossModule module) : Components.CastHint(module, (uint)AID.AlternatePlumage, "Prepare to dispel buff");
sealed class CaberToss(BossModule module) : Components.CastInterruptHint(module, (uint)AID.CaberToss);

sealed class Hints(BossModule module) : BossComponent(module)
{
    private bool isShielded;
    private bool isWindburned;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.VulnerabilityDown:
                isShielded = true;
                break;
            case (uint)SID.Windburn:
                isWindburned = true;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.VulnerabilityDown:
                isShielded = false;
                break;
            case (uint)SID.Windburn:
                isWindburned = false;
                break;
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (isShielded)
        {
            hints.Add($"Dispel {Module.PrimaryActor.Name} with Eerie Soundwave!");
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (isWindburned)
        {
            hints.Add("Windburn on you! Cleanse it with Exuviation.");
        }
    }
}

sealed class Stage26Act1States : StateMachineBuilder
{
    public Stage26Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CaberToss>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AlternatePlumage>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 695u, NameID = 9230u, SortOrder = 1)]
public sealed class Stage26Act1 : BossModule
{
    public Stage26Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        _prePullHints =
        [
            $"{PrimaryActor.Name} will cast Alternate Plumage, which makes him almost immune to damage.",
            "Use Eerie Soundwave to dispel it. Caber Toss must be interrupted or you will wipe.",
            "Additionally Exuviation and earth spells are recommended for act 2."
        ];
    }

    private readonly string[] _prePullHints;

    public override string[] PrePullHints => _prePullHints;
}
