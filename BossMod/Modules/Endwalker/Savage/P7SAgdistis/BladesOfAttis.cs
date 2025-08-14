namespace BossMod.Endwalker.Savage.P7SAgdistis;

class BladesOfAttis(BossModule module) : Components.Exaflare(module, 7f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BladesOfAttisFirst)
        {
            Lines.Add(new(caster.Position, 7f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 2d, 8, 8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BladesOfAttisFirst or (uint)AID.BladesOfAttisRest)
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
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
