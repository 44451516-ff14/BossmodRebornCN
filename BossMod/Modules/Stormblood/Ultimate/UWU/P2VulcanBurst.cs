namespace BossMod.Stormblood.Ultimate.UWU;

class VulcanBurst(BossModule module, uint aid, Actor? source) : Components.GenericKnockback(module, aid)
{
    protected Actor? SourceActor = source;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (SourceActor != null)
            return new Knockback[1] { new(SourceActor.Position, 15f) }; // TODO: activation
        return [];
    }
}

class P2VulcanBurst(BossModule module) : VulcanBurst(module, (uint)AID.VulcanBurst, ((UWU)module).Ifrit());
class P4VulcanBurst(BossModule module) : VulcanBurst(module, (uint)AID.VulcanBurstUltima, ((UWU)module).Ultima());
