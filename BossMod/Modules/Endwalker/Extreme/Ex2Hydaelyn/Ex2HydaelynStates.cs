namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class Ex2HydaelynStates : StateMachineBuilder
{
    public Ex2HydaelynStates(BossModule module) : base(module)
    {
        SimplePhase(0u, Phase1, "P1")
            .ActivateOnEnter<WeaponTracker>()
            .Raw.Update = () => Module.Enemies((uint)OID.CrystalOfLight).Count != 0;
        DeathPhase(1u, Phase2)
            .ActivateOnEnter<WeaponTracker>();
    }

    private void Phase1(uint id)
    {
        HerosRadiance(id, 10.2f);
        ShiningSaber(id + 0x10000u, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000u, 5.5f, false);
        ForkByWeapon(id + 0x30000u, 8u, ForkFirstStaff, ForkFirstChakram);
    }

    private void ForkByWeapon(uint id, uint secondOffset, Action<uint> forkStaff, Action<uint> forkChakram)
    {
        Dictionary<WeaponTracker.Stance, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [WeaponTracker.Stance.Staff] = ((id >> 24) + 1u, forkStaff),
            [WeaponTracker.Stance.Chakram] = ((id >> 24) + secondOffset, forkChakram)
        };
        ComponentConditionFork<WeaponTracker, WeaponTracker.Stance>(id, 0f, _ => true, static comp => comp.CurStance, dispatch);
    }

    private void ForkFirstStaff(uint id)
    {
        MagosRadiance(id, 2.7f);
        Aureole(id + 0x10000u, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000u, 4.6f, false);
        MousaScorn(id + 0x30000u, 3.2f);
        Aureole(id + 0x40000u, 5.2f);
        CrystallizeSwitchWeapon(id + 0x50000u, 4.6f, true);
    }

    private void ForkFirstChakram(uint id)
    {
        MousaScorn(id, 3.2f);
        Aureole(id + 0x10000u, 5.2f);
        CrystallizeSwitchWeapon(id + 0x20000u, 4.6f, false);
        MagosRadiance(id + 0x30000u, 2.7f);
        Aureole(id + 0x40000u, 5.2f);
        CrystallizeSwitchWeapon(id + 0x50000u, 4.6f, true);
    }

    private void Phase2(uint id)
    {
        Intermission(id + 0x100000u, 2.2f);
        Halo(id + 0x200000u, 10.2f);
        Lightwave1(id + 0x210000u, 4.1f);
        Lightwave2(id + 0x220000u, 2.1f);
        Halo(id + 0x230000u, 3.8f);
        HerosSundering(id + 0x240000u, 6.1f);
        ShiningSaber(id + 0x250000u, 5.3f);
        SwitchWeapon(id + 0x260000u, 3.4f, false);
        ForkByWeapon(id + 0x270000u, 4u, ForkSecondStaff, ForkSecondChakram);
    }

    private void ForkSecondStaff(uint id)
    {
        MagosRadiance(id, 5.2f);
        CrystallizeParhelicCircleAureole(id + 0x10000u, 5.2f);
        SwitchWeapon(id + 0x20000u, 2.5f, false);
        MousaScorn(id + 0x30000u, 5.2f);
        ParhelionCrystallizeAureole(id + 0x40000u, 6.8f);
        SwitchWeapon(id + 0x50000u, 2.5f, true);
        ForkSecondMerge(id, 8f);
    }

    private void ForkSecondChakram(uint id)
    {
        MousaScorn(id, 5.2f);
        ParhelionCrystallizeAureole(id + 0x10000u, 6.8f);
        SwitchWeapon(id + 0x20000u, 2.5f, false);
        MagosRadiance(id + 0x30000u, 5.2f);
        CrystallizeParhelicCircleAureole(id + 0x40000u, 5.2f);
        SwitchWeapon(id + 0x50000u, 2.5f, true);
        ForkSecondMerge(id, 8f);
    }

    private void ForkSecondMerge(uint id, float delay)
    {
        RadiantHalo(id + 0x100000u, delay);
        Lightwave3(id + 0x110000u, 5.2f);
        CrystallizeShiningSaber(id + 0x120000u, 9.1f); // TODO: can there be aureole instead of saber here?..
        SwitchWeapon(id + 0x130000u, 1.3f, false, true); // note: we don't create a fork here, since it's kind of irrelevant...
        Lightwave3(id + 0x140000u, 7.3f);
        CrystallizeAureole(id + 0x150000u, 9.1f, true);
        SwitchWeapon(id + 0x160000u, 1.3f, false, true);
        CrystallizeAureole(id + 0x170000u, 7.3f, false);
        SwitchWeapon(id + 0x180000u, 1.3f, true, true);
        Cast(id + 0x190000u, AID.HerosRadianceEnrage, 9.5f, 10f, "Enrage");
    }

    private void Intermission(uint id, float delay)
    {
        var echoes = Module.Enemies((uint)OID.Echo);

        Targetable(id, false, delay, "Intermission start");
        ComponentCondition<PureCrystal>(id + 0x10000u, 12.5f, static comp => comp.NumCasts > 0, "Raidwide + adds appear")
            .ActivateOnEnter<PureCrystal>()
            .DeactivateOnExit<PureCrystal>()
            .SetHint(StateMachine.StateHint.DowntimeEnd); // crystals become targetable ~0.1s before, echoes ~0.1s after
        Condition(id + 0x20000u, 60f, () =>
        {
            var echoes = Module.Enemies((uint)OID.Echo);
            var count = echoes.Count;
            for (var i = 0; i < count; ++i)
            {
                var echo = echoes[i];
                if (echo.IsTargetable && !echo.IsDead)
                {
                    return false;
                }
            }
            return true;
        }, "Adds down", 10000f, 1f) // note that time is arbitrary
            .ActivateOnEnter<IntermissionAdds>()
            .DeactivateOnExit<IntermissionAdds>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        // +2.1s: boss casts 26043 'Exodus'
        ComponentCondition<Exodus>(id + 0x30000u, 16.9f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Exodus>()
            .DeactivateOnExit<Exodus>();
        Targetable(id + 0x40000u, true, 5.2f, "Intermission end");
    }

    private void Lightwave1(uint id, float delay)
    {
        Cast(id, AID.LightwaveSword, delay, 4f, "Lightwave1");
        ComponentCondition<Lightwave1>(id + 0x1000u, 12.1f, static comp => comp.NumCasts > 0, "Crystal1")
            .ActivateOnEnter<Lightwave1>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Lightwave1>(id + 0x2000u, 2.1f, static comp => comp.NumCasts > 1, "Crystal2");
        CastStart(id + 0x3000u, AID.InfralateralArc, 1.3f)
            .ActivateOnEnter<InfralateralArc>();
        CastEnd(id + 0x3001u, 4.9f, "InfralateralArc");
        ComponentCondition<Lightwave1>(id + 0x4000u, 1.3f, static comp => comp.NumCasts > 2, "Crystal3")
            .DeactivateOnExit<Lightwave1>();
        ComponentCondition<InfralateralArc>(id + 0x6000u, 2.2f, static comp => comp.NumCasts > 2, "Resolve", 1.5f) // very large variance here...
            .DeactivateOnExit<InfralateralArc>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Lightwave2(uint id, float delay)
    {
        Cast(id, AID.LightwaveSword, delay, 4f, "Lightwave2");
        Cast(id + 0x1000u, AID.HerosGlory, 4.7f, 5u, "Glory1")
            .ActivateOnEnter<Lightwave2>() // note that we don't show any hints until first glory starts casting, since it's a bit misleading...
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Lightwave2>(id + 0x2000u, 4.6f, static comp => comp.NumCasts > 0, "Crystal1");
        ComponentCondition<Lightwave2>(id + 0x3000u, 3.0f, static comp => comp.NumCasts > 1);
        ComponentCondition<Lightwave2>(id + 0x4000u, 2.9f, static comp => comp.NumCasts > 2);
        ComponentCondition<Lightwave2>(id + 0x5000u, 3.0f, static comp => comp.NumCasts > 3);
        Cast(id + 0x6000u, AID.HerosGlory, 0.5f, 5f, "Glory2");
        ComponentCondition<Lightwave2>(id + 0x7000u, 1.3f, static comp => comp.NumCasts > 4, "Resolve")
            .DeactivateOnExit<Lightwave2>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // note: keeps Lightwave3 component active, since it is relevant for next mechanic
    private void Lightwave3(uint id, float delay)
    {
        CastMulti(id, [AID.LightwaveSword, AID.LightwaveStaff, AID.LightwaveChakram], delay, 4f, "Lightwave");
        CastStartMulti(id + 0x1000u, [AID.EchoesSword, AID.EchoesStaff, AID.EchoesChakram], 15.2f)
            .ActivateOnEnter<Lightwave3>()
            .ActivateOnEnter<Echoes>(); // note that icon appears slightly before cast start...
        CastEnd(id + 0x1001u, 5f, "Stack");
        // + ~1.0s: new lightwaves
        ComponentCondition<Echoes>(id + 0x2000u, 4.5f, static comp => comp.NumCasts > 4, "Echoes resolve")
            .DeactivateOnExit<Echoes>()
            .DeactivateOnExit<Lightwave3>();
        ComponentCondition<Spectrum>(id + 0x3000u, 3.5f, static comp => comp.NumCasts > 0, "Stack/spread")
            .ActivateOnEnter<Spectrum>()
            .ActivateOnEnter<Lightwave3>()
            .DeactivateOnExit<Spectrum>();
    }

    private void HerosRadiance(uint id, float delay)
    {
        Cast(id, AID.HerosRadiance, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MagosRadiance(uint id, float delay)
    {
        Cast(id, AID.MagosRadiance, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MousaScorn(uint id, float delay)
    {
        Cast(id, AID.MousaScorn, delay, 5f, "Shared Tankbuster")
            .ActivateOnEnter<MousaScorn>()
            .DeactivateOnExit<MousaScorn>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Halo(uint id, float delay)
    {
        Cast(id, AID.Halo, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RadiantHalo(uint id, float delay)
    {
        Cast(id, AID.RadiantHalo, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HerosSundering(uint id, float delay)
    {
        Cast(id, AID.HerosSundering, delay, 5f, "AOE Tankbuster")
            .ActivateOnEnter<HerosSundering>()
            .DeactivateOnExit<HerosSundering>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ShiningSaber(uint id, float delay)
    {
        // note: resolve happens slightly after cast, but variance is too large (0.2-0.5s), so just ignore it...
        Cast(id, AID.ShiningSaber, delay, 4.9f, "Stack")
            .ActivateOnEnter<ShiningSaber>()
            .DeactivateOnExit<ShiningSaber>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State Aureole(uint id, float delay)
    {
        // note: what is the difference between aureole spells? seems to be determined by weapon?..
        CastMulti(id, [AID.Aureole1, AID.Aureole2, AID.LateralAureole1, AID.LateralAureole2], delay, 5f)
            .ActivateOnEnter<Aureole>();
        return ComponentCondition<Aureole>(id + 2u, 0.5f, static comp => comp.NumCasts != 0, "Aureole")
            .DeactivateOnExit<Aureole>();
    }

    private void ParhelicCircle(uint id, float delay)
    {
        Cast(id, AID.ParhelicCircle, delay, 6f, "Orbs")
            .ActivateOnEnter<ParhelicCircle>();
        ComponentCondition<ParhelicCircle>(id + 0x10u, 1.9f, static comp => comp.NumCasts > 0, "Orbs resolve")
            .DeactivateOnExit<ParhelicCircle>();
    }

    private void SwitchWeapon(uint id, float delay, bool toSword, bool shorter = default)
    {
        ComponentCondition<WeaponTracker>(id, delay, static comp => comp.AOEImminent, "Select weapon");
        ComponentCondition<WeaponTracker>(id + 0x10u, toSword && !shorter ? 6.9f : !toSword && !shorter ? 6 : toSword && shorter ? 5.8f : 5, static comp => !comp.AOEImminent, "Weapon AOE");
    }

    // note: activates Crystallize component and sets positioning flag
    private State CrystallizeCast(uint id, float delay, string name = "Crystallize")
    {
        // note: there are several crystallize spells, concrete is determined by element and current weapon; weapon to switch to doesn't seem to matter
        return CastMulti(id, [AID.CrystallizeSwordStaffWater, AID.CrystallizeStaffEarth, AID.CrystallizeStaffIce, AID.CrystallizeChakramIce, AID.CrystallizeChakramEarth, AID.CrystallizeChakramWater], delay, 4, name)
            .ActivateOnEnter<Crystallize>()
            .SetHint(StateMachine.StateHint.PositioningStart);
    }

    // note: deactivates Crystallize component and clears positioning flag
    private void CrystallizeResolve(uint id, float delay, string name = "Element resolve")
    {
        ComponentCondition<Crystallize>(id, delay, static comp => comp.CurElement == Crystallize.Element.None, name)
            .DeactivateOnExit<Crystallize>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void CrystallizeSwitchWeapon(uint id, float delay, bool toSword)
    {
        CrystallizeCast(id, delay);
        SwitchWeapon(id + 0x200u, 3.1f, toSword);
        CrystallizeResolve(id + 0x300u, toSword ? 3.2f : 4f);
    }

    private void CrystallizeParhelicCircleAureole(uint id, float delay)
    {
        CrystallizeCast(id, delay, "Crystallize (ice)");
        ParhelicCircle(id + 0x1000u, 4.8f);
        CrystallizeResolve(id + 0x3000u, 3.5f, "Ice resolve");
        Aureole(id + 0x4000u, 1f);
    }

    private void ParhelionCrystallizeAureole(uint id, float delay)
    {
        Cast(id, AID.Parhelion, delay, 5f, "Parhelion")
            .ActivateOnEnter<Parhelion>();
        CrystallizeCast(id + 0x1000u, 4.8f, "Crystallize (water)");
        Cast(id + 0x2000u, AID.Subparhelion, 3.2f, 5f, "Subparhelion");
        CrystallizeResolve(id + 0x2800, 2f, "Water resolve");
        Aureole(id + 0x3000u, 3.3f) // note that aureole cast starts slightly before last subparhelion resolves
            .DeactivateOnExit<Parhelion>(); // note that last beacon happens slightly after cast start
    }

    // note: expects Lightwave3 component
    private void CrystallizeShiningSaber(uint id, float delay)
    {
        CrystallizeCast(id, delay)
            .DeactivateOnExit<Lightwave3>();
        ShiningSaber(id + 0x1000u, 3.2f);
        CrystallizeResolve(id + 0x3000u, 4.3f);
    }

    private void CrystallizeAureole(uint id, float delay, bool afterLightwave)
    {
        CrystallizeCast(id, delay)
            .DeactivateOnExit<Lightwave3>(afterLightwave);
        Aureole(id + 0x1000u, 3.1f);
        CrystallizeResolve(id + 0x3000u, 3.7f);
    }
}
