namespace BossMod.Dawntrail.Savage.M10STheXtremes;

sealed class M10STheXtremesStates : StateMachineBuilder
{
    private readonly M10STheXtremes _module;
    public M10STheXtremesStates(M10STheXtremes module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<DebuffTracker>();
    }

    private void SinglePhase(uint id)
    {
        RedHot(id, 9.3f);
        DeepBlue(id + 0x10000, 1.9f);
        XtremeSpectacular(id + 0x20000, 8.2f);
        InsaneAir1(id + 0x30000, 3.9f);
        Snaking(id + 0x40000, 10.9f);
        DeepAerial(id + 0x50000, 8.4f);
        SplitArena(id + 0x60000, 10.7f);
        InsaneAir2(id + 0x70000, 10.6f);
        AlleyOopPyro(id + 0x80000, 16.2f);
        Enrage(id + 0x90000, 12.5f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void RedHot(uint id, float delay)
    {
        Cast(id, (uint)AID.HotImpact, delay, 5f, "Shared tankbuster AOE")
            .ActivateOnEnter<HotImpact>()
            .ActivateOnExit<FlameFloater>()
            .ActivateOnExit<FlameFloaterPuddle>()
            .DeactivateOnExit<HotImpact>();

        ComponentCondition<FlameFloater>(id + 0x10, 8.4f, comp => comp.Active, "Flame Floater order"); //22.65
        ComponentCondition<FlameFloater>(id + 0x11, 7.4f, comp => comp.NumCasts > 0, "Flame Floater start"); //30.04
        ComponentCondition<FlameFloater>(id + 0x12, 9.7f, comp => comp.NumCasts >= 4, "Flame Floater end") //39.72
            .ActivateOnExit<AlleyOopInferno>()
            .ActivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<FlameFloater>();

        ComponentCondition<AlleyOopInferno>(id + 0x20, 13.2f, comp => comp.NumFinishedSpreads > 0, "Spread puddles") // 47.91 - 52.895
            .DeactivateOnExit<AlleyOopInferno>();

        CastStart(id + 0x30, (uint)AID.CutbackBlazeCast, 4.5f, "Bait safe cone")
            .ActivateOnExit<CutbackBlaze>()
            .ActivateOnExit<CutbackBlazePuddle>();

        ComponentCondition<CutbackBlaze>(id + 0x31, 5.2f, comp => comp.NumCasts > 0)
            .ActivateOnExit<Pyrorotation>()
            .ActivateOnExit<PyrorotationPuddle>()
            .DeactivateOnExit<CutbackBlaze>();

        ComponentCondition<Pyrorotation>(id + 0x40, 6.5f, comp => comp.CastCounter > 0, "Stack x3 start"); // 63.69 icon - 69.01 1st hit - 72.96 last hit
        ComponentCondition<Pyrorotation>(id + 0x41, 4f, comp => !comp.Active)
            .DeactivateOnExit<Pyrorotation>();

        Cast(id + 0x50, (uint)AID.DiversDareRed, 4.3f, 5f, "Raidwide") // 77.21 - 82.27
            .ActivateOnEnter<DiversDareRed>()
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<FlameFloaterPuddle>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<CutbackBlazePuddle>()
            .DeactivateOnExit<PyrorotationPuddle>();

        Targetable(id + 0x60, false, 6.4f, "Red untargetable"); // 88.655
    }

    private void DeepBlue(uint id, float delay)
    {
        ActorTargetable(id, _module.DeepBlue, true, delay, "Blue targetable") //90.53
            .ActivateOnEnter<FreakyPyrotation>() // use to hide takeoff LP stack until after freaky
            .ActivateOnEnter<AwesomeSplashSlab>();

        ActorTargetable(id + 0x10, _module.DeepBlue, false, 15.4f, "Untargetable") // 105.9
            .ActivateOnEnter<SickSwell1>()
            .ActivateOnEnter<SickestTakeOff>();

        //ActorTargetable(id + 0x20, _module.DeepBlue, true, 11.3f, "Targetable") //117.2 - 117.61 stack/spread
        ComponentCondition<AwesomeSplashSlab>(id + 0x20, 11.7f, comp => !comp.Active, "Stack/spread")
            .ActivateOnExit<AlleyOopWater>()
            .ActivateOnExit<AlleyOopWaterAfter>()
            .DeactivateOnExit<SickSwell1>()
            .DeactivateOnExit<SickestTakeOff>()
            .DeactivateOnExit<AwesomeSplashSlab>()
            .DeactivateOnExit<FreakyPyrotation>();

        ComponentCondition<AlleyOopWater>(id + 0x30, 8.3f, comp => comp.NumCasts > 0, "Cones");
        ComponentCondition<AlleyOopWaterAfter>(id + 0x40, 2.5f, comp => comp.NumCasts > 0, "Followup")
            .DeactivateOnExit<AlleyOopWater>()
            .DeactivateOnExit<AlleyOopWaterAfter>();

        ActorCast(id + 0x50, _module.DeepBlue, (uint)AID.DeepImpactCast, 3f, 5f, true)
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DeepImpactKnockback>();
        //ComponentCondition<DeepImpact>(id + 0x51, 0.5f, comp => !comp.Active, "Baited tankbuster cleave")
        ComponentCondition<DeepImpact>(id + 0x51, 0.8f, comp => comp.ActiveBaits.Count == 0, "Baited tankbuster cleave", checkDelay: 1f)
            .DeactivateOnExit<DeepImpactKnockback>()
            .DeactivateOnExit<DeepImpact>();

        ActorCast(id + 0x60, _module.DeepBlue, (uint)AID.DiversDareBlue, 1.4f, 5f, true, "Raidwide") // 138.67 - 143.65
            .ActivateOnExit<DiversDareBlue>()
            .DeactivateOnExit<DiversDareBlue>();
    }

    private void XtremeSpectacular(uint id, float delay)
    {
        Targetable(id, true, delay, "Red targetable")
            .ActivateOnExit<XtremeSpectacular>()
            .ActivateOnExit<XtremeSpectacularLast>();

        Targetable(id + 0x10, false, 10.1f, "Both untargetable"); // 161.97

        ComponentCondition<XtremeSpectacular>(id + 0x20, 7f, comp => comp.NumCasts > 0, "Proximity AOE"); // 168.94

        ComponentCondition<XtremeSpectacularLast>(id + 0x30, 4.8f, comp => comp.NumCasts > 0, "Raidwides") // 173.72
            .DeactivateOnExit<XtremeSpectacular>()
            .DeactivateOnExit<XtremeSpectacularLast>();
    }

    private void InsaneAir1(uint id, float delay)
    {
        Targetable(id, true, delay, "Both targetable") // 177.6
            .ActivateOnEnter<InsaneAirTest>()
            .ActivateOnEnter<PyrorotationPuddle>()
            .ActivateOnEnter<BlastingSnapPuddle>();

        // red is primary so just use red casts
        CastStart(id + 0x10, (uint)AID.InsaneAirRed, 11.3f, "Insane Air start"); // 188.91
        ComponentCondition<InsaneAirTest>(id + 0x20, 5.9f, comp => comp.ActiveBaits.Count > 0, "1st mechanic");
        ComponentCondition<InsaneAirTest>(id + 0x30, 31f, comp => comp.ActiveBaits.Count == 0, "Insane Air end") // 219.92
            .DeactivateOnExit<InsaneAirTest>();

        Cast(id + 0x40, (uint)AID.DiversDareRed, 4.7f, 5.1f, "Raidwide") // 224.58 - 229.66
            .DeactivateOnExit<PyrorotationPuddle>()
            .DeactivateOnExit<BlastingSnapPuddle>();
    }

    private void Snaking(uint id, float delay)
    {
        // 15s between cast resolution, can't greed mits to block both
        // possible to condition fork so it only shows fire/water mechanics?
        // but want to see other mechs so eg. water doesn't clip fire players with cones
        Cast(id, (uint)AID.Firesnaking, delay, 5f, "Raidwide (snaking)") // 240.39 - 245.44
            .ActivateOnEnter<Firesnaking>()
            .ActivateOnEnter<Watersnaking>()
            .ActivateOnEnter<AlleyOopWater>()
            .ActivateOnEnter<AlleyOopWaterAfter>()
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>();

        // puddles resolve 0.5s before alley oop; use alley oop for state
        ComponentCondition<AlleyOopWater>(id + 0x10, 13f, comp => comp.NumCasts > 0, "Cones + puddles") // 252.96 - 258.02 - 258.47
            .DeactivateOnExit<AlleyOopWater>()
            .DeactivateOnExit<AlleyOopInferno>();

        ComponentCondition<AlleyOopWaterAfter>(id + 0x20, 2.7f, comp => comp.NumCasts > 0, "Followup") // 261.13
            .ActivateOnExit<SickSwell1>()
            .ActivateOnExit<SickestTakeOff>()
            .DeactivateOnExit<AlleyOopWaterAfter>();

        Cast(id + 0x30, (uint)AID.HotImpact2, 3.3f, 5f, "Shared tankbuster") // 264.42 - 269.42
            .ActivateOnEnter<HotImpact2>()
            .ActivateOnExit<AwesomeSplashSlabAerial>()
            .DeactivateOnExit<HotImpact2>();

        ActorCast(id + 0x40, _module.DeepBlue, (uint)AID.DeepVarialCast, 1.1f, 6.8f, true, "Big cone AOE") // 270.52 - 277.3
            .ActivateOnEnter<DeepVarial>()
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<SteamBurst>()
            .DeactivateOnExit<DeepVarial>();

        // inferno resolve 0.3s after stack/spread; use fire for state
        ComponentCondition<AlleyOopInferno>(id + 0x50, 4.9f, comp => comp.NumFinishedSpreads > 0, "Stack/spreads") // inferno start 275.54 - water stack/spread 280.32 - fire spread 280.61 - steam burst 281.61
            .ActivateOnEnter<FreakyPyrotation>() // use to hide takeoff LP stack until after freaky
            .ActivateOnEnter<AwesomeSplashSlab>()
            .DeactivateOnExit<AlleyOopInferno>()
            .DeactivateOnExit<AwesomeSplashSlabAerial>();

        CastStart(id + 0x60, (uint)AID.HotAeriaCast, 3.8f, "Fire baits start") // 284.36
            .ActivateOnEnter<HotAerial>();

        // hot aerial 1 290.72 - water knockback 300.05 - cutback blaze start 303.09 - water stack/spread 304.68 - cutback actual 308.29
        ComponentCondition<SickSwell1>(id + 0x70, 17.7f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SickSwell1>()
            .DeactivateOnExit<SickestTakeOff>();

        ComponentCondition<AwesomeSplashSlab>(id + 0x80, 2.6f, comp => !comp.Active, "Water stack/spread")
            .ActivateOnExit<CutbackBlaze>()
            .DeactivateOnExit<AwesomeSplashSlab>()
            .DeactivateOnExit<FreakyPyrotation>();

        ComponentCondition<CutbackBlaze>(id + 0x90, 3.6f, comp => comp.NumCasts > 0, "Bait safe cone")
            .ActivateOnEnter<CutbackBlazePuddle>()
            .DeactivateOnExit<CutbackBlaze>();

        ActorCast(id + 0xA0, _module.DeepBlue, (uint)AID.DeepImpactCast, 0.7f, 5f, true)
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DeepImpactKnockback>();
        //ComponentCondition<DeepImpact>(id + 0xA1, 0.5f, comp => !comp.Active, "Baited tankbuster cleave")
        ComponentCondition<DeepImpact>(id + 0xA1, 0.8f, comp => comp.ActiveBaits.Count == 0, "Baited tankbuster cleave", checkDelay: 1f)
            .DeactivateOnExit<DeepImpactKnockback>()
            .DeactivateOnExit<DeepImpact>();

        Cast(id + 0xB0, (uint)AID.DiversDareRed, 2.1f, 5f, "Raidwide") // 316.62 - 321.66
            .ActivateOnEnter<DiversDareRed>()
            .ActivateOnEnter<DiversDareBlue>()
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<DiversDareBlue>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<CutbackBlazePuddle>()
            .DeactivateOnExit<SteamBurst>();
    }

    private void DeepAerial(uint id, float delay)
    {
        // Deep Aerial isn't necessarily finished casting when changing state
        // better to check active towers?
        ComponentCondition<DeepAerial>(id, delay, comp => comp.Towers.Count > 0, "")
            .ActivateOnEnter<DeepAerial>();

        ComponentCondition<DeepAerial>(id + 0x10, 6f, comp => comp.NumCasts > 0, "Towers + arena change")
            //.ActivateOnExit<WateryGrave>()
            .DeactivateOnExit<DeepAerial>();

        ActorTargetable(id + 0x20, _module.WateryGrave, true, 3.3f, "Orb targetable") // 339.26
            .ActivateOnExit<WateryGrave>()
            .ActivateOnExit<ScathingSteam>()
            .ActivateOnExit<RedTether>()
            .ActivateOnExit<BlueTether>();

        // casts waves x6, 342.26 - 347.36 - 390.21
        CastStart(id + 0x30, (uint)AID.XtremeWaveRedFirstCast, 3f, "Deep aerial start");

        ComponentCondition<RedTether>(id + 0x40, 48.3f, comp => comp.NumCasts >= 6, "Deep aerial finish");

        Cast(id + 0x50, (uint)AID.DiversDareRed, 5.3f, 5f, "Raidwide") // 395.54 - 400.5
            .ActivateOnEnter<DiversDareRed>()
            .ActivateOnEnter<DiversDareBlue>()
            .ActivateOnExit<SickestTakeOff>()
            .ActivateOnExit<SickSwell1>()
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<DiversDareBlue>()
            .DeactivateOnExit<WateryGrave>()
            .DeactivateOnExit<ScathingSteam>()
            .DeactivateOnExit<RedTether>()
            .DeactivateOnExit<BlueTether>();
    }

    private void SplitArena(uint id, float delay)
    {
        // sick swell 409.14
        Cast(id, (uint)AID.FlameFloaterSplitCast, delay, 5f, "Arena split") // 411.2 - 416.2 - 416.42
            .ActivateOnEnter<FlameFloaterSplit>()
            .ActivateOnEnter<FlameFloaterPuddle>()
            .DeactivateOnExit<FlameFloaterSplit>();

        Cast(id + 0x10, (uint)AID.AlleyOopInfernoCast, 4f, 5.1f, "Flame puddles") // 420.42 - 425.5
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>()
            .ActivateOnExit<SteamBurst>()
            .ActivateOnExit<AlleyOopWater>()
            .ActivateOnExit<AlleyOopWaterAfter>()
            .DeactivateOnExit<AlleyOopInferno>();

        ComponentCondition<AlleyOopWater>(id + 0x20, 8.3f, comp => comp.NumCasts > 0, "Cones") // cast start 425.4 - 431.06
            .ActivateOnEnter<FreakyPyrotation>() // use to hide takeoff LP stack until after freaky
            .ActivateOnEnter<PyrorotationPuddle>()
            .ActivateOnEnter<AwesomeSplashSlab>();

        ComponentCondition<AlleyOopWaterAfter>(id + 0x30, 2.5f, comp => comp.NumCasts > 0, "Followup") // freaky pyrorotation icons 433.39 - followup 433.65
            .DeactivateOnExit<AlleyOopWater>()
            .DeactivateOnExit<AlleyOopWaterAfter>();

        ComponentCondition<FreakyPyrotation>(id + 0x40, 5.3f, comp => comp.NumFinishedStacks > 0, "Fire puddles"); // 438.67

        ComponentCondition<SickSwell1>(id + 0x50, 7.5f, comp => comp.NumCasts > 0, "Knockback") // 439.66 - 446.7
            .DeactivateOnExit<SickestTakeOff>()
            .DeactivateOnExit<SickSwell1>();

        ComponentCondition<AwesomeSplashSlab>(id + 0x60, 2.6f, comp => !comp.Active, "LP stack") // raidwide cast start 448.83 - 449.25
            .ActivateOnEnter<DiversDareRed>()
            .ActivateOnEnter<DiversDareBlue>()
            .DeactivateOnExit<AwesomeSplashSlab>();

        ComponentCondition<DiversDareRed>(id + 0x70, 4.7f, comp => comp.NumCasts > 0, "Raidwide") // 438.83 - 453.96
            .DeactivateOnExit<SteamBurst>()
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<DiversDareBlue>()
            .DeactivateOnEnter<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<PyrorotationPuddle>();
    }

    private void InsaneAir2(uint id, float delay)
    {
        Cast(id, (uint)AID.XtremeFiresnaking, delay, 5f, "Raidwide (snaking)") // 464.6 - 469.74
            .ActivateOnEnter<XtremeFiresnaking>()
            .ActivateOnEnter<XtremeWatersnaking>()
            .ActivateOnExit<InsaneAirTest>();

        ComponentCondition<InsaneAirTest>(id + 0x10, 8f, comp => comp.ActiveBaits.Count > 0, "Insane Air 2 start"); // 477.42
        ComponentCondition<InsaneAirTest>(id + 0x20, 41f, comp => comp.ActiveBaits.Count == 0, "Insane Air end") // 518.46
            .ActivateOnEnter<Bailout>()
            .ActivateOnEnter<BlastingSnapPuddle>()
            .ActivateOnEnter<PyrorotationPuddle>()
            .DeactivateOnExit<InsaneAirTest>();

        Cast(id, (uint)AID.DiversDareRed, 4.7f, 5f, "Raidwide")
            .ActivateOnEnter<DiversDareRed>()
            .ActivateOnEnter<DiversDareBlue>()
            .ActivateOnExit<AlleyOopWater>()
            .DeactivateOnExit<Bailout>()
            .DeactivateOnExit<BlastingSnapPuddle>()
            .DeactivateOnExit<PyrorotationPuddle>()
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<DiversDareBlue>();
    }

    private void AlleyOopPyro(uint id, float delay)
    {
        ComponentCondition<AlleyOopWater>(id, delay, comp => comp.NumCasts > 0, "Cones + puddle bait") // 538.8 - 547.06
            .ActivateOnEnter<AlleyOopWaterAfter>()
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>()
            .ActivateOnEnter<SteamBurst>()
            .ActivateOnExit<DeepImpact>()
            .ActivateOnExit<DeepImpactKnockback>()
            .ActivateOnExit<Pyrorotation>()
            .ActivateOnExit<PyrorotationPuddle>()
            .DeactivateOnExit<AlleyOopWater>();

        ComponentCondition<AlleyOopWaterAfter>(id + 0x10, 2.6f, comp => comp.NumCasts > 0, "Followup + puddles")
            .DeactivateOnExit<AlleyOopWaterAfter>()
            .DeactivateOnExit<AlleyOopInferno>();

        // deep impact and pyrorotation start casting roughly same time
        // probably safer to activate it beforehand instead of relying on cast
        // pyrorotation happens slightly before, resolves later, use to control state
        ComponentCondition<Pyrorotation>(id + 0x20, 3.3f, comp => comp.Active); // 550.25
        ComponentCondition<DeepImpact>(id + 0x21, 5.7f, comp => comp.NumCasts > 0, "Puddles + baited tankbuster cleave");

        ComponentCondition<Pyrorotation>(id + 0x30, 9.2f, comp => !comp.Active, "Finish puddles") // 883.45
            .DeactivateOnExit<DeepImpact>()
            .DeactivateOnExit<DeepImpactKnockback>()
            .DeactivateOnExit<Pyrorotation>();

        Cast(id + 0x40, (uint)AID.DiversDareRed, 4.4f, 5f, "Raidwide 1")
            .ActivateOnEnter<DiversDareRed>()
            .ActivateOnEnter<DiversDareBlue>()
            .DeactivateOnExit<PyrorotationPuddle>();

        Cast(id + 0x50, (uint)AID.DiversDareRed, 4.2f, 5f, "Raidwide 2")
            .DeactivateOnExit<DiversDareRed>()
            .DeactivateOnExit<DiversDareBlue>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, (uint)AID.OverTheFallsRed, delay, 9f, "Enrage");
    }
}
