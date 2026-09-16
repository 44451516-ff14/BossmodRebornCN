namespace BossMod.Endwalker.Alliance.A14Naldthal;

public sealed class A14NaldthalStates : StateMachineBuilder
{
    public A14NaldthalStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<HeavensTrialCone>()
            .ActivateOnEnter<HeavensTrialStack>()
            .ActivateOnEnter<GoldenTenet>()
            .ActivateOnEnter<StygianTenet>()
            .ActivateOnEnter<HeatAboveFlamesBelow>()
            .ActivateOnEnter<FarFlungFire>()
            .ActivateOnEnter<DeepestPit>()
            .ActivateOnEnter<OnceAboveEverBelow>()
            .ActivateOnEnter<HellOfFire>()
            .ActivateOnEnter<WaywardSoul>()
            .ActivateOnEnter<FortuneFluxOrder>()
            .ActivateOnEnter<FortuneFluxAOE>()
            .ActivateOnEnter<FortuneFluxKnockback>()
            .ActivateOnEnter<TippedScales>()
            .ActivateOnEnter<Twingaze>()
            .ActivateOnEnter<MagmaticSpell>()
            .ActivateOnEnter<SoulVessel>();
    }

    private void SinglePhase(uint id)
    {
        AsAboveSoBelow(id, 6.7f);
        HeatAboveFlamesBelow(id + 0x10000u, 2.2f);
        AsAboveSoBelow(id + 0x20000u, 4.1f);
        HeatAboveFlamesBelow(id + 0x30000u, 2.2f);
        HeavensTrial(id + 0x40000u, 6.2f);
        GoldenTenet(id + 0x50000u, 2.3f);
        AsAboveSoBelow(id + 0x60000u, 6.0f);
        FarAboveDeepBelow(id + 0x70000u, 2.2f);
        OnceAboveEverBelow(id + 0x80000u, 2.2f);
        HellOfFire(id + 0x90000u, 7f); // note: large variance (5 to 9)
        WaywardSoul(id + 0xA0000u, 7f); // note: large variance (6.5 to 7.5)
        HellOfFire(id + 0xB0000u, 6.2f);
        FiredUp(id + 0xC0000u, 11.9f, false);
        FiredUp(id + 0xD0000u, 2.1f, true);
        SoulMeasure(id + 0xE0000u, 4.4f);

        // note: mechanics below have many variations...
        AsAboveSoBelow(id + 0x100000u, 6.4f);
        OnceAboveEverBelowHeavensTrialOrStygianTenet(id + 0x110000u, 2.2f);
        AsAboveSoBelow(id + 0x120000u, 4.7f);
        HearthAboveFlightBelow(id + 0x130000u, 2.2f);
        HellOfFire(id + 0x140000u, 8.4f); // note: 5.4 if previous was hell's trial
        WaywardSoulHellOfFire(id + 0x150000u, 9.5f); // TODO: sometimes we can get fired up here instead?..
        StygianTenet(id + 0x160000u, 4.2f);
        HellsTrial(id + 0x170000u, 9.7f);
        AsAboveSoBelow(id + 0x180000u, 5.5f);

        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private State AsAboveSoBelow(uint id, float delay)
    {
        return CastMulti(id, [AID.AsAboveSoBelowNald, AID.AsAboveSoBelowThal], delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HellsTrial(uint id, float delay)
    {
        Cast(id, AID.HellsTrial, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavensTrial(uint id, float delay)
    {
        CastStart(id, AID.HeavensTrial, delay);
        CastEnd(id + 1u, 5f);
        ComponentCondition<HeavensTrialStack>(id + 2u, 0.5f, static comp => !comp.Active, "Stack");
        ComponentCondition<HeavensTrialCone>(id + 3u, 0.4f, static comp => comp.NumCasts != 0, "Baited cones")
            .ExecOnExit<HeavensTrialCone>(static comp => comp.NumCasts = 0);
    }

    private State GoldenTenet(uint id, float delay)
    {
        Cast(id, AID.GoldenTenet, delay, 5f);
        return ComponentCondition<GoldenTenet>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Shared tankbuster")
            .ExecOnExit<GoldenTenet>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StygianTenet(uint id, float delay)
    {
        Cast(id, AID.StygianTenet, delay, 5f);
        ComponentCondition<StygianTenet>(id + 0x10u, 0.5f, static comp => comp.NumCasts > 0, "Tankbusters")
            .ExecOnExit<StygianTenet>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeatAboveFlamesBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [AID.HeatAboveFlamesBelowNald, AID.HeatAboveFlamesBelowThal], delay);
        ComponentCondition<HeatAboveFlamesBelow>(id + 1u, 12f, static comp => comp.NumCasts != 0, "In or out")
            .ExecOnExit<HeatAboveFlamesBelow>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void FarAboveDeepBelow(uint id, float delay)
    {
        CastStartMulti(id, [AID.FarAboveDeepBelowThal, AID.FarAboveDeepBelowNald], delay);
        CastEnd(id + 1u, 12);
        Condition(id + 0x10u, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts != 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead
            .ExecOnExit<FarFlungFire>(static comp => comp.NumCasts = 0);
        AsAboveSoBelow(id + 0x100, 5.3f); // note: 5.8s for deepest pit
    }

    private void OnceAboveEverBelowStart(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [AID.OnceAboveEverBelowThalNald, AID.OnceAboveEverBelowThal, AID.OnceAboveEverBelowNaldThal, AID.OnceAboveEverBelowNald], delay);
        ComponentCondition<OnceAboveEverBelow>(id + 2u, 12.6f, static comp => comp.NumCasts != 0, "Exaflares start")
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void OnceAboveEverBelow(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        ComponentCondition<OnceAboveEverBelow>(id + 0x10u, 6f, static comp => comp.NumCasts > 30, "Exaflares end");
    }

    private void OnceAboveEverBelowHeavensTrialOrStygianTenet(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        CastStartMulti(id + 0x10u, [AID.HeavensTrial, AID.StygianTenet], 5.6f);
        ComponentCondition<OnceAboveEverBelow>(id + 0x20u, 0.4f, static comp => comp.NumCasts > 30);
        CastEnd(id + 0x30u, 4.6f)
            .ExecOnExit<OnceAboveEverBelow>(static comp => comp.NumCasts = 0);
        Condition(id + 0x40u, 0.5f, () => Module.FindComponent<HeavensTrialStack>()!.NumFinishedStacks != 0 ||
        Module.FindComponent<HeavensTrialCone>()!.NumCasts != 0 && Module.FindComponent<StygianTenet>()!.NumCasts != 0, "Tankbusters -or- Stack & baited cones")
            .ExecOnExit<StygianTenet>(static comp => comp.NumCasts = 0)
            .ExecOnExit<HeavensTrialStack>(static comp => comp.NumFinishedStacks = 0)
            .ExecOnExit<HeavensTrialCone>(static comp => comp.NumCasts = 0);
    }

    private void HearthAboveFlightBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [AID.HearthAboveFlightBelowThalNald, AID.HearthAboveFlightBelowThal, AID.HearthAboveFlightBelowNald, AID.HearthAboveFlightBelowNaldThal], delay);
        ComponentCondition<HeatAboveFlamesBelow>(id + 1u, 12f, static comp => comp.NumCasts != 0, "In or out")
            .ExecOnExit<HeatAboveFlamesBelow>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.BossCastEnd);
        Condition(id + 0x10u, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts != 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead; sometimes we get 0.1 delay instead
            .ExecOnExit<FarFlungFire>(static comp => comp.NumCasts = 0);
        // orange => golden tenet, blue => hell's trial
        CastMulti(id + 0x100u, [AID.GoldenTenet, AID.HellsTrial], 5.3f, 5f, "Shared tankbuster -or- Raidwide")
            // last puddle ends ~3s into cast
            .ExecOnExit<GoldenTenet>(static comp => comp.NumCasts = 0); // note: actual aoe happens ~0.5s later, but that would complicate the condition...
    }

    private State HellOfFire(uint id, float delay)
    {
        CastMulti(id, [AID.HellOfFireFront, AID.HellOfFireBack], delay, 8f)
            .ExecOnExit<OnceAboveEverBelow>(static comp => comp.NumCasts = 0);
        return Condition(id + 2u, 1f, () => Module.FindComponent<HellOfFire>()!.NumCasts != 0, "Half-arena cleave")
            .ExecOnExit<HellOfFire>(static comp => comp.NumCasts = 0);
    }

    private void WaywardSoulStart(uint id, float delay)
    {
        Cast(id, AID.WaywardSoul, delay, 3f);
        ComponentCondition<WaywardSoul>(id + 0x10u, 0.8f, static comp => comp.Casters.Count != 0);
        ComponentCondition<WaywardSoul>(id + 0x20u, 8, static comp => comp.NumCasts != 0, "Circles start");
        // +5.5s: second set of 3
        // +11.0s: third set of 3
    }

    private void WaywardSoul(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        ComponentCondition<WaywardSoul>(id + 0x100, 32.2f, static comp => comp.Casters.Count == 0, "Circles resolve")
            .ExecOnExit<WaywardSoul>(static comp => comp.NumCasts = 0);
    }

    private void WaywardSoulHellOfFire(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        HellOfFire(id + 0x100u, 14.1f) // sometimes it's 9.2s instead...
            .ExecOnExit<WaywardSoul>(static comp => comp.NumCasts = 0);// last aoe ends ~2.5s into cast
    }

    private void FiredUp(uint id, float delay, bool three)
    {
        CastMulti(id, [AID.FiredUp1Knockback, AID.FiredUp1AOE], delay, 4f);
        CastMulti(id + 0x10u, [AID.FiredUp2Knockback, AID.FiredUp2AOE], 2.1f, 4f);
        if (three)
        {
            CastMulti(id + 0x20u, [AID.FiredUp3Knockback, AID.FiredUp3AOE], 2.1f, 4f);
        }
        Cast(id + 0x100u, AID.FortuneFlux, 2.1f, 8f);
        ComponentCondition<FortuneFluxOrder>(id + 0x110u, 2.5f, static comp => comp.NumComplete != 0, "AOE/Knockback 1");
        var resolve = ComponentCondition<FortuneFluxOrder>(id + 0x120u, 2.0f, static comp => comp.NumComplete > 1, "AOE/Knockback 2");
        if (three)
        {
            resolve = ComponentCondition<FortuneFluxOrder>(id + 0x130u, 1.5f, static comp => comp.NumComplete > 2, "AOE/Knockback 3");
        }
        resolve
            .ExecOnExit<FortuneFluxOrder>(static comp => comp.NumComplete = 0);
    }

    private void SoulMeasure(uint id, float delay)
    {
        Cast(id, AID.SoulsMeasure, delay, 6f);
        Targetable(id + 0x10u, false, 1.1f, "Boss disappears");
        ComponentCondition<SoulVessel>(id + 0x20u, 20.6f, static comp => comp.ActiveActors.Count != 0, "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SoulVessel>(id + 0x30u, 100, static comp => comp.ActiveActors.Count == 0, "Adds enrage")
            .DeactivateOnExit<Twingaze>()
            .DeactivateOnExit<MagmaticSpell>()
            .DeactivateOnExit<SoulVessel>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Cast(id + 0x100u, AID.Balance, 5.2f, 12.5f, "Balance check");
        ComponentCondition<TippedScales>(id + 0x110u, 38.2f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<TippedScales>();
        Targetable(id + 0x120, true, 8.1f, "Boss reappears");
    }
}
