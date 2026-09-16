namespace BossMod.Endwalker.Savage.P11SThemis;

sealed class HeartOfJudgment : Components.GenericTowers
{
    public HeartOfJudgment(BossModule module) : base(module)
    {
        for (var i = 0; i < 4; ++i)
        {
            Towers.Add(new(Arena.Center + 11.5f * (45f + i * 90f).Degrees().ToDirection(), 4f, 2, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Explosion or (uint)AID.MassiveExplosion)
        {
            ++NumCasts;
            Towers.Clear();
        }
    }
}
