namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class A13ArkAngelsStates : StateMachineBuilder
{
    private readonly A13ArkAngels _module;

    public A13ArkAngelsStates(A13ArkAngels module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Cloudsplitter>()
            .ActivateOnEnter<TachiYukikaze>()
            .ActivateOnEnter<TachiGekko>()
            .ActivateOnEnter<TachiKasha>()
            .ActivateOnEnter<ConcertedDissolution>()
            .ActivateOnEnter<LightsChain>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<Utsusemi>()
            .ActivateOnEnter<HavokSpiral>()
            .ActivateOnEnter<SpiralFinish>()
            .ActivateOnEnter<Dragonfall>()
            .ActivateOnEnter<Guillotine>()
            .ActivateOnEnter<DominionSlash>()
            .ActivateOnEnter<CrossReaver>()
            .ActivateOnEnter<Rampage>()
            .ActivateOnEnter<Raiton>()
            .ActivateOnEnter<ArroganceIncarnate>()
            .Raw.Update = () => AllDeadOrDestroyed(A13ArkAngels.Bosses);
    }

    private void SinglePhase(uint id)
    {
        DecisiveBattle(id, 0.2f);
        Cloudsplitter(id + 0x10000, 4.2f);
        MeikyoShisui(id + 0x20000, 4.6f);
        Meteor(id + 0x30000, 2.1f);
        HavocSpiral(id + 0x40000, 3.2f);
        Dragonfall(id + 0x50000, 5.6f);
        Guillotine(id + 0x60000, 2.4f);

        Intermission(id + 0x100000, 10.2f);
        UtsusemiDominionSlash(id + 0x110000, 3.2f);
        Holy(id + 0x120000, 4.2f);
        MijinGakure(id + 0x130000, 7.9f);
        Rampage(id + 0x140000, 5.8f);
        Guillotine(id + 0x150000, 6.0f);
        MeikyoShisuiCrossReaverMeteor(id + 0x160000, 4.4f);
        ArroganceIncarnate(id + 0x170000, 4.2f);
        Cloudsplitter(id + 0x180000, 1.0f);
        Rampage(id + 0x190000, 12.7f);
        Guillotine(id + 0x1A0000, 0.6f);
        CriticalReaver(id + 0x1B0000, 7.0f);

        DominionSlashHavokSpiral(id + 0x200000, 9.2f);
        Meteor(id + 0x210000, 7.2f);
        MeikyoShisuiCrossReaver(id + 0x220000, 5.1f);
        Cloudsplitter(id + 0x230000, 3.0f);
        Raiton(id + 0x240000, 8.2f);
        DominionSlashCrossReaver(id + 0x250000, 2.9f);
        Dragonfall(id + 0x260000, 1.3f);
        ArroganceIncarnate(id + 0x270000, 15.2f);
        Rampage(id + 0x280000, 15.1f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void DecisiveBattle(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, (uint)AID.DecisiveBattleMR, delay, 4, true, "Assign target");
    }

    private void Cloudsplitter(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, (uint)AID.Cloudsplitter, delay, 5, true);
        ComponentCondition<Cloudsplitter>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Tankbusters")
            .ResetComp<Cloudsplitter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void MeikyoShisuiStart(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, (uint)AID.MeikyoShisui, delay, 4, true);
        ComponentCondition<TachiYukikaze>(id + 0x10, 1.1f, comp => comp.Casters.Count > 0);
        ComponentCondition<TachiYukikaze>(id + 0x11, 3, comp => comp.Casters.Count == 0, "Criss-cross 1");
        ComponentCondition<TachiYukikaze>(id + 0x12, 1.6f, comp => comp.Casters.Count > 0);
        ComponentCondition<TachiGekko>(id + 0x13, 2.4f, comp => comp.NumCasts > 0, "Gaze")
            .ResetComp<TachiGekko>();
        ComponentCondition<TachiYukikaze>(id + 0x14, 0.6f, comp => comp.Casters.Count == 0, "Criss-cross 2");
        ComponentCondition<ConcertedDissolution>(id + 0x15, 1.5f, comp => comp.Casters.Count > 0);
        ComponentCondition<TachiKasha>(id + 0x16, 2.9f, comp => comp.NumCasts > 0, "Out")
            .ResetComp<TachiKasha>();
        ComponentCondition<LightsChain>(id + 0x17, 0.6f, comp => comp.Casters.Count > 0);
    }

    private void MeikyoShisui(uint id, float delay)
    {
        MeikyoShisuiStart(id, delay);
        ComponentCondition<ConcertedDissolution>(id + 0x100, 2.5f, comp => comp.NumCasts == 6, "Cones")
            .ResetComp<ConcertedDissolution>();
        ComponentCondition<LightsChain>(id + 0x110, 5.5f, comp => comp.NumCasts > 0, "Donut")
            .ResetComp<LightsChain>();
    }

    private void Meteor(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, (uint)AID.Meteor, delay, 11, true, "Interrupt", true)
            .OnExit(() => _module.Arena.Bounds = A13ArkAngels.DefaultBounds); // fall back for people who joined fight late
    }

    private State HavocSpiral(uint id, float delay)
    {
        ActorCastStart(id, _module.BossMR, (uint)AID.HavocSpiral, delay, true);
        ActorCastEnd(id + 1, _module.BossMR, 5);
        ComponentCondition<HavokSpiral>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Rotation start");
        ActorCast(id + 0x10, _module.BossMR, (uint)AID.SpiralFinish, 0.6f, 11, true)
            .ResetComp<HavokSpiral>();
        return ComponentCondition<SpiralFinish>(id + 0x12, 0.5f, comp => comp.NumCasts > 0, "Knockback")
            .ResetComp<SpiralFinish>();
    }

    private void Dragonfall(uint id, float delay)
    {
        ActorCast(id, _module.BossGK, (uint)AID.Dragonfall, delay, 9, true);
        ComponentCondition<Dragonfall>(id + 2, 0.3f, comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<Dragonfall>(id + 3, 2.4f, comp => comp.NumCasts > 1, "Stack 2");
        ComponentCondition<Dragonfall>(id + 4, 2.4f, comp => comp.NumCasts > 2, "Stack 3")
            .ResetComp<Dragonfall>();
    }

    private void Guillotine(uint id, float delay)
    {
        ActorCast(id, _module.BossTT, (uint)AID.Guillotine, delay, 10.5f, true);
        ComponentCondition<Guillotine>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Cone start");
        ComponentCondition<Guillotine>(id + 0x10, 3.4f, comp => comp.NumCasts >= 4, "Cone resolve")
            .ResetComp<Guillotine>();
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Bosses disappear")
            .DeactivateOnExit<ArenaChange>()
            .DeactivateOnExit<DecisiveBattle>();
        ActorTargetable(id + 1, _module.BossHM, true, 4.3f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void UtsusemiDominionSlash(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, (uint)AID.Utsusemi, delay, 3);
        ActorCastEnd(id + 2, _module.BossEV, 2, false, "Raidwide") // dominion slash starts at the same time
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 3, 0.7f, comp => comp.AOEs.Count > 0);
        ActorCast(id + 0x10, _module.BossHM, (uint)AID.MightyStrikesClones, 0.5f, 5);
        ActorCast(id + 0x20, _module.BossHM, (uint)AID.CrossReaver, 6.3f, 3);
        ComponentCondition<CrossReaver>(id + 0x22, 1, comp => comp.Casters.Count > 0);
        ComponentCondition<DominionSlash>(id + 0x23, 2.3f, comp => comp.AOEs.Count == 0)
            .ResetComp<DominionSlash>();
        ComponentCondition<CrossReaver>(id + 0x24, 3.7f, comp => comp.NumCasts > 0, "Cross")
            .ResetComp<CrossReaver>();
    }

    private void Holy(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, (uint)AID.Holy, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MijinGakure(uint id, float delay)
    {
        ActorTargetable(id, _module.BossEV, false, delay, "Shield");
        ActorCast(id + 0x10, _module.BossHM, (uint)AID.MijinGakure, 1, 30, true, "Interrupt", true);
    }

    private void Rampage(uint id, float delay)
    {
        ActorCast(id, _module.BossMR, (uint)AID.Rampage, delay, 8, true);
        ComponentCondition<Rampage>(id + 0x10, 0.2f, comp => comp.NumCasts > 0, "Charges start");
        ComponentCondition<Rampage>(id + 0x20, 5.2f, comp => comp.NumCasts > 4, "Charges resolve")
            .ResetComp<Rampage>();
    }

    private void MeikyoShisuiCrossReaverStart(uint id, float delay)
    {
        MeikyoShisuiStart(id, delay);

        ActorCastStart(id + 0x100, _module.BossHM, (uint)AID.CrossReaver, 1.9f, true);
        ComponentCondition<ConcertedDissolution>(id + 0x101, 0.6f, comp => comp.NumCasts == 6, "Cones")
            .ResetComp<ConcertedDissolution>();
        ActorCastEnd(id + 0x102, _module.BossHM, 2.4f, true);
        ComponentCondition<CrossReaver>(id + 0x103, 1, comp => comp.Casters.Count > 0);
        ComponentCondition<LightsChain>(id + 0x104, 2.1f, comp => comp.NumCasts > 0, "Donut")
            .ResetComp<LightsChain>();
    }

    private void MeikyoShisuiCrossReaver(uint id, float delay)
    {
        MeikyoShisuiCrossReaverStart(id, delay);
        ComponentCondition<CrossReaver>(id + 0x200, 3.9f, comp => comp.NumCasts > 0, "Cross")
            .ResetComp<CrossReaver>();
    }

    private void MeikyoShisuiCrossReaverMeteor(uint id, float delay)
    {
        MeikyoShisuiCrossReaverStart(id, delay);

        ActorCastStart(id + 0x200, _module.BossTT, (uint)AID.Meteor, 1.1f, true);
        ComponentCondition<CrossReaver>(id + 0x201, 2.8f, comp => comp.NumCasts > 0, "Cross")
            .ResetComp<CrossReaver>();
        ActorCastEnd(id + 0x202, _module.BossTT, 8.2f, true, "Interrupt", true);
    }

    private void ArroganceIncarnate(uint id, float delay)
    {
        ActorCastStart(id, _module.BossEV, (uint)AID.ArroganceIncarnate, delay, true);
        ActorCastEnd(id + 1, _module.BossEV, 5, true);
        ComponentCondition<ArroganceIncarnate>(id + 2, 0.7f, comp => comp.NumFinishedStacks > 0, "Stack 1");
        ComponentCondition<ArroganceIncarnate>(id + 0x10, 4.3f, comp => comp.NumFinishedStacks >= 5, "Stack 5")
            .ResetComp<ArroganceIncarnate>();
    }

    private void CriticalReaver(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, (uint)AID.MightyStrikesBoss, delay, 5, true);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x10, 2.1f, comp => comp.NumCasts >= 1, "Raidwide 1")
            .ActivateOnEnter<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x11, 2.1f, comp => comp.NumCasts >= 2, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x12, 2.1f, comp => comp.NumCasts >= 3, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<CriticalReaverRaidwide>(id + 0x13, 2.1f, comp => comp.NumCasts >= 4, "Raidwide 4")
            .DeactivateOnExit<CriticalReaverRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x20, _module.BossHM, (uint)AID.CriticalReaverEnrage, 2.1f, 10, true, "Interrupt", true)
            .ActivateOnEnter<CriticalReaverEnrage>()
            .DeactivateOnExit<CriticalReaverEnrage>();
    }

    private void DominionSlashHavokSpiral(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, (uint)AID.DominionSlash, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 2, 0.7f, comp => comp.AOEs.Count > 0);
        HavocSpiral(id + 0x100, 2.6f);
    }

    private void Raiton(uint id, float delay)
    {
        ActorCast(id, _module.BossHM, (uint)AID.Raiton, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DominionSlashCrossReaver(uint id, float delay)
    {
        ActorCast(id, _module.BossEV, (uint)AID.DominionSlash, delay, 5, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DominionSlash>(id + 2, 0.7f, comp => comp.AOEs.Count > 0);
        ActorCast(id + 0x10, _module.BossHM, (uint)AID.CrossReaver, 8.8f, 3, true);
        ComponentCondition<CrossReaver>(id + 0x20, 1, comp => comp.Casters.Count > 0);
        ComponentCondition<CrossReaver>(id + 0x30, 6, comp => comp.NumCasts > 0, "Cross")
            .ResetComp<DominionSlash>()
            .ResetComp<CrossReaver>();
    }
}
