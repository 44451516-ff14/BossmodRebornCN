namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class ChainCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(60f, 2.5f);
    private readonly DetermineArena _arena = module.FindComponent<DetermineArena>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsBrionacArena)
            return CollectionsMarshal.AsSpan(_aoes);
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainCannonFirst)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
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
                    if (++aoe.ActorID == 7u)
                    {
                        _aoes.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}
