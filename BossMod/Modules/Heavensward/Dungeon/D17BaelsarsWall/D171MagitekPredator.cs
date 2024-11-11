namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D171MagitekPredator;

public enum OID : uint
{
    Boss = 0x1938, // R2.94
    SkyArmorReinforcement = 0x1939, // R2.0
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    MagitekClaw = 7346, // Boss->player, 4.0s cast, single-target, tankbuster
    MagitekHookMarker = 7349, // SkyArmorReinforcement->player, no cast, single-target
    MagitekHook = 7350, // Helper->player, no cast, single-target
    MagitekRay = 7347, // Boss->self, 3.0s cast, range 40+R width 6 rect
    MagitekMissile = 7348 // Boss->player, no cast, single-target
}

public enum SID : uint
{
    Prey = 562, // none->player, extra=0x0
    DamageUp = 290 // none->SkyArmorReinforcement/Helper, extra=0x0
}

class MagitekRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekRay), new AOEShapeRect(42.94f, 3));
class MagitekClaw(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.MagitekClaw));
class MagitekMissile(BossModule module) : Components.SingleTargetInstant(module, ActionID.MakeSpell(AID.MagitekMissile), 5, "50% HP damage on prey targets")
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Prey)
            Targets.Add((Raid.FindSlot(actor.InstanceID), WorldState.FutureTime(5)));
    }
}

class D171MagitekPredatorStates : StateMachineBuilder
{
    public D171MagitekPredatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<MagitekClaw>()
            .ActivateOnEnter<MagitekMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5564)]
public class D171MagitekPredator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-174, 73), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.SkyArmorReinforcement).Concat([PrimaryActor]));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.SkyArmorReinforcement => 1,
                _ => 0
            };
        }
    }
}
