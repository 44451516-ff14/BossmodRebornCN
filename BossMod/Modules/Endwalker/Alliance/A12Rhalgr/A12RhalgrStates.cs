namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class A12RhalgrStates : StateMachineBuilder
{
    public A12RhalgrStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<DestructiveBolt>()
            .ActivateOnEnter<HandOfTheDestroyer>()
            .ActivateOnEnter<BrokenWorld>()
            .ActivateOnEnter<BrokenShards>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<RhalgrBeaconAOE>()
            .ActivateOnEnter<RhalgrBeaconKnockback>()
            .ActivateOnEnter<RhalgrBeaconShock>()
            .ActivateOnEnter<BronzeLightning>()
            .ActivateOnEnter<StrikingMeteor>();
    }

    private void SinglePhase(uint id)
    {
        LightningReign(id, 7.2f);
        HandOfTheDestroyer(id + 0x10000u, 7.2f);
        BrokenWorld(id + 0x20000u, 12.5f);
        RhalgrBeacon(id + 0x30000u, 1.6f);
        HandOfTheDestroyer(id + 0x40000u, 11.2f);
        DestructiveBolt(id + 0x50000u, 11.5f);
        HandOfTheDestroyerBrokenShards(id + 0x60000u, 6.2f, true);
        HandOfTheDestroyerBrokenShards(id + 0x70000u, 12.4f);

        BronzeWork(id + 0x100000u, 9.8f);
        HandOfTheDestroyerBrokenWorldLightningStorm(id + 0x110000u, 1.1f); // any?
        HellOfLightningRhalgrBeacon(id + 0x120000u, 5.9f);
        HandOfTheDestroyerBrokenShards(id + 0x130000u, 8.5f); // any?
        LightningReign(id + 0x140000u, 4.4f);

        BronzeWork(id + 0x200000u, 11.5f);
        HandOfTheDestroyerBrokenWorldLightningStorm(id + 0x210000u, 1.1f); // any?
        HellOfLightningRhalgrBeacon(id + 0x220000u, 5.9f);
        HandOfTheDestroyerBrokenShards(id + 0x230000u, 8.5f); // any? (note: didn't see this and beyond)
        LightningReign(id + 0x240000u, 4.4f);

        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void LightningReign(uint id, float delay)
    {
        Cast(id, (uint)AID.LightningReign, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State DestructiveBolt(uint id, float delay)
    {
        Cast(id, (uint)AID.DestructiveBolt, delay, 4f);
        return ComponentCondition<DestructiveBolt>(id + 0x10u, 1f, comp => comp.NumCasts != 0, "Tankbusters")
            .ResetComp<DestructiveBolt>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HandOfTheDestroyer(uint id, float delay)
    {
        Cast(id, (uint)AID.AdventOfTheEighth, delay, 4);
        CastMulti(id + 0x10u, [(uint)AID.HandOfTheDestroyerWrath, (uint)AID.HandOfTheDestroyerJudgment], 6.1f, 9f);
        ComponentCondition<HandOfTheDestroyer>(id + 0x20u, 0.4f, comp => comp.NumCasts != 0, "Side cleave")
            .ResetComp<HandOfTheDestroyer>();
    }

    private void BrokenWorld(uint id, float delay)
    {
        Cast(id, (uint)AID.BrokenWorld, delay, 3f);
        ComponentCondition<BrokenWorld>(id + 0x10u, 2.1f, comp => comp.Casters.Count != 0);
        ComponentCondition<BrokenWorld>(id + 0x20u, 10.6f, comp => comp.NumCasts != 0, "Proximity")
            .ResetComp<BrokenWorld>();
    }

    private void HandOfTheDestroyerBrokenShards(uint id, float delay, bool first = false)
    {
        Cast(id, (uint)AID.AdventOfTheEighth, delay, 4f);
        Cast(id + 0x10u, (uint)AID.BrokenWorld, first ? 7.5f : 6.1f, 3f);
        CastMulti(id + 0x20u, [(uint)AID.HandOfTheDestroyerWrathBroken/*, (uint)AID.HandOfTheDestroyerJudgmentBroken*/], 2.1f, 9f);
        ComponentCondition<BrokenShards>(id + 0x30u, 5.7f, comp => comp.NumCasts >= 9, "AOEs")
            .ResetComp<BrokenShards>();
    }

    private void HandOfTheDestroyerBrokenWorldLightningStorm(uint id, float delay)
    {
        Cast(id, (uint)AID.AdventOfTheEighth, delay, 4f);
        Cast(id + 0x10u, (uint)AID.BrokenWorld, 6.2f, 3f);
        CastMulti(id + 0x20, [(uint)AID.HandOfTheDestroyerWrath, (uint)AID.HandOfTheDestroyerJudgment], 2.1f, 9f);
        ComponentCondition<HandOfTheDestroyer>(id + 0x30u, 0.4f, comp => comp.NumCasts != 0, "Side cleave")
            .ResetComp<HandOfTheDestroyer>();
        ComponentCondition<BrokenWorld>(id + 0x40u, 1.2f, comp => comp.NumCasts != 0, "Proximity") // spreads start ~0.5s before proximity
            .ResetComp<BrokenWorld>();
        ComponentCondition<LightningStorm>(id + 0x50u, 7.5f, comp => comp.NumFinishedSpreads != 0, "Spreads")
            .ResetComp<LightningStorm>();
    }

    private void RhalgrBeacon(uint id, float delay)
    {
        Cast(id, (uint)AID.RhalgrsBeacon, delay, 9.3f);
        ComponentCondition<RhalgrBeaconKnockback>(id + 0x10u, 0.7f, comp => comp.NumCasts != 0, "Knockback")
            .ResetComp<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconAOE>(id + 0x11, 0.3f, comp => comp.NumCasts != 0, "AOE")
            .ResetComp<RhalgrBeaconAOE>();
    }

    private void HellOfLightningRhalgrBeacon(uint id, float delay)
    {
        Cast(id, (uint)AID.HellOfLightning, delay, 3f);
        Cast(id + 0x10u, (uint)AID.RhalgrsBeacon, 2.1f, 9.3f); // shock actors are created ~0.1s into cast, start their casts 7s later
        ComponentCondition<RhalgrBeaconKnockback>(id + 0x20, 0.7f, comp => comp.NumCasts != 0, "Knockback")
            .ResetComp<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconAOE>(id + 0x21, 0.3f, comp => comp.NumCasts != 0, "AOE")
            .ResetComp<RhalgrBeaconAOE>();
        ComponentCondition<RhalgrBeaconShock>(id + 0x30, 2.7f, comp => comp.NumCasts != 0, "Lightning orbs");
    }

    private void BronzeWork(uint id, float delay)
    {
        Cast(id, (uint)AID.BronzeWork, delay, 6.5f); // puddles start ~0.5s before cast end

        ComponentCondition<BronzeLightning>(id + 0x10u, 0.5f, comp => comp.NumCasts != 0, "Cones 1");
        ComponentCondition<BronzeLightning>(id + 0x20u, 2.0f, comp => comp.NumCasts > 4, "Cones 2")
            .ResetComp<BronzeLightning>();
        DestructiveBolt(id + 0x100u, 1.6f)
            .ResetComp<StrikingMeteor>(); // second set of puddles finish ~0.4s into cast
    }
}
