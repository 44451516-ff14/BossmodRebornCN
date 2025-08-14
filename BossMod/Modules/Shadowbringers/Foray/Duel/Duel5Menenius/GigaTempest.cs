namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

abstract class GigaTempest(BossModule module, AOEShapeRect shape, uint aidFirst, uint aidRest) : Components.Exaflare(module, shape)
{

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == aidFirst)
        {
            var advance = GetExaDirection(caster);
            if (advance == null)
                return;
            Lines.Add(new(caster.Position, advance.Value, Module.CastFinishAt(caster.CastInfo!), 0.9d, 5, 5, caster.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == aidFirst || spell.Action.ID == aidRest)
        {
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
        }
    }

    // The Gigatempest caster's heading is only used for rotating the AOE shape.
    // The exaflare direction must be derived from the caster's location.
    private static WDir? GetExaDirection(Actor caster)
    {
        Angle? forwardAngle = null;
        if (caster.Position.Z == 536f)
            forwardAngle = 180f.Degrees();
        else if (caster.Position.Z == 504f)
            forwardAngle = default;
        else if (caster.Position.X == -82f)
            forwardAngle = 90f.Degrees();
        else if (caster.Position.X == -794f)
            forwardAngle = 270f.Degrees();

        if (forwardAngle == null)
            return null;

        return 8f * forwardAngle.Value.ToDirection();
    }
}

sealed class SmallGigaTempest(BossModule module) : GigaTempest(module, new AOEShapeRect(10f, 6.5f), (uint)AID.GigaTempestSmallStart, (uint)AID.GigaTempestSmallMove);
sealed class LargeGigaTempest(BossModule module) : GigaTempest(module, new AOEShapeRect(35f, 6.5f), (uint)AID.GigaTempestLargeStart, (uint)AID.GigaTempestLargeMove);
