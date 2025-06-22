namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class TwoFourSnapTwist(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(20f, 20f);
    private readonly FunkyFloor _checkerboard = module.FindComponent<FunkyFloor>()!;
    private readonly Moonburn _aoe = module.FindComponent<Moonburn>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = _checkerboard.AOEs.Count != 0 || _aoe.Casters.Count != 0 ? 1 : count;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (max > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistFirst1:
            case (uint)AID.TwoSnapTwistFirst2:
            case (uint)AID.TwoSnapTwistFirst3:
            case (uint)AID.TwoSnapTwistFirst4:
            case (uint)AID.TwoSnapTwistFirst5:
            case (uint)AID.TwoSnapTwistFirst6:
            case (uint)AID.TwoSnapTwistFirst7:
            case (uint)AID.TwoSnapTwistFirst8:
            case (uint)AID.FourSnapTwistFirst1:
            case (uint)AID.FourSnapTwistFirst2:
            case (uint)AID.FourSnapTwistFirst3:
            case (uint)AID.FourSnapTwistFirst4:
            case (uint)AID.FourSnapTwistFirst5:
            case (uint)AID.FourSnapTwistFirst6:
            case (uint)AID.FourSnapTwistFirst7:
            case (uint)AID.FourSnapTwistFirst8:
                AddAOE();
                AddAOE(180f.Degrees(), 3.5f);
                break;
        }
        void AddAOE(Angle offset = default, float delay = default) => _aoes.Add(new(rect, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.TwoSnapTwist2:
                case (uint)AID.TwoSnapTwist3:
                case (uint)AID.FourSnapTwist4:
                case (uint)AID.FourSnapTwist5:
                    _aoes.RemoveAt(0);
                    break;
            }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 2)
            return;
        // make ai stay close to boss to ensure successfully dodging the combo
        var aoe = _aoes[0];
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, aoe.Rotation, 2f, 2f, 40f), aoe.Activation);
    }
}
