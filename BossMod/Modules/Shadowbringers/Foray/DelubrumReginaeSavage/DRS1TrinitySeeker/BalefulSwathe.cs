namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class BalefulSwathe(BossModule module) : Components.GenericAOEs(module, (uint)AID.BalefulSwathe)
{
    private readonly DateTime _activation = module.WorldState.FutureTime(7.6d); // from verdant path cast start
    private static readonly AOEShapeRect _shape = new(50f, 50f, -5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new AOEInstance[2];
        aoes[0] = new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 90f.Degrees(), _activation);
        aoes[1] = new(_shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation - 90f.Degrees(), _activation);
        return aoes;
    }
}
