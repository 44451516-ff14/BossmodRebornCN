namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

sealed class LeftArmMetalCutterAOE(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { FirstAOEs, SecondAOEs, Done }

    public State CurState;
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(40f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LeftArmMetalCutterAOE1)
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LeftArmMetalCutterAOE1:
                if (CurState == State.FirstAOEs)
                {
                    for (var i = 0; i < _aoes.Count; ++i)
                    {
                        var aoe = _aoes[i];
                        aoe.Rotation += 180f.Degrees();
                        aoe.Activation = WorldState.FutureTime(5.1d);
                        _aoes[i] = aoe;
                    }
                    CurState = State.SecondAOEs;
                }
                break;
            case (uint)AID.LeftArmMetalCutterAOE2:
                CurState = State.Done;
                _aoes.Clear();
                break;
        }
    }
}

class LeftArmMetalCutterKnockback(BossModule module, uint aid, float distance) : Components.GenericKnockback(module, aid)
{
    private readonly float _distance = distance;
    private Knockback? _instance;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _instance);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftArmMetalCutter or (uint)AID.ArmUnit)
            _instance = new(caster.Position, _distance, Module.CastFinishAt(spell, 0.6f));
    }
}
sealed class LeftArmMetalCutterKnockbackShort(BossModule module) : LeftArmMetalCutterKnockback(module, (uint)AID.LeftArmMetalCutterKnockbackShort, 5f);
sealed class LeftArmMetalCutterKnockbackLong(BossModule module) : LeftArmMetalCutterKnockback(module, (uint)AID.LeftArmMetalCutterKnockbackLong, 15f);
