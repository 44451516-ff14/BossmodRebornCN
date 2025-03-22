namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

class ScarletPlumeTailFeather(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private static readonly AOEShapeCircle circle = new(9f);
    private static readonly uint[] _feathers = [(uint)OID.ScarletTailFeather, (uint)OID.ScarletPlume];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingAndAPrayerTailFeather)
            AOEs.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WingAndAPrayerTailFeather)
            AOEs.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var feathers = Module.Enemies((uint)OID.ScarletTailFeather);
        var count = feathers.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; ++i)
            {
                if (feathers[i].IsTargetable)
                {
                    hints.Add("Kill birds outside of AOEs!");
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var birds = Module.Enemies((uint)OID.ScarletLady);
        var feathers = Module.Enemies(_feathers);
        var countB = birds.Count;
        var countF = feathers.Count;
        for (var i = 0; i < countB; ++i)
        {
            var b = birds[i];
            if (b.IsDead)
                continue;
            var notInAOE = true;
            for (var j = 0; j < countF; ++j)
            {
                var f = feathers[j];
                if (f.IsDead)
                    continue;
                if (b.Position.InCircle(f.Position, 7.12f))
                {
                    notInAOE = false;
                    hints.SetPriority(b, AIHints.Enemy.PriorityForbidden);
                    break;
                }
            }
            if (notInAOE)
                hints.SetPriority(b, 1);
        }
    }
}
