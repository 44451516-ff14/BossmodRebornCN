namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class Distortion(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Eye> _eyes = [with(3)];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => CollectionsMarshal.AsSpan(_eyes);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u)
        {
            bool? inverted = actor.OID switch
            {
                (uint)OID.BlackDissonance => true,
                (uint)OID.WhiteDissonance => false,
                _ => null
            };
            if (inverted is bool inv)
            {
                var loc = actor.Position.Quantized();
                _eyes.Add(new(loc, WorldState.FutureTime(8.1d), inverted: inv, eyeCenter: loc));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_eyes.Count != 0 && spell.Action.ID is (uint)AID.WhiteDissonance or (uint)AID.BlackDissonance)
        {
            _eyes.RemoveAt(0);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_eyes.Count == 0)
        {
            return;
        }
        ref readonly var eye = ref _eyes.Ref(0);
        var inverted = eye.Inverted;
        if ((actor.Rotation.ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0f) != inverted) // 0f = cos(pi/2)
        {
            hints.Add(inverted ? "Face the eye!" : "Turn away from gaze!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_eyes.Count == 0)
        {
            return;
        }
        ref readonly var eye = ref _eyes.Ref(0);
        var actorpos = actor.Position;
        var pos = eye.Position;
        var direction = eye.Inverted ? Angle.FromDirection(actorpos - pos) : Angle.FromDirection(pos - actorpos);
        hints.ForbiddenDirections.Add((direction, 90f.Degrees(), eye.Activation));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_eyes.Count == 0)
        {
            return;
        }
        ref readonly var eye = ref _eyes.Ref(0);
        var rot = pc.Rotation;
        var pos = pc.Position;
        var inverted = eye.Inverted;
        var danger = (rot.ToDirection().Dot((eye.Position - pos).Normalized()) >= 0f) != inverted;
        Arena.DrawEye(eye.EyeCenter!.Value, danger, inverted);

        var (min, max) = inverted ? (90f, 270f) : (-90f, 90f);
        Arena.PathArcTo(pos, 1.5f, (rot + min.Degrees()).Rad, (rot + max.Degrees()).Rad);
        Arena.PathStroke(false, Colors.Enemy);
    }
}
