namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class Footprint(BossModule module) : Components.GenericKnockback(module, (uint)AID.Footprint)
{
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Module.PrimaryActor.Position, 20f) }; // TODO: activation
    }
}
