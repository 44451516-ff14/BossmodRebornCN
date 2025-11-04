namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class ShieldProtocol(BossModule module) : BossComponent(module)
{
    public readonly Actor?[] AssignedBoss = new Actor?[PartyState.MaxAllianceSize];
    private readonly A22SuperiorFlightUnits bossmod = (A22SuperiorFlightUnits)module;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var assignedSlot = ref AssignedBoss[slot];
        if (slot < PartyState.MaxAllianceSize && assignedSlot != null && WorldState.Actors.Find(actor.TargetID) is Actor target)
        {
            if (target != assignedSlot && target.OID is (uint)OID.FlightUnitALpha or (uint)OID.FlightUnitBEta or (uint)OID.FlightUnitCHi)
            {
                hints.Add($"Target {assignedSlot?.Name}!");
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ShieldProtocol && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
        }
    }

    // fall back since players outside arena bounds do not get tethered but will still receive status effects
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var boss = status.ID switch
        {
            (uint)SID.ShieldProtocolA => Module.PrimaryActor,
            (uint)SID.ShieldProtocolB => bossmod.BossBeta,
            (uint)SID.ShieldProtocolC => bossmod.BossChi,
            _ => null
        };
        if (boss != null && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = boss;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.ShieldProtocolA or (uint)SID.ShieldProtocolB or (uint)SID.ShieldProtocolC && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ref var assignedSlot = ref AssignedBoss[slot];
        if (slot < PartyState.MaxAllianceSize && assignedSlot != null)
        {
            var count = hints.PotentialTargets.Count;
            for (var i = 0; i < count; ++i)
            {
                var enemy = hints.PotentialTargets[i];
                if (enemy.Actor != assignedSlot)
                    enemy.Priority = AIHints.Enemy.PriorityInvincible;
            }
        }
    }
}
