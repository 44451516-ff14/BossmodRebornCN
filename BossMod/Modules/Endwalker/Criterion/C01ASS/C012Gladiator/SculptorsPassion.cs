namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

abstract class SculptorsPassion(BossModule module, uint aid) : Components.GenericWildCharge(module, 4f, aid)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.SculptorsPassionTargetSelection)
        {
            Source = Module.PrimaryActor;
            foreach (var (slot, player) in Raid.WithSlot(true, true, true))
                PlayerRoles[slot] = player.InstanceID == spell.MainTargetID ? PlayerRole.Target : player.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
}
sealed class NSculptorsPassion(BossModule module) : SculptorsPassion(module, (uint)AID.NSculptorsPassion);
sealed class SSculptorsPassion(BossModule module) : SculptorsPassion(module, (uint)AID.SSculptorsPassion);
