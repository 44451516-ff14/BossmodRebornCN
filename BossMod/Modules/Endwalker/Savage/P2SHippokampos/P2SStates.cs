namespace BossMod.Endwalker.Savage.P2SHippokampos;

sealed class P2SStates : StateMachineBuilder
{
    public P2SStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<SewageDeluge>();
    }

    private void SinglePhase(uint id)
    {
        MurkyDepths(id, 10.2f);
        DoubledImpact(id + 0x010000u, 5.2f);

        // deluge 1
        SewageDeluge(id + 0x100000u, 7.8f);
        Cataract(id + 0x110000u, 15);
        Coherence(id + 0x120000u, 8.1f);
        MurkyDepths(id + 0x130000u, 8.2f);
        OminousBubblingShockwave(id + 0x140000u, 3.7f);
        AvariceCataract(id + 0x150000u, 10.9f);
        // note: deluge 1 ends here...

        Flow1(id + 0x200000u, 8.6f);
        DoubledImpact(id + 0x210000u, 8.2f);
        MurkyDepths(id + 0x220000u, 5.2f);

        // deluge 2
        SewageDeluge(id + 0x300000u, 11.7f);
        Shockwave(id + 0x310000u, 9.6f);
        KampeosHarma(id + 0x320000u, 4.4f);
        DoubledImpact(id + 0x330000u, 10);
        MurkyDepths(id + 0x340000u, 4.2f);
        Flow2(id + 0x350000u, 8.6f);
        Cataract(id + 0x360000u, 0.7f);
        // note: deluge 2 ends here...

        AvariceDissociationCataract(id + 0x400000u, 15.2f);
        DissociationEruptionFloodCoherence(id + 0x410000u, 9.8f);
        DoubledImpact(id + 0x420000u, 7.2f);
        MurkyDepths(id + 0x430000u, 3.2f);

        // deluge 3
        SewageDeluge(id + 0x500000u, 12.8f);
        Flow3(id + 0x510000u, 11.7f);
        DissociationEruption(id + 0x520000u, 6.9f);
        OminousBubblingShockwave(id + 0x530000u, 0.8f);
        DoubledImpact(id + 0x540000u, 5.4f);
        MurkyDepths(id + 0x550000u, 7.2f);
        MurkyDepths(id + 0x560000u, 6.2f);

        Cast(id + 0x600000u, AID.Enrage, 5.3f, 10f, "Enrage");
    }

    private void MurkyDepths(uint id, float delay)
    {
        Cast(id, AID.MurkyDepths, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DoubledImpact(uint id, float delay)
    {
        Cast(id, AID.DoubledImpact, delay, 5f, "Shared Tankbuster")
            .ActivateOnEnter<DoubledImpact>()
            .DeactivateOnExit<DoubledImpact>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SewageDeluge(uint id, float delay)
    {
        Cast(id, AID.SewageDeluge, delay, 5f, "Deluge")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State Cataract(uint id, float delay)
    {
        CastStartMulti(id, [AID.SpokenCataract, AID.WingedCataract], delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        return CastEnd(id + 1u, 8f, "Cataract")
            .ActivateOnEnter<Cataract>()
            .DeactivateOnExit<Cataract>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private State Coherence(uint id, float delay)
    {
        Cast(id, AID.Coherence, delay, 12f)
            .ActivateOnEnter<Coherence>();
        return ComponentCondition<Coherence>(id + 2u, 3.2f, static comp => comp.NumCasts > 0, "Coherence")
            .DeactivateOnExit<Coherence>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Shockwave(uint id, float delay)
    {
        // TODO: some component (knockback distance=16? or just make sure autorot uses arms length?)
        Cast(id, AID.Shockwave, delay, 8f, "Shockwave")
            .SetHint(StateMachine.StateHint.Knockback);
    }

    // note: this activates component, which has to be deactivated later manually
    private void PredatoryAvarice(uint id, float delay)
    {
        Cast(id, AID.PredatoryAvarice, delay, 4f, "Avarice")
            .ActivateOnEnter<PredatoryAvarice>();
    }

    // note: this activates component, which has to be deactivated later manually
    private void Dissociation(uint id, float delay)
    {
        Cast(id, AID.Dissociation, delay, 4f, "Dissociation")
            .ActivateOnEnter<Dissociation>();
    }

    // note: this activates component, which has to be deactivated later manually
    private State TaintedFlood(uint id, float delay)
    {
        return Cast(id, AID.TaintedFlood, delay, 3f, "Flood")
            .ActivateOnEnter<TaintedFlood>();
    }

    // note: this activates component, which has to be deactivated later manually
    private State SewageEruption(uint id, float delay)
    {
        return Cast(id, AID.SewageEruption, delay, 5f, "Eruption")
            .ActivateOnEnter<SewageEruption>();
    }

    private void AvariceCataract(uint id, float delay)
    {
        PredatoryAvarice(id, delay);
        Cataract(id + 0x1000u, 9.8f)
            .ClearHint(StateMachine.StateHint.PositioningEnd);
        ComponentCondition<PredatoryAvarice>(id + 0x2000u, 6.2f, static comp => !comp.Active, "Avarice resolve")
            .DeactivateOnExit<PredatoryAvarice>()
            .SetHint(StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide);
    }

    private void AvariceDissociationCataract(uint id, float delay)
    {
        PredatoryAvarice(id, delay);
        Dissociation(id + 0x1000u, 2.4f);
        // note: we don't create separate avarice/dissociation resolve states here, since they all resolve at almost the same time
        Cataract(id + 0x2000u, 10.1f)
            .DeactivateOnExit<PredatoryAvarice>()
            .DeactivateOnExit<Dissociation>()
            .SetHint(StateMachine.StateHint.Raidwide); // avarice resolve
    }

    private void DissociationEruptionFloodCoherence(uint id, float delay)
    {
        Dissociation(id, delay);
        SewageEruption(id + 0x1000u, 8.2f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        TaintedFlood(id + 0x2000u, 2.4f)
            .DeactivateOnExit<Dissociation>(); // resolve happens mid-cast
        Coherence(id + 0x3000u, 4.7f)
            .DeactivateOnExit<SewageEruption>() // resolve happens right before cast start
            .DeactivateOnExit<TaintedFlood>() // resolve happens mid-cast
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void DissociationEruption(uint id, float delay)
    {
        Dissociation(id, delay);
        SewageEruption(id + 0x1000u, 8.3f)
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<SewageEruption>(id + 0x2000u, 7.6f, static comp => comp.Casters.Count == 0 && comp.NumCasts > 0, "Resolve")
            .DeactivateOnExit<Dissociation>() // resolve happens at the same time as the first set of eruptions
            .DeactivateOnExit<SewageEruption>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void Flow1(uint id, float delay)
    {
        Cast(id, AID.ChannelingFlow, delay, 5f, "Flow 1")
            .ActivateOnEnter<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<ChannelingFlow>(id + 0x1000u, 14f, static comp => comp.NumStunned > 0)
            .SetHint(StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart);
        ComponentCondition<ChannelingFlow>(id + 0x2000u, 3f, static comp => comp.NumStunned == 0, "Flow resolve")
            .DeactivateOnExit<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide);
    }

    private void Flow2(uint id, float delay)
    {
        // flow 2: same statuses as first flow, different durations
        Cast(id, AID.ChannelingOverflow, delay, 5f, "Flow 2")
            .ActivateOnEnter<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        TaintedFlood(id + 0x1000u, 4.2f);
        ComponentCondition<ChannelingFlow>(id + 0x2000u, 6.8f, static comp => comp.NumStunned > 0);
        ComponentCondition<TaintedFlood>(id + 0x2001u, 0.3f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<TaintedFlood>();
        ComponentCondition<ChannelingFlow>(id + 0x2002u, 2.7f, static comp => comp.NumStunned == 0, "Hit 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        TaintedFlood(id + 0x3000u, 2.6f);
        ComponentCondition<ChannelingFlow>(id + 0x4000u, 6.4f, static comp => comp.NumStunned > 0);
        ComponentCondition<TaintedFlood>(id + 0x4001u, 0.6f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<TaintedFlood>();
        ComponentCondition<ChannelingFlow>(id + 0x4002u, 2.4f, static comp => comp.NumStunned == 0, "Hit 2")
            .DeactivateOnExit<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void Flow3(uint id, float delay)
    {
        // flow 3: same as flow 2, but with coherence instead of floods
        Cast(id, AID.ChannelingOverflow, delay, 5f, "Flow 3")
            .ActivateOnEnter<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Coherence(id + 0x1000u, 5.5f); // first hit is around coherence cast end
        ComponentCondition<ChannelingFlow>(id + 0x2000u, 8.3f, static comp => comp.NumStunned > 0);
        ComponentCondition<ChannelingFlow>(id + 0x3000u, 3f, static comp => comp.NumStunned == 0, "Flow resolve") // second hit
            .DeactivateOnExit<ChannelingFlow>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void OminousBubblingShockwave(uint id, float delay)
    {
        Cast(id, AID.OminousBubbling, delay, 3f, "TwoStacks")
            .ActivateOnEnter<OminousBubbling>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        Shockwave(id + 0x1000u, 2.8f);
        ComponentCondition<OminousBubbling>(id + 0x2000u, 3.8f, static comp => comp.NumCasts > 0, "AOE resolve")
            .DeactivateOnExit<OminousBubbling>()
            .SetHint(StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd);
    }

    private void KampeosHarma(uint id, float delay)
    {
        CastStart(id, AID.KampeosHarma, delay)
            .ActivateOnEnter<KampeosHarma>() // note: icons appear right before harma cast start...
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1u, 8.5f, "Harma");
        Targetable(id + 2u, false, 0f); // usually happens together with cast end
        Targetable(id + 3u, true, 7.4f, "Harma resolve")
            .DeactivateOnExit<KampeosHarma>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
