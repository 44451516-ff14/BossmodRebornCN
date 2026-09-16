namespace BossMod.Dawntrail.Alliance.A12Fafnir;

sealed class A12FafnirStates : StateMachineBuilder
{
    public A12FafnirStates(BossModule module) : base(module)
    {
        SimplePhase(default, Phase1, "P1: Until 85%")
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BalefulBreath>()
            .ActivateOnEnter<SpikeFlail>()
            .ActivateOnEnter<DragonBreath>()
            .ActivateOnEnter<DragonBreathArenaChange>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<Darter>()
            .ActivateOnEnter<PestilentSphere>()
            .ActivateOnEnter<Venom>()
            .ActivateOnEnter<DarkMatterBlast>()
            .ActivateOnEnter<SharpSpike>()
            .ActivateOnEnter<HurricaneWingAOE>()
            .ActivateOnEnter<Whirlwinds>()
            .ActivateOnEnter<GreatWhirlwindLarge>()
            .ActivateOnEnter<GreatWhirlwindSmall>()
            .ActivateOnEnter<HorridRoarPuddle>()
            .ActivateOnEnter<HorridRoarSpread>()
            .ActivateOnEnter<AbsoluteTerror>()
            .ActivateOnEnter<HurricaneWingRW>()
            .ActivateOnEnter<WingedTerror>()
            .ActivateOnEnter<ShudderingEarth>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.FindComponent<ShudderingEarth>()?.NumCasts > 0;
        SimplePhase(1u, Phase2, "P2: Until 15%")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.DarkMatterBlast;
        DeathPhase(2u, Phase3);
    }

    private void Phase1(uint id)
    {
        DarkMatterBlast(id, 6.2f);
        OffensivePostureSpikeFlail(id + 0x10000u, 5.2f);
        OffensivePostureTouchdown(id + 0x20000u, 5.2f);
        SimpleState(id + 0x30000u, 5.2f, "Next phase (85% hp)");
    }

    private void Phase2(uint id)
    {
        OffensivePostureDragonBreathTouchdown(id, 4.5f);
        OffensivePostureSpikeFlail(id + 0x10000u, 3.0f);
        BalefulBreath(id + 0x20000u, 2.2f);
        SharpSpike(id + 0x30000u, 3.7f);
        HurricaneWing1(id + 0x40000u, 14.2f);
        OffensivePostureSpikeFlail(id + 0x50000u, 2.6f);
        BalefulBreath(id + 0x60000u, 2.2f);
        AbsoluteWingedTerror(id + 0x70000u, 14.4f);
        AbsoluteWingedTerror(id + 0x80000u, 10.7f);
        SimpleState(id + 0x90000u, 4.8f, "Next phase (15% hp)");
    }

    private void Phase3(uint id)
    {
        Phase3Repeat(id, 0f, true);
        Phase3Repeat(id + 0x100000u, 10.3f, false);

        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void Phase3Repeat(uint id, float delay, bool first)
    {
        DarkMatterBlast(id, delay);
        HorridRoarOffensivePosture(id + 0x10000u, first ? 6.2f : 9.2f);
        SharpSpike(id + 0x20000u, 2.0f);
        HurricaneWing2(id + 0x30000u, 12.2f);
        OffensivePostureSpikeFlail(id + 0x40000u, 6.8f);
        BalefulBreath(id + 0x50000u, 2.2f);
    }

    private void DarkMatterBlast(uint id, float delay)
    {
        Cast(id, AID.DarkMatterBlast, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void OffensivePostureSpikeFlail(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureSpikeFlail, delay, 8f);
        ComponentCondition<SpikeFlail>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Back cleave")
            .ExecOnExit<SpikeFlail>(static comp => comp.NumCasts = 0);
    }

    private State OffensivePostureTouchdown(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureTouchdown, delay, 8f);
        return ComponentCondition<Touchdown>(id + 2u, 1.2f, static comp => comp.NumCasts > 0, "Out")
            .ExecOnExit<Touchdown>(static comp => comp.NumCasts = 0);
    }

    private void OffensivePostureDragonBreathTouchdown(uint id, float delay)
    {
        Cast(id, AID.OffensivePostureDragonBreath, delay, 8f);
        ComponentCondition<DragonBreath>(id + 2u, 1.2f, static comp => comp.NumCasts > 0, "In");
        OffensivePostureTouchdown(id + 0x1000u, 9.5f)
            .ExecOnExit<DragonBreath>(static comp => comp.NumCasts = 0);
    }

    private void BalefulBreath(uint id, float delay)
    {
        ComponentCondition<BalefulBreath>(id, delay, static comp => comp.CurrentBaits.Count == 1, "Line stack 1");
        ComponentCondition<BalefulBreath>(id + 0x10u, 13.4f, static comp => comp.CurrentBaits.Count == 0, "Line stack 4");
    }

    private void SharpSpike(uint id, float delay)
    {
        CastStart(id, AID.SharpSpike, delay);
        CastEnd(id + 1u, 5f);
        ComponentCondition<SharpSpike>(id + 2u, 1.2f, static comp => comp.NumCasts > 0, "Tankbusters")
            .ExecOnExit<SharpSpike>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HurricaneWingRaidwide(uint id, float delay)
    {
        Cast(id, AID.HurricaneWingRaidwide, delay, 3f);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x10u, 2.7f, static comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HurricaneWingRaidwide>(id + 0x20u, 8.5f, static comp => comp.NumCasts >= 9, "Raidwide 9")
            .DeactivateOnExit<HurricaneWingRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HurricaneWing1(uint id, float delay)
    {
        HurricaneWingRaidwide(id, delay);

        ComponentCondition<HurricaneWingAOE>(id + 0x100, 2, static comp => comp.AOEs.Count > 0);
        ComponentCondition<Whirlwinds>(id + 0x110, 2.5f, static comp => comp.Active);
        ComponentCondition<HurricaneWingAOE>(id + 0x120, 2.5f, static comp => comp.NumCasts > 0, "Concentric 1");
        CastStart(id + 0x121, AID.HorridRoar, 0.9f);
        ComponentCondition<GreatWhirlwindSmall>(id + 0x122, 1.1f, static comp => comp.NumCasts == 3, "Whirlwinds");
        CastEnd(id + 0x123, 1.9f);
        ComponentCondition<HurricaneWingAOE>(id + 0x130, 2.1f, static comp => comp.NumCasts >= 4);
        ComponentCondition<HurricaneWingAOE>(id + 0x200, 1.1f, static comp => comp.AOEs.Count > 0);
        Cast(id + 0x210, AID.HorridRoar, 1, 3, "Concentric 2");
        CastStart(id + 0x220, AID.HorridRoar, 4.2f);
        ComponentCondition<HurricaneWingAOE>(id + 0x221, 1.9f, static comp => comp.NumCasts >= 4);
        CastEnd(id + 0x222, 1.1f);
        ComponentCondition<HurricaneWingAOE>(id + 0x300, 4, static comp => comp.NumCasts > 0, "Concentric 3");
        Cast(id + 0x310, AID.HorridRoar, 0.2f, 3);
        ComponentCondition<HurricaneWingAOE>(id + 0x320, 2.8f, static comp => comp.NumCasts >= 4)
            .ExecOnExit<HurricaneWingAOE>(static comp => comp.NumCasts = 0);
        ComponentCondition<Whirlwinds>(id + 0x400, 5.9f, static comp => !comp.Active, "Whirlwind end");
    }

    private State AbsoluteWingedTerror(uint id, float delay)
    {
        CastMulti(id, [AID.AbsoluteTerror, AID.WingedTerror], delay, 6);
        return Condition(id + 2, 1.4f, () => Module.FindComponent<AbsoluteTerror>()?.NumCasts > 0 || Module.FindComponent<WingedTerror>()?.NumCasts > 0, "Center/sides")
            .ExecOnExit<AbsoluteTerror>(static comp => comp.NumCasts = 0)
            .ExecOnExit<WingedTerror>(static comp => comp.NumCasts = 0);
    }

    private void HorridRoarOffensivePosture(uint id, float delay)
    {
        Cast(id, AID.HorridRoar, delay, 3f);
        ComponentCondition<HorridRoarPuddle>(id + 0x10u, 1.1f, static comp => comp.Casters.Count > 0);
        ComponentCondition<HorridRoarPuddle>(id + 0x11u, 4, static comp => comp.NumCasts > 0, "Puddles");
        Cast(id + 0x1000u, AID.OffensivePostureDragonBreath, 2.9f, 8);
        ComponentCondition<DragonBreath>(id + 0x1002u, 1.2f, static comp => comp.NumCasts > 0, "In");
        CastMulti(id + 0x2000u, [AID.OffensivePostureSpikeFlail, AID.OffensivePostureTouchdown], 7.2f, 8)
            .ExecOnExit<HorridRoarPuddle>(static comp => comp.NumCasts = 0);
        Condition(id + 0x2002u, 1.1f, () => Module.FindComponent<SpikeFlail>()?.NumCasts > 0 || Module.FindComponent<Touchdown>()?.NumCasts > 0, "Out/Back cleave")
            .ExecOnExit<SpikeFlail>(static comp => comp.NumCasts = 0)
            .ExecOnExit<Touchdown>(static comp => comp.NumCasts = 0)
            .ExecOnExit<DragonBreath>(static comp => comp.NumCasts = 0);
    }

    private void HurricaneWing2(uint id, float delay)
    {
        HurricaneWingRaidwide(id, delay);

        ComponentCondition<HurricaneWingAOE>(id + 0x100u, 2, static comp => comp.AOEs.Count > 0);
        ComponentCondition<Whirlwinds>(id + 0x110u, 2.5f, static comp => comp.Active);
        CastStart(id + 0x120u, AID.HorridRoar, 1.4f);
        ComponentCondition<HurricaneWingAOE>(id + 0x121u, 1.1f, static comp => comp.NumCasts > 0, "Concentric 1");
        CastEnd(id + 0x122u, 1.9f, "Whirlwinds");
        ComponentCondition<HurricaneWingAOE>(id + 0x130u, 4.1f, static comp => comp.NumCasts >= 4);
        CastStart(id + 0x200u, AID.HorridRoar, 0.1f);
        ComponentCondition<HurricaneWingAOE>(id + 0x201u, 1, static comp => comp.AOEs.Count > 0);
        CastEnd(id + 0x202u, 2);
        ComponentCondition<HurricaneWingAOE>(id + 0x203u, 2, static comp => comp.NumCasts > 0, "Concentric 2");
        Cast(id + 0x210u, AID.HorridRoar, 2.1f, 3);
        ComponentCondition<HurricaneWingAOE>(id + 0x220u, 0.9f, static comp => comp.NumCasts >= 4);
        ComponentCondition<HurricaneWingAOE>(id + 0x300u, 1.1f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<HurricaneWingAOE>(id + 0x301u, 4, static comp => comp.NumCasts > 0, "Concentric 3");
        ComponentCondition<HurricaneWingAOE>(id + 0x310u, 6, static comp => comp.NumCasts >= 4)
            .ExecOnExit<HurricaneWingAOE>(static comp => comp.NumCasts = 0);
        AbsoluteWingedTerror(id + 0x400u, 3.2f);
    }
}
