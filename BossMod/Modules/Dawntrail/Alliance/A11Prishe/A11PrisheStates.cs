namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class A11PrisheStates : StateMachineBuilder
{
    public A11PrisheStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<CrystallineThornsHint>()
            .ActivateOnEnter<Banishga>()
            .ActivateOnEnter<BanishgaIV>()
            .ActivateOnEnter<BanishStorm>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<AsuranFists>()
            .ActivateOnEnter<AuroralUppercut>()
            .ActivateOnEnter<AuroralUppercutHint>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<NullifyingDropkick>();
    }

    private void SinglePhase(uint id)
    {
        Banishga(id, 6.2f);
        KnuckleSandwich(id + 0x10000, 9.5f);
        KnuckleSandwich(id + 0x20000, 5.4f);
        NullifyingDropkick(id + 0x30000, 3.2f);
        BanishStormHoly(id + 0x40000, 4.7f);
        CrystallineThornsAuroralUppercut(id + 0x50000, 7.0f);
        BanishgaIV(id + 0x60000, 5.2f);
        CrystallineThornsAuroralUppercut(id + 0x70000, 2.4f);
        AsuranFists(id + 0x80000, 4.6f);

        Dictionary<uint, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [(uint)AID.BanishStorm] = ((id >> 24) + 1, ForkBanishStormFirst),
            [(uint)AID.BanishgaIV] = ((id >> 24) + 2, ForkBanishgaFirst)
        };
        CastStartFork(id + 0xC0000, fork, 9.9f, "Exaflares/Orbs");
    }

    private void ForkBanishStormFirst(uint id)
    {
        ForkBanishStormFirstRepeat(id, 0, true);
        ForkBanishStormFirstRepeat(id + 0x100000, 6.8f, false);
        ForkBanishStormFirstRepeat(id + 0x200000, 5.9f, false);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void ForkBanishStormFirstRepeat(uint id, float delay, bool firstTime)
    {
        BanishStormKnuckleSandwich(id, delay);
        NullifyingDropkick(id + 0x10000, 2.2f);
        BanishgaIVCrystallineThornsAuroralUppercut(id + 0x20000, firstTime ? 6.7f : 2.6f);
        Holy(id + 0x30000, 3.2f);
        AsuranFists(id + 0x40000, 2.2f);
    }

    private void ForkBanishgaFirst(uint id)
    {
        // first loop has slightly different mechanic order
        BanishgaIVKnuckleSandwichHoly(id, 0);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x10000, 5.2f);
        NullifyingDropkick(id + 0x20000, 8.2f);
        AsuranFists(id + 0x30000, 4.7f);

        ForkBanishgaFirstRepeat(id + 0x100000, 6.8f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void ForkBanishgaFirstRepeat(uint id, float delay)
    {
        BanishgaIVKnuckleSandwichHoly(id, delay);
        NullifyingDropkick(id + 0x10000, 2.1f);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x20000, 2.6f);
        AsuranFists(id + 0x30000, 4.2f);
    }

    private void Banishga(uint id, float delay)
    {
        Cast(id, (uint)AID.Banishga, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnuckleSandwich(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.KnuckleSandwichVisual1, (uint)AID.KnuckleSandwichVisual2, (uint)AID.KnuckleSandwichVisual3], delay, 12);
        ComponentCondition<KnuckleSandwich>(id + 0x10, 1, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x11, 1.5f, comp => comp.NumCasts > 1, "In")
            .ResetComp<KnuckleSandwich>();
    }

    private void NullifyingDropkick(uint id, float delay)
    {
        Cast(id, (uint)AID.NullifyingDropkickVisual, delay, 5, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<NullifyingDropkick>(id + 2, 1.5f, comp => comp.NumCasts > 0, "Tankbuster 2")
            .ResetComp<NullifyingDropkick>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Holy(uint id, float delay)
    {
        Cast(id, (uint)AID.HolyVisual, delay, 4);
        ComponentCondition<Holy>(id + 0x10, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ResetComp<Holy>();
    }

    private void BanishStormHoly(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active);
        ComponentCondition<BanishStorm>(id + 0x20, 9.1f, comp => comp.NumCasts > 0, "Exaflares start");
        Holy(id + 0x100, 4.4f);
        ComponentCondition<BanishStorm>(id + 0x200, 2, comp => comp.Done, "Exaflares end")
            .ResetComp<BanishStorm>();
    }

    private void CrystallineThornsAuroralUppercut(uint id, float delay)
    {
        CastStart(id, (uint)AID.CrystallineThorns, delay);
        CastEnd(id + 1, 4);
        ComponentCondition<ArenaChanges>(id + 2, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        CastMulti(id + 0x10, [(uint)AID.AuroralUppercut1, (uint)AID.AuroralUppercut2, (uint)AID.AuroralUppercut3], 3.1f, 11.4f);
        ComponentCondition<AuroralUppercut>(id + 0x12, 4.6f, comp => comp.NumCasts > 0, "Knockback")
            .ResetComp<AuroralUppercut>();
        ComponentCondition<ArenaChanges>(id + 0x20, 2, comp => !comp.Active, "Spikes end")
            .ResetComp<ArenaChanges>();
    }

    private void BanishgaIV(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10, 7.8f, comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<Explosion>(id + 0x20, 12, comp => comp.NumCasts >= 41, "Explosions end")
            .ResetComp<Explosion>();
    }

    private void AsuranFists(uint id, float delay)
    {
        Cast(id, (uint)AID.AsuranFistsVisual, delay, 6.5f);
        ComponentCondition<AsuranFists>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Tower start");
        ComponentCondition<AsuranFists>(id + 0x20, 7.8f, comp => comp.NumCasts >= 8, "Tower resolve")
            .ResetComp<AsuranFists>();
    }

    private void BanishStormKnuckleSandwich(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active);
        CastStartMulti(id + 0x20, [(uint)AID.KnuckleSandwichVisual1, (uint)AID.KnuckleSandwichVisual2, (uint)AID.KnuckleSandwichVisual3], 5.7f);
        ComponentCondition<BanishStorm>(id + 0x21, 3.4f, comp => comp.NumCasts > 0, "Exaflares start");
        CastEnd(id + 0x22, 8.6f);
        ComponentCondition<KnuckleSandwich>(id + 0x30, 1, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x31, 1.5f, comp => comp.NumCasts > 1, "In")
            .ResetComp<KnuckleSandwich>()
            .ResetComp<BanishStorm>();
    }

    private void BanishgaIVCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10, 7.8f, comp => comp.NumCasts > 0, "Explosions start");
        CastStart(id + 0x20, (uint)AID.CrystallineThorns, 1.6f);
        CastEnd(id + 0x21, 4);
        ComponentCondition<ArenaChanges>(id + 0x22, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        CastStartMulti(id + 0x30, [(uint)AID.AuroralUppercut1, (uint)AID.AuroralUppercut2, (uint)AID.AuroralUppercut3], 3.1f);
        ComponentCondition<Explosion>(id + 0x31, 2.2f, comp => comp.NumCasts >= 41, "Explosions end")
            .ResetComp<Explosion>();
        CastEnd(id + 0x32, 9.2f);
        ComponentCondition<AuroralUppercut>(id + 0x33, 4.6f, comp => comp.NumCasts > 0, "Knockback")
            .ResetComp<AuroralUppercut>();
        ComponentCondition<ArenaChanges>(id + 0x40, 2, comp => !comp.Active, "Spikes end")
            .ResetComp<ArenaChanges>();
    }

    private void BanishgaIVKnuckleSandwichHoly(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishgaIV, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x10, [(uint)AID.KnuckleSandwichVisual1, (uint)AID.KnuckleSandwichVisual2, (uint)AID.KnuckleSandwichVisual3], 4.4f, 12);
        ComponentCondition<Explosion>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<KnuckleSandwich>(id + 0x21, 0.5f, comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x22, 1.5f, comp => comp.NumCasts > 1, "In")
            .ResetComp<KnuckleSandwich>();

        CastStart(id + 0x100, (uint)AID.HolyVisual, 8.2f);
        ComponentCondition<Explosion>(id + 0x101, 1.8f, comp => comp.NumCasts >= 41, "Explosions end")
            .ResetComp<Explosion>();
        CastEnd(id + 0x102, 2.2f);
        ComponentCondition<Holy>(id + 0x110, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ResetComp<Holy>();
    }

    private void BanishStormCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, (uint)AID.BanishStorm, delay, 4);
        ComponentCondition<BanishStorm>(id + 0x10, 2.7f, comp => comp.Active);
        CastStart(id + 0x20, (uint)AID.CrystallineThorns, 1.7f);
        CastEnd(id + 0x21, 4);
        ComponentCondition<ArenaChanges>(id + 0x22, 1.1f, comp => comp.NumCasts > 0, "Spikes");
        ComponentCondition<BanishStorm>(id + 0x30, 2.4f, comp => comp.NumCasts > 0, "Exaflares start");
        CastMulti(id + 0x40, [(uint)AID.AuroralUppercut1, (uint)AID.AuroralUppercut2, (uint)AID.AuroralUppercut3], 0.7f, 11.4f)
            .ResetComp<BanishStorm>();
        ComponentCondition<AuroralUppercut>(id + 0x50, 4.6f, comp => comp.NumCasts > 0, "Knockback")
            .ResetComp<AuroralUppercut>();
        ComponentCondition<ArenaChanges>(id + 0x60, 2, comp => !comp.Active, "Spikes end")
            .ResetComp<ArenaChanges>();
    }
}
