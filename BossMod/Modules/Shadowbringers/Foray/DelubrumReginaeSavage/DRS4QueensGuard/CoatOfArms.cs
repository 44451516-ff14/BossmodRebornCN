namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class CoatOfArms(BossModule module) : Components.DirectionalParry(module, [(uint)OID.AetherialWard])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var sides = spell.Action.ID switch
        {
            (uint)AID.CoatOfArmsFB => Side.Front | Side.Back,
            (uint)AID.CoatOfArmsLR => Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(caster.InstanceID, sides);
    }
}
