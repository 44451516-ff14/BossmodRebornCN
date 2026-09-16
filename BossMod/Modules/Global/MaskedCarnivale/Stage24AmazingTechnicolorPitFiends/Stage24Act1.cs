namespace BossMod.Global.MaskedCarnivale.Stage24.Act1;

public enum OID : uint
{
    Boss = 0x2735, //R=1.0
    ArenaViking = 0x2734 //R=1.0
}

public enum AID : uint
{
    AutoAttack = 6497, // ArenaViking->player, no cast, single-target

    Fire = 14266, // Boss->player, 1.0s cast, single-target
    Starstorm = 15317, // Boss->location, 3.0s cast, range 5 circle
    RagingAxe = 15316, // ArenaViking->self, 3.0s cast, range 4+R 90-degree cone
    LightningSpark = 15318 // Boss->player, 6.0s cast, single-target
}

sealed class Starstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Starstorm, 5f);
sealed class RagingAxe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RagingAxe, new AOEShapeCone(5f, 45f.Degrees()));
sealed class LightningSpark(BossModule module) : Components.CastInterruptHint(module, (uint)AID.LightningSpark);

sealed class Hints2(Stage24Act1 module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var primary = Module.PrimaryActor;
        if (!primary.IsDeadOrDestroyed)
        {
            hints.Add($"{primary.Name} is immune to magical damage!");
        }
        if (module.Viking is Actor viking && !viking.IsDeadOrDestroyed)
        {
            hints.Add($"{viking.Name} is immune to physical damage!");
        }
    }
}

sealed class Stage24Act1States : StateMachineBuilder
{
    public Stage24Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Starstorm>()
            .ActivateOnEnter<RagingAxe>()
            .ActivateOnEnter<LightningSpark>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage24Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 634u, NameID = 8127u, SortOrder = 1)]
public sealed class Stage24Act1 : BossModule
{
    public Stage24Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        _prePullHints =
        [
            $"The {PrimaryActor.Name} is immune to magic, the viking is immune to physical attacks.",
            "For the 2nd act Diamondback is highly recommended.",
            "For the 3rd act a ranged physical spell such as Fire Angon is highly recommended."
        ];
    }
    public static readonly uint[] Trash = [(uint)OID.ArenaViking, (uint)OID.Boss];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArenaViking));
    }

    public Actor? Viking;

    protected override void UpdatePreModuleActivation()
    {
        Viking ??= GetActor((uint)OID.ArenaViking);
    }

    // protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    // {
    //     for (var i = 0; i < hints.PotentialTargets.Count; ++i)
    //     {
    //         var e = hints.PotentialTargets[i];
    //         e.Priority = e.Actor.OID switch
    //         {
    //             (uint)OID.Boss or (uint)OID.ArenaViking => 0, // TODO: ideally Viking should only be attacked with magical abilities and Magus should only be attacked with physical abilities
    //             _ => 0
    //         };
    //     }
    // }

    private readonly string[] _prePullHints;

    public override string[] PrePullHints => _prePullHints;
}
