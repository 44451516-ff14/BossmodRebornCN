namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

sealed class MiasmaCounter(BossModule module) : BossComponent(module)
{
    private readonly LingeringMiasma _exa1 = module.FindComponent<LingeringMiasma>()!;
    private readonly SwirlingMiasma _exa2 = module.FindComponent<SwirlingMiasma>()!;
    private readonly CreepingMiasma _exa3 = module.FindComponent<CreepingMiasma>()!;
    public int NumLinesFinished => _exa1.NumLinesFinished + _exa2.NumLinesFinished + _exa3.NumCasts;
}

sealed class SwirlingMiasma(BossModule module) : Components.SimpleExaflare(module, new AOEShapeDonut(5f, 19f), (uint)AID.SwirlingMiasmaFirst, (uint)AID.SwirlingMiasmaRest, 6f, 1.6d, 8, 2, locationBased: true);
sealed class LingeringMiasma(BossModule module) : Components.SimpleExaflare(module, 8f, (uint)AID.LingeringMiasmaFirst, (uint)AID.LingeringMiasmaRest, 6f, 1.6d, 8, 2, locationBased: true);

sealed class CreepingMiasma(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect rect = new(50f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CreepingMiasmaFirst)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CreepingMiasmaFirst or (uint)AID.CreepingMiasmaRest)
        {
            var count = _aoes.Count;
            var pos = spell.LocXZ;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin == pos)
                {
                    if (++aoe.ActorID == 3ul)
                    {
                        _aoes.RemoveAt(i);
                        ++NumCasts;
                    }
                    break;
                }
            }
        }
    }
}
