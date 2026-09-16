namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

abstract class C033StaticeStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C033StaticeStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Aero(id, 8.2f);
        TrickReload1(id + 0x10000u, 5.7f);
        TrickReload2(id + 0x20000u, 8.6f);
        Aero(id + 0x30000u, 3.1f);
        Intermission(id + 0x40000u, 7.2f);
        ShockingAbandon(id + 0x50000u, 1.2f);
        PinwheelingDartboard(id + 0x60000u, 7.2f);
        Aero(id + 0x70000u, 3.1f);
        TrickReload3(id + 0x80000, 8.8f);
        Aero(id + 0x90000u, 2.0f);
        Aero(id + 0xA0000u, 3.2f);
        Cast(id + 0xB0000u, AID.Enrage, 3.2f, 10f, "Enrage");
    }

    private void Aero(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAero : AID.NAero, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ShockingAbandon(uint id, float delay)
    {
        Cast(id, _savage ? AID.SShockingAbandon : AID.NShockingAbandon, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TrickReload1(uint id, float delay)
    {
        Cast(id, _savage ? AID.STrickReload : AID.NTrickReload, delay, 4f)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x10u, _savage ? AID.STrapshooting1 : AID.NTrapshooting1, 10.7f, 4f)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<Trapshooting>(id + 0x12u, 4.1f, static comp => comp.NumResolves > 0, "Stack/spread");
        Cast(id + 0x20u, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 2.1f, 4.3f)
            .ActivateOnEnter<NTriggerHappy>(!_savage)
            .ActivateOnEnter<STriggerHappy>(_savage);
        ComponentCondition<TriggerHappy>(id + 0x22u, 0.7f, static comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();
        Cast(id + 0x30u, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 4.2f, 3)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x40u, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 10.2f, 4f);
        ComponentCondition<RingARingOExplosions>(id + 0x50u, 4.0f, static comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x51u, 0.1f, static comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }

    private void TrickReload2(uint id, float delay)
    {
        Cast(id, _savage ? AID.STrickReload : AID.NTrickReload, delay, 4)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x10u, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 10.7f, 3f)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x20u, _savage ? AID.SDartboardOfDancingExplosives : AID.NDartboardOfDancingExplosives, 2.2f, 3f)
            .ActivateOnEnter<Dartboard>(); // bullseye statuses are applied on 3 players ~0.6s after cast end
        Cast(id + 0x30u, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 9.7f, 4f)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<RingARingOExplosions>(id + 0x40u, 1.9f, static comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x41u, 2.2f, static comp => comp.NumResolves > 0, "Stack/spread");
        ComponentCondition<Dartboard>(id + 0x42u, 1.1f, static comp => comp.NumCasts > 0, "Colors")
            .DeactivateOnExit<Dartboard>();

        Cast(id + 0x100u, _savage ? AID.SSurpriseBalloon : AID.NSurpriseBalloon, 8.5f, 4f);
        Cast(id + 0x110u, _savage ? AID.SBeguilingGlitter : AID.NBeguilingGlitter, 3.2f, 4f);
        CastStart(id + 0x120u, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 3.2f);
        ComponentCondition<SurpriseBalloon>(id + 0x121u, 2.7f, static comp => comp.NumCasts > 0, "Knockback 1")
            .ActivateOnEnter<NTriggerHappy>(!_savage)
            .ActivateOnEnter<STriggerHappy>(_savage)
            .ActivateOnEnter<NSurpriseBalloon>(!_savage)
            .ActivateOnEnter<SSurpriseBalloon>(_savage);
        CastEnd(id + 0x122, 1.6f);
        ComponentCondition<TriggerHappy>(id + 0x123u, 0.7f, static comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();
        ComponentCondition<SurpriseBalloon>(id + 0x124u, 2.5f, static comp => comp.NumCasts > 1, "Knockback 2")
            .DeactivateOnExit<SurpriseBalloon>();

        Cast(id + 0x130u, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 0.7f, 4)
            .ActivateOnEnter<BeguilingGlitter>();
        ComponentCondition<BeguilingGlitter>(id + 0x132u, 1.2f, static comp => comp.NumActiveForcedMarches > 0, "Forced march");
        ComponentCondition<Trapshooting>(id + 0x133u, 2.9f, static comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<BeguilingGlitter>() // forced marches end ~0.9s before resolve
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss disappears");
        Cast(id + 0x10u, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 1.5f, 3f)
            .ActivateOnEnter<RingARingOExplosions>();
        Cast(id + 0x20u, _savage ? AID.SPresentBox : AID.NPresentBox, 2.1f, 3f)
            .ActivateOnEnter<Fireworks>()
            .ActivateOnEnter<Fireworks1Hints>();
        // +0.9s: spawn 4x staffs, 2x missiles/claws
        // +1.6s: missiles/claws tether to players
        Cast(id + 0x30u, _savage ? AID.SFireworks : AID.NFireworks, 2.1f, 3f)
            .ActivateOnEnter<NFaerieRing>(!_savage) // casts start ~2.2s into cast
            .ActivateOnEnter<SFaerieRing>(_savage);
        ComponentCondition<BurningChains>(id + 0x40u, 4.6f, static comp => comp.Active, "Chains")
            .ActivateOnEnter<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x50u, 5.1f, static comp => comp.Spreads.Count == 0, "Stack/spread")
            .DeactivateOnExit<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x51u, 0.1f, static comp => !comp.Active)
            .DeactivateOnExit<Fireworks1Hints>()
            .DeactivateOnExit<Fireworks>();
        ComponentCondition<RingARingOExplosions>(id + 0x52u, 0.1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<FaerieRing>(id + 0x53u, 0.2f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<FaerieRing>();
        Targetable(id + 0x60u, true, 2.0f, "Boss reappears");
    }

    private void PinwheelingDartboard(uint id, float delay)
    {
        Cast(id, _savage ? AID.SPinwheelingDartboard : AID.NPinwheelingDartboard, delay, 3f)
            .ActivateOnEnter<Dartboard>()
            .ActivateOnEnter<FireSpread>() // first cast starts ~2.6s after cast-end
            .ActivateOnEnter<Fireworks>()
            .ActivateOnEnter<Fireworks2Hints>();
        Cast(id + 0x10u, _savage ? AID.SFireworks : AID.NFireworks, 5.7f, 3);
        ComponentCondition<FireSpread>(id + 0x20u, 1.9f, static comp => comp.NumCasts > 0);
        ComponentCondition<BurningChains>(id + 0x30u, 2.7f, static comp => comp.Active, "Chains")
            .ActivateOnEnter<BurningChains>();
        ComponentCondition<Fireworks>(id + 0x40u, 5.1f, static comp => !comp.Active, "Stack/spread")
            .DeactivateOnExit<Fireworks2Hints>()
            .DeactivateOnExit<BurningChains>()
            .DeactivateOnExit<Fireworks>();
        ComponentCondition<Dartboard>(id + 0x50u, 0.4f, static comp => comp.NumCasts > 0, "Colors")
            .DeactivateOnExit<Dartboard>();
        ComponentCondition<FireSpread>(id + 0x60u, 3.9f, static comp => comp.NumCasts >= 36, "Fire wall resolve")
            .DeactivateOnExit<FireSpread>();
    }

    private void TrickReload3(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBeguilingGlitter : AID.NBeguilingGlitter, delay, 4f);
        Cast(id + 0x10u, _savage ? AID.STrickReload : AID.NTrickReload, 3.2f, 4f)
            .ActivateOnEnter<TrickReload>();
        Cast(id + 0x20u, _savage ? AID.STrapshooting1 : AID.NTrapshooting1, 10.7f, 4f)
            .ActivateOnEnter<Trapshooting>();
        ComponentCondition<Trapshooting>(id + 0x22u, 4.1f, static comp => comp.NumResolves > 0, "Stack/spread");

        Cast(id + 0x30u, _savage ? AID.SPresentBox : AID.NPresentBox, 0.1f, 3f);
        // +1.0s: spawn 4x staffs
        Cast(id + 0x40u, _savage ? AID.SRingARingOExplosions : AID.NRingARingOExplosions, 3.6f, 3f)
            .ActivateOnEnter<BeguilingGlitter>()
            .ActivateOnEnter<NFaerieRing>(!_savage)
            .ActivateOnEnter<SFaerieRing>(_savage);
        Cast(id + 0x50u, _savage ? AID.STriggerHappy : AID.NTriggerHappy, 3.1f, 4.3f)
            .ActivateOnEnter<NTriggerHappy>(!_savage) // TODO: ideally we'd like to show this earlier...
            .ActivateOnEnter<STriggerHappy>(_savage);
        ComponentCondition<FaerieRing>(id + 0x52u, 0.6f, static comp => comp.NumCasts > 0, "Donuts")
            .DeactivateOnExit<FaerieRing>();
        ComponentCondition<TriggerHappy>(id + 0x53u, 0.1f, static comp => comp.NumCasts > 0, "Pizza")
            .DeactivateOnExit<TriggerHappy>();

        CastStart(id + 0x60u, _savage ? AID.STrapshooting2 : AID.NTrapshooting2, 2.1f)
            .ActivateOnEnter<RingARingOExplosions>();
        CastEnd(id + 0x61u, 4);
        ComponentCondition<RingARingOExplosions>(id + 0x62u, 4.0f, static comp => comp.NumCasts > 0, "Bombs")
            .DeactivateOnExit<RingARingOExplosions>();
        ComponentCondition<Trapshooting>(id + 0x63u, 0.1f, static comp => comp.NumResolves > 1, "Spread/stack")
            .DeactivateOnExit<BeguilingGlitter>()
            .DeactivateOnExit<Trapshooting>()
            .DeactivateOnExit<TrickReload>();
    }
}

sealed class C033NStaticeStates(BossModule module) : C033StaticeStates(module, false);
sealed class C033SStaticeStates(BossModule module) : C033StaticeStates(module, true);
