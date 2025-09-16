namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class StrikingSmiting(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle circleSmall = new(10f), circleBig = new(30f);
    private Actor? leftArm;
    private Actor? rightArm;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var id = actor.OID;
        if (id == (uint)OID.SculptedArm2)
        {
            leftArm = actor;
        }
        else if (id == (uint)OID.SculptedArm1)
        {
            rightArm = actor;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 2)
        {
            return;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.StrikingLeft1:
            case (uint)AID.SmitingLeftSequence:
                AddAOEs(leftArm);
                break;
            case (uint)AID.StrikingRight1:
            case (uint)AID.SmitingRightSequence:
                AddAOEs(rightArm);
                break;
            case (uint)AID.StrikingLeftTelegraph:
                AddAOEs(leftArm, 7d);
                break;
            case (uint)AID.StrikingRightTelegraph:
                AddAOEs(rightArm, 7d);
                break;
        }
        void AddAOEs(Actor? arm, double delayFirst = default)
        {
            AddAOE(circleSmall, null, delayFirst);
            AddAOE(circleBig, arm, 5.1d);
            void AddAOE(AOEShape shape, Actor? actor = null, double delay = default)
            => _aoes.Add(new(shape, actor == null ? spell.LocXZ : actor.Position.Quantized(), default, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.StrikingLeft1 or (uint)AID.StrikingLeft2 or (uint)AID.StrikingRight1
        or (uint)AID.StrikingRight2 or (uint)AID.SmitingLeft1 or (uint)AID.SmitingLeft2 or (uint)AID.SmitingRight1 or (uint)AID.SmitingRight2
        or (uint)AID.SmitingLeftSequence or (uint)AID.SmitingRightSequence)
        {
            _aoes.RemoveAt(0);
        }
    }
}
