namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class Implosion(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone _shapeSmall = new(12f, 90f.Degrees()), _shapeLarge = new(90f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.FindComponent<GigaSlash>()?.AOEs.Count == 0 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.ImplosionLargeL or (uint)AID.ImplosionLargeR => _shapeLarge,
            (uint)AID.ImplosionSmallL or (uint)AID.ImplosionSmallR => _shapeSmall,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ImplosionLargeL or (uint)AID.ImplosionSmallR or (uint)AID.ImplosionLargeR or (uint)AID.ImplosionSmallL)
        {
            ++NumCasts;
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == caster.InstanceID)
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
