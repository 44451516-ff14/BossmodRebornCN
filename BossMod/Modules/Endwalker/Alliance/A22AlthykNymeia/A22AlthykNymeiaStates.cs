namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

sealed class A22AlthykNymeiaStates : StateMachineBuilder
{
    private readonly A22AlthykNymeia _module;

    public A22AlthykNymeiaStates(A22AlthykNymeia module) : base(module)
    {
        _module = module;
        SimplePhase(0u, SinglePhase, "Single phase")
            .ActivateOnEnter<Axioma>()
            .Raw.Update = () => (_module.Althyk()?.IsDeadOrDestroyed ?? true) && (_module.Nymeia()?.IsDeadOrDestroyed ?? true);
    }

    private void SinglePhase(uint id)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, 10.3f, 4.5f);
        Dictionary<SpinnersWheelSelect.Branch, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [SpinnersWheelSelect.Branch.Gaze] = ((id >> 24) + 1, ForkGaze),
            [SpinnersWheelSelect.Branch.StayMove] = ((id >> 24) + 2, ForkStayMove)
        };
        ComponentConditionFork<SpinnersWheelSelect, SpinnersWheelSelect.Branch>(id + 0x10, 0.9f, static comp => comp.SelectedBranch != SpinnersWheelSelect.Branch.None, static comp => comp.SelectedBranch, dispatch, "Gaze -or- stay/move")
            .ActivateOnEnter<SpinnersWheelSelect>()
            .DeactivateOnExit<SpinnersWheelSelect>();
    }

    private void ForkGaze(uint id)
    {
        SpinnersWheelGazeResolveMythrilGreataxe(id, 10.1f);
        SpinnersWheelGazeTimeAndTide(id + 0x10000u, 8.9f);
        Axioma(id + 0x20000u, 20.5f);
        Hydroptosis(id + 0x30000u, 3.4f);
        InexorablePull(id + 0x40000u, 5.8f);
        Hydrorythmos(id + 0x50000u, 8.9f);
        HydrostasisPetrai(id + 0x60000u, 14.8f);
        SpinnersWheelGazeMythrilGreataxe(id + 0x70000u, 11.3f);
        Hydroptosis(id + 0x80000u, 0.6f);
        Petrai(id + 0x90000u, 10.2f);
        HydrostasisTimeAndTide(id + 0xA0000u, 8.8f);
        Axioma(id + 0xB0000u, 14.5f);
        SpinnersWheelGazeHydrorythmosTimeAndTide(id + 0xC0000u, 9.4f);
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void ForkStayMove(uint id)
    {
        SpinnersWheelStayMoveResolveMythrilGreataxe(id, 4.8f);
        SpinnersWheelStayMoveTimeAndTide(id + 0x10000u, 10.7f);
        Axioma(id + 0x20000u, 16.5f);
        Hydroptosis(id + 0x30000u, 3.4f);
        InexorablePull(id + 0x40000u, 6.0f);
        Hydrorythmos(id + 0x50000u, 8.9f);
        HydrostasisPetrai(id + 0x60000u, 14.8f);
        SpinnersWheelStayMoveMythrilGreataxe(id + 0x70000u, 13.4f);
        Hydroptosis(id + 0x80000u, 0.7f);
        Petrai(id + 0x90000u, 11.2f);
        HydrostasisTimeAndTide(id + 0xA0000u, 8.9f);
        Axioma(id + 0xB0000u, 14.5f);
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void MythrilGreataxe(uint id, float delay)
    {
        ActorCast(id, _module.Althyk, AID.MythrilGreataxe, delay, 7f, false, "Cleave")
            .ActivateOnEnter<MythrilGreataxe>()
            .DeactivateOnExit<MythrilGreataxe>();
    }

    private void Hydrorythmos(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.Hydrorythmos, delay, 5f, false, "Line 1")
            .ActivateOnEnter<Hydrorythmos>();
        ComponentCondition<Hydrorythmos>(id + 0x10u, 2.1f, static comp => comp.NumCasts == 3);
        ComponentCondition<Hydrorythmos>(id + 0x11u, 2.1f, static comp => comp.NumCasts == 5);
        ActorCastStart(id + 0x20u, _module.Althyk, AID.MythrilGreataxe, 0.7f);
        ComponentCondition<Hydrorythmos>(id + 0x30u, 1.4f, static comp => comp.NumCasts == 7)
            .ActivateOnEnter<MythrilGreataxe>();
        ComponentCondition<Hydrorythmos>(id + 0x31u, 2.1f, static comp => comp.NumCasts == 9)
            .DeactivateOnExit<Hydrorythmos>();
        ActorCastEnd(id + 0x40u, _module.Althyk, 3.5f, false, "Cleave")
            .DeactivateOnExit<MythrilGreataxe>();
    }

    private State SpinnersWheelGazeResolve(uint id, float delay)
    {
        return Condition(id, delay, () => _module.FindComponent<SpinnersWheelArcaneAttraction>()!.NumCasts + _module.FindComponent<SpinnersWheelAttractionReversed>()!.NumCasts > 0, "Gaze resolve")
            .DeactivateOnExit<SpinnersWheelArcaneAttraction>()
            .DeactivateOnExit<SpinnersWheelAttractionReversed>();
    }

    private void SpinnersWheelStayMoveResolve(uint id, float delay)
    {
        ComponentCondition<SpinnersWheelStayMove>(id, delay, static comp => comp.ActiveDebuffs > 0, "Stay/move");
        ComponentCondition<SpinnersWheelStayMove>(id + 1u, 2f, static comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
            .DeactivateOnExit<SpinnersWheelStayMove>();
    }

    private void SpinnersWheelGazeResolveMythrilGreataxe(uint id, float delay)
    {
        SpinnersWheelGazeResolve(id, delay)
            .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
            .ActivateOnEnter<SpinnersWheelAttractionReversed>();
        MythrilGreataxe(id + 0x1000u, 0.8f);
    }

    private void SpinnersWheelStayMoveResolveMythrilGreataxe(uint id, float delay)
    {
        ActorCastStart(id, _module.Althyk, AID.MythrilGreataxe, delay)
            .ActivateOnEnter<SpinnersWheelStayMove>();
        ComponentCondition<SpinnersWheelStayMove>(id + 1u, 5.2f, static comp => comp.ActiveDebuffs > 0, "Stay/move")
            .ActivateOnEnter<MythrilGreataxe>();
        ActorCastEnd(id + 2u, _module.Althyk, 1.8f, false, "Cleave")
            .DeactivateOnExit<MythrilGreataxe>();
        ComponentCondition<SpinnersWheelStayMove>(id + 3u, 0.2f, static comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
            .DeactivateOnExit<SpinnersWheelStayMove>();
    }

    private void SpinnersWheelGazeMythrilGreataxe(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
        SpinnersWheelGazeResolveMythrilGreataxe(id + 0x1000u, 21f);
    }

    private void SpinnersWheelStayMoveMythrilGreataxe(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
        ComponentCondition<SpinnersWheelStayMove>(id + 0x10u, 20.9f, static comp => comp.ActiveDebuffs > 0, "Stay/move")
            .ActivateOnEnter<SpinnersWheelStayMove>();
        ActorCastStart(id + 0x11u, _module.Althyk, AID.MythrilGreataxe, 0.9f);
        ComponentCondition<SpinnersWheelStayMove>(id + 0x12u, 1.1f, static comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
            .ActivateOnEnter<MythrilGreataxe>()
            .DeactivateOnExit<SpinnersWheelStayMove>();
        ActorCastEnd(id + 0x13u, _module.Althyk, 5.9f, false, "Cleave")
            .DeactivateOnExit<MythrilGreataxe>();
    }

    private void SpinnersWheelGazeTimeAndTide(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f)
            .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
            .ActivateOnEnter<SpinnersWheelAttractionReversed>();
        ActorCast(id + 0x10u, _module.Althyk, AID.TimeAndTide, 1.7f, 10);
        SpinnersWheelGazeResolve(id + 0x20u, 2.7f);
    }

    private void SpinnersWheelStayMoveTimeAndTide(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f)
            .ActivateOnEnter<SpinnersWheelStayMove>();
        ActorCast(id + 0x10u, _module.Althyk, AID.TimeAndTide, 1.7f, 10f);
        SpinnersWheelStayMoveResolve(id + 0x20u, 2.7f);
    }

    private void SpinnersWheelGazeHydrorythmosTimeAndTide(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
        ActorCastStart(id + 0x10u, _module.Althyk, AID.TimeAndTide, 1.5f)
            .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
            .ActivateOnEnter<SpinnersWheelAttractionReversed>();
        ActorCast(id + 0x20u, _module.Nymeia, AID.Hydrorythmos, 0.6f, 5f, false, "Line 1")
            .ActivateOnEnter<Hydrorythmos>();
        ComponentCondition<Hydrorythmos>(id + 0x30u, 2.1f, static comp => comp.NumCasts == 3);
        ComponentCondition<Hydrorythmos>(id + 0x31u, 2.1f, static comp => comp.NumCasts == 5);
        ActorCastEnd(id + 0x40u, _module.Althyk, 0.2f);
        // TODO: below should happen faster... didn't see any good logs however...
        ComponentCondition<Hydrorythmos>(id + 0x50u, 1.9f, static comp => comp.NumCasts == 7);
        ComponentCondition<Hydrorythmos>(id + 0x51u, 2.1f, static comp => comp.NumCasts == 9)
            .DeactivateOnExit<Hydrorythmos>();
        SpinnersWheelGazeResolve(id + 0x60u, 5.6f);
    }

    private void Axioma(uint id, float delay)
    {
        ActorCast(id, _module.Althyk, AID.Axioma, delay, 5f, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Hydroptosis(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.Hydroptosis, delay, 5f, false, "Spread")
            .ActivateOnEnter<Hydroptosis>()
            .DeactivateOnExit<Hydroptosis>();
    }

    private void InexorablePull(uint id, float delay)
    {
        ActorCast(id, _module.Althyk, AID.InexorablePull, delay, 6f);
        ComponentCondition<Axioma>(id + 0x10u, 0.7f, static comp => !comp.ShouldBeInZone, "Kick up");
    }

    private void Petrai(uint id, float delay)
    {
        ActorCast(id, _module.Althyk, AID.Petrai, delay, 5)
            .ActivateOnEnter<Petrai>();
        ComponentCondition<Petrai>(id + 0x10u, 1f, static comp => comp.NumCasts > 0, "Shared tankbuster")
            .DeactivateOnExit<Petrai>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HydrostasisPetrai(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.Hydrostasis, delay, 4)
            .ActivateOnEnter<Petrai>();
        ActorCastEnd(id + 2u, _module.Althyk, 1.1f); // this cast ends and tower knockback casts start at the same time
        ComponentCondition<Petrai>(id + 0x10u, 1f, static comp => comp.NumCasts > 0, "Shared tankbuster")
            .ActivateOnEnter<Hydrostasis>()
            .DeactivateOnExit<Petrai>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<Hydrostasis>(id + 0x20u, 15f, static comp => comp.NumCasts >= 1, "Knockback 1")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<Hydrostasis>(id + 0x21u, 3f, static comp => comp.NumCasts >= 2, "Knockback 2");
        ComponentCondition<Hydrostasis>(id + 0x22u, 3f, static comp => comp.NumCasts >= 3, "Knockback 3")
            .DeactivateOnExit<Hydrostasis>();
    }

    private void HydrostasisTimeAndTide(uint id, float delay)
    {
        ActorCast(id, _module.Nymeia, AID.Hydrostasis, delay, 4);
        ComponentCondition<Hydrostasis>(id + 0x10u, 2.0f, static comp => comp.Active)
            .ActivateOnEnter<Hydrostasis>();
        ActorCast(id + 0x20, _module.Althyk, AID.TimeAndTide, 0.1f, 10); // TODO: boss often dies here...
        ComponentCondition<Hydrostasis>(id + 0x30u, 2f, static comp => comp.NumCasts >= 1, "Knockback 1")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<Hydrostasis>(id + 0x31u, 3f, static comp => comp.NumCasts >= 2, "Knockback 2");
        ComponentCondition<Hydrostasis>(id + 0x32u, 3f, static comp => comp.NumCasts >= 3, "Knockback 3")
            .DeactivateOnExit<Hydrostasis>();
    }
}
