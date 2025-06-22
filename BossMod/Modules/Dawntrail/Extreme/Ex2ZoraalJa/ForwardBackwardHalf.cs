namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class ForwardBackwardHalf(BossModule module) : Components.GenericAOEs(module, (uint)AID.HalfFullShortAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeEdge = new(50f, 30f, 10f);
    private static readonly AOEShapeRect _shapeSide = new(60f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (relevant, front, left) = spell.Action.ID switch
        {
            (uint)AID.ForwardHalfR or (uint)AID.ForwardHalfLongR => (true, true, false),
            (uint)AID.ForwardHalfL or (uint)AID.ForwardHalfLongL => (true, true, true),
            (uint)AID.BackwardHalfR or (uint)AID.BackwardHalfLongR => (true, false, false),
            (uint)AID.BackwardHalfL or (uint)AID.BackwardHalfLongL => (true, false, true),
            _ => default
        };
        if (!relevant)
            return;

        var cleaveDir = spell.Rotation + (front ? 180f : default).Degrees();
        var pos = WPos.ClampToGrid(caster.Position);
        var act = Module.CastFinishAt(spell);
        _aoes.Add(new(_shapeEdge, pos, cleaveDir, act));
        _aoes.Add(new(_shapeSide, pos, cleaveDir + (left ? 90f : -90f).Degrees(), act));
    }
}

sealed class HalfFull(BossModule module) : Components.GenericAOEs(module, (uint)AID.HalfFullLongAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeSide = new(60f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HalfFullR or (uint)AID.HalfFullL)
        {
            var cleaveDir = spell.Rotation + (spell.Action.ID == (uint)AID.HalfFullL ? 90f : -90f).Degrees();
            _aoes.Add(new(_shapeSide, spell.LocXZ, cleaveDir, Module.CastFinishAt(spell)));
        }
    }
}
