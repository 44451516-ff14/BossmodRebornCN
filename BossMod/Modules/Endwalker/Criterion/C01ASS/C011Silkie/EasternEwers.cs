namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

sealed class EasternEwers(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver)
        {
            Lines.Add(new(caster.Position, new(default, 5.1f), Module.CastFinishAt(spell), 0.8d, 11, int.MaxValue));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver or (uint)AID.NRinse or (uint)AID.SRinse)
        {
            var count = Lines.Count;
            var pos = caster.Position.X;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (Math.Abs(line.Next.X - pos) < 1f)
                {
                    AdvanceLine(line, caster.Position);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}
