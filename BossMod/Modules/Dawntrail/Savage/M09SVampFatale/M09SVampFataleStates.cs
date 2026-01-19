namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class M09SVampFataleStates : StateMachineBuilder
{
    public M09SVampFataleStates(M09SVampFatale module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        // try group states by bigger mechs? eg. hector video split
        KillerVoice(id, 5.2f);
        Hardcore(id + 0x10000u, 5.2f);
        CurseOfTheBombpyre(id + 0x30000u, 5f);
        PreArena1(id + 0x35000u, 6.9f);
        Arena1(id + 0x40000u, 7f);
        CrowdKill(id + 0x50000u, 8.4f);
        FinaleFatale(id + 0x60000u, 12.8f);
        Aetherletting(id + 0x70000u, 6f);
        Hardcore(id + 0x80000u, 1.9f);
        CurseOfTheBombpyre(id + 0x90000u, 5.2f);
        HalfMoon(id + 0xA0000u, 5f);
        Arena2Start(id + 0xB0000u, 5f);
        Arena2(id + 0xC0000u, 5f);
        HellInACell(id + 0xD0000u, 8.5f);
        SanguineScratch(id + 0xE0000u, 22.1f);
        Enrage(id + 0xF0000u, 3.9f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void KillerVoice(uint id, float delay)
    {
        Cast(id, (uint)AID.KillerVoice, delay, 5f, "Raidwide")
            .ActivateOnEnter<KillerVoice>()
            .DeactivateOnExit<KillerVoice>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Hardcore(uint id, float delay)
    {
        Cast(id, (uint)AID.HardcoreCast, delay, 3f)
            .ActivateOnEnter<Hardcore>();

        ComponentCondition<Hardcore>(id + 0x10, 2f, comp => comp.CurrentBaits.Count == 0, "Tankbuster spread")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<Hardcore>();
    }

    private void CurseOfTheBombpyre(uint id, float delay)
    {
        // vamp stomp cast start
        // vampettes action timeline, grab rotations at cast start otherwise too late
        // tp to middle 30.6s
        // stack icon appears 41.6s, same time as last 5 vampettes finish
        // activate BrutalRain component at same time as Bombyre?
        CastStart(id, (uint)AID.VampStomp, delay)
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<BombpyreRing>()
            .ActivateOnEnter<BlastBeat>()
            .ActivateOnEnter<CurseOfTheBombpyre>()
            .ActivateOnEnter<BrutalRain>();

        ComponentCondition<VampStomp>(id + 0x10, 5f, comp => comp.NumCasts > 0, "Circle AOE")
            .DeactivateOnExit<VampStomp>();

        ComponentCondition<BlastBeat>(id + 0x20, 11f, comp => comp.NumCasts >= 10, "Bombpyre resolve")
            .DeactivateOnExit<BlastBeat>();

        // does timeline get thrown off if waiting for spreads? just keep it active, or deactivate after party stack?
        ComponentCondition<CurseOfTheBombpyre>(id + 0x30, 2f, comp => comp.Spreads.Count == 0, "Finish spreads")
            .DeactivateOnExit<CurseOfTheBombpyre>()
            .DeactivateOnExit<BombpyreRing>();
    }

    private void PreArena1(uint id, float delay)
    {
        // stack appears just before bombpyre fully ends, safe to check count = 0 without waiting for count > 0?
        // brutal rain can be 3+ hits; depends on number of times used or satisfied stacks?
        // change brutal rain either delay higher or max overdue higher, can be 3+ hits, 1 sec per hit
        ComponentCondition<BrutalRain>(id, delay, comp => comp.Stacks.Count == 0, "Party stack", checkDelay: 1f)
            .DeactivateOnExit<BrutalRain>();

        CastStart(id + 0x10, (uint)AID.SadisticScreech, 6.3f)
            .ActivateOnEnter<SadisticScreech>();

        ComponentCondition<ArenaChanges>(id + 0x20, 5.9f, comp => comp.Active, "Raidwide + Arena change")
            .DeactivateOnExit<SadisticScreech>()
            .ActivateOnExit<Coffinmaker>()
            .ActivateOnExit<Coffinfiller>()
            .ActivateOnExit<DeadWake>()
            .ActivateOnExit<HalfMoon>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Arena1(uint id, float delay)
    {
        ComponentCondition<Coffinmaker>(id, delay, comp => comp.ActiveActors.Count > 0, "Coffinmaker spawn");

        // boss may become targetable while casting last 2 halfmoons depending on DPS
        //Targetable(id + 0x10, true, 69.5f, "Enrage 1");
        // timing for sadistic screech based on coffinmaker death? not based on boss targetability
        // fixed from expected dead wake end time? 125.671 -> 5s cast time -> 130.671 expected
        // sadistic screech start 132.733
        // another run: 124.03 died before 4th dead wake -> sadistic screech start 132.574
        // sadistic screech always starts ~132.5s

        // can't use CastStart on sadistic screech if boss has another cast before it
        ComponentCondition<HalfMoon>(id + 0x10, 55.6f, comp => comp.NumCasts >= 8);
        CastStart(id + 0x20, (uint)AID.SadisticScreech, 9.2f).ActivateOnEnter<SadisticScreech>();

        ComponentCondition<ArenaChanges>(id + 0x30, 5.7f, comp => !comp.Active, "Raidwide + Arena change")
            .DeactivateOnExit<SadisticScreech>()
            .DeactivateOnExit<Coffinfiller>()
            .DeactivateOnExit<DeadWake>()
            .DeactivateOnExit<HalfMoon>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CrowdKill(uint id, float delay)
    {
        Cast(id, (uint)AID.CrowdKillCast, delay, 0.5f)
            .ActivateOnEnter<CrowdKill>();
        ComponentCondition<CrowdKill>(id + 0x10, 5.3f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<CrowdKill>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void FinaleFatale(uint id, float delay)
    {
        CastStart(id, (uint)AID.FinaleFataleCast, delay)
            .ActivateOnEnter<FinaleFatale>();
        ComponentCondition<FinaleFatale>(id + 0x10, 6.1f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<FinaleFatale>()
            .ActivateOnEnter<PulpingPulse>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Aetherletting(uint id, float delay)
    {
        CastStart(id, (uint)AID.AetherlettingCast, delay, "Aetherletting")
            .ActivateOnEnter<AetherlettingCone>()
            .ActivateOnEnter<AetherlettingPuddle>()
            .ActivateOnEnter<AetherlettingCross>();

        // puddles and crosses already casting while cones are still going off
        // write the states in order? or turn it into 1 big state/component?

        ComponentCondition<AetherlettingCone>(id + 0x10, 4f, comp => comp.ActiveCasters.Length > 0, "Cones start");
        ComponentCondition<AetherlettingCone>(id + 0x20, 15.2f, comp => comp.NumCasts >= 8, "Cones end")
            .DeactivateOnExit<AetherlettingCone>();

        ComponentCondition<AetherlettingPuddle>(id + 0x30, 1.9f, comp => comp.NumFinishedSpreads >= 8, "Puddles end")
            .DeactivateOnExit<AetherlettingPuddle>();

        ComponentCondition<AetherlettingCross>(id + 0x40, 14.5f, comp => comp.NumCasts >= 8, "Crosses end")
            .DeactivateOnExit<AetherlettingCross>()
            .ActivateOnEnter<HalfMoon>();
    }

    private void HalfMoon(uint id, float delay)
    {
        ComponentCondition<HalfMoon>(id, delay, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<HalfMoon>(id + 0x10, 3f, comp => comp.NumCasts > 1, "Cleave 2")
            .DeactivateOnExit<HalfMoon>()
            .ActivateOnExit<BrutalRain>();

        ComponentCondition<BrutalRain>(id + 0x20, 2.1f, comp => comp.Stacks.Count > 0);
        ComponentCondition<BrutalRain>(id + 0x30, 10.1f, comp => comp.Stacks.Count == 0, "Party stack")
            .DeactivateOnExit<BrutalRain>();
    }

    private void Arena2Start(uint id, float delay)
    {
        Cast(id, (uint)AID.InsatiableThirstCast, delay, 2.8f)
            .ActivateOnEnter<InsatiableThirst>();
        ComponentCondition<InsatiableThirst>(id + 0x10, 3.2f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<InsatiableThirst>()
            .SetHint(StateMachine.StateHint.Raidwide);

        CastStart(id + 0x10, (uint)AID.SadisticScreech, 7.3f)
            .ActivateOnEnter<SadisticScreech>()
            .ActivateOnEnter<GravegrazerBig>()
            .ActivateOnEnter<GravegrazerSmall>();

        ComponentCondition<ArenaChanges>(id + 0x20, 5.9f, comp => comp.Active, "Raidwide + Arena change")
            .DeactivateOnExit<SadisticScreech>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Arena2(uint id, float delay)
    {
        // arena changes
        // 3x aoes + add spawn
        // 2x raidwides between towers
        // sadistic screech + arena change
        ComponentCondition<ArenaChanges>(id, delay, comp => comp.Active)
            .ActivateOnEnter<DeadlyDoornail>()
            .ActivateOnEnter<FatalFlail>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<ElectrocutionVoidzone>();

        ComponentCondition<Plummet>(id + 0x10, 12.2f, comp => comp.NumCasts > 0, "Tank towers 1");
        KillerVoice(id + 0x20, 4f);
        ComponentCondition<Plummet>(id + 0x30, 9.1f, comp => comp.NumCasts > 2, "Tank towers 2");
        KillerVoice(id + 0x40, 4f);
        ComponentCondition<Plummet>(id + 0x50, 9.1f, comp => comp.NumCasts > 4, "Tank towers 3");

        CastStart(id + 0x60, (uint)AID.SadisticScreech, 18f)
            .ActivateOnEnter<SadisticScreech>();

        ComponentCondition<ArenaChanges>(id + 0x70, 5.7f, comp => !comp.Active, "Raidwide + Arena change")
            .DeactivateOnExit<SadisticScreech>()
            .DeactivateOnExit<GravegrazerBig>()
            .DeactivateOnExit<GravegrazerSmall>()
            .DeactivateOnExit<DeadlyDoornail>()
            .DeactivateOnExit<FatalFlail>()
            .DeactivateOnExit<Plummet>()
            .DeactivateOnExit<Electrocution>()
            .DeactivateOnExit<ElectrocutionVoidzone>();
    }

    private void HellInACell(uint id, float delay)
    {
        // charnel cell actors spawn immediately after finale fatale
        // pulping pulse sometimes twice; based on boss stacks? keep active to avoid breaking timeline
        // each tower soaker tethered to a vampette 454.315; each vampette gets status extra 0x4B
        // sanguine scratch cast start; cast and first actual same time, ignore cast?
        CrowdKill(id, delay);
        CastStart(id + 0x10, (uint)AID.FinaleFataleCast, 12.8f)
            .ActivateOnEnter<FinaleFatale>();

        ComponentCondition<FinaleFatale>(id + 0x20, 6.1f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<FinaleFatale>()
            .ActivateOnExit<BloodyBondage>()
            .SetHint(StateMachine.StateHint.Raidwide);

        // pulping pulse

        // tower spawn times likely static, using cell deaths for state can throw off timeline
        // tower 1 start @ 396.093 -> 401.177
        // spread & amp
        // tower 2 start @ 418.491 -> 423.564
        // spread & amp
        // puddles x1 may start before all cells dead
        // last puddles @ 447.715
        // LP tower starts 448.939

        // always puddles x2, use this to get time left into LP towers otherwise timeline can get thrown off
        // check puddles numcasts > 10 (sets of 8)
        // last puddles at 447.715
        // LP tower starts 448.939

        // Hell In A Cell cast at exact same time as Bloody Bondage; would ActivateOnEnter work with CastStart on Hell?
        ComponentCondition<BloodyBondage>(id + 0x30, 11f, comp => comp.NumCasts > 0, "Towers (party 1)")
            .ActivateOnEnter<CharnelCells>()
            .ActivateOnExit<UltrasonicSpreadTank>()
            .ActivateOnExit<UltrasonicSpreadRest>()
            .ActivateOnExit<UltrasonicAmp>()
            .DeactivateOnExit<PulpingPulse>();

        // 2nd tower spawn time static or relative to previous cells killed?
        ComponentCondition<BloodyBondage>(id + 0x40, 22.4f, comp => comp.NumCasts > 4, "Towers (party 2)")
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnExit<CharnelCells>()
            .DeactivateOnExit<BloodyBondage>();

        ComponentCondition<PulpingPulse>(id + 0x50, 24.2f, comp => comp.NumCasts > 10)
            .ActivateOnExit<BloodyBondageUndeadDeathmatch>()
            .DeactivateOnExit<CharnelCells>()
            .DeactivateOnExit<UltrasonicSpreadTank>()
            .DeactivateOnExit<UltrasonicSpreadRest>()
            .DeactivateOnExit<UltrasonicAmp>();

        // spawns 2 vampettes 440.106 to use later for sanguince scratch

        ComponentCondition<BloodyBondageUndeadDeathmatch>(id + 0x60, 6.2f, comp => comp.NumCasts > 0, "Light party towers")
            .DeactivateOnExit<BloodyBondageUndeadDeathmatch>()
            .ActivateOnExit<SanguineScratch>()
            .ActivateOnExit<BreakdownWing>();
    }

    private void SanguineScratch(uint id, float delay)
    {
        ComponentCondition<BreakdownWing>(id, delay, comp => comp.NumCasts > 0, "Bat explosion 1");
        ComponentCondition<BreakdownWing>(id + 0x10, 21.3f, comp => comp.NumCasts > 2, "Bat explosion 2")
            .DeactivateOnExit<BreakdownWing>()
            .DeactivateOnExit<SanguineScratch>()
            .ActivateOnExit<BrutalRain>();

        ComponentCondition<BrutalRain>(id + 0x20, 1.7f, comp => comp.Stacks.Count > 0, "Party stack");
        ComponentCondition<BrutalRain>(id + 0x30, 10f, comp => comp.Stacks.Count == 0, "Stack resolve")
            .DeactivateOnExit<BrutalRain>()
            .ActivateOnExit<HalfMoon>();
    }

    private void Enrage(uint id, float delay)
    {
        // bombpyre status and vamp stomp happen at same time
        // does CurseOfTheBombpyre need to be activated beforehand to track status?
        // halfmoons start coming out right before blast beat ends; activate before bombpyre?
        CurseOfTheBombpyre(id, delay);

        // no party stack after thie one; take party stack out of HalfMoon above?
        ComponentCondition<HalfMoon>(id + 0x10, 4.8f, comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<HalfMoon>(id + 0x20, 3f, comp => comp.NumCasts > 1, "Cleave 2")
            .DeactivateOnExit<HalfMoon>()
            .ActivateOnExit<SanguineScratch>();

        Hardcore(id + 0x30, 2.2f);
        // sanguine scratch, no bats, only 1 set
        // insatiable thirst start 565.273
        // can't do cast start check for insatiable since boss casts sanguine first; check for both casts in order or wait for sanguine to end
        ComponentCondition<SanguineScratch>(id + 0x40, 21.9f, comp => comp.NumCasts >= 40)
            .DeactivateOnExit<SanguineScratch>();

        Cast(id + 0x50, (uint)AID.InsatiableThirstCast, 4.6f, 2.8f)
            .ActivateOnEnter<InsatiableThirst>();
        ComponentCondition<InsatiableThirst>(id + 0x60, 3.2f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<InsatiableThirst>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CrowdKill(id + 0x70, 7.9f);
        Cast(id + 0x80, (uint)AID.FinaleFataleEnrageCast, 14f, 10f, "Enrage");
    }
}
