namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NorthSouthwind(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;
    private readonly Stormcall _aoe = module.FindComponent<Stormcall>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindVisual)
            _kb = new(spell.LocXZ, 40f, Module.CastFinishAt(spell, 0.1f), Direction: spell.Rotation, Kind: Kind.DirForward);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NorthWind:
            case (uint)AID.SouthWind:
                _kb = null;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var dirKB = kb.Direction;
                var dir = new WDir(default, 1f);
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center - 20f * dirKB.ToDirection(), dir, 4f, 4f, 24f), act);
                var aoes = _aoe.AOEs;
                if (aoes.Count != 0)
                    hints.AddForbiddenZone(ShapeDistance.Rect(aoes[0].Origin, dir, 99f, 99f, 35f), act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.AOEs;
        return aoes.Count != 0 && aoes[0].Check(pos) || !Module.InBounds(pos);
    }
}
