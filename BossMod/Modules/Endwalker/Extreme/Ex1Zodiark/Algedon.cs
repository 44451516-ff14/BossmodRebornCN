namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to algedon mechanic
sealed class Algedon(BossModule module) : BossComponent(module)
{
    private Actor? _caster;

    private readonly AOEShapeRect _shape = new(60f, 15f);

    public bool Done => _caster == null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_shape.Check(actor.Position, _caster))
            hints.Add("GTFO from diagonal aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _shape.Draw(Arena, _caster);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AlgedonAOE)
            _caster = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AlgedonAOE)
            _caster = null;
    }
}
