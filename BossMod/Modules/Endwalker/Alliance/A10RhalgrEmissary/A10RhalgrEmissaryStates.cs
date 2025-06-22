namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class A10RhalgrEmissaryStates : StateMachineBuilder
{
    public A10RhalgrEmissaryStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Boltloop>()
            .ActivateOnEnter<BoltsFromTheBlue>()
            .ActivateOnEnter<DestructiveStrike>()
            .ActivateOnEnter<DestructiveStatic>()
            .ActivateOnEnter<DestructiveCharge>();
    }

    private void SinglePhase(uint id)
    {
        DestructiveStatic(id, 5.4f);
        DestructiveChargeLightningBolt(id + 0x10000u, 14.4f);
        BoltsFromTheBlue(id + 0x20000u, 2.2f);
        DestructiveChargeDestructiveStatic(id + 0x30000u, 12.4f);
        Boltloop(id + 0x40000u, 6.2f);
        DestructiveStrike(id + 0x50000u, 4.1f);
        BoltsFromTheBlue(id + 0x60000u, 6.2f);
        DestructiveChargeLightningBolt(id + 0x70000u, 13.5f);

        SimpleState(id + 0xFF0000u, 10, "???");
    }

    private void DestructiveStatic(uint id, float delay)
    {
        Cast(id, (uint)AID.DestructiveStatic, delay, 8f, "Frontal cleave");
    }

    private void DestructiveChargeLightningBolt(uint id, float delay)
    {
        ComponentCondition<DestructiveCharge>(id, delay, comp => comp.AOEs.Count != 0);
        CastStart(id + 0x10u, (uint)AID.LightningBolt, 6.7f);
        ComponentCondition<DestructiveCharge>(id + 0x20u, 2.4f, comp => comp.NumCasts != 0, "Diagonal cleaves")
            .ResetComp<DestructiveCharge>();
        CastEnd(id + 0x30u, 0.6f);
        ComponentCondition<LightningBolt>(id + 0x40u, 1f, comp => comp.Casters.Count != 0); // 3x3, 3sec cast each, 2s between sets
        ComponentCondition<LightningBolt>(id + 0x50u, 7f, comp => comp.Casters.Count == 0, "Puddles resolve");
    }

    private void DestructiveChargeDestructiveStatic(uint id, float delay)
    {
        ComponentCondition<DestructiveCharge>(id, delay, comp => comp.AOEs.Count != 0);
        CastStart(id + 0x10u, (uint)AID.DestructiveStatic, 3.9f);
        ComponentCondition<DestructiveCharge>(id + 0x20u, 5.2f, comp => comp.NumCasts != 0, "Diagonal cleaves")
            .ResetComp<DestructiveCharge>();
        CastEnd(id + 0x30u, 2.8f, "Frontal cleave");
    }

    private void BoltsFromTheBlue(uint id, float delay)
    {
        Cast(id, (uint)AID.BoltsFromTheBlue, delay, 5f);
        ComponentCondition<BoltsFromTheBlue>(id + 2u, 1f, comp => comp.NumCasts != 0, "Raidwide")
            .ResetComp<BoltsFromTheBlue>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Boltloop(uint id, float delay)
    {
        Cast(id, (uint)AID.Boltloop, delay, 2f);
        ComponentCondition<Boltloop>(id + 0x10u, 1.1f, comp => comp.NumCasts >= 2, "Concentric AOE 1");
        ComponentCondition<Boltloop>(id + 0x20u, 2f, comp => comp.NumCasts >= 4, "Concentric AOE 2");
        ComponentCondition<Boltloop>(id + 0x30u, 2f, comp => comp.NumCasts >= 6, "Concentric AOE 3")
            .ResetComp<Boltloop>();
    }

    private void DestructiveStrike(uint id, float delay)
    {
        Cast(id, (uint)AID.DestructiveStrike, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}
