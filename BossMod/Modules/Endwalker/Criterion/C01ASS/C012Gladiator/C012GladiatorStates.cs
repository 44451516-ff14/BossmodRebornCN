namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

abstract class C012GladiatorStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C012GladiatorStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FlashOfSteel(id, 6.2f);
        SpecterOfMight(id + 0x10000u, 8.2f);
        SculptorsPassion(id + 0x20000u, 2.6f);
        MightySmite(id + 0x30000u, 10.2f);
        CurseOfTheFallen(id + 0x40000u, 8.2f);
        FlashOfSteel(id + 0x50000u, 2.8f);
        HatefulVisage(id + 0x60000u, 12.5f);
        FlashOfSteel(id + 0x70000u, 3.4f);
        AccursedVisage(id + 0x80000u, 7.2f);
        FlashOfSteel(id + 0x90000u, 3.4f);
        CurseOfTheMonument(id + 0xA0000u, 12.4f);
        FlashOfSteel(id + 0xB0000u, 2.2f);
        SpecterOfMight(id + 0xC0000u, 10.2f);
        SculptorsPassion(id + 0xD0000u, 2.6f);
        FlashOfSteel(id + 0xE0000u, 10.6f);
        Cast(id + 0xF0000u, AID.Enrage, 2.1f, 10f, "Enrage");
    }

    private void FlashOfSteel(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFlashOfSteel : AID.NFlashOfSteel, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MightySmite(uint id, float delay)
    {
        Cast(id, _savage ? AID.SMightySmite : AID.NMightySmite, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SpecterOfMight(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSpecterOfMight : AID.NSpecterOfMight, delay, 4f);
        ComponentCondition<RushOfMightFront>(id + 0x10u, 4.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NRushOfMightFront>(!_savage)
            .ActivateOnEnter<SRushOfMightFront>(_savage);
        ComponentCondition<RushOfMightFront>(id + 0x11u, 10.5f, static comp => comp.NumCasts > 0, "Charge 1 front")
            .DeactivateOnExit<RushOfMightFront>();

        CastStart(id + 0x20u, _savage ? AID.SSpecterOfMight : AID.NSpecterOfMight, 0.5f)
            .ActivateOnEnter<NRushOfMightBack>(!_savage)
            .ActivateOnEnter<SRushOfMightBack>(_savage);
        ComponentCondition<RushOfMightBack>(id + 0x21u, 1.5f, static comp => comp.NumCasts > 0, "Charge 1 back")
            .DeactivateOnExit<RushOfMightBack>();
        CastEnd(id + 0x22u, 2.5f);
        ComponentCondition<RushOfMightFront>(id + 0x30u, 4.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NRushOfMightFront>(!_savage)
            .ActivateOnEnter<SRushOfMightFront>(_savage);
        ComponentCondition<RushOfMightFront>(id + 0x31u, 10.5f, static comp => comp.NumCasts > 0, "Charge 2 front")
            .DeactivateOnExit<RushOfMightFront>();
        ComponentCondition<RushOfMightBack>(id + 0x40u, 2f, static comp => comp.NumCasts > 0, "Charge 2 back")
            .ActivateOnEnter<NRushOfMightBack>(!_savage)
            .ActivateOnEnter<SRushOfMightBack>(_savage)
            .DeactivateOnExit<RushOfMightBack>();
    }

    private void SculptorsPassion(uint id, float delay)
    {
        CastStart(id, _savage ? AID.SSculptorsPassion : AID.NSculptorsPassion, delay)
            .ActivateOnEnter<NSculptorsPassion>(!_savage)
            .ActivateOnEnter<SSculptorsPassion>(_savage);
        CastEnd(id + 1u, 5f);
        ComponentCondition<SculptorsPassion>(id + 2u, 0.3f, static comp => comp.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<SculptorsPassion>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CurseOfTheFallen(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCurseOfTheFallen : AID.NCurseOfTheFallen, delay, 5f);
        ComponentCondition<CurseOfTheFallen>(id + 2u, 1.1f, static comp => comp.Active)
            .ActivateOnEnter<CurseOfTheFallen>();

        CastMulti(id + 0x10u, [_savage ? AID.SRingOfMight1Out : AID.NRingOfMight1Out, _savage ? AID.SRingOfMight2Out : AID.NRingOfMight2Out, _savage ? AID.SRingOfMight3Out : AID.NRingOfMight3Out], 3.7f, 10, "Out")
            .ActivateOnEnter<NRingOfMight1Out>(!_savage)
            .ActivateOnEnter<NRingOfMight2Out>(!_savage)
            .ActivateOnEnter<NRingOfMight3Out>(!_savage)
            .ActivateOnEnter<SRingOfMight1Out>(_savage)
            .ActivateOnEnter<SRingOfMight2Out>(_savage)
            .ActivateOnEnter<SRingOfMight3Out>(_savage)
            .DeactivateOnExit<RingOfMight1Out>()
            .DeactivateOnExit<RingOfMight2Out>()
            .DeactivateOnExit<RingOfMight3Out>()
            .SetHint(StateMachine.StateHint.Raidwide); // first debuff resolve ~0.2s later

        Condition(id + 0x20u, 2f, () => Module.FindComponent<RingOfMight1In>()!.NumCasts + Module.FindComponent<RingOfMight2In>()!.NumCasts + Module.FindComponent<RingOfMight3In>()!.NumCasts > 0, "In")
            .ActivateOnEnter<NRingOfMight1In>(!_savage)
            .ActivateOnEnter<NRingOfMight2In>(!_savage)
            .ActivateOnEnter<NRingOfMight3In>(!_savage)
            .ActivateOnEnter<SRingOfMight1In>(_savage)
            .ActivateOnEnter<SRingOfMight2In>(_savage)
            .ActivateOnEnter<SRingOfMight3In>(_savage)
            .DeactivateOnExit<RingOfMight1In>()
            .DeactivateOnExit<RingOfMight2In>()
            .DeactivateOnExit<RingOfMight3In>();

        ComponentCondition<CurseOfTheFallen>(id + 0x30u, 1.4f, static comp => !comp.Active, "Resolve")
            .DeactivateOnExit<CurseOfTheFallen>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WrathOfRuin(uint id, float delay)
    {
        // -0.7s: HatefulVisage actors spawn
        Cast(id, _savage ? AID.SWrathOfRuin : AID.NWrathOfRuin, delay, 3f)
            .ActivateOnEnter<GoldenSilverFlame>(); // casts start ~2.1s after cast-start
        // +1.4s: first set of Regrets spawn
        // +3.4s: second set of Regrets spawn
        // +5.4s: first set of RackAndRuin cast-starts
        CastStart(id + 0x10u, _savage ? AID.SNothingBesideRemains : AID.NNothingBesideRemains, 5.7f)
            .ActivateOnEnter<NRackAndRuin>(!_savage)
            .ActivateOnEnter<SRackAndRuin>(_savage);
        // +1.7s: second set of RackAndRuin cast-starts
        ComponentCondition<GoldenSilverFlame>(id + 0x20u, 3.4f, static comp => !comp.Active, "Cells")
            .ActivateOnEnter<NNothingBesideRemains>(!_savage)
            .ActivateOnEnter<SNothingBesideRemains>(_savage)
            .DeactivateOnExit<GoldenSilverFlame>();
        ComponentCondition<RackAndRuin>(id + 0x21u, 0.3f, static comp => comp.NumCasts > 0, "Lines 1");
        CastEnd(id + 0x30u, 1.3f, "Spread")
            .DeactivateOnExit<NothingBesideRemains>();
        ComponentCondition<RackAndRuin>(id + 0x31u, 0.7f, static comp => comp.Casters.Count == 0, "Lines 2")
            .DeactivateOnExit<RackAndRuin>();
    }

    private void HatefulVisage(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHatefulVisage : AID.NHatefulVisage, delay, 3f);
        WrathOfRuin(id + 0x100u, 2.2f);
    }

    private void AccursedVisage(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAccursedVisage : AID.NAccursedVisage, delay, 3f);
        // +1.1s: gilded/silvered fate statuses
        WrathOfRuin(id + 0x100u, 2.2f);
    }

    // TODO: component for tether?..
    private void CurseOfTheMonument(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCurseOfTheMonument : AID.NCurseOfTheMonument, delay, 4f);
        // +1.0s: debuffs/tethers appear

        // after central cast, other pairs are staggered by ~0.6s
        ComponentCondition<SunderedRemains>(id + 0x10u, 1.2f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NSunderedRemains>(!_savage)
            .ActivateOnEnter<SSunderedRemains>(_savage);
        ComponentCondition<SunderedRemains>(id + 0x11u, 8.4f, static comp => comp.Casters.Count == 0, "Last aoe")
            .DeactivateOnExit<SunderedRemains>();

        // note: spread explosions happen ~0.8s before tower explosions...
        Cast(id + 0x100u, _savage ? AID.SColossalWreck : AID.NColossalWreck, 4.9f, 6)
            .ActivateOnEnter<ScreamOfTheFallen>();
        ComponentCondition<ScreamOfTheFallen>(id + 0x110u, 0.5f, static comp => comp.NumCasts > 0, "Towers 1");
        ComponentCondition<ScreamOfTheFallen>(id + 0x120u, 4f, static comp => comp.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<ScreamOfTheFallen>();
    }
}

sealed class C012NGladiatorStates(BossModule module) : C012GladiatorStates(module, false);
sealed class C012SGladiatorStates(BossModule module) : C012GladiatorStates(module, true);
