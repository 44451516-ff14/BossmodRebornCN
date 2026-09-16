namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P1HandOfPain(TEA module) : Components.CastCounter(module, (uint)AID.HandOfPain)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (module.LiquidHand2 is Actor hand)
        {
            var primary = module.PrimaryActor;
            var diff = (int)(hand.HPMP.CurHP - primary.HPMP.CurHP) * 100.0f / primary.HPMP.MaxHP;
            hints.Add($"Hand HP: {(diff > 0f ? "+" : "")}{diff:f1}%");
        }
    }
}
