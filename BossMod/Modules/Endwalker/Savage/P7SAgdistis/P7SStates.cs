namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class P7SStates : StateMachineBuilder
{
    public P7SStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<Border>();
    }

    private void SinglePhase(uint id)
    {
        SparkOfLife(id, 7.3f);
        DispersedCondensedAero(id + 0x10000u, 6.3f);
        BladesOfAttisImmortalsObol(id + 0x20000u, 3.8f);
        ForbiddenFruit1(id + 0x30000u, 12.5f);
        DispersedCondensedAero(id + 0x40000u, 3.9f);
        SparkOfLife(id + 0x50000u, 4.7f);
        InviolateBonds(id + 0x60000u, 5.3f);
        RootsOfAttis(id + 0x70000u, 2.7f, "N bridge disappear");
        DispersedCondensedAero(id + 0x80000u, 11.5f);
        ForbiddenFruit2(id + 0x90000u, 3.6f);
        RootsOfAttis(id + 0xA0000u, 4.3f, "All bridges disappear");
        ForbiddenFruit3(id + 0xB0000u, 7.4f);
        BoughOfAttisFrontSide(id + 0xC0000u, 10.4f);
        DispersedCondensedAero(id + 0xD0000u, 2.2f);
        ForbiddenFruit4(id + 0xE0000u, 2.6f);
        BladesOfAttisMulticast(id + 0xF0000u, 4.8f);
        DispersedCondensedAero(id + 0x100000u, 7.4f, true);
        ForbiddenFruit5(id + 0x110000u, 4.6f);
        SparkOfLife(id + 0x120000u, 3.5f);
        ImmortalsObol(id + 0x130000u, 7.4f);
        ForbiddenFruit6(id + 0x140000u, 5.2f);
        ForbiddenFruit7(id + 0x150000u, 4.8f);
        FaminesHarvest(id + 0x160000u, 2.9f);
        DeathsHarvest(id + 0x170000u, 7.8f);
        WarsHarvest(id + 0x180000u, 7.8f);
        SparkOfLife(id + 0x190000u, 8f);
        BoughOfAttisFrontSideHemitheosHoly(id + 0x1A0000u, 7.3f);
        SparkOfLife(id + 0x1B0000u, 3.2f);
        SparkOfLife(id + 0x1C0000u, 8.3f);
        Cast(id + 0x1D0000u, AID.Enrage, 7.3f, 10f, "Enrage");
    }

    private void SparkOfLife(uint id, float delay)
    {
        Cast(id, AID.SparkOfLife, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DispersedCondensedAero(uint id, float delay, bool duringBladesOfAttis = false)
    {
        CastMulti(id, [AID.DispersedAero, AID.CondensedAero], delay, 7f)
            .ActivateOnEnter<DispersedCondensedAero>()
            .DeactivateOnExit<BladesOfAttis>(duringBladesOfAttis)
            .SetHint(StateMachine.StateHint.PositioningEnd, duringBladesOfAttis);
        ComponentCondition<DispersedCondensedAero>(id + 2u, 1.1f, static comp => comp.Done, "Tankbuster")
            .DeactivateOnExit<DispersedCondensedAero>();
    }

    private void ImmortalsObol(uint id, float delay)
    {
        Cast(id, AID.ImmortalsObol, delay, 5.5f, "Destroy platform");
    }

    private void RootsOfAttis(uint id, float delay, string name)
    {
        Cast(id, AID.RootsOfAttis, delay, 3, name);
    }

    private void BoughOfAttisFrontSide(uint id, float delay, bool withHoly = false)
    {
        Cast(id, AID.BoughOfAttisFront, delay, 5.8f)
            .ActivateOnEnter<BoughOfAttisFront>();
        ComponentCondition<BoughOfAttisFront>(id + 0x2, 1.2f, static comp => comp.NumCasts > 0, withHoly ? "Front hit + stacks" : "Front hit")
            .DeactivateOnExit<BoughOfAttisFront>()
            .DeactivateOnExit<HemitheosHoly>(withHoly);
        CastMulti(id + 0x10, [AID.BoughOfAttisSideW, AID.BoughOfAttisSideE], 2.4f, 4)
            .ActivateOnEnter<BoughOfAttisSide>();
        ComponentCondition<BoughOfAttisSide>(id + 0x12, 1, static comp => comp.NumCasts > 0, "Side hit")
            .DeactivateOnExit<BoughOfAttisSide>();
    }

    private void BoughOfAttisFrontSideHemitheosHoly(uint id, float delay)
    {
        Cast(id, AID.HemitheosHoly, delay, 3)
            .ActivateOnEnter<HemitheosHoly>();
        BoughOfAttisFrontSide(id + 0x100, 3.2f, true);
    }

    // leaves component active
    private void BladesOfAttisImmortalsObol(uint id, float delay)
    {
        Cast(id, AID.BladesOfAttis, delay, 3)
            .ActivateOnEnter<BladesOfAttis>();
        ImmortalsObol(id + 0x10, 3.2f);
    }

    // leaves component & pos flag active
    private void BladesOfAttisMulticast(uint id, float delay)
    {
        Cast(id, AID.BladesOfAttis, delay, 3f)
            .ActivateOnEnter<BladesOfAttis>();
        Cast(id + 0x10u, AID.Multicast, 3.2f, 3f);
        ComponentCondition<HemitheosHoly>(id + 0x20u, 2.7f, static comp => comp.Active)
            .ActivateOnEnter<HemitheosAeroKnockback2>()
            .ActivateOnEnter<HemitheosHoly>();
        ComponentCondition<HemitheosAeroKnockback2>(id + 0x21u, 3.3f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<HemitheosAeroKnockback2>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<HemitheosHoly>(id + 0x22u, 2.6f, static comp => !comp.Active, "Stacks")
            .DeactivateOnExit<HemitheosHoly>();
    }

    private State ForbiddenFruitHarvestStart(uint id, float delay, AID cast = AID.ForbiddenFruit)
    {
        Cast(id, cast, delay, 4f);
        return Cast(id + 2u, AID.ForbiddenFruitInvis, 2.1f, 3f);
    }

    // expects blades of attis component to be active from previous state
    private void ForbiddenFruit1(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit1>()
            .DeactivateOnExit<BladesOfAttis>();
        Cast(id + 0x10u, AID.HemitheosHoly, 3.3f, 3f);
        CastStart(id + 0x20u, AID.BoughOfAttisBack, 3.2f); // at the same time HemitheosHolyAOE and BoughOfAttisBackAOE cast starts happen
        ComponentCondition<ForbiddenFruit1>(id + 0x21u, 3.6f, static comp => comp.CastsActive)
            .ActivateOnEnter<HemitheosHoly>()
            .ActivateOnEnter<BoughOfAttisBack>();
        ComponentCondition<HemitheosHoly>(id + 0x22u, 2.4f, static comp => !comp.Active, "Stacks")
            .DeactivateOnExit<HemitheosHoly>();
        CastEnd(id + 0x23u, 0.2f);
        ComponentCondition<ForbiddenFruit1>(id + 0x24u, 0.4f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<ForbiddenFruit1>();
        ComponentCondition<BoughOfAttisBack>(id + 0x25u, 0.5f, static comp => comp.NumCasts > 0, "Fruit 1 (2 birds + bull + stacks) resolve")
            .DeactivateOnExit<BoughOfAttisBack>();
    }

    private void InviolateBonds(uint id, float delay)
    {
        Cast(id, AID.InviolateBonds, delay, 4f);
        ComponentCondition<WindsHoly>(id + 2, 1, static comp => comp.Active)
            .ActivateOnEnter<WindsHoly>();
        CastStart(id + 0x10u, AID.BoughOfAttisFront, 3.2f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x11u, 5.8f)
            .ActivateOnEnter<BoughOfAttisFront>();
        ComponentCondition<WindsHoly>(id + 0x12u, 0.1f, static comp => comp.NumCasts >= 1, "Stack/spread back");
        ComponentCondition<BoughOfAttisFront>(id + 0x13u, 1.1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<BoughOfAttisFront>();
        CastMulti(id + 0x20u, [AID.BoughOfAttisSideW, AID.BoughOfAttisSideE], 2.4f, 4f)
            .ActivateOnEnter<BoughOfAttisSide>();
        ComponentCondition<BoughOfAttisSide>(id + 0x22u, 1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<BoughOfAttisSide>();
        ComponentCondition<WindsHoly>(id + 0x23u, 2.5f, static comp => comp.NumCasts >= 2, "Stack/spread front")
            .DeactivateOnExit<WindsHoly>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ForbiddenFruit2(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit2>();
        Cast(id + 0x10u, AID.Multicast, 2.1f, 3f);
        ComponentCondition<HemitheosHolySpread>(id + 0x20u, 4.8f, static comp => comp.Active)
            .ActivateOnEnter<HemitheosAeroKnockback1>()
            .ActivateOnEnter<HemitheosHolySpread>();
        ComponentCondition<HemitheosAeroKnockback1>(id + 0x30u, 1.2f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<HemitheosAeroKnockback1>();
        ComponentCondition<HemitheosHolySpread>(id + 0x40u, 3.8f, static comp => !comp.Active, "Spread")
            .DeactivateOnExit<HemitheosHolySpread>();
        ComponentCondition<ForbiddenFruit2>(id + 0x50u, 1f, static comp => comp.NumCasts > 0, "Fruit 2 (3 birds + spread) resolve")
            .DeactivateOnExit<ForbiddenFruit2>();
    }

    private void ForbiddenFruit3(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit3>();
        Cast(id + 0x10, AID.HemitheosHoly, 3.8f, 3f);
        ComponentCondition<HemitheosHoly>(id + 0x20u, 0.1f, static comp => comp.Active)
            .ActivateOnEnter<HemitheosHoly>();
        ComponentCondition<HemitheosHoly>(id + 0x30u, 6f, static comp => !comp.Active, "Stacks")
            .DeactivateOnExit<HemitheosHoly>();
        ComponentCondition<ForbiddenFruit3>(id + 0x40u, 1.1f, static comp => comp.NumCasts > 0, "Fruit 3 (3 bulls + stack) resolve")
            .DeactivateOnExit<ForbiddenFruit3>();
    }

    private void ForbiddenFruit4(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit4>();
        ComponentCondition<ForbiddenFruit4>(id + 0x10u, 6.5f, static comp => comp.NumAssignedTethers > 0, "Tethers");
        ComponentCondition<ForbiddenFruit4>(id + 0x20u, 6.4f, static comp => comp.MinotaursBaited, "Center bait");
        ComponentCondition<ForbiddenFruit4>(id + 0x30u, 3.0f, static comp => comp.NumCasts > 0, "Fruit 4 (bull + minotaurs) resolve")
            .DeactivateOnExit<ForbiddenFruit4>();
    }

    private void ForbiddenFruit5(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit5>();
        ComponentCondition<ForbiddenFruit5>(id + 0x10u, 6f, static comp => comp.NumAssignedTethers > 0, "Tethers");
        ComponentCondition<ForbiddenFruit5>(id + 0x20u, 9.6f, static comp => comp.NumCasts > 0, "Fruit 5 (birds + towers) resolve")
            .DeactivateOnExit<ForbiddenFruit5>();
    }

    private void ForbiddenFruit6(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit6>();
        Cast(id + 0x10, AID.InviolatePurgation, 2.2f, 4f);
        ComponentCondition<WindsHoly>(id + 0x12u, 1f, static comp => comp.Active)
            .ActivateOnEnter<WindsHoly>();
        CastStart(id + 0x20, AID.LightOfLife, 6.3f);
        ComponentCondition<WindsHoly>(id + 0x30u, 3.8f, static comp => comp.NumCasts >= 1, "Stack/spread 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ForbiddenFruit6>(id + 0x40u, 0.4f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<ForbiddenFruit6>();
        ComponentCondition<WindsHoly>(id + 0x50u, 14.6f, static comp => comp.NumCasts >= 2, "Stack/spread 2")
            .ActivateOnEnter<HemitheosTornado>()
            .ActivateOnEnter<HemitheosGlareMine>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastEnd(id + 0x60u, 7.2f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x61u, AID.LightOfLife, 5.2f);
        ComponentCondition<WindsHoly>(id + 0x70u, 2.6f, static comp => comp.NumCasts >= 3, "Stack/spread 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<WindsHoly>(id + 0x80u, 15f, static comp => comp.NumCasts >= 4, "Stack/spread 4")
            .DeactivateOnExit<WindsHoly>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastEnd(id + 0x90u, 8.4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HemitheosGlareMine>(id + 0x91u, 6.7f, static comp => comp.NumCasts >= 4, "Last dropped AOE")
            .DeactivateOnExit<HemitheosGlareMine>()
            .DeactivateOnExit<HemitheosTornado>();
    }

    private void ForbiddenFruit7(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay)
            .ActivateOnEnter<ForbiddenFruit7>();
        RootsOfAttis(id + 0x10u, 2.2f, "");
        Cast(id + 0x20u, AID.HemitheosGlare, 2.2f, 5f);
        ComponentCondition<ForbiddenFruit7>(id + 0x30u, 7.7f, static comp => comp.NumCasts > 0, "Fruit 7 (2 birds + chasing aoes) resolve")
            .DeactivateOnExit<ForbiddenFruit7>();
    }

    private void FaminesHarvest(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay, AID.FaminesHarvest)
            .ActivateOnEnter<ForbiddenFruit8>();
        ComponentCondition<ForbiddenFruit8>(id + 0x10u, 6.5f, static comp => comp.NumAssignedTethers > 0, "Tethers");
        ComponentCondition<ForbiddenFruit8>(id + 0x20u, 6.4f, static comp => comp.MinotaursBaited, "Bait");
        ComponentCondition<ForbiddenFruit8>(id + 0x30u, 3.0f, static comp => comp.NumCasts > 0, "Fruit 8 (6 minotaurs + 2 birds) resolve")
            .DeactivateOnExit<ForbiddenFruit8>();
    }

    private void DeathsHarvest(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay, AID.DeathsHarvest)
            .ActivateOnEnter<ForbiddenFruit9>();
        ComponentCondition<ForbiddenFruit9>(id + 0x10u, 6.5f, static comp => comp.NumAssignedTethers > 0, "Tethers");
        ComponentCondition<ForbiddenFruit9>(id + 0x20u, 9.3f, static comp => comp.NumCasts > 0, "Fruit 9 (3 bulls + 2 birds) resolve")
            .DeactivateOnExit<ForbiddenFruit9>();
    }

    private void WarsHarvest(uint id, float delay)
    {
        ForbiddenFruitHarvestStart(id, delay, AID.WarsHarvest)
            .ActivateOnEnter<ForbiddenFruit10>();
        ComponentCondition<ForbiddenFruit10>(id + 0x10u, 6.5f, static comp => comp.NumAssignedTethers > 0, "Tethers");
        ComponentCondition<ForbiddenFruit10>(id + 0x20u, 9.1f, static comp => comp.NumCasts > 0, "Fruit 10 (bull + 2 birds + 2 minotaurs) resolve")
            .DeactivateOnExit<ForbiddenFruit10>();
    }
}
