namespace BossMod.Shadowbringers.Ultimate.TEA;

// note: boss moves to position around the component activation time
class P3Inception3Sacrament(BossModule module) : Components.GenericAOEs(module, (uint)AID.SacramentInception)
{
    public bool Active => _source != null;

    private readonly Actor? _source = ((TEA)module).AlexPrime();
    private readonly DateTime _activation = module.WorldState.FutureTime(4.1d);
    private static readonly AOEShapeCross _shape = new(100f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            return new AOEInstance[1] { new(_shape, _source.Position, _source.Rotation, _activation) };
        return [];
    }
}

class P3Inception3Debuffs(BossModule module) : Components.GenericStackSpread(module)
{
    private Actor? _sharedSentence;
    private BitMask _avoid;
    private BitMask _tethered;
    private bool _inited; // we init stuff on first update, since component is activated when statuses are already applied

    public override void Update()
    {
        if (!_inited)
        {
            _inited = true;
            if (_sharedSentence != null)
                Stacks.Add(new(_sharedSentence, 4f, 3, int.MaxValue, WorldState.FutureTime(4d), _avoid));
        }
        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_tethered[slot] && FindPartner(slot) is var partner && partner != null && (partner.Position - actor.Position).LengthSq() < 900f)
            hints.Add("Stay farther from partner!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (_tethered[pcSlot] && FindPartner(pcSlot) is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, (partner.Position - pc.Position).LengthSq() < 900f ? 0 : Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AggravatedAssault:
                _avoid[Raid.FindSlot(actor.InstanceID)] = true;
                break;
            case (uint)SID.SharedSentence:
                _sharedSentence = actor;
                break;
            case (uint)SID.RestrainingOrder:
                _tethered[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var remaining = _tethered;
        remaining[slot] = false;
        return remaining.Any() ? Raid[remaining.LowestSetBit()] : null;
    }
}
