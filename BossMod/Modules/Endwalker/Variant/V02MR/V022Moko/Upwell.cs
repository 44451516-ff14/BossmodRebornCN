namespace BossMod.Endwalker.VariantCriterion.V02MR.V022Moko;

// note: each initial line sends out two 'exaflares' to the left & right
// each subsequent exaflare moves by distance 5, and happen approximately 2s apart
// each wave is 5 subsequent lines, except for two horizontal ones that go towards edges - they only have 1 line - meaning there's a total 22 'rest' casts
class UpwellFirst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UpwellFirst), new AOEShapeRect(30, 5, 30), color: Colors.Danger);

class UpwellRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(30, 2.5f, 30))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminentDeadline = WorldState.FutureTime(5);
        foreach (var (c, t, r) in FutureAOEs())
            if (t <= imminentDeadline)
                yield return new(Shape, c, r, t, FutureColor);
        foreach (var (c, t, r) in ImminentAOEs())
            if (t <= imminentDeadline)
                yield return new(Shape, c, r, t, ImminentColor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UpwellFirst)
        {
            var check = caster.Position.AlmostEqual(Arena.Center, 1);
            var isNorth = caster.Position.Z == 530;
            var isSouth = caster.Position.Z == 550;
            var numExplosions1 = check || isSouth ? 5 : 1;
            var numExplosions2 = check || isNorth ? 5 : 1;
            var advance1 = spell.Rotation.ToDirection().OrthoR() * 7.5f;
            var advance2 = spell.Rotation.ToDirection().OrthoR() * 5;
            Lines.Add(new() { Next = caster.Position + advance1, Advance = advance2, Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = numExplosions2, MaxShownExplosions = 2 });
            Lines.Add(new() { Next = caster.Position - advance1, Advance = -advance2, Rotation = (spell.Rotation + 180.Degrees()).Normalized(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = numExplosions1, MaxShownExplosions = 2 });
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UpwellRest)
        {
            ++NumCasts;
            var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 3) && l.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad));
            if (index >= 0)
            {
                AdvanceLine(Lines[index], caster.Position + 2.5f * Lines[index].Rotation.ToDirection().OrthoR());
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}
