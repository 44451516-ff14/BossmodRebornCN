namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2ArrowTrail(BossModule module) : Components.Exaflare(module, new AOEShapeRect(10f, 5f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailHint)
        {
            var dir = new WDir(default, 5f);
            Lines.Add(new(caster.Position - dir, dir, Module.CastFinishAt(spell, 0.4d), 0.5d, 8, 3, Angle.AnglesCardinals[1]));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailAOE)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position - new WDir(default, 5f);
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
}

class P2DownhillArrowTrailDownhill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArrowTrailDownhill, 6f);
