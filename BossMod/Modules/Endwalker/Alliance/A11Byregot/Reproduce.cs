namespace BossMod.Endwalker.Alliance.A11Byregot;

class Reproduce(BossModule module) : Components.Exaflare(module, 7f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.CloudToGroundFast or (uint)AID.CloudToGroundSlow)
        {
            var fast = spell.Action.ID == (uint)AID.CloudToGroundFast;
            Lines.Add(new(caster.Position, new(-8.5f, default), Module.CastFinishAt(spell), fast ? 0.6f : 1.4f, 6, fast ? 5 : 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CloudToGroundFast or (uint)AID.CloudToGroundSlow or (uint)AID.CloudToGroundFastAOE or (uint)AID.CloudToGroundSlowAOE)
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
