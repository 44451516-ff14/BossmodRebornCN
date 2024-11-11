namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class SeasonsOfTheFleeting(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(70, 22.5f.Degrees());
    private static readonly AOEShapeRect rect = new(46, 2.5f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly HashSet<AID> castEnd = [AID.SeasonOfEarth, AID.SeasonOfWater, AID.SeasonOfFire, AID.SeasonOfLightning];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var aoeCount = Math.Clamp(count, 0, 4);
            for (var i = aoeCount; i < Math.Clamp(count, 0, 8); ++i)
                yield return _aoes[i];
            for (var i = 0; i < aoeCount; ++i)
                yield return _aoes[i] with { Color = Colors.Danger };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireAndWaterTelegraph:
                _aoes.Add(new(rect, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 7.4f)));
                break;
            case AID.EarthAndLightningTelegraph:
                _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 7.4f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}
