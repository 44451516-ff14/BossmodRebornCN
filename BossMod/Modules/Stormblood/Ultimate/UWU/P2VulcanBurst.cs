namespace BossMod.Stormblood.Ultimate.UWU;

abstract class VulcanBurst(BossModule module, uint aid, Actor? source) : Components.GenericKnockback(module, aid)
{
    protected Actor? SourceActor = source;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (SourceActor != null)
            return new Knockback[1] { new(SourceActor.Position, 15f) }; // TODO: activation
        return [];
    }
}

sealed class P2VulcanBurst(UWU module) : VulcanBurst(module, (uint)AID.VulcanBurst, module.Ifrit());
sealed class P4VulcanBurst(UWU module) : VulcanBurst(module, (uint)AID.VulcanBurstUltima, module.Ultima());
