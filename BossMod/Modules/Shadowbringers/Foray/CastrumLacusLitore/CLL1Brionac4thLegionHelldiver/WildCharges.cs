namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class WildCharges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(6)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var halfWidth = spell.Action.ID switch
        {
            (uint)AID.LinearDive => 3f,
            (uint)AID.DiveFormation => 5f,
            _ => default
        };
        if (halfWidth != default)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), halfWidth, invertForbiddenZone: true), caster.Position.Quantized(), Angle.FromDirection(dir), Module.CastFinishAt(spell), Colors.SafeFromAOE, arenaProjectionLayer: 0, restrictToArenaProjectionLayer: true));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.DiveFormation or (uint)AID.LinearDive)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Module.ActorMatchesArenaProjectionLayer(actor, 0, true))
        {
            return;
        }
        var count = _aoes.Count;
        if (count != 0)
        {
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref _aoes.Ref(i);
                forbidden[i] = aoe.Shape.Distance(aoe.Origin, aoe.Rotation);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), _aoes.Ref(0).Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        if (!Module.ActorMatchesArenaProjectionLayer(actor, 0, true))
        {
            return;
        }

        var risky = true;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (aoe.Check(actor.Position))
            {
                risky = false;
                break;
            }
        }
        hints.Add("Share damage inside wildcharge!", risky);
    }
}
