namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

sealed class CrushHelm(BossModule module) : BossComponent(module)
{
    public int NumSmallHits;
    public int NumLargeHits;
    private DateTime _lastSmallHit;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CrushHelmAOEFirst:
                if (WorldState.CurrentTime > _lastSmallHit.AddSeconds(0.2d))
                {
                    ++NumSmallHits;
                    _lastSmallHit = WorldState.CurrentTime;
                }
                break;
            case (uint)AID.CrushHelmAOERest:
                ++NumLargeHits;
                break;
        }
    }
}
