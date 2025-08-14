namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V24Shishio;

sealed class Reisho1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ReishoFirst, 6f);

sealed class Reisho2(BossModule module) : Components.Voidzone(module, 6f, GetGhosts, 10f)
{
    private static List<Actor> GetGhosts(BossModule module) => module.Enemies((uint)OID.HauntingThrall);

    private bool started;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ReishoFirst)
        {
            started = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (started && NumCasts != 20)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (started && NumCasts != 20)
        {
            base.DrawArenaBackground(pcSlot, pc);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ReishoRest)
        {
            ++NumCasts;
        }
        else if (spell.Action.ID == (uint)AID.ThunderVortex) // not sure if mechanic can repeat if fight takes long enough
        {
            started = false;
            NumCasts = 0;
        }
    }
}
