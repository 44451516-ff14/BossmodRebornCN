namespace BossMod.Endwalker.Savage.P11SThemis;

sealed class P11SThemisStates : StateMachineBuilder
{
    public P11SThemisStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Eunomia(id, 10.2f);
        Dike(id + 0x10000u, 3.2f);
        JuryOverruling(id + 0x20000u, 7.2f);
        UpheldOverruling(id + 0x30000u, 6.2f);
        DivisiveOverruling(id + 0x40000u, 7.3f);
        Styx(id + 0x50000u, 4.3f, 5);
        ArcaneRevelationMirrors(id + 0x60000u, 10.4f);
        ShadowedMessengers(id + 0x70000u, 22.9f);
        Styx(id + 0x80000u, 3.1f, 6);
        Lightstream(id + 0x90000u, 8.2f);
        Eunomia(id + 0xA0000u, 7.2f);
        UpheldOverruling(id + 0xB0000u, 7.2f);
        DarkAndLight(id + 0xC0000u, 14.3f);
        Styx(id + 0xD0000u, 3.3f, 7);
        Dike(id + 0xE0000u, 5.1f);
        DarkCurrent(id + 0xF0000u, 15.4f);
        JuryOverruling(id + 0x100000u, 2.6f);
        UpheldOverruling(id + 0x110000u, 6.2f);
        DivisiveOverruling(id + 0x120000u, 9.3f);
        Eunomia(id + 0x130000u, 8.2f);
        LetterOfTheLaw(id + 0x140000u, 22.4f);
        Styx(id + 0x150000u, 3.6f, 8);
        Lightstream(id + 0x160000u, 7.2f);
        Dike(id + 0x170000u, 9.2f);
        JuryOverruling(id + 0x180000u, 10.1f);
        Eunomia(id + 0x190000u, 6.1f);
        Cast(id + 0x1A0000u, AID.UltimateVerdict, 6.5f, 10f, "Enrage");
    }

    private void Eunomia(uint id, float delay)
    {
        Cast(id, AID.Eunomia, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Dike(uint id, float delay)
    {
        Cast(id, AID.Dike, delay, 7f, "Tankbuster 1")
            .ActivateOnEnter<Dike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<Dike>(id + 2u, 3.1f, static comp => comp.NumCasts > 0, "Tankbuster 2")
            .DeactivateOnExit<Dike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Styx(uint id, float delay, int numCasts)
    {
        Cast(id, AID.Styx, delay, 5f, "Stack hit 1")
            .ActivateOnEnter<Styx>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Styx>(id + 0x10u, 1.1f * numCasts - 1.0f, comp => comp.NumCasts >= numCasts, $"Stack hit {numCasts}")
            .DeactivateOnExit<Styx>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void JuryOverrulingResolve(uint id, float delay)
    {
        CastEnd(id, delay)
            .ActivateOnEnter<JuryOverrulingProtean>();
        ComponentCondition<JuryOverrulingProtean>(id + 0x10u, 1.8f, static comp => comp.NumCasts > 0, "Proteans")
            .ActivateOnEnter<InevitableLawSentence>()
            .DeactivateOnExit<JuryOverrulingProtean>();
        ComponentCondition<InevitableLawSentence>(id + 0x20u, 4.2f, static comp => !comp.Active, "Circles/donuts + Party/pair stacks")
            .ActivateOnEnter<IllusoryGlare>() // casts start 1.1s after proteans
            .ActivateOnEnter<IllusoryGloom>()
            .DeactivateOnExit<IllusoryGlare>() // casts end 0.1s before stacks
            .DeactivateOnExit<IllusoryGloom>()
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void JuryOverruling(uint id, float delay)
    {
        CastStartMulti(id, [AID.JuryOverrulingLight, AID.JuryOverrulingDark], delay);
        JuryOverrulingResolve(id + 0x100u, 6f);
    }

    private void UpheldOverruling(uint id, float delay)
    {
        CastMulti(id, [AID.UpheldOverrulingLight, AID.UpheldOverrulingDark], delay, 7.3f)
            .ActivateOnEnter<UpheldOverruling>();
        ComponentCondition<UpheldOverruling>(id + 0x10u, 0.4f, static comp => !comp.Active, "Stack/away from tank")
            .ActivateOnEnter<InevitableLawSentence>()
            .DeactivateOnExit<UpheldOverruling>();
        ComponentCondition<InevitableLawSentence>(id + 0x20u, 4.2f, static comp => !comp.Active, "In/out + Party/pair stacks")
            .ActivateOnEnter<LightburstBoss>() // cast starts 0.6s after jump
            .ActivateOnEnter<DarkPerimeterBoss>()
            .DeactivateOnExit<LightburstBoss>() // cast ends 0.1s before stacks
            .DeactivateOnExit<DarkPerimeterBoss>()
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DivisiveOverrulingResolve(uint id, float delay)
    {
        ComponentCondition<DivisiveOverruling>(id, delay, static comp => comp.NumCasts > 0, "Narrow line");
        ComponentCondition<DivisiveOverruling>(id + 1u, 2.6f, static comp => comp.NumCasts > 1, "Wide line/sides")
            .DeactivateOnExit<DivisiveOverruling>();
        ComponentCondition<InevitableLawSentence>(id + 0x10u, 0.1f, static comp => !comp.Active, "Party/pair stacks")
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DivisiveOverruling(uint id, float delay)
    {
        CastMulti(id, [AID.DivisiveOverrulingSoloLight, AID.DivisiveOverrulingSoloDark], delay, 6.3f)
            .ActivateOnEnter<InevitableLawSentence>()
            .ActivateOnEnter<DivisiveOverruling>();
        DivisiveOverrulingResolve(id + 0x100u, 1.9f);
    }

    private void ArcaneRevelationMirrors(uint id, float delay)
    {
        CastMulti(id, [AID.ArcaneRevelationMirrorsLight, AID.ArcaneRevelationMirrorsDark], delay, 5)
            .ActivateOnEnter<ArcaneRevelation>(); // PATE happens 1s after cast end, actual cast starts ~3s after PATE
        CastMulti(id + 0x10u, [AID.DismissalOverrulingLight, AID.DismissalOverrulingDark], 2.1f, 5)
            .ActivateOnEnter<InevitableLawSentence>() // TODO: not sure whether this should be activated now (earliest point) or later...
            .ActivateOnEnter<DismissalOverruling>();
        ComponentCondition<DismissalOverruling>(id + 0x20u, 0.5f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<DismissalOverruling>();
        ComponentCondition<ArcaneRevelation>(id + 0x30u, 2f, static comp => comp.NumCasts > 0, "Mirrors")
            .DeactivateOnExit<ArcaneRevelation>();
        ComponentCondition<InevitableLawSentence>(id + 0x40u, 3.0f, static comp => !comp.Active, "In/out + Party/pair stacks")
            .ActivateOnEnter<InnerLight>() // note: these start casting together with dismissal overruling, but we want to start showing hints only after mirrors are done
            .ActivateOnEnter<OuterDark>()
            .DeactivateOnExit<InnerLight>() // casts end 0.1s before stacks
            .DeactivateOnExit<OuterDark>()
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ShadowedMessengers(uint id, float delay)
    {
        Cast(id, AID.ShadowedMessengers, delay, 4f);
        ComponentCondition<DivisiveOverruling>(id + 0x10u, 5.2f, static comp => comp.AOEs.Count > 0) // clones start their casts
            .ActivateOnEnter<DivisiveOverruling>();
        CastStartMulti(id + 0x20u, [AID.DivisiveOverrulingBossLight, AID.DivisiveOverrulingBossDark], 4.0f);
        ComponentCondition<DivisiveOverruling>(id + 0x30u, 5.2f, static comp => comp.NumCasts > 0, "Clone lines 1");
        // +1.6s: upheld ruling casts start
        CastEnd(id + 0x40u, 2.6f)
            .ActivateOnEnter<InevitableLawSentence>(); // note: activated after first lines (cross) are resolved
        ComponentCondition<DivisiveOverruling>(id + 0x50u, 0.6f, static comp => comp.NumCasts > 2, "Clone lines 2");
        ComponentCondition<DivisiveOverruling>(id + 0x60u, 1.3f, static comp => comp.NumCasts > 5, "Narrow line");
        ComponentCondition<DivisiveOverruling>(id + 0x70u, 2.6f, static comp => comp.NumCasts > 6, "Wide line/sides")
            .DeactivateOnExit<DivisiveOverruling>();
        ComponentCondition<InevitableLawSentence>(id + 0x80u, 0.1f, static comp => !comp.Active, "Party/pair stacks")
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<UpheldOverruling>(id + 0x100u, 5.4f, static comp => !comp.Active, "Stack away from tank") // note: casts end ~0.4s before aoes
            .ActivateOnEnter<UpheldOverruling>() // note: actual casts start way earlier, but we want to start showing these hints only after previous stacks resolve
            .DeactivateOnExit<UpheldOverruling>();
        // jury overruling slightly overlaps with upheld aoes
        CastStartMulti(id + 0x110u, [AID.JuryOverrulingLight, AID.JuryOverrulingDark], 3.8f)
            .ActivateOnEnter<LightburstClone>() // cast starts 0.6s after jump
            .ActivateOnEnter<DarkPerimeterClone>();
        ComponentCondition<DarkPerimeterClone>(id + 0x120u, 0.3f, static comp => comp.NumCasts > 0, "Circle + donut")
            .DeactivateOnExit<LightburstClone>()
            .DeactivateOnExit<DarkPerimeterClone>();
        JuryOverrulingResolve(id + 0x200u, 5.7f);
    }

    private void Lightstream(uint id, float delay)
    {
        Cast(id, AID.Lightstream, delay, 4f);
        ComponentCondition<Lightstream>(id + 0x10u, 12.2f, static comp => comp.NumCasts > 0, "Rotating orbs start")
            .ActivateOnEnter<Lightstream>();
        CastStartMulti(id + 0x20u, [AID.DivisiveOverrulingSoloLight, AID.DivisiveOverrulingSoloDark], 0.6f); // TODO: second time it is 0.1 instead
        ComponentCondition<Lightstream>(id + 0x30u, 5.8f, static comp => comp.NumCasts >= 21, maxOverdue: 2f) // TODO: second time it is 6.3 instead
            .ActivateOnEnter<InevitableLawSentence>()
            .ActivateOnEnter<DivisiveOverruling>()
            .DeactivateOnExit<Lightstream>();
        CastEnd(id + 0x40u, 0.5f); // TODO: second time it typically happens just before lightstream end instead
        DivisiveOverrulingResolve(id + 0x100u, 1.9f);
    }

    private void DarkAndLight(uint id, float delay)
    {
        Cast(id, AID.DarkAndLight, delay, 4f)
            .ActivateOnEnter<DarkAndLight>();
        CastMulti(id + 0x1000u, [AID.ArcaneRevelationSpheresLight, AID.ArcaneRevelationSpheresDark], 8.2f, 5f)
            .ActivateOnEnter<ArcaneRevelation>(); // PATE happens 1s after cast end, actual cast starts ~3s after PATE
        ComponentCondition<ArcaneRevelation>(id + 0x1010, 9.7f, static comp => comp.NumCasts > 0, "Spheres")
            .ExecOnEnter<DarkAndLight>(static comp => comp.ShowSafespots = false) // TODO: reconsider?
            .DeactivateOnExit<ArcaneRevelation>();
        JuryOverruling(id + 0x2000u, 0.5f);
        DivisiveOverruling(id + 0x3000u, 6.2f);
        Cast(id + 0x4000u, AID.EmissarysWill, 3.2f, 4, "Tethers resolve")
            .DeactivateOnExit<DarkAndLight>();
    }

    private void DarkCurrent(uint id, float delay)
    {
        Cast(id, AID.DarkCurrent, delay, 4f)
            .ActivateOnEnter<DarkCurrent>();
        CastStart(id + 0x10u, AID.BlindingLight, 5.2f);
        ComponentCondition<DarkCurrent>(id + 0x11u, 2.9f, static comp => comp.NumCasts > 0, "Rotating aoe start")
            .ActivateOnEnter<BlindingLight>();
        CastEnd(id + 0x12u, 2.1f, "Spreads")
            .DeactivateOnExit<BlindingLight>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DarkCurrent>(id + 0x20u, 5.5f, static comp => comp.NumCasts >= 24, "Rotating aoe end")
            .DeactivateOnExit<DarkCurrent>();
    }

    private void LetterOfTheLaw(uint id, float delay)
    {
        Cast(id, AID.LetterOfTheLaw, delay, 4f);
        CastMulti(id + 0x10u, [AID.TwofoldRevelationLight, AID.TwofoldRevelationDark], 2.1f, 5f)
            .ActivateOnEnter<ArcaneRevelation>() // PATE happens 1s after cast end, actual cast starts ~3s after PATE
            .ActivateOnEnter<UpheldOverruling>(); // ruling casts start ~1.1s after this cast end
        Cast(id + 0x20u, AID.HeartOfJudgment, 6.2f, 3f);
        ComponentCondition<ArcaneRevelation>(id + 0x30u, 0.5f, static comp => comp.NumCasts > 0, "Mirrors + spheres")
            .DeactivateOnExit<ArcaneRevelation>();
        ComponentCondition<UpheldOverruling>(id + 0x40u, 2.5f, static comp => !comp.Active, "Stack away from tank")
            .DeactivateOnExit<UpheldOverruling>();
        // +3.2s: divisive casts start
        ComponentCondition<DarkPerimeterClone>(id + 0x50u, 4.1f, static comp => comp.NumCasts > 0, "Circle + donut")
            .ActivateOnEnter<LightburstClone>() // cast starts 0.6s after jump
            .ActivateOnEnter<DarkPerimeterClone>()
            .DeactivateOnExit<LightburstClone>()
            .DeactivateOnExit<DarkPerimeterClone>();
        ComponentCondition<HeartOfJudgment>(id + 0x60u, 4.9f, static comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<HeartOfJudgment>() // note: activated now, since towers should be resolved after circle/donut
            .DeactivateOnExit<HeartOfJudgment>();
        ComponentCondition<DivisiveOverruling>(id + 0x70u, 3.4f, static comp => comp.NumCasts > 0, "Clone lines 1")
            .ActivateOnEnter<DivisiveOverruling>();
        CastStartMulti(id + 0x80u, [AID.DismissalOverrulingLight, AID.DismissalOverrulingDark], 0.7f);
        ComponentCondition<DivisiveOverruling>(id + 0x90u, 2.5f, static comp => comp.NumCasts > 2, "Clone lines 2")
            .DeactivateOnExit<DivisiveOverruling>();
        CastEnd(id + 0xA0u, 2.5f)
            .ActivateOnEnter<DismissalOverruling>() // TODO: is this the best place to activate?
            .ActivateOnEnter<InevitableLawSentence>(); // TODO: is this the best place to activate?
        ComponentCondition<DismissalOverruling>(id + 0xB0u, 0.5f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<DismissalOverruling>();
        ComponentCondition<InevitableLawSentence>(id + 0xC0u, 5.1f, static comp => !comp.Active, "In/out + Party/pair stacks")
            .ActivateOnEnter<InnerLight>() // note: these start casting together with dismissal overruling, but we want to start showing hints only after knockbacks are done (?)
            .ActivateOnEnter<OuterDark>()
            .DeactivateOnExit<InnerLight>() // casts end 0.1s before stacks
            .DeactivateOnExit<OuterDark>()
            .DeactivateOnExit<InevitableLawSentence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
