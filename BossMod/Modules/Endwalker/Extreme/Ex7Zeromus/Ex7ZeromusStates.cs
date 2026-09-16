namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

sealed class Ex7ZeromusStates : StateMachineBuilder
{
    public Ex7ZeromusStates(BossModule module) : base(module)
    {
        SimplePhase(0u, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1u || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.RendTheRift;
        DeathPhase(1u, Phase2) // starts at around 25%, after current mechanic is resolved
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1u || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.Enrage;
        DeathPhase(2u, EnrageP2); // starts at around 660s after current mechanic is resolved
    }

    private void Phase1(uint id)
    {
        AbyssalNoxEchoesSableThread(id, 6.2f, false);
        DarkMatter(id + 0x10000u, 7.4f);
        VisceralWhirl(id + 0x20000u, 5.3f);
        Flare(id + 0x30000u, 7.1f);
        VoidBioVisceralWhirl(id + 0x40000u, 5.1f);
        BigBang(id + 0x50000u, 5.1f);
        VoidMeteor(id + 0x60000u, 10.2f);
        VisceralWhirlChainsBombs(id + 0x70000u, 4.4f);
        DarkMatterForkedLightningDarkBeckons(id + 0x80000u, 2.4f);
        BlackHole(id + 0x90000u, 5.3f);
        BigCrunch(id + 0xA0000u, 4.5f);
        AbyssalNoxEchoesSableThread(id + 0xB0000u, 10.3f, true);
        SparklingBrandingFlare(id + 0xC0000u, 7.5f);
        VoidBioVisceralWhirl(id + 0xD0000u, 3.9f);
        DarkMatter(id + 0xE0000u, 7.1f);
        SparklingBrandingFlare(id + 0xF0000u, 5.2f);
        AbyssalNoxEchoesSableThread(id + 0x100000u, 13.9f, true);
        SparklingBrandingFlare(id + 0x110000u, 7.5f);
        VoidBioVisceralWhirl(id + 0x120000u, 3.9f);
        DarkMatter(id + 0x130000u, 7.1f);
        SparklingBrandingFlare(id + 0x140000u, 5.2f);
        AbyssalNoxEchoesEnrage(id + 0x150000u, 13.9f);
    }

    private void Phase2(uint id)
    {
        RendTheRift(id, 0f);
        DimensionalSurgeNostalgia(id + 0x10000u, 8.2f);
        FlowOfTheAbyss(id + 0x20000u, 7.4f, false);
        FlowOfTheAbyss(id + 0x30000u, 10.7f, true);
        DimensionalSurgeNostalgia(id + 0x40000u, 7.7f);
        FlowOfTheAbyss(id + 0x50000u, 7.4f, true);
        FlowOfTheAbyss(id + 0x60000u, 10.7f, true);
        DimensionalSurgeNostalgia(id + 0x70000u, 7.7f);
        FlowOfTheAbyss(id + 0x80000u, 7.4f, true);
        FlowOfTheAbyss(id + 0x90000u, 10.7f, true);
        DimensionalSurgeNostalgia(id + 0xA0000u, 7.7f);
        FlowOfTheAbyss(id + 0xB0000u, 7.4f, true);
        FlowOfTheAbyss(id + 0xC0000u, 10.7f, true);
        DimensionalSurgeNostalgia(id + 0xD0000u, 7.7f);
        FlowOfTheAbyss(id + 0xE0000u, 7.4f, true);
        FlowOfTheAbyss(id + 0xF0000u, 10.7f, true);

        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void EnrageP2(uint id)
    {
        Cast(id, AID.Enrage, 5f, 10f, "Enrage")
            .ActivateOnEnter<BigBangPuddle>() // first puddle/spread starts at the same time, but the rest are slightly staggered
            .ActivateOnEnter<BigBangSpread>();
    }

    private void AbyssalNoxEchoesSableThread(uint id, float delay, bool second)
    {
        Cast(id, AID.AbyssalNox, delay, 5f);
        ComponentCondition<AbyssalEchoes>(id + 0x1000u, 0.1f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<AbyssalEchoes>()
            .ExecOnEnter<AbyssalEchoes>(static comp => comp.MaxCasts = 0); // don't show aoes before doom, this is misleading
        ComponentCondition<AbyssalEchoes>(id + 0x1010u, 5f, static comp => comp.Casters.Count > 5, "1 hp"); // dooms are slightly staggered apply around here
        ComponentCondition<AbyssalEchoes>(id + 0x1020u, 11f, static comp => comp.NumCasts > 0, "Circles 1")
            .ExecOnEnter<AbyssalEchoes>(static comp => comp.MaxCasts = 5);
        ComponentCondition<AbyssalEchoes>(id + 0x1030u, 5f, static comp => comp.Casters.Count == 0, "Circles 2")
            .ActivateOnEnter<SableThread>() // second can very slightly overlap
            .DeactivateOnExit<AbyssalEchoes>();

        Cast(id + 0x2000u, AID.SableThread, second ? 0 : 5.1f, 5);
        ComponentCondition<SableThread>(id + 0x2010u, 0.7f, static comp => comp.NumCasts > 0, "Wild charge start");
        ComponentCondition<SableThread>(id + 0x2020u, second ? 8.9f : 7.5f, comp => comp.NumCasts >= (second ? 7 : 6), "Wild charge resolve")
            .DeactivateOnExit<SableThread>();
    }

    private void AbyssalNoxEchoesEnrage(uint id, float delay)
    {
        Cast(id, AID.AbyssalNox, delay, 5f);
        ComponentCondition<AbyssalEchoes>(id + 0x1000u, 0.1f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<AbyssalEchoes>()
            .ExecOnEnter<AbyssalEchoes>(static comp => comp.MaxCasts = 0); // don't show aoes before doom, this is misleading
        ComponentCondition<AbyssalEchoes>(id + 0x1010u, 5f, static comp => comp.Casters.Count > 5, "1 hp"); // dooms are slightly staggered apply around here
        ComponentCondition<AbyssalEchoes>(id + 0x1020u, 11f, static comp => comp.NumCasts > 0, "Circles 1")
            .ExecOnEnter<AbyssalEchoes>(static comp => comp.MaxCasts = 5);
        ComponentCondition<AbyssalEchoes>(id + 0x1030u, 5f, static comp => comp.Casters.Count == 0, "Circles 2")
            .DeactivateOnExit<AbyssalEchoes>();
        Cast(id + 0x2000u, AID.Enrage, 5f, 10f, "Enrage")
            .ActivateOnEnter<BigBangPuddle>() // first puddle/spread starts at the same time, but the rest are slightly staggered
            .ActivateOnEnter<BigBangSpread>();
    }

    private void DarkMatterCast(uint id, float delay, bool withStackSpread)
    {
        CastStart(id, AID.DarkMatter, delay)
            .ActivateOnEnter<DarkMatter>()
            .ActivateOnEnter<ForkedLightningDarkBeckons>(withStackSpread);
        CastEnd(id + 1u, 4f);
    }

    private void DarkMatterResolve(uint id, float delay)
    {
        ComponentCondition<DarkMatter>(id, delay, static comp => comp.RemainingCasts <= 2, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DarkMatter>(id + 1u, 1.6f, static comp => comp.RemainingCasts <= 1, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DarkMatter>(id + 2u, 1.6f, static comp => comp.RemainingCasts <= 0, "Tankbuster 3")
            .DeactivateOnExit<DarkMatter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void DarkMatter(uint id, float delay)
    {
        DarkMatterCast(id, delay, false);
        DarkMatterResolve(id + 0x10u, 0.8f);
    }

    private void DarkMatterForkedLightningDarkBeckons(uint id, float delay)
    {
        DarkMatterCast(id, delay, true);
        ComponentCondition<ForkedLightningDarkBeckons>(id + 0x10u, 0.6f, static comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<ForkedLightningDarkBeckons>();
        DarkMatterResolve(id + 0x20u, 0.2f);
    }

    private State VisceralWhirl(uint id, float delay)
    {
        CastMulti(id, [AID.VisceralWhirlR, AID.VisceralWhirlL], delay, 8f)
            .ActivateOnEnter<VisceralWhirl>();
        ComponentCondition<VisceralWhirl>(id + 2u, 0.8f, static comp => !comp.Active, "Lines")
            .DeactivateOnExit<VisceralWhirl>();
        ComponentCondition<MiasmicBlast>(id + 0x10u, 0.3f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<MiasmicBlast>();
        return ComponentCondition<MiasmicBlast>(id + 0x11u, 8f, static comp => comp.NumCasts > 0, "Crosses")
            .DeactivateOnExit<MiasmicBlast>();
    }

    private void VoidBioVisceralWhirl(uint id, float delay)
    {
        Cast(id, AID.VoidBio, delay, 5f, "Bubbles spawn")
            .ActivateOnEnter<VoidBio>();
        VisceralWhirl(id + 0x100u, 6.1f)
            .DeactivateOnExit<VoidBio>();
    }

    private void VisceralWhirlChainsBombs(uint id, float delay)
    {
        CastStartMulti(id, [AID.VisceralWhirlR, AID.VisceralWhirlL], delay);
        ComponentCondition<BondsOfDarkness>(id + 1u, 1.9f, static comp => comp.NumTethers > 0, "Chains appear")
            .ActivateOnEnter<VisceralWhirl>()
            .ActivateOnEnter<BondsOfDarkness>(); // tethers have ~5s to be broken
        // +3.1s: acceleration bomb icons
        CastEnd(id + 3, 6, "Stay still") // TODO: check when exactly does the stillness check happen, add hint?
            .DeactivateOnExit<BondsOfDarkness>();
        // +0.5s: acceleration bomb debuffs expire
        ComponentCondition<VisceralWhirl>(id + 5u, 0.8f, static comp => !comp.Active, "Lines")
            .DeactivateOnExit<VisceralWhirl>();

        ComponentCondition<MiasmicBlast>(id + 0x10u, 0.3f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<MiasmicBlast>();
        // +6.3s: spread icons
        ComponentCondition<MiasmicBlast>(id + 0x12u, 8f, static comp => comp.NumCasts > 0, "Crosses")
            .DeactivateOnExit<MiasmicBlast>();
        ComponentCondition<DarkDivides>(id + 0x20u, 3.5f, static comp => !comp.Active, "Spread")
            .ActivateOnEnter<DarkDivides>()
            .DeactivateOnExit<DarkDivides>();
    }

    private void Flare(uint id, float delay)
    {
        Cast(id, AID.Flare, delay, 7f)
            .ActivateOnEnter<FlareTowers>();
        ComponentCondition<FlareTowers>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<FlareScald>()
            .DeactivateOnExit<FlareTowers>();

        ComponentCondition<ProminenceSpine>(id + 0x10u, 2.1f, static comp => comp.Casters.Count > 0, "AOEs at towers") // first scald happens at the same time
            .ActivateOnEnter<ProminenceSpine>();
        ComponentCondition<ProminenceSpine>(id + 0x20u, 5f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<FlareScald>()
            .DeactivateOnExit<ProminenceSpine>();
    }

    private void SparklingBrandingFlare(uint id, float delay)
    {
        CastMulti(id, [AID.SparkingFlare, AID.BrandingFlare], delay, 7f)
            .ActivateOnEnter<FlareTowers>();
        ComponentCondition<FlareTowers>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<FlareScald>()
            .DeactivateOnExit<FlareTowers>();

        CastStart(id + 0x10u, AID.Nox, 1.1f)
            .ActivateOnEnter<SparklingBrandingFlare>() // note: individual casts start 5s into towers cast, but it makes no sense to show hints too early
            .ActivateOnEnter<Nox>();
        ComponentCondition<ProminenceSpine>(id + 0x11u, 1f, static comp => comp.Casters.Count > 0, "AOEs at towers") // first scald happens at the same time
            .ActivateOnEnter<ProminenceSpine>();
        CastEnd(id + 0x12u, 3f);

        ComponentCondition<ProminenceSpine>(id + 0x20u, 2f, static comp => comp.NumCasts > 0, "Stack/spread + Lines")
            .DeactivateOnExit<SparklingBrandingFlare>() // stack/spread ends ~0.1s earlier
            .DeactivateOnExit<FlareScald>()
            .DeactivateOnExit<ProminenceSpine>();

        ComponentCondition<Nox>(id + 0x30u, 4f, static comp => comp.NumCasts > 0, "Chaser start");
        ComponentCondition<Nox>(id + 0x40u, 6.3f, static comp => comp.Chasers.Count == 0, "Chaser resolve")
            .DeactivateOnExit<Nox>();
    }

    private void BigBang(uint id, float delay)
    {
        Cast(id, AID.BigBang, delay, 10f, "Raidwide")
            .ActivateOnEnter<BigBangPuddle>() // first puddle/spread starts at the same time, but the rest are slightly staggered
            .ActivateOnEnter<BigBangSpread>()
            .DeactivateOnExit<BigBangPuddle>()
            .DeactivateOnExit<BigBangSpread>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void BigCrunch(uint id, float delay)
    {
        Cast(id, AID.BigCrunch, delay, 10f, "Raidwide")
            .ActivateOnEnter<BigCrunchPuddle>() // first puddle/spread starts at the same time, but the rest are slightly staggered
            .ActivateOnEnter<BigCrunchSpread>()
            .DeactivateOnExit<BigCrunchPuddle>()
            .DeactivateOnExit<BigCrunchSpread>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void VoidMeteor(uint id, float delay)
    {
        Cast(id, AID.VoidMeteor, delay, 4.8f)
            .ActivateOnEnter<MeteorImpactProximity>();
        ComponentCondition<MeteorImpactProximity>(id + 2, 1.2f, static comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<MeteorImpactCharge>()
            .DeactivateOnExit<MeteorImpactProximity>();
        Cast(id + 0x10u, AID.MeteorImpact, 0.9f, 11f);
        // +0.9s: first set bound
        ComponentCondition<MeteorImpactCharge>(id + 0x20u, 2.7f, static comp => comp.NumCasts >= 4, "Charges 1");
        // +5.4s: second set bound
        ComponentCondition<MeteorImpactCharge>(id + 0x30u, 7.0f, static comp => comp.NumCasts >= 8, "Charges 2")
            .DeactivateOnExit<MeteorImpactCharge>();
        ComponentCondition<MeteorImpactExplosion>(id + 0x40u, 2.1f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<MeteorImpactExplosion>();
        ComponentCondition<MeteorImpactExplosion>(id + 0x41u, 5, static comp => comp.NumCasts > 0, "Explosions")
            .DeactivateOnExit<MeteorImpactExplosion>();
    }

    private void BlackHole(uint id, float delay)
    {
        Cast(id, AID.BlackHole, delay, 5f);
        ComponentCondition<BlackHole>(id + 2u, 0.8f, static comp => comp.Baiter != null)
            .ActivateOnEnter<BlackHole>();
        CastStartMulti(id + 0x10u, [AID.FracturedEventideWE, AID.FracturedEventideEW], 1.4f);
        ComponentCondition<BlackHole>(id + 0x11u, 7.9f, static comp => comp.Voidzone != null, "Black hole bait")
            .ActivateOnEnter<FracturedEventide>();
        CastEnd(id + 0x12u, 2.1f);
        ComponentCondition<FracturedEventide>(id + 0x13u, 0.5f, static comp => comp.NumCasts > 0, "Laser start");
        ComponentCondition<FracturedEventide>(id + 0x20u, 9.2f, static comp => comp.NumCasts > 20, "Laser end")
            .DeactivateOnExit<FracturedEventide>();
        ComponentCondition<BlackHole>(id + 0x30u, 4.1f, static comp => comp.Voidzone == null, "Black hole resolve")
            .DeactivateOnExit<BlackHole>();
    }

    private void RendTheRift(uint id, float delay)
    {
        Cast(id, AID.RendTheRift, delay, 6f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DimensionalSurgeNostalgia(uint id, float delay)
    {
        ComponentCondition<NostalgiaDimensionalSurge>(id, delay, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NostalgiaDimensionalSurge>();
        CastStart(id + 0x10u, AID.Nostalgia, 4f, "First puddles");
        ComponentCondition<NostalgiaDimensionalSurge>(id + 0x20u, 4f, static comp => comp.Casters.Count == 0, "Last puddles")
            .DeactivateOnExit<NostalgiaDimensionalSurge>();
        CastEnd(id + 0x30u, 1f);
        ComponentCondition<Nostalgia>(id + 0x40u, 0.8f, static comp => comp.NumCasts >= 1, "Raidwides start")
            .ActivateOnEnter<Nostalgia>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x41u, 1f, static comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x42u, 1f, static comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x43u, 1f, static comp => comp.NumCasts >= 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x44u, 2f, static comp => comp.NumCasts >= 5)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x45u, 1f, static comp => comp.NumCasts >= 6)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Nostalgia>(id + 0x46u, 3f, static comp => comp.NumCasts >= 7, "Raidwides end")
            .DeactivateOnExit<Nostalgia>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FlowOfTheAbyss(uint id, float delay, bool withPuddles)
    {
        CastStart(id, AID.FlowOfTheAbyss, delay)
            .ActivateOnEnter<NostalgiaDimensionalSurge>(withPuddles);
        ComponentCondition<FlowOfTheAbyssSpreadStack>(id + 1u, 3.1f, static comp => comp.Active)
            .ActivateOnEnter<FlowOfTheAbyssDimensionalSurge>()
            .ActivateOnEnter<FlowOfTheAbyssSpreadStack>()
            .DeactivateOnExit<NostalgiaDimensionalSurge>(withPuddles);
        CastEnd(id + 2u, 3.9f);
        ComponentCondition<FlowOfTheAbyssSpreadStack>(id + 3u, 1.1f, static comp => !comp.Active, "Spread/stack/pairs")
            .ActivateOnEnter<FlowOfTheAbyssAkhRhai>()
            .DeactivateOnExit<FlowOfTheAbyssSpreadStack>();
        ComponentCondition<FlowOfTheAbyssDimensionalSurge>(id + 4u, 0.9f, static comp => comp.NumCasts > 0, "Line")
            .DeactivateOnExit<FlowOfTheAbyssDimensionalSurge>();

        Cast(id + 0x10u, AID.ChasmicNails, 4.2f, 7f)
            .ActivateOnEnter<ChasmicNails>()
            .ActivateOnEnter<FlowOfTheAbyssDimensionalSurge>()
            .ActivateOnEnter<NostalgiaDimensionalSurge>() // first puddles start ~4s into cast
            .DeactivateOnExit<FlowOfTheAbyssAkhRhai>();
        ComponentCondition<ChasmicNails>(id + 0x20u, 0.7f, static comp => comp.NumCasts >= 1, "Pizza start");
        ComponentCondition<NostalgiaDimensionalSurge>(id + 0x21u, 0.3f, static comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<NostalgiaDimensionalSurge>();
        ComponentCondition<ChasmicNails>(id + 0x22u, 0.4f, static comp => comp.NumCasts >= 2);
        ComponentCondition<FlowOfTheAbyssDimensionalSurge>(id + 0x23u, 0.6f, static comp => comp.NumCasts > 0, "Line")
            .DeactivateOnExit<FlowOfTheAbyssDimensionalSurge>();
        ComponentCondition<ChasmicNails>(id + 0x24u, 0.1f, static comp => comp.NumCasts >= 3);
        ComponentCondition<ChasmicNails>(id + 0x25u, 0.7f, static comp => comp.NumCasts >= 4);
        ComponentCondition<ChasmicNails>(id + 0x26u, 0.7f, static comp => comp.NumCasts >= 5, "Pizza resolve")
            .DeactivateOnExit<ChasmicNails>();
    }
}
