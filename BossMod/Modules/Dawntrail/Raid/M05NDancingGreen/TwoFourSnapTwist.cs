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
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = _checkerboard.AOEs.Count != 0 || _aoe.Casters.Count != 0 ? 1 : count;
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
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
                AddAOE(180f.Degrees(), 3.5d);
                break;
        }
        void AddAOE(Angle offset = default, double delay = default) => _aoes.Add(new(rect, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay), risky: false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
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
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 2)
        {
            return;
        }
        // make ai stay close to boss to ensure successfully dodging the combo
        hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, new WDir(1f, default), 2f, 2f, 40f), _aoes.Ref(0).Activation);
    }
}
