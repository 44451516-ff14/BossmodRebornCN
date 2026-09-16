namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class ChainCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private readonly AOEShapeRect rect = new(60f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonFirst)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoes.Add(new(rect, loc, rot, Module.CastFinishAt(spell), shapeDistance: rect.Distance(loc, rot), arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChainCannonFirst or (uint)AID.ChainCannonRepeat)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 0.1f))
                {
                    if (++aoe.ActorID == 7ul)
                    {
                        _aoes.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}
