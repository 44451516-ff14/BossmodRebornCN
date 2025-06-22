namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class RuneAxeStatus(BossModule module) : BossComponent(module)
{
    private int numStatuses;
    public readonly List<(int Order, Actor Actor)> StatusBig = [];
    public readonly List<(int Order, Actor Actor)> StatusSmall = [];
    public int CurrentOrder;
    public BitMask IsTarget;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RuinousRuneBig or (uint)AID.RuinousRuneSmall)
        {
            var countStatusSmall = StatusSmall.Count;
            var curOrder = 3;
            for (var i = 0; i < countStatusSmall; ++i)
            {
                var order = StatusSmall[i].Order;
                if (curOrder > order)
                {
                    curOrder = order;
                }
            }
            var countStatusBig = StatusBig.Count;
            for (var i = 0; i < countStatusBig; ++i)
            {
                var order = StatusBig[i].Order;
                if (curOrder > order)
                {
                    curOrder = order;
                }
            }
            ++NumCasts;
            CurrentOrder = curOrder;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
        {
            < 10d => 0,
            < 15d => 1,
            _ => 2
        };
        switch (status.ID)
        {
            case (uint)SID.PreyGreaterAxebit:
                AddTarget(StatusBig);
                break;
            case (uint)SID.PreyLesserAxebit:
                AddTarget(StatusSmall);
                break;
        }
        void AddTarget(List<(int, Actor)> list)
        {
            list.Add((order, actor));
            ++numStatuses;
            IsTarget[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PreyGreaterAxebit:
                RemoveStatus(StatusBig);
                break;
            case (uint)SID.PreyLesserAxebit:
                RemoveStatus(StatusSmall);
                break;
        }
        void RemoveStatus(List<(int, Actor)> list)
        {
            var count = list.Count;
            for (var i = 0; i < count; ++i)
            {
                if (list[i].Item2 == actor)
                {
                    list.RemoveAt(i);
                    IsTarget[Raid.FindSlot(actor.InstanceID)] = false;
                    return;
                }
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (numStatuses > 7)
        {
            hints.Add($"Too many targets, mechanic potentially unsolveable!");
        }
    }
}

sealed class RuneAxeAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly RuneAxeStatus _status = module.FindComponent<RuneAxeStatus>()!;
    private AOEInstance? _aoePrepare;
    private readonly List<AOEInstance>[] _aoeHintsForStatus = [new(3), new(1), new(3), new(1)];
    private readonly List<AOEInstance>[] _aoeHintsNoStatus = [new(1), new(1)];
    private bool prepare;
    private static readonly AOEShapeRect square = new(15.5f, 15.5f, 15.5f);
    public bool Show = true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        if (prepare)
        {
            return Utils.ZeroOrOne(ref _aoePrepare);
        }

        if (_status.IsTarget[slot])
        {
            var countStatusSmall = _status.StatusSmall.Count;
            var playerOrder = 3;
            var isSmall = false;
            for (var i = 0; i < countStatusSmall; ++i)
            {
                var s = _status.StatusSmall[i];
                if (s.Actor == actor)
                {
                    playerOrder = s.Order;
                    isSmall = true;
                    break;
                }
            }
            if (!isSmall) // not found in first list, checking next
            {
                var countStatusBig = _status.StatusBig.Count;
                for (var i = 0; i < countStatusBig; ++i)
                {
                    var s = _status.StatusBig[i];
                    if (s.Actor == actor)
                    {
                        playerOrder = s.Order;
                        break;
                    }
                }
            }
            if (_status.CurrentOrder == playerOrder)
            {
                return CollectionsMarshal.AsSpan(_aoeHintsForStatus[playerOrder == 2 && isSmall ? 3 : playerOrder]);
            }
        }
        else
        {
            if (_status.CurrentOrder is 0 or 2)
            {
                return CollectionsMarshal.AsSpan(_aoeHintsNoStatus[_status.CurrentOrder == 0 ? 0 : 1]);
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuneAxe)
        {
            prepare = true;
            var act = Module.CastFinishAt(spell);
            var center = Arena.Center;
            _aoePrepare = new(FTB4Magitaur.CircleMinusSquares, center, default, act);
            var actOrder1 = act.AddSeconds(10.2d);
            var actOrder2 = act.AddSeconds(14.2d);
            var actOrder3 = act.AddSeconds(22.2d);
            for (var i = 0; i < 3; ++i)
            {
                var pos = FTB4Magitaur.SquarePositions[i];
                var rot = FTB4Magitaur.SquareAngles[i];
                AddAOE(_aoeHintsForStatus[0], square, pos, actOrder1, rot);
                AddAOE(_aoeHintsForStatus[2], square, pos, actOrder2, rot);
            }

            AddAOE(_aoeHintsNoStatus[0], FTB4Magitaur.CircleMinusSquares, center, actOrder1);
            AddAOE(_aoeHintsNoStatus[1], FTB4Magitaur.CircleMinusSquares, center, actOrder3);
            AddAOE(_aoeHintsForStatus[1], FTB4Magitaur.CircleMinusSquaresSpread, center, actOrder2);
            AddAOE(_aoeHintsForStatus[3], FTB4Magitaur.CircleMinusSquaresSpread, center, actOrder3);
            static void AddAOE(List<AOEInstance> list, AOEShape shape, WPos position, DateTime activation, Angle rotation = default) => list.Add(new(shape, position, rotation, activation));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.PreyGreaterAxebit or (uint)SID.PreyLesserAxebit)
        {
            prepare = false;
            _aoePrepare = null;
        }
    }
}
