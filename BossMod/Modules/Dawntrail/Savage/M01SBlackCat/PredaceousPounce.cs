namespace BossMod.Dawntrail.Savage.M01SBlackCat;

class PredaceousPounce(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private bool sorted;
    private static readonly AOEShapeCircle circle = new(11);
    private static readonly HashSet<AID> chargeTelegraphs = [AID.PredaceousPounceTelegraphCharge1, AID.PredaceousPounceTelegraphCharge2,
            AID.PredaceousPounceTelegraphCharge3, AID.PredaceousPounceTelegraphCharge4, AID.PredaceousPounceTelegraphCharge5,
            AID.PredaceousPounceTelegraphCharge6];
    private static readonly HashSet<AID> circleTelegraphs = [AID.PredaceousPounceTelegraphCircle1, AID.PredaceousPounceTelegraphCircle2,
            AID.PredaceousPounceTelegraphCircle3, AID.PredaceousPounceTelegraphCircle4, AID.PredaceousPounceTelegraphCircle5,
            AID.PredaceousPounceTelegraphCircle6];
    private static readonly HashSet<AID> castEnd = [AID.PredaceousPounceCharge1, AID.PredaceousPounceCharge2, AID.PredaceousPounceCircle1, AID.PredaceousPounceCircle2];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count > 0)
        {
            var aoeCount = Math.Clamp(count, 0, 2);
            for (var i = aoeCount; i < count; ++i)
                yield return AOEs[i];
            for (var i = 0; i < aoeCount; ++i)
                yield return AOEs[i] with { Color = Colors.Danger };
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (chargeTelegraphs.Contains((AID)spell.Action.ID))
        {
            var dir = spell.LocXZ - caster.Position;
            AOEs.Add(new(new AOEShapeRect(dir.Length(), 3), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell)));
        }
        else if (circleTelegraphs.Contains((AID)spell.Action.ID))
            AOEs.Add(new(circle, caster.Position, default, Module.CastFinishAt(spell)));
        var count = AOEs.Count;
        if (count == 12 && !sorted)
        {
            AOEs.SortBy(x => x.Activation);
            for (var i = 0; i < count; ++i)
            {
                var aoe = AOEs[i];
                aoe.Activation = WorldState.FutureTime(13.5f + i * 0.5f);
                AOEs[i] = aoe;
            }
            sorted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (castEnd.Contains((AID)spell.Action.ID))
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}
