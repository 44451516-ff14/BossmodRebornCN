namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

sealed class CrypticFlames(BossModule module) : BossComponent(module)
{
    public bool ReadyToBreak;
    private readonly int[] _playerOrder = new int[4];
    private readonly List<(Actor laser, int order)> _lasers = [];
    private int _numBrokenLasers;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var order = _playerOrder[slot];
        if (order != 0)
            hints.Add($"Break order: {order}", order == CurrentBreakOrder);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _lasers.Count;
        if (count == 0)
            return;
        var order = _playerOrder[pcSlot];
        for (var i = 0; i < count; ++i)
        {
            var l = _lasers[i];
            var dir = l.laser.Rotation.ToDirection();
            var extent = 2f * dir * dir.Dot(Arena.Center - l.laser.Position);
            var color = l.order != _playerOrder[pcSlot] ? Colors.Enemy : order == CurrentBreakOrder ? Colors.Safe : default;
            Arena.AddLine(l.laser.Position, l.laser.Position + extent, color, 2f);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.FirstBrand:
                SetPlayerOrder(actor, 1);
                break;
            case (uint)SID.SecondBrand:
                SetPlayerOrder(actor, 2);
                break;
            case (uint)SID.ThirdBrand:
                SetPlayerOrder(actor, 3);
                break;
            case (uint)SID.FourthBrand:
                SetPlayerOrder(actor, 4);
                break;
            case (uint)SID.FirstFlame:
            case (uint)SID.SecondFlame:
            case (uint)SID.ThirdFlame:
            case (uint)SID.FourthFlame:
                ReadyToBreak = true;
                break;
            case (uint)SID.Counter:
                var order = status.Extra switch
                {
                    0x1C1 => -1, // unbreakable
                    0x1C2 or 0x1C6 => 1,
                    0x1C3 or 0x1C7 => 2,
                    0x1C4 or 0x1C8 => 3,
                    0x1C5 or 0x1C9 => 4,
                    _ => 0
                };
                if (order != 0)
                    _lasers.Add((actor, order));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Counter)
        {
            _numBrokenLasers += _lasers.RemoveAll(l => l.laser == actor);
        }
    }

    private void SetPlayerOrder(Actor player, int order)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        if (slot >= 0 && slot < _playerOrder.Length)
            _playerOrder[slot] = order;
    }

    private int CurrentBreakOrder => _numBrokenLasers switch
    {
        < 4 => _numBrokenLasers + 1,
        < 8 => _numBrokenLasers - 4 + 1,
        _ => 0
    };
}
