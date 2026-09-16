namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

abstract class C032LalaStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C032LalaStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        InfernoTheorem(id, 5.2f);
        AngularAdditionArcaneBlight(id + 0x10000u, 4.4f);
        Analysis(id + 0x20000u, 5.4f);
        StrategicStrike(id + 0x30000u, 5.6f);
        PlanarTactics(id + 0x40000u, 8.2f);
        StrategicStrike(id + 0x50000u, 5.2f);
        SpatialTactics(id + 0x60000u, 10.5f);
        InfernoTheorem(id + 0x70000u, 6.2f);
        SymmetricSurge(id + 0x80000u, 7.2f);
        StrategicStrike(id + 0x90000u, 8.0f);
        InfernoTheorem(id + 0xA0000u, 3.2f);
        Analysis(id + 0xB0000u, 9.5f);
        StrategicStrike(id + 0xC0000u, 5.7f);
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private State InfernoTheorem(uint id, float delay)
    {
        return Cast(id, _savage ? AID.SInfernoTheorem : AID.NInfernoTheorem, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StrategicStrike(uint id, float delay)
    {
        Cast(id, _savage ? AID.SStrategicStrike : AID.NStrategicStrike, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void AngularAddition(uint id, float delay)
    {
        CastMulti(id, [_savage ? AID.SAngularAdditionThree : AID.NAngularAdditionThree, _savage ? AID.SAngularAdditionFive : AID.NAngularAdditionFive], delay, 3f);
    }

    private State ArcaneBlight(uint id, float delay)
    {
        return CastMulti(id, [_savage ? AID.SArcaneBlightFront : AID.NArcaneBlightFront, _savage ? AID.SArcaneBlightBack : AID.NArcaneBlightBack, _savage ? AID.SArcaneBlightLeft
            : AID.NArcaneBlightLeft, _savage ? AID.SArcaneBlightRight : AID.NArcaneBlightRight], delay, 6f, "Cleave")
            .ActivateOnEnter<NArcaneBlight>(!_savage)
            .ActivateOnEnter<SArcaneBlight>(_savage)
            .DeactivateOnExit<ArcaneBlight>();
    }

    private void AngularAdditionArcaneBlight(uint id, float delay)
    {
        AngularAddition(id, delay);
        ArcaneBlight(id + 0x10u, 2.1f);
    }

    private void Analysis(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAnalysis : AID.NAnalysis, delay, 3)
            .ActivateOnEnter<Analysis>();
        Cast(id + 0x10u, _savage ? AID.SArcaneArray1 : AID.NArcaneArray1, 2.1f, 3f)
            .ActivateOnEnter<ArcaneArray>()
            .ActivateOnEnter<AnalysisRadiance>();
        AngularAddition(id + 0x20u, 2.1f);
        ComponentCondition<ArcaneArray>(id + 0x30u, 0.6f, static comp => comp.NumCasts >= 2);
        ComponentCondition<ArcaneArray>(id + 0x31u, 1.2f, static comp => comp.NumCasts >= 4);
        ComponentCondition<AnalysisRadiance>(id + 0x32u, 0.2f, static comp => comp.NumCasts > 0, "Orb 1 gaze");
        ArcaneBlight(id + 0x40u, 0.1f)
            .ActivateOnEnter<TargetedLight>();
        CastStart(id + 0x50u, _savage ? AID.STargetedLight : AID.NTargetedLight, 3.2f);
        ComponentCondition<ArcaneArray>(id + 0x51u, 0.1f, static comp => comp.NumCasts >= 20);
        ComponentCondition<AnalysisRadiance>(id + 0x52u, 0.2f, static comp => comp.NumCasts > 1, "Orb 2 gaze")
            .DeactivateOnExit<AnalysisRadiance>();
        CastEnd(id + 0x53u, 4.7f)
            .ExecOnEnter<TargetedLight>(static comp => comp.Active = true);
        ComponentCondition<TargetedLight>(id + 0x54u, 0.5f, static comp => comp.NumCasts > 0, "Tether gaze")
            .DeactivateOnExit<TargetedLight>()
            .DeactivateOnExit<Analysis>()
            .DeactivateOnExit<ArcaneArray>();
    }

    private void PlanarTactics(uint id, float delay)
    {
        Cast(id, _savage ? AID.SPlanarTactics : AID.NPlanarTactics, delay, 5f)
            .ActivateOnEnter<PlanarTactics>(); // debuffs appear ~0.9s after cast end
        Cast(id + 0x10u, _savage ? AID.SArcaneMine : AID.NArcaneMine, 2.1f, 13.1f)
            .ActivateOnEnter<PlanarTacticsForcedMarch>(); // icons appear ~4.8s into cast
        ComponentCondition<PlanarTacticsForcedMarch>(id + 0x20u, 1.7f, static comp => comp.NumActiveForcedMarches > 0, "Forced march")
            .DeactivateOnExit<PlanarTactics>();
        CastStart(id + 0x30u, _savage ? AID.SInfernoTheorem : AID.NInfernoTheorem, 5.4f)
            .ActivateOnEnter<SymmetricSurge>();
        ComponentCondition<SymmetricSurge>(id + 0x31u, 0.7f, static comp => !comp.Active, "Stack")
            .DeactivateOnExit<PlanarTacticsForcedMarch>()
            .DeactivateOnExit<SymmetricSurge>();
        CastEnd(id + 0x32u, 4.3f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SpatialTactics(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSpatialTactics : AID.NSpatialTactics, delay, 5f);
        Cast(id + 0x10u, _savage ? AID.SArcaneArray2 : AID.NArcaneArray2, 2.1f, 3f)
            .ActivateOnEnter<ArcaneArray>()
            .ActivateOnEnter<SpatialTactics>();
        ComponentCondition<ArcaneArray>(id + 0x20u, 5.8f, static comp => comp.NumCasts >= 2);
        ComponentCondition<ArcaneArray>(id + 0x21u, 1.2f, static comp => comp.NumCasts >= 4);
        ComponentCondition<SpatialTactics>(id + 0x22u, 0.2f, static comp => comp.NumCasts > 0, "First explosion");
        ComponentCondition<ArcaneArray>(id + 0x23u, 1.0f, static comp => comp.NumCasts >= 6);
        ComponentCondition<ArcaneArray>(id + 0x24u, 1.2f, static comp => comp.NumCasts >= 8);
        ComponentCondition<ArcaneArray>(id + 0x25u, 1.2f, static comp => comp.NumCasts >= 10);
        AngularAddition(id + 0x30u, 0.5f);
        ComponentCondition<ArcaneArray>(id + 0x40u, 1.2f, static comp => comp.NumCasts >= 18);
        ArcaneBlight(id + 0x50u, 0.9f)
            .DeactivateOnExit<SpatialTactics>();
        InfernoTheorem(id + 0x60u, 3.2f)
            .DeactivateOnExit<ArcaneArray>();
    }

    private void SymmetricSurge(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSymmetricSurge : AID.NSymmetricSurge, delay, 5f);
        Cast(id + 0x10u, _savage ? AID.SConstructiveFigure : AID.NConstructiveFigure, 2.1f, 3f)
            .ActivateOnEnter<NConstructiveFigure>(!_savage)
            .ActivateOnEnter<SConstructiveFigure>(_savage);
        Cast(id + 0x20u, _savage ? AID.SArcanePlot : AID.NArcanePlot, 2.1f, 3f);
        ComponentCondition<ArcanePlot>(id + 0x30u, 5.8f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<ArcanePlot>()
            .ActivateOnEnter<ArcanePoint>();
        Cast(id + 0x40u, _savage ? AID.SArcanePoint : AID.NArcanePoint, 3.4f, 5f);
        ComponentCondition<ConstructiveFigure>(id + 0x50u, 0.5f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<ConstructiveFigure>();
        ComponentCondition<ArcanePoint>(id + 0x51, 0.2f, static comp => comp.NumCasts > 0);
        Cast(id + 0x60u, _savage ? AID.SExplosiveTheorem : AID.NExplosiveTheorem, 3.4f, 5f, "Spread")
            .ActivateOnEnter<NExplosiveTheorem>(!_savage)
            .ActivateOnEnter<SExplosiveTheorem>(_savage)
            .DeactivateOnExit<ExplosiveTheorem>()
            .DeactivateOnExit<ArcanePoint>();
        ComponentCondition<TelluricTheorem>(id + 0x70u, 0.7f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NTelluricTheorem>(!_savage)
            .ActivateOnEnter<STelluricTheorem>(_savage)
            .ActivateOnEnter<SymmetricSurge>();
        ComponentCondition<SymmetricSurge>(id + 0x71u, 3.8f, static comp => !comp.Active, "Stack")
            .DeactivateOnExit<SymmetricSurge>();
        ComponentCondition<TelluricTheorem>(id + 0x72u, 0.7f, static comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<TelluricTheorem>()
            .DeactivateOnExit<ArcanePlot>();
    }
}

sealed class C032NLalaStates(BossModule module) : C032LalaStates(module, false);
sealed class C032SLalaStates(BossModule module) : C032LalaStates(module, true);
