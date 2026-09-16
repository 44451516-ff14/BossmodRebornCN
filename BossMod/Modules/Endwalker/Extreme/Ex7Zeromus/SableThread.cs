namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

sealed class SableThread(BossModule module) : Components.GenericWildCharge(module, 6f, (uint)AID.SableThreadAOE, 60f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.SableThreadTarget)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true, true, true))
                PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
}
