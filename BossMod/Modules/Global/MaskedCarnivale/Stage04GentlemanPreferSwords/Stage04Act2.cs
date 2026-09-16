namespace BossMod.Global.MaskedCarnivale.Stage04.Act2;

public enum OID : uint
{
    Boss = 0x25D5, //R=2.5
    Beetle = 0x25D6 //R=0.6
}

public enum AID : uint
{
    AutoAttack1 = 6497, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // Beetle->player, no cast, single-target

    GrandStrike = 14366, // Boss->self, 1.5s cast, range 75+R width 2 rect
    MagitekField = 14369, // Boss->self, 5.0s cast, single-target
    Spoil = 14362, // Beetle->self, no cast, range 6+R circle
    MagitekRay = 14368 // Boss->location, 3.0s cast, range 6 circle
}

sealed class GrandStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandStrike, new AOEShapeRect(77.5f, 2f));
sealed class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, 6f);
sealed class MagitekField(BossModule module) : Components.CastInterruptHint(module, (uint)AID.MagitekField);

sealed class Stage04Act2States : StateMachineBuilder
{
    public Stage04Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekField>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 614U, NameID = 8087U, SortOrder = 2)]
public sealed class Stage04Act2 : BossModule
{
    public Stage04Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        var name = PrimaryActor.Name;
        _prePullHints =
        [
            $"{name} is weak to lightning spells. During the fight he will spawn 6 beetles. If available you CAN use the Ram's Voice + Ultravibration combo for the instant kill.",
            $"{name} is weak against lightning spells and can be frozen."
        ];
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Beetle));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Beetle => 1,
                _ => 0
            };
        }
    }

    private readonly string[] _prePullHints;

    public override string[] PrePullHints => _prePullHints;
}
