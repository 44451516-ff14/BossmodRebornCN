namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2ArrowTrail(BossModule module) : Components.Exaflare(module, new AOEShapeRect(10f, 5f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailHint)
            Lines.Add(new() { Next = caster.Position, Advance = 5f * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell, 0.4f), TimeToMove = 0.5f, ExplosionsLeft = 8, MaxShownExplosions = 3 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArrowTrailAOE)
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
        }
    }
}

class P2DownhillArrowTrailDownhill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArrowTrailDownhill, 6f);
