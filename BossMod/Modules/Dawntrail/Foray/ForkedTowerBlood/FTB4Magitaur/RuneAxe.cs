namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class RuneAxeStatus(BossModule module) : BossComponent(module)
{
    private int numStatuses;
    public readonly List<(int Order, Actor Actor, DateTime expireAt)> StatusBig = [];
    public readonly List<(int Order, Actor Actor, DateTime expireAt)> StatusSmall = [];
    public int CurrentOrder;
    public BitMask IsTargetAny;
    public BitMask IsTargetSmall;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RuinousRuneBig or (uint)AID.RuinousRuneSmall)
        {
            var curOrder = 3;
            UpdateCurrentOrder(StatusSmall);
            UpdateCurrentOrder(StatusBig);

            void UpdateCurrentOrder(List<(int Order, Actor, DateTime)> list)
            {
                var count = list.Count;
                for (var i = 0; i < count; ++i)
                {
                    var order = list[i].Order;
                    if (curOrder > order)
                    {
                        curOrder = order;
                    }
                }
            }
            ++NumCasts;
            CurrentOrder = curOrder;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var expire = status.ExpireAt;
        var order = (expire - WorldState.CurrentTime).TotalSeconds switch
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
                IsTargetSmall[Raid.FindSlot(actor.InstanceID)] = true;
                break;
        }
        void AddTarget(List<(int, Actor, DateTime)> list)
        {
            list.Add((order, actor, expire));
            ++numStatuses;
            IsTargetAny[Raid.FindSlot(actor.InstanceID)] = true;
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
                IsTargetSmall[Raid.FindSlot(actor.InstanceID)] = false;
                break;
        }
        void RemoveStatus(List<(int, Actor, DateTime)> list)
        {
            var count = list.Count;
            for (var i = 0; i < count; ++i)
            {
                if (list[i].Item2 == actor)
                {
                    list.RemoveAt(i);
                    IsTargetAny[Raid.FindSlot(actor.InstanceID)] = false;
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
    private readonly List<AOEInstance>[] _aoeHintsForStatus = [new(1), new(1), new(1), new(1)];
    private readonly List<AOEInstance>[] _aoeHintsNoStatus = [new(1), new(1)];
    private bool prepare;
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

        if (_status.IsTargetAny[slot])
        {
            var playerOrder = 3;
            var isSmall = _status.IsTargetSmall[slot];
            if (isSmall)
            {
                CheckListForOrder(_status.StatusSmall);
            }
            else
            {
                CheckListForOrder(_status.StatusBig);
            }
            void CheckListForOrder(List<(int Order, Actor Actor, DateTime)> list)
            {
                var count = list.Count;
                for (var i = 0; i < count; ++i)
                {
                    var s = list[i];
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

            AddAOE(_aoeHintsForStatus[0], FTB4Magitaur.BigSpreadHint, center, actOrder1);
            AddAOE(_aoeHintsForStatus[2], FTB4Magitaur.BigSpreadHint, center, actOrder3);
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

sealed class RuneAxeSmallSpreadAOEs(BossModule module) : Components.GenericAOEs(module)
{
    private readonly RuneAxeStatus _status = module.FindComponent<RuneAxeStatus>()!;
    public bool Show = true;
    private static readonly AOEShapeRect square = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        if (_status.IsTargetSmall[slot])
        {
            var playerOrder = 3;
            var count = _status.StatusSmall.Count;
            DateTime act = default;

            for (var i = 0; i < count; ++i)
            {
                var s = _status.StatusSmall[i];
                if (s.Actor == actor)
                {
                    playerOrder = s.Order;
                    act = s.expireAt;
                    break;
                }
            }
            if (_status.CurrentOrder != playerOrder)
            {
                return [];
            }
            var playersWithSameOrder = new List<Actor>();
            for (var i = 0; i < count; ++i)
            {
                var s = _status.StatusSmall[i];
                if (s.Order == playerOrder)
                {
                    playersWithSameOrder.Add(s.Actor);
                }
            }

            var aoes = new List<AOEInstance>(3);
            var countP = playersWithSameOrder.Count;
            for (var i = 0; i < countP; ++i)
            {
                var a = playersWithSameOrder[i];
                if (a == actor)
                {
                    continue;
                }
                InSquare(square, a.Position, aoes, act);
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        else
        {
            var aoes = new List<AOEInstance>(3);
            var countP = _status.StatusSmall.Count;
            for (var i = 0; i < countP; ++i)
            {
                var a = _status.StatusSmall[i];
                if (a.Order != _status.CurrentOrder)
                {
                    continue;
                }
                InSquare(FTB4Magitaur.Square, a.Actor.Position, aoes, a.expireAt);
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        static void InSquare(AOEShape shape, WPos position, List<AOEInstance> list, DateTime activation)
        {
            for (var j = 0; j < 3; ++j)
            {
                if (position.InSquare(FTB4Magitaur.SquarePositions[j], 10f, FTB4Magitaur.SquareDirs[j]))
                {
                    list.Add(new(shape, FTB4Magitaur.SquarePositions[j], FTB4Magitaur.SquareAngles[j], activation));
                    break;
                }
            }
        }
    }
}
