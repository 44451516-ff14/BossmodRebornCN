namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class EasternEwers(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BrimOver)
        {
            Lines.Add(new(caster.Position, new(default, 3.38788f), Module.CastFinishAt(spell), 0.8d, 14, 14));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Rinse)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 0.1f))
                {
                    AdvanceLine(line, pos);
                    if (line.Next.Z > -135f)
                    {
                        line.Next = new(line.Next.X, -135f); // ewers stop at z = -135 and hit twice there
                    }
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}
