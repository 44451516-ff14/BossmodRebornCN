namespace BossMod.Dawntrail.Alliance.A11Prishe;

sealed class A11PrisheStates : StateMachineBuilder
{
    public A11PrisheStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
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
        KnuckleSandwich(id + 0x10000u, 9.5f);
        KnuckleSandwich(id + 0x20000u, 5.4f);
        NullifyingDropkick(id + 0x30000u, 3.2f);
        BanishStormHoly(id + 0x40000u, 4.7f);
        CrystallineThornsAuroralUppercut(id + 0x50000u, 7.0f);
        BanishgaIV(id + 0x60000u, 5.2f);
        CrystallineThornsAuroralUppercut(id + 0x70000u, 2.4f);
        AsuranFists(id + 0x80000u, 4.6f);

        Dictionary<AID, (uint seqID, Action<uint> buildState)> fork = new()
        {
            [AID.BanishStorm] = ((id >> 24) + 1u, ForkBanishStormFirst),
            [AID.BanishgaIV] = ((id >> 24) + 2u, ForkBanishgaFirst)
        };
        CastStartFork(id + 0xC0000, fork, 9.9f, "Exaflares/Orbs");
    }

    private void ForkBanishStormFirst(uint id)
    {
        ForkBanishStormFirstRepeat(id, 0f, true);
        ForkBanishStormFirstRepeat(id + 0x100000u, 6.8f, false);
        ForkBanishStormFirstRepeat(id + 0x200000u, 5.9f, false);

        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void ForkBanishStormFirstRepeat(uint id, float delay, bool firstTime)
    {
        BanishStormKnuckleSandwich(id, delay);
        NullifyingDropkick(id + 0x10000u, 2.2f);
        BanishgaIVCrystallineThornsAuroralUppercut(id + 0x20000u, firstTime ? 6.7f : 2.6f);
        Holy(id + 0x30000u, 3.2f);
        AsuranFists(id + 0x40000u, 2.2f);
    }

    private void ForkBanishgaFirst(uint id)
    {
        // first loop has slightly different mechanic order
        BanishgaIVKnuckleSandwichHoly(id, 0f);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x10000u, 5.2f);
        NullifyingDropkick(id + 0x20000u, 8.2f);
        AsuranFists(id + 0x30000u, 4.7f);

        ForkBanishgaFirstRepeat(id + 0x100000u, 6.8f);

        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void ForkBanishgaFirstRepeat(uint id, float delay)
    {
        BanishgaIVKnuckleSandwichHoly(id, delay);
        NullifyingDropkick(id + 0x10000u, 2.1f);
        BanishStormCrystallineThornsAuroralUppercut(id + 0x20000u, 2.6f);
        AsuranFists(id + 0x30000u, 4.2f);
    }

    private void Banishga(uint id, float delay)
    {
        Cast(id, AID.Banishga, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnuckleSandwich(uint id, float delay)
    {
        CastMulti(id, [AID.KnuckleSandwichVisual1, AID.KnuckleSandwichVisual2, AID.KnuckleSandwichVisual3], delay, 12f);
        ComponentCondition<KnuckleSandwich>(id + 0x10, 1f, static comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x11, 1.5f, static comp => comp.NumCasts > 1, "In")
            .ExecOnExit<KnuckleSandwich>(static comp => comp.NumCasts = 0);
    }

    private void NullifyingDropkick(uint id, float delay)
    {
        Cast(id, AID.NullifyingDropkickVisual, delay, 5f, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<NullifyingDropkick>(id + 2u, 1.5f, static comp => comp.NumCasts > 0, "Tankbuster 2")
            .ExecOnExit<NullifyingDropkick>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Holy(uint id, float delay)
    {
        Cast(id, AID.HolyVisual, delay, 4f);
        ComponentCondition<Holy>(id + 0x10u, 1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .ExecOnExit<Holy>(static comp => comp.NumFinishedSpreads = 0);
    }

    private void BanishStormHoly(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4f);
        ComponentCondition<BanishStorm>(id + 0x10u, 2.7f, static comp => comp.Lines.Count != 0);
        ComponentCondition<BanishStorm>(id + 0x20u, 9.1f, static comp => comp.NumCasts > 0, "Exaflares start");
        Holy(id + 0x100u, 4.4f);
        ComponentCondition<BanishStorm>(id + 0x200u, 2f, static comp => comp.Done, "Exaflares end")
            .ExecOnExit<BanishStorm>(static comp => comp.Reset());
    }

    private void CrystallineThornsAuroralUppercut(uint id, float delay)
    {
        CastStart(id, AID.CrystallineThorns, delay);
        CastEnd(id + 1u, 4f);
        ComponentCondition<ArenaChanges>(id + 2u, 1.1f, static comp => comp.NumCasts > 0, "Spikes");
        CastMulti(id + 0x10u, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 3.1f, 11.4f);
        ComponentCondition<AuroralUppercut>(id + 0x12u, 4.6f, static comp => comp.NumCasts > 0, "Knockback")
            .ExecOnExit<AuroralUppercut>(static comp => comp.NumCasts = 0);
        ComponentCondition<ArenaChanges>(id + 0x20u, 2f, static comp => !comp.Active, "Spikes end");
    }

    private void BanishgaIV(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10u, 7.8f, static comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<Explosion>(id + 0x20u, 12, static comp => comp.NumCasts >= 41, "Explosions end")
            .ExecOnExit<Explosion>(static comp => comp.NumCasts = 0);
    }

    private void AsuranFists(uint id, float delay)
    {
        Cast(id, AID.AsuranFistsVisual, delay, 6.5f);
        ComponentCondition<AsuranFists>(id + 0x10u, 0.5f, static comp => comp.NumCasts > 0, "Tower start");
        ComponentCondition<AsuranFists>(id + 0x20u, 7.8f, static comp => comp.NumCasts >= 8, "Tower resolve")
            .ExecOnExit<AsuranFists>(static comp => comp.NumCasts = 0);
    }

    private void BanishStormKnuckleSandwich(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4f);
        ComponentCondition<BanishStorm>(id + 0x10u, 2.7f, static comp => comp.Lines.Count != 0);
        CastStartMulti(id + 0x20u, [AID.KnuckleSandwichVisual1, AID.KnuckleSandwichVisual2, AID.KnuckleSandwichVisual3], 5.7f);
        ComponentCondition<BanishStorm>(id + 0x21u, 3.4f, static comp => comp.NumCasts > 0, "Exaflares start");
        CastEnd(id + 0x22u, 8.6f);
        ComponentCondition<KnuckleSandwich>(id + 0x30u, 1, static comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x31u, 1.5f, static comp => comp.NumCasts > 1, "In")
            .ExecOnExit<KnuckleSandwich>(static comp => comp.NumCasts = 0)
            .ExecOnExit<BanishStorm>(static comp => comp.NumCasts = 0);
    }

    private void BanishgaIVCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Explosion>(id + 0x10u, 7.8f, static comp => comp.NumCasts > 0, "Explosions start");
        CastStart(id + 0x20u, AID.CrystallineThorns, 1.6f);
        CastEnd(id + 0x21u, 4f);
        ComponentCondition<ArenaChanges>(id + 0x22u, 1.1f, static comp => comp.NumCasts > 0, "Spikes");
        CastStartMulti(id + 0x30u, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 3.1f);
        ComponentCondition<Explosion>(id + 0x31u, 2.2f, static comp => comp.NumCasts >= 41, "Explosions end")
            .ExecOnExit<Explosion>(static comp => comp.NumCasts = 0);
        CastEnd(id + 0x32u, 9.2f);
        ComponentCondition<AuroralUppercut>(id + 0x33u, 4.6f, static comp => comp.NumCasts > 0, "Knockback")
            .ExecOnExit<AuroralUppercut>(static comp => comp.NumCasts = 0);
        ComponentCondition<ArenaChanges>(id + 0x40u, 2, static comp => !comp.Active, "Spikes end");
    }

    private void BanishgaIVKnuckleSandwichHoly(uint id, float delay)
    {
        Cast(id, AID.BanishgaIV, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastMulti(id + 0x10u, [AID.KnuckleSandwichVisual1, AID.KnuckleSandwichVisual2, AID.KnuckleSandwichVisual3], 4.4f, 12f);
        ComponentCondition<Explosion>(id + 0x20u, 0.5f, static comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<KnuckleSandwich>(id + 0x21u, 0.5f, static comp => comp.NumCasts > 0, "Out");
        ComponentCondition<KnuckleSandwich>(id + 0x22u, 1.5f, static comp => comp.NumCasts > 1, "In")
            .ExecOnExit<KnuckleSandwich>(static comp => comp.NumCasts = 0);

        CastStart(id + 0x100u, AID.HolyVisual, 8.2f);
        ComponentCondition<Explosion>(id + 0x101u, 1.8f, static comp => comp.NumCasts >= 41, "Explosions end")
            .ExecOnExit<Explosion>(static comp => comp.NumCasts = 0);
        CastEnd(id + 0x102u, 2.2f);
        ComponentCondition<Holy>(id + 0x110u, 1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .ExecOnExit<Holy>(static comp => comp.NumFinishedSpreads = 0);
    }

    private void BanishStormCrystallineThornsAuroralUppercut(uint id, float delay)
    {
        Cast(id, AID.BanishStorm, delay, 4f);
        ComponentCondition<BanishStorm>(id + 0x10u, 2.7f, static comp => comp.Lines.Count != 0);
        CastStart(id + 0x20u, AID.CrystallineThorns, 1.7f);
        CastEnd(id + 0x21u, 4f);
        ComponentCondition<ArenaChanges>(id + 0x22u, 1.1f, static comp => comp.NumCasts > 0, "Spikes");
        ComponentCondition<BanishStorm>(id + 0x30u, 2.4f, static comp => comp.NumCasts > 0, "Exaflares start");
        CastMulti(id + 0x40u, [AID.AuroralUppercut1, AID.AuroralUppercut2, AID.AuroralUppercut3], 0.7f, 11.4f)
            .ExecOnExit<AuroralUppercut>(static comp => comp.NumCasts = 0);
        ComponentCondition<AuroralUppercut>(id + 0x50, 4.6f, static comp => comp.NumCasts > 0, "Knockback")
            .ExecOnExit<AuroralUppercut>(static comp => comp.NumCasts = 0);
        ComponentCondition<ArenaChanges>(id + 0x60, 2, static comp => !comp.Active, "Spikes end");
    }
}
