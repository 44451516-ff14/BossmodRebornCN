namespace BossMod.Endwalker.Unreal.Un2Sephirot;

sealed class Un2SephirotStates : StateMachineBuilder
{
    private readonly Un2Sephirot _module;

    public Un2SephirotStates(Un2Sephirot module) : base(module)
    {
        _module = module;
        SimplePhase(0u, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1u, Phase2, "P2: adds")
            .ActivateOnEnter<P2GenesisCochma>()
            .ActivateOnEnter<P2GenesisBinah>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.FindComponent<P2GenesisCochma>()!.NumCasts >= 2 && Module.FindComponent<P2GenesisBinah>()!.NumCasts >= 12;
        SimplePhase(2u, Phase3, "P3")
            .ActivateOnEnter<P3Yesod>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BossP3()?.IsDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        Phase1Start(id, 6.1f);
        Phase1Repeat(id + 0x100000u, 6.1f);
        Phase1Repeat(id + 0x200000u, 6.1f);
        Phase1Repeat(id + 0x300000u, 6.1f); // and so on...
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void Phase1Start(uint id, float delay)
    {
        ComponentCondition<P1TripleTrial>(id, delay, static comp => comp.NumCasts >= 1, "Cleave 1")
            .ActivateOnEnter<P1TripleTrial>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1TripleTrial>(id + 0x10u, 14.3f, static comp => comp.NumCasts >= 2, "Cleave 2")
            .DeactivateOnExit<P1TripleTrial>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Phase1Repeat(uint id, float delay)
    {
        P1FiendishRage(id, delay);
        P1Chesed(id + 0x10000u, 11f);
        P1EinRatzon(id + 0x20000u, 8.1f);
        P1Chesed(id + 0x30000u, 5.8f);
    }

    private void P1FiendishRage(uint id, float delay)
    {
        ComponentCondition<EinSof>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<EinSof>();
        ComponentCondition<EinSof>(id + 2u, 4f, static comp => comp.NumCasts > 0, "Orbs"); // first hit
        ComponentCondition<P1FiendishRage>(id + 0x10u, 6.3f, static comp => comp.NumCasts > 0, "Hit 1")
            .ActivateOnEnter<P1FiendishRage>();
        ComponentCondition<P1FiendishRage>(id + 0x11u, 3.3f, static comp => comp.NumCasts > 1, "Hit 2")
            .DeactivateOnExit<P1FiendishRage>();
        ComponentCondition<EinSof>(id + 0x20u, 2.4f, static comp => !comp.Active, "Orbs disappear")
            .DeactivateOnExit<EinSof>();
    }

    private void P1Chesed(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.Chesed, delay, 4f, true, "Tankbuster")
            .ActivateOnEnter<P1TripleTrial>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1TripleTrial>(id + 0x10u, 2.2f, static comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<P1TripleTrial>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1EinRatzon(uint id, float delay)
    {
        ComponentCondition<EinSof>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<EinSof>();
        ComponentCondition<EinSof>(id + 2u, 4f, static comp => comp.NumCasts > 0, "Orbs"); // first hit
        ActorCastStart(id + 0x10u, _module.BossP1, AID.Ein, 2.2f, true, "Bait")
            .ActivateOnEnter<P1Ratzon>();
        ActorCastEnd(id + 0x11u, _module.BossP1, 4f, true)
            .ActivateOnEnter<P1Ein>()
            .DeactivateOnExit<P1Ein>();
        ComponentCondition<EinSof>(id + 0x20u, 5.7f, static comp => !comp.Active, "Orbs disappear")
            .DeactivateOnExit<P1Ratzon>()
            .DeactivateOnExit<EinSof>();
    }

    private void Phase2(uint id)
    {
        // TODO: adds spawn either when all from previous set are dead or by timeout...
        Timeout(id, 0f)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Condition(id + 1u, 2.6f, () => _module.Enemies((uint)OID.Cochma).Any(c => c.IsTargetable), "Initial adds")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        SimpleState(id + 0xFF0000u, 10000f, "Adds enrage");
    }

    private void Phase3(uint id)
    {
        Phase3Start(id);
        Phase3Repeat(id + 0x100000u, 1.2f);
        Phase3Repeat(id + 0x200000u, 8.1f);
        SimpleState(id + 0x300000u, 16.2f, "Enrage"); // repeats impact of hod + 2x pillar of severity, latter now oneshotting
    }

    private void Phase3Start(uint id)
    {
        Timeout(id, 0u)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P3EinSofOhr>(id + 0x10u, 29.2f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P3EinSofOhr>()
            .DeactivateOnExit<P3EinSofOhr>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x20u, _module.BossP3, true, 9f, "Reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void Phase3Repeat(uint id, float delay)
    {
        P3Yesod(id, delay);
        P3ForceField(id + 0x10000u, 10.2f);
        P3EarthshakerYesod(id + 0x20000u, 2.5f);
        P3Daat(id + 0x30000u, 1.6f);
        P3FiendishWail(id + 0x40000u, 1.1f);
        P3GevurahChesed(id + 0x50000u, 2.1f); // consider merge with prev
        P3PillarsOfMercy(id + 0x60000u, 3.6f);
        P3Earthshaker(id + 0x70000u, 4.4f);
        P3DaatYesad(id + 0x80000u, 3.3f);
        P3FiendishWail(id + 0x90000u, 1.1f);
        P3GevurahChesed(id + 0xA0000u, 2.1f); // consider merge with prev
        P3Malkuth(id + 0xB0000u, 1.6f);
        P3StormOfWords(id + 0xC0000u, 4.2f);
    }

    private void P3Yesod(uint id, float delay)
    {
        ComponentCondition<P3Yesod>(id, delay, static comp => comp.Casters.Count > 0, "Twisters bait");
        ComponentCondition<P3Yesod>(id + 1u, 3f, static comp => comp.Casters.Count == 0, "Twisters resolve");
    }

    private void P3ForceField(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP3, [AID.GevurahChesed, AID.ChesedGevurah], delay, 5f, true)
            .ActivateOnEnter<P3GevurahChesed>();
        ComponentCondition<P3GevurahChesed>(id + 2u, 0.6f, static comp => comp.NumCasts > 0, "Match color")
            .DeactivateOnExit<P3GevurahChesed>();
        ComponentCondition<P3FiendishWail>(id + 0x10u, 1.6f, static comp => comp.Active)
            .ActivateOnEnter<P3FiendishWail>();
        ComponentCondition<P3FiendishWail>(id + 0x11u, 4f, static comp => !comp.Active, "Towers 1")
            .ActivateOnEnter<EinSof>()
            .DeactivateOnExit<P3FiendishWail>();

        // TODO: tethers

        ComponentCondition<P3FiendishWail>(id + 0x40u, 14.6f, static comp => comp.Active)
            .ActivateOnEnter<P3FiendishWail>()
            .DeactivateOnExit<EinSof>();
        ComponentCondition<P3FiendishWail>(id + 0x41u, 4f, static comp => !comp.Active, "Towers 2")
            .DeactivateOnExit<P3FiendishWail>();

        ActorCastMulti(id + 0x50u, _module.BossP3, [AID.GevurahChesed, AID.ChesedGevurah], 2.2f, 5f, true)
            .ActivateOnEnter<P3GevurahChesed>();
        ComponentCondition<P3GevurahChesed>(id + 0x52u, 0.6f, static comp => comp.NumCasts > 0, "Match color")
            .DeactivateOnExit<P3GevurahChesed>();
    }

    private void P3EarthshakerYesod(uint id, float delay)
    {
        ComponentCondition<P3Earthshaker>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<P3Earthshaker>();
        ComponentCondition<P3Yesod>(id + 1u, 3.6f, static comp => comp.Casters.Count > 0, "Twisters bait");
        ComponentCondition<P3Earthshaker>(id + 2u, 1.3f, static comp => !comp.Active, "Earthshakers")
            .DeactivateOnExit<P3Earthshaker>();
        ComponentCondition<P3Yesod>(id + 3u, 1.7f, static comp => comp.Casters.Count == 0, "Twisters resolve");
    }

    private void P3Daat(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.DaatMT, delay, 5f, true, "Spread hit 1")
            .ActivateOnEnter<P3Daat>();
        ComponentCondition<P3Daat>(id + 0x10u, 3.2f, static comp => comp.NumCasts >= 3, "Spread hit 4")
            .DeactivateOnExit<P3Daat>();
    }

    private void P3FiendishWail(uint id, float delay)
    {
        ComponentCondition<P3FiendishWail>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<P3FiendishWail>();
        ComponentCondition<P3FiendishWail>(id + 1u, 4f, static comp => !comp.Active, "Towers (tanks)")
            .DeactivateOnExit<P3FiendishWail>();
    }

    private void P3GevurahChesed(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP3, [AID.GevurahChesed, AID.ChesedGevurah], delay, 5, true)
            .ActivateOnEnter<P3GevurahChesed>();
        ComponentCondition<P3GevurahChesed>(id + 2u, 0.6f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<P3GevurahChesed>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3PillarsOfMercy(uint id, float delay)
    {
        ComponentCondition<P3PillarOfMercyKnockback>(id, delay, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P3PillarOfMercyAOE>()
            .ActivateOnEnter<P3PillarOfMercyKnockback>()
            .ActivateOnEnter<EinSof>();
        ComponentCondition<P3Yesod>(id + 1u, 1.7f, static comp => comp.Casters.Count > 0, "Twisters bait");
        ComponentCondition<P3PillarOfMercyKnockback>(id + 2u, 2.7f, static comp => comp.NumCasts >= 1, "Knockback 1");
        ComponentCondition<P3Yesod>(id + 3u, 0.3f, static comp => comp.Casters.Count == 0); // no name, since time difference is too small
        ComponentCondition<P3PillarOfMercyKnockback>(id + 0x10u, 4.8f, static comp => comp.NumCasts >= 2, "Knockback 2");
        ComponentCondition<P3PillarOfMercyKnockback>(id + 0x20u, 4.0f, static comp => comp.NumCasts >= 3, "Knockback 3")
            .DeactivateOnExit<P3PillarOfMercyAOE>()
            .DeactivateOnExit<P3PillarOfMercyKnockback>()
            .DeactivateOnExit<EinSof>(); // TODO: this happens a bit later, but need more investigation...
        //ComponentCondition<P1EinSof>(id + 0x30, 2.6f, static comp => !comp.Active, "Orbs disappear")
        //    .DeactivateOnExit<P1EinSof>();
    }

    private void P3Earthshaker(uint id, float delay)
    {
        ComponentCondition<P3Earthshaker>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<P3Earthshaker>();
        ComponentCondition<P3Earthshaker>(id + 1u, 4.9f, static comp => !comp.Active, "Earthshakers")
            .DeactivateOnExit<P3Earthshaker>();
    }

    private void P3DaatYesad(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP3, AID.DaatMT, delay, true);
        ComponentCondition<P3Yesod>(id + 1u, 4.1f, static comp => comp.Casters.Count > 0, "Twisters bait")
            .ActivateOnEnter<P3Daat>();
        ActorCastEnd(id + 2u, _module.BossP3, 0.9f, true, "Spread hit 1");
        ComponentCondition<P3Yesod>(id + 0x10u, 2.1f, static comp => comp.Casters.Count == 0, "Twisters resolve");
        ComponentCondition<P3Daat>(id + 0x20u, 1.1f, static comp => comp.NumCasts >= 3, "Spread hit 4")
            .DeactivateOnExit<P3Daat>();
    }

    private void P3Malkuth(uint id, float delay)
    {
        ActorCast(id, _module.BossP3, AID.Malkuth, delay, 4f, true, "Knockback")
            .ActivateOnEnter<P3Malkuth>()
            .DeactivateOnExit<P3Malkuth>();
    }

    private void P3StormOfWords(uint id, float delay)
    {
        P3GevurahChesed(id, delay);

        ComponentCondition<P3FiendishWail>(id + 0x1000u, 1.5f, static comp => comp.Active)
            .ActivateOnEnter<P3FiendishWail>();
        ComponentCondition<P3Yesod>(id + 0x1001u, 2.7f, static comp => comp.Casters.Count > 0, "Twisters bait");
        ComponentCondition<P3FiendishWail>(id + 0x1002u, 1.3f, static comp => !comp.Active, "Towers (tanks)")
            .DeactivateOnExit<P3FiendishWail>();
        ComponentCondition<P3Yesod>(id + 0x1003u, 1.7f, static comp => comp.Casters.Count == 0, "Twisters resolve");

        P3GevurahChesed(id + 0x2000u, 0.5f);

        ComponentCondition<P3Yesod>(id + 0x3000u, 9.5f, static comp => comp.Casters.Count > 0, "Twisters bait");
        ActorCastStartMulti(id + 0x3001u, _module.BossP3, [AID.GevurahChesed, AID.ChesedGevurah], 1.1f, true);
        ComponentCondition<P3Yesod>(id + 0x3002u, 1.9f, static comp => comp.Casters.Count == 0, "Twisters resolve")
            .ActivateOnEnter<P3GevurahChesed>();
        ActorCastEnd(id + 0x3003u, _module.BossP3, 3.1f, true);
        ComponentCondition<P3GevurahChesed>(id + 0x3004u, 0.6f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<P3GevurahChesed>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<P3Ascension>(id + 0x4000u, 10.6f, static comp => comp.NumCasts > 0, "Ascension")
            .ActivateOnEnter<P3Ascension>()
            .DeactivateOnExit<P3Ascension>();
    }
}
