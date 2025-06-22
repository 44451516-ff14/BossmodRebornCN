namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(30f, 3f);
    private readonly List<AOEInstance> _aoes = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 3 ? 3 : count;
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerrestrialTitans)
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 7f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Towerfall)
            _aoes.RemoveAt(0);
    }
}
