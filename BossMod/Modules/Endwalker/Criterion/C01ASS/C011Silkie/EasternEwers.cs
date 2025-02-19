﻿namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

class EasternEwers(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver)
        {
            Lines.Add(new() { Next = caster.Position, Advance = new(default, 5.1f), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 11, MaxShownExplosions = int.MaxValue });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBrimOver or (uint)AID.SBrimOver or (uint)AID.NRinse or (uint)AID.SRinse)
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
