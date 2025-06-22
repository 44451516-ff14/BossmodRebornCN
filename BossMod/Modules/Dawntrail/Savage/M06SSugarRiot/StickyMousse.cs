namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class StickyMousse(BossModule module) : Components.GenericStackSpread(module, true, raidwideOnResolve: false)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StickyMousseVisual)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            var act = Module.CastFinishAt(spell, 0.8f);
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                Spreads.Add(new(p, 4f, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StickyMousse)
        {
            Spreads.Clear();
            Stacks.Add(new(WorldState.Actors.Find(spell.MainTargetID)!, 4f, 4, 4, WorldState.FutureTime(6d)));
        }
        else if (spell.Action.ID == (uint)AID.BurstStickyMousse)
        {
            ++NumCasts;
        }
    }
}
