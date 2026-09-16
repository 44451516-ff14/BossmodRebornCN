namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

abstract class C013ShadowcasterStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C013ShadowcasterStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChange>();
    }

    private void SinglePhase(uint id)
    {
        ShowOfStrength(id, 10.2f);
        InfernBrand1(id + 0x10000u, 8.2f);
        FiresteelFracture(id + 0x20000u, 5.1f);
        InfernBrand2(id + 0x30000u, 8.2f);
        ShowOfStrength(id + 0x40000u, 4.1f);
        InfernBrand3(id + 0x50000u, 8.2f);
        FiresteelFracture(id + 0x60000u, 3.4f);
        InfernBrand4(id + 0x70000u, 8.2f);
        ShowOfStrength(id + 0x80000u, 5.1f);
        InfernBrand5(id + 0x90000u, 8.2f);
        FiresteelFracture(id + 0xA0000u, 1.6f);
        ShowOfStrength(id + 0xB0000u, 4.1f);
        Cast(id + 0xC0000u, AID.Enrage, 4.8f, 10f, "Enrage");
    }

    private void ShowOfStrength(uint id, float delay)
    {
        Cast(id, _savage ? AID.SShowOfStrength : AID.NShowOfStrength, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FiresteelFracture(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFiresteelFracture : AID.NFiresteelFracture, delay, 5f)
            .ActivateOnEnter<NFiresteelFracture>(!_savage)
            .ActivateOnEnter<SFiresteelFracture>(_savage);
        ComponentCondition<FiresteelFracture>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<FiresteelFracture>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void InfernBrand1(uint id, float delay)
    {
        Cast(id, AID.InfernBrand, delay, 4f);
        Cast(id + 0x10u, AID.CrypticPortal, 3.2f, 4f)
            .ActivateOnEnter<NPortalsBurn>(!_savage)
            .ActivateOnEnter<SPortalsBurn>(_savage); // eobjanims ~0.8s after cast end
        Cast(id + 0x20u, AID.FiresteelStrike, 6.2f, 4.9f)
            .ActivateOnEnter<FiresteelStrike>(); // TODO: consider activating earlier?..
        ComponentCondition<FiresteelStrike>(id + 0x22u, 0.3f, static comp => comp.NumJumps > 0, "Jump 1");
        ComponentCondition<PortalsAOE>(id + 0x30u, 1f, static comp => comp.NumCasts > 0, "Portal aoe")
            .DeactivateOnExit<PortalsAOE>();
        ComponentCondition<FiresteelStrike>(id + 0x31u, 0.5f, static comp => comp.NumJumps > 1, "Jump 2");

        Cast(id + 0x40u, AID.BlessedBeacon, 3.1f, 4.9f);
        ComponentCondition<FiresteelStrike>(id + 0x42u, 0.5f, static comp => comp.NumCleaves > 0, "Cleave 1");
        ComponentCondition<FiresteelStrike>(id + 0x43u, 2.2f, static comp => comp.NumCleaves > 1, "Cleave 2")
            .DeactivateOnExit<FiresteelStrike>();
    }

    // TODO: hints for mirrors
    private void InfernBrand2(uint id, float delay)
    {
        Cast(id, AID.InfernBrand, delay, 4f)
            .ActivateOnEnter<NBlazingBenifice>(!_savage)
            .ActivateOnEnter<SBlazingBenifice>(_savage); // TODO: proper activation time (first set of arcane fonts spawn around cryptic flames cast start, second spawn ~12s later)
        Cast(id + 0x10u, AID.CrypticFlames, 3.2f, 8.3f)
            .ActivateOnEnter<CrypticFlames>(); // note: statuses appear right before cast start
        ComponentCondition<CrypticFlames>(id + 0x12u, 2.7f, static comp => comp.ReadyToBreak, "Lasers break start");
        CastStart(id + 0x20u, AID.CastShadow, 6.7f);
        ComponentCondition<BlazingBenifice>(id + 0x21u, 4.1f, static comp => comp.NumCasts > 0, "Mirrors 1")
            .ActivateOnEnter<CastShadow>(); // all cast-shadow casts start at the same time
        CastEnd(id + 0x22u, 0.6f);
        ComponentCondition<CastShadow>(id + 0x23u, 0.7f, static comp => comp.NumCasts == 6, "Pizzas 1");
        ComponentCondition<CastShadow>(id + 0x24u, 2, static comp => comp.NumCasts == 12, "Pizzas 2")
            .DeactivateOnExit<CastShadow>();

        CastStart(id + 0x30u, _savage ? AID.SFiresteelFracture : AID.NFiresteelFracture, 7.7f);
        ComponentCondition<BlazingBenifice>(id + 0x31u, 1f, static comp => comp.NumCasts >= 5, "Mirrors 2")
            .ActivateOnEnter<NFiresteelFracture>(!_savage)
            .ActivateOnEnter<SFiresteelFracture>(_savage)
            .DeactivateOnExit<CrypticFlames>()
            .DeactivateOnExit<BlazingBenifice>();
        CastEnd(id + 0x32u, 4f);
        ComponentCondition<FiresteelFracture>(id + 0x33u, 0.2f, static comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<FiresteelFracture>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void InfernBrand3(uint id, float delay)
    {
        Cast(id, AID.InfernBrand, delay, 4f)
            .ActivateOnEnter<CrypticFlames>(); // lasers gain counter 1C1 ~1.9s after cast end
        Cast(id + 0x10u, AID.InfernWave, 4.2f, 4f);
        Cast(id + 0x20u, AID.Banishment, 4.5f, 4f)
            .ActivateOnEnter<NInfernWave1>(!_savage)
            .ActivateOnEnter<SInfernWave1>(_savage) // two of the beacons activate ~2.6s after cast start
            .ActivateOnEnter<PortalsWave>(); // portal statuses appear right at cast end, eobjanims on portals happen ~0.8s after cast end
        Cast(id + 0x30u, AID.InfernWard, 4.2f, 4f);
        ComponentCondition<PortalsWave>(id + 0x40u, 2.9f, static comp => comp.Done, "Portals")
            .ExecOnExit<InfernWave>(static comp => comp.ShowHints = true)
            .DeactivateOnExit<PortalsWave>();
        ComponentCondition<InfernWave>(id + 0x50u, 4.8f, static comp => comp.NumCasts > 0, "Wave 1");
        // +0.9s: stun
        // +5.9s: stun end
        ComponentCondition<InfernWave>(id + 0x60u, 8, static comp => comp.NumCasts > 4, "Wave 2")
            .DeactivateOnExit<CrypticFlames>()
            .DeactivateOnExit<InfernWave>();
    }

    private void InfernBrand4(uint id, float delay)
    {
        Cast(id, AID.InfernBrand, delay, 4f);
        Cast(id + 0x10u, AID.CrypticPortal, 3.2f, 4f)
            .ActivateOnEnter<NPortalsMirror>(!_savage)
            .ActivateOnEnter<SPortalsMirror>(_savage) // eobjanims ~0.8s after cast end
            .ActivateOnEnter<FiresteelStrike>();
        Cast(id + 0x20u, AID.FiresteelStrike, 6.2f, 4.9f);
        ComponentCondition<FiresteelStrike>(id + 0x22u, 0.3f, static comp => comp.NumJumps > 0, "Jump 1");
        ComponentCondition<PortalsAOE>(id + 0x30u, 1f, static comp => comp.NumCasts > 0, "Mirror aoe")
            .DeactivateOnExit<PortalsAOE>();
        ComponentCondition<FiresteelStrike>(id + 0x31u, 0.5f, static comp => comp.NumJumps > 1, "Jump 2");

        Cast(id + 0x40u, AID.BlessedBeacon, 3.1f, 4.9f);
        ComponentCondition<FiresteelStrike>(id + 0x42u, 0.5f, static comp => comp.NumCleaves > 0, "Cleave 1");
        ComponentCondition<FiresteelStrike>(id + 0x43u, 2.1f, static comp => comp.NumCleaves > 1, "Cleave 2")
            .DeactivateOnExit<FiresteelStrike>();
    }

    private void InfernBrand5(uint id, float delay)
    {
        Cast(id, AID.InfernBrand, delay, 4f);
        Cast(id + 0x10u, AID.InfernWave, 4.2f, 4f)
            .ActivateOnEnter<NInfernWave2>(!_savage)
            .ActivateOnEnter<SInfernWave2>(_savage); // first beacon activates ~2.3s after cast end
        Cast(id + 0x20u, AID.CrypticFlames, 4.2f, 8.3f)
            .ActivateOnEnter<CrypticFlames>(); // note: statuses appear right before cast start
        ComponentCondition<CrypticFlames>(id + 0x22u, 2.7f, static comp => comp.ReadyToBreak, "Lasers break start");

        ComponentCondition<InfernWave>(id + 0x30u, 1.3f, static comp => comp.NumCasts > 0, "Wave 1");
        CastStart(id + 0x40u, AID.PureFire, 8f);
        ComponentCondition<InfernWave>(id + 0x41, 2f, static comp => comp.NumCasts > 2, "Wave 2");
        CastEnd(id + 0x42u, 1f);
        ComponentCondition<PureFire>(id + 0x50u, 0.8f, static comp => comp.Casters.Count > 0, "Puddle bait")
            .ActivateOnEnter<NPureFire>(!_savage)
            .ActivateOnEnter<SPureFire>(_savage);
        ComponentCondition<PureFire>(id + 0x51u, 3f, static comp => comp.Casters.Count == 0)
            .DeactivateOnExit<PureFire>();

        CastStart(id + 0x60u, AID.CastShadow, 4.9f);
        ComponentCondition<InfernWave>(id + 0x61u, 0.4f, static comp => comp.NumCasts > 4, "Wave 3")
            .ActivateOnEnter<CastShadow>();
        CastEnd(id + 0x62u, 4.4f);
        ComponentCondition<CastShadow>(id + 0x63u, 0.7f, static comp => comp.NumCasts == 6, "Pizzas 1");
        ComponentCondition<CastShadow>(id + 0x64u, 2f, static comp => comp.NumCasts == 12, "Pizzas 2")
            .DeactivateOnExit<CastShadow>();
        ComponentCondition<InfernWave>(id + 0x65u, 3f, static comp => comp.NumCasts > 6, "Wave 4")
            .DeactivateOnExit<InfernWave>()
            .DeactivateOnExit<CrypticFlames>();
    }
}
sealed class C013NShadowcasterStates(BossModule module) : C013ShadowcasterStates(module, false);
sealed class C013SShadowcasterStates(BossModule module) : C013ShadowcasterStates(module, true);
