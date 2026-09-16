namespace BossMod.Endwalker.Savage.P3SPhoinix;

sealed class P3SStates : StateMachineBuilder
{
    public P3SStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ScorchedExaltation(id, 8.1f);
        HeatOfCondemnation(id + 0x010000u, 3.2f);
        FireplumeCinderwing(id + 0x020000u, 5.1f);
        DarkenedFire(id + 0x030000u, 8.2f);
        HeatOfCondemnation(id + 0x040000u, 6.6f);
        ScorchedExaltation(id + 0x050000u, 2.1f);
        DevouringBrandFireplumeBreezeCinderwing(id + 0x060000u, 7.2f);
        HeatOfCondemnation(id + 0x070000u, 3.2f);

        FlyAwayBirds(id + 0x100000u, 2.1f);

        DeadRebirth(id + 0x200000u, 9.2f);
        HeatOfCondemnation(id + 0x210000u, 9.2f);
        FledglingFlight(id + 0x220000u, 7.1f);
        GloryplumeMulti(id + 0x230000u, 8);
        FountainOfFire(id + 0x240000u, 12.1f);

        ScorchedExaltation(id + 0x300000u, 2.1f);
        ScorchedExaltation(id + 0x310000u, 2.1f);
        HeatOfCondemnation(id + 0x320000u, 5.2f);
        FirestormsOfAsphodelos(id + 0x330000u, 8.6f);
        ConesAshplume(id + 0x340000u, 3.2f);
        ConesStorms(id + 0x350000u, 2.1f);
        DarkblazeTwister(id + 0x360000u, 2.2f);
        ScorchedExaltation(id + 0x370000u, 2.1f);
        DeathToll(id + 0x380000u, 7.2f);

        GloryplumeSingle(id + 0x400000u, 7.3f);
        FlyAwayNoBirds(id + 0x410000u, 3f);
        DevouringBrandFireplumeBreezeCinderwing(id + 0x420000u, 5.1f);
        ScorchedExaltation(id + 0x430000u, 6.2f);
        ScorchedExaltation(id + 0x440000u, 2.2f);
        Cast(id + 0x450000u, AID.FinalExaltation, 2.1f, 10f, "Enrage");
    }

    private void ScorchedExaltation(uint id, float delay)
    {
        Cast(id, AID.ScorchedExaltation, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DeadRebirth(uint id, float delay)
    {
        Cast(id, AID.DeadRebirth, delay, 10f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FirestormsOfAsphodelos(uint id, float delay)
    {
        Cast(id, AID.FirestormsOfAsphodelos, delay, 5f, "Raidwide")
            .ActivateOnExit<FlamesOfAsphodelos>()
            .ActivateOnExit<TwisterVoidzone>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeatOfCondemnation(uint id, float delay)
    {
        Cast(id, AID.HeatOfCondemnation, delay, 6f)
            .ActivateOnEnter<HeatOfCondemnation>();
        ComponentCondition<HeatOfCondemnation>(id + 2u, 1.1f, static comp => comp.NumCasts > 0, "Tankbuster tethers")
            .DeactivateOnExit<HeatOfCondemnation>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // note - activates component, which should be later deactivated manually
    // note - positioning state is set at the end, make sure to clear later - this is because this mechanic overlaps with other stuff
    private State Fireplume(uint id, float delay)
    {
        // mechanics:
        // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
        // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
        //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them; each cast lasts 2 sec
        // so center (last/only) plume hits around 6s after cast end
        // note that our helpers rely on 233C casts rather than states
        CastStartMulti(id, [AID.ExperimentalFireplumeSingle, AID.ExperimentalFireplumeMulti], delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        return CastEnd(id + 1u, 5f, "Fireplume")
            .ActivateOnEnter<Fireplume>();
    }

    // note - no positioning flags, since this is part of mechanics that manage it themselves
    // note - since it resolves in a complex way, make sure to add a resolve state!
    private void AshplumeCast(uint id, float delay)
    {
        CastMulti(id, [AID.ExperimentalAshplumeStack, AID.ExperimentalAshplumeSpread], delay, 5f, "Stack/Spread")
            .ActivateOnEnter<Ashplume>();
    }

    private State AshplumeResolve(uint id, float delay)
    {
        return ComponentCondition<Ashplume>(id, delay, static comp => comp.CurState == Ashplume.State.Done, "Stack/Spread resolve")
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void GloryplumeMulti(uint id, float delay)
    {
        // first part for this mechanic always seems to be "multi-plume", works just like fireplume
        // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
        // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
        Cast(id, AID.ExperimentalGloryplumeMulti, delay, 5, "Circles")
            .ActivateOnEnter<Ashplume>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Ashplume>(id + 0x10u, 13.2f, static comp => comp.CurState == Ashplume.State.Done, "Circles resolve")
            .ActivateOnEnter<Fireplume>()
            .DeactivateOnExit<Fireplume>()
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void GloryplumeSingle(uint id, float delay)
    {
        // first part for this mechanic always seems to be "single-plume", works just like fireplume
        // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
        // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
        // note that our helpers rely on casts rather than states
        Cast(id, AID.ExperimentalGloryplumeSingle, delay, 5f, "Circle")
            .ActivateOnEnter<Ashplume>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Ashplume>(id + 0x10u, 7.2f, static comp => comp.CurState == Ashplume.State.Done, "Circle resolves")
            .ActivateOnEnter<Fireplume>()
            .DeactivateOnExit<Fireplume>()
            .DeactivateOnExit<Ashplume>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private State Cinderwing(uint id, float delay)
    {
        return CastMulti(id, [AID.RightCinderwing, AID.LeftCinderwing], delay, 5f, "Side cleave")
            .ActivateOnEnter<Cinderwing>()
            .DeactivateOnExit<Cinderwing>();
    }

    private void FireplumeCinderwing(uint id, float delay)
    {
        Fireplume(id, delay); // pos-start
        Cinderwing(id + 0x1000u, 5.7f)
            .DeactivateOnExit<Fireplume>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void DevouringBrandFireplumeBreezeCinderwing(uint id, float delay)
    {
        Cast(id, AID.DevouringBrand, delay, 3f, "Devouring Brand");
        Fireplume(id + 0x1000u, 2.1f); // pos-start
        CastStart(id + 0x2000u, AID.SearingBreeze, 7.2f)
            .ActivateOnEnter<SearingBreeze>()
            .ActivateOnEnter<DevouringBrand>() // start showing brand aoe after fireplume cast is done
            .DeactivateOnExit<Fireplume>();
        CastEnd(id + 0x2001u, 3f, "Baited AOEs");
        Cinderwing(id + 0x3000u, 3.2f)
            .SetHint(StateMachine.StateHint.PositioningEnd)
            .DeactivateOnExit<SearingBreeze>()
            .OnEnter(Module.DeactivateComponent<DevouringBrand>); // TODO: stop showing brand when aoes finish...
    }

    private void DarkenedFire(uint id, float delay)
    {
        // 3s after cast ends, adds start casting 26299
        CastStart(id, AID.DarkenedFire, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x1000u, 6f, "Darkened Fire phase")
            .ActivateOnEnter<DarkenedFire>()
            .DeactivateOnExit<DarkenedFire>();
        CastStart(id + 0x2000u, AID.BrightenedFire, 5.2f)
            .ActivateOnEnter<BrightenedFire>(); // icons appear just before cast start
        CastEnd(id + 0x2001f, 5f, "Numbers") // at the end boss starts shooting 1-8
            .ActivateOnEnter<DarkenedFireAdd>();
        ComponentCondition<BrightenedFire>(id + 0x3000u, 8.4f, static comp => comp.NumCasts == 8)
            .DeactivateOnExit<BrightenedFire>();
        Timeout(id + 0x4000u, 6.6f, "Darkened Fire resolve") // this timer is max time to kill adds before enrage, timeout is ok here
            .DeactivateOnExit<DarkenedFireAdd>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private State TrailOfCondemnation(uint id, float delay)
    {
        // at this point boss teleports to one of the cardinals
        // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
        CastMulti(id, [AID.TrailOfCondemnationCenter, AID.TrailOfCondemnationSides], delay, 6)
            .ActivateOnEnter<TrailOfCondemnationAOE>()
            .ActivateOnEnter<TrailOfCondemnation>();
        return ComponentCondition<TrailOfCondemnation>(id + 2u, 1.5f, static comp => comp.Done, "Sides/Center AOE")
            .DeactivateOnExit<TrailOfCondemnation>()
            .DeactivateOnExit<TrailOfCondemnationAOE>();
    }

    // note: expects downtime at enter, clears when birds spawn, reset when birds die
    private void SmallBirdsPhase(uint id, float delay)
    {
        ComponentCondition<SunBirdSmall>(id, delay, static comp => comp.ActiveActors.Count != 0, "Small birds", 10000f)
            .ActivateOnEnter<SunBirdSmall>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SunBirdSmall>(id + 0x010u, 25f, static comp => comp.ActiveActors.Count == 0, "Small birds enrage", 10000f)
            .ActivateOnEnter<SmallBirdDistance>()
            .DeactivateOnExit<SmallBirdDistance>()
            .DeactivateOnExit<SunBirdSmall>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart); // raidwide (26326) happens ~3sec after last bird death
    }

    // note: expects downtime at enter, clears when birds spawn, reset when birds die
    private void LargeBirdsPhase(uint id, float delay)
    {
        ComponentCondition<SunBirdLarge>(id, delay, static comp => comp.ActiveActors.Count != 0, "Large birds", 10000f)
            .ActivateOnEnter<SunBirdLarge>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SunBirdLarge>(id + 0x1000u, 18.2f, static comp => comp.FinishedTethers >= 4 || comp.ActiveActors.Count == 0, "", 10000f)
            .ActivateOnEnter<BirdTether>() // note that first tethers appear ~5s after this
            .DeactivateOnExit<BirdTether>();
        ComponentCondition<SunBirdLarge>(id + 0x2000u, 36.8f, static comp => comp.ActiveActors.Count == 0, "Large birds enrage", 10000f) // enrage is ~55sec after spawn
            .ActivateOnEnter<LargeBirdDistance>()
            .DeactivateOnExit<LargeBirdDistance>()
            .DeactivateOnExit<SunBirdLarge>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart); // raidwide (26326) happens ~3sec after last bird death
    }

    private void FlyAwayBirds(uint id, float delay)
    {
        Fireplume(id, delay); // pos-start
        Targetable(id + 0x10000u, false, 4.6f, "Boss disappears");
        TrailOfCondemnation(id + 0x20000u, 3.8f)
            .DeactivateOnExit<Fireplume>();
        SmallBirdsPhase(id + 0x30000u, 7.3f);
        LargeBirdsPhase(id + 0x40000u, 4.3f);
        Targetable(id + 0x50000u, true, 5.2f, "Boss reappears")
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void FlyAwayNoBirds(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears");
        TrailOfCondemnation(id + 0x1000u, 3.8f);
        Targetable(id + 0x2000u, true, 3.1f, "Boss reappears");
    }

    private void FledglingFlight(uint id, float delay)
    {
        // mechanic timeline:
        // 0s cast end
        // 2s icons appear
        // 8s 3540's teleport to players
        // 10s 3540's start casting 26342
        // 14s 3540's finish casting 26342
        // note that helper relies on icons and cast events rather than states
        Cast(id, AID.FledglingFlight, delay, 3f)
            .ActivateOnEnter<FledglingFlight>();
        ComponentCondition<FledglingFlight>(id + 0x10u, 10.3f, static comp => comp.PlacementDone, "Eyes place");
        ComponentCondition<FledglingFlight>(id + 0x20u, 4f, static comp => comp.CastsDone, "Eyes resolve")
            .DeactivateOnExit<FledglingFlight>();
    }

    private void DeathToll(uint id, float delay)
    {
        // notes on mechanics:
        // - on 26349 cast end, debuffs with 25sec appear
        // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
        Cast(id, AID.DeathToll, delay, 6f, "Death Toll");
        Cast(id + 0x1000u, AID.FledglingFlight, 3.2f, 3f, "Eyes")
            .ActivateOnEnter<FledglingFlight>();
        Cast(id + 0x2000u, AID.LifesAgonies, 2.1f, 24f, "Life Agonies")
            .DeactivateOnExit<FledglingFlight>();
    }

    private void FountainOfFire(uint id, float delay)
    {
        // TODO: healer component - not even sure, mechanic looks so simple...
        Cast(id, AID.FountainOfFire, delay, 6f, "Fountain of Fire")
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000u, AID.SunsPinion, 2.1f, 6f, "First birds");
        ComponentCondition<SunshadowTether>(id + 0x2000u, 16.1f, static comp => comp.NumCharges == 6, "Charges")
            .ActivateOnEnter<SunshadowTether>()
            .DeactivateOnExit<SunshadowTether>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ConesAshplume(uint id, float delay)
    {
        Cast(id, AID.FlamesOfAsphodelos, delay, 3f, "Cones")
            .SetHint(StateMachine.StateHint.PositioningStart);
        AshplumeCast(id + 0x1000u, 2.1f);
        AshplumeResolve(id + 0x2000u, 6.1f)
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ConesStorms(uint id, float delay)
    {
        Cast(id, AID.FlamesOfAsphodelos, delay, 3f, "Cones")
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000u, AID.StormsOfAsphodelos, 10.2f, 8f, "Storms")
            .ActivateOnEnter<StormsOfAsphodelos>()
            .DeactivateOnExit<StormsOfAsphodelos>()
            .DeactivateOnExit<FlamesOfAsphodelos>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd);
    }

    private void DarkblazeTwister(uint id, float delay)
    {
        Cast(id, AID.DarkblazeTwister, delay, 4f, "Twister")
            .ActivateOnEnter<DarkTwister>()
            .ActivateOnEnter<BurningTwister>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Cast(id + 0x1000u, AID.SearingBreeze, 4.1f, 3f, "Searing Breeze")
            .ActivateOnEnter<SearingBreeze>();
        AshplumeCast(id + 0x2000u, 4.1f);
        ComponentCondition<DarkTwister>(id + 0x3000u, 2.8f, static comp => comp.Casters.Count == 0, "Knockback")
            .DeactivateOnEnter<SearingBreeze>()
            .DeactivateOnExit<DarkTwister>()
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<BurningTwister>(id + 0x4000u, 2, static comp => comp.NumCasts == 2, "AOE")
            .DeactivateOnExit<BurningTwister>();
        AshplumeResolve(id + 0x5000, 2.3f)
            .DeactivateOnExit<TwisterVoidzone>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
