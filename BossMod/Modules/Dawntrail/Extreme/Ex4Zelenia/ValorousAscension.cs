namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class ValorousAscension(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.ValorousAscension1,
(uint)AID.ValorousAscension2, (uint)AID.ValorousAscension3]);

sealed class ValorousAscensionRect(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 4f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count > 1 ? CollectionsMarshal.AsSpan(_aoes)[..2] : [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BriarThorn && id == 0x11DBu)
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(10.9d)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ValorousAscensionRect)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
