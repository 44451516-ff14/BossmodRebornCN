namespace BossMod.Endwalker.Alliance.A14Naldthal;

public class A14NaldthalStates : StateMachineBuilder
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
        return CastMulti(id, [(uint)AID.AsAboveSoBelowNald, (uint)AID.AsAboveSoBelowThal], delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HellsTrial(uint id, float delay)
    {
        Cast(id, (uint)AID.HellsTrial, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavensTrial(uint id, float delay)
    {
        CastStart(id, (uint)AID.HeavensTrial, delay);
        CastEnd(id + 1u, 5f);
        ComponentCondition<HeavensTrialStack>(id + 2u, 0.5f, comp => !comp.Active, "Stack");
        ComponentCondition<HeavensTrialCone>(id + 3u, 0.4f, comp => comp.NumCasts != 0, "Baited cones")
            .ResetComp<HeavensTrialCone>();
    }

    private State GoldenTenet(uint id, float delay)
    {
        Cast(id, (uint)AID.GoldenTenet, delay, 5f);
        return ComponentCondition<GoldenTenet>(id + 2u, 0.5f, comp => comp.NumCasts > 0, "Shared tankbuster")
            .ResetComp<GoldenTenet>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void StygianTenet(uint id, float delay)
    {
        Cast(id, (uint)AID.StygianTenet, delay, 5f);
        ComponentCondition<StygianTenet>(id + 0x10u, 0.5f, comp => comp.NumCasts > 0, "Tankbusters")
            .ResetComp<StygianTenet>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeatAboveFlamesBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [(uint)AID.HeatAboveFlamesBelowNald, (uint)AID.HeatAboveFlamesBelowThal], delay);
        ComponentCondition<HeatAboveFlamesBelow>(id + 1u, 12f, comp => comp.NumCasts != 0, "In or out")
            .ResetComp<HeatAboveFlamesBelow>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void FarAboveDeepBelow(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.FarAboveDeepBelowThal, (uint)AID.FarAboveDeepBelowNald], delay);
        CastEnd(id + 1u, 12);
        Condition(id + 0x10u, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts != 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead
            .ResetComp<FarFlungFire>();
        AsAboveSoBelow(id + 0x100, 5.3f) // note: 5.8s for deepest pit
            .ResetComp<DeepestPit>();
    }

    private void OnceAboveEverBelowStart(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [(uint)AID.OnceAboveEverBelowThalNald, (uint)AID.OnceAboveEverBelowThal, (uint)AID.OnceAboveEverBelowNaldThal, (uint)AID.OnceAboveEverBelowNald], delay);
        ComponentCondition<OnceAboveEverBelow>(id + 2u, 12.6f, comp => comp.NumCasts != 0, "Exaflares start")
            .SetHint(StateMachine.StateHint.BossCastEnd);
    }

    private void OnceAboveEverBelow(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        ComponentCondition<OnceAboveEverBelow>(id + 0x10u, 6f, comp => comp.NumCasts > 30, "Exaflares end");
    }

    private void OnceAboveEverBelowHeavensTrialOrStygianTenet(uint id, float delay)
    {
        OnceAboveEverBelowStart(id, delay);
        CastStartMulti(id + 0x10u, [(uint)AID.HeavensTrial, (uint)AID.StygianTenet], 5.6f);
        ComponentCondition<OnceAboveEverBelow>(id + 0x20u, 0.4f, comp => comp.NumCasts > 30);
        CastEnd(id + 0x30u, 4.6f)
            .ResetComp<OnceAboveEverBelow>();
        Condition(id + 0x40u, 0.5f, () => Module.FindComponent<HeavensTrialStack>()!.NumFinishedStacks != 0 ||
        Module.FindComponent<HeavensTrialCone>()!.NumCasts != 0 && Module.FindComponent<StygianTenet>()!.NumCasts != 0, "Tankbusters -or- Stack & baited cones")
            .ResetComp<StygianTenet>()
            .ResetComp<HeavensTrialStack>()
            .ResetComp<HeavensTrialCone>();
    }

    private void HearthAboveFlightBelow(uint id, float delay)
    {
        // unfortunately, one of the boss casts ends 1s earlier - just use actual casts instead
        CastStartMulti(id, [(uint)AID.HearthAboveFlightBelowThalNald, (uint)AID.HearthAboveFlightBelowThal, (uint)AID.HearthAboveFlightBelowNald, (uint)AID.HearthAboveFlightBelowNaldThal], delay);
        ComponentCondition<HeatAboveFlamesBelow>(id + 1u, 12f, comp => comp.NumCasts != 0, "In or out")
            .ResetComp<HeatAboveFlamesBelow>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        Condition(id + 0x10u, 0.9f, () => Module.FindComponent<FarFlungFire>()!.NumCasts != 0 || Module.FindComponent<DeepestPit>()!.Active, "Line stack or baited puddles start") // note: deepest pit start is 1.4s instead; sometimes we get 0.1 delay instead
            .ResetComp<FarFlungFire>();
        // orange => golden tenet, blue => hell's trial
        CastMulti(id + 0x100u, [(uint)AID.GoldenTenet, (uint)AID.HellsTrial], 5.3f, 5, "Shared tankbuster -or- Raidwide")
            .ResetComp<DeepestPit>() // last puddle ends ~3s into cast
            .ResetComp<GoldenTenet>(); // note: actual aoe happens ~0.5s later, but that would complicate the condition...
    }

    private State HellOfFire(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.HellOfFireFront, (uint)AID.HellOfFireBack], delay, 8f)
            .ResetComp<OnceAboveEverBelow>();
        return Condition(id + 2u, 1f, () => Module.FindComponent<HellOfFire>()!.NumCasts != 0, "Half-arena cleave")
            .ResetComp<HellOfFire>();
    }

    private void WaywardSoulStart(uint id, float delay)
    {
        Cast(id, (uint)AID.WaywardSoul, delay, 3f);
        ComponentCondition<WaywardSoul>(id + 0x10u, 0.8f, comp => comp.Casters.Count != 0);
        ComponentCondition<WaywardSoul>(id + 0x20u, 8, comp => comp.NumCasts != 0, "Circles start");
        // +5.5s: second set of 3
        // +11.0s: third set of 3
    }

    private void WaywardSoul(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        ComponentCondition<WaywardSoul>(id + 0x100, 32.2f, comp => comp.Casters.Count == 0, "Circles resolve")
            .ResetComp<WaywardSoul>();
    }

    private void WaywardSoulHellOfFire(uint id, float delay)
    {
        WaywardSoulStart(id, delay);
        HellOfFire(id + 0x100u, 14.1f) // sometimes it's 9.2s instead...
            .ResetComp<WaywardSoul>(); // last aoe ends ~2.5s into cast
    }

    private void FiredUp(uint id, float delay, bool three)
    {
        CastMulti(id, [(uint)AID.FiredUp1Knockback, (uint)AID.FiredUp1AOE], delay, 4f);
        CastMulti(id + 0x10u, [(uint)AID.FiredUp2Knockback, (uint)AID.FiredUp2AOE], 2.1f, 4f);
        if (three)
            CastMulti(id + 0x20u, [(uint)AID.FiredUp3Knockback, (uint)AID.FiredUp3AOE], 2.1f, 4f);
        Cast(id + 0x100u, (uint)AID.FortuneFlux, 2.1f, 8);
        ComponentCondition<FortuneFluxOrder>(id + 0x110u, 2.5f, comp => comp.NumComplete != 0, "AOE/Knockback 1");
        var resolve = ComponentCondition<FortuneFluxOrder>(id + 0x120u, 2.0f, comp => comp.NumComplete > 1, "AOE/Knockback 2");
        if (three)
            resolve = ComponentCondition<FortuneFluxOrder>(id + 0x130u, 1.5f, comp => comp.NumComplete > 2, "AOE/Knockback 3");
        resolve
            .ResetComp<FortuneFluxOrder>()
            .ResetComp<FortuneFluxAOE>()
            .ResetComp<FortuneFluxKnockback>();
    }

    private void SoulMeasure(uint id, float delay)
    {
        Cast(id, (uint)AID.SoulsMeasure, delay, 6f);
        Targetable(id + 0x10u, false, 1.1f, "Boss disappears");
        ComponentCondition<SoulVessel>(id + 0x20u, 20.6f, comp => comp.ActiveActors.Count != 0, "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SoulVessel>(id + 0x30u, 100, comp => comp.ActiveActors.Count == 0, "Adds enrage")
            .ResetComp<Twingaze>()
            .ResetComp<MagmaticSpell>()
            .ResetComp<SoulVessel>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Cast(id + 0x100u, (uint)AID.Balance, 5.2f, 12.5f, "Balance check");
        ComponentCondition<TippedScales>(id + 0x110u, 38.2f, comp => comp.NumCasts > 0, "Raidwide")
            .ResetComp<TippedScales>();
        Targetable(id + 0x120, true, 8.1f, "Boss reappears");
    }
}
