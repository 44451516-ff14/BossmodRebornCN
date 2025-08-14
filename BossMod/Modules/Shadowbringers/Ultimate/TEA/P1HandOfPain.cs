namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P1HandOfPain(BossModule module) : Components.CastCounter(module, (uint)AID.HandOfPain)
{
    private readonly TEA bossmod = (TEA)module;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (bossmod.LiquidHand2 is Actor hand)
        {
            var primary = Module.PrimaryActor;
            var diff = (int)(hand.HPMP.CurHP - primary.HPMP.CurHP) * 100.0f / primary.HPMP.MaxHP;
            hints.Add($"Hand HP: {(diff > 0f ? "+" : "")}{diff:f1}%");
        }
    }
}
