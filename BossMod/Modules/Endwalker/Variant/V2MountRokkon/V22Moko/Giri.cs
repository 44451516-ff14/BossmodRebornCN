namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

sealed class Giri(BossModule module) : Components.GenericAOEs(module)
{
    private enum NextSafeDirection { None, Front, Back, Left, Right }
    private NextSafeDirection nextDirection;
    private static readonly AOEShapeCone cone = new(60f, 135f.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private (WPos, Angle, DateTime) initialRotation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IaiKasumiGiri1:
            case (uint)AID.IaiKasumiGiri2:
            case (uint)AID.IaiKasumiGiri3:
            case (uint)AID.IaiKasumiGiri4:
            case (uint)AID.DoubleKasumiGiriFirst1:
            case (uint)AID.DoubleKasumiGiriFirst2:
            case (uint)AID.DoubleKasumiGiriFirst3:
            case (uint)AID.DoubleKasumiGiriFirst4:
                initialRotation = (spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                InitIfReady();
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GiriDirection)
        {
            nextDirection = status.Extra switch
            {
                0x248 => NextSafeDirection.Back,
                0x24A => NextSafeDirection.Front,
                0x249 => NextSafeDirection.Left,
                0x24B => NextSafeDirection.Right,
                _ => NextSafeDirection.None
            };
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (nextDirection != NextSafeDirection.None && initialRotation != default)
        {
            var count = _aoes.Count == 1;
            var angle = initialRotation.Item2;
            var rotation = CalculateRotation(nextDirection, angle, count);
            var finishTime = count ? initialRotation.Item3.AddSeconds(3.5d) : initialRotation.Item3;

            _aoes.Add(new(cone, initialRotation.Item1, rotation, finishTime));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.IaiKasumiGiri1:
                case (uint)AID.IaiKasumiGiri2:
                case (uint)AID.IaiKasumiGiri3:
                case (uint)AID.IaiKasumiGiri4:
                case (uint)AID.DoubleKasumiGiriFirst1:
                case (uint)AID.DoubleKasumiGiriFirst2:
                case (uint)AID.DoubleKasumiGiriFirst3:
                case (uint)AID.DoubleKasumiGiriFirst4:
                case (uint)AID.DoubleKasumiGiriRest1:
                case (uint)AID.DoubleKasumiGiriRest2:
                case (uint)AID.DoubleKasumiGiriRest3:
                case (uint)AID.DoubleKasumiGiriRest4:
                    _aoes.RemoveAt(0);
                    if (_aoes.Count == 0)
                    {
                        initialRotation = default;
                        nextDirection = NextSafeDirection.None;
                    }
                    break;
            }
        }
    }

    private static Angle CalculateRotation(NextSafeDirection direction, Angle baseAngle, bool isFirstAoe)
        => direction switch
        {
            NextSafeDirection.Back => baseAngle,
            NextSafeDirection.Front => isFirstAoe ? baseAngle + 180f.Degrees() : baseAngle,
            NextSafeDirection.Left => isFirstAoe ? baseAngle - 90f.Degrees() : baseAngle,
            NextSafeDirection.Right => isFirstAoe ? baseAngle + 90f.Degrees() : baseAngle,
            _ => default
        };
}
