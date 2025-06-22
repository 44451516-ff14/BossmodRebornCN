namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class FTB1DemonTabletStates : StateMachineBuilder
{
    public FTB1DemonTabletStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        DemonicDarkII(id, 10f);
        RayOfDangersNearExpulsionAfar(id + 0x10000u, 15.8f);
        RayOfDangersNearExpulsionAfar(id + 0x20000u, 13.2f);
        OccultChisel(id + 0x30000u, 14.1f);
        DemonographOfDangersNearExpulsionAfar(id + 0x40000u, 16.7f);
        Rotation(id + 0x50000u, 14.1f);
        CometeorOfDangersNearExpulsionAfar(id + 0x60000u, 5.1f);
        Rotation(id + 0x70000u, 14.4f);
        Summon(id + 0x80000u, 15.1f);
        GravityOfDangersNearExpulsionAfar(id + 0x90000u, 45f);
        Rotation(id + 0xA0000u, 15f);
        CometeorOfDangersNearExpulsionAfar(id + 0xB0000u, 5.1f);
        RayOfDangersNearExpulsionAfar(id + 0xC0000u, 16f);
        OccultChisel(id + 0xD0000u, 14.2f);
        DemonicDarkII(id + 0xE0000u, 11.9f);
        SimpleState(id + 0xF0000u, 22.3f, "Enrage");
    }

    private void DemonicDarkII(uint id, float delay)
    {
        ComponentCondition<DemonicDarkII>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<DemonicDarkII>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<DemonicDarkII>();
    }

    private void RayOfDangersNearExpulsionAfar(uint id, float delay)
    {
        ComponentCondition<RayOfIgnorance>(id, delay, comp => comp.NumCasts != 0, "Rect AOEs + maybe Knockback")
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<LandingMedium>()
            .ActivateOnEnter<LandingSmall>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<LandingMedium>()
            .DeactivateOnExit<LandingSmall>()
            .DeactivateOnExit<RayOfIgnorance>();
    }

    private void OccultChisel(uint id, float delay)
    {
        ComponentCondition<OccultChisel>(id, delay, comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<OccultChisel>()
            .DeactivateOnExit<OccultChisel>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void DemonographOfDangersNearExpulsionAfar(uint id, float delay)
    {
        ComponentCondition<LandingSmall>(id, delay, comp => comp.NumCasts != 0, "Rect AOEs + maybe Knockback")
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<DemonographTowers>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<LandingMedium>()
            .ActivateOnEnter<LandingSmall>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<LandingMedium>()
            .DeactivateOnExit<LandingSmall>()
            .DeactivateOnExit<RayOfIgnorance>();
        ComponentCondition<DemonographTowers>(id + 0x10u, 4.5f, comp => comp.NumCasts != 0, "Towers resolve")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DemonographTowers>();
    }

    private void Rotation(uint id, float delay)
    {
        ComponentCondition<RotationBig>(id, delay, comp => comp.NumCasts != 0, "Boss rotates 1")
            .ActivateOnEnter<LacunateStream>()
            .ActivateOnEnter<RotationBig>()
            .ActivateOnEnter<RotationSmall>();
        ComponentCondition<RotationBig>(id + 0x10u, 12.1f, comp => comp.NumCasts == 4, "Boss rotates 2")
            .DeactivateOnExit<RotationBig>()
            .DeactivateOnExit<RotationSmall>();
        ComponentCondition<LacunateStream>(id + 0x20u, 4.2f, comp => comp.NumCasts == 1, "Half-room cleave start");
        ComponentCondition<LacunateStream>(id + 0x30u, 4.2f, comp => comp.NumCasts == 5, "Half-room cleave end")
            .DeactivateOnExit<LacunateStream>();
    }

    private void CometeorOfDangersNearExpulsionAfar(uint id, float delay)
    {
        ComponentCondition<PortentousCometeorBait>(id, delay, comp => comp.CurrentBaits.Count != 0, "Baited meteors appear")
            .ActivateOnEnter<PortentousCometeorBait>();
        ComponentCondition<LandingSmall>(id + 0x10u, 10.5f, comp => comp.NumCasts != 0, "Rect AOEs + maybe Knockback")
            .ActivateOnEnter<PortentousComet1>()
            .ActivateOnEnter<PortentousComet2>()
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<PortentousCometKnockback>()
            .ActivateOnEnter<LandingMedium>()
            .ActivateOnEnter<LandingSmall>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<RayOfIgnorance>()
            .DeactivateOnExit<LandingMedium>()
            .DeactivateOnExit<LandingSmall>();
        ComponentCondition<PortentousCometeorBait>(id + 0x20u, 1.7f, comp => comp.CurrentBaits.Count == 0, "Baits resolve")
            .ActivateOnEnter<PortentousCometeor>()
            .DeactivateOnExit<PortentousCometeorBait>();
        ComponentCondition<PortentousComet1>(id + 0x30u, 5f, comp => comp.NumFinishedStacks != 0, "Knockback stacks resolve")
            .DeactivateOnExit<PortentousComet1>()
            .DeactivateOnExit<PortentousComet2>()
            .DeactivateOnExit<PortentousCometKnockback>();
        ComponentCondition<PortentousCometeor>(id + 0x40u, 7f, comp => comp.NumCasts != 0, "Comet AOEs resolve")
            .DeactivateOnExit<PortentousCometeor>();
    }

    private void Summon(uint id, float delay)
    {
        ComponentCondition<Summon>(id, delay, comp => comp.NumCasts != 0, "Rect AOE and add phase starts")
            .ActivateOnEnter<Summon>()
            .DeactivateOnExit<Summon>();
        ComponentCondition<SummonedDemons>(id + 0x10u, 1f, comp => comp.ActiveActors.Count != 0, "Adds targetable")
            .ActivateOnEnter<DarkDefenses>()
            .ActivateOnEnter<SummonedDemons>();
        ComponentCondition<SummonedDemons>(id + 0x20u, 45f, comp => comp.ActiveActors.Count == 0, "Add phase end")
            .DeactivateOnExit<DarkDefenses>()
            .DeactivateOnExit<SummonedDemons>();
    }

    private void GravityOfDangersNearExpulsionAfar(uint id, float delay)
    {
        ComponentCondition<GravityTowers>(id, delay, comp => comp.Active, "Towers appear")
            .ActivateOnEnter<GravityTowers>();
        ComponentCondition<LandingSmall>(id + 0x10u, 11.7f, comp => comp.NumCasts != 0, "Rect AOEs + maybe Knockback")
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<EraseGravity>()
            .ActivateOnEnter<LandingMedium>()
            .ActivateOnEnter<LandingSmall>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<LandingMedium>()
            .DeactivateOnExit<RayOfIgnorance>()
            .DeactivateOnExit<LandingSmall>();
        ComponentCondition<EraseGravity>(id + 0x20u, 3.5f, comp => comp.NumCasts != 0, "Levitation buffs")
            .DeactivateOnExit<EraseGravity>();
        ComponentCondition<GravityTowers>(id + 0x30u, 5.7f, comp => comp.NumCasts != 0, "Towers resolve")
            .DeactivateOnExit<GravityTowers>();
        ComponentCondition<LandingCircle>(id + 0x40u, 6.4f, comp => comp.NumCasts != 0, "Circle AOEs")
            .ActivateOnEnter<LandingCircle>()
            .DeactivateOnExit<LandingCircle>();
    }
}
