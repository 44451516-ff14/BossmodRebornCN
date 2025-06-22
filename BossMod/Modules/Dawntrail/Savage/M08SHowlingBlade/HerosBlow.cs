namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class HerosBlow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(15f, 30f);
    private static readonly AOEShapeCircle circle = new(22f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.HerosBlow1 or (uint)AID.HerosBlow2 => cone,
            (uint)AID.FangedPerimeter => donut,
            (uint)AID.FangedMaw => circle,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HerosBlow1 or (uint)AID.HerosBlow2)
        {
            ++NumCasts;
        }
    }
}
