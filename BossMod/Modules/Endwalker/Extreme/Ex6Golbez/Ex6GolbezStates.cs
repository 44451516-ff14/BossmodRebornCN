namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class Ex6GolbezStates : StateMachineBuilder
{
    public Ex6GolbezStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        TerrastormLingeringSpark(id, 12.6f);
        BindingCold(id + 0x10000u, 4.5f);
        GaleSphere1(id + 0x20000u, 10.4f);
        BindingCold(id + 0x30000u, 4.5f);
        VoidMeteor(id + 0x40000u, 3.2f);
        BlackFang(id + 0x50000u, 9.6f);

        AzdajasShadow(id + 0x60000u, 8.6f);
        PhasesOfTheShadow(id + 0x70000u, 4.4f);

        DoubleMeteor(id + 0x80000u, 2.9f);

        AzdajasShadow(id + 0x90000u, 13.7f);
        VoidStardust(id + 0xA0000u, 4.5f);
        BindingCold(id + 0xB0000u, 8.0f);
        VoidMeteor(id + 0xC0000u, 3.1f);
        PhasesOfTheShadow(id + 0xD0000u, 4.4f);

        TerrastormArcticAssault(id + 0xE0000u, 9.9f);
        BindingCold(id + 0xF0000u, 2.2f);
        GaleSphere2(id + 0x100000u, 8.4f);
        BindingCold(id + 0x110000u, 4.5f);

        AzdajasShadow(id + 0x120000u, 8.2f);
        VoidStardustLingeringSpark(id + 0x130000u, 4.4f);
        PhasesOfTheShadow(id + 0x140000u, 6.1f);

        DoubleMeteor(id + 0x150000u, 2.9f);
        VoidMeteor(id + 0x160000u, 8.6f);
        GaleSphere2(id + 0x170000u, 9.6f);
        BindingCold(id + 0x180000u, 4.5f);

        AzdajasShadow(id + 0x190000u, 8.2f);
        PhasesOfTheShadow(id + 0x1A0000u, 5);
        BindingCold(id + 0x1B0000u, 5);
        BindingCold(id + 0x1C0000u, 8.2f);
        VoidMeteor(id + 0x1D0000u, 3);
        BlackFangEnrage(id + 0x1E0000u, 6.5f);
    }

    private void BindingCold(uint id, float delay)
    {
        Cast(id, AID.BindingCold, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LingeringSparkStart(uint id, float delay)
    {
        CastStart(id, AID.LingeringSpark, delay);
    }

    private void LingeringSparkEnd(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<LingeringSpark>(id + 1u, 1.2f, static comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<LingeringSpark>();
        ComponentCondition<LingeringSpark>(id + 2u, 3f, static comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<LingeringSpark>();
    }

    private void PhasesOfTheBlade(uint id, float delay)
    {
        Cast(id, AID.PhasesOfTheBlade, delay, 5f, "Front cleave")
            .ActivateOnEnter<PhasesOfTheBladeFront>()
            .DeactivateOnExit<PhasesOfTheBladeFront>();
        ComponentCondition<PhasesOfTheBladeBack>(id + 2u, 3.4f, static comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheBladeBack>()
            .DeactivateOnExit<PhasesOfTheBladeBack>();
    }

    private void TerrastormLingeringSpark(uint id, float delay)
    {
        Cast(id, AID.Terrastorm, delay, 3f);
        ComponentCondition<Terrastorm>(id + 0x10u, 1.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Terrastorm>();
        LingeringSparkStart(id + 0x20u, 6.0f);
        ComponentCondition<Terrastorm>(id + 0x30u, 2.0f, static comp => comp.NumCasts > 0, "Diagonals")
            .DeactivateOnExit<Terrastorm>();
        LingeringSparkEnd(id + 0x40u, 1.0f);

        PhasesOfTheBlade(id + 0x1000u, 0.3f);
    }

    private void TerrastormArcticAssault(uint id, float delay)
    {
        Cast(id, AID.Terrastorm, delay, 3f);
        ComponentCondition<Terrastorm>(id + 0x10u, 1.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Terrastorm>();
        CastStart(id + 0x20u, AID.ArcticAssault, 2.0f)
            .ActivateOnEnter<VoidBlizzard>(); // icons appear and spells start at the same time
        CastEnd(id + 0x21u, 3f)
            .ActivateOnEnter<ArcticAssault>();
        ComponentCondition<ArcticAssault>(id + 0x22u, 1.0f, static comp => comp.NumCasts > 0, "Half room")
            .DeactivateOnExit<ArcticAssault>();
        ComponentCondition<Terrastorm>(id + 0x30u, 2.0f, static comp => comp.NumCasts > 0, "Diagonals + Party stacks")
            .DeactivateOnExit<Terrastorm>()
            .DeactivateOnExit<VoidBlizzard>(); // ends at the same time
    }

    private void GaleSphere1(uint id, float delay)
    {
        Cast(id, AID.GaleSphere, delay, 3f);
        CastStart(id + 0x10u, AID.ArcticAssault, 12.7f)
            .ActivateOnEnter<GaleSphere>();
        ComponentCondition<GaleSphere>(id + 0x12u, 0.5f, static comp => comp.NumCasts >= 1, "Spheres 1");
        CastEnd(id + 0x13u, 2.5f)
            .ActivateOnEnter<ArcticAssault>(); // start showing this only after first sphere aoes are done
        ComponentCondition<GaleSphere>(id + 0x14u, 1.0f, static comp => comp.NumCasts >= 2, "Spheres 2 + Half room")
            .DeactivateOnExit<ArcticAssault>(); // happens at the same time
        ComponentCondition<GaleSphere>(id + 0x20u, 3.5f, static comp => comp.NumCasts >= 3, "Spheres 3");
        ComponentCondition<GaleSphere>(id + 0x30u, 3.5f, static comp => comp.NumCasts >= 4, "Spheres 4")
            .DeactivateOnExit<GaleSphere>();

        PhasesOfTheBlade(id + 0x1000u, 0.3f);
    }

    // very similar to 1, but with extra 2/4 man stacks
    private void GaleSphere2(uint id, float delay)
    {
        Cast(id, AID.GaleSphere, delay, 3f);
        CastStart(id + 0x10u, AID.ArcticAssault, 12.7f)
            .ActivateOnEnter<GaleSphere>()
            .ActivateOnEnter<VoidAero>() // these start 7s after previous cast end
            .ActivateOnEnter<VoidTornado>();
        Condition(id + 0x11u, 0.3f, () => !(Module.FindComponent<VoidAero>()?.Active ?? false) && !(Module.FindComponent<VoidTornado>()?.Active ?? false), "Stacks")
            .DeactivateOnExit<VoidAero>()
            .DeactivateOnExit<VoidTornado>();
        ComponentCondition<GaleSphere>(id + 0x12u, 0.2f, static comp => comp.NumCasts >= 1, "Spheres 1");
        CastEnd(id + 0x13u, 2.5f)
            .ActivateOnEnter<ArcticAssault>(); // start showing this only after first sphere aoes are done
        ComponentCondition<GaleSphere>(id + 0x14u, 1.0f, static comp => comp.NumCasts >= 2, "Spheres 2 + Half room")
            .DeactivateOnExit<ArcticAssault>(); // happens at the same time
        ComponentCondition<GaleSphere>(id + 0x20u, 3.5f, static comp => comp.NumCasts >= 3, "Spheres 3")
            .ActivateOnEnter<VoidAero>() // these start 1.2s before 3rd spheres
            .ActivateOnEnter<VoidTornado>();
        ComponentCondition<GaleSphere>(id + 0x30u, 3.5f, static comp => comp.NumCasts >= 4, "Spheres 4")
            .DeactivateOnExit<GaleSphere>();

        CastStart(id + 0x1000u, AID.PhasesOfTheBlade, 0.3f);
        Condition(id + 0x1001u, 0.9f, () => !(Module.FindComponent<VoidAero>()?.Active ?? false) && !(Module.FindComponent<VoidTornado>()?.Active ?? false), "Stacks")
            .ActivateOnEnter<PhasesOfTheBladeFront>()
            .DeactivateOnExit<VoidAero>()
            .DeactivateOnExit<VoidTornado>();
        CastEnd(id + 0x1002u, 4.1f, "Front cleave")
            .DeactivateOnExit<PhasesOfTheBladeFront>();
        ComponentCondition<PhasesOfTheBladeBack>(id + 0x1003u, 3.4f, static comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheBladeBack>()
            .DeactivateOnExit<PhasesOfTheBladeBack>();
    }

    private void VoidMeteor(uint id, float delay)
    {
        CastStart(id, AID.VoidMeteor, delay)
            .ActivateOnEnter<VoidMeteor>();
        CastEnd(id + 1u, 5f, "Small tankbusters start");
        // comets at +0.1, +1.1, +2.1 & +3.1
        ComponentCondition<VoidMeteor>(id + 0x10u, 4.1f, static comp => comp.NumCasts > 0, "Tankbuster hit")
            .DeactivateOnExit<VoidMeteor>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BlackFang(uint id, float delay)
    {
        Cast(id, AID.AzdajasShadowBlackFang, delay, 5f);
        Cast(id + 0x10u, AID.BlackFang, 6.2f, 4f);
        ComponentCondition<BlackFang>(id + 0x20u, 3.8f, static comp => comp.NumCasts > 0, "Raidwide hit 1")
            .ActivateOnEnter<BlackFang>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BlackFang>(id + 0x30u, 2.9f, static comp => comp.NumCasts >= 6, "Raidwide hit 6")
            .DeactivateOnExit<BlackFang>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void BlackFangEnrage(uint id, float delay)
    {
        Cast(id, AID.AzdajasShadowEnrage, delay, 5f);
        Cast(id + 0x10u, AID.BlackFangEnrage, 5.1f, 4);
        ComponentCondition<BlackFang>(id + 0x20u, 3.8f, static comp => comp.NumCasts > 0, "Raidwide x5")
            .ActivateOnEnter<BlackFang>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BlackFang>(id + 0x30u, 3.1f, static comp => comp.NumCasts >= 6, "Enrage");
    }

    // leaves component active
    private void AzdajasShadow(uint id, float delay)
    {
        CastMulti(id, [AID.AzdajasShadowCircleStack, AID.AzdajasShadowDonutSpread], delay, 8f)
            .ActivateOnEnter<AzdajasShadow>();
        ComponentCondition<FlamesOfEventide>(id + 0x10u, 5.2f, static comp => comp.NumCasts >= 1, "Tankbuster 1")
            .ActivateOnEnter<FlamesOfEventide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<FlamesOfEventide>(id + 0x11u, 3.1f, static comp => comp.NumCasts >= 2, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<FlamesOfEventide>(id + 0x12u, 3.1f, static comp => comp.NumCasts >= 3, "Tankbuster 3")
            .DeactivateOnExit<FlamesOfEventide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // resolve for azdaja's shadow
    private void PhasesOfTheShadow(uint id, float delay)
    {
        Cast(id, AID.PhasesOfTheShadow, delay, 5f, "Front cleave")
            .ActivateOnEnter<PhasesOfTheShadowFront>()
            .DeactivateOnExit<PhasesOfTheShadowFront>();
        ComponentCondition<PhasesOfTheShadowBack>(id + 2u, 3.4f, static comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheShadowBack>()
            .ActivateOnEnter<RisingBeacon>() // note: these casts start right after front cleave, but we don't want that interfering with first cleave aoe
            .ActivateOnEnter<RisingRing>()
            .ActivateOnEnter<BurningShade>() // note: these casts start ~1s before second cleave
            .ActivateOnEnter<ImmolatingShade>()
            .DeactivateOnExit<PhasesOfTheShadowBack>();
        Condition(id + 0x10u, 1.3f, () => (Module.FindComponent<RisingBeacon>()?.NumCasts ?? 0) + (Module.FindComponent<RisingRing>()?.NumCasts ?? 0) > 0, "In/out")
            .DeactivateOnExit<RisingBeacon>()
            .DeactivateOnExit<RisingRing>();
        Condition(id + 0x20u, 2.7f, () => !(Module.FindComponent<BurningShade>()?.Active ?? false) && !(Module.FindComponent<ImmolatingShade>()?.Active ?? false), "Spread/stack")
            .DeactivateOnExit<BurningShade>()
            .DeactivateOnExit<ImmolatingShade>()
            .DeactivateOnExit<AzdajasShadow>();
    }

    private void DoubleMeteor(uint id, float delay)
    {
        CastStart(id, AID.DoubleMeteor, delay)
            .ActivateOnEnter<DragonsDescent>()
            .ActivateOnEnter<DoubleMeteor>()
            .ActivateOnEnter<Explosion>();
        ComponentCondition<DragonsDescent>(id + 0x10u, 8.0f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<DragonsDescent>();
        ComponentCondition<Explosion>(id + 0x11u, 2.4f, static comp => comp.Done, "Towers")
            .ActivateOnEnter<Cauterize>() // icon & tether appear 0.5s after knockback
            .DeactivateOnExit<Explosion>();
        CastEnd(id + 0x12u, 0.5f);
        ComponentCondition<DoubleMeteor>(id + 0x13u, 0.2f, static comp => !comp.Active, "Flares")
            .DeactivateOnExit<DoubleMeteor>();
        ComponentCondition<Cauterize>(id + 0x20u, 1.5f, static comp => comp.NumCasts > 0, "Line")
            .DeactivateOnExit<Cauterize>();
    }

    // leaves abyssal quasar active
    private void VoidStardustAbyssalQuasarStart(uint id, float delay)
    {
        Cast(id, AID.VoidStardust, delay, 3f);
        ComponentCondition<VoidStardust>(id + 0x10u, 6.3f, static comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<AbyssalQuasar>(); // casts start ~4.3s after boss cast end
        ComponentCondition<VoidStardust>(id + 0x20u, 2.9f, static comp => comp.NumCasts >= 16, "Exaflares end")
            .DeactivateOnExit<VoidStardust>();
    }

    private State VoidStardustAbyssalQuasarEnd(uint id, float delay)
    {
        return ComponentCondition<AbyssalQuasar>(id, delay, static comp => !comp.Active, "Stack in pairs")
            .DeactivateOnExit<AbyssalQuasar>();
    }

    private void VoidStardust(uint id, float delay)
    {
        VoidStardustAbyssalQuasarStart(id, delay);
        CastStartMulti(id + 0x100u, [AID.EventideTriad, AID.EventideFall], 3.0f);
        VoidStardustAbyssalQuasarEnd(id + 0x110u, 0.1f)
            .ActivateOnEnter<EventideFallTriad>();
        CastEnd(id + 0x120u, 4.9f, "Triad/fall")
            .DeactivateOnExit<EventideFallTriad>();
        // fall: aoe at +1.6 & +4.6
        // triad: aoe at +1.4 & +3.2 & +5.0
    }

    private void VoidStardustLingeringSpark(uint id, float delay)
    {
        VoidStardustAbyssalQuasarStart(id, delay);
        LingeringSparkStart(id + 0x100u, 1.0f);
        VoidStardustAbyssalQuasarEnd(id + 0x110u, 2.1f);
        LingeringSparkEnd(id + 0x120u, 0.9f);
        CastMulti(id + 0x200u, [AID.EventideTriad, AID.EventideFall], 0.1f, 5f, "Triad/fall")
            .ActivateOnEnter<EventideFallTriad>()
            .DeactivateOnExit<EventideFallTriad>();
    }
}
