namespace BossMod.Dawntrail.Raid.M01NBlackCat;

class OneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private enum Pattern { None, WestEast, EastWest }
    private Pattern _currentPattern;
    private static readonly AOEShapeCone cone = new(60, 90.Degrees());
    private static readonly Angle[] angles = [Angle.AnglesCardinals[3], Angle.AnglesCardinals[0]];
    private Actor? helper;
    private static readonly HashSet<AID> oneTwoPaw = [AID.OneTwoPaw1, AID.OneTwoPaw2, AID.OneTwoPaw3, AID.OneTwoPaw4];
    private static readonly HashSet<AID> leapingVisuals = [AID.LeapingBlackCatCrossingVisual1, AID.LeapingBlackCatCrossingVisual2,
    AID.LeapingBlackCatCrossingVisual3, AID.LeapingBlackCatCrossingVisual4];
    private static readonly HashSet<AID> allOneTwoPaws = [.. oneTwoPaw, .. new HashSet<AID> { AID.LeapingOneTwoPaw1,
    AID.LeapingOneTwoPaw2, AID.LeapingOneTwoPaw3, AID.LeapingOneTwoPaw4}];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapingAttacks)
        {
            helper = actor;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (oneTwoPaw.Contains((AID)spell.Action.ID))
        {
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(x => x.Activation);
        }
        switch ((AID)spell.Action.ID)
        {
            case AID.LeapingOneTwoPawVisual1:
            case AID.LeapingOneTwoPawVisual3:
                _currentPattern = Pattern.EastWest;
                break;
            case AID.LeapingOneTwoPawVisual2:
            case AID.LeapingOneTwoPawVisual4:
                _currentPattern = Pattern.WestEast;
                break;
        }
        InitIfReady();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (leapingVisuals.Contains((AID)spell.Action.ID))
            helper = null;

        else if (_aoes.Count > 0 && allOneTwoPaws.Contains((AID)spell.Action.ID))
        {
            _currentPattern = Pattern.None;
            _aoes.RemoveAt(0);
        }
    }

    private void InitIfReady()
    {
        if (helper != null && _currentPattern != Pattern.None)
        {
            var angle = _currentPattern == Pattern.EastWest ? angles : angles.AsEnumerable().Reverse().ToArray();
            _aoes.Add(new(cone, helper.Position, angle[0], WorldState.FutureTime(8.7f)));
            _aoes.Add(new(cone, helper.Position, angle[1], WorldState.FutureTime(10.7f)));
            _currentPattern = Pattern.None;
            helper = null;
        }
    }
}
