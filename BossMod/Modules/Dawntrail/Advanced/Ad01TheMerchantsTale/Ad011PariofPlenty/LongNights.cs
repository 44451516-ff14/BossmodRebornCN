namespace BossMod.Modules.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

sealed class LeftRightFireflight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFireflightFourLongNights, (uint)AID.RightFireflightFourLongNights], new AOEShapeRect(40f, 2f)); //simple aoe for first cast

sealed class WheelOfFireflight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        return len == 0 ? [] : aoes[..1];
    }

    private bool _startLeft = false;
    private Angle _currentRot = default;
    private uint _prevIcon = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftFireflightFourLongNights)
        {
            _startLeft = true;
        }
        else if (spell.Action.ID is (uint)AID.RightFireflightFourLongNights)
        {
            _startLeft = false;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.TurningRight or (uint)IconID.TurningLeft or (uint)IconID.TurningRRight or (uint)IconID.TurningRLeft)
        {
            if (_prevIcon == default)
            {
                if (_startLeft)
                {
                    _currentRot = iconID is (uint)IconID.TurningRight or (uint)IconID.TurningRRight ? 180f.Degrees() : 0f.Degrees();
                }
                else
                {
                    _currentRot = iconID is (uint)IconID.TurningRight or (uint)IconID.TurningRRight ? 0f.Degrees() : 180f.Degrees();
                }
            }
            else
            {
                if (_prevIcon == iconID)
                {
                    _currentRot += 180f.Degrees();
                }
            }

            _aoes.Add(new(new AOEShapeCone(40f, 90f.Degrees()), Module.PrimaryActor.Position, _currentRot));
            _prevIcon = iconID;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WheelOfFireflight or (uint)AID.WheelOfFireflight1 or (uint)AID.WheelOfFireflight2 or (uint)AID.WheelOfFireflight3)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
                if (_aoes.Count == 0)
                {
                    _currentRot = default;
                    _prevIcon = default;
                }
            }
        }
    }
}
