namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P5Enrage(BossModule module) : Components.UniformStackSpread(module, default, 4f)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Enrage)
        {
            AddSpreads(Raid.WithoutSlot(true, true, true), Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Enrage or (uint)AID.EnrageAOE)
        {
            ++NumCasts;
        }
    }
}
