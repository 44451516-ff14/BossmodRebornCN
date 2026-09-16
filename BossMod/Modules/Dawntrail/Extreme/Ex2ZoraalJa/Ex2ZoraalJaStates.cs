namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class Ex2ZoraalJaStates : StateMachineBuilder
{
    public Ex2ZoraalJaStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Actualize(id, 10.7f);
        MultidirectionalDivideHalf(id + 0x10000u, 5.6f);
        MultidirectionalDivideRegicidalRage(id + 0x20000u, 4.5f);
        DawnOfAnAge1(id + 0x30000u, 8.4f);
        ProjectionOfTriumph1(id + 0x40000u, 7.2f);
        ProjectionOfTurmoil1(id + 0x50000u, 7.2f);
        DawnOfAnAge2(id + 0x60000u, 8.4f);
        ProjectionOfTriumph2(id + 0x70000u, 8.3f);
        ProjectionOfTurmoil2(id + 0x80000u, 7.2f);
        DawnOfAnAge3(id + 0x90000u, 8.4f);
        MultidirectionalDivideHalf(id + 0xA0000u, 8.3f);
        Cast(id + 0xB0000u, AID.Enrage, 5.3f, 10, "Enrage");
    }

    private void Actualize(uint id, float delay)
    {
        Cast(id, AID.Actualize, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MultidirectionalDivideHalf(uint id, float delay)
    {
        Cast(id, AID.MultidirectionalDivide, delay, 5f, "Cross")
            .ActivateOnEnter<MultidirectionalDivide>()
            .DeactivateOnExit<MultidirectionalDivide>();
        CastStartMulti(id + 0x10u, [AID.ForwardHalfR, AID.ForwardHalfL, AID.BackwardHalfR, AID.BackwardHalfL], 3.1f)
            .ActivateOnEnter<MultidirectionalDivideMain>()
            .ActivateOnEnter<MultidirectionalDivideExtra>();
        CastEnd(id + 0x11u, 8f, "Criss-cross") // criss-cross resolves around the cast end
            .ActivateOnEnter<ForwardBackwardHalf>()
            .DeactivateOnExit<MultidirectionalDivideMain>()
            .DeactivateOnExit<MultidirectionalDivideExtra>();
        ComponentCondition<ForwardBackwardHalf>(id + 0x20u, 1.1f, static comp => comp.NumCasts > 0, "Cleaves")
            .DeactivateOnExit<ForwardBackwardHalf>();
    }

    private void MultidirectionalDivideRegicidalRage(uint id, float delay)
    {
        Cast(id, AID.MultidirectionalDivide, delay, 5f, "Cross")
            .ActivateOnEnter<MultidirectionalDivide>()
            .DeactivateOnExit<MultidirectionalDivide>();
        CastStart(id + 0x10u, AID.RegicidalRage, 3.2f)
            .ActivateOnEnter<MultidirectionalDivideMain>()
            .ActivateOnEnter<MultidirectionalDivideExtra>()
            .ActivateOnEnter<RegicidalRage>(); // tethers appear ~0.1s before cast starts
        ComponentCondition<MultidirectionalDivideMain>(id + 0x11u, 7.8f, static comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<MultidirectionalDivideMain>()
            .DeactivateOnExit<MultidirectionalDivideExtra>();
        CastEnd(id + 0x12u, 0.2f);
        ComponentCondition<RegicidalRage>(id + 0x13u, 0.1f, static comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<RegicidalRage>();
    }

    private void HalfFull(uint id, float delay)
    {
        CastMulti(id, [AID.HalfFullR, AID.HalfFullL], delay, 6f)
            .ActivateOnEnter<HalfFull>();
        ComponentCondition<HalfFull>(id + 2u, 0.3f, static comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfFull>();
    }

    private void DawnOfAnAge(uint id, float delay)
    {
        Cast(id, AID.DawnOfAnAge, delay, 7u, "Raidwide + small arena")
            .ActivateOnEnter<DawnOfAnAgeArenaChange>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DutysEdgeResolve(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<DutysEdge>(id + 1u, 0.4f, static comp => comp.NumCasts >= 1, "Line stack 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 2u, 2.1f, static comp => comp.NumCasts >= 2, "Line stack 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 3u, 2.1f, static comp => comp.NumCasts >= 3, "Line stack 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DutysEdge>(id + 4u, 2.1f, static comp => comp.NumCasts >= 4, "Line stack 4")
            .DeactivateOnExit<DutysEdge>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge1(uint id, float delay)
    {
        DawnOfAnAge(id, delay);

        Cast(id + 0x1000u, AID.VollokSmall, 10.2f, 4)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>();
        Cast(id + 0x1010u, AID.Sync, 5.4f, 5);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1020u, 0.9f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<ChasmOfVollokFangSmall>();
        CastStartMulti(id + 0x1030u, [AID.HalfFullR, AID.HalfFullL], 2.3f);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1031u, 5.8f, static comp => comp.NumCasts > 0, "Swords")
            .ActivateOnEnter<HalfFull>()
            .DeactivateOnExit<ChasmOfVollokFangSmall>();
        CastEnd(id + 0x1032u, 0.2f);
        ComponentCondition<HalfFull>(id + 0x1033u, 0.3f, static comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfFull>();

        Cast(id + 0x2000u, AID.GreaterGateway, 4.9f, 4)
            .ActivateOnEnter<ForgedTrack>(); // envc happen ~0.9s after cast end
        Cast(id + 0x2010u, AID.BladeWarp, 4.2f, 4)
            .ActivateOnEnter<ForgedTrackKnockback>();
        Cast(id + 0x2020u, AID.ForgedTrack, 4.2f, 4);
        ComponentCondition<ForgedTrack>(id + 0x2030u, 8.2f, static comp => comp.NumCasts > 0, "Lanes") // wide aoe happens ~0.2s later, knockback ~0.6s later
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before lanes resolve
            .ExecOnExit<ChasmOfVollokPlayer>(static comp => comp.Active = true);

        CastStart(id + 0x3000u, AID.Actualize, 4.9f, "Cells")
            .DeactivateOnExit<ForgedTrackKnockback>()
            .DeactivateOnExit<ForgedTrack>()
            .DeactivateOnExit<ChasmOfVollokPlayer>(); // this resolves right as cast starts
        CastEnd(id + 0x3001u, 5f, "Raidwide + normal arena")
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge2(uint id, float delay)
    {
        DawnOfAnAge(id, delay);
        ComponentCondition<DrumOfVollokPlatforms>(id + 0x10u, 4.9f, static comp => comp.Active)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<DrumOfVollokPlatforms>()
            .DeactivateOnExit<DrumOfVollokPlatforms>();

        Cast(id + 0x1000u, AID.DrumOfVollok, 5.3f, 7.4f)
            .ActivateOnEnter<DrumOfVollok>()
            .ActivateOnEnter<DrumOfVollokKnockback>();
        ComponentCondition<DrumOfVollok>(id + 0x1010u, 0.6f, static comp => comp.NumFinishedStacks > 0, "Enumeration")
            .DeactivateOnExit<DrumOfVollok>()
            .DeactivateOnExit<DrumOfVollokKnockback>();

        Cast(id + 0x2000u, AID.VollokLarge, 6.1f, 5f)
            .ActivateOnEnter<ChasmOfVollokFangLarge>();
        Cast(id + 0x2010u, AID.Sync, 4.2f, 5f);
        CastStart(id + 0x2020u, AID.AeroIII, 4.2f)
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before cast start
            .ExecOnEnter<ChasmOfVollokPlayer>(static comp => comp.Active = true);
        ComponentCondition<ChasmOfVollokFangLarge>(id + 0x2021u, 4.7f, static comp => comp.NumCasts > 0, "Swords")
            .DeactivateOnExit<ChasmOfVollokFangLarge>();
        ComponentCondition<ChasmOfVollokPlayer>(id + 0x2022u, 0.1f, static comp => comp.NumCasts > 0, "Cells")
            .DeactivateOnExit<ChasmOfVollokPlayer>();
        CastEnd(id + 0x2023u, 0.2f);
        ComponentCondition<AeroIII>(id + 0x2030u, 1.2f, static comp => comp.Voidzones.Count > 0)
            .ActivateOnEnter<AeroIII>();

        CastMulti(id + 0x3000u, [AID.ForwardHalfLongR, AID.ForwardHalfLongL, AID.BackwardHalfLongR, AID.BackwardHalfLongL], 5.2f, 9f)
            .ActivateOnEnter<ForwardBackwardHalf>();
        ComponentCondition<ForwardBackwardHalf>(id + 0x3010u, 1.1f, static comp => comp.NumCasts > 0, "Cleaves")
            .DeactivateOnExit<ForwardBackwardHalf>();

        CastStart(id + 0x4000u, AID.DutysEdge, 2.1f)
            .ActivateOnEnter<DutysEdge>();
        DutysEdgeResolve(id + 0x4010u, 4.9f);

        Cast(id + 0x5000u, AID.BurningChains, 2.1f, 5f, "Chains")
            .ActivateOnEnter<BurningChains>();
        Cast(id + 0x6000u, AID.Actualize, 15.2f, 5f, "Raidwide + normal arena")
            .DeactivateOnExit<BurningChains>()
            .DeactivateOnExit<AeroIII>()
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .OnExit(() => Module.Arena.Center = new(100f, 100f))
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DawnOfAnAge3(uint id, float delay)
    {
        DawnOfAnAge(id, delay);

        Cast(id + 0x1000u, AID.VollokSmall, 10.2f, 4f);
        Cast(id + 0x1010u, AID.Sync, 4.2f, 5f);
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1020u, 0.9f, static comp => comp.AOEs.Count > 0)
            .DeactivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<ChasmOfVollokFangSmall>();
        ComponentCondition<ChasmOfVollokFangSmall>(id + 0x1030u, 8f, static comp => comp.NumCasts > 0, "Swords")
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~2.7s before swords resolve
            .DeactivateOnExit<ChasmOfVollokFangSmall>();
        CastStart(id + 0x1040u, AID.DutysEdge, 3.3f, "Cells") // player cells resolve together with cast start
            .ExecOnEnter<ChasmOfVollokPlayer>(static comp => comp.Active = true)
            .ActivateOnEnter<DutysEdge>()
            .DeactivateOnExit<ChasmOfVollokPlayer>();
        DutysEdgeResolve(id + 0x1050u, 4.9f);

        Cast(id + 0x2000u, AID.GreaterGateway, 5.1f, 4f)
            .ActivateOnEnter<ForgedTrack>(); // envc happen ~0.9s after cast end
        Cast(id + 0x2010u, AID.BladeWarp, 4.2f, 4f)
            .ActivateOnEnter<ForgedTrackKnockback>();
        Cast(id + 0x2020u, AID.ForgedTrack, 4.2f, 4f);
        ComponentCondition<ForgedTrack>(id + 0x2030u, 8.2f, static comp => comp.NumCasts > 0, "Lanes") // wide aoe happens ~0.2s later, knockback ~0.6s later
            .ActivateOnEnter<ChasmOfVollokPlayer>() // icons appear ~1.2s before lanes resolve
            .ExecOnExit<ChasmOfVollokPlayer>(static comp => comp.Active = true);

        CastStart(id + 0x3000u, AID.Actualize, 4.9f, "Cells")
            .DeactivateOnExit<ForgedTrackKnockback>()
            .DeactivateOnExit<ForgedTrack>()
            .DeactivateOnExit<ChasmOfVollokPlayer>(); // this resolves right as cast starts
        CastEnd(id + 0x3001u, 5f, "Raidwide + normal arena")
            .OnExit(() => Module.Arena.Bounds = Trial.T02ZoraalJa.ZoraalJa.GetDefaultBounds())
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ProjectionOfTriumph1(uint id, float delay)
    {
        Cast(id, AID.ProjectionOfTriumph, delay, 5f)
            .ActivateOnEnter<ProjectionOfTriumph>();
        Cast(id + 0x10u, AID.ProjectionOfTriumph, 5.2f, 5f, "Swords 1"); // first set of circles/donuts happen right before cast ends
        ComponentCondition<ProjectionOfTriumph>(id + 0x20u, 4.9f, static comp => comp.NumCasts >= 16, "Swords 2");
        CastStartMulti(id + 0x30u, [AID.ForwardHalfR, AID.ForwardHalfL, AID.BackwardHalfR, AID.BackwardHalfL], 4.5f);
        ComponentCondition<ProjectionOfTriumph>(id + 0x31u, 0.6f, static comp => comp.NumCasts >= 32, "Swords 3")
            .ActivateOnEnter<ForwardBackwardHalf>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x40u, 5.0f, static comp => comp.NumCasts >= 48, "Swords 4");
        CastEnd(id + 0x41u, 2.4f);
        ComponentCondition<ForwardBackwardHalf>(id + 0x42u, 1.1f, static comp => comp.NumCasts > 0, "Cleaves")
           .DeactivateOnExit<ForwardBackwardHalf>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x50u, 1.5f, static comp => comp.NumCasts >= 56, "Swords 5");
        CastStart(id + 0x60u, AID.Actualize, 3.6f);
        ComponentCondition<ProjectionOfTriumph>(id + 0x61u, 1.5f, static comp => comp.NumCasts >= 64, "Swords 6")
            .DeactivateOnExit<ProjectionOfTriumph>();
        CastEnd(id + 0x62u, 3.5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ProjectionOfTriumph2(uint id, float delay)
    {
        Cast(id, AID.ProjectionOfTriumph, delay, 5f)
            .ActivateOnEnter<ProjectionOfTriumph>();
        Cast(id + 0x10u, AID.ProjectionOfTriumph, 5.2f, 5f, "Swords 1"); // first set of circles/donuts happen right before cast ends
        CastStartMulti(id + 0x20u, [AID.HalfCircuitCircle, AID.HalfCircuitDonut], 4.9f, "Swords 2"); // second set of circles/donuts happen together with cast start
        ComponentCondition<ProjectionOfTriumph>(id + 0x30u, 5.1f, static comp => comp.NumCasts >= 32, "Swords 3")
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<HalfCircuitDonut>()
            .ActivateOnEnter<HalfCircuitCircle>();
        CastEnd(id + 0x40, 1.9f, "In/out")
            .DeactivateOnExit<HalfCircuitDonut>()
            .DeactivateOnExit<HalfCircuitCircle>();
        ComponentCondition<HalfCircuitRect>(id + 0x41u, 0.3f, static comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HalfCircuitRect>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x50u, 2.8f, static comp => comp.NumCasts >= 48, "Swords 4");
        ComponentCondition<ProjectionOfTriumph>(id + 0x60u, 5f, static comp => comp.NumCasts >= 56, "Swords 5");
        CastStart(id + 0x61u, AID.RegicidalRage, 0.1f)
            .ActivateOnEnter<RegicidalRage>();
        ComponentCondition<ProjectionOfTriumph>(id + 0x70u, 5.0f, static comp => comp.NumCasts >= 64, "Swords 6")
            .DeactivateOnExit<ProjectionOfTriumph>();
        CastEnd(id + 0x71u, 3);
        ComponentCondition<RegicidalRage>(id + 0x72u, 0.1f, static comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<RegicidalRage>();
    }

    private State BitterWhirlwind(uint id, float delay)
    {
        Cast(id, AID.BitterWhirlwind, delay, 5f, "Tankbuster 1")
            .ActivateOnEnter<BitterWhirlwind>();
        ComponentCondition<BitterWhirlwind>(id + 0x10u, 3.3f, static comp => comp.NumCasts >= 2, "Tankbuster 2");
        return ComponentCondition<BitterWhirlwind>(id + 0x11u, 3.1f, static comp => comp.NumCasts >= 3, "Tankbuster 3")
            .DeactivateOnExit<BitterWhirlwind>();
    }

    private void ProjectionOfTurmoil1(uint id, float delay)
    {
        Cast(id, AID.ProjectionOfTurmoil, delay, 5f, "Moving line with stacks")
            .ActivateOnEnter<ProjectionOfTurmoil>();
        BitterWhirlwind(id + 0x100u, 45.3f)
            .DeactivateOnExit<ProjectionOfTurmoil>();
    }

    private void ProjectionOfTurmoil2(uint id, float delay)
    {
        Cast(id, AID.ProjectionOfTurmoil, delay, 5f, "Moving line with stacks")
            .ActivateOnEnter<ProjectionOfTurmoil>();
        HalfFull(id + 0x100u, 16.6f);
        HalfFull(id + 0x200u, 2.9f);
        HalfFull(id + 0x300u, 4.1f);
        BitterWhirlwind(id + 0x400u, 3.9f)
            .DeactivateOnExit<ProjectionOfTurmoil>();
    }
}
