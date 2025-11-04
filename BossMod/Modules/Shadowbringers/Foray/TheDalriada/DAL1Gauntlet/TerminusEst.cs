namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    public static readonly AOEShapeRect Rect = new(50f, 4f);
    public readonly List<Actor> Casters = new(10);
    public Angle CommonAngle;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
        {
            Casters.Add(actor);
            if (Casters.Count == 10)
            {
                // find the most common angle for use in the unseen mechanic, add the rest to aoes
                var angleA = Casters[0].Rotation;
                var angleB = (Angle)default;
                var countA = 1;
                var countB = 0;
                var count = Casters.Count;
                for (var i = 1; i < count; ++i)
                {
                    var angle = Casters[i].Rotation;
                    if (angleA == angle)
                    {
                        ++countA;
                    }
                    else if (countB == 0)
                    {
                        angleB = angle;
                        countB = 1;
                    }
                    else if (angleB == angle)
                    {
                        ++countB;
                    }
                }

                CommonAngle = countA > countB ? angleA : angleB;
                var leastCommon = countA > countB ? angleB : angleA;

                var act = WorldState.FutureTime(10d);
                for (var i = 0; i < count; ++i)
                {
                    var c = Casters[i];
                    var rot = c.Rotation;
                    if (rot == leastCommon)
                    {
                        _aoes.Add(new(Rect, c.Position.Quantized(), rot, act));
                    }
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEst)
        {
            _aoes.Clear();
        }
    }
}

sealed class TerminusEstUnseen(BossModule module) : Components.CastWeakpoint(module, (uint)AID.TerminusEst, TerminusEst.Rect, default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen)
{
    private readonly TerminusEst _aoe = module.FindComponent<TerminusEst>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        base.OnStatusGain(actor, ref status);
        if (_casters.Count == 0 && status.ID is (uint)SID.BackUnseen or (uint)SID.LeftUnseen or (uint)SID.RightUnseen)
        {
            var angle = _aoe.CommonAngle;
            var count = _aoe.Casters.Count;
            fallbackTime = 6.5f;
            for (var i = 0; i < count; ++i)
            {
                var c = _aoe.Casters[i];
                if (c.Rotation == angle)
                {
                    _casters.Add(_aoe.Casters[i]);
                }
            }
        }
    }
}
