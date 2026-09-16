namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class A14ShadowLordStates : StateMachineBuilder
{
    public A14ShadowLordStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<UmbraSmash>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<GigaSlash>()
            .ActivateOnEnter<DamningStrikes>()
            .ActivateOnEnter<CthonicFury>()
            .ActivateOnEnter<BurningCourtMoatKeepBattlements>()
            .ActivateOnEnter<DarkNebula>()
            .ActivateOnEnter<EchoesOfAgony>()
            .ActivateOnEnter<BindingSigil>()
            .ActivateOnEnter<UnbridledRage>()
            .ActivateOnEnter<DoomArc>()
            .ActivateOnEnter<DarkNova>();
    }

    private void SinglePhase(uint id)
    {
        // note: this is a very weird fight, if you wipe, it uses a slightly different script (no initial giga slash and slightly different cthonic fury 1)
        Dictionary<bool, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [true] = (1u, SinglePhaseInitial),
            [false] = (2u, SinglePhaseAfterWipe),
        };
        ConditionFork(id, 10.2f, () => Module.PrimaryActor.CastInfo != null || Module.FindComponent<Teleport>()?.NumCasts > 0, () => (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) is (uint)AID.GigaSlashL or (uint)AID.GigaSlashR, dispatch, "First mechanic...")
            .ActivateOnEnter<Teleport>()
            .DeactivateOnExit<Teleport>();
    }

    private void SinglePhaseInitial(uint id)
    {
        GigaSlash(id, default);
        PhaseInitial(id + 0x100000u, 6.5f, true);
        PhaseRepeats(id + 0x200000u, 12.2f);
        PhaseRepeats(id + 0x300000u, 10.2f);
        PhaseRepeats(id + 0x400000u, 10.2f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void SinglePhaseAfterWipe(uint id)
    {
        PhaseInitial(id + 0x100000u, 2.4f, false);
        PhaseRepeats(id + 0x200000u, 12.2f);
        PhaseRepeats(id + 0x300000u, 10.2f);
        PhaseRepeats(id + 0x400000u, 10.2f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void PhaseInitial(uint id, float delay, bool initialPull)
    {
        UmbraSmashGigaSlash(id, delay);
        FlamesOfHatred(id + 0x10000u, 4.1f);
        Implosion(id + 0x20000u, 3.2f);
        CthonicFury1(id + 0x30000u, 3.6f, initialPull);
        NightfallTeraSlash(id + 0x40000u, 4.8f);
    }

    private void PhaseRepeats(uint id, float delay)
    {
        GigaSlashNightfall(id, delay);
        ShadowSpawnGigaSlashNightfallImplosion(id + 0x10000u, 5.4f);
        UnbridledRage(id + 0x20000u, 2.6f);
        EchoesOfAgony(id + 0x30000u, 1.2f, 7);
        BindingSigil(id + 0x40000u, 2.6f);
        DamningStrikes(id + 0x50000u, 3.5f);
        CthonicFury2(id + 0x60000u, 9.0f);
        ShadowSpawnUmbraSmashGigaSlashNightfall(id + 0x70000u, 4.6f);
        DoomArc(id + 0x80000u, 3f);
    }

    private State GigaSlash(uint id, float delay)
    {
        CastMulti(id, [AID.GigaSlashL, AID.GigaSlashR], delay, 11f);
        ComponentCondition<GigaSlash>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Cleave 1");
        return ComponentCondition<GigaSlash>(id + 0x10u, 2.1f, static comp => comp.NumCasts > 1, "Cleave 2")
            .ExecOnExit<GigaSlash>(static comp => comp.NumCasts = 0);
    }

    private void UmbraSmashGigaSlash(uint id, float delay)
    {
        Cast(id, AID.UmbraSmashBoss, delay, 4f);
        ComponentCondition<UmbraSmash>(id + 0x10u, 0.5f, static comp => comp.NumCasts > 0, "Exalines");
        GigaSlash(id + 0x100u, 9.1f)
            .ExecOnExit<UmbraSmash>(static comp => comp.NumCasts = 0);
    }

    private void FlamesOfHatred(uint id, float delay)
    {
        Cast(id, AID.FlamesOfHatred, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Implosion(uint id, float delay)
    {
        CastMulti(id, [AID.ImplosionL, AID.ImplosionR], delay, 8f);
        ComponentCondition<Implosion>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Circle + cleave")
            .ExecOnExit<Implosion>(static comp => comp.NumCasts = 0);
    }

    private void BurningCourtMoat(uint id, float delay)
    {
        ComponentCondition<BurningCourtMoatKeepBattlements>(id, delay, static comp => comp.AOEs.Count > 0);
        ComponentCondition<BurningCourtMoatKeepBattlements>(id + 1u, 7f, static comp => comp.AOEs.Count == 0, "Platform in/out");
    }

    private void EchoesOfAgony(uint id, float delay, int numCasts)
    {
        CastStart(id, AID.EchoesOfAgony, delay);
        CastEnd(id + 1u, 8f);
        ComponentCondition<EchoesOfAgony>(id + 2u, 1.1f, static comp => comp.NumFinishedStacks > 0, "Stack 1");
        ComponentCondition<EchoesOfAgony>(id + 0x10u, numCasts == 7 ? 6.4f : 4.3f, comp => comp.NumFinishedStacks >= numCasts, $"Stack {numCasts}")
            .ExecOnExit<EchoesOfAgony>(static comp => comp.NumFinishedStacks = 0);
    }

    private void CthonicFuryStart(uint id, float delay)
    {
        Cast(id, AID.CthonicFuryStart, delay, 7f, "Raidwide + platforms start")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CthonicFuryEnd(uint id, float delay)
    {
        Cast(id, AID.CthonicFuryEnd, delay, 7f, "Raidwide + platforms end")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DarkNebula(uint id, float delay)
    {
        Cast(id, AID.DarkNebula, delay, 3f);
        ComponentCondition<DarkNebula>(id + 0x10u, 1.2f, static comp => comp.KBs.Count > 0);
        ComponentCondition<DarkNebula>(id + 0x11f, 5f, static comp => comp.NumCasts > 0, "Knockback")
            .ExecOnExit<DarkNebula>(static comp => comp.NumCasts = 0);
    }

    private void CthonicFury1(uint id, float delay, bool initialPull)
    {
        CthonicFuryStart(id, delay);
        if (initialPull)
        {
            BurningCourtMoat(id + 0x1000u, 8.2f);
            BurningCourtMoat(id + 0x2000u, 3.1f);
            DarkNebula(id + 0x3000u, 6f);
        }
        else
        {
            BurningCourtMoat(id + 0x1000u, 3.2f);
            DarkNebula(id + 0x3000u, 3f);
        }
        Implosion(id + 0x4000u, 4.1f);
        BurningCourtMoat(id + 0x5000u, 2.2f);
        EchoesOfAgony(id + 0x6000u, 4f, 5);
        CthonicFuryEnd(id + 0x7000u, 1.7f);
    }

    private void NightfallTeraSlash(uint id, float delay)
    {
        Cast(id, AID.Nightfall, delay, 5f);
        ComponentCondition<TeraSlash>(id + 0x10u, 34.1f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<TeraSlash>()
            .DeactivateOnExit<TeraSlash>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State GigaSlashNightfall(uint id, float delay, bool removeComponent = true)
    {
        CastMulti(id, [AID.GigaSlashNightfallLRF, AID.GigaSlashNightfallLRB, AID.GigaSlashNightfallRLF, AID.GigaSlashNightfallRLB], delay, 14f);
        ComponentCondition<GigaSlash>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<GigaSlash>(id + 0x10u, 2.1f, static comp => comp.NumCasts > 1, "Cleave 2");
        var comp = ComponentCondition<GigaSlash>(id + 0x11u, 2.1f, static comp => comp.NumCasts > 2, "Cleave 3");
        if (removeComponent)
        {
            comp.ExecOnExit<GigaSlash>(static comp => comp.NumCasts = 0);
        }
        return comp;
    }

    private void ShadowSpawnGigaSlashNightfallImplosion(uint id, float delay)
    {
        Cast(id, AID.ShadowSpawn, delay, 3f);
        GigaSlashNightfall(id + 0x100u, 3.1f, false);

        CastStartMulti(id + 0x200u, [AID.ImplosionL, AID.ImplosionR], 7.5f);
        ComponentCondition<GigaSlash>(id + 0x201u, 1.5f, static comp => comp.NumCasts > 3, "Cleave 4");
        ComponentCondition<GigaSlash>(id + 0x202u, 2.1f, static comp => comp.NumCasts > 4, "Cleave 5")
            .ExecOnExit<GigaSlash>(static comp => comp.NumCasts = 0);
        CastEnd(id + 0x203u, 4.4f);
        ComponentCondition<Implosion>(id + 0x204u, 1f, static comp => comp.NumCasts > 0, "Circle + cleave 1");
        ComponentCondition<Implosion>(id + 0x210u, 4.6f, static comp => comp.NumCasts > 2, "Circle + cleave 2")
            .ExecOnExit<Implosion>(static comp => comp.NumCasts = 0);
    }

    private void DarkNova(uint id, float delay)
    {
        ComponentCondition<DarkNova>(id, delay, static comp => comp.Active);
        ComponentCondition<DarkNova>(id + 1u, 6f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .ExecOnExit<DarkNova>(static comp => comp.NumFinishedSpreads = 0);
    }

    private void UnbridledRage(uint id, float delay)
    {
        CastStart(id, AID.UnbridledRage, delay);
        CastEnd(id + 1u, 5f);
        ComponentCondition<UnbridledRage>(id + 2u, 0.8f, static comp => comp.NumCasts > 0, "Tankbusters")
            .ExecOnExit<UnbridledRage>(static comp => comp.NumCasts = 0)
            .SetHint(StateMachine.StateHint.Tankbuster);
        DarkNova(id + 0x1000u, 0.2f);
    }

    private void BindingSigil(uint id, float delay)
    {
        Cast(id, AID.BindingSigil, delay, 12f);
        ComponentCondition<BindingSigil>(id + 2u, 2.1f, static comp => comp.NumCasts > 0, "Puddles 1"); // 8 or 9
        ComponentCondition<BindingSigil>(id + 3u, 2.5f, static comp => comp.NumCasts > 9, "Puddles 2"); // 16 or 17
        ComponentCondition<BindingSigil>(id + 4u, 2.5f, static comp => comp.NumCasts > 17, "Puddles 3") // 25
            .ExecOnExit<BindingSigil>(static comp => comp.NumCasts = 0);
    }

    private void DamningStrikes(uint id, float delay)
    {
        CastMulti(id, [AID.DamningStrikes1, AID.DamningStrikes2], delay, 8f); // note: alt cast is longer by 0.7s, whatever...
        ComponentCondition<DamningStrikes>(id + 2u, 2.5f, static comp => comp.NumCasts >= 1, "Tower 1");
        ComponentCondition<DamningStrikes>(id + 3u, 2.5f, static comp => comp.NumCasts >= 2, "Tower 2");
        ComponentCondition<DamningStrikes>(id + 4u, 2.7f, static comp => comp.NumCasts >= 3, "Tower 3")
            .ExecOnExit<DamningStrikes>(static comp => comp.NumCasts = 0);
    }

    private void DarkNebulaGigaSlashNightfall(uint id, float delay)
    {
        Cast(id, AID.DarkNebula, delay, 3f);
        ComponentCondition<DarkNebula>(id + 0x10u, 1.2f, static comp => comp.KBs.Count > 0);
        ComponentCondition<DarkNebula>(id + 0x20u, 13, static comp => comp.NumCasts > 0, "Knockback 1");
        ComponentCondition<DarkNebula>(id + 0x21u, 3f, static comp => comp.NumCasts > 1, "Knockback 2");
        CastStartMulti(id + 0x23u, [AID.GigaSlashNightfallLRF, AID.GigaSlashNightfallLRB, AID.GigaSlashNightfallRLF, AID.GigaSlashNightfallRLB], 1.4f);
        ComponentCondition<DarkNebula>(id + 0x24u, 1.6f, static comp => comp.NumCasts > 2, "Knockback 3");
        ComponentCondition<DarkNebula>(id + 0x25u, 3f, static comp => comp.NumCasts > 3, "Knockback 4")
            .ExecOnExit<DarkNebula>(static comp => comp.NumCasts = 0);
        CastEnd(id + 0x26u, 9.4f);
        ComponentCondition<GigaSlash>(id + 0x30u, 1f, static comp => comp.NumCasts > 0, "Cleave 1");
        ComponentCondition<GigaSlash>(id + 0x31u, 2.1f, static comp => comp.NumCasts > 1, "Cleave 2");
        ComponentCondition<GigaSlash>(id + 0x32u, 2.1f, static comp => comp.NumCasts > 2, "Cleave 3")
            .ExecOnExit<GigaSlash>(static comp => comp.NumCasts = 0);
    }

    private void CthonicFury2(uint id, float delay)
    {
        CthonicFuryStart(id, delay);
        DarkNebulaGigaSlashNightfall(id + 0x1000u, 3.2f);
        BurningCourtMoat(id + 0x2000u, 3f);
        DarkNova(id + 0x3000u, 5.4f);
        CthonicFuryEnd(id + 0x4000u, 2.1f);
    }

    private void ShadowSpawnUmbraSmashGigaSlashNightfall(uint id, float delay)
    {
        Cast(id, AID.ShadowSpawn, delay, 3f);
        Cast(id + 0x10u, AID.UmbraSmashBoss, 4.2f, 4f);
        ComponentCondition<UmbraSmash>(id + 0x20u, 0.5f, static comp => comp.NumCasts > 0, "Exalines");
        GigaSlashNightfall(id + 0x100u, 12.2f)
            .ExecOnExit<UmbraSmash>(static comp => comp.NumCasts = 0);
    }

    private void DoomArc(uint id, float delay)
    {
        Cast(id, AID.DoomArc, delay, 15f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
