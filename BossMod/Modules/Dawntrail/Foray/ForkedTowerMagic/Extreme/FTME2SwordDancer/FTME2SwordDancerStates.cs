namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

sealed class FTME2SwordDancerStates : StateMachineBuilder
{
    public FTME2SwordDancerStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SwordStorm(id, 5.3f);
        ThrowingMystique(id + 0x10000u, 5.3f);
        SwordStorm(id + 0x20000u, 6.7f);
        Cycloswords1(id + 0x30000u, 11.4f);
        SwordDance(id + 0x40000u, 2.1f);
        SwordStorm(id + 0x50000u, 4f);
        LeapingLift(id + 0x60000u, 11.3f);
        Cycloswords2(id + 0x70000u, 7.6f);
        SwordDance(id + 0x80000u, 2.2f);
        SwordStorm(id + 0x90000u, 4f);
        ThrowingMystique2(id + 0xA0000u, 9.2f);
        SwordStorm(id + 0xB0000u, 6.5f);
        Cycloswords3(id + 0xC0000u, 9.2f);
        SwordDance(id + 0xD0000u, 0.2f);
        SwordStorm(id + 0xE0000u, 3.9f);
        Enrage(id + 0xF0000u, 9.3f);
    }

    private void SwordStorm(uint id, float delay)
    {
        Cast(id, AID.SwordStorm, delay, 5f, "Raidwide")
            .ActivateOnEnter<SwordStorm>()
            .DeactivateOnExit<SwordStorm>();
    }

    private void ThrowingMystique(uint id, float delay)
    {
        Cast(id, AID.ThrowingSwordsCast, delay, 3f, "")
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnOuter>();
        ComponentCondition<Rush>(id + 0x1000u, 4f, static comp => comp.NumCasts >= 4, "Throwing Swords");
        ComponentCondition<TurnInner>(id + 0x2000u, 13.2f, static comp => comp.NumCasts >= 3, "Rush + Turn")
            .ActivateOnEnter<RushLong>()
            .ActivateOnEnter<MartialMystique>();
        ComponentCondition<MartialMystique>(id + 0x3000u, 23.3f, static comp => comp.NumCasts >= 3, "Rush + Cleaves")
            .DeactivateOnExit<Rush>()
            .DeactivateOnExit<TurnInner>()
            .DeactivateOnExit<TurnOuter>()
            .DeactivateOnExit<RushLong>()
            .DeactivateOnExit<MartialMystique>()
            .ActivateOnExit<Cyclosword>()
            .ExecOnExit<Cyclosword>(static comp => comp.MaxCasts = 1);
    }

    private void Cycloswords1(uint id, float delay)
    {
        Cast(id, AID.CycloswordsUnsheathed, delay, 3f, "Cyclosword 1 start")
            .ActivateOnEnter<RushLong>();
        ComponentCondition<RushLong>(id + 0x1000u, 17.2f, static comp => comp.NumCasts >= 8, "Circle/Donut + Rush")
            .DeactivateOnExit<Cyclosword>()
            .DeactivateOnExit<RushLong>();
    }

    private void SwordDance(uint id, float delay)
    {
        Cast(id, AID.SwordDanceCast, delay, 4.3f, "Sword Dance start")
            .ActivateOnEnter<SwordDance>();
        ComponentCondition<SwordDance>(id + 0x1000u, 14.1f, static comp => comp.NumCasts >= 4, "Sword Dance")
            .DeactivateOnExit<SwordDance>();
    }

    private void LeapingLift(uint id, float delay)
    {
        Cast(id, AID.LeapingLiftCast, delay, 3f, "Leaping Lift start")
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsforge>()
            .ActivateOnEnter<Steelsbreath>()
            .ActivateOnEnter<RushLong>();
        ComponentCondition<RushLong>(id + 0x2000u, 28.4f, static comp => comp.ActiveCasters.Length != 0, "KB x5 + AOE x2");
        ComponentCondition<RushLong>(id + 0x3000u, 5.9f, static comp => comp.NumCasts >= 8, "KB into Rush")
            .DeactivateOnExit<RushLong>()
            .DeactivateOnExit<Steelsbreath>()
            .DeactivateOnExit<Steelsforge>()
            .DeactivateOnExit<Pierce>()
            .ActivateOnExit<Cyclosword>()
            .ExecOnExit<Cyclosword>(static comp => comp.MaxCasts = 3);
    }

    private void Cycloswords2(uint id, float delay)
    {
        Cast(id, AID.CycloswordsUnsheathed, delay, 3f, "Cyclosword 2 start");
        ComponentCondition<Cyclosword>(id + 0x1000u, 21.1f, static comp => comp.NumCasts >= 6, "Circle/Donut x3")
            .DeactivateOnExit<Cyclosword>();
    }

    private void ThrowingMystique2(uint id, float delay)
    {
        Cast(id, AID.ThrowingSwordsCast, delay, 3f, "")
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<TurnMiddle>();
        ComponentCondition<Rush>(id + 0x1000u, 4f, static comp => comp.NumCasts >= 4, "Throwing Swords");
        ComponentCondition<TurnMiddle>(id + 0x2000u, 13.2f, static comp => comp.NumCasts >= 6, "Rush + Turn")
            .ActivateOnEnter<RushLong>()
            .ActivateOnEnter<MartialMystique>();
        ComponentCondition<MartialMystique>(id + 0x3000u, 23.5f, static comp => comp.NumCasts >= 3, "Rush + Cleaves")
            .DeactivateOnExit<Rush>()
            .DeactivateOnExit<TurnMiddle>()
            .DeactivateOnExit<RushLong>()
            .DeactivateOnExit<MartialMystique>()
            .ActivateOnExit<Cyclosword>()
            .ExecOnExit<Cyclosword>(static comp => comp.MaxCasts = 2);
    }

    private void Cycloswords3(uint id, float delay)
    {
        Cast(id, AID.CycloswordsUnsheathed, delay, 3f, "Cyclosword 3 start");
        ComponentCondition<Cyclosword>(id + 0x1000u, 29.2f, static comp => comp.NumCasts >= 6, "Circle/Donut x3 in order")
            .DeactivateOnExit<Cyclosword>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.SwordDanseMacabre, delay, 12f, "Enrage");
    }
}
