namespace BossMod.Endwalker.Alliance.A21Nophica;

sealed class A21NophicaStates : StateMachineBuilder
{
    public A21NophicaStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ArenaBounds>();
    }

    private void SinglePhase(uint id)
    {
        Abundance(id, 6.2f);
        MatronsPlenty(id + 0x10000u, 6.2f);
        ReapersGale(id + 0x20000u, 8.2f);
        MatronsPlentyFloralHaze(id + 0x30000u, 2.2f);
        MatronsBreath1(id + 0x40000u, 10.4f);
        Abundance(id + 0x50000u, 0.6f);
        MatronsPlentyFloralHazeReapersGaleLandwaker(id + 0x60000u, 6.2f);
        MatronsBreath2(id + 0x70000u, 10.3f);
        Furrow(id + 0x80000u, 0.1f);
        HeavensEarth(id + 0x90000u, 3.1f);
        Abundance(id + 0xA0000u, 6.3f);
        Abundance(id + 0xB0000u, 4.2f);
        MatronsPlentyFloralHazeReapersGaleLandwaker(id + 0xC0000u, 11.2f); // TODO: verify. even on a MINE run i only saw the start of this
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void Abundance(uint id, float delay)
    {
        Cast(id, AID.Abundance, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ReapersGale(uint id, float delay)
    {
        Cast(id, AID.ReapersGale1, delay, 4f)
            .ActivateOnEnter<ReapersGale>();
        ComponentCondition<ReapersGale>(id + 0x10u, 0.5f, static comp => comp.NumCasts > 0, "Criss-cross 1");
        ComponentCondition<ReapersGale>(id + 0x20u, 2.5f, static comp => comp.Casters.Count == 0, "Criss-cross 2")
            .DeactivateOnExit<ReapersGale>();
    }

    private State GivingLandResolve(uint id, float delay)
    {
        return Condition(id, delay, () => Module.FindComponent<SummerShade>()!.NumCasts + Module.FindComponent<SpringFlowers>()!.NumCasts > 0, "In/out")
            .DeactivateOnExit<SummerShade>()
            .DeactivateOnExit<SpringFlowers>();
    }

    private void GivingLand(uint id, float delay, bool withFloralHaze = false)
    {
        CastStartMulti(id, [AID.GivingLandDonut, AID.GivingLandCircle], delay)
            .ActivateOnEnter<FloralHaze>(withFloralHaze);
        CastEnd(id + 1u, 5f)
            .ActivateOnEnter<SummerShade>()
            .ActivateOnEnter<SpringFlowers>();
        GivingLandResolve(id + 2u, 0.3f)
            .DeactivateOnExit<FloralHaze>(withFloralHaze);
    }

    private State MatronsHarvest(uint id, float delay)
    {
        return CastMulti(id, [AID.MatronsHarvestDonut, AID.MatronsHarvestCircle], delay, 8f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MatronsPlenty(uint id, float delay)
    {
        Cast(id, AID.MatronsPlenty, delay, 2.5f);
        // +3.3s: env control for arena transition (800375AD.39 = 00200010 spring)
        GivingLand(id + 0x10u, 6.2f);
        // +2.9s: seasons passing
        // +3.5s: env control for arena transition (800375AD.39 = 01000001 summer)
        GivingLand(id + 0x20u, 6.0f);
        MatronsHarvest(id + 0x30u, 4.9f);
        // +2.0s: env control for arena transition (800375AD.39 = 00080004 normal)
    }

    private void MatronsPlentyFloralHaze(uint id, float delay)
    {
        Cast(id, AID.MatronsPlenty, delay, 2.5f);
        // +3.3s: env control for arena transition (800375AD.39 = 00020001 summer)
        Cast(id + 0x10u, AID.FloralHaze, 5.2f, 3);
        // +0.6s: direction statuses appear
        GivingLand(id + 0x20u, 3.2f);
        // +1.9s: seasons passing
        // +2.5s: env control for arena transition (800375AD.39 = 00800010 spring)
        GivingLand(id + 0x30u, 5.0f, true);
        MatronsHarvest(id + 0x40u, 4.9f);
        // +2.0s: env control for arena transition (800375AD.39 = 00400004 normal)
    }

    private void MatronsPlentyFloralHazeReapersGaleLandwaker(uint id, float delay)
    {
        Cast(id, AID.MatronsPlenty, delay, 2.5f);
        // +3.3s: env control for arena transition (800375AD.39 = 00020001 summer)
        Cast(id + 0x10u, AID.FloralHaze, 6.2f, 3f);
        // +0.6s: direction statuses appear
        Cast(id + 0x20u, AID.Landwaker, 2.1f, 4f, "Raidwide");
        ComponentCondition<Landwaker>(id + 0x30u, 1.6f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Landwaker>(); // TODO: consider activating it later, after first criss-cross
        CastMulti(id + 0x40u, [AID.ReapersGale2, AID.ReapersGale3], 1.5f, 4f) // TODO: what is the difference between these casts?
            .ActivateOnEnter<ReapersGale>();
        ComponentCondition<ReapersGale>(id + 0x50u, 0.5f, static comp => comp.NumCasts > 0, "Criss-cross 1");
        CastStartMulti(id + 0x60u, [AID.GivingLandDonut, AID.GivingLandCircle], 1.6f);
        ComponentCondition<ReapersGale>(id + 0x70u, 0.9f, static comp => comp.Casters.Count == 0, "Puddles + Criss-cross 2")
            .DeactivateOnExit<Landwaker>() // usually completes slightly before
            .DeactivateOnExit<ReapersGale>();
        CastEnd(id + 0x80u, 4.1f)
            .ActivateOnEnter<SummerShade>()
            .ActivateOnEnter<SpringFlowers>()
            .ActivateOnEnter<FloralHaze>();
        GivingLandResolve(id + 0x81u, 0.3f);

        Cast(id + 0x100u, AID.SowingCircle, 2.9f, 4f)
            .ActivateOnEnter<SowingCircle>();
        ComponentCondition<SowingCircle>(id + 0x110u, 1f, static comp => comp.NumCasts > 0, "Exaflare start");
        // +1.2s: seasons passing
        // +1.7s: env control for arena transition (800375AD.39 = 00800010 spring)
        GivingLand(id + 0x120u, 3.2f);

        MatronsHarvest(id + 0x200u, 3.9f)
            .DeactivateOnExit<FloralHaze>()
            .DeactivateOnExit<SowingCircle>();
        // +2.0s: env control for arena transition (800375AD.39 = 00400004 normal)
    }

    private void MatronsBreath1(uint id, float delay)
    {
        Cast(id, AID.MatronsBreath, delay, 2.5f)
            .ActivateOnEnter<MatronsBreath>();
        ComponentCondition<MatronsBreath>(id + 0x10u, 15.5f, static comp => comp.NumCasts >= 1, "Tower 1");
        ComponentCondition<MatronsBreath>(id + 0x11u, 6, static comp => comp.NumCasts >= 2, "Tower 2");
        ComponentCondition<MatronsBreath>(id + 0x12u, 6, static comp => comp.NumCasts >= 3, "Tower 3")
            .DeactivateOnExit<MatronsBreath>();
    }

    private void MatronsBreath2(uint id, float delay)
    {
        Cast(id, AID.MatronsBreath, delay, 2.5f)
            .ActivateOnEnter<MatronsBreath>();
        ComponentCondition<MatronsBreath>(id + 0x10u, 15.5f, static comp => comp.NumCasts >= 1, "Tower 1");
        ComponentCondition<MatronsBreath>(id + 0x11u, 3.5f, static comp => comp.NumCasts >= 2, "Tower 2");
        ComponentCondition<MatronsBreath>(id + 0x12u, 3.5f, static comp => comp.NumCasts >= 3, "Tower 3");
        ComponentCondition<MatronsBreath>(id + 0x13u, 3.5f, static comp => comp.NumCasts >= 4, "Tower 4");
        ComponentCondition<MatronsBreath>(id + 0x14u, 3.5f, static comp => comp.NumCasts >= 5, "Tower 5");
        ComponentCondition<MatronsBreath>(id + 0x15u, 3.5f, static comp => comp.NumCasts >= 6, "Tower 6")
            .DeactivateOnExit<MatronsBreath>();
    }

    private void Furrow(uint id, float delay)
    {
        Cast(id, AID.Furrow, delay, 6f, "Stack")
            .ActivateOnEnter<Furrow>()
            .DeactivateOnExit<Furrow>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavensEarth(uint id, float delay)
    {
        Cast(id, AID.HeavensEarth, delay, 5f, "Tankbusters")
            .ActivateOnEnter<HeavensEarth>()
            .DeactivateOnExit<HeavensEarth>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}
