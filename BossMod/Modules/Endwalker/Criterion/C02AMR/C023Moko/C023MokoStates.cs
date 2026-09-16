namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class C023MokoStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C023MokoStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChange>();
    }

    private void SinglePhase(uint id)
    {
        KenkiRelease(id, 10.2f);
        TripleKasumiGiri(id + 0x10000u, 7.2f);
        TripleKasumiGiri(id + 0x20000u, 2.2f);
        LateralSlice(id + 0x30000u, 4.3f);
        ScarletAuspice(id + 0x40000u, 7.4f);
        KenkiRelease(id + 0x50000u, 3.1f);
        ShadowTwin(id + 0x60000u, 8.5f);
        KenkiRelease(id + 0x70000u, 0.1f);
        AzureAuspice(id + 0x80000u, 9.4f);
        KenkiRelease(id + 0x90000u, 2.2f);
        SoldiersOfDeath(id + 0xA0000u, 9.6f);
        TripleKasumiGiri(id + 0xB0000u, 1.5f);
        KenkiRelease(id + 0xC0000u, 2.1f);
        LateralSlice(id + 0xD0000u, 5.3f);
        KenkiRelease(id + 0xE0000u, 3.1f);
        Cast(id + 0xF0000u, AID.Enrage, 5.3f, 10f, "Enrage");
    }

    private void KenkiRelease(uint id, float delay)
    {
        Cast(id, _savage ? AID.SKenkiRelease : AID.NKenkiRelease, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LateralSlice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SLateralSlice : AID.NLateralSlice, delay, 5f)
            .ActivateOnEnter<NLateralSlice>(!_savage)
            .ActivateOnEnter<SLateralSlice>(_savage);
        ComponentCondition<LateralSlice>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<LateralSlice>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State TripleKasumiGiri(uint id, float delay)
    {
        AID[] firstCasts = _savage
            ? [AID.STripleKasumiGiriOutFrontFirst, AID.STripleKasumiGiriOutRightFirst, AID.STripleKasumiGiriOutBackFirst, AID.STripleKasumiGiriOutLeftFirst, AID.STripleKasumiGiriInFrontFirst, AID.STripleKasumiGiriInRightFirst, AID.STripleKasumiGiriInBackFirst, AID.STripleKasumiGiriInLeftFirst]
            : [AID.NTripleKasumiGiriOutFrontFirst, AID.NTripleKasumiGiriOutRightFirst, AID.NTripleKasumiGiriOutBackFirst, AID.NTripleKasumiGiriOutLeftFirst, AID.NTripleKasumiGiriInFrontFirst, AID.NTripleKasumiGiriInRightFirst, AID.NTripleKasumiGiriInBackFirst, AID.NTripleKasumiGiriInLeftFirst];
        AID[] restCasts = _savage
            ? [AID.STripleKasumiGiriOutFrontRest, AID.STripleKasumiGiriOutRightRest, AID.STripleKasumiGiriOutBackRest, AID.STripleKasumiGiriOutLeftRest, AID.STripleKasumiGiriInFrontRest, AID.STripleKasumiGiriInRightRest, AID.STripleKasumiGiriInBackRest, AID.STripleKasumiGiriInLeftRest]
            : [AID.NTripleKasumiGiriOutFrontRest, AID.NTripleKasumiGiriOutRightRest, AID.NTripleKasumiGiriOutBackRest, AID.NTripleKasumiGiriOutLeftRest, AID.NTripleKasumiGiriInFrontRest, AID.NTripleKasumiGiriInRightRest, AID.NTripleKasumiGiriInBackRest, AID.NTripleKasumiGiriInLeftRest];
        CastMulti(id, firstCasts, delay, 12f, "Cleave 1")
            .ActivateOnEnter<TripleKasumiGiri>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastMulti(id + 0x10u, restCasts, 2.1f, 1f, "Cleave 2");
        return CastMulti(id + 0x20u, restCasts, 2.1f, 1f, "Cleave 3")
            .DeactivateOnExit<TripleKasumiGiri>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ScarletAuspice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SScarletAuspice : AID.NScarletAuspice, delay, 5f, "Out")
            .ActivateOnEnter<NScarletAuspice>(!_savage)
            .ActivateOnEnter<SScarletAuspice>(_savage)
            .DeactivateOnExit<ScarletAuspice>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x10u, AID.BoundlessScarlet, 3.2f, 2.4f)
            .ActivateOnEnter<NBoundlessScarletFirst>(!_savage)
            .ActivateOnEnter<SBoundlessScarletFirst>(_savage);
        ComponentCondition<BoundlessScarletFirst>(id + 0x12, 0.6f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<BoundlessScarletFirst>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        Cast(id + 0x20u, AID.InvocationOfVengeance, 3.5f, 3f)
            .ActivateOnEnter<InvocationOfVengeance>(); // +0.8s: stack/spread debuffs
        Cast(id + 0x30u, AID.FleetingIaiGiri, 3.2f, 9f, "Jump")
            .ActivateOnEnter<FleetingIaiGiriBait>()
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<NBoundlessScarletRest>(!_savage) // first pair starts 0.6 into cast, pairs 7s apart
            .ActivateOnEnter<SBoundlessScarletRest>(_savage);
        CastMulti(id + 0x40u, [AID.NFleetingIaiGiriFront, AID.NFleetingIaiGiriRight, AID.NFleetingIaiGiriLeft, AID.SFleetingIaiGiriFront, AID.SFleetingIaiGiriRight, AID.SFleetingIaiGiriLeft], 1.6f, 1, "Cleave")
            .DeactivateOnExit<FleetingIaiGiriBait>()
            .DeactivateOnExit<IaiGiriResolve>();
        ComponentCondition<InvocationOfVengeance>(id + 0x50u, 1.2f, static comp => comp.NumMechanics > 0, "Stack/spread"); // first pair of explosions happen right before this
        ComponentCondition<BoundlessScarletRest>(id + 0x60u, 6.8f, static comp => comp.Casters.Count == 0, "Lines resolve")
            .DeactivateOnExit<BoundlessScarletRest>();
        ComponentCondition<InvocationOfVengeance>(id + 0x70u, 1.1f, static comp => comp.NumMechanics > 1, "Spread/stack")
            .DeactivateOnExit<InvocationOfVengeance>();
    }

    private void AzureAuspice(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAzureAuspice : AID.NAzureAuspice, delay, 5f, "In")
            .ActivateOnEnter<NAzureAuspice>(!_savage)
            .ActivateOnEnter<SAzureAuspice>(_savage)
            .DeactivateOnExit<AzureAuspice>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x10u, AID.BoundlessAzure, 3.2f, 2.4f)
            .ActivateOnEnter<NBoundlessAzure>(!_savage)
            .ActivateOnEnter<SBoundlessAzure>(_savage);
        ComponentCondition<BoundlessAzure>(id + 0x12, 0.6f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<BoundlessAzure>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<Upwell>(id + 0x20u, 11.0f, static comp => comp.NumCasts > 0, "Expanding start")
            .ActivateOnEnter<Upwell>();

        TripleKasumiGiri(id + 0x1000u, 7.8f)
            .DeactivateOnExit<Upwell>();
    }

    private void ShadowTwin(uint id, float delay)
    {
        Cast(id, AID.ShadowTwin, delay, 3f);
        // +0.8s: PATE 1E43 on 2 shadows
        Cast(id + 0x10u, _savage ? AID.SMoonlessNight : AID.NMoonlessNight, 3.1f, 3f, "Raidwide")
            .ActivateOnEnter<DoubleIaiGiriBait>() // first statuses appear 0.1s after cast start
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<Clearout>() // PATEs 1.0s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x20u, false, 3.1f, "Boss disappears"); // around here we get second statuses
        CastMulti(id + 0x30u, [AID.FarEdge, AID.NearEdge], 0.1f, 6f)
            .ActivateOnEnter<AccursedEdge>();
        ComponentCondition<AccursedEdge>(id + 0x32u, 0.1f, static comp => comp.NumCasts > 0, "Jump/bait 1") // also first clearout
            .DeactivateOnExit<AccursedEdge>();
        ComponentCondition<IaiGiriResolve>(id + 0x40u, 2.8f, static comp => comp.NumCasts >= 2, "Cleave back");
        ComponentCondition<IaiGiriResolve>(id + 0x50u, 3.1f, static comp => comp.NumCasts >= 4, "Cleave 2")
            .DeactivateOnExit<DoubleIaiGiriBait>() // note: this could be deactivated early, but we want to make sure resolve components picks up everything
            .DeactivateOnExit<Clearout>() // TODO: last aoe ~1.2s before?..
            .DeactivateOnExit<IaiGiriResolve>();

        Cast(id + 0x100u, _savage ? AID.SMoonlessNight : AID.NMoonlessNight, 2.5f, 3f, "Raidwide")
            .ActivateOnEnter<DoubleIaiGiriBait>() // first statuses appear 0.3s after cast start
            .ActivateOnEnter<IaiGiriResolve>()
            .ActivateOnEnter<Clearout>() // PATEs 1.0s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x110u, [AID.FarEdge, AID.NearEdge], 3.2f, 6f)
            .ActivateOnEnter<AccursedEdge>();
        ComponentCondition<AccursedEdge>(id + 0x112u, 0.1f, static comp => comp.NumCasts > 0, "Jump/bait 2") // also first clearout
           .DeactivateOnExit<AccursedEdge>();
        ComponentCondition<IaiGiriResolve>(id + 0x120u, 2.8f, static comp => comp.NumCasts >= 2, "Cleave back")
            .DeactivateOnExit<DoubleIaiGiriBait>(); // note: this could be deactivated early, but we want to make sure resolve components picks up everything
        ComponentCondition<IaiGiriResolve>(id + 0x130u, 3.1f, static comp => comp.NumCasts >= 4, "Cleave 2")
            .DeactivateOnExit<Clearout>() // TODO: last aoe ~1.2s before?..
            .DeactivateOnExit<IaiGiriResolve>();

        Targetable(id + 0x200u, true, 2.2f, "Boss reappears");
    }

    private void SoldiersOfDeath(uint id, float delay)
    {
        Cast(id, AID.SoldiersOfDeath, delay, 3f);
        Cast(id + 0x10u, AID.ShadowTwin, 3.2f, 3f);
        ComponentCondition<IronRainStorm>(id + 0x20u, 0.9f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DoubleIaiGiriBait>() // casts start 2.4s after iron rain/storm, but we activate earlier, so that rain/storm component can provide hints
            .ActivateOnEnter<IronRainStorm>();
        ComponentCondition<IronRainStorm>(id + 0x30u, 15, static comp => comp.NumCasts > 0, "Jumps + AOEs")
            .ActivateOnEnter<IaiGiriResolve>()
            .DeactivateOnExit<DoubleIaiGiriBait>(); // baits happen ~0.6s before aoe
        ComponentCondition<IaiGiriResolve>(id + 0x40u, 2.3f, static comp => comp.NumCasts > 0, "Cleave back");
        ComponentCondition<IaiGiriResolve>(id + 0x50u, 3.1f, static comp => comp.NumCasts > 4, "Cleave sides")
            .DeactivateOnExit<IaiGiriResolve>();
        ComponentCondition<IronRainStorm>(id + 0x60u, 0.7f, static comp => comp.NumCasts > 5, "AOE resolve")
            .DeactivateOnExit<IronRainStorm>();
    }
}

sealed class C023NMokoStates(BossModule module) : C023MokoStates(module, false);
sealed class C023SMokoStates(BossModule module) : C023MokoStates(module, true);
