namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class Ex5NecronStates : StateMachineBuilder
{
    private Wipe? wipe;
    private Intermission? intermission;
    private Prisons? doom;

    public Ex5NecronStates(Ex5Necron module) : base(module)
    {
        bool IsWipedOrLeftRaid()
        {
            wipe ??= module.FindComponent<Wipe>();
            return (wipe?.Wiped ?? false) || module.WorldState.CurrentCFCID != 1062u;
        }
        bool IntermissionStarted()
        {
            intermission ??= module.FindComponent<Intermission>();
            return intermission?.Started ?? false;
        }
        bool TrackDoom()
        {
            doom ??= module.FindComponent<Prisons>();
            return doom?.NumDooms > 0 && module.PrimaryActor.IsTargetable;
        }
        SimplePhase(default, Phase1, "P1")
            .ActivateOnEnter<Wipe>()
            .ActivateOnEnter<Intermission>()
            .Raw.Update = () => module.PrimaryActor.IsDead || IsWipedOrLeftRaid() || IntermissionStarted();
        SimplePhase(1u, Intermission, "P2")
            .Raw.Update = () => module.PrimaryActor.IsDead || TrackDoom() || IsWipedOrLeftRaid();
        SimplePhase(2u, Phase3, "P3")
            .Raw.Update = () => module.PrimaryActor.IsDead || IsWipedOrLeftRaid();
    }

    private void Phase1(uint id)
    {
        BlueShockwave(id, 7.2f);
        FearOfDeath1(id + 0x10000u, 5f);
        ColdGripExistentialDread(id + 0x20000u, 5.2f);
        MementoMori(id + 0x30000u, 10.6f);
        SoulReapingTwoFourfoldBlight1(id + 0x40000u, 15.6f);
        FearOfDeath2(id + 0x50000u, 13.8f);
        SoulReapingTwoFourfoldBlight2(id + 0x60000u, 7.1f);
        GrandCross(id + 0x70000u, 14.8f);
        SimpleState(id + 0x80000u, 9.6f, "Intermission");
    }

    private void Intermission(uint id)
    {
        DarknessOfEternity(id, 50.4f);
        Prisons(id + 0x10000u, 13.5f);
    }

    private void Phase3(uint id)
    {
        SpecterOfDeathEndsEmbraceChokingGraspColdGrip(id, 17.9f);
        RelentlessReapingSecondFourthSeason(id + 0x10000u, 40.1f);
        CircleOfLivesSoulReaping(id + 0x20000u, 20.9f);
        MassMacabre(id + 0x30000u, 12.9f);
        RelentlessReapingSecondFourthSeason(id + 0x40000u, 39.2f);
        FearOfDeath1(id + 0x50000u, 1.7f);
        CircleOfLivesInvitation(id + 0x60000u, 15.3f);
        MementoMori(id + 0x70000u, 7.3f);
        ColdGripExistentialDread(id + 0x80000u, 4.7f);
        GrandCross(id + 0x90000u, 11.6f);
        Targetable(id + 0xA0000u, false, 19.7f, "Enrage");
    }

    private void BlueShockwave(uint id, float delay)
    {
        ComponentCondition<BlueShockwave>(id, delay, comp => comp.CurrentBaits.Count != 0, "Tank swap")
            .ActivateOnEnter<BlueShockwave>();
        ComponentCondition<BlueShockwave>(id + 0x10u, 7.2f, comp => comp.NumCasts != 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<BlueShockwave>(id + 0x20u, 4f, comp => comp.NumCasts == 2, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<BlueShockwave>();
    }

    private void FearOfDeath1(uint id, float delay)
    {
        Cast(id, (uint)AID.FearOfDeath, delay, 5f, "Raidwide")
            .ActivateOnEnter<FearOfDeath>()
            .ActivateOnEnter<FearOfDeathAOE>()
            .ActivateOnEnter<ChokingGraspBait1>()
            .DeactivateOnExit<FearOfDeath>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FearOfDeathAOE>(id + 0x10u, 3.2f, comp => comp.NumCasts != 0, "Circle AOEs")
            .DeactivateOnExit<FearOfDeathAOE>();
        ComponentCondition<ChokingGraspBait1>(id + 0x20u, 2.7f, comp => comp.NumCasts != 0, "Baited line AOEs")
            .DeactivateOnExit<ChokingGraspBait1>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FearOfDeath2(uint id, float delay)
    {
        ComponentCondition<FearOfDeath>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<FearOfDeath>()
            .ActivateOnEnter<FearOfDeathAOE>()
            .ActivateOnEnter<ChokingGraspBait1>()
            .DeactivateOnExit<FearOfDeath>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FearOfDeathAOE>(id + 0x10u, 3.2f, comp => comp.NumCasts != 0, "Circle AOEs")
            .DeactivateOnExit<FearOfDeathAOE>();
        ComponentCondition<ChokingGraspBait1>(id + 0x20u, 2.7f, comp => comp.NumCasts != 0, "Baited line AOEs")
            .DeactivateOnExit<ChokingGraspBait1>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<TheEndsEmbrace>(id + 0x30u, 4.3f, comp => comp.NumFinishedSpreads != 0, "Spreads resolve")
            .ActivateOnEnter<TheEndsEmbrace>()
            .DeactivateOnExit<TheEndsEmbrace>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ChokingGraspBait2>(id + 0x40u, 0.6f, comp => comp.CurrentBaits.Count != 0, "Baited line AOEs start")
            .ActivateOnEnter<ChokingGraspBait2>();
        ComponentCondition<ChokingGrasp>(id + 0x50u, 2.5f, comp => comp.Casters.Count != 0, "Baited line AOEs end")
            .ActivateOnEnter<ChokingGrasp>()
            .DeactivateOnExit<ChokingGraspBait2>();
        ComponentCondition<BlueShockwave>(id + 0x60u, 0.9f, comp => comp.CurrentBaits.Count != 0, "Tank swap")
            .ActivateOnEnter<BlueShockwave>();
        ComponentCondition<ChokingGrasp>(id + 0x70u, 2.4f, comp => comp.Casters.Count == 0, "Baited line AOEs resolve")
            .DeactivateOnExit<ChokingGrasp>();
        ComponentCondition<BlueShockwave>(id + 0x80u, 4.7f, comp => comp.NumCasts != 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<BlueShockwave>(id + 0x90u, 4.1f, comp => comp.NumCasts == 2, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<BlueShockwave>();
    }

    private void ColdGripExistentialDread(uint id, float delay)
    {
        ComponentCondition<ColdGripExistentialDread>(id, delay, comp => comp.NumCasts != 0, "Cleaves 1")
            .ActivateOnEnter<ColdGripExistentialDread>();
        ComponentCondition<ColdGripExistentialDread>(id + 0x10u, 1.6f, comp => comp.NumCasts == 3, "Cleave 2")
            .DeactivateOnExit<ColdGripExistentialDread>();
    }

    private void MementoMori(uint id, float delay)
    {
        ComponentCondition<MementoMori>(id, delay, comp => comp.NumCasts != 0, "Line AOE")
            .ActivateOnEnter<SmiteOfGloom>()
            .ActivateOnEnter<ChokingGraspMM>()
            .ActivateOnEnter<MementoMori>();
        ComponentCondition<MementoMori>(id + 0x10u, 1.1f, comp => Module.Arena.Bounds is not ArenaBoundsRect, "Arena split");
        ComponentCondition<ChokingGraspMM>(id + 0x20u, 5.2f, comp => comp.NumCasts != 0, "Line AOEs")
            .DeactivateOnExit<ChokingGraspMM>();
        ComponentCondition<SmiteOfGloom>(id + 0x30u, 0.8f, comp => comp.NumCasts != 0, "Spreads resolve")
            .DeactivateOnExit<SmiteOfGloom>();
        ComponentCondition<MementoMori>(id + 0x40u, 4f, comp => Module.Arena.Bounds is ArenaBoundsRect, "Restore arena")
            .DeactivateOnExit<MementoMori>();
    }

    private void SoulReapingTwoFourfoldBlight1(uint id, float delay)
    {
        ComponentCondition<Aetherblight>(id, delay, comp => comp.NumCasts != 0, "AOE resolves")
            .ActivateOnEnter<Shockwave>()
            .ExecOnExit<Aetherblight>(comp => comp.Show = false)
            .ExecOnExit<Aetherblight>(comp => comp.NumCasts = 0)
            .ActivateOnEnter<Aetherblight>();
        ComponentCondition<Shockwave>(id + 0x10u, 0.2f, comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<Shockwave>();
    }

    private void SoulReapingTwoFourfoldBlight2(uint id, float delay)
    {
        ComponentCondition<Aetherblight>(id, delay, comp => comp.NumCasts != 0, "AOE resolves")
            .ActivateOnEnter<Shockwave>()
            .ExecOnEnter<Aetherblight>(comp => comp.Show = true)
            .ExecOnEnter<Aetherblight>(comp => comp.UpdateAOEs(7.1d))
            .DeactivateOnExit<Aetherblight>();
        ComponentCondition<Shockwave>(id + 0x10u, 0.2f, comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<Shockwave>();
    }

    private void GrandCross(uint id, float delay)
    {
        ComponentCondition<GrandCross>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<GrandCross>()
            .ActivateOnEnter<GrandCrossArenaChange>()
            .ActivateOnEnter<GrandCrossRect>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<GrandCross>();
        ComponentCondition<GrandCrossArenaChange>(id + 0x10u, 1.1f, comp => Module.Arena.Bounds is not ArenaBoundsRect, "Arena change")
            .DeactivateOnExit<GrandCrossArenaChange>();
        ComponentCondition<GrandCrossBait>(id + 0x20u, 7.1f, comp => comp.NumCasts != 0, "Baited circle AOEs 1")
            .ActivateOnEnter<GrandCrossBait>();
        ComponentCondition<Shock>(id + 0x30u, 1f, comp => comp.Towers.Count != 0, "Spreads + Towers 1 appear")
            .ActivateOnEnter<GrandCrossSpread>()
            .ActivateOnEnter<Shock>();
        ComponentCondition<GrandCrossBait>(id + 0x40u, 1f, comp => comp.NumCasts != 0, "Baited circle AOEs 2")
            .ExecOnEnter<GrandCrossBait>(comp => comp.NumCasts = 0);
        ComponentCondition<GrandCrossRect>(id + 0x50u, 0.5f, comp => comp.NumCasts != 0, "Line AOE 1");
        ComponentCondition<GrandCrossRect>(id + 0x60u, 1.9f, comp => comp.NumCasts == 2, "Line AOE 2");
        ComponentCondition<Shock>(id + 0x70u, 1.6f, comp => comp.Towers.Count == 0, "Spreads + Towers 1 resolve");
        ComponentCondition<GrandCrossRect>(id + 0x80u, 3.1f, comp => comp.NumCasts == 3, "Line AOE 3");
        ComponentCondition<Shock>(id + 0x90u, 2.9f, comp => comp.Towers.Count != 0, "Spreads + Towers 2 appear");
        ComponentCondition<GrandCrossRect>(id + 0xA0u, 1.2f, comp => comp.NumCasts == 4, "Line AOE 4");
        ComponentCondition<GrandCrossRect>(id + 0xB0u, 2.6f, comp => comp.NumCasts == 5, "Line AOE 5")
            .DeactivateOnExit<GrandCrossRect>();
        ComponentCondition<Shock>(id + 0xC0u, 1.2f, comp => comp.Towers.Count == 0, "Spreads + Towers 2 resolve")
            .DeactivateOnExit<GrandCrossSpread>()
            .DeactivateOnExit<Shock>();
        ComponentCondition<GrandCrossBait>(id + 0xD0u, 7.1f, comp => comp.NumCasts != 0, "Baited circle AOEs 3")
            .ActivateOnEnter<GrandCrossProx>()
            .ActivateOnEnter<GrandCrossRW>()
            .ExecOnEnter<GrandCrossBait>(comp => comp.NumCasts = 0);
        ComponentCondition<GrandCrossProx>(id + 0xE0u, 1.6f, comp => comp.NumCasts != 0, "Proximity AOE")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<GrandCrossRW>()
            .DeactivateOnExit<GrandCrossProx>();
        ComponentCondition<GrandCrossBait>(id + 0xF0u, 0.4f, comp => comp.NumCasts != 0, "Baited circle AOEs 4")
            .DeactivateOnExit<GrandCrossBait>()
            .ExecOnEnter<GrandCrossBait>(comp => comp.NumCasts = 0);
        ComponentCondition<NeutronRing>(id + 0x100u, 12.5f, comp => comp.NumCasts != 0, "Raidwide + restore Arena")
            .ActivateOnEnter<NeutronRing>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .OnEnter(() => Module.Arena.Bounds = new ArenaBoundsRect(18f, 15f))
            .DeactivateOnExit<NeutronRing>();
    }

    private void DarknessOfEternity(uint id, float delay)
    {
        CastStart(id, (uint)AID.DarknessOfEternityVisual, delay, "Raidwide or enrage")
            .ActivateOnEnter<ChokingGrasp>()
            .ActivateOnEnter<FearOfDeathAOE2>()
            .ActivateOnEnter<MutedStruggle>()
            .ActivateOnEnter<LimitBreakAdds>()
            .ActivateOnExit<DarknessOfEternity>();
        ComponentCondition<DarknessOfEternity>(id + 0x10u, 16.4f, comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnExit<MutedStruggle>()
            .DeactivateOnExit<ChokingGrasp>()
            .DeactivateOnExit<FearOfDeathAOE2>()
            .DeactivateOnExit<LimitBreakAdds>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .SetHint(StateMachine.StateHint.DowntimeStart)
            .DeactivateOnExit<DarknessOfEternity>();
    }

    private void Prisons(uint id, float delay)
    {
        ComponentCondition<Prisons>(id, delay, comp => comp.NumDooms > 0, "Prison start")
            .ActivateOnEnter<Prisons>()
            .ActivateOnEnter<PrisonAdds>()
            .ActivateOnEnter<SpreadingFearEnrage>()
            .ActivateOnEnter<SpreadingFearInterrupt>()
            .ActivateOnEnter<ChokingGraspTB>()
            .ActivateOnEnter<ChokingGraspHealer>()
            .ActivateOnEnter<ChillingFingers>()
            .ActivateOnEnter<ChokingGrasp3>()
            .ActivateOnEnter<Slow>();
        ComponentCondition<Prisons>(id + 0x10u, 50f, comp => comp.Doomed == default, "Prison end");
        Targetable(id + 0x20u, true, 2.1f, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void SpecterOfDeathEndsEmbraceChokingGraspColdGrip(uint id, float delay)
    {
        ComponentCondition<Invitation>(id, delay, comp => comp.NumCasts != 0, "Line AOEs (prison!)")
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<TheEndsEmbrace>()
            .DeactivateOnExit<Invitation>();
        ComponentCondition<TheEndsEmbrace>(id + 0x10u, 1f, comp => comp.NumFinishedSpreads != 0, "Spreads resolve")
            .DeactivateOnExit<TheEndsEmbrace>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<ChokingGraspBait2>(id + 0x20u, 0.6f, comp => comp.CurrentBaits.Count != 0, "Baited line AOEs start")
            .ActivateOnEnter<ChokingGraspBait2>();
        ComponentCondition<ChokingGrasp>(id + 0x30u, 2.6f, comp => comp.Casters.Count != 0, "Baited line AOEs end")
            .ActivateOnEnter<ChokingGrasp>()
            .DeactivateOnExit<ChokingGraspBait2>();
        ComponentCondition<ChokingGrasp>(id + 0x40u, 3.1f, comp => comp.Casters.Count == 0, "Baited line AOEs resolve")
            .DeactivateOnExit<ChokingGrasp>();
        ComponentCondition<ColdGripExistentialDread>(id + 0x50u, 1.7f, comp => comp.NumCasts != 0, "Cleaves 1")
            .ActivateOnEnter<ColdGripExistentialDread>();
        ComponentCondition<ColdGripExistentialDread>(id + 0x60u, 1.6f, comp => comp.NumCasts == 3, "Cleave 2")
            .DeactivateOnExit<ColdGripExistentialDread>();
    }

    private void RelentlessReapingSecondFourthSeason(uint id, float delay)
    {
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var time = i == 1 ? delay : 2.8f;
            var desc = $"AOE resolve {i}";
            var casts = i;
            var hints = 4 - i;
            var cond = ComponentCondition<Aetherblight>(offset, time, i == 1 ? comp => comp.NumCasts == casts : comp => comp.Hints.Count == hints, desc);
            if (i == 1)
            {
                cond
                    .ActivateOnEnter<Shockwave>()
                    .ActivateOnEnter<Aetherblight>();
            }
            else if (i == 4)
            {
                cond
                    .DeactivateOnExit<Aetherblight>();
            }
        }
        ComponentCondition<Shockwave>(id + 0x40u, 0.2f, comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<Shockwave>();
    }

    private void CircleOfLivesSoulReaping(uint id, float delay)
    {
        ComponentCondition<CircleOfLives>(id, delay, comp => comp.NumCasts != 0, "Donut AOE 1")
            .ActivateOnEnter<Invitation>()
            .ExecOnEnter<Invitation>(comp => comp.Show = false)
            .ActivateOnEnter<CircleOfLives>();
        ComponentCondition<CircleOfLives>(id + 0x10u, 4.9f, comp => comp.NumCasts == 2, "Donut AOE 2")
            .ExecOnExit<Invitation>(comp => comp.Show = true)
            .ExecOnEnter<Invitation>(comp => comp.NextIsDanger = true);
        ComponentCondition<Invitation>(id + 0x20u, 2.7f, comp => comp.NumCasts != 0, "Line AOE (prison!)")
            .DeactivateOnExit<Invitation>()
            .ActivateOnExit<Aetherblight>()
            .ExecOnExit<Aetherblight>(comp => comp.Show = false);
        for (var i = 3; i <= 5; ++i)
        {
            var offset = id + 0x10u + (uint)((i - 1) * 0x10u);
            var time = i == 3 ? 2.3f : 5f;
            var desc = $"Donut AOE {i}";
            var casts = i;
            var cond = ComponentCondition<CircleOfLives>(offset, time, comp => comp.NumCasts == casts, desc);
            if (i == 5)
            {
                cond
                    .DeactivateOnExit<CircleOfLives>()
                    .ExecOnExit<Aetherblight>(comp => comp.Show = true)
                    .ActivateOnExit<Shockwave>();
            }
        }
        ComponentCondition<Aetherblight>(id + 0x60u, 6.6f, comp => comp.NumCasts != 0, "AOE resolves")
            .DeactivateOnExit<Aetherblight>();
        ComponentCondition<Shockwave>(id + 0x70u, 0.2f, comp => comp.NumCasts != 0, "Stacks resolve")
            .DeactivateOnExit<Shockwave>();
    }

    private void MassMacabre(uint id, float delay)
    {
        ComponentCondition<MassMacabre>(id, delay, comp => comp.Towers.Count > 2, "Towers spawn")
            .ActivateOnEnter<MassMacabre>();
        ComponentCondition<MementoMori>(id + 0x10u, 6f, comp => comp.NumCasts != 0, "Line AOE")
            .ActivateOnEnter<ChokingGraspMM>()
            .ExecOnEnter<ChokingGraspMM>(comp => comp.isRisky = false)
            .ActivateOnExit<BlueShockwave>()
            .ActivateOnEnter<MementoMori>();
        ComponentCondition<MementoMori>(id + 0x20u, 1.1f, comp => Module.Arena.Bounds is not ArenaBoundsRect, "Arena split");
        ComponentCondition<BlueShockwave>(id + 0x30u, 3.5f, comp => comp.CurrentBaits.Count != 0, "Tank swap");
        ComponentCondition<ChokingGraspMM>(id + 0x40u, 1.5f, comp => comp.NumCasts != 0, "Line AOEs")
            .DeactivateOnExit<ChokingGraspMM>();
        ComponentCondition<MementoMori>(id + 0x50u, 5f, comp => Module.Arena.Bounds is ArenaBoundsRect, "Restore arena")
            .DeactivateOnExit<MementoMori>();
        ComponentCondition<BlueShockwave>(id + 0x60u, 0.6f, comp => comp.NumCasts != 0, "Tankbuster 1");
        ComponentCondition<BlueShockwave>(id + 0x70u, 4.1f, comp => comp.NumCasts == 2, "Tankbuster 2")
            .DeactivateOnExit<BlueShockwave>();
        ComponentCondition<MassMacabre>(id + 0x80u, 1.1f, comp => comp.Towers.Count == 0, "Towers resolve")
            .ActivateOnEnter<ColdGripExistentialDread>();
        ComponentCondition<ColdGripExistentialDread>(id + 0x90u, 5.8f, comp => comp.NumCasts != 0, "Cleaves 1", 10f);
        ComponentCondition<ColdGripExistentialDread>(id + 0xA0u, 1.6f, comp => comp.NumCasts == 3, "Cleave 2")
            .DeactivateOnExit<ColdGripExistentialDread>();
    }

    private void CircleOfLivesInvitation(uint id, float delay)
    {
        ComponentCondition<CircleOfLives>(id, delay, comp => comp.NumCasts != 0, "Donut AOE 1")
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<CircleOfLives>();
        ComponentCondition<CircleOfLives>(id + 0x10u, 4.9f, comp => comp.NumCasts == 2, "Donut AOE 2");
        ComponentCondition<Invitation>(id + 0x20u, 0.3f, comp => comp.NumCasts == 2, "Line AOEs (prison!) 1");
        ComponentCondition<CircleOfLives>(id + 0x30u, 4.9f, comp => comp.NumCasts == 3, "Donut AOE 3");
        ComponentCondition<Invitation>(id + 0x40u, 4.9f, comp => comp.NumCasts == 3, "Line AOE (prison!) 2");
        ComponentCondition<CircleOfLives>(id + 0x50u, 0.1f, comp => comp.NumCasts == 4, "Donut AOE 4")
            .DeactivateOnExit<CircleOfLives>();
        ComponentCondition<Invitation>(id + 0x20u, 2.7f, comp => comp.NumCasts != 0, "Line AOE (prison!)")
            .DeactivateOnExit<Invitation>();
    }
}
