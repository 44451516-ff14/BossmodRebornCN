namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

sealed class RuinationCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ruination, new AOEShapeCross(40f, 4f));

sealed class RuinationExaflare(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RuinationExaStart)
        {
            Lines.Add(new(caster.Position, 4f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.1d, 6, 6));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RuinationExaStart or (uint)AID.RuinationExaMove)
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
}
