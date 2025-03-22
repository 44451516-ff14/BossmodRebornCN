namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotStrikeJump(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJump), 8f);
class ByregotStrikeJumpCone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ByregotStrikeJumpCone), 8f);
class ByregotStrikeKnockback(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.ByregotStrikeKnockback), 18f)
{
    private static readonly Angle a45 = 45f.Degrees(), a180 = 180f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var act = Module.CastFinishAt(source.CastInfo);
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(source.Position, 14f, source.Rotation + a180, a45), act);
        }
    }
}

class ByregotStrikeCone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone _shape = new(90f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeKnockback && Module.PrimaryActor.FindStatus((uint)SID.Glow) != null)
            for (var i = 0; i < 4; ++i)
                _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation + i * 90f.Degrees(), Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ByregotStrikeCone)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
