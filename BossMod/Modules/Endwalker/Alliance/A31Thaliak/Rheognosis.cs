namespace BossMod.Endwalker.Alliance.A31Thaliak;

class RheognosisKnockback(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _knockback;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Rheognosis or (uint)AID.RheognosisPetrine)
            _knockback = new(Arena.Center, 25f, Module.CastFinishAt(spell, 20.3d), direction: spell.Rotation + 180f.Degrees(), kind: Kind.DirForward);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RheognosisKnockback)
        {
            _knockback = null;
            ++NumCasts;
        }
    }
}

public class RheognosisCrash : Components.Exaflare
{
    public RheognosisCrash(BossModule module) : base(module, new AOEShapeRect(10f, 12f)) => ImminentColor = Colors.AOE;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x01 && state is 0x01000001u or 0x02000001u)
        {
            var west = index == 0x00;
            var right = state == 0x01000001u;
            var south = west == right;
            var start = Arena.Center + new WDir(west ? -24f : 24f, (south ? 24f : -24f) * 0.5f);
            var dir = (west ? 90f : -90f).Degrees();
            Lines.Add(new(start, 10f * dir.ToDirection(), WorldState.FutureTime(4d), 0.2d, 5, 5, dir));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RheognosisCrash)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
