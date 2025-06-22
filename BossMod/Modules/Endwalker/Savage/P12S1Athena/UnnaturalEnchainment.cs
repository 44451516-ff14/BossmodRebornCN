namespace BossMod.Endwalker.Savage.P12S1Athena;

// TODO: consider using envcontrols instead
class UnnaturalEnchainment(BossModule module) : Components.GenericAOEs(module, (uint)AID.Sample)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(5, 10, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UnnaturalEnchainment)
            _aoes.Add(new(_shape, WPos.ClampToGrid(source.Position), default, WorldState.FutureTime(8.2d)));
    }
}
