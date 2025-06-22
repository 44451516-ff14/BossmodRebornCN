namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class SpinnersWheelSelect(BossModule module) : BossComponent(module)
{
    public enum Branch { None, Gaze, StayMove }

    public Branch SelectedBranch;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var branch = status.ID switch
        {
            (uint)SID.ArcaneAttraction or (uint)SID.AttractionReversed => Branch.Gaze,
            (uint)SID.ArcaneFever or (uint)SID.FeverReversed => Branch.StayMove,
            _ => Branch.None
        };
        if (branch != Branch.None)
            SelectedBranch = branch;
    }
}

abstract class SpinnersWheelGaze(BossModule module, bool inverted, uint aid, uint sid) : Components.GenericGaze(module, aid)
{
    private readonly Actor? _source = module.Enemies((uint)OID.Nymeia).FirstOrDefault();
    private DateTime _activation;
    private BitMask _affected;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source != null && _affected[slot])
            return new Eye[1] { new(_source.Position, _activation, Inverted: inverted) };
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == sid)
        {
            _activation = status.ExpireAt;
            _affected[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }
}
class SpinnersWheelArcaneAttraction(BossModule module) : SpinnersWheelGaze(module, false, (uint)AID.SpinnersWheelArcaneAttraction, (uint)SID.ArcaneAttraction);
class SpinnersWheelAttractionReversed(BossModule module) : SpinnersWheelGaze(module, true, (uint)AID.SpinnersWheelAttractionReversed, (uint)SID.AttractionReversed);

class SpinnersWheelStayMove(BossModule module) : Components.StayMove(module)
{
    public int ActiveDebuffs;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.ArcaneFever:
                if (Raid.FindSlot(actor.InstanceID) is var feverSlot && feverSlot >= 0)
                    PlayerStates[feverSlot] = new(Requirement.Stay, status.ExpireAt);
                break;
            case (uint)SID.FeverReversed:
                if (Raid.FindSlot(actor.InstanceID) is var revSlot && revSlot >= 0)
                    PlayerStates[revSlot] = new(Requirement.Move, status.ExpireAt);
                break;
            case (uint)SID.Pyretic:
            case (uint)SID.FreezingUp:
                ++ActiveDebuffs;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.Pyretic or (uint)SID.FreezingUp)
        {
            --ActiveDebuffs;
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                PlayerStates[slot] = default;
        }
    }
}
