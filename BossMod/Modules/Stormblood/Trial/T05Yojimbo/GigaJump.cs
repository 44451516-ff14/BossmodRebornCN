namespace BossMod.Stormblood.Trial.T05Yojimbo;

class GigaJump(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Icon87, 0, 25f, 6f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GigaJump1 or (uint)AID.GigaJump2)
            Spreads.Clear();
    }
}
