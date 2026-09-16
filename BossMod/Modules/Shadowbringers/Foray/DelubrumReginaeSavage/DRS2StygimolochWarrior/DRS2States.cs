namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

sealed class DRS2StygimolochWarriorStates : StateMachineBuilder
{
    public DRS2StygimolochWarriorStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<Devour>(); // TODO: reconsider...
    }

    private void SinglePhase(uint id)
    {
        SurgeOfVigor(id, 8.2f);
        UnrelentingCharge(id + 0x10000u, 9.2f);
        Entrapment(id + 0x20000u, 9.0f);
        ViciousSwipeCrazedRampage(id + 0x30000u, 1.1f);
        FocusedTremorForcefulStrike(id + 0x40000u, 8.2f);
        InescapableEntrapment1(id + 0x50000u, 11.5f);
        SurgeOfVigor(id + 0x60000u, 9.4f); // 12.6 if previous was withering curse
        FocusedTremorFlailingStrike(id + 0x70000u, 11.2f);
        InescapableEntrapment2(id + 0x80000u, 9.8f);
        FocusedTremorCoerceForcefulStrike(id + 0x90000u, 9.7f);
        SurgeOfVigor(id + 0xA0000u, 10.2f);
        UnrelentingCharge(id + 0xB0000u, 11.2f);
        ViciousSwipeCrazedRampage(id + 0xC0000u, 1.6f);
        SunsIre(id + 0xD0000u, 9.5f);
    }

    private void SurgeOfVigor(uint id, float delay)
    {
        // note: it could be preceeded by optional devour cast, ignore it
        Condition(id, delay, () => (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.SurgeOfVigor, maxOverdue: 100f)
            .SetHint(StateMachine.StateHint.BossCastStart);
        CastEnd(id + 1u, 3f, "Damage up");
    }

    private void UnrelentingCharge(uint id, float delay)
    {
        Cast(id, AID.UnrelentingCharge, delay, 3f)
            .ActivateOnEnter<UnrelentingCharge>();
        ComponentCondition<UnrelentingCharge>(id + 0x10u, 0.3f, static comp => comp.NumCasts >= 1, "Knockback 1");
        ComponentCondition<UnrelentingCharge>(id + 0x11u, 1.6f, static comp => comp.NumCasts >= 2, "Knockback 2");
        ComponentCondition<UnrelentingCharge>(id + 0x12u, 1.6f, static comp => comp.NumCasts >= 3, "Knockback 3")
            .DeactivateOnExit<UnrelentingCharge>();
    }

    private void Entrapment(uint id, float delay)
    {
        Cast(id, AID.Entrapment, delay, 3f)
            .ActivateOnEnter<EntrapmentAttract>();
        ComponentCondition<EntrapmentAttract>(id + 0x10u, 0.8f, static comp => comp.NumCasts > 0, "Traps")
            .DeactivateOnExit<EntrapmentAttract>();

        Cast(id + 0x20u, AID.LethalBlow, 1.3f, 20f, "Traps deadline")
            .ActivateOnEnter<LethalBlow>()
            .ActivateOnEnter<EntrapmentNormal>()
            .DeactivateOnExit<LethalBlow>();
        ComponentCondition<Entrapment>(id + 0x30u, 1.0f, static comp => comp.NumCasts >= 16, "Traps resolve")
            .DeactivateOnExit<Entrapment>();
    }

    private void InescapableEntrapment1(uint id, float delay)
    {
        Cast(id, AID.InescapableEntrapment, delay, 3f, "Traps")
            .ActivateOnEnter<EntrapmentInescapable>();
        CastMulti(id + 0x10u, [AID.SurgingFlames, AID.WitheringCurse], 8.2f, 13f, "Traps resolve (ice/mini)") // note: different start delay depending on cast
            .DeactivateOnExit<EntrapmentInescapable>();
        // note: withering curse is followed by devour here
    }

    private void InescapableEntrapment2(uint id, float delay)
    {
        Cast(id, AID.InescapableEntrapment, delay, 3f, "Traps")
            .ActivateOnEnter<EntrapmentInescapable>();
        CastMulti(id + 0x10u, [AID.SurgingFlames, AID.WitheringCurse], 3.2f, 13f, "Traps resolve (ice/mini)"); // note: different start delay depending on cast
        // note: withering curse is followed by devour here

        Condition(id + 0x100u, 6.6f, () => (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.SurgingFlood, maxOverdue: 100f) // 4.2 if previous was surging flames
            .SetHint(StateMachine.StateHint.BossCastStart);
        CastEnd(id + 0x101u, 10f, "Traps resolve (toad)");
        Cast(id + 0x110, AID.LeapingSpark, 0.3f, 8, "Remove toad")
            .DeactivateOnExit<EntrapmentInescapable>();
        ComponentCondition<LeapingSpark>(id + 0x120, 0.5f, static comp => comp.NumCasts >= 1)
            .ActivateOnEnter<LeapingSpark>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<LeapingSpark>(id + 0x121, 1.1f, static comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<LeapingSpark>(id + 0x122, 1.1f, static comp => comp.NumCasts >= 3, "Raidwide x3")
            .DeactivateOnExit<LeapingSpark>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ViciousSwipeCrazedRampage(uint id, float delay)
    {
        Cast(id, AID.ViciousSwipe, delay, 4f, "Out")
            .ActivateOnEnter<ViciousSwipe>()
            .ActivateOnEnter<CrazedRampage>() // starts at the same time
            .DeactivateOnExit<ViciousSwipe>();
        ComponentCondition<CrazedRampage>(id + 0x10u, 2f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<CrazedRampage>();
    }

    private State FocusedTremorForcefulStrike(uint id, float delay)
    {
        Cast(id, AID.FocusedTremor, delay, 3f);
        ComponentCondition<FocusedTremorLarge>(id + 0x10u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorLarge>();

        CastStart(id + 0x20u, AID.ForcefulStrike, 2.7f);
        ComponentCondition<FocusedTremorLarge>(id + 0x30u, 5.2f, static comp => comp.NumCasts >= 1, "Tile 1");
        ComponentCondition<FocusedTremorLarge>(id + 0x31u, 2.0f, static comp => comp.NumCasts >= 2, "Tile 2");
        ComponentCondition<FocusedTremorLarge>(id + 0x32u, 2.0f, static comp => comp.NumCasts >= 3, "Tile 3");
        ComponentCondition<FocusedTremorLarge>(id + 0x33u, 2.0f, static comp => comp.NumCasts >= 4, "Tile 4")
            .ActivateOnEnter<ForcefulStrike>()
            .DeactivateOnExit<FocusedTremorLarge>();
        return CastEnd(id + 0x40u, 3.8f, "Cleave")
            .DeactivateOnExit<ForcefulStrike>();
    }

    private void FocusedTremorFlailingStrike(uint id, float delay)
    {
        Cast(id, AID.FocusedTremor, delay, 3f);
        ComponentCondition<FocusedTremorSmall>(id + 0x10u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorSmall>();

        ComponentCondition<FlailingStrikeBait>(id + 0x20u, 4.7f, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<FlailingStrikeBait>();
        CastStart(id + 0x30u, AID.FlailingStrikeFirst, 6.1f, "Cone bait")
            .DeactivateOnExit<FlailingStrikeBait>();
        CastEnd(id + 0x31u, 3f, "Cone 1")
            .ActivateOnEnter<FlailingStrike>()
            .ExecOnEnter<FocusedTremorSmall>(static comp => comp.Activate());
        ComponentCondition<FocusedTremorSmall>(id + 0x40u, 1.2f, static comp => comp.NumCasts >= 1, "Tile 1");

        ComponentCondition<FlailingStrike>(id + 0x100u, 7.5f, static comp => comp.NumCasts >= 6, "Cone 6")
            .DeactivateOnExit<FlailingStrike>();
        ViciousSwipeCrazedRampage(id + 0x200u, 0.8f);
        ComponentCondition<FocusedTremorSmall>(id + 0x300u, 0.7f, static comp => comp.NumCasts >= 16, "Tiles resolve")
            .DeactivateOnExit<FocusedTremorSmall>();
    }

    private void FocusedTremorCoerceForcefulStrike(uint id, float delay)
    {
        Cast(id, AID.Coerce, delay, 3f)
            .ActivateOnEnter<Coerce>(); // face debuff appears ~0.7s after cast end
        SurgeOfVigor(id + 0x10u, 3.2f);

        Cast(id + 0x100u, AID.FocusedTremor, 9.2f, 3f);
        ComponentCondition<FocusedTremorLarge>(id + 0x110u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorLarge>();

        CastStart(id + 0x120u, AID.ForcefulStrike, 2.7f);
        ComponentCondition<FocusedTremorLarge>(id + 0x130u, 5.2f, static comp => comp.NumCasts >= 1, "Tile 1");
        ComponentCondition<Coerce>(id + 0x131u, 1.6f, static comp => comp.NumActiveForcedMarches > 0, "Forced march");
        ComponentCondition<FocusedTremorLarge>(id + 0x132u, 0.4f, static comp => comp.NumCasts >= 2, "Tile 2");
        ComponentCondition<FocusedTremorLarge>(id + 0x133u, 2.0f, static comp => comp.NumCasts >= 3, "Tile 3");
        ComponentCondition<FocusedTremorLarge>(id + 0x134u, 2.0f, static comp => comp.NumCasts >= 4, "Tile 4")
            .ActivateOnEnter<ForcefulStrike>()
            .DeactivateOnExit<FocusedTremorLarge>()
            .DeactivateOnExit<Coerce>();
        CastEnd(id + 0x140u, 3.8f, "Cleave")
            .DeactivateOnExit<ForcefulStrike>();
    }

    private void SunsIre(uint id, float delay)
    {
        Cast(id, AID.SunsIre, delay, 12f);
        SimpleState(id + 0x10u, 0.7f, "Enrage");
    }
}
