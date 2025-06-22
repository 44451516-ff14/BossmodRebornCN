namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class DimensionalDistortion(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DimensionalDistortionFirst)
        {
            Lines.Add(new() { Next = spell.LocXZ, Advance = 8.5f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DimensionalDistortionFirst or (uint)AID.DimensionalDistortionRest)
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
