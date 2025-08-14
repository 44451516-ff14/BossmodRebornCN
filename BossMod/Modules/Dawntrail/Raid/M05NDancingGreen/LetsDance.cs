namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rect = new(25f, 45f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 2 ? 2 : count;
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = true;
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
        }
        return aoes[..max];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes[0].Activation.AddSeconds(count * 2d) : WorldState.FutureTime(18.2d);
            _aoes.Add(new(rect, Arena.Center.Quantized(), modelState == 5 ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0], act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.LetsDance)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count < 2)
        {
            return;
        }
        // make ai stay close to boss to ensure successfully dodging the combo
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, new WDir(1f, default), 2f, 2f, 40f), _aoes.Ref(0).Activation);
    }
}
