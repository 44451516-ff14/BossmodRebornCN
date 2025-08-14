namespace BossMod.Stormblood.Ultimate.UCOB;

class P3SeventhUmbralEra(BossModule module) : Components.GenericKnockback(module, (uint)AID.SeventhUmbralEra)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(5.3d);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 11f, _activation, ignoreImmunes: true) };
    }
}

class P3CalamitousFlame(BossModule module) : Components.CastCounter(module, (uint)AID.CalamitousFlame);
class P3CalamitousBlaze(BossModule module) : Components.CastCounter(module, (uint)AID.CalamitousBlaze);
