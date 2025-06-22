namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA2Raiden;

sealed class BA2RaidenStates : StateMachineBuilder
{
    public BA2RaidenStates(BossModule module) : base(module)
    {
        SimplePhase(default, Phase1, "P1")
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<SpiritsOfTheFallen>()
            .ActivateOnEnter<Shingan>()
            .ActivateOnEnter<AmeNoSakahoko>()
            .ActivateOnEnter<WhirlingZantetsuken>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || !Module.PrimaryActor.IsTargetable;
        DeathPhase(1u, Phase2) // starts at around 70% or after becoming untargetable
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<LateralZantetsuken>()
            .ActivateOnEnter<LancingBlowSpread>()
            .ActivateOnEnter<LancingBlowAOE>()
            .ActivateOnEnter<CloudToGround>()
            .ActivateOnEnter<BitterBarbs>()
            .ActivateOnEnter<Levinwhorl>()
            .ActivateOnEnter<BoomingLament>()
            .ActivateOnEnter<SilentLevin>()
            .ActivateOnEnter<ForHonor>()
            .ActivateOnEnter<UltimateZantetsuken>();
    }

    private void Phase1(uint id) // all timings seem to have lots of variation, maybe HP based conditions
    {
        SpiritsOfTheFallen(id, 7.8f);
        Shingan(id + 0x10000u, 6f);
        Thundercall(id + 0x20000u, 8f);
        AmeNoSakahoko(id + 0x30000u, 9.4f);
        WhirlingZantetsuken(id + 0x40000u, 1.1f);
        SpiritsOfTheFallen(id + 0x50000u, 9f); // apparently this raidwide can be skipped without actually getting a phase change?
        AmeNoSakahoko(id + 0x60000u, 6.3f);
        WhirlingZantetsuken(id + 0x70000u, 1.1f);
        IsTargetable(id + 0x80000u, false, 3.6f);
    }

    private void Phase2(uint id)  // many timings seem to have lots of variation, no idea if they are predictable or just due to server load and latency
    {
        BallLightningLateralZantetsuken(id, 9f);
        Targetable(id + 0x10000u, true, 4.3f);
        SpiritsOfTheFallen(id + 0x20000u, 2.6f);
        UltimateZantetsuken(id + 0x30000u, 12.8f);
        SpiritsOfTheFallen(id + 0x40000u, 6f);
        BoomingLamentCloudToGround(id + 0x50000u, 2.4f);
        WhirlingZantetsuken(id + 0x60000u, 2.6f);
        SpiritsOfTheFallen(id + 0x70000u, 6.1f);
        CloudToGroundLevinWhorl(id + 0x80000u, 13f);
        AmeNoSakahoko(id + 0x90000u, 5.4f);
        WhirlingZantetsuken(id + 0xA0000u, 1.1f);
        SilentLevinBoomingLament(id + 0xB0000u, 2f);
        SpiritsOfTheFallen(id + 0xC0000u, 5.3f);
        BallLightningLateralZantetsukenUltimateZantetsuken(id + 0xD0000u, 8.2f);
        SpiritsOfTheFallen(id + 0xE0000u, 5.9f);
        Shingan(id + 0xF0000u, 1.9f);
        AmeNoSakahoko(id + 0x100000u, 7.1f);
        ForHonor(id + 0x110000u, 1.1f);
        WhirlingZantetsukenSilentLevin(id + 0x120000u, 2.1f);
        SpiritsOfTheFallen(id + 0x130000u, 7.8f);
        SpiritsOfTheFallen(id + 0x140000u, 2.1f);
        CloudToGroundSilentLevin(id + 0x150000u, 16.2f);
        WhirlingZantetsuken(id + 0x160000u, 0.2f);
        SpiritsOfTheFallen(id + 0x170000u, 6.2f);
        CloudToGroundLevinWhorl(id + 0x180000u, 13.1f);
        AmeNoSakahoko(id + 0x190000u, 1.3f);
        WhirlingZantetsuken(id + 0x1A0000u, 1.2f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Shingan(uint id, float delay)
    {
        Cast(id, (uint)AID.Shingan, delay, 4f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SpiritsOfTheFallen(uint id, float delay)
    {
        Cast(id, (uint)AID.SpiritsOfTheFallen, delay, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Thundercall(uint id, float delay)
    {
        Cast(id, (uint)AID.Thundercall, delay, 3f, "Arena wall");
    }

    private void AmeNoSakahoko(uint id, float delay)
    {
        ComponentCondition<AmeNoSakahoko>(id, delay, comp => comp.Casters.Count != 0, "Circle AOE");
        ComponentCondition<AmeNoSakahoko>(id + 0x10u, 7.5f, comp => comp.Casters.Count == 0, "AOE resolve");
    }

    private void WhirlingZantetsuken(uint id, float delay)
    {
        Cast(id, (uint)AID.WhirlingZantetsuken, delay, 5.5f, "Donut AOE");
    }

    private void IsTargetable(uint id, bool targetable, float delay)
    {
        Targetable(id, targetable, delay);
    }

    private void BallLightningLateralZantetsuken(uint id, float delay)
    {
        ComponentCondition<Shock>(id, delay, comp => comp.Casters.Count != 0, "Baited circles");
        CastStartMulti(id + 0x10u, [(uint)AID.LateralZantetsuken1, (uint)AID.LateralZantetsuken2], 0.8f, "Half room cleave starts");
        ComponentCondition<Shock>(id + 0x20u, 2.1f, comp => comp.Casters.Count == 0, "Circles resolve");
        CastEnd(id + 0x30u, 4.3f, "Cleave resolves");
    }

    private void BallLightningLateralZantetsukenUltimateZantetsuken(uint id, float delay)
    {
        Targetable(id, false, delay);
        ComponentCondition<LancingBlowSpread>(id + 0x10u, 3.3f, comp => comp.Spreads.Count != 0, "Spread markers appear");
        ComponentCondition<Shock>(id + 0x20u, 1.1f, comp => comp.Casters.Count != 0, "Baited circles");
        ComponentCondition<Shock>(id + 0x30u, 3f, comp => comp.Casters.Count == 0, "Circles resolve");
        ComponentCondition<LancingBlowSpread>(id + 0x40u, 1.1f, comp => comp.Spreads.Count == 0, "Adds spawn");
        CastStartMulti(id + 0x50u, [(uint)AID.LateralZantetsuken1, (uint)AID.LateralZantetsuken2], 0.1f, "Half room cleave starts");
        ComponentCondition<LancingBlowAOE>(id + 0x60u, 1f, comp => comp.NumCasts != 0, "AOEs around adds");
        ComponentCondition<SilentLevin>(id + 0x70u, 0.3f, comp => comp.Casters.Count != 0, "Baited circles 1");
        ComponentCondition<SilentLevin>(id + 0x80u, 1.9f, comp => comp.Casters.Count > 3, "Baited circles 2");
        ComponentCondition<SilentLevin>(id + 0x90u, 2.7f, comp => comp.NumCasts >= 3 && comp.Casters.Count > 3, "Baited circles 3");
        CastEnd(id + 0xA0u, 1.6f, "Cleave resolves");
        Targetable(id + 0xB0u, true, 4.7f);
        CastStart(id + 0xC0u, (uint)AID.UltimateZantetsuken, 2.1f, "Enrage start");
        CastEnd(id + 0xD0u, 18f, "Enrage")
            .ResetComp<LancingBlowSpread>()
            .ResetComp<SilentLevin>();
    }

    private void UltimateZantetsuken(uint id, float delay)
    {
        ComponentCondition<LancingBlowSpread>(id, delay, comp => comp.Spreads.Count != 0, "Spread markers appear");
        ComponentCondition<LancingBlowSpread>(id + 0x10u, 4.7f, comp => comp.Spreads.Count == 0, "Adds spawn");
        ComponentCondition<LancingBlowAOE>(id + 0x20u, 1, comp => comp.NumCasts != 0, "AOEs around adds");
        Targetable(id + 0x30u, false, 2);
        Targetable(id + 0x40u, true, 4);
        CastStart(id + 0x50u, (uint)AID.UltimateZantetsuken, 1.6f, "Enrage start");
        CastEnd(id + 0x60u, 18f, "Enrage")
            .ResetComp<LancingBlowSpread>();
    }

    private void BoomingLamentCloudToGround(uint id, float delay)
    {
        Cast(id, (uint)AID.BoomingLament, delay, 4f, "Circle AOE");
        ComponentCondition<CloudToGround>(id + 0x10u, 8f, comp => comp.Lines.Count != 0, "Exaflares start");
        ComponentCondition<SilentLevin>(id + 0x20u, 15f, comp => comp.Casters.Count != 0, "Baited circles 1");
        ComponentCondition<BitterBarbs>(id + 0x30u, 0.5f, comp => comp.TethersAssigned, "Chains");
        ComponentCondition<SilentLevin>(id + 0x40u, 1.2f, comp => comp.Casters.Count > 3, "Baited circles 2");
        ComponentCondition<SilentLevin>(id + 0x50u, 2f, comp => comp.NumCasts >= 3 && comp.Casters.Count > 3, "Baited circles 3");
        ComponentCondition<CloudToGround>(id + 0x60u, 6.2f, comp => comp.Lines.Count == 0, "Exaflares end")
            .ResetComp<SilentLevin>()
            .ResetComp<BitterBarbs>();
    }

    private void CloudToGroundSilentLevin(uint id, float delay)
    {
        ComponentCondition<CloudToGround>(id, delay, comp => comp.Lines.Count != 0, "Exaflares start");
        ComponentCondition<SilentLevin>(id + 0x10u, 15f, comp => comp.Casters.Count != 0, "Baited circles 1");
        ComponentCondition<BitterBarbs>(id + 0x20u, 0.5f, comp => comp.TethersAssigned, "Chains");
        ComponentCondition<SilentLevin>(id + 0x30u, 1.2f, comp => comp.Casters.Count > 3, "Baited circles 2");
        ComponentCondition<SilentLevin>(id + 0x40u, 2f, comp => comp.NumCasts >= 3 && comp.Casters.Count > 3, "Baited circles 3");
        ComponentCondition<CloudToGround>(id + 0x50u, 6.2f, comp => comp.Lines.Count == 0, "Exaflares end")
            .ResetComp<SilentLevin>()
            .ResetComp<BitterBarbs>();
    }

    private void CloudToGroundLevinWhorl(uint id, float delay)
    {
        ComponentCondition<CloudToGround>(id, delay, comp => comp.Lines.Count != 0, "Exaflares start");
        Cast(id + 0x10u, (uint)AID.LevinwhorlVisual, 6.3f, 8f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CloudToGround>(id + 0x20u, 13.4f, comp => comp.Lines.Count == 0, "Exaflares end");
    }

    private void SilentLevinBoomingLament(uint id, float delay)
    {
        ComponentCondition<SilentLevin>(id, delay, comp => comp.Casters.Count != 0, "Baited circles 1");
        ComponentCondition<SilentLevin>(id + 0x10u, 1.1f, comp => comp.Casters.Count > 3, "Baited circles 2");
        ComponentCondition<SilentLevin>(id + 0x20u, 2.1f, comp => comp.Casters.Count > 6 || comp.NumCasts >= 3 && comp.Casters.Count > 3, "Baited circles 3");
        ComponentCondition<SilentLevin>(id + 0x30u, 2.7f, comp => comp.Casters.Count == 0, "Baits end")
            .ResetComp<SilentLevin>();
    }

    private void ForHonor(uint id, float delay)
    {
        Cast(id, (uint)AID.ForHonor, delay, 4.5f, "Circle AOE");
    }

    private void WhirlingZantetsukenSilentLevin(uint id, float delay)
    {
        CastStart(id, (uint)AID.WhirlingZantetsuken, delay, "Donut AOE");
        ComponentCondition<SilentLevin>(id + 0x10u, 5.1f, comp => comp.Casters.Count != 0, "Baited circles 1");
        CastEnd(id + 0x20u, 0.4f, "Donut AOE");
        ComponentCondition<SilentLevin>(id + 0x30u, 1.6f, comp => comp.Casters.Count > 3, "Baited circles 2");
        ComponentCondition<SilentLevin>(id + 0x40u, 2.5f, comp => comp.Casters.Count > 6 || comp.NumCasts >= 3 && comp.Casters.Count > 3, "Baited circles 3");
        Cast(id + 0x50u, (uint)AID.BoomingLament, 1.1f, 4f, "Circle AOE")
            .ResetComp<SilentLevin>();
    }
}
