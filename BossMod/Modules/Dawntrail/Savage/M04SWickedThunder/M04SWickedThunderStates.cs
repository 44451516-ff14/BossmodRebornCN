namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class M04SWickedThunderStates : StateMachineBuilder
{
    private readonly M04SWickedThunder _module;

    public M04SWickedThunderStates(M04SWickedThunder module) : base(module)
    {
        _module = module;
        SimplePhase(default, SinglePhase, "Single phase")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void SinglePhase(uint id)
    {
        WrathOfZeus(id, 10.2f);
        BewitchingFlight(id + 0x10000u, 16.7f);
        ElectrifyingWitchHunt(id + 0x20000u, 4.6f);
        WideningNarrowingWitchHunt(id + 0x30000u, 2.7f);
        WrathOfZeus(id + 0x40000u, 7.7f);
        ElectropeEdge(id + 0x50000u, 8.4f);
        WickedJolt(id + 0x60000u, 3.2f);
        LightningCage(id + 0x70000u, 8.4f);
        WickedBolt(id + 0x80000u, 2.0f);
        IonCluster(id + 0x90000u, 4.2f);
        WickedJolt(id + 0xA0000u, 0.7f);
        ElectropeTransplant(id + 0xB0000u, 17.8f);
        PhaseTransition(id + 0xC0000u, 10.4f);

        Sabertail(id + 0x100000u, 1.0f);
        WickedSpecial(id + 0x110000u, 0.6f);
        MustardBomb(id + 0x120000u, 7.2f);
        AetherialConversion(id + 0x130000u, 0.6f);
        AzureThunder(id + 0x140000u, 12.2f);
        TwilightSabbath(id + 0x150000u, 3.2f);
        MidnightSabbath(id + 0x160000u, 7.3f);
        WickedThunder(id + 0x170000u, 2.2f);
        FlameSlash(id + 0x180000u, 4.2f);
        MustardBomb(id + 0x190000u, 3.1f);
        SunriseSabbath(id + 0x1A0000u, 0.7f);
        SwordQuiver(id + 0x1B0000u, 6.2f);
        SwordQuiver(id + 0x1C0000u, 3.1f);
        SwordQuiver(id + 0x1D0000u, 3.1f);
        ActorCast(id + 0x1E0000u, _module.BossP2, AID.Enrage, 9f, 10f, true, "Enrage");
    }

    private void WrathOfZeus(uint id, float delay)
    {
        Cast(id, AID.WrathOfZeus, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WickedJolt(uint id, float delay)
    {
        Cast(id, AID.WickedJolt, delay, 5f)
            .ActivateOnEnter<WickedJolt>();
        ComponentCondition<WickedJolt>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<WickedJolt>(id + 3u, 3.2f, static comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<WickedJolt>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BewitchingFlight(uint id, float delay)
    {
        CastMulti(id, [AID.BewitchingFlightR, AID.BewitchingFlightL], delay, 6f)
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Electray>();
        ComponentCondition<BewitchingFlight>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<BewitchingFlight>()
            .DeactivateOnExit<Electray>();
    }

    private void ElectrifyingWitchHunt(uint id, float delay)
    {
        ComponentCondition<ElectrifyingWitchHuntBurst>(id, delay, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ElectrifyingWitchHuntBurst>();
        Cast(id + 0x10u, AID.ElectrifyingWitchHunt, 1f, 5f, "Center/sides")
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<ElectrifyingWitchHuntSpread>()
            .DeactivateOnExit<ElectrifyingWitchHuntBurst>()
            .DeactivateOnExit<Electray>();
        ComponentCondition<ElectrifyingWitchHuntSpread>(id + 0x20u, 0.1f, static comp => comp.Spreads.Count == 0, "Spread")
            .DeactivateOnExit<ElectrifyingWitchHuntSpread>();

        ComponentCondition<ElectrifyingWitchHuntBurst>(id + 0x100u, 2.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ElectrifyingWitchHuntBurst>();
        Cast(id + 0x110u, AID.WitchHunt, 0.9f, 5f)
            .ActivateOnEnter<ElectrifyingWitchHuntResolve>();
        ComponentCondition<ElectrifyingWitchHuntBurst>(id + 0x120u, 0.1f, static comp => comp.NumCasts > 0, "Sides/center")
            .DeactivateOnExit<ElectrifyingWitchHuntBurst>();
        ComponentCondition<ElectrifyingWitchHuntResolve>(id + 0x121u, 0.4f, static comp => comp.CurMechanic == ElectrifyingWitchHuntResolve.Mechanic.None && comp.ForbidBait.None(), "Spread")
            .DeactivateOnExit<ElectrifyingWitchHuntResolve>();
    }

    private void WideningNarrowingWitchHunt(uint id, float delay)
    {
        CastMulti(id, [AID.WideningWitchHunt, AID.NarrowingWitchHunt], delay, 14f)
            .ActivateOnEnter<WideningNarrowingWitchHunt>()
            .ActivateOnEnter<WideningNarrowingWitchHuntBait>();
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x10u, 1.1f, static comp => comp.NumCasts >= 1, "In/out + Bait 1"); // note: bait happens ~0.1s after in/out, but it's unreliable when people are dead
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x20u, 3.6f, static comp => comp.NumCasts >= 2, "Out/in + Bait 2");
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x30u, 3.6f, static comp => comp.NumCasts >= 3, "In/out + Bait 3");
        ComponentCondition<WideningNarrowingWitchHunt>(id + 0x40u, 3.6f, static comp => comp.NumCasts >= 4, "Out/in + Bait 4")
            .DeactivateOnExit<WideningNarrowingWitchHunt>()
            .DeactivateOnExit<WideningNarrowingWitchHuntBait>();
    }

    private void ElectropeEdge(uint id, float delay)
    {
        Cast(id, AID.ElectropeEdge, delay, 3f);
        Cast(id + 0x10u, AID.ElectropeEdgeWitchgleam, 4.2f, 3)
            .ActivateOnEnter<ElectropeEdgeWitchgleam>();
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x12u, 1.2f, static comp => comp.NumCasts >= 2, "Lines");
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x13u, 1.6f, static comp => comp.NumCasts >= 4);
        ComponentCondition<ElectropeEdgeWitchgleam>(id + 0x14u, 1.6f, static comp => comp.NumCasts >= 6)
            .DeactivateOnExit<ElectropeEdgeWitchgleam>();
        Cast(id + 0x20u, AID.SymphonyFantastique, 0.4f, 3);
        CastMulti(id + 0x30u, [AID.ElectropeEdgeSidewiseSparkR, AID.ElectropeEdgeSidewiseSparkL], 3.2f, 7f, "Corner + Pairs/Spread") // everything resolves within 0.1s of each other
            .ActivateOnEnter<ElectropeEdgeSpark1>()
            .ActivateOnEnter<ElectropeEdgeSpark2>()
            .ActivateOnEnter<ElectropeEdgeSidewiseSpark>()
            .ActivateOnEnter<ElectropeEdgeStar>()
            .DeactivateOnExit<ElectropeEdgeSpark1>()
            .DeactivateOnExit<ElectropeEdgeSpark2>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSpark>()
            .DeactivateOnExit<ElectropeEdgeStar>();
    }

    private void LightningCage(uint id, float delay)
    {
        Cast(id, AID.ElectropeEdge, delay, 3f)
            .ActivateOnEnter<LightningCage>(); // statuses appear ~1.1s after cast ends
        Cast(id + 0x10u, AID.LightningCageWitchgleam, 3.2f, 3)
            .ActivateOnEnter<LightningCageWitchgleam>();
        ComponentCondition<LightningCageWitchgleam>(id + 0x12u, 1.2f, static comp => comp.NumCasts > 0, "Proteans");
        ComponentCondition<LightningCageWitchgleam>(id + 0x13u, 1.6f, static comp => comp.NumCasts > 4);
        ComponentCondition<LightningCageWitchgleam>(id + 0x14u, 1.6f, static comp => comp.NumCasts > 8);
        ComponentCondition<LightningCageWitchgleam>(id + 0x15u, 1.6f, static comp => comp.NumCasts > 12)
            .DeactivateOnExit<LightningCageWitchgleam>();
        Cast(id + 0x20u, AID.LightningCage, 0.4f, 3f);
        ComponentCondition<LightningCage>(id + 0x30u, 1.0f, static comp => comp.Active);
        ComponentCondition<LightningCage>(id + 0x31u, 6.7f, static comp => comp.NumSparks > 0);
        ComponentCondition<LightningCage>(id + 0x32u, 0.3f, static comp => comp.NumCasts > 0, "Anchor 1");
        CastMulti(id + 0x40u, [AID.ElectropeEdgeSidewiseSparkR, AID.ElectropeEdgeSidewiseSparkL], 2.2f, 7, "Side + Pairs/Spread") // everything resolves within 0.1s of each other
            .ActivateOnEnter<ElectropeEdgeSidewiseSpark>()
            .ActivateOnEnter<ElectropeEdgeStar>()
            .DeactivateOnExit<ElectropeEdgeSidewiseSpark>()
            .DeactivateOnExit<ElectropeEdgeStar>();
        ComponentCondition<LightningCage>(id + 0x50u, 4.2f, static comp => comp.Active);
        ComponentCondition<LightningCage>(id + 0x51u, 6.4f, static comp => comp.NumSparks > 4);
        ComponentCondition<LightningCage>(id + 0x52u, 0.6f, static comp => comp.NumCasts > 12, "Anchor 2")
            .DeactivateOnExit<LightningCage>();
    }

    private void WickedBolt(uint id, float delay)
    {
        CastStart(id, AID.WickedBolt, delay)
            .ActivateOnEnter<WickedBolt>(); // icon appears ~0.1s before cast start
        CastEnd(id + 1u, 4f);
        ComponentCondition<WickedBolt>(id + 0x10u, 1.1f, static comp => comp.NumFinishedStacks >= 1, "Stack 1");
        ComponentCondition<WickedBolt>(id + 0x11u, 1f, static comp => comp.NumFinishedStacks >= 2);
        ComponentCondition<WickedBolt>(id + 0x12u, 1f, static comp => comp.NumFinishedStacks >= 3);
        ComponentCondition<WickedBolt>(id + 0x13u, 1f, static comp => comp.NumFinishedStacks >= 4);
        ComponentCondition<WickedBolt>(id + 0x14u, 1f, static comp => comp.NumFinishedStacks >= 5, "Stack 5")
            .DeactivateOnExit<WickedBolt>();
    }

    private void ElectronStream(uint id, float delay, int count)
    {
        CastMulti(id, [AID.ElectronStream1, AID.ElectronStream2], delay, 6f, $"Side {count}")
            .ActivateOnEnter<ElectronStream>()
            .DeactivateOnExit<ElectronStream>();
        ComponentCondition<ElectronStreamCurrent>(id + 2u, 5.1f, static comp => comp.NumCasts > 0, $"Debuffs {count}", checkDelay: 5f) // if proximity debuff holder dies, everything explodes early
            .ActivateOnEnter<ElectronStreamCurrent>()
            .DeactivateOnExit<ElectronStreamCurrent>();
    }

    private void IonCluster(uint id, float delay)
    {
        Cast(id, AID.IonCluster, delay, 3f);
        ComponentCondition<StampedingThunder>(id + 0x10u, 11.7f, static comp => comp.AOE.Length != 0)
            .ActivateOnEnter<StampedingThunder>();
        ComponentCondition<StampedingThunder>(id + 0x11u, 2.4f, static comp => comp.NumCasts >= 1, "Cannon start");
        ComponentCondition<StampedingThunder>(id + 0x12u, 1.1f, static comp => comp.NumCasts >= 2);
        ComponentCondition<StampedingThunder>(id + 0x13u, 1.1f, static comp => comp.NumCasts >= 3);
        ComponentCondition<StampedingThunder>(id + 0x14u, 1.1f, static comp => comp.NumCasts >= 4);
        ComponentCondition<StampedingThunder>(id + 0x15u, 1.1f, static comp => comp.NumCasts >= 5);
        ComponentCondition<StampedingThunder>(id + 0x20u, 2.7f, static comp => comp.SmallArena, "Destroy platform");

        ElectronStream(id + 0x100u, 4.2f, 1);
        ElectronStream(id + 0x200u, 2.1f, 2);
        ElectronStream(id + 0x300u, 2.1f, 3);

        ComponentCondition<StampedingThunder>(id + 0x400u, 2.5f, static comp => !comp.SmallArena, "Restore platform")
            .DeactivateOnExit<StampedingThunder>();
    }

    private void FulminousField(uint id, float delay)
    {
        ComponentCondition<FulminousField>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<FulminousField>();
        ComponentCondition<FulminousField>(id + 1u, 3f, static comp => comp.NumCasts > 0, "Cones start");
        ComponentCondition<FulminousField>(id + 2u, 3f, static comp => comp.NumCasts > 8)
            .ActivateOnEnter<ConductionPoint>();
        ComponentCondition<FulminousField>(id + 3u, 3f, static comp => comp.NumCasts > 16);
        ComponentCondition<FulminousField>(id + 4u, 3f, static comp => comp.NumCasts > 24);
        ComponentCondition<FulminousField>(id + 5u, 3f, static comp => comp.NumCasts > 32, "Cones 5 + Spread")
            .ActivateOnEnter<ForkedFissures>()
            .DeactivateOnExit<ConductionPoint>();
        ComponentCondition<FulminousField>(id + 6u, 3f, static comp => comp.NumCasts > 40, "Cones 6 + Charges")
            .DeactivateOnExit<ForkedFissures>();
        ComponentCondition<FulminousField>(id + 7u, 3f, static comp => comp.NumCasts > 48, "Cones end")
            .DeactivateOnExit<FulminousField>();
    }

    private void ElectropeTransplant(uint id, float delay)
    {
        Cast(id, AID.ElectropeTransplant, delay, 4f);
        FulminousField(id + 0x100u, 4.3f);
        FulminousField(id + 0x200u, 5f);
    }

    private void PhaseTransition(uint id, float delay)
    {
        ComponentCondition<Soulshock>(id, delay, static comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<Soulshock>()
            .ActivateOnEnter<CannonboltKB>()
            .OnEnter(() => Module.Arena.Bounds = M04SWickedThunder.GetTransitionBounds())
            .OnEnter(() => Module.Arena.Center = new(100f, 130f))
            .DeactivateOnExit<Soulshock>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Impact>(id + 1u, 3.3f, static comp => comp.NumCasts > 0, "Raidwide 2")
            .ActivateOnEnter<Impact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Impact>(id + 2u, 2.6f, static comp => comp.NumCasts > 1, "Raidwide 3")
            .DeactivateOnExit<Impact>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Cannonbolt>(id + 3u, 2.6f, static comp => comp.NumCasts > 0, "Raidwide 4")
            .ActivateOnEnter<Cannonbolt>()
            .DeactivateOnExit<Cannonbolt>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Targetable(id + 0x10u, false, 0.1f, "Boss disappears");
        ActorTargetable(id + 0x20u, _module.BossP2, true, 11.9f, "Boss reappears")
            .DeactivateOnEnter<CannonboltKB>()
            .OnEnter(() => Module.Arena.Bounds = new ArenaBoundsRect(20f, 15f))
            .OnEnter(() => Module.Arena.Center = new(100f, 165f))
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x30u, _module.BossP2, AID.CrossTailSwitch, 7.2f, 5f, true);
        ComponentCondition<CrossTailSwitch>(id + 0x40, 1.2f, static comp => comp.NumCasts > 0, "Multi-hit raidwide 1")
            .ActivateOnEnter<CrossTailSwitch>()
            .DeactivateOnExit<CrossTailSwitch>() // 8 hits every second, then different hit
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CrossTailSwitchLast>(id + 0x50, 8.2f, static comp => comp.NumCasts > 0, "Multi-hit raidwide 9")
            .ActivateOnEnter<CrossTailSwitchLast>()
            .DeactivateOnExit<CrossTailSwitchLast>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void AzureThunder(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.AzureThunder, delay, 5f, true, "Raidwide")
            .OnExit(() => Module.Arena.Bounds = M04SWickedThunder.GetP2CircleBounds()) // at the end of the cast arena changes to circle
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WickedThunder(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.WickedThunder, delay, 5f, true, "Raidwide")
            .OnEnter(() => Module.Arena.Bounds = new ArenaBoundsRect(20f, 15f)) // at the beginning of the cast arena changes back to square
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Sabertail(uint id, float delay)
    {
        ComponentCondition<Sabertail>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<Sabertail>()
            .ActivateOnEnter<WickedBlaze>();
        ActorCast(id + 0x10u, _module.BossP2, AID.WickedBlaze, 2.8f, 5f, true);
        ComponentCondition<Sabertail>(id + 0x20u, 0.2f, static comp => comp.NumCasts > 0, "Exaflares + Stacks"); // first stacks resolve ~0.1s earlier
        ComponentCondition<Sabertail>(id + 0x30u, 3.4f, static comp => comp.NumCasts > 60, "Exaflares resolve") // 16+14+10+10+10+6 hits
            .DeactivateOnExit<Sabertail>();
        ComponentCondition<WickedBlaze>(id + 0x40u, 1.0f, static comp => comp.NumFinishedStacks > 3, "Stacks resolve")
            .DeactivateOnExit<WickedBlaze>();
    }

    private void WickedSpecial(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], delay, 5f, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        Condition(id + 2u, 1f, () => Module.FindComponent<WickedSpecialCenter>()?.NumCasts > 0 || Module.FindComponent<WickedSpecialSides>()?.NumCasts > 0, "Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>()
            .DeactivateOnExit<WickedSpecialSides>();
    }

    private void MustardBomb(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MustardBomb, delay, 8f, true)
            .ActivateOnEnter<MustardBomb>();
        ComponentCondition<MustardBomb>(id + 2u, 0.8f, static comp => comp.CurMechanic > Savage.M04SWickedThunder.MustardBomb.Mechanic.Tethers, "Spread");
        ComponentCondition<MustardBomb>(id + 3u, 9.7f, static comp => comp.CurMechanic > Savage.M04SWickedThunder.MustardBomb.Mechanic.Nisi, "Nisi")
            .DeactivateOnExit<MustardBomb>();
    }

    private void AetherialConversionResolve(uint id, float delay, bool activate)
    {
        ActorCastStartMulti(id, _module.BossP2, [AID.TailThrust1HitL, AID.TailThrust1KnockbackL, AID.TailThrust1HitR, AID.TailThrust1KnockbackR], delay, true)
            .ActivateOnEnter<AetherialConversionSwitchOfTides>(activate)
            .ActivateOnEnter<AetherialConversionTailThrust>(activate);
        ActorCastEnd(id + 1u, _module.BossP2, 5f, true);
        ComponentCondition<AetherialConversion>(id + 2u, 1.1f, static comp => comp.NumCasts > 0, "AOE/Knockback L/R");
        ComponentCondition<AetherialConversion>(id + 3u, 4.1f, static comp => comp.NumCasts > 1, "AOE/Knockback R/L")
            .DeactivateOnExit<AetherialConversionSwitchOfTides>()
            .DeactivateOnExit<AetherialConversionTailThrust>()
            .DeactivateOnExit<AetherialConversion>();
    }

    private void AetherialConversion(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7f, true)
            .ActivateOnEnter<AetherialConversion>()
            .ActivateOnEnter<AetherialConversionTailThrust>()
            .ActivateOnEnter<AetherialConversionSwitchOfTides>();
        AetherialConversionResolve(id + 0x10u, 3.2f, false);
    }

    private void TwilightSabbath(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.TwilightSabbath, delay, 3f, true);
        ComponentCondition<TwilightSabbath>(id + 2u, 3.2f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TwilightSabbath>();
        ActorCast(id + 0x10u, _module.BossP2, AID.WickedFire, 1.0f, 4f, true);
        ComponentCondition<WickedFire>(id + 0x12u, 0.1f, static comp => comp.Casters.Count > 0, "Puddle bait")
            .ActivateOnEnter<WickedFire>();
        ComponentCondition<TwilightSabbath>(id + 0x20u, 3.0f, static comp => comp.NumCasts > 0, "Cleaves 1");
        ComponentCondition<WickedFire>(id + 0x30u, 1.0f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<WickedFire>();
        ActorCastMulti(id + 0x40u, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], 1.1f, 5f, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        ComponentCondition<TwilightSabbath>(id + 0x50u, 1f, static comp => comp.NumCasts > 2, "Cleaves 2 + Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>() // resolves at the same time
            .DeactivateOnExit<WickedSpecialSides>()
            .DeactivateOnExit<TwilightSabbath>();
    }

    private void MidnightSabbath(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.MidnightSabbath, delay, 3f, true);
        ComponentCondition<MidnightSabbath>(id + 2u, 3.2f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<MidnightSabbath>();
        ActorCastMulti(id + 0x10u, _module.BossP2, [AID.ConcentratedBurst, AID.ScatteredBurst], 1.0f, 7f, true)
            .ActivateOnEnter<ConcentratedScatteredBurst>();
        ComponentCondition<MidnightSabbath>(id + 0x20u, 0.1f, static comp => comp.NumCasts > 0, "Lines/Donuts + Spread/pairs 1"); // spread/stack resolves almost at the same time
        ActorCastStartMulti(id + 0x30u, _module.BossP2, [AID.WickedSpecialCenter, AID.WickedSpecialSides], 3.1f, true);
        ComponentCondition<MidnightSabbath>(id + 0x31u, 0.9f, static comp => comp.NumCasts > 4, "Lines/Donuts + Pairs/spread 2") // spread/stack resolves almost at the same time
            .DeactivateOnExit<ConcentratedScatteredBurst>()
            .DeactivateOnExit<MidnightSabbath>();
        ActorCastEnd(id + 0x32u, _module.BossP2, 4.1f, true)
            .ActivateOnEnter<WickedSpecialCenter>()
            .ActivateOnEnter<WickedSpecialSides>();
        Condition(id + 0x33u, 1f, () => Module.FindComponent<WickedSpecialCenter>()?.NumCasts > 0 || Module.FindComponent<WickedSpecialSides>()?.NumCasts > 0, "Center/Sides")
            .DeactivateOnExit<WickedSpecialCenter>()
            .DeactivateOnExit<WickedSpecialSides>();
    }

    private void FlameSlash(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7, true)
            .ActivateOnEnter<AetherialConversion>();
        ActorCast(id + 0x10u, _module.BossP2, AID.FlameSlash, 3.1f, 6f, true)
            .ActivateOnEnter<FlameSlash>();
        ComponentCondition<FlameSlash>(id + 0x12u, 1f, static comp => comp.NumCasts > 0, "Destroy center")
            .OnExit(() => Module.Arena.Bounds = M04SWickedThunder.GetP2TowersBounds());
        ActorCast(id + 0x20u, _module.BossP2, AID.RainingSwords, 2.2f, 2, true)
            .ActivateOnEnter<RainingSwords>();
        ComponentCondition<RainingSwords>(id + 0x22, 1f, static comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<RainingSwords>();
        ActorCast(id + 0x30u, _module.BossP2, AID.ChainLightning, 4.2f, 16f, true)
            .ActivateOnEnter<ChainLightning>();
        ComponentCondition<ChainLightning>(id + 0x40u, 0.8f, static comp => comp.NumCasts >= 3, "Lightning start");
        ComponentCondition<ChainLightning>(id + 0x41u, 2.7f, static comp => comp.NumCasts >= 6);
        ComponentCondition<ChainLightning>(id + 0x42u, 2.7f, static comp => comp.NumCasts >= 9);
        ComponentCondition<ChainLightning>(id + 0x43u, 2.7f, static comp => comp.NumCasts >= 12);
        ComponentCondition<ChainLightning>(id + 0x44u, 2.7f, static comp => comp.NumCasts >= 15);
        ComponentCondition<ChainLightning>(id + 0x45u, 2.7f, static comp => comp.NumCasts >= 18);
        ComponentCondition<ChainLightning>(id + 0x46u, 2.7f, static comp => comp.NumCasts >= 21);
        ComponentCondition<ChainLightning>(id + 0x47u, 2.7f, static comp => comp.NumCasts >= 24)
            .DeactivateOnExit<ChainLightning>();
        ComponentCondition<FlameSlash>(id + 0x50u, 1, static comp => !comp.SmallArena, "Restore center")
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsRect(20f, 15f))
            .DeactivateOnExit<FlameSlash>();
        AetherialConversionResolve(id + 0x60u, 0.4f, true);
    }

    private void SunriseSabbath(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.AetherialConversionHitLR, AID.AetherialConversionKnockbackLR, AID.AetherialConversionHitRL, AID.AetherialConversionKnockbackRL], delay, 7f, true)
            .ActivateOnEnter<AetherialConversion>();
        AzureThunder(id + 0x10u, 3.2f);
        ActorCast(id + 0x20u, _module.BossP2, AID.SunriseSabbathIonCluster, 3.2f, 3f, true)
            .ActivateOnEnter<SunriseSabbath>(); // buffs appear ~0.8s after cast end
        ActorCast(id + 0x30u, _module.BossP2, AID.SunriseSabbath, 3.2f, 3f, true);
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x40u, 3.2f, static comp => comp.Towers.Count > 0)
            .ActivateOnEnter<SunriseSabbathSoaringSoulpress>();
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x41u, 3.1f, static comp => comp.Cannons.Count > 0)
            .ActivateOnEnter<SunriseSabbathElectronStream>();
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x50u, 7.1f, static comp => comp.NumCasts > 0, "Towers 1");
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x51u, 0.5f, static comp => comp.NumCasts > 0, "Baits 1");
        WickedSpecial(id + 0x60, 1.4f);
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x70u, 1.7f, static comp => comp.Cannons.Count > 0);
        ComponentCondition<SunriseSabbathSoaringSoulpress>(id + 0x80u, 7.1f, static comp => comp.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<SunriseSabbathSoaringSoulpress>();
        ComponentCondition<SunriseSabbathElectronStream>(id + 0x81u, 0.5f, static comp => comp.NumCasts > 4, "Baits 2")
            .DeactivateOnExit<SunriseSabbathElectronStream>()
            .DeactivateOnExit<SunriseSabbath>()
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsRect(20f, 15f)); // restore bounds (set to circle by azure thunder)
        AetherialConversionResolve(id + 0x90u, 0.9f, true);
    }

    private void SwordQuiver(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.SwordQuiverN, AID.SwordQuiverC, AID.SwordQuiverS], delay, 5f, true)
            .ActivateOnEnter<SwordQuiverBurst>()
            .ActivateOnEnter<SwordQuiverLaceration>();
        ComponentCondition<SwordQuiverRaidwide>(id + 0x10u, 1.4f, static comp => comp.NumCasts >= 1)
            .ActivateOnEnter<SwordQuiverRaidwide>();
        ComponentCondition<SwordQuiverRaidwide>(id + 0x11u, 1.0f, static comp => comp.NumCasts >= 2);
        ComponentCondition<SwordQuiverRaidwide>(id + 0x12u, 1.0f, static comp => comp.NumCasts >= 3);
        ComponentCondition<SwordQuiverRaidwide>(id + 0x13u, 1.2f, static comp => comp.NumCasts >= 4)
            .DeactivateOnExit<SwordQuiverRaidwide>();
        ComponentCondition<SwordQuiverBurst>(id + 0x20u, 4.3f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<SwordQuiverBurst>();
        ComponentCondition<SwordQuiverLaceration>(id + 0x21u, 0.2f, static comp => comp.NumCasts > 0, "Swords")
            .DeactivateOnExit<SwordQuiverLaceration>();
    }
}
