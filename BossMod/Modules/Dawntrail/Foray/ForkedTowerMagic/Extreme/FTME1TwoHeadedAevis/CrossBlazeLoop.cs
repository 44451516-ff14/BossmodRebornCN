namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

sealed class CrossBlazeLoop(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(4)];
    private readonly AOEShapeCircle _circle = new(5f);
    private readonly AOEShapeDonut _donut = new(5f, 60f);
    private readonly AOEShapeCross _cross = new(35f, 5f);
    private WPos? _nextPos = null;
    private AOEShape? _secondShape = null;
    private ulong? _casterId = null;
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        //ref var aoe = ref aoes[0];
        //aoe.Color = count > 1 ? Colors.Danger : default;
        return aoes[..max];
    }

    private void InitIfReady()
    {
        if (_nextPos is WPos nextPos && _secondShape is AOEShape shape && _casterId is ulong actorID)
        {
            var loc = nextPos.Quantized();
            var rot = Angle.AnglesCardinals[1];
            _nextPos = null;
            _secondShape = null;
            _casterId = null;
            AddAOE(_circle);
            AddAOE(shape, 2d);

            void AddAOE(AOEShape shape, double delay = 0d)
            {
                var is1st = delay == 0d;
                AOEs.Add(new(shape, loc, rot, is1st ? _activation : _activation.AddSeconds(2d), is1st ? Colors.Danger : default, actorID: actorID, shapeDistance: shape.Distance(loc, rot)));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // actual AOE and visual indicators ID by order cast, not specific spell
        // either boss can start with crossblaze / blazeloop
        // either repeats the same donut/cross or switches
        // tether target may not have moved after both tethered and cast started depending on latency
        // cache 2nd shape and base position on cast location of 1st, 2nd, 3rd/4th blaze
        if (spell.Action.ID is var id && (id is >= (uint)AID.CrossblazeAndRepeat1 and <= (uint)AID.BlazeloopCrossblaze2 || id is >= (uint)AID.CrossblazeCast and <= (uint)AID.BlazeloopCast))
        {
            _casterId = caster.InstanceID;
            _activation = Module.CastFinishAt(spell);

            if ((id & 1) == 0) // donut
            {
                _secondShape = _donut;
            }
            else // cross
            {
                _secondShape = _cross;
            }
            InitIfReady();
        }
        else if (id is (uint)AID.BlazeFirst or (uint)AID.BlazeSecond or (uint)AID.BlazeFollowup)
        {
            _nextPos = spell.LocXZ;
            _activation = Module.CastFinishAt(spell);
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = AOEs.Count;
        if (count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.BlazeFirst:
                case (uint)AID.BlazeSecond:
                case (uint)AID.Blazeloop:
                case (uint)AID.Crossblaze:
                case (uint)AID.BlazeFollowup:
                    ++NumCasts;
                    AOEs.RemoveAt(0);
                    if (count >= 2)
                    {
                        var aoes = CollectionsMarshal.AsSpan(AOEs);
                        //aoes[0].Color = default;
                        aoes[0].Color = Colors.Danger;
                        if (count >= 3)
                        {
                            //aoes[1].Color = Colors.Danger;
                            aoes[1].Color = default;
                        }
                    }
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AOEs.Count != 0)
        {
            ref var aoe = ref AOEs.Ref(0);
            if (aoe.Shape is AOEShapeCircle)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(aoe.Origin, 10f));
            }
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}
