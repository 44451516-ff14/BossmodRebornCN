namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: add sludge voidzones?..
class P3Gaols(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { None, TargetSelection, Fetters, Done }
    private readonly UWUConfig _config = Service.Config.Get<UWUConfig>();

    public State CurState;
    private BitMask _targets;

    private static readonly AOEShapeCircle _freefireShape = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurState != State.Fetters || _targets[slot])
            return [];

        var includedTargets = Raid.WithSlot(true, true, true).IncludedInMask(_targets);
        var count = 0;

        foreach (var (_, target) in includedTargets)
            ++count;

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        foreach (var (_, target) in includedTargets)
            aoes[index++] = new(_freefireShape, target.Position);

        return aoes;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurState == State.TargetSelection && _targets.Any())
        {
            var hintBuilder = new StringBuilder();
            var priorities = _config.P3GaolPriorities.Resolve(Raid);
            foreach (var i in priorities)
            {
                if (_targets[i.slot])
                {
                    var name = Raid[i.slot]?.Name;
                    if (name != null)
                    {
                        if (hintBuilder.Length > 0)
                        {
                            hintBuilder.Append(" > ");
                        }
                        hintBuilder.Append(name);
                    }
                }
            }
            hints.Add($"Gaols: {hintBuilder}");
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Fetters)
        {
            if (CurState == State.TargetSelection)
            {
                CurState = State.Fetters;
                _targets = default; // note that if target dies, fetters is applied to random player
            }
            _targets[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.RockThrowBoss:
            case (uint)AID.RockThrowHelper:
                CurState = State.TargetSelection;
                _targets.Set(Raid.FindSlot(spell.MainTargetID));
                break;
            case (uint)AID.FreefireGaol:
                var (closestSlot, closestPlayer) = Raid.WithSlot(true, true, true).IncludedInMask(_targets).Closest(caster.Position);
                if (closestPlayer != null)
                {
                    _targets.Clear(closestSlot);
                    if (_targets.None())
                        CurState = State.Done;
                }
                break;
        }
    }
}
