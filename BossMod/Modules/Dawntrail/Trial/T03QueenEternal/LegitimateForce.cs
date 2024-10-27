namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 40);
    private static readonly HashSet<AID> castEnds = [AID.LegitimateForceLL, AID.LegitimateForceLR, AID.LegitimateForceRR, AID.LegitimateForceRL,
    AID.LegitimateForceR, AID.LegitimateForceL];
    private static readonly WDir offset1 = new(0, 20);
    private static readonly WDir offset2 = new(0, 8);
    private readonly Besiegement _aoe = module.FindComponent<Besiegement>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.AOEs.Count == 0)
        {
            var count = _aoes.Count;
            var compare = count > 1 && _aoes[0].Rotation != _aoes[1].Rotation;
            for (var i = 0; i < count; i++)
            {
                if (i == 0)
                    yield return compare ? _aoes[i] with { Color = Colors.Danger } : _aoes[i];
                else if (i == 1 && compare)
                    yield return _aoes[i] with { Risky = false };
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LegitimateForceLL:
                AddAOEs(caster, spell, -90, -90);
                break;
            case AID.LegitimateForceLR:
                AddAOEs(caster, spell, -90, 90);
                break;
            case AID.LegitimateForceRR:
                AddAOEs(caster, spell, 90, 90);
                break;
            case AID.LegitimateForceRL:
                AddAOEs(caster, spell, 90, -90);
                break;
        }
    }

    private void AddAOEs(Actor caster, ActorCastInfo spell, float first, float second)
    {
        _aoes.Add(new(rect, caster.Position, spell.Rotation + first.Degrees(), Module.CastFinishAt(spell)));
        _aoes.Add(new(rect, caster.Position, spell.Rotation + second.Degrees(), Module.CastFinishAt(spell, 3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnds.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var besiege = _aoe.AOEs;
        var count = _aoes.Count;
        var besiegeCount = besiege.Count;
        var gravityBounds = Arena.Bounds == T03QueenEternal.SplitGravityBounds;
        var leftCenter = T03QueenEternal.LeftSplitCenter;
        var rightCenter = T03QueenEternal.RightSplitCenter;
        if (count > 0 && Arena.Center != T03QueenEternal.SplitArena.Center || besiegeCount == 0 && count == 2 && gravityBounds)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + offset1, Arena.Center - offset1, 3), _aoes[0].Activation);
        else if (count != 2 && besiegeCount == 0 && gravityBounds)
        {
            var forbidden = new List<Func<WPos, float>>
            {
            ShapeDistance.InvertedRect(leftCenter + offset2, leftCenter - offset2, 4),
            ShapeDistance.InvertedRect(rightCenter + offset2, rightCenter - offset2, 4)
            };
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Max(), count > 0 ? _aoes[0].Activation : besiegeCount > 0 ? besiege[0].Activation : default);
        }
    }
}
