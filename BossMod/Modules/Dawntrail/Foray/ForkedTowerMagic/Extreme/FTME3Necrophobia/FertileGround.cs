namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

[SkipLocalsInit]
sealed class FertileGround(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(30f, 40f);
    private BitMask _panic = new();
    private BitMask _dread = new();
    private readonly List<WPos> _heads = [];
    private readonly List<(Angle Rotation, ushort Extra)> _angles = [];
    private BitMask _playerUpdated = new();
    private int _headNum = 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        if (!_dread[slot] && !_panic[slot])
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = aoes.Length;
        var start = _headNum - (_playerUpdated[slot] || actor.IsDead ? 0 : 1);
        start = (start < 0 ? 0 : start) * 2;
        if (count >= start + 2)
        {
            start += _dread[slot] ? 0 : 1;
            var end = start + 1;
            return aoes[start..end];
        }

        return [];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.GrowingDread or (uint)SID.GrowingPanic)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            switch (status.ID)
            {
                case (uint)SID.GrowingDread:
                    _dread.Set(slot);
                    _panic.Clear(slot);
                    break;
                case (uint)SID.GrowingPanic:
                    _dread.Clear(slot);
                    _panic.Set(slot);
                    break;
            }
            // status gained at slightly different times per player
            // track separately so no blips in radar
            _playerUpdated.Set(slot);
        }
        else if (actor.OID == (uint)OID.SeveringHead && status.ID == (uint)SID.Element)
        {
            // heads get status with extra 0x45D/E, which half casts dread/panic
            // heads move to cardinal/intercardinal
            // heads cast in move order?
            var position = actor.Position;
            var extra = status.Extra;
            var rotation = (Arena.Center - position).ToAngle();
            var initialDelay = 13.7d;
            var extraDelay = 4.7d * _heads.Count;
            var activation = WorldState.CurrentTime.AddSeconds(initialDelay + extraDelay);

            _heads.Add(position);

            var angles = Math.Abs(Arena.Center.X - position.X) < 1f || Math.Abs(Arena.Center.Z - position.Z) < 1f ? Angle.AnglesCardinals : Angle.AnglesIntercardinals;
            var angle = 0f.Degrees();
            for (var i = 0; i < 4; i++)
            {
                var card = angles[i];
                if (rotation.AlmostEqual(card, 0.1f))
                {
                    _angles.Add((card, extra));
                    angle = card;
                    break;
                }
            }

            var dread = angle + 90f.Degrees() + (extra == 0x45D ? 0f.Degrees() : 180f.Degrees());
            _aoes.Add(new(_rect, Arena.Center, dread, activation));
            _aoes.Add(new(_rect, Arena.Center, dread + 180f.Degrees(), activation));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        // status lost .1-.2s before panic/dread resolves
        if (actor.OID == (uint)OID.SeveringHead && status.ID == (uint)SID.Element)
        {
            _headNum++;
            ResetPlayerUpdated();
        }
    }

    private void ResetPlayerUpdated()
    {
        _playerUpdated.Reset();

        // set dead players so AOE marker doesn't lag behind on revive
        var players = Raid.WithSlot();
        var count = players.Length;
        for (var i = 0; i < count; i++)
        {
            var player = players[i].Item2;
            if (player.IsDead)
            {
                _playerUpdated.Set(players[i].Item1);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // 48-man, dread and panic have x2 EventCast; is it x1 if less than 24 players?
        // dread1 resolve before panic1, panic2 resolve before dread2? best not to rely on this
        switch (spell.Action.ID)
        {
            case (uint)AID.SowingDread1:
            case (uint)AID.SowingDread2:
            case (uint)AID.SowingPanic1:
            case (uint)AID.SowingPanic2:
                ++NumCasts;
                break;
        }
    }

#if DEBUG
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var count = _heads.Count;
        for (var i = 0; i < count; i++)
        {
            ref var head = ref _heads.Ref(i);
            Arena.ZoneCircle(head, 1.5f, 0x99FFFF00);
            Arena.TextWorld(head, $"{i + 1}", 0xFF000000);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_dread[slot])
        {
            hints.Add("Dread", false);
        }
        else if (_panic[slot])
        {
            hints.Add("Panic", false);
        }
    }
#endif
}
