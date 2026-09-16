namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class C021ShishioStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C021ShishioStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChange>();
    }

    private void SinglePhase(uint id)
    {
        Enkyo(id, 6.2f);
        StormcloudSummonsRokujoRevel(id + 0x10000u, 4.5f);
        SplittingCrySlither(id + 0x20000u, 3.4f); // note: delay could be 4.7 or 8.2, depending on breath-in count
        NoblePursuit(id + 0x30000u, 7.6f);
        UnnaturalWailHauntingCry(id + 0x40000u, 4.3f);
        StormcloudSummonsLightningBolt(id + 0x50000u, 6.6f); // always 1 breath-in
        UnnaturalWailEyeVortex(id + 0x60000u, 4.4f);
        Enkyo(id + 0x70000u, 3.5f);
        HauntingCryAddsTowers(id + 0x80000u, 5.2f);
        ThunderVortex(id + 0x90000u, 2.1f);
        SplittingCrySlither(id + 0xA0000u, 2.1f);
        StormcloudSummonsRokujoRevel(id + 0xB0000u, 7.4f);
        StormcloudSummonsLightningBolt(id + 0xC0000u, 4.7f); // note: delay could be 6.1 or 9.7, depending on breath-in count; always 2 or 3 breath-ins?
        UnnaturalWailEyeVortex(id + 0xD0000u, 0.6f); // note: delay could be 2.9, depending on breath-in count
        Cast(id + 0xE0000u, _savage ? AID.SEnrage : AID.NEnrage, 5.2f, 10f, "Enrage");
    }

    private void Enkyo(uint id, float delay)
    {
        Cast(id, _savage ? AID.SEnkyo : AID.NEnkyo, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormcloudSummonsRokujoRevel(uint id, float delay)
    {
        Cast(id, _savage ? AID.SStormcloudSummons : AID.NStormcloudSummons, delay, 3f);
        // +0.7s: envc 34/35 - circular arena?
        // +1.0s: spawn 18x raiun
        Cast(id + 0x10u, _savage ? AID.SSmokeaterFirst : AID.NSmokeaterFirst, 2.2f, 2.5f)
            .ActivateOnEnter<RokujoRevel>();
        // +1.5s: first absorbs
        CastStart(id + 0x20u, _savage ? AID.SRokujoRevelFirst : AID.NRokujoRevelFirst, 2.1f) // note: delay could be 4.2 or 6.3, depending on breath-in count
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x21u, 7.5f);
        // subsequent revel casts start with 2.5s delay
        ComponentCondition<RokujoRevel>(id + 0x30u, 0.5f, static comp => comp.NumCasts > 0, "Lines + circles start");
        ComponentCondition<RokujoRevel>(id + 0x40u, 4.3f, static comp => !comp.Active, "Lines + circles resolve", 3) // note: delay could be 5.0 or 5.8, depending on breath-in count
            .DeactivateOnExit<RokujoRevel>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void StormcloudSummonsLightningBolt(uint id, float delay)
    {
        Cast(id, _savage ? AID.SStormcloudSummons : AID.NStormcloudSummons, delay, 3f);
        // +0.7s: envc 34/35 - circular arena?
        // +1.0s: spawn 18x raiun
        Cast(id + 0x10u, _savage ? AID.SSmokeaterFirst : AID.NSmokeaterFirst, 2.2f, 2.5f);
        // +1.5s: first absorbs
        Cast(id + 0x20u, _savage ? AID.SLightningBolt : AID.NLightningBolt, 2.1f, 3f) // note: delay could be 4.2 or 6.3, depending on breath-in count
            .ActivateOnEnter<NLightningBolt>(!_savage)
            .ActivateOnEnter<SLightningBolt>(_savage);
        ComponentCondition<LightningBolt>(id + 0x30u, 1.0f, static comp => comp.NumCasts > 0, "Lines start")
            .DeactivateOnExit<LightningBolt>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<CloudToCloud>(id + 0x40u, 0.9f, static comp => comp.Active)
            .ActivateOnEnter<CloudToCloud>();
        ComponentCondition<CloudToCloud>(id + 0x50u, 18.3f, static comp => !comp.Active, "Lines resolve", 5f) // note: delay could be 20.8 or 21.2, depending on breath-in count
            .DeactivateOnExit<CloudToCloud>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void SplittingCrySlither(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSplittingCry : AID.NSplittingCry, delay, 5f, "Tankbuster")
            .ActivateOnEnter<NSplittingCry>(!_savage)
            .ActivateOnEnter<SSplittingCry>(_savage)
            .ActivateOnEnter<Slither>()
            .DeactivateOnExit<SplittingCry>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x10u, _savage ? AID.SSlither : AID.NSlither, 2.2f, 2f, "Back cleave")
            .DeactivateOnExit<Slither>();
    }

    private void NoblePursuit(uint id, float delay)
    {
        CastStart(id, _savage ? AID.SNoblePursuitFirst : AID.NNoblePursuitFirst, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1u, 8f, "Charge 1")
            .ActivateOnEnter<NoblePursuit>();
        ComponentCondition<NoblePursuit>(id + 0x10u, 1.4f, static comp => comp.NumCasts > 1, "Charge 2");
        ComponentCondition<NoblePursuit>(id + 0x20u, 1.4f, static comp => comp.NumCasts > 2, "Charge 3");
        ComponentCondition<NoblePursuit>(id + 0x30u, 1.4f, static comp => comp.NumCasts > 3, "Charge 4");
        ComponentCondition<NoblePursuit>(id + 0x40u, 0.8f, static comp => !comp.Active)
            .DeactivateOnExit<NoblePursuit>()
            .SetHint(StateMachine.StateHint.PositioningEnd);

        Enkyo(id + 0x1000u, 0.7f);
    }

    private void UnnaturalWailHauntingCry(uint id, float delay)
    {
        Cast(id, _savage ? AID.SUnnaturalWail : AID.NUnnaturalWail, delay, 3f)
            .ActivateOnEnter<UnnaturalWail>(); // debuffs are assigned 0.9s after cast ends
        Cast(id + 0x10u, _savage ? AID.SHauntingCry : AID.NHauntingCry, 2.2f, 3f);
        ComponentCondition<HauntingCrySwipes>(id + 0x20u, 12f, static comp => comp.NumCasts > 0, "Swipes")
            .ActivateOnEnter<HauntingCrySwipes>();
        ComponentCondition<UnnaturalWail>(id + 0x21u, 0.9f, static comp => comp.NumMechanics > 0, "Spread/stack 1");
        ComponentCondition<HauntingCrySwipes>(id + 0x30u, 6.1f, static comp => comp.NumCasts > 4, "Swipes")
            .DeactivateOnExit<HauntingCrySwipes>();
        ComponentCondition<UnnaturalWail>(id + 0x31u, 0.9f, static comp => comp.NumMechanics > 1, "Stack/spread 2")
            .DeactivateOnExit<UnnaturalWail>();
    }

    private void UnnaturalWailEyeVortex(uint id, float delay)
    {
        Cast(id, _savage ? AID.SUnnaturalWail : AID.NUnnaturalWail, delay, 3)
            .ActivateOnEnter<UnnaturalWail>(); // debuffs are assigned 0.9s after cast ends
        CastMulti(id + 0x10u, [_savage ? AID.SEyeOfTheThunderVortexFirst : AID.NEyeOfTheThunderVortexFirst, _savage ? AID.SVortexOfTheThunderEyeFirst : AID.NVortexOfTheThunderEyeFirst], 2.2f, 5.2f, "In/out")
            .ActivateOnEnter<EyeThunderVortex>();
        ComponentCondition<UnnaturalWail>(id + 0x20u, 0.6f, static comp => comp.NumMechanics > 0, "Spread/stack 1");
        ComponentCondition<EyeThunderVortex>(id + 0x30u, 3.4f, static comp => comp.NumCasts > 1, "Out/in")
            .DeactivateOnExit<EyeThunderVortex>();
        ComponentCondition<UnnaturalWail>(id + 0x31u, 0.6f, static comp => comp.NumMechanics > 1, "Stack/spread 2")
            .DeactivateOnExit<UnnaturalWail>();
    }

    private void HauntingCryAddsTowers(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHauntingCry : AID.NHauntingCry, delay, 3f)
            .ActivateOnEnter<HauntingCryReisho>();
        // +0.9s: tethers appear
        ComponentCondition<HauntingCryReisho>(id + 0x10u, 6.0f, static comp => comp.NumCasts > 0, "Ghost aoes start");
        CastStart(id + 0x20u, _savage ? AID.SVengefulSouls : AID.NVengefulSouls, 1.5f);
        ComponentCondition<HauntingCryReisho>(id + 0x30u, 0.5f, static comp => comp.NumCasts > 1)
            .ActivateOnEnter<NHauntingCryVermilionAura>(!_savage)
            .ActivateOnEnter<NHauntingCryStygianAura>(!_savage)
            .ActivateOnEnter<SHauntingCryVermilionAura>(_savage)
            .ActivateOnEnter<SHauntingCryStygianAura>(_savage);
        ComponentCondition<HauntingCryReisho>(id + 0x31u, 2.1f, static comp => comp.NumCasts > 2);
        ComponentCondition<HauntingCryReisho>(id + 0x32u, 2.1f, static comp => comp.NumCasts > 3);
        ComponentCondition<HauntingCryReisho>(id + 0x33u, 2.1f, static comp => comp.NumCasts > 4);
        ComponentCondition<HauntingCryReisho>(id + 0x34u, 2.1f, static comp => comp.NumCasts > 5);
        ComponentCondition<HauntingCryReisho>(id + 0x35u, 2.1f, static comp => comp.NumCasts > 6);
        ComponentCondition<HauntingCryReisho>(id + 0x36u, 2.1f, static comp => comp.NumCasts > 7, "Ghost aoes end")
            .DeactivateOnExit<HauntingCryReisho>();
        CastEnd(id + 0x40u, 2.1f, "Towers/defamations")
            .DeactivateOnExit<HauntingCryVermilionAura>()
            .DeactivateOnExit<HauntingCryStygianAura>();
    }

    private void ThunderVortex(uint id, float delay)
    {
        Cast(id, _savage ? AID.SThunderVortex : AID.NThunderVortex, delay, 5f, "In")
            .ActivateOnEnter<NThunderVortex>(!_savage)
            .ActivateOnEnter<SThunderVortex>(_savage)
            .DeactivateOnExit<ThunderVortex>();
    }
}

sealed class C021NShishioStates(BossModule module) : C021ShishioStates(module, false);
sealed class C021SShishioStates(BossModule module) : C021ShishioStates(module, true);
