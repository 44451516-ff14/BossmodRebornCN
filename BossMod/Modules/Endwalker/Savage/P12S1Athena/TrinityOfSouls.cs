namespace BossMod.Endwalker.Savage.P12S1Athena;

sealed class TrinityOfSouls(BossModule module) : Components.GenericAOEs(module)
{
    private bool _invertMiddle;
    private uint _moves; // bit 0 - move after first, bit1 - move after second
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count == 3 || NumCasts > 0)
        {
            var hint = _moves switch
            {
                0 => "stay",
                1 => "move after first",
                2 => "move before last",
                _ => "move twice"
            };
            hints.Add($"Trinity: {hint}");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var invert = spell.Action.ID switch
        {
            (uint)AID.TrinityOfSoulsDirectTR => false,
            (uint)AID.TrinityOfSoulsDirectTL => false,
            (uint)AID.TrinityOfSoulsInvertBR => true,
            (uint)AID.TrinityOfSoulsInvertBL => true,
            _ => (bool?)null
        };
        if (invert != null)
        {
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _invertMiddle = invert.Value;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TrinityOfSoulsDirectTR or (uint)AID.TrinityOfSoulsDirectTL or
                (uint)AID.TrinityOfSoulsDirectMR or (uint)AID.TrinityOfSoulsDirectML or
                (uint)AID.TrinityOfSoulsDirectBR or (uint)AID.TrinityOfSoulsDirectBL or
                (uint)AID.TrinityOfSoulsInvertBR or (uint)AID.TrinityOfSoulsInvertBL or
                (uint)AID.TrinityOfSoulsInvertMR or (uint)AID.TrinityOfSoulsInvertML or
                (uint)AID.TrinityOfSoulsInvertTR or (uint)AID.TrinityOfSoulsInvertTL:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.WingTLFirst:
                VerifyFirstAOE(90f.Degrees(), false);
                break;
            case (uint)IconID.WingTRFirst:
                VerifyFirstAOE(-90f.Degrees(), false);
                break;
            case (uint)IconID.WingML:
                AddSubsequentAOE(90f.Degrees(), false);
                break;
            case (uint)IconID.WingMR:
                AddSubsequentAOE(-90f.Degrees(), false);
                break;
            case (uint)IconID.WingBLFirst:
                VerifyFirstAOE(90f.Degrees(), true);
                break;
            case (uint)IconID.WingBRFirst:
                VerifyFirstAOE(-90f.Degrees(), true);
                break;
            case (uint)IconID.WingTLLast:
                AddSubsequentAOE(90f.Degrees(), true);
                break;
            case (uint)IconID.WingTRLast:
                AddSubsequentAOE(-90f.Degrees(), true);
                break;
            case (uint)IconID.WingBLLast:
                AddSubsequentAOE(90f.Degrees(), true);
                break;
            case (uint)IconID.WingBRLast:
                AddSubsequentAOE(-90f.Degrees(), true);
                break;
        }
    }

    private void VerifyFirstAOE(Angle offset, bool inverted)
    {
        if (_aoes.Count == 0)
        {
            ReportError("No AOEs active");
            return;
        }
        if (!Module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
            ReportError($"Unexpected boss position: expected {_aoes[0].Origin}, have {Module.PrimaryActor.Position}");
        if (!_aoes[0].Rotation.AlmostEqual(Module.PrimaryActor.Rotation + offset, 0.05f))
            ReportError($"Unexpected first aoe angle: expected {Module.PrimaryActor.Rotation}+{offset}, have {_aoes[0].Rotation}");
        if (_invertMiddle != inverted)
            ReportError($"Unexpected invert: expected {inverted}, have {_invertMiddle}");
    }

    private void AddSubsequentAOE(Angle offset, bool last)
    {
        var expectedCount = last ? 2 : 1;
        if (_aoes.Count != expectedCount)
            ReportError($"Unexpected AOE count: expected {expectedCount}, got {_aoes.Count}");
        if (_aoes.Count > 0 && !Module.PrimaryActor.Position.AlmostEqual(_aoes[0].Origin, 1))
            ReportError($"Unexpected boss position: expected {_aoes[0].Origin}, have {Module.PrimaryActor.Position}");

        var rotation = (!last && _invertMiddle) ? Module.PrimaryActor.Rotation - offset : Module.PrimaryActor.Rotation + offset;
        _aoes.Add(new(_shape, Module.PrimaryActor.Position, rotation, _aoes.LastOrDefault().Activation.AddSeconds(2.6f)));
        if (_aoes.Count > 1 && !_aoes[^1].Rotation.AlmostEqual(_aoes[^2].Rotation, 0.05f))
            _moves |= last ? 2u : 1u;
    }
}
