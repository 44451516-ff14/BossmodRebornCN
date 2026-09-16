namespace BossMod.Endwalker.Alliance.A31Thaliak;

sealed class A31ThaliakStates : StateMachineBuilder
{
    public A31ThaliakStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Katarraktes(id, 5.2f);
        Rheognosis(id + 0x10000u, 7.8f);
        Thlipsis(id + 0x20000u, 3.9f);
        LeftRightBank(id + 0x30000u, 8.5f);
        LeftRightBank(id + 0x40000u, 2.1f);
        Hydroptosis(id + 0x50000u, 2.1f);
        Rhyton(id + 0x60000u, 6.2f);
        Tetraktys1(id + 0x70000u, 8.7f);
        RheognosisPetrine(id + 0x80000u, 8.4f);
        Hieroglyphica(id + 0x90000u, 6.5f);
        HieroglyphicaLeftRightBank(id + 0xA0000u, 4.4f);
        Tetraktys2(id + 0xB0000u, 8.4f);
        Rhyton(id + 0xC0000u, 6.1f);
        RheognosisPetrine(id + 0xD0000u, 7.6f);
        Thlipsis(id + 0xE0000u, 2.1f);
        HieroglyphicaLeftRightBank(id + 0xF0000u, 6.6f);
        Hydroptosis(id + 0x100000u, 2.1f);
        Katarraktes(id + 0x110000u, 5.2f);
        Tetraktys2(id + 0x120000u, 7.7f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Katarraktes(uint id, float delay)
    {
        Cast(id, AID.Katarraktes, delay, 5f);
        ComponentCondition<Katarraktes>(id + 2u, 0.7f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Katarraktes>()
            .DeactivateOnExit<Katarraktes>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Rheognosis(uint id, float delay)
    {
        Cast(id, AID.Rheognosis, delay, 5f)
            .ActivateOnEnter<RheognosisKnockback>();
        ComponentCondition<RheognosisKnockback>(id + 0x10u, 20.3f, static comp => comp.NumCasts > 0, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<RheognosisKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RheognosisPetrine(uint id, float delay)
    {
        Cast(id, AID.RheognosisPetrine, delay, 5f)
            .ActivateOnEnter<RheognosisKnockback>();
        ComponentCondition<RheognosisKnockback>(id + 0x10u, 20.3f, static comp => comp.NumCasts > 0, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<RheognosisCrash>()
            .DeactivateOnExit<RheognosisKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RheognosisCrash>(id + 0x20u, 1.8f, static comp => comp.NumCasts >= 5, "Half-arena cleave")
            .DeactivateOnExit<RheognosisCrash>();
    }

    private void Thlipsis(uint id, float delay)
    {
        Cast(id, AID.Thlipsis, delay, 4f)
            .ActivateOnEnter<Thlipsis>();
        ComponentCondition<Thlipsis>(id + 2u, 2f, static comp => comp.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<Thlipsis>();
    }

    private void LeftRightBank(uint id, float delay)
    {
        CastMulti(id, [AID.LeftBank, AID.RightBank], delay, 5f, "Half-arena cleave")
            .ActivateOnEnter<Bank>()
            .DeactivateOnExit<Bank>();
    }

    private void Hydroptosis(uint id, float delay)
    {
        Cast(id, AID.Hydroptosis, delay, 4f)
            .ActivateOnEnter<Hydroptosis>();
        ComponentCondition<Hydroptosis>(id + 2u, 1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Hydroptosis>();
    }

    private void Rhyton(uint id, float delay)
    {
        CastStart(id, AID.Rhyton, delay)
            .ActivateOnEnter<Rhyton>();
        CastEnd(id + 1u, 5f);
        ComponentCondition<Rhyton>(id + 2u, 0.9f, static comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<Rhyton>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Tetraktys1(uint id, float delay)
    {
        CastStart(id, AID.Tetraktys, delay)
            .ActivateOnEnter<TetraktysBorder>(); // telegraph appears ~0.1s before cast start
        CastEnd(id + 1u, 6f);
        ComponentCondition<TetraktysBorder>(id + 2u, 0.4f, static comp => comp.Active, "Triangles start");
        ComponentCondition<Tetraktys>(id + 0x10u, 3.6f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Tetraktys>();
        ComponentCondition<Tetraktys>(id + 0x11u, 3.9f, static comp => comp.NumCasts >= 3, "Small tri 1");
        ComponentCondition<Tetraktys>(id + 0x20u, 2.5f, static comp => comp.NumCasts >= 6, "Small tri 2");
        ComponentCondition<Tetraktys>(id + 0x30u, 2.5f, static comp => comp.NumCasts >= 9, "Small tri 3");
        ComponentCondition<Tetraktys>(id + 0x40u, 2.5f, static comp => comp.NumCasts >= 10, "Large tri 1");
        ComponentCondition<Tetraktys>(id + 0x50u, 2.5f, static comp => comp.NumCasts >= 11, "Large tri 2");
        ComponentCondition<Tetraktys>(id + 0x60u, 2.5f, static comp => comp.NumCasts >= 12, "Large tri 3")
            .DeactivateOnExit<Tetraktys>();

        Cast(id + 0x100, AID.TetraktuosKosmos, 1.7f, 4f);
        ComponentCondition<TetraktuosKosmos>(id + 0x110u, 0.8f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TetraktuosKosmosCounter>()
            .ActivateOnEnter<TetraktuosKosmos>();
        CastStart(id + 0x120u, AID.TetraktuosKosmos, 6.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x121u, 1.7f, static comp => comp.NumCasts >= 1, "Splitting tri 1");
        CastEnd(id + 0x122u, 2.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x130u, 0.8f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<TetraktuosKosmos>(id + 0x140u, 8f, static comp => comp.NumCasts >= 3, "Splitting tri 2+3")
            .DeactivateOnExit<TetraktuosKosmos>();
        ComponentCondition<TetraktysBorder>(id + 0x200u, 4.2f, static comp => !comp.Active, "Triangles resolve")
            .DeactivateOnExit<TetraktysBorder>();
    }

    private void Tetraktys2(uint id, float delay)
    {
        CastStart(id, AID.Tetraktys, delay)
            .ActivateOnEnter<TetraktysBorder>(); // telegraph appears ~0.1s before cast start
        CastEnd(id + 1u, 6f);
        ComponentCondition<TetraktysBorder>(id + 2u, 0.4f, static comp => comp.Active, "Triangles start");
        ComponentCondition<Tetraktys>(id + 0x10u, 3.6f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Tetraktys>();
        ComponentCondition<Tetraktys>(id + 0x11u, 3.9f, static comp => comp.NumCasts >= 3, "Small tri 1");
        ComponentCondition<Tetraktys>(id + 0x20u, 2.5f, static comp => comp.NumCasts >= 6, "Small tri 2");
        ComponentCondition<Tetraktys>(id + 0x30u, 2.5f, static comp => comp.NumCasts >= 9, "Small tri 3");
        ComponentCondition<Tetraktys>(id + 0x40u, 2.5f, static comp => comp.NumCasts >= 10, "Large tri 1");
        ComponentCondition<Tetraktys>(id + 0x50u, 2.5f, static comp => comp.NumCasts >= 11, "Large tri 2");
        CastStart(id + 0x60u, AID.TetraktuosKosmos, 2.2f);
        ComponentCondition<Tetraktys>(id + 0x61u, 0.3f, static comp => comp.NumCasts >= 12, "Large tri 3")
            .DeactivateOnExit<Tetraktys>();

        CastEnd(id + 0x100u, 3.7f);
        ComponentCondition<TetraktuosKosmos>(id + 0x110u, 0.8f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TetraktuosKosmos>();
        CastStart(id + 0x120u, AID.TetraktuosKosmos, 6.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x121u, 1.7f, static comp => comp.NumCasts >= 2, "Splitting tri 1+2");
        CastEnd(id + 0x122u, 2.3f);
        ComponentCondition<TetraktuosKosmos>(id + 0x130u, 0.8f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<TetraktuosKosmos>(id + 0x140u, 8f, static comp => comp.NumCasts >= 4, "Splitting tri 3+4")
            .DeactivateOnExit<TetraktuosKosmos>();

        ComponentCondition<TetraktysBorder>(id + 0x200u, 3.2f, static comp => !comp.Active, "Triangles resolve")
            .DeactivateOnExit<TetraktysBorder>();
    }

    private void Hieroglyphica(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5f);
        ComponentCondition<Hieroglyphika>(id + 0x10u, 0.9f, static comp => comp.SafeSideDir != default)
            .ActivateOnEnter<Hieroglyphika>();
        ComponentCondition<Hieroglyphika>(id + 0x11u, 2f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<Hieroglyphika>(id + 0x20u, 13f, static comp => comp.BindsAssigned, "Binds");
        ComponentCondition<Hieroglyphika>(id + 0x30u, 4.1f, static comp => comp.NumCasts > 0, "Squares")
            .DeactivateOnExit<Hieroglyphika>();
    }

    private void HieroglyphicaLeftRightBank(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5f)
            .ActivateOnEnter<Bank>();
        ComponentCondition<Hieroglyphika>(id + 0x10u, 0.9f, static comp => comp.SafeSideDir != default)
            .ActivateOnEnter<Hieroglyphika>();
        CastStartMulti(id + 0x11u, [AID.HieroglyphikaLeftBank, AID.HieroglyphikaRightBank], 1.2f);
        ComponentCondition<Hieroglyphika>(id + 0x12u, 0.8f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<Hieroglyphika>(id + 0x20u, 13, static comp => comp.BindsAssigned, "Binds");
        ComponentCondition<Hieroglyphika>(id + 0x30u, 4.1f, static comp => comp.NumCasts > 0, "Squares")
            .DeactivateOnExit<Hieroglyphika>();
        CastEnd(id + 0x40u, 4.1f, "Half-arena cleave")
            .DeactivateOnExit<Bank>();
    }
}
