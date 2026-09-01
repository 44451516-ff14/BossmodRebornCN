namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

[SkipLocalsInit]
sealed class CrossBlazeLoop(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(5f);
    private readonly AOEShapeDonut _donut = new(5f, 60f);
    private readonly AOEShapeCross _cross = new(35f, 5f);
    private (uint Head, ulong InstanceID, Actor Target) _nextPos; // actual boss OID, instanceID of boss helper, aoe origin

    public ReadOnlySpan<AOEInstance> ActiveCasters
    {
        get
        {
            var count = _aoes.Count;
            var max = count > 2 ? 2 : count;
            return CollectionsMarshal.AsSpan(_aoes)[..max];
        }
    }
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        aoe0.Color = count > 1 ? Colors.Danger : default;
        aoe0.Risky = true;
        return aoes[..max];
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tether)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target?.OID == (uint)OID.CrossBlazeTarget && source.OID is (uint)OID.GreenHead1 or (uint)OID.BlueHead1)
            {
                var head = source.OID == (uint)OID.GreenHead1 ? (uint)OID.GreenHead : (uint)OID.BlueHead;
                _nextPos = (head, source.InstanceID, target);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // actual AOE and visual indicators ID by order cast, not specific spell
        // either boss can start with crossblaze / blazeloop
        // either repeats the same donut/cross or switches
        // tether comes from boss helper 1 tick before boss does the cast indicating mechanic
        // use actual target actor; actor position at time of tether not at position of AOE

        if (spell.Action.ID is >= 47671 and <= 47678)
        {
            var activation = Module.CastFinishAt(spell);
            var position = _nextPos.Target.Position;

            if (spell.Action.ID % 2 == 0) //donut
            {
                _aoes.Add(new(_circle, position, default, activation, default, false, _nextPos.InstanceID, _circle.Distance(position, default)));
                _aoes.Add(new(_donut, position, default, activation.AddSeconds(2d), default, false, _nextPos.InstanceID, _donut.Distance(position, default)));
            }
            else // cross
            {
                _aoes.Add(new(_circle, position, default, activation, default, false, _nextPos.InstanceID, _circle.Distance(position, default)));
                _aoes.Add(new(_cross, position, default, activation.AddSeconds(2d), default, false, _nextPos.InstanceID, _cross.Distance(position, default)));
            }
        }
        else if (spell.Action.ID is (uint)AID.CrossblazeCast or (uint)AID.BlazeloopCast)
        {
            var activation = Module.CastFinishAt(spell);
            var position = _nextPos.Target.Position;
            AOEShape shape = spell.Action.ID == (uint)AID.CrossblazeCast ? _cross : _donut;

            _aoes.Add(new(_circle, position, default, activation, default, false, _nextPos.InstanceID, _circle.Distance(position, default)));
            _aoes.Add(new(shape, position, default, activation.AddSeconds(2d), default, false, _nextPos.InstanceID, shape.Distance(position, default)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.BlazeFirst:
                case (uint)AID.BlazeSecond:
                case (uint)AID.Blazeloop:
                case (uint)AID.Crossblaze:
                case (uint)AID.BlazeFollowup:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        /*
        // stay near initial circle if not during knockback
        if (_aoes.Count != 0)
        {
            var hissing = Module.FindComponent<HissingResonance>();
            ref var aoe = ref _aoes.Ref(0);
            if (hissing == null && aoe.Shape is AOEShapeCircle)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(aoe.Origin, 8f));
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
        */
        if (_aoes.Count != 0)
        {
            ref var aoe = ref _aoes.Ref(0);
            if (aoe.Shape is AOEShapeCircle)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(aoe.Origin, 10f));
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
