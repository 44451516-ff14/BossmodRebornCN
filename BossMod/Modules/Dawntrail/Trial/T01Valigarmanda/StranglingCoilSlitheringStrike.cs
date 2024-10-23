namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class StranglingCoilSusurrantBreath(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(8, 30);
    private static readonly AOEShapeCone cone = new(50, 40.Degrees());
    private readonly ArcaneLightning aoe = module.FindComponent<ArcaneLightning>()!;
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoe.AOEs.Count == 0 ? Utils.ZeroOrOne(_aoe) : ([]);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StranglingCoil)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell));
        else if ((AID)spell.Action.ID == AID.SusurrantBreath)
            _aoe = new(cone, new(100, 75), default, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.StranglingCoil or AID.SusurrantBreath)
            _aoe = null;
    }
}
