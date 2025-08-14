namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: consider using envcontrols instead
sealed class UnnaturalEnchainment(BossModule module) : Components.GenericAOEs(module, (uint)AID.Sample)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(5f, 10f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UnnaturalEnchainment)
            _aoes.Add(new(_shape, source.Position.Quantized(), default, WorldState.FutureTime(8.2d)));
    }
}
