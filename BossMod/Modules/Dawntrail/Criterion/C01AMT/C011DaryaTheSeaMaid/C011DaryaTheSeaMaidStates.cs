namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

sealed class C011DaryaTheSeaMaidStates : StateMachineBuilder
{
    public C011DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FamilarCall(id, 7.1f);
        AlluringOrder1(id + 0x200u, 6.3f);
        CeaselessCurrent(id + 0x300u, 7.4f);
        AlluringOrder2(id + 0x400u, 7.0f);
        AquaSpear1(id + 0x500u, 5.9f);
        SunkenTreasure1(id + 0x700u, 5.2f);
        Enrage(id + 0x1000u, 1f);
    }

    private void FamilarCall(uint id, float delay)
    {
        Cast(id, AID.PiercingPlunge, delay, 5f, "Raidwide")
            .ActivateOnEnter<PiercingPlunge>()
            .DeactivateOnExit<PiercingPlunge>();

        Cast(id + 0x10u, AID.FamiliarCall, 10.4f, 3f, "Adds spawn")
            .ActivateOnEnter<EchoedSerenade>();
        Cast(id + 0x50u, AID.EchoedSerenade, 5.1f, 8.5f)
            .ActivateOnEnter<Hydrobullet>();
        ComponentCondition<EchoedSerenade>(id + 0x60u, 3.6f, static comp => comp.NumCasts > 0, "First add");
        ComponentCondition<EchoedSerenade>(id + 0x70u, 3.1f, static comp => comp.NumCasts > 1, "Second add");
        ComponentCondition<Hydrobullet>(id + 0x80u, 0.1f, hydrobullet => hydrobullet.NumFinishedSpreads > 0, "Spreads");
        ComponentCondition<EchoedSerenade>(id + 0x90u, 3.0f, static comp => comp.NumCasts > 2, "Third add");
        ComponentCondition<EchoedSerenade>(id + 0x100u, 3.2f, static comp => comp.NumCasts > 3, "Fourth add")
            .DeactivateOnExit<EchoedSerenade>();
        ComponentCondition<Hydrobullet>(id + 0x110u, 0.2f, hydrobullet => hydrobullet.NumFinishedSpreads > 4, "Spreads")
            .ActivateOnEnter<SurgingCurrent>()
            .DeactivateOnExit<Hydrobullet>();
        ComponentCondition<SurgingCurrent>(id + 0x120u, 8.0f, static comp => comp.NumCasts >= 4, "SurgingCurrent")
            .DeactivateOnExit<SurgingCurrent>();
    }

    private void AlluringOrder1(uint id, float delay)
    {
        Cast(id, AID.AlluringOrder, delay, 4f, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .DeactivateOnExit<AlluringOrder>()
            .ActivateOnEnter<AlluringOrderForcedMarch>()
            .ActivateOnEnter<Tidalspout>();
        Cast(id + 0x10u, AID.SwimmingInTheAir, 5.4f, 4f, "SwimmingInTheAir")
            .ActivateOnEnter<SwimmingInTheAir>();
        ComponentCondition<SwimmingInTheAir>(id + 0x20u, 7.0f, static comp => comp.activeAOEs >= 7, "Puddles Spawn");
        ComponentCondition<AlluringOrderForcedMarch>(id + 0x30u, 2.7f, static comp => comp.NumActiveForcedMarches > 0, "Controlled Walk");
        ComponentCondition<SwimmingInTheAir>(id + 0x40u, 3.9f, static comp => comp.NumCasts > 0, "Puddles + Stack Resolve")
            .DeactivateOnExit<SwimmingInTheAir>()
            .DeactivateOnExit<Tidalspout>()
            .DeactivateOnExit<AlluringOrderForcedMarch>();
    }

    private void CeaselessCurrent(uint id, float delay)
    {
        Cast(id + 0x10u, AID.CeaselessCurrent, delay, 4f, "CeaselessCurrent")
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SurgingCurrent2>()
            .ActivateOnEnter<CrossCurrent>();
        ComponentCondition<CeaselessCurrent>(id + 0x20u, 8.1f, static comp => comp.NumCasts > 0, "Exaflares start");
        ComponentCondition<CeaselessCurrent>(id + 0x30u, 8.3f, static comp => comp.NumCasts >= 10, "Exaflares end")
            .DeactivateOnExit<CeaselessCurrent>()
            .DeactivateOnExit<SurgingCurrent2>()
            .DeactivateOnExit<CrossCurrent>();
    }

    private void AlluringOrder2(uint id, float delay)
    {
        Cast(id, AID.PiercingPlunge, delay, 5f, "Raidwide")
            .ActivateOnEnter<PiercingPlunge>()
            .DeactivateOnExit<PiercingPlunge>();

        Cast(id + 0x10, AID.AlluringOrder, 7.2f, 4f, "Raidwide")
            .ActivateOnEnter<AlluringOrder>()
            .DeactivateOnExit<AlluringOrder>()
            .ActivateOnEnter<AlluringOrderForcedMarch>()
            .ActivateOnEnter<Tidalspout>();

        Cast(id + 0x20, AID.SunkenTreasure, 5.2f, 3f, "Spawns Spheres/Donuts")
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<SurgingCurrent2>();

        ComponentCondition<AlluringOrderForcedMarch>(id + 0x30u, 13f, static comp => comp.NumActiveForcedMarches > 0, "Controlled Walk");
        ComponentCondition<Tidalspout>(id + 0x40u, 4.0f, static comp => !comp.Active, "Sphere Shatter + Stack Resolves")
            .DeactivateOnExit<AlluringOrderForcedMarch>()
            .DeactivateOnExit<Tidalspout>();
        ComponentCondition<SurgingCurrent>(id + 0x50u, 3.4f, static comp => comp.NumCasts >= 4, "2nd SurgingCurrent Resolves")
            .DeactivateOnExit<SurgingCurrent2>();
        ComponentCondition<SunkenTreasure>(id + 0x60u, 3.4f, static comp => comp.NumCasts >= 6, "2nd Sphere Shatter")
            .DeactivateOnExit<SunkenTreasure>();
    }

    private void AquaSpear1(uint id, float delay)
    {
        Cast(id, AID.AquaSpear, delay, 3f, "Aqua Spear")
            .ActivateOnEnter<AquaSpear>();
        ComponentCondition<AquaSpear>(id + 0x10u, 6.8f, static comp => comp.NumCasts >= 4, "Spreads resolve");

        Cast(id + 0x20u, AID.FamiliarCall, 4.7f, 3, "Adds spawn")
            .ActivateOnEnter<EchoedSerenade2>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<CrossCurrent>();
        Cast(id + 0x30u, AID.EchoedSerenade, 5.1f, 8.5f);
        ComponentCondition<EchoedSerenade>(id + 0x40u, 3.6f, static comp => comp.NumCasts > 0, "First add");
        ComponentCondition<EchoedSerenade>(id + 0x50u, 3.1f, static comp => comp.NumCasts > 1, "Second add");
        ComponentCondition<EchoedSerenade>(id + 0x60u, 3.1f, static comp => comp.NumCasts > 2, "Third add");
        ComponentCondition<EchoedSerenade>(id + 0x70u, 3.1f, static comp => comp.NumCasts > 3, "Fourth add");

        Cast(id + 0x100u, AID.FamiliarCall, 13.9f, 3, "Adds spawn")
            .DeactivateOnExit<AquaBall>()
            .DeactivateOnExit<CrossCurrent>();
        Cast(id + 0x110u, AID.EchoedReprise, 6.2f, 4.0f);
        ComponentCondition<EchoedSerenade>(id + 0x120u, 3.5f, static comp => comp.NumCasts > 0, "First add");
        ComponentCondition<EchoedSerenade>(id + 0x130u, 3.1f, static comp => comp.NumCasts > 1, "Second add");
        ComponentCondition<EchoedSerenade>(id + 0x140u, 3.1f, static comp => comp.NumCasts > 2, "Third add");
        ComponentCondition<EchoedSerenade>(id + 0x150u, 3.1f, static comp => comp.NumCasts > 3, "Fourth add")
            .ActivateOnExit<SurgingCurrent>()
            .DeactivateOnExit<EchoedSerenade2>();
        ComponentCondition<SurgingCurrent>(id + 0x160u, 5.9f, static comp => comp.NumCasts >= 4, "SurgingCurrent")
            .DeactivateOnExit<SurgingCurrent>()
            .DeactivateOnExit<AquaSpear>();
    }

    private void SunkenTreasure1(uint id, float delay)
    {
        Cast(id, AID.SeaShackles, delay, 4f, "Sea Shackles")
            .ActivateOnEnter<SeaShackles>()
            .ActivateOnEnter<HydrobulletStack>();
        Cast(id + 0x10u, AID.SunkenTreasure, 3.2f, 3, "Spawns Spheres/Donuts")
            .ActivateOnEnter<SunkenTreasure2>();
        Cast(id + 0x20u, AID.AquaBall, 8.2f, 1.9f)
            .ActivateOnEnter<AquaBall>();
        ComponentCondition<AquaBall>(id + 0x30u, 1.1f, static comp => comp.Casters.Count > 0, "1st Aqua Ball Baits");
        ComponentCondition<AquaBall>(id + 0x40u, 2.0f, static comp => comp.Casters.Count >= 8, "2nd Aqua Ball Baits");
        ComponentCondition<AquaBall>(id + 0x50u, 2.5f, static comp => comp.Casters.Count >= 8 && comp.NumCasts >= 4,
            "3rd Aqua Ball Baits + 1st Spheres/Donuts resolve")
            .DeactivateOnExit<HydrobulletStack>();
        ComponentCondition<SunkenTreasure2>(id + 0x60u, 6.8f, static comp => comp.NumCasts >= 8, "2nd Sphere Shatter + Tethers resolve")
            .DeactivateOnExit<SunkenTreasure2>()
            .DeactivateOnExit<AquaBall>()
            .DeactivateOnExit<SeaShackles>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.PiercingPlunge, delay, 5f, "Raidwide")
            .ActivateOnEnter<PiercingPlunge>()
            .DeactivateOnExit<PiercingPlunge>();
        Cast(id, AID.PiercingPlungeEnrage, 9.1f, 10f, "Enrage"); // TODO: check if this is correct
    }
}