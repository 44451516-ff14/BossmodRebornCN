namespace BossMod.RealmReborn.Dungeon.D27SastashaHard.D271Karlabos;

public enum OID : uint
{
    Boss = 0xCBA,
    Tail = 0xD08
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Impale = 3054, // Boss->player, no cast, single-target
    MarkOfDeath = 3055, // Boss->player, no cast, single-target
    Inspire = 3056, // Boss->player, no cast, single-target
    StunClaws = 3057, // Boss->player, no cast, single-target
    Expire = 3058, // Boss->self, no cast, single-target
    AquaBall = 3059, // D08->player, no cast, range 5 circle
    TailScrew = 3060, // Boss->player, 3.0s cast, single-target
    WildInstinct = 3061 // Boss->self, no cast, single-target
}

public enum SID : uint
{
    Slime = 569, // Tail->player, extra=0x0
    Prey = 420 // Boss->player, extra=0x0
}

class MarkOfDeath(BossModule module) : Components.SingleTargetInstant(module, (uint)AID.MarkOfDeath)
{
    private BitMask _marked;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
        {
            _marked[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Prey)
        {
            _marked[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // Tanks should pass the stun to a DPS so they can stun TailScrew later
        if (_marked[slot] && actor.Role == Role.Tank)
        {
            hints.Add("Run through a DPS!");
        }
    }
}

// Cast is vulnerable to stun, but not interrupt
class TailScrew(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TailScrew, false, true);
class AquaBall(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Slime, "Slime", "slimed");

class D271KarlabosStates : StateMachineBuilder
{
    public D271KarlabosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<TailScrew>()
            .ActivateOnEnter<MarkOfDeath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Zaventh", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 28, NameID = 3014)]
public class D271Karlabos(WorldState ws, Actor primary) : BossModule(ws, primary, new(166f, -85f), new ArenaBoundsCircle(21f));
