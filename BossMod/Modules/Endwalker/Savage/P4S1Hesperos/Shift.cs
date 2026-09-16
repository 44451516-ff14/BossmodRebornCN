namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to shift mechanics
sealed class Shift(BossModule module) : BossComponent(module)
{
    private readonly AOEShapeCone _swordAOE = new(50f, 60f.Degrees());
    private Actor? _swordCaster;
    private Actor? _cloakCaster;

    private const float _knockbackRange = 30f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_swordAOE.Check(actor.Position, _swordCaster))
        {
            hints.Add("GTFO from sword!");
        }
        else if (_cloakCaster != null && !Arena.InBounds(Components.GenericKnockback.AwayFromSource(actor.Position, _cloakCaster, _knockbackRange)))
        {
            hints.Add("About to be knocked into wall!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _swordAOE.Draw(Arena, _swordCaster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_cloakCaster != null)
        {
            Arena.ZoneCircleOutline(_cloakCaster.Position, 5, Colors.Safe);

            var adjPos = Components.GenericKnockback.AwayFromSource(pc.Position, _cloakCaster, _knockbackRange);
            if (adjPos != pc.Position)
            {
                Arena.AddLine(pc.Position, adjPos, Colors.Danger);
                Arena.Actor(adjPos, pc.Rotation, Colors.Danger);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShiftingStrikeCloak:
                _cloakCaster = caster;
                break;
            case (uint)AID.ShiftingStrikeSword:
                _swordCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShiftingStrikeCloak:
                _cloakCaster = null;
                break;
            case (uint)AID.ShiftingStrikeSword:
                _swordCaster = null;
                break;
        }
    }
}
