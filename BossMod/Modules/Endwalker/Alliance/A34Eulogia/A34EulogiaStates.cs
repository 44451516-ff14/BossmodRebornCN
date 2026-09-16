namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class A34EulogiaStates : StateMachineBuilder
{
    public A34EulogiaStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
             .ActivateOnEnter<ArenaChanges>();
    }

    public void SinglePhase(uint id)
    {
        DawnOfTime(id, 9.5f);
        Quintessence(id + 0x10000u, 10f);

        Quintessence(id + 0x20000u, 18.3f);
        Sunbeam(id + 0x30000u, 7.2f);
        Whorl(id + 0x40000u, 8.6f);
        LovesLight(id + 0x50000u, 3.2f);
        SolarFans(id + 0x60000u, 0.1f);
        Hydrostasis(id + 0x70000u, 2.2f);
        DestructiveBolt(id + 0x80000u, 2.3f);
        HieroglyphikaHandOfTheDestroyer(id + 0x90000u, 5.5f);
        MatronsBreath(id + 0xA0000u, 5.8f);
        TorrentialTridents(id + 0xB0000u, 1.5f);
        DestructiveBolt(id + 0xC0000u, 0.6f);
        ByregotsStrike(id + 0xD0000u, 3.4f);
        ThousandfoldThrust(id + 0xE0000u, 1.2f);
        AsAboveSoBelow(id + 0xF0000u, 5.8f);
        EudaimonEorzea(id + 0x100000u, 6.9f);

        Quintessence(id + 0x110000u, 17.7f);
        Sunbeam(id + 0x120000u, 7.2f);
        Whorl(id + 0x130000u, 10.7f);

        MechanicsInRandomOrder(id + 0x140000u, 241.7f);
        DawnOfTime(id + 0x150000u, 0f);
        Quintessence(id + 0x160000u, 13.6f); // no logs past this here, just guessed
        Sunbeam(id + 0x170000u, 7.2f);
        Whorl(id + 0x180000u, 8.6f);
        MechanicsInRandomOrder(id + 0x190000u, 241.7f);
    }

    private void MechanicsInRandomOrder(uint id, float delay)
    {
        // following mechanics order is either fully random, or has multiple possible forks...
        // Hydrostasis(id + 0x140000, 3.2f);
        // SolarFans(id + 0x150000, 2.5f);
        // ByregotsStrike(id + 0x160000, 2.6f);
        // ThousandfoldThrust(id + 0x170000, 1.2f);
        // DestructiveBolt(id + 0x180000, 2.5f);
        // LovesLight(id + 0x190000, 5.4f);
        Timeout(id + 0x140000u, delay, "Mechanics in random order")
             .ActivateOnEnter<LovesLight>()
             .ActivateOnEnter<SolarFans>()
             .ActivateOnEnter<RadiantRhythm>()
             .ActivateOnEnter<RadiantFlourish>()
             .ActivateOnEnter<Hydrostasis>()
             .ActivateOnEnter<DestructiveBolt>()
             .ActivateOnEnter<Hieroglyphika>()
             .ActivateOnEnter<HandOfTheDestroyer>()
             .ActivateOnEnter<MatronsBreath>()
             .ActivateOnEnter<TorrentialTrident>()
             .ActivateOnEnter<ByregotStrikeJump>()
             .ActivateOnEnter<ByregotStrikeKnockback>()
             .ActivateOnEnter<ByregotStrikeCone>()
             .ActivateOnEnter<ThousandfoldThrust>()
             .ActivateOnEnter<AsAboveSoBelow>()
             .ActivateOnEnter<ClimbingShot>()
             .ActivateOnEnter<SoaringMinuet>();
    }

    private void DawnOfTime(uint id, float delay)
    {
        Cast(id, AID.DawnOfTime, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Quintessence(uint id, float delay)
    {
        CastStartMulti(id, [AID.FirstFormRight, AID.FirstFormLeft, AID.FirstFormDonut], delay)
            .ActivateOnEnter<Quintessence>();
        CastEnd(id + 1u, 7f);
        CastMulti(id + 0x10u, [AID.SecondFormRight, AID.SecondFormLeft, AID.SecondFormDonut], 0.2f, 7f);
        CastMulti(id + 0x20u, [AID.ThirdFormRight, AID.ThirdFormLeft, AID.ThirdFormDonut], 0.2f, 7f);
        Cast(id + 0x30u, AID.Quintessence, 0.2f, 4f);
        ComponentCondition<Quintessence>(id + 0x40u, 0.8f, static comp => comp.NumCasts > 0, "Form 1");
        ComponentCondition<Quintessence>(id + 0x50u, 3.5f, static comp => comp.NumCasts > 1, "Form 2");
        ComponentCondition<Quintessence>(id + 0x60u, 3.6f, static comp => comp.NumCasts > 2, "Form 3")
            .DeactivateOnExit<Quintessence>();
    }

    private void Sunbeam(uint id, float delay)
    {
        Cast(id, AID.Sunbeam, delay, 5f, "Tankbusters")
            .ActivateOnEnter<Sunbeam>()
            .DeactivateOnExit<Sunbeam>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Whorl(uint id, float delay)
    {
        Cast(id, AID.Whorl, delay, 7f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LovesLight(uint id, float delay)
    {
        Cast(id, AID.LovesLight, delay, 4f);
        Cast(id + 0x10u, AID.FullBright, 5.1f, 3);
        ComponentCondition<LovesLight>(id + 0x20u, 0.9f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<LovesLight>();
        ComponentCondition<LovesLight>(id + 0x30u, 10.3f, static comp => comp.NumCasts >= 1, "Line 1");
        ComponentCondition<LovesLight>(id + 0x31u, 2f, static comp => comp.NumCasts >= 2, "Line 2");
        ComponentCondition<LovesLight>(id + 0x32u, 2f, static comp => comp.NumCasts >= 3, "Line 3");
        ComponentCondition<LovesLight>(id + 0x33u, 2f, static comp => comp.NumCasts >= 4, "Line 4")
            .DeactivateOnExit<LovesLight>();
    }

    private void SolarFans(uint id, float delay)
    {
        Cast(id, AID.SolarFans, delay, 3f)
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<RadiantRhythm>()
            .ActivateOnEnter<RadiantFlourish>();
        ComponentCondition<SolarFans>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Charge")
            .DeactivateOnExit<SolarFans>();
        ComponentCondition<RadiantRhythm>(id + 0x10u, 2.8f, static comp => comp.NumCasts > 0);
        ComponentCondition<RadiantRhythm>(id + 0x20u, 2.1f, static comp => comp.NumCasts > 2);
        ComponentCondition<RadiantRhythm>(id + 0x30u, 2.1f, static comp => comp.NumCasts > 4);
        ComponentCondition<RadiantRhythm>(id + 0x40u, 2.1f, static comp => comp.NumCasts > 6)
            .DeactivateOnExit<RadiantRhythm>();
        Cast(id + 0x50u, AID.RadiantFinish, 1.5f, 3f, "Solar fans resolve")
            .DeactivateOnExit<RadiantFlourish>();
    }

    private void Hydrostasis(uint id, float delay)
    {
        Cast(id, AID.Hydrostasis, delay, 4f);
        Cast(id + 0x10u, AID.TimeAndTide, 2.1f, 6f)
            .ActivateOnEnter<Hydrostasis>();
        ComponentCondition<Hydrostasis>(id + 0x20u, 2.9f, static comp => comp.NumCasts > 0, "Knockback 1")
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<Hydrostasis>(id + 0x21u, 3f, static comp => comp.NumCasts > 1, "Knockback 2");
        ComponentCondition<Hydrostasis>(id + 0x22u, 3f, static comp => comp.NumCasts > 2, "Knockback 3")
            .DeactivateOnExit<Hydrostasis>();
    }

    private void DestructiveBolt(uint id, float delay)
    {
        Cast(id, AID.DestructiveBolt, delay, 6f)
            .ActivateOnEnter<DestructiveBolt>();
        ComponentCondition<DestructiveBolt>(id + 2u, 1.1f, static comp => comp.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<DestructiveBolt>();
    }

    private void HieroglyphikaHandOfTheDestroyer(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5f);
        ComponentCondition<Hieroglyphika>(id + 0x10u, 1, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Hieroglyphika>();
        ComponentCondition<Hieroglyphika>(id + 0x20u, 12.8f, static comp => comp.BindsAssigned, "Binds");
        CastStartMulti(id + 0x30u, [AID.HandOfTheDestroyerWrath, AID.HandOfTheDestroyerJudgment], 0.5f);
        ComponentCondition<Hieroglyphika>(id + 0x31u, 2.8f, static comp => comp.NumCasts > 0, "Squares")
            .ActivateOnEnter<HandOfTheDestroyer>()
            .DeactivateOnExit<Hieroglyphika>();
        CastEnd(id + 0x32u, 4.7f);
        ComponentCondition<HandOfTheDestroyer>(id + 0x33, 0.5f, static comp => comp.NumCasts != 0, "Half-arena cleave")
            .DeactivateOnExit<HandOfTheDestroyer>();
    }

    private void MatronsBreath(uint id, float delay)
    {
        Cast(id, AID.MatronsBreath, delay, 3f)
            .ActivateOnEnter<MatronsBreath>();
        ComponentCondition<MatronsBreath>(id + 0x10u, 15.1f, static comp => comp.NumCasts >= 1, "Flower 1");
        ComponentCondition<MatronsBreath>(id + 0x11u, 3.5f, static comp => comp.NumCasts >= 2, "Flower 2");
        ComponentCondition<MatronsBreath>(id + 0x12u, 3.5f, static comp => comp.NumCasts >= 3, "Flower 3");
        ComponentCondition<MatronsBreath>(id + 0x13u, 3.5f, static comp => comp.NumCasts >= 4, "Flower 4")
            .DeactivateOnExit<MatronsBreath>();
    }

    private void TorrentialTridents(uint id, float delay)
    {
        Cast(id, AID.TorrentialTridents, delay, 2f);
        ComponentCondition<TorrentialTrident>(id + 0x10u, 0.9f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TorrentialTrident>();
        ComponentCondition<TorrentialTrident>(id + 0x20u, 5.5f, static comp => comp.AOEs.Count > 5, "Raidwide x6");
        ComponentCondition<TorrentialTrident>(id + 0x30u, 8.1f, static comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<TorrentialTrident>(id + 0x40u, 5f, static comp => comp.NumCasts > 5, "Explosions end")
            .DeactivateOnExit<TorrentialTrident>();
    }

    private void ByregotsStrike(uint id, float delay)
    {
        Cast(id, AID.ByregotStrike, delay, 6f, "Jump")
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .DeactivateOnExit<ByregotStrikeJump>();
        ComponentCondition<ByregotStrikeKnockback>(id + 2u, 0.7f, static comp => comp.NumCasts > 0, "Knockback + cones")
            .DeactivateOnExit<ByregotStrikeKnockback>()
            .DeactivateOnExit<ByregotStrikeCone>();
    }

    private void ThousandfoldThrust(uint id, float delay)
    {
        CastMulti(id, [AID.ThousandfoldThrustR, AID.ThousandfoldThrustL], delay, 5f)
            .ActivateOnEnter<ThousandfoldThrust>();
        ComponentCondition<ThousandfoldThrust>(id + 0x10u, 1.3f, static comp => comp.NumCasts > 0, "Half-room cleave start");
        ComponentCondition<ThousandfoldThrust>(id + 0x20u, 4.3f, static comp => comp.NumCasts > 4, "Half-room cleave resolve")
            .DeactivateOnExit<ThousandfoldThrust>();
    }

    private void AsAboveSoBelow(uint id, float delay)
    {
        CastMulti(id, [AID.AsAboveSoBelowNald, AID.AsAboveSoBelowThal], delay, 5f);
        CastMulti(id + 0x10u, [AID.ClimbingShotNald, AID.ClimbingShotThal], 4.1f, 8f)
            .ActivateOnEnter<AsAboveSoBelow>()
            .ActivateOnEnter<ClimbingShot>();
        ComponentCondition<ClimbingShot>(id + 0x20u, 0.2f, static comp => comp.NumCasts > 0, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<ClimbingShot>();
        ComponentCondition<AsAboveSoBelow>(id + 0x30u, 0.8f, static comp => comp.NumCasts > 0, "Exaflare start");
        Cast(id + 0x40u, AID.SoaringMinuet, 2.3f, 7f, "Wide cleave")
            .ActivateOnEnter<SoaringMinuet>()
            .DeactivateOnExit<SoaringMinuet>()
            .DeactivateOnExit<AsAboveSoBelow>();
    }

    private void EudaimonEorzea(uint id, float delay)
    {
        Cast(id, AID.EudaimonEorzea, delay, 22.2f);
        ComponentCondition<EudaimonEorzea>(id + 0x10u, 2.7f, static comp => comp.NumCasts > 0, "Raidwide x13")
            .ActivateOnEnter<EudaimonEorzea>()
            .DeactivateOnExit<EudaimonEorzea>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
