namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
abstract class LimitCut(BossModule module, double alphaDelay) : Components.GenericBaitAway(module)
{
    private enum State { Teleport, Alpha, Blasty }

    public int[] PlayerOrder = new int[PartyState.MaxPartySize];
    private readonly double _alphaDelay = alphaDelay;
    private State _nextState;
    private Actor? _chaser;
    private WPos _prevPos;
    private DateTime _nextHit;

    private static readonly AOEShapeCone _shapeAlpha = new(30f, 45f.Degrees());
    private static readonly AOEShapeRect _shapeBlasty = new(55f, 5f);

    public override void Update()
    {
        if (_nextState == State.Teleport && _chaser != null && _chaser.Position != _prevPos)
        {
            _nextState = State.Alpha;
            _prevPos = _chaser.Position;
            SetNextBaiter(NumCasts + 1, _shapeAlpha);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (PlayerOrder[slot] > 0)
            hints.Add($"Order: {PlayerOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (PlayerOrder[slot] > NumCasts)
        {
            var hitIn = Math.Max(default, (float)(_nextHit - WorldState.CurrentTime).TotalSeconds);
            var hitIndex = NumCasts + 1;
            while (PlayerOrder[slot] > hitIndex)
            {
                hitIn += (hitIndex & 1) != 0 ? 1.5f : 3.2f;
                ++hitIndex;
            }
            if (hitIn < 5)
            {
                var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionDefinitions.Surecast : ActionDefinitions.Armslength;
                hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.High, hitIn);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= 79u and <= 86u)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                PlayerOrder[slot] = (int)iconID - 78;

            if (_chaser == null)
            {
                // initialize baits on first icon; note that icons appear over ~300ms
                _chaser = ((TEA)Module).CruiseChaser();
                _prevPos = _chaser?.Position ?? default;
                _nextHit = WorldState.FutureTime(9.5d);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlphaSwordP2:
                ++NumCasts;
                _nextState = State.Blasty;
                SetNextBaiter(NumCasts + 1, _shapeBlasty);
                _nextHit = WorldState.FutureTime(1.5d);
                break;
            case (uint)AID.SuperBlasstyChargeP2:
            case (uint)AID.SuperBlasstyChargeP3:
                ++NumCasts;
                _nextState = State.Teleport;
                CurrentBaits.Clear();
                _nextHit = WorldState.FutureTime(_alphaDelay);
                break;

        }
    }

    private void SetNextBaiter(int order, AOEShape shape)
    {
        CurrentBaits.Clear();
        var target = Raid[Array.IndexOf(PlayerOrder, order)];
        if (_chaser != null && target != null)
            CurrentBaits.Add(new(_chaser, target, shape));
    }
}
