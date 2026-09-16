namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

sealed class P12S2PallasAthenaStates : StateMachineBuilder
{
    public P12S2PallasAthenaStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Ultima(id, 6.2f);
        PalladianGrasp(id + 0x10000u, 7.2f);
        Gaiaochos1(id + 0x20000u, 6.1f);
        ClassicalConcepts1(id + 0x30000u, 5.0f);
        Ultima(id + 0x40000u, 1.6f);
        CrushHelm(id + 0x50000u, 4.2f);
        CaloricTheory1(id + 0x60000u, 6.1f);
        Ekpyrosis(id + 0x70000u, 6.3f);
        Pangenesis(id + 0x80000u, 7.2f);
        ClassicalConcepts2(id + 0x90000u, 6.2f);
        Ultima(id + 0xA0000u, 1.6f);
        CrushHelm(id + 0xB0000u, 4.2f);
        CaloricTheory2(id + 0xC0000u, 6.1f);
        Gaiaochos2(id + 0xD0000u, 7.2f);
        Cast(id + 0xE0000u, AID.Ignorabimus, 13.8f, 15f, "Enrage");
    }

    private State Ultima(uint id, float delay)
    {
        return Cast(id, AID.UltimaNormal, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void PalladianGraspResolve(uint id, float delay)
    {
        ComponentCondition<PalladianGrasp>(id, delay, static comp => comp.NumCasts >= 1, "Cleave 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x10u, AID.PalladianGrasp2, 2.1f, 1);
        ComponentCondition<PalladianGrasp>(id + 0x20u, 0.2f, static comp => comp.NumCasts >= 2, "Cleave 2")
            .DeactivateOnExit<PalladianGrasp>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void PalladianGrasp(uint id, float delay)
    {
        Cast(id, AID.PalladianGrasp1, delay, 5f)
            .ActivateOnEnter<PalladianGrasp>();
        PalladianGraspResolve(id + 0x100u, 0.2f);
    }

    private void CrushHelm(uint id, float delay)
    {
        Cast(id, AID.CrushHelm, delay, 5f);
        ComponentCondition<CrushHelm>(id + 0x10u, 4.0f, static comp => comp.NumSmallHits >= 4, "Max vuln stacks")
            .ActivateOnEnter<CrushHelm>();
        ComponentCondition<CrushHelm>(id + 0x20u, 2.1f, static comp => comp.NumLargeHits > 0, "Tankbuster")
            .DeactivateOnExit<CrushHelm>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void EkpyrosisResolve(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<EkpyrosisExaflare>(id + 0x10u, 6.8f, static comp => comp.NumCasts > 0, "Exaflare")
            .ActivateOnEnter<EkpyrosisProximityH>()
            .ActivateOnEnter<EkpyrosisProximityV>()
            .ActivateOnEnter<EkpyrosisExaflare>();
        Condition(id + 0x20u, 1.0f, () => Module.FindComponent<EkpyrosisProximityH>()?.NumCasts > 0 || Module.FindComponent<EkpyrosisProximityV>()?.NumCasts > 0, "Proximity")
            .DeactivateOnExit<EkpyrosisProximityH>()
            .DeactivateOnExit<EkpyrosisProximityV>();
        ComponentCondition<EkpyrosisSpread>(id + 0x30u, 3.1f, static comp => !comp.Active, "Spread")
            .ActivateOnEnter<EkpyrosisSpread>()
            .DeactivateOnExit<EkpyrosisSpread>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Ultima(id + 0x40u, 3.3f)
            .DeactivateOnExit<EkpyrosisExaflare>(); // last happens exaflare ~0.8s into cast
    }

    private void Ekpyrosis(uint id, float delay)
    {
        CastStart(id, AID.Ekpyrosis, delay);
        EkpyrosisResolve(id + 0x100u, 4f);
    }

    private void Gaiaochos1(uint id, float delay)
    {
        Cast(id, AID.Gaiaochos, delay, 7f, "Raidwide + small arena 1 start")
            .ActivateOnEnter<Gaiaochos>()
            .DeactivateOnExit<Gaiaochos>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (new(100f, 90f), new ArenaBoundsCircle(7f)))
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10u, AID.SummonDarkness, 8.2f, 3f);
        ComponentCondition<MissingLink>(id + 0x20u, 7.1f, static comp => comp.TethersAssigned, "Tethers")
            .ActivateOnEnter<MissingLink>()
            .ActivateOnEnter<UltimaRay>(); // PATE happens 0.8s after cast end, casts start 4.9s
        ComponentCondition<UltimaRay>(id + 0x30u, 4.8f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<UltimaRay>();
        ComponentCondition<MissingLink>(id + 0x40u, 1.3f, static comp => comp.NumCasts > 0) // note: no point in having a name here, they should resolve automatically
            .DeactivateOnExit<MissingLink>();

        Cast(id + 0x100u, AID.DemiParhelion, 6.0f, 3f)
            .ActivateOnEnter<DemiParhelion>(); // note: casts start ~0.8s after boss cast ends
        CastMulti(id + 0x110u, [AID.GeocentrismV, AID.GeocentrismC, AID.GeocentrismH], 3.2f, 7f)
            .ActivateOnEnter<Geocentrism>()
            .ActivateOnEnter<DivineExcoriation>(); // icons appear 4.9s into cast
        ComponentCondition<DemiParhelion>(id + 0x120u, 0.1f, static comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<DemiParhelion>();
        ComponentCondition<Geocentrism>(id + 0x130u, 0.5f, static comp => comp.NumCasts > 0, "Lines/donut start");
        ComponentCondition<DivineExcoriation>(id + 0x140u, 0.4f, static comp => !comp.Active, "Spreads")
            .DeactivateOnExit<DivineExcoriation>();
        ComponentCondition<Geocentrism>(id + 0x150u, 2.5f, static comp => comp.NumCasts >= comp.NumConcurrentAOEs * 6, "Lines/donut resolve")
            .DeactivateOnExit<Geocentrism>();

        Cast(id + 0x200u, AID.UltimaGaiaochos, 1.6f, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<GaiaochosEnd>(id + 0x210u, 2.2f, static comp => comp.Finished, "Small arena 1 resolve")
            .ActivateOnEnter<GaiaochosEnd>()
            .DeactivateOnExit<GaiaochosEnd>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (new(100f, 95f), new ArenaBoundsRect(20f, 15f)));
    }

    private void Gaiaochos2(uint id, float delay)
    {
        Cast(id, AID.Gaiaochos, delay, 7f, "Raidwide + small arena 2 start")
            .ActivateOnEnter<Gaiaochos>()
            .DeactivateOnExit<Gaiaochos>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (new(100f, 90f), new ArenaBoundsCircle(7f)))
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10u, AID.SummonDarkness, 8.2f, 3f);
        Cast(id + 0x20u, AID.DemiParhelion, 3.1f, 3f)
            .ActivateOnEnter<DemiParhelion>(); // note: casts start ~0.8s after boss cast ends
        CastStartMulti(id + 0x30u, [AID.GeocentrismV, AID.GeocentrismC, AID.GeocentrismH], 3.2f);
        ComponentCondition<MissingLink>(id + 0x40u, 1.8f, static comp => comp.TethersAssigned, "Tethers")
            .ActivateOnEnter<MissingLink>()
            .ActivateOnEnter<Geocentrism>()
            .ActivateOnEnter<UltimaRay>(); // cast starts 0.7s into geocentrism cast
        CastEnd(id + 0x50u, 5.2f);
        ComponentCondition<DemiParhelion>(id + 0x60u, 0.1f, static comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<DemiParhelion>();
        ComponentCondition<Geocentrism>(id + 0x70u, 0.5f, static comp => comp.NumCasts > 0, "Lines/donut start")
            .DeactivateOnExit<UltimaRay>(); // casts finish at the same time
        ComponentCondition<DivineExcoriation>(id + 0x80u, 0.3f, static comp => comp.Active)
            .ActivateOnEnter<DivineExcoriation>()
            .DeactivateOnExit<MissingLink>(); // finishes at the same time as icons appear
        ComponentCondition<Geocentrism>(id + 0x90u, 2.6f, static comp => comp.NumCasts >= comp.NumConcurrentAOEs * 6, "Lines/donut resolve")
            .DeactivateOnExit<Geocentrism>();
        ComponentCondition<DivineExcoriation>(id + 0xA0u, 0.5f, static comp => !comp.Active, "Spreads")
            .DeactivateOnExit<DivineExcoriation>();

        Cast(id + 0x100u, AID.SummonDarkness, 5.1f, 3f);
        CastStart(id + 0x110u, AID.UltimaGaiaochos, 10.2f)
            .ActivateOnEnter<UltimaBlow>(); // tethers appear 2.8s after previous cast end
        ComponentCondition<UltimaBlow>(id + 0x120u, 0.8f, static comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<UltimaBlow>();
        CastEnd(id + 0x130u, 4.2f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x200u, AID.UltimaGaiaochos, 9.2f)
            .ActivateOnEnter<UltimaBlow>(); // tethers appear 1.7s after previous cast end
        ComponentCondition<UltimaBlow>(id + 0x210u, 0.6f, static comp => comp.NumCasts > 0, "Charges")
            .DeactivateOnExit<UltimaBlow>();
        CastEnd(id + 0x220u, 4.4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State PalladianRayResolve(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<PalladianRayBait>(id + 0x10u, 2.7f, static comp => comp.NumCasts > 0, "Cones bait")
            .ActivateOnEnter<PalladianRayAOE>()
            .DeactivateOnExit<PalladianRayBait>();
        ComponentCondition<PalladianRayAOE>(id + 0x20u, 1.6f, static comp => comp.NumCasts > 0); // first non-baited hit
        return ComponentCondition<PalladianRayAOE>(id + 0x30u, 2.3f, static comp => comp.NumCasts >= 5 * comp.NumConcurrentAOEs, "Cones resolve")
            .DeactivateOnExit<PalladianRayAOE>();
    }

    private void ClassicalConcepts1(uint id, float delay)
    {
        Cast(id, AID.ClassicalConcepts, delay, 7f, "Raidwide + playstation 1 start")
            .ActivateOnEnter<ClassicalConcepts1>() // icons appear ~0.9s after cast end, concepts spawn ~1.1s after cast end, tethers between players ~5s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ClassicalConcepts>(id + 0x10u, 5.0f, static comp => comp.NumPlayerTethers > 0, "Player tethers");
        ComponentCondition<ClassicalConcepts>(id + 0x20u, 7.8f, static comp => comp.NumShapeTethers > 0, "Shape tethers");
        ComponentCondition<ClassicalConcepts>(id + 0x30u, 3.0f, static comp => comp.NumShapeTethers == 0, "Tethers resolve")
            .SetHint(StateMachine.StateHint.Raidwide); // note: actual damage effects are randomly staggered over ~1s after tethers disappear

        ComponentCondition<Implode>(id + 0x100u, 2.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Implode>()
            .DeactivateOnExit<ClassicalConcepts>(); // note: it fully deactivates slightly later...
        CastStart(id + 0x110u, AID.PalladianRay, 1.6f)
            .ActivateOnEnter<PalladianRayBait>(); // TODO: reconsider activation point?
        ComponentCondition<Implode>(id + 0x120u, 1.4f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<Implode>();
        PalladianRayResolve(id + 0x200u, 0.6f);
    }

    private void ClassicalConcepts2(uint id, float delay)
    {
        Cast(id, AID.ClassicalConcepts, delay, 7f, "Raidwide + playstation 2 start")
            .ActivateOnEnter<ClassicalConcepts2>() // icons appear and concepts spawn ~0.9s after cast end, tethers between players ~5s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10u, AID.PantaRhei, 7.2f, 10f);
        // +0.8s: shapes teleport to mirrored positions
        ComponentCondition<ClassicalConcepts>(id + 0x20u, 2.6f, static comp => comp.NumShapeTethers > 0, "Shape tethers");
        CastStart(id + 0x30u, AID.PalladianRay, 1.6f)
            .ActivateOnEnter<PalladianRayBait>();
        ComponentCondition<ClassicalConcepts>(id + 0x40u, 1.4f, static comp => comp.NumShapeTethers == 0, "Tethers resolve")
            .ActivateOnEnter<Implode>() // note: starts ~2.2s after palladian ray cast end
            .DeactivateOnExit<ClassicalConcepts>()
            .SetHint(StateMachine.StateHint.Raidwide); // note: actual damage effects are randomly staggered over ~1s after tethers disappear
        PalladianRayResolve(id + 0x100u, 0.6f)
            .DeactivateOnExit<Implode>(); // note: ends ~1.3s before ray resolve
    }

    private void CaloricTheory1(uint id, float delay)
    {
        CastStart(id, AID.CaloricTheory, delay)
            .ActivateOnEnter<CaloricTheory1Part1>(); // icons appear right before cast start
        CastEnd(id + 1u, 8f, "Raidwide + caloric 1 start")
            .DeactivateOnExit<CaloricTheory1Part1>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<CaloricTheory1Part2>(id + 0x10u, 0.8f, static comp => comp.Active)
            .ActivateOnEnter<CaloricTheory1Part2>();
        ComponentCondition<CaloricTheory1Part2>(id + 0x11u, 12f, static comp => !comp.Active, "Fire stacks")
            .DeactivateOnExit<CaloricTheory1Part2>();

        ComponentCondition<CaloricTheory1Part3>(id + 0x20u, 0.6f, static comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<CaloricTheory1Part3>();
        ComponentCondition<CaloricTheory1Part3>(id + 0x30u, 10.8f, static comp => !comp.Active, "Caloric 1 resolve")
            .DeactivateOnExit<CaloricTheory1Part3>(); // note: spreads resolve ~0.2s earlier
        // TODO: movement debuffs disappear ~1.7s later
    }

    private void CaloricTheory2(uint id, float delay)
    {
        CastStart(id, AID.CaloricTheory, delay)
            .ActivateOnEnter<CaloricTheory2Part1>(); // icons appear right before cast start
        CastEnd(id + 1u, 8f, "Raidwide + caloric 2 start")
            .DeactivateOnExit<CaloricTheory2Part1>()
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x100u, AID.Ekpyrosis, 26.2f)
            .ActivateOnEnter<CaloricTheory2Part2>()
            .ActivateOnEnter<EntropicExcess>();
        ComponentCondition<CaloricTheory2Part2>(id + 0x110u, 1.7f, static comp => comp.Done, "Caloric 2 resolve")
            .DeactivateOnExit<EntropicExcess>()
            .DeactivateOnExit<CaloricTheory2Part2>();
        // TODO: movement debuffs disappear ~0.8s later
        EkpyrosisResolve(id + 0x200u, 2.3f);
    }

    private void Pangenesis(uint id, float delay)
    {
        Cast(id, AID.Pangenesis, delay, 7f, "Raidwide + towers start")
            .ActivateOnEnter<Pangenesis>() // statuses appear 0.7s after cast end
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10u, AID.Pantheos, 4.1f, 4);
        ComponentCondition<Pangenesis>(id + 0x20u, 5.8f, static comp => comp.NumCasts >= 2, "Towers 1");
        ComponentCondition<Pangenesis>(id + 0x30u, 5.0f, static comp => comp.NumCasts >= 6, "Towers 2");
        ComponentCondition<Pangenesis>(id + 0x40u, 5.0f, static comp => comp.NumCasts >= 10, "Towers 3")
            .DeactivateOnExit<Pangenesis>();

        // next mechanic (palladian grasp) overlaps with resolve
        CastStart(id + 0x1000u, AID.PalladianGrasp1, 10.4f)
            .ActivateOnEnter<FactorIn>(); // tethers appear ~5.3s after towers resolve
        ComponentCondition<FactorIn>(id + 0x1010u, 2.0f, static comp => comp.NumCasts > 0, "Slime baits")
            .ActivateOnEnter<PalladianGrasp>()
            .DeactivateOnExit<FactorIn>();
        CastEnd(id + 0x1020u, 3.0f);
        PalladianGraspResolve(id + 0x1100u, 0.2f);
    }
}
