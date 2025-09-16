namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class GaeaStream(BossModule module) : Components.Exaflare(module, new AOEShapeRect(4f, 12f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
        {
            return;
        }

        var imminentAOEs = ImminentAOEs(linesCount);

        // use only imminent aoes for hints
        var len = imminentAOEs.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref imminentAOEs[i];
            hints.AddForbiddenZone(Shape, aoe.Item1, aoe.Item3, aoe.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GaeaStreamFirst)
        {
            Lines.Add(new(caster.Position, 4f * caster.Rotation.Round(1f).ToDirection(), Module.CastFinishAt(spell), 2.1d, 6, 2, spell.Rotation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GaeaStreamFirst or (uint)AID.GaeaStreamRest)
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
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}
