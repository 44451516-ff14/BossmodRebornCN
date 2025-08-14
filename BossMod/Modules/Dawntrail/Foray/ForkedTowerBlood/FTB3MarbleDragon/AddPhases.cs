namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class VulnerabilityDown(BossModule module) : Components.Dispel(module, (uint)SID.VulnerabilityDown);
sealed class DamageUp(BossModule module) : Components.CastHint(module, (uint)AID.FrozenHeart, "Add enrage!");
sealed class IceGolems(BossModule module) : Components.Adds(module, (uint)OID.IceGolem, 1);
sealed class IceSprite(BossModule module) : Components.Adds(module, (uint)OID.IceSprite)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus((uint)SID.Invincibility) != null)
            {
                e.Priority = AIHints.Enemy.PriorityInvincible;
            }
            else
            {
                e.Priority = 1;
            }
        }
    }
}

sealed class LifelessLegacy(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.LifelessLegacyVisual, (uint)AID.LifelessLegacy, 1.8f)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var castInfo = Module.PrimaryActor.CastInfo;
        if (castInfo is ActorCastInfo info && info.RemainingTime < 6f)
            hints.Add(Hint);
    }
}
