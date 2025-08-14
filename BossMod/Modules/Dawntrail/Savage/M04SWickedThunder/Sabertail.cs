namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class Sabertail(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SabertailFirst)
        {
            var dir = spell.Rotation.ToDirection();
            var startOffset = caster.Position - Arena.Center;
            startOffset = new(startOffset.X, startOffset.Z * 0.99f); // handle exaflares right on N/S borders
            var distanceToBorder = Arena.Bounds.IntersectRay(startOffset, dir);
            Lines.Add(new(caster.Position, 6.5f * dir, Module.CastFinishAt(spell), 0.7d, (int)(distanceToBorder / 6.5f) + 1, 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SabertailFirst or (uint)AID.SabertailRest)
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
