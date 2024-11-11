namespace BossMod.Dawntrail.Raid.M01NBlackCat;

public class BlackCatCrossing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60, 22.5f.Degrees());
    private enum Pattern { None, Cardinals, Intercardinals }
    private Pattern _currentPattern;
    private Actor? helper;
    private static readonly HashSet<AID> leapingVisuals = [AID.LeapingOneTwoPawVisual1, AID.LeapingOneTwoPawVisual2,
    AID.LeapingOneTwoPawVisual3, AID.LeapingOneTwoPawVisual4];
    private static readonly HashSet<AID> castEnd = [AID.BlackCatCrossingFirst, AID.BlackCatCrossingRest,
    AID.LeapingBlackCatCrossingFirst, AID.LeapingBlackCatCrossingRest];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 3)
            for (var i = 0; i < 4; ++i)
                yield return _aoes[i] with { Color = Colors.Danger };
        if (count > 7)
            for (var i = 4; i < 8; ++i)
                yield return _aoes[i] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BlackCatCrossingFirst:
            case AID.BlackCatCrossingRest:
                _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.SortBy(x => x.Activation);
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.Extra != 0x307 && _currentPattern == Pattern.None)
        {
            switch ((SID)status.ID)
            {
                case SID.BlackCatCrossing1:
                    _currentPattern = Pattern.Cardinals;
                    break;
                case SID.BlackCatCrossing2:
                    _currentPattern = Pattern.Intercardinals;
                    break;
            }
            InitIfReady();
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LeapingAttacks)
        {
            helper = actor;
            InitIfReady();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (leapingVisuals.Contains((AID)spell.Action.ID))
            helper = null;
        else if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
        {
            _currentPattern = Pattern.None;
            _aoes.RemoveAt(0);
        }
    }

    private void InitIfReady()
    {
        if (helper != null && _currentPattern != Pattern.None)
        {
            AddAOEs(helper, _currentPattern == Pattern.Cardinals ? Angle.AnglesCardinals : Angle.AnglesIntercardinals, 9);
            AddAOEs(helper, _currentPattern == Pattern.Cardinals ? Angle.AnglesIntercardinals : Angle.AnglesCardinals, 11);
            _currentPattern = Pattern.None;
            helper = null;
        }
    }

    private void AddAOEs(Actor actor, Angle[] angles, int futureTime)
    {
        for (var i = 0; i < angles.Length; ++i)
            _aoes.Add(new(cone, actor.Position, angles[i], WorldState.FutureTime(futureTime)));
    }
}
