namespace BossMod.Endwalker.Extreme.Ex6Golbez;

// TODO: improve/generalize
sealed class EventideFallTriad(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, Parties, Roles }

    private Mechanic _curMechanic;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_curMechanic != Mechanic.None)
            hints.Add($"Stack by: {_curMechanic}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = spell.Action.ID switch
        {
            (uint)AID.EventideFall => Mechanic.Parties,
            (uint)AID.EventideTriad => Mechanic.Roles,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
            _curMechanic = mechanic;
    }
}
