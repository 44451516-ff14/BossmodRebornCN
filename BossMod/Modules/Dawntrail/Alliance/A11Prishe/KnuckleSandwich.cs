namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class KnuckleSandwich(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9f), new AOEShapeCircle(18f), new AOEShapeCircle(27f), new AOEShapeDonut(9f, 60f),
    new AOEShapeDonut(18f, 60f), new AOEShapeDonut(27f, 60f)];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // extra ai hint: stay close to the edge of the first aoe
        if (_aoes.Count == 2 && _aoes[1].Shape is AOEShapeDonut donut)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_aoes[0].Origin, donut.InnerRadius + 2f), _aoes[0].Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.KnuckleSandwich1 => _shapes[0],
            (uint)AID.KnuckleSandwich2 => _shapes[1],
            (uint)AID.KnuckleSandwich3 => _shapes[2],
            (uint)AID.BrittleImpact1 => _shapes[3],
            (uint)AID.BrittleImpact2 => _shapes[4],
            (uint)AID.BrittleImpact3 => _shapes[5],
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
            _aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.KnuckleSandwich1:
            case (uint)AID.KnuckleSandwich2:
            case (uint)AID.KnuckleSandwich3:
            case (uint)AID.BrittleImpact1:
            case (uint)AID.BrittleImpact2:
            case (uint)AID.BrittleImpact3:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}
