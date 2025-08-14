namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class SeasonsOfTheFleeting(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(70f, 22.5f.Degrees());
    private static readonly AOEShapeRect rect = new(46f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(16);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 8 ? 8 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (max > 4)
        {
            for (var i = 0; i < 4; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Color = Colors.Danger;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 7.4d)));
        switch (spell.Action.ID)
        {
            case (uint)AID.FireAndWaterTelegraph:
                AddAOE(rect);
                break;
            case (uint)AID.EarthAndLightningTelegraph:
                AddAOE(cone);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.SeasonOfFire:
                case (uint)AID.SeasonOfWater:
                case (uint)AID.SeasonOfLightning:
                case (uint)AID.SeasonOfEarth:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}
