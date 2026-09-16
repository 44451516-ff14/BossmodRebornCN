namespace BossMod.Endwalker.Savage.P6SHegemone;

sealed class P6SStates : StateMachineBuilder
{
    public P6SStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        HemitheosDark(id, 8.2f);
        Synergy(id + 0x10000u, 6.7f);
        PolyominoidUnholyDarknessExocleaver(id + 0x20000u, 9.9f);
        PathogenicCells(id + 0x30000u, 6);
        ExchangeOfAgoniesChorosIxou(id + 0x40000u, 8.3f);
        Synergy(id + 0x50000u, 7.1f);
        HemitheosDark(id + 0x60000u, 7.2f);
        TransmissionChorosIxou(id + 0x70000u, 6.7f);
        PolyominoidSigmaDarkDome(id + 0x80000u, 6.2f);
        ExchangeOfAgoniesExocleaver(id + 0x90000u, 6.6f);
        Synergy(id + 0xA0000u, 6.1f);
        HemitheosDark(id + 0xB0000u, 7.2f);
        PolyominoidUnholyDarkness(id + 0xC0000u, 6.7f);
        DarkAshesChorosIxou(id + 0xD0000u, 7.8f);
        CachexiaDualPredationPteraIxou(id + 0xE0000u, 8.9f);
        Synergy(id + 0xF0000u, 7.2f);
        HemitheosDark(id + 0x100000u, 7.2f);
        PolyominoidDarkSphereDarkDome(id + 0x110000u, 6.7f);
        ExchangeOfAgoniesChorosIxou(id + 0x120000u, 7.6f);
        PolyominoidSigmaChorosIxou(id + 0x130000u, 6.1f);
        Synergy(id + 0x140000u, 6.1f);
        HemitheosDark(id + 0x150000u, 7.2f);
        CachexiaTransmissionPolyominoidPteraIxou(id + 0x160000u, 9.4f);
        AethericPolyominoidDarkDome(id + 0x170000u, 8.2f);
        AethericPolyominoidChorosIxou(id + 0x180000u, 7.5f);
        Synergy(id + 0x190000u, 6.2f);
        HemitheosDark(id + 0x1A0000u, 7.2f);
        Cast(id + 0x1B0000u, AID.Enrage, 10f, 10f, "Enrage");
    }

    private void HemitheosDark(uint id, float delay)
    {
        Cast(id, AID.HemitheosDark, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Synergy(uint id, float delay)
    {
        // note that casts have different time, but resolve is the same
        CastStartMulti(id, [AID.Synergy, AID.ChelicSynergy], delay);
        ComponentCondition<Synergy>(id + 1u, 7f, static comp => comp.Done, "Tankbuster")
            .ActivateOnEnter<Synergy>()
            .DeactivateOnExit<Synergy>()
            .SetHint(StateMachine.StateHint.Tankbuster | StateMachine.StateHint.BossCastEnd);
    }

    private void PathogenicCells(uint id, float delay)
    {
        CastStart(id, AID.PathogenicCells, delay)
            .ActivateOnEnter<PathogenicCells>();
        CastEnd(id + 1u, 8f, "Limit cut start");
        ComponentCondition<PathogenicCells>(id + 0x10u, 14f, static comp => comp.NumCasts >= 8, "Limit cut resolve")
            .DeactivateOnExit<PathogenicCells>();
    }

    // leaves component active for second cone
    private void ChorosIxouStart(uint id, float delay, bool withParasiteStun = false)
    {
        CastMulti(id, [AID.ChorosIxouFSFront, AID.ChorosIxouSFSides], delay, 4.5f, withParasiteStun ? "Parasite stun" : "")
            .ActivateOnEnter<ChorosIxou>()
            .SetHint(StateMachine.StateHint.DowntimeStart, withParasiteStun);
        ComponentCondition<ChorosIxou>(id + 2u, 0.5f, static comp => comp.FirstDone, "Cones 1");
    }

    // happens ~3.1s after start
    private void ChorosIxouEnd(uint id, float delay)
    {
        ComponentCondition<ChorosIxou>(id, delay, static comp => comp.SecondDone, "Cones 2")
            .DeactivateOnExit<ChorosIxou>();
    }

    // leaves component active; includes two 'exchange' casts
    private void ExchangeOfAgoniesStart(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3f);
        Cast(id + 2u, AID.ExchangeOfAgonies, 2.7f, 4f);
        ComponentCondition<Agonies>(id + 4u, 0.9f, static comp => comp.Active)
            .ActivateOnEnter<Agonies>();
    }

    // happens ~7s after start
    private void ExchangeOfAgoniesResolve(uint id, float delay)
    {
        ComponentCondition<Agonies>(id, delay, static comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<Agonies>();
    }

    // leaves component active
    private State DarkDomeBait(uint id, float delay, string activateName = "")
    {
        Cast(id, AID.DarkDome, delay, 4u, "Puddles bait");
        return ComponentCondition<DarkDome>(id + 2u, 0.9f, static comp => comp.Casters.Count > 0, activateName)
            .ActivateOnEnter<DarkDome>();
    }

    // happens ~4s after bait
    private void DarkDomeEnd(uint id, float delay)
    {
        ComponentCondition<DarkDome>(id, delay, static comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<DarkDome>();
    }

    private void ExchangeOfAgoniesChorosIxou(uint id, float delay)
    {
        ExchangeOfAgoniesStart(id, delay);
        ChorosIxouStart(id + 0x10u, 1.9f);
        ExchangeOfAgoniesResolve(id + 0x20u, 0.1f);
        ChorosIxouEnd(id + 0x30u, 3f);
    }

    private void TransmissionChorosIxou(uint id, float delay)
    {
        Cast(id, AID.Transmission, delay, 5f)
            .ActivateOnEnter<Transmission>();
        ChorosIxouStart(id + 0x10u, 8.4f, true); // out-of-control is applied right as cast ends
        ComponentCondition<Transmission>(id + 0x20u, 1.5f, static comp => !comp.StunsActive)
            .DeactivateOnExit<Transmission>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ChorosIxouEnd(id + 0x30u, 1.6f);
    }

    private void DarkAshesChorosIxou(uint id, float delay)
    {
        Cast(id, AID.DarkAshes, delay, 4f)
            .ActivateOnEnter<DarkAshes>();
        ChorosIxouStart(id + 0x10u, 3.5f);
        ComponentCondition<DarkAshes>(id + 0x20u, 0.4f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<DarkAshes>();
        ChorosIxouEnd(id + 0x30u, 2.7f);
    }

    private void PolyominoidSigmaChorosIxou(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3f);
        Cast(id + 0x10u, AID.PolyominoidSigma, 2.7f, 4f)
            .ActivateOnEnter<Polyominoid>();
        ChorosIxouStart(id + 0x20u, 7.4f);
        ComponentCondition<Polyominoid>(id + 0x30u, 0.3f, static comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        ChorosIxouEnd(id + 0x40u, 2.8f);
    }

    private void AethericPolyominoidChorosIxou(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4f)
            .ActivateOnEnter<Polyominoid>();
        ChorosIxouStart(id + 0x20u, 4.8f);
        ComponentCondition<Polyominoid>(id + 0x30u, 0.2f, static comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        ChorosIxouEnd(id + 0x40u, 2.9f);
    }

    private void PolyominoidUnholyDarkness(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3f);
        Cast(id + 0x10u, AID.PolyominoidSigma, 2.6f, 4f)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x20u, AID.UnholyDarkness, 2.7f, 4);
        ComponentCondition<UnholyDarkness>(id + 0x30u, 7.1f, static comp => comp.NumFinishedStacks > 0, "Cells resolve + Party stacks")
            .ActivateOnEnter<UnholyDarkness>() // activates ~1s after cast end
            .DeactivateOnExit<UnholyDarkness>()
            .DeactivateOnExit<Polyominoid>(); // resolves in the same frame
    }

    private void PolyominoidUnholyDarknessExocleaver(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4f)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x10u, AID.UnholyDarkness, 2.7f, 4f);
        Cast(id + 0x20u, AID.Exocleaver, 2.2f, 4, "Cells resolve + Pizzas 1")
            .ActivateOnEnter<UnholyDarkness>() // activates ~0.1s after cast start
            .ActivateOnEnter<Exocleaver>()
            .DeactivateOnExit<Polyominoid>(); // resolves ~0.2s before cast end
        ComponentCondition<UnholyDarkness>(id + 0x30u, 2.2f, static comp => comp.NumFinishedStacks > 0, "Party stacks")
            .DeactivateOnExit<UnholyDarkness>();
        ComponentCondition<Exocleaver>(id + 0x31u, 0.4f, static comp => comp.NumCasts > 0, "Pizzas 2")
            .DeactivateOnExit<Exocleaver>();
    }

    private void ExchangeOfAgoniesExocleaver(uint id, float delay)
    {
        ExchangeOfAgoniesStart(id, delay);
        Cast(id + 0x10u, AID.Exocleaver, 3f, 4f, "Pizzas 1")
            .ActivateOnEnter<Exocleaver>();
        ExchangeOfAgoniesResolve(id + 0x20u, 0f);
        ComponentCondition<Exocleaver>(id + 0x30u, 2.6f, static comp => comp.NumCasts > 0, "Pizzas 2")
            .DeactivateOnExit<Exocleaver>();
    }

    private void PolyominoidSigmaDarkDome(uint id, float delay)
    {
        Cast(id, AID.AetherialExchange, delay, 3f);
        Cast(id + 0x10u, AID.PolyominoidSigma, 2.7f, 4f)
            .ActivateOnEnter<Polyominoid>();
        DarkDomeBait(id + 0x20u, 5.7f);
        ComponentCondition<Polyominoid>(id + 0x30u, 3.9f, static comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        DarkDomeEnd(id + 0x40u, 0.1f);
    }

    private void AethericPolyominoidDarkDome(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4f)
            .ActivateOnEnter<Polyominoid>();
        DarkDomeBait(id + 0x10u, 4.7f);
        ComponentCondition<Polyominoid>(id + 0x20u, 3.6f, static comp => comp.NumCasts > 0, "Cells resolve")
            .DeactivateOnExit<Polyominoid>();
        DarkDomeEnd(id + 0x30u, 0.4f);
    }

    private void PolyominoidDarkSphereDarkDome(uint id, float delay)
    {
        Cast(id, AID.AethericPolyominoid, delay, 4f)
            .ActivateOnEnter<Polyominoid>();
        Cast(id + 0x10u, AID.DarkSphere, 2.7f, 4f)
            .ActivateOnEnter<DarkSphere>(); // activates ~0.9s after cast end
        DarkDomeBait(id + 0x20u, 2.1f, "Spread + Cells resolve")
            .DeactivateOnExit<DarkSphere>() // resolves ~0.1s before bait
            .DeactivateOnExit<Polyominoid>(); // resolves in the same frame
        DarkDomeEnd(id + 0x30u, 4f);
    }

    private void CachexiaDualPredationPteraIxou(uint id, float delay)
    {
        Cast(id, AID.Cachexia, delay, 3f)
            .ActivateOnEnter<AetheronecrosisPredation>();
        ComponentCondition<AetheronecrosisPredation>(id + 2u, 0.9f, static comp => comp.Active, "Cachexia 1 start");
        ComponentCondition<AetheronecrosisPredation>(id + 0x10u, 8f, static comp => comp.NumCastsAetheronecrosis > 0, "Explode 1");
        CastStart(id + 0x20u, AID.DualPredationFirst, 2.3f);
        ComponentCondition<AetheronecrosisPredation>(id + 0x21u, 1.7f, static comp => comp.NumCastsAetheronecrosis > 2, "Explode 2");
        ComponentCondition<AetheronecrosisPredation>(id + 0x22u, 4f, static comp => comp.NumCastsAetheronecrosis > 4, "Explode 3");
        CastEnd(id + 0x23u, 0.3f);
        ComponentCondition<AetheronecrosisPredation>(id + 0x24u, 0.9f, static comp => comp.NumCastsDualPredation > 0, "Wing/snake 1");
        ComponentCondition<AetheronecrosisPredation>(id + 0x25u, 2.8f, static comp => comp.NumCastsAetheronecrosis > 6, "Explode 4");
        ComponentCondition<AetheronecrosisPredation>(id + 0x30u, 1.2f, static comp => comp.NumCastsDualPredation > 1, "Wing/snake 2");
        ComponentCondition<AetheronecrosisPredation>(id + 0x31u, 4f, static comp => comp.NumCastsDualPredation > 2, "Wing/snake 3");
        ComponentCondition<AetheronecrosisPredation>(id + 0x32u, 4f, static comp => comp.NumCastsDualPredation > 3, "Wing/snake 4")
            .DeactivateOnExit<AetheronecrosisPredation>();
        Cast(id + 0x40u, AID.PteraIxou, 3.1f, 6f)
            .ActivateOnEnter<PteraIxou>(); // old statuses are removed ~0.4s before cast start
        ComponentCondition<PteraIxou>(id + 0x42u, 1f, static comp => comp.NumCasts > 0, "Sides")
            .DeactivateOnExit<PteraIxou>();
    }

    private void CachexiaTransmissionPolyominoidPteraIxou(uint id, float delay)
    {
        Cast(id, AID.Cachexia, delay, 3f)
            .ActivateOnEnter<PteraIxou>(); // activate early, since side selection is first thing we do - and boss won't rotate
        Cast(id + 0x10u, AID.Transmission, 2.7f, 5f)
            .ActivateOnEnter<Transmission>();
        Cast(id + 0x20u, AID.AetherialExchange, 4.6f, 3f);
        Cast(id + 0x30u, AID.PolyominoidSigma, 2.7f, 4f)
            .ActivateOnEnter<Polyominoid>();
        CastStart(id + 0x40u, AID.PteraIxou, 7.5f);
        ComponentCondition<Transmission>(id + 0x41u, 5.2f, static comp => comp.StunsActive, "Parasite stun")
            .ActivateOnEnter<PteraIxouSpreadStack>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        CastEnd(id + 0x42u, 0.8f, "Cells resolve + Spread/stack")
            .DeactivateOnExit<PteraIxouSpreadStack>()
            .DeactivateOnExit<Polyominoid>();
        ComponentCondition<PteraIxou>(id + 0x43u, 1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<PteraIxou>();
        ComponentCondition<Transmission>(id + 0x44u, 0.2f, static comp => !comp.StunsActive, "Sides")
            .DeactivateOnExit<Transmission>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }
}
