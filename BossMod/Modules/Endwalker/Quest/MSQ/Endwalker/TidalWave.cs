namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

// TODO: Make AI function for Destination Unsafe
sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWaveVisual, 25f, kind: Kind.DirForward, stopAtWall: true)
{
    private readonly Megaflare _megaflare = module.FindComponent<Megaflare>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_megaflare.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}
