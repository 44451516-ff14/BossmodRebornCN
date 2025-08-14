namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class UmbraSmash(BossModule module) : Components.Exaflare(module, new AOEShapeRect(5f, 30f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return;

        var imminentAOEs = ImminentAOEs(linesCount);

        // use only imminent aoes for hints
        var len = imminentAOEs.Length;
        for (var i = 0; i < len; ++i)
        {
            var aoe = imminentAOEs[i];
            hints.AddForbiddenZone(Shape, aoe.Item1, aoe.Item3, aoe.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddLine(WPos origin, WDir direction, Angle offset)
        => Lines.Add(new(origin, 5f * direction, Module.CastFinishAt(spell), 2.3d, 6, 2, spell.Rotation + offset));

        switch (spell.Action.ID)
        {
            case (uint)AID.UmbraSmashAOE1:
            case (uint)AID.UmbraSmashAOE2:
            case (uint)AID.UmbraSmashAOE3:
            case (uint)AID.UmbraSmashAOE4:
            case (uint)AID.UmbraSmashAOEClone:
                var dir = spell.Rotation.ToDirection();
                var origin = caster.Position + 30f * dir;
                AddLine(origin, dir.OrthoL(), 90f.Degrees());
                AddLine(origin, dir.OrthoR(), -90f.Degrees());
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UmbraSmashAOE1:
            case (uint)AID.UmbraSmashAOE2:
            case (uint)AID.UmbraSmashAOE3:
            case (uint)AID.UmbraSmashAOE4:
            case (uint)AID.UmbraSmashAOEClone:
                ++NumCasts;
                var origin = caster.Position + 30f * spell.Rotation.ToDirection();
                var count = Lines.Count;
                for (var i = 0; i < count; ++i)
                {
                    var l = Lines[i];
                    if (l.Next.AlmostEqual(origin, 1f))
                    {
                        l.Next = origin + l.Advance;
                        l.TimeToMove = 1f;
                        l.NextExplosion = WorldState.FutureTime(l.TimeToMove);
                        --l.ExplosionsLeft;
                    }
                }
                break;
            case (uint)AID.UmbraWave:
                ++NumCasts;
                var count2 = Lines.Count;
                var pos = caster.Position;
                for (var i = 0; i < count2; ++i)
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
                break;
        }
    }
}
