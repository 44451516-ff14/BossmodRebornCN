namespace BossMod.Stormblood.Trial.T05Yojimbo;

class EpicStormsplitter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _centerRect = new(40f, 2f, 1f);
    private static readonly AOEShapeRect _sideRect = new(40f, 10f, 1f);

    private readonly List<AOEInstance> _aoes = [];
    private DateTime _stormsplitterCastStart;
    private DateTime _seasplitterCastEnd;
    private Actor? _caster;
    private Angle _lockedRotation;
    private WPos _lockedPosition;
    private bool _sideAOEsDisplayed;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EpicStormsplitter)
        {
            _caster = caster;
            _stormsplitterCastStart = WorldState.CurrentTime;
            _lockedRotation = caster.Rotation;
            _lockedPosition = caster.Position;

            _aoes.Add(new(_centerRect, _lockedPosition, _lockedRotation, _stormsplitterCastStart.AddSeconds(5d), Colors.Danger));
        }
        else if (spell.Action.ID == (uint)AID.Seasplitter1)
        {
            _seasplitterCastEnd = WorldState.CurrentTime.AddSeconds(spell.TotalTime);

            if (!_sideAOEsDisplayed)
            {
                AddSideAOEs();
                _sideAOEsDisplayed = true;
            }
        }
    }

    private void AddSideAOEs()
    {
        var offset = _centerRect.HalfWidth + _sideRect.HalfWidth - 1.5f;
        var perp = _lockedRotation.ToDirection().OrthoR();

        _aoes.Add(new(_sideRect, _lockedPosition - perp * offset, _lockedRotation, _seasplitterCastEnd));
        _aoes.Add(new(_sideRect, _lockedPosition + perp * offset, _lockedRotation, _seasplitterCastEnd));
    }

    private void UpdateSideAOEColors()
    {
        for (var i = 0; i < _aoes.Count; ++i)
        {
            if (_aoes[i].Shape == _sideRect)
            {
                var aoe = _aoes[i];
                aoe.Color = Colors.Danger;
                _aoes[i] = aoe;
            }
        }
    }

    public override void Update()
    {
        if (_caster == null)
            return;

        if (WorldState.CurrentTime >= _stormsplitterCastStart.AddSeconds(5d))
        {
            _aoes.RemoveAll(aoe => aoe.Shape == _centerRect);
            UpdateSideAOEColors();
        }

        if (_sideAOEsDisplayed && WorldState.CurrentTime >= _seasplitterCastEnd)
        {
            _aoes.RemoveAll(aoe => aoe.Shape == _sideRect);
            _sideAOEsDisplayed = false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EpicStormsplitter)
        {
            _aoes.RemoveAll(aoe => aoe.Shape == _centerRect);
        }
        else if (spell.Action.ID == (uint)AID.Seasplitter1)
        {
            _aoes.RemoveAll(aoe => aoe.Shape == _sideRect);
            _sideAOEsDisplayed = false;
        }
    }
}
