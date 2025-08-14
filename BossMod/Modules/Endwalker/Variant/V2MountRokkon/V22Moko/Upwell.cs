namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 22 'rest' casts
sealed class UpwellFirst : Components.SimpleAOEs
{
    public UpwellFirst(BossModule module) : base(module, (uint)AID.UpwellFirst, new AOEShapeRect(60f, 5f))
    {
        Color = Colors.Danger;
    }
}

sealed class UpwellRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(30f, 2.5f, 30f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
        {
            return [];
        }
        var futureAOEs = FutureAOEs(linesCount);
        var imminentAOEs = ImminentAOEs(linesCount);
        var futureCount = futureAOEs.Count;
        var imminentCount = imminentAOEs.Length;
        var imminentDeadline = WorldState.FutureTime(5d);
        var index = 0;
        var aoes = new AOEInstance[futureCount + imminentCount];
        for (var i = 0; i < futureCount; ++i)
        {
            var aoe = futureAOEs[i];
            if (aoe.Item2 <= imminentDeadline)
            {
                aoes[index++] = new(Shape, aoe.Item1.Quantized(), aoe.Item3, aoe.Item2, FutureColor);
            }
        }

        for (var i = 0; i < imminentCount; ++i)
        {
            var aoe = imminentAOEs[i];
            if (aoe.Item2 <= imminentDeadline)
            {
                aoes[index++] = new(Shape, aoe.Item1.Quantized(), aoe.Item3, aoe.Item2, ImminentColor);
            }
        }
        return aoes.AsSpan()[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UpwellFirst)
        {
            var pos = caster.Position;
            var check = pos.AlmostEqual(Arena.Center, 1f);
            var isNorth = pos.Z == 530f;
            var isSouth = pos.Z == 550f;
            var numExplosions1 = check || isSouth ? 5 : 1;
            var numExplosions2 = check || isNorth ? 5 : 1;
            var dir = spell.Rotation.ToDirection().OrthoR();
            var advance1 = dir * 7.5f;
            var advance2 = dir * 5f;
            AddLine(pos + advance1, advance2, default, numExplosions2);
            AddLine(pos - advance1, -advance2, 180f.Degrees(), numExplosions1);
            void AddLine(WPos first, WDir dir, Angle offset, int explosions)
            => Lines.Add(new(first, dir, Module.CastFinishAt(spell), 2d, explosions, 2, spell.Rotation + offset));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UpwellRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            var rot = caster.Rotation;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 3f) && line.Rotation.AlmostEqual(rot, Angle.DegToRad))
                {
                    AdvanceLine(line, pos + 2.5f * line.Rotation.ToDirection().OrthoR());
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
