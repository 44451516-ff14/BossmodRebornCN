namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

class C032LalaStates : StateMachineBuilder
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
        AngularAdditionArcaneBlight(id + 0x10000, 4.4f);
        Analysis(id + 0x20000, 5.4f);
        StrategicStrike(id + 0x30000, 5.6f);
        PlanarTactics(id + 0x40000, 8.2f);
        StrategicStrike(id + 0x50000, 5.2f);
        SpatialTactics(id + 0x60000, 10.5f);
        InfernoTheorem(id + 0x70000, 6.2f);
        SymmetricSurge(id + 0x80000, 7.2f);
        StrategicStrike(id + 0x90000, 8.0f);
        InfernoTheorem(id + 0xA0000, 3.2f);
        Analysis(id + 0xB0000, 9.5f);
        StrategicStrike(id + 0xC0000, 5.7f);
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private State InfernoTheorem(uint id, float delay)
    {
        return Cast(id, _savage ? (uint)AID.SInfernoTheorem : (uint)AID.NInfernoTheorem, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StrategicStrike(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SStrategicStrike : (uint)AID.NStrategicStrike, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void AngularAddition(uint id, float delay)
    {
        CastMulti(id, [_savage ? (uint)AID.SAngularAdditionThree : (uint)AID.NAngularAdditionThree, _savage ? (uint)AID.SAngularAdditionFive : (uint)AID.NAngularAdditionFive], delay, 3);
    }

    private State ArcaneBlight(uint id, float delay)
    {
        return CastMulti(id, [_savage ? (uint)AID.SArcaneBlightFront : (uint)AID.NArcaneBlightFront, _savage ? (uint)AID.SArcaneBlightBack : (uint)AID.NArcaneBlightBack, _savage ? (uint)AID.SArcaneBlightLeft : (uint)AID.NArcaneBlightLeft, _savage ? (uint)AID.SArcaneBlightRight : (uint)AID.NArcaneBlightRight], delay, 6, "Cleave")
            .ActivateOnEnter<NArcaneBlight>(!_savage)
            .ActivateOnEnter<SArcaneBlight>(_savage)
            .DeactivateOnExit<ArcaneBlight>();
    }

    private void AngularAdditionArcaneBlight(uint id, float delay)
    {
        AngularAddition(id, delay);
        ArcaneBlight(id + 0x10, 2.1f);
    }

    private void Analysis(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SAnalysis : (uint)AID.NAnalysis, delay, 3)
            .ActivateOnEnter<Analysis>();
        Cast(id + 0x10, _savage ? (uint)AID.SArcaneArray1 : (uint)AID.NArcaneArray1, 2.1f, 3)
            .ActivateOnEnter<ArcaneArray>()
            .ActivateOnEnter<AnalysisRadiance>();
        AngularAddition(id + 0x20, 2.1f);
        ComponentCondition<ArcaneArray>(id + 0x30, 0.6f, comp => comp.NumCasts >= 2);
        ComponentCondition<ArcaneArray>(id + 0x31, 1.2f, comp => comp.NumCasts >= 4);
        ComponentCondition<AnalysisRadiance>(id + 0x32, 0.2f, comp => comp.NumCasts > 0, "Orb 1 gaze");
        ArcaneBlight(id + 0x40, 0.1f)
            .ActivateOnEnter<TargetedLight>();
        CastStart(id + 0x50, _savage ? (uint)AID.STargetedLight : (uint)AID.NTargetedLight, 3.2f);
        ComponentCondition<ArcaneArray>(id + 0x51, 0.1f, comp => comp.NumCasts >= 20);
        ComponentCondition<AnalysisRadiance>(id + 0x52, 0.2f, comp => comp.NumCasts > 1, "Orb 2 gaze")
            .DeactivateOnExit<AnalysisRadiance>();
        CastEnd(id + 0x53, 4.7f)
            .ExecOnEnter<TargetedLight>(comp => comp.Active = true);
        ComponentCondition<TargetedLight>(id + 0x54, 0.5f, comp => comp.NumCasts > 0, "Tether gaze")
            .DeactivateOnExit<TargetedLight>()
            .DeactivateOnExit<Analysis>()
            .DeactivateOnExit<ArcaneArray>();
    }

    private void PlanarTactics(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SPlanarTactics : (uint)AID.NPlanarTactics, delay, 5)
            .ActivateOnEnter<PlanarTactics>(); // debuffs appear ~0.9s after cast end
        Cast(id + 0x10, _savage ? (uint)AID.SArcaneMine : (uint)AID.NArcaneMine, 2.1f, 13.1f)
            .ActivateOnEnter<PlanarTacticsForcedMarch>(); // icons appear ~4.8s into cast
        ComponentCondition<PlanarTacticsForcedMarch>(id + 0x20, 1.7f, comp => comp.NumActiveForcedMarches > 0, "Forced march")
            .DeactivateOnExit<PlanarTactics>();
        CastStart(id + 0x30, _savage ? (uint)AID.SInfernoTheorem : (uint)AID.NInfernoTheorem, 5.4f)
            .ActivateOnEnter<SymmetricSurge>();
        ComponentCondition<SymmetricSurge>(id + 0x31, 0.7f, comp => !comp.Active, "Stack")
            .DeactivateOnExit<PlanarTacticsForcedMarch>()
            .DeactivateOnExit<SymmetricSurge>();
        CastEnd(id + 0x32, 4.3f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SpatialTactics(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SSpatialTactics : (uint)AID.NSpatialTactics, delay, 5);
        Cast(id + 0x10, _savage ? (uint)AID.SArcaneArray2 : (uint)AID.NArcaneArray2, 2.1f, 3)
            .ActivateOnEnter<ArcaneArray>()
            .ActivateOnEnter<SpatialTactics>();
        ComponentCondition<ArcaneArray>(id + 0x20, 5.8f, comp => comp.NumCasts >= 2);
        ComponentCondition<ArcaneArray>(id + 0x21, 1.2f, comp => comp.NumCasts >= 4);
        ComponentCondition<SpatialTactics>(id + 0x22, 0.2f, comp => comp.NumCasts > 0, "First explosion");
        ComponentCondition<ArcaneArray>(id + 0x23, 1.0f, comp => comp.NumCasts >= 6);
        ComponentCondition<ArcaneArray>(id + 0x24, 1.2f, comp => comp.NumCasts >= 8);
        ComponentCondition<ArcaneArray>(id + 0x25, 1.2f, comp => comp.NumCasts >= 10);
        AngularAddition(id + 0x30, 0.5f);
        ComponentCondition<ArcaneArray>(id + 0x40, 1.2f, comp => comp.NumCasts >= 18);
        ArcaneBlight(id + 0x50, 0.9f)
            .DeactivateOnExit<SpatialTactics>();
        InfernoTheorem(id + 0x60, 3.2f)
            .DeactivateOnExit<ArcaneArray>();
    }

    private void SymmetricSurge(uint id, float delay)
    {
        Cast(id, _savage ? (uint)AID.SSymmetricSurge : (uint)AID.NSymmetricSurge, delay, 5);
        Cast(id + 0x10, _savage ? (uint)AID.SConstructiveFigure : (uint)AID.NConstructiveFigure, 2.1f, 3)
            .ActivateOnEnter<NConstructiveFigure>(!_savage)
            .ActivateOnEnter<SConstructiveFigure>(_savage);
        Cast(id + 0x20, _savage ? (uint)AID.SArcanePlot : (uint)AID.NArcanePlot, 2.1f, 3);
        ComponentCondition<ArcanePlot>(id + 0x30, 5.8f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<ArcanePlot>()
            .ActivateOnEnter<ArcanePoint>();
        Cast(id + 0x40, _savage ? (uint)AID.SArcanePoint : (uint)AID.NArcanePoint, 3.4f, 5);
        ComponentCondition<ConstructiveFigure>(id + 0x50, 0.5f, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<ConstructiveFigure>();
        ComponentCondition<ArcanePoint>(id + 0x51, 0.2f, comp => comp.NumCasts > 0);
        Cast(id + 0x60, _savage ? (uint)AID.SExplosiveTheorem : (uint)AID.NExplosiveTheorem, 3.4f, 5, "Spread")
            .ActivateOnEnter<NExplosiveTheorem>(!_savage)
            .ActivateOnEnter<SExplosiveTheorem>(_savage)
            .DeactivateOnExit<ExplosiveTheorem>()
            .DeactivateOnExit<ArcanePoint>();
        ComponentCondition<TelluricTheorem>(id + 0x70, 0.7f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NTelluricTheorem>(!_savage)
            .ActivateOnEnter<STelluricTheorem>(_savage)
            .ActivateOnEnter<SymmetricSurge>();
        ComponentCondition<SymmetricSurge>(id + 0x71, 3.8f, comp => !comp.Active, "Stack")
            .DeactivateOnExit<SymmetricSurge>();
        ComponentCondition<TelluricTheorem>(id + 0x72, 0.7f, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<TelluricTheorem>()
            .DeactivateOnExit<ArcanePlot>();
    }
}
class C032NLalaStates(BossModule module) : C032LalaStates(module, false);
class C032SLalaStates(BossModule module) : C032LalaStates(module, true);
