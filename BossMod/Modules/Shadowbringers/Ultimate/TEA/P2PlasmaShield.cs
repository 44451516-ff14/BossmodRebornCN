namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P2PlasmaShield(BossModule module) : Components.DirectionalParry(module, [(uint)OID.PlasmaShield], forbiddenPriority: AIHints.Enemy.PriorityInvincible)
{
    private readonly TEA bossmod = (TEA)module;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PlasmaShield)
            PredictParrySide(actor.InstanceID, Side.Left | Side.Right | Side.Back);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (id, _) in ActorStates)
        {
            var e = hints.Enemies.FirstOrDefault(d => d?.Actor.InstanceID == id);
            if (e == null)
                continue;

            // ranged, healers, and CC tank should prioritize shield, others should attack BJ
            if (actor.Class.GetRole() is Role.Ranged or Role.Healer || bossmod.CruiseChaser()?.TargetID == actor.InstanceID)
                e.Priority = 1;
        }

        // overwrite priority if player is not inside vuln angle
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        Arena.Actors(Module.Enemies((uint)OID.PlasmaShield));
    }
}

class P2CCInvincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
