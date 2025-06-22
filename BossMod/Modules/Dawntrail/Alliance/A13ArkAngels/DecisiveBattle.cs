namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class DecisiveBattle(BossModule module) : BossComponent(module)
{
    public readonly Actor?[] AssignedBoss = new Actor?[PartyState.MaxAllianceSize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var assignedSlot = ref AssignedBoss[slot];
        if (slot < PartyState.MaxAllianceSize && assignedSlot != null)
        {
            var target = WorldState.Actors.Find(actor.TargetID);
            if (target != null && target != assignedSlot && target.OID is (uint)OID.BossMR or (uint)OID.BossTT or (uint)OID.BossGK)
                hints.Add($"Target {assignedSlot?.Name}!");
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.DecisiveBattle && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0)
        {
            AssignedBoss[slot] = WorldState.Actors.Find(tether.Target);
        }
    }

    // fall back since players outside arena bounds do not get tethered but will still receive status effects
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var boss = status.ID switch
        {
            (uint)SID.EpicHero => (uint)OID.BossMR,
            (uint)SID.VauntedHero => (uint)OID.BossTT,
            (uint)SID.FatedHero => (uint)OID.BossGK,
            _ => default
        };
        if (boss != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            AssignedBoss[slot] = Module.Enemies(boss)[0];
    }

    // if player joins fight late, statemachine won't reset this component properly
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.EpicVillain)
            Array.Clear(AssignedBoss);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ref var assignedSlot = ref AssignedBoss[slot];
        if (slot < AssignedBoss.Length && assignedSlot != null)
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
