namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class Ch01CloudOfDarknessStates : StateMachineBuilder
{
    public Ch01CloudOfDarknessStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        BladeOfDarkness(id, 6.2f);
        BladeOfDarkness(id + 0x10000, 4.4f);
        DelugeOfDarkness1(id + 0x20000, 4.5f, true);

        // note: flare starts casting ~1.9s before first criss-cross, aero/death ~3.2s after
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x30000, 4f, comp => comp.Casters.Count > 0);
        Dictionary<bool, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [true] = (1, Fork1),
            [false] = (2, Fork2),
        };
        ConditionFork(id + 0x31000, 8f, () => Module.PrimaryActor.CastInfo != null, () => Module.PrimaryActor.CastInfo!.IsSpell(AID.Flare), dispatch, "Criss-cross start ...");
    }

    private void Fork1(uint id)
    {
        Subphase1Variant1End(id, 0f);
        FloodOfDarkness1(id + 0x100000, 8f);
        Subphase2(id + 0x200000, 6.1f);

        DelugeOfDarkness1(id + 0x400000, 8.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x410000, 4f, comp => comp.Casters.Count > 0);
        Subphase1Variant2End(id + 0x410000, 8f);

        Cast(id + 0x500000, (uint)AID.Enrage, 11.2f, 12f, "Enrage");
    }

    private void Fork2(uint id)
    {
        Subphase1Variant2End(id, 0f);
        FloodOfDarkness1(id + 0x100000, 11.2f);
        Subphase2(id + 0x200000, 6.1f);

        DelugeOfDarkness1(id + 0x400000, 8.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x410000, 4f, comp => comp.Casters.Count > 0);
        Subphase1Variant1End(id + 0x410000, 6.1f);

        Cast(id + 0x500000, (uint)AID.Enrage, 8f, 12f, "Enrage");
    }

    private void Subphase1Variant1End(uint id, float delay)
    {
        FlareUnholyDarknessBladeOfDarkness(id + 0x100, delay);
        EndeathEnaero(id + 0x10000, 1.4f);
        Break(id + 0x20000, 13.2f);
        BladeOfDarknessEndeathEnaeroResolve(id + 0x30000, 4f);
        RazingVolleyParticleBeamStart(id + 0x40000, 4.2f);
        DeathAero(id + 0x50000, 12.3f);
    }

    private void Subphase1Variant2End(uint id, float delay)
    {
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x100, delay, comp => comp.NumCasts > 0, "Criss-cross start");
        DeathAero(id + 0x10000, 3.2f);
        EndeathEnaero(id + 0x20000, 6f);
        RapidSequenceParticleBeam(id + 0x30000, 4.1f);
        RazingVolleyParticleBeamBladeOfDarknessEndeathEnaeroResolve(id + 0x40000, 12.3f);
        Flare(id + 0x50000, 9.3f);
    }

    private void Subphase2(uint id, float delay)
    {
        DelugeOfDarkness2(id, delay);
        DarkDominion(id + 0x10000, 9.3f); // note: 1s after cast ends, outer ring becomes dangerous
        ThirdArtOfDarknessParticleConcentration(id + 0x20000, 4f); // note: 3s after towers resolve, outer ring becomes normal
        GhastlyGloom(id + 0x30000, 12.3f);
        CurseOfDarkness(id + 0x40000, 8.3f);
        EvilSeedChaosCondensedDiffusiveForceParticleBeam(id + 0x50000, 10f);
        ActivePivotParticleBeam(id + 0x70000, 4.5f);
        LoomingChaos(id + 0x80000, 6.2f);

        CurseOfDarkness(id + 0x100000, 11.9f);
        ParticleConcentrationPhaser(id + 0x110000, 4.2f);
        DarkDominion(id + 0x120000, 1f); // note: 1s after cast ends, outer ring becomes dangerous
        FeintParticleBeamThirdActOfDarkness(id + 0x130000, 3.1f); // note: 2.5s after act of darkness resolves, outer ring becomes normal
        GhastlyGloom(id + 0x140000, 11.4f);
        PhaserChaosCondensedDiffusiveForceParticleBeam(id + 0x150000, 3.3f);
        FloodOfDarknessAdds(id + 0x160000, 2.9f);
        FloodOfDarkness2(id + 0x170000, 8.6f);
    }

    private void BladeOfDarkness(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.BladeOfDarknessL, (uint)AID.BladeOfDarknessR, (uint)AID.BladeOfDarknessC], delay, 7f)
            .ActivateOnEnter<BladeOfDarkness>();
        ComponentCondition<BladeOfDarkness>(id + 2, 0.7f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<BladeOfDarkness>();
    }

    private void DelugeOfDarkness1(uint id, float delay, bool first = false)
    {
        var cond = Cast(id, (uint)AID.DelugeOfDarkness1, delay, 8f, "Raidwide + arena transition")
            .SetHint(StateMachine.StateHint.Raidwide);
        if (!first)
            cond.DeactivateOnEnter<Phase2InnerCells>();
        CastMulti(id + 0x100, [(uint)AID.GrimEmbraceForward, (uint)AID.GrimEmbraceBackward], 9.2f, 5f, "Debuffs 1")
            .ActivateOnEnter<GrimEmbraceBait>()
            .ActivateOnEnter<GrimEmbraceAOE>();
        CastMulti(id + 0x110, [(uint)AID.GrimEmbraceForward, (uint)AID.GrimEmbraceBackward], 3.1f, 5f, "Debuffs 2")
            .ActivateOnEnter<RazingVolleyParticleBeam>() // has weird overlaps, easier to keep active for the entirety of the phase
            .ActivateOnEnter<EnaeroEndeath>() // we want to keep all these components active, so that they provide advance hints for delayed resolve
            .ActivateOnEnter<EndeathAOE>() // death has extra resolve steps, which make writing states weird
            .ActivateOnEnter<EnaeroAOE>();
    }

    private void RazingVolleyParticleBeamStart(uint id, float delay)
    {
        ComponentCondition<RazingVolleyParticleBeam>(id, delay, comp => comp.Casters.Count > 0);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x10, 8f, comp => comp.NumCasts > 0, "Criss-cross start");
    }

    private void DeathAero(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.Death, (uint)AID.Aero], delay, 5.6f);
        ComponentCondition<EnaeroEndeath>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Knockback/attract");
    }

    private void EndeathEnaero(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.Endeath, (uint)AID.Enaero], delay, 5f, "Store knockback/attract");
    }

    private void BladeOfDarknessEndeathEnaeroResolve(uint id, float delay)
    {
        BladeOfDarkness(id, delay);
        ComponentCondition<EnaeroEndeath>(id + 0x100, 2.2f, comp => comp.NumCasts > 0, "Knockback/attract");
    }

    private void RazingVolleyParticleBeamBladeOfDarknessEndeathEnaeroResolve(uint id, float delay)
    {
        RazingVolleyParticleBeamStart(id, delay);
        BladeOfDarknessEndeathEnaeroResolve(id + 0x1000, 2.2f);
    }

    private void Break(uint id, float delay)
    {
        Cast(id, (uint)AID.BreakBoss, delay, 5)
            .ActivateOnEnter<Break>();
        ComponentCondition<Break>(id + 0x10, 1.1f, comp => comp.Eyes.Count == 0, "Gazes")
            .DeactivateOnExit<Break>();
    }

    private void RapidSequenceParticleBeam(uint id, float delay)
    {
        Cast(id, (uint)AID.RapidSequenceParticleBeam, delay, 7f)
            .ActivateOnEnter<RapidSequenceParticleBeam>();
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x10, 0.8f, comp => comp.NumCasts > 0, "Wild charges 1");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x11, 2f, comp => comp.NumCasts > 3, "Wild charges 2");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x13, 2f, comp => comp.NumCasts > 6, "Wild charges 3");
        ComponentCondition<RapidSequenceParticleBeam>(id + 0x14, 2f, comp => comp.NumCasts > 9, "Wild charges 4")
            .DeactivateOnExit<RapidSequenceParticleBeam>();
    }

    private void Flare(uint id, float delay)
    {
        Cast(id, (uint)AID.Flare, delay, 4);
        ComponentCondition<Flare>(id + 0x10, 1f, comp => comp.Spreads.Count != 0)
            .ActivateOnEnter<Flare>();
        ComponentCondition<Flare>(id + 0x20, 8.1f, comp => comp.NumFinishedSpreads > 0, "Flares")
            .DeactivateOnExit<Flare>();
    }

    private void FlareUnholyDarknessBladeOfDarkness(uint id, float delay)
    {
        CastStart(id, (uint)AID.Flare, delay);
        ComponentCondition<RazingVolleyParticleBeam>(id + 1, 1.9f, comp => comp.NumCasts > 0, "Criss-cross start");
        ComponentCondition<RazingVolleyParticleBeam>(id + 2, 2f, comp => comp.NumCasts > 1);
        CastEnd(id + 3, 0.1f);
        ComponentCondition<Flare>(id + 0x10, 1f, comp => comp.Spreads.Count != 0)
            .ActivateOnEnter<Flare>();
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x11, 0.9f, comp => comp.NumCasts > 2);
        CastStart(id + 0x20, (uint)AID.UnholyDarkness, 1.2f);
        ComponentCondition<RazingVolleyParticleBeam>(id + 0x21, 0.8f, comp => comp.NumCasts > 3);
        CastEnd(id + 0x22, 4.2f);
        ComponentCondition<UnholyDarkness>(id + 0x30, 0.7f, comp => comp.Stacks.Count > 0)
            .ActivateOnEnter<UnholyDarkness>();
        ComponentCondition<Flare>(id + 0x40, 0.3f, comp => comp.NumFinishedSpreads != 0, "Flares")
            .DeactivateOnExit<Flare>();
        CastStartMulti(id + 0x50, [(uint)AID.BladeOfDarknessL, (uint)AID.BladeOfDarknessR, (uint)AID.BladeOfDarknessC], 7.1f);
        ComponentCondition<UnholyDarkness>(id + 0x51, 0.7f, comp => comp.NumFinishedStacks > 0, "Stacks")
            .ActivateOnEnter<BladeOfDarkness>()
            .DeactivateOnExit<UnholyDarkness>();
        CastEnd(id + 0x52, 6.3f);
        ComponentCondition<BladeOfDarkness>(id + 0x53, 0.7f, comp => comp.NumCasts > 0, "In/out")
            .DeactivateOnExit<BladeOfDarkness>();
    }

    private void FloodOfDarkness1(uint id, float delay)
    {
        Cast(id, (uint)AID.FloodOfDarkness1, delay, 7f, "Raidwide + arena transition")
            .DeactivateOnExit<GrimEmbraceBait>()
            .DeactivateOnExit<GrimEmbraceAOE>()
            .DeactivateOnExit<RazingVolleyParticleBeam>()
            .DeactivateOnExit<EnaeroEndeath>()
            .DeactivateOnExit<EndeathAOE>()
            .DeactivateOnExit<EnaeroAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DelugeOfDarkness2(uint id, float delay)
    {
        Cast(id, (uint)AID.DelugeOfDarkness2, delay, 8f, "Raidwide + arena transition")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<StygianShadow>(id + 0x10, 4.2f, comp => comp.ActiveActors.Count != 0, "Platform adds")
            .ActivateOnEnter<StygianShadow>()
            .ActivateOnEnter<Phase2AIHints>()
            .ActivateOnEnter<Atomos>()
            .ActivateOnEnter<Phase2InnerCells>()
            .ActivateOnEnter<DarkEnergyParticleBeam>(); // overlaps with multiple mechanics
    }

    private void DarkDominion(uint id, float delay)
    {
        Cast(id, (uint)AID.DarkDominion, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThirdArtOfDarknessParticleConcentration(uint id, float delay)
    {
        ComponentCondition<ThirdArtOfDarknessCleave>(id, delay, comp => comp.Mechanics.Count > 0)
            .ActivateOnEnter<ThirdArtOfDarknessCleave>()
            .ActivateOnEnter<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .ActivateOnEnter<ThirdArtOfDarknessMultiProngedParticleBeam>();
        Cast(id + 0x10, (uint)AID.ParticleConcentration, 2.1f, 6f)
            .ActivateOnEnter<ParticleConcentration>(); // note: towers appear ~1s after cast end
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x20, 1.5f, comp => comp.NumCasts > 0, "Add cleave 1");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x30, 3f, comp => comp.NumCasts > 2, "Add cleave 2");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x40, 3f, comp => comp.NumCasts > 4, "Add cleave 3")
            .DeactivateOnExit<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessMultiProngedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessCleave>();
        ComponentCondition<ParticleConcentration>(id + 0x50, 3.6f, comp => comp.Towers.Count == 0, "Towers")
            .ExecOnEnter<ParticleConcentration>(comp => comp.ShowOuterTowers())
            .DeactivateOnExit<ParticleConcentration>();
    }

    private State GhastlyGloom(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.GhastlyGloomCross, (uint)AID.GhastlyGloomDonut], delay, 7.8f)
            .ActivateOnEnter<GhastlyGloomCross>()
            .ActivateOnEnter<GhastlyGloomDonut>();
        return Condition(id + 2, 0.7f, () => Module.FindComponent<GhastlyGloomCross>()?.NumCasts > 0 || Module.FindComponent<GhastlyGloomDonut>()?.NumCasts > 0, "Cross/donut")
            .DeactivateOnExit<GhastlyGloomCross>()
            .DeactivateOnExit<GhastlyGloomDonut>();
    }

    private void CurseOfDarkness(uint id, float delay)
    {
        ComponentCondition<CurseOfDarkness>(id, delay, comp => comp.NumCasts > 0, "Raidwide + bait debuffs")
            .ActivateOnEnter<CurseOfDarkness>()
            .DeactivateOnExit<CurseOfDarkness>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State FloodOfDarknessAdds(uint id, float delay)
    {
        ComponentCondition<FloodOfDarknessAdd>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FloodOfDarknessAdd>();
        return Timeout(id + 1, 6f, "Interrupt adds")
            .DeactivateOnExit<FloodOfDarknessAdd>();
    }

    private void EvilSeedChaosCondensedDiffusiveForceParticleBeam(uint id, float delay)
    {
        ComponentCondition<EvilSeedBait>(id, delay, comp => comp.Baiters.Any())
            .ActivateOnEnter<EvilSeedBait>();
        ComponentCondition<EvilSeedAOE>(id + 0x10, 8.1f, comp => comp.Casters.Count > 0, "Seed plant")
            .ActivateOnEnter<EvilSeedAOE>()
            .ActivateOnEnter<EvilSeedVoidzone>()
            .DeactivateOnExit<EvilSeedBait>();

        GhastlyGloom(id + 0x1000, 2.8f)
            .DeactivateOnExit<EvilSeedAOE>();

        ComponentCondition<ThornyVine>(id + 0x2000, 14f, comp => comp.Targets.Any())
            .ActivateOnEnter<ThornyVine>();
        ComponentCondition<ThornyVine>(id + 0x2010, 3f, comp => comp.TethersAssigned, "Tethers");
        FloodOfDarknessAdds(id + 0x2020, 2.2f);

        CastMulti(id + 0x3000, [(uint)AID.ChaosCondensedParticleBeam, (uint)AID.DiffusiveForceParticleBeam], 8.1f, 8f)
            .ActivateOnEnter<ChaosCondensedParticleBeam>()
            .ActivateOnEnter<DiffusiveForceParticleBeam>()
            .DeactivateOnExit<EvilSeedVoidzone>()
            .DeactivateOnExit<ThornyVine>();
        Condition(id + 0x3010, 0.8f, () => Module.FindComponent<ChaosCondensedParticleBeam>()?.NumCasts > 0 || Module.FindComponent<DiffusiveForceParticleBeam>()?.NumCasts > 0, "Spread/line stacks")
            .DeactivateOnExit<ChaosCondensedParticleBeam>()
            .DeactivateOnExit<DiffusiveForceParticleBeam>(); // TODO: show second wave ...
    }

    private void ActivePivotParticleBeam(uint id, float delay)
    {
        CastStartMulti(id, [(uint)AID.ActivePivotParticleBeamCW, (uint)AID.ActivePivotParticleBeamCCW], delay);
        ComponentCondition<Phaser>(id + 1, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<ActivePivotParticleBeam>()
            .ActivateOnEnter<Phaser>();
        ComponentCondition<Phaser>(id + 2, 8f, comp => comp.NumCasts > 0, "Adds front/sides");
        ComponentCondition<Phaser>(id + 3, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        CastEnd(id + 4, 3.7f);
        ComponentCondition<ActivePivotParticleBeam>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Rotation start");
        ComponentCondition<ActivePivotParticleBeam>(id + 0x20, 6.6f, comp => comp.NumCasts > 4, "Rotation end")
            .DeactivateOnExit<ActivePivotParticleBeam>();
        ComponentCondition<Excruciate>(id + 0x30, 0.7f, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Excruciate>();
        ComponentCondition<Excruciate>(id + 0x31, 5f, comp => comp.NumCasts > 0, "Adds tankbusters")
            .DeactivateOnExit<Excruciate>();
    }

    private void LoomingChaos(uint id, float delay)
    {
        Cast(id, (uint)AID.LoomingChaosBoss, delay, 7);
        ComponentCondition<LoomingChaos>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide + swap positions")
            .ActivateOnEnter<LoomingChaos>()
            .DeactivateOnExit<LoomingChaos>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ParticleConcentrationPhaser(uint id, float delay)
    {
        CastStart(id, (uint)AID.ParticleConcentration, delay);
        ComponentCondition<Phaser>(id + 1, 1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Phaser>();
        CastEnd(id + 2, 5f);
        ComponentCondition<Phaser>(id + 0x10, 3f, comp => comp.NumCasts > 0, "Adds front/sides")
            .ActivateOnEnter<ParticleConcentration>(); // TODO: towers appear 1s after cast end
        ComponentCondition<Phaser>(id + 0x11, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        ComponentCondition<ParticleConcentration>(id + 0x20, 6.6f, comp => comp.Towers.Count == 0, "Towers")
            .ExecOnEnter<ParticleConcentration>(comp => comp.ShowOuterTowers())
            .DeactivateOnExit<ParticleConcentration>();
    }

    private void FeintParticleBeamThirdActOfDarkness(uint id, float delay)
    {
        CastStart(id, (uint)AID.FeintParticleBeam, delay);
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 1, 4.9f, comp => comp.Mechanics.Count > 0)
            .ActivateOnEnter<ThirdArtOfDarknessCleave>()
            .ActivateOnEnter<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .ActivateOnEnter<ThirdArtOfDarknessMultiProngedParticleBeam>();
        CastEnd(id + 2, 1.1f);
        ComponentCondition<FeintParticleBeam>(id + 0x10, 1.1f, comp => comp.Chasers.Count > 0)
            .ActivateOnEnter<FeintParticleBeam>();
        ComponentCondition<FeintParticleBeam>(id + 0x11, 4f, comp => comp.NumCasts > 0, "Chasers start");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x20, 3.5f, comp => comp.NumCasts > 0, "Add cleave 1");
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x30, 3f, comp => comp.NumCasts > 2, "Add cleave 2")
            .DeactivateOnExit<FeintParticleBeam>(); // chasers resolve ~0.5s earlier
        ComponentCondition<ThirdArtOfDarknessCleave>(id + 0x40, 3f, comp => comp.NumCasts > 4, "Add cleave 3")
            .DeactivateOnExit<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessMultiProngedParticleBeam>()
            .DeactivateOnExit<ThirdArtOfDarknessCleave>();
    }

    private void PhaserChaosCondensedDiffusiveForceParticleBeam(uint id, float delay)
    {
        ComponentCondition<Phaser>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Phaser>();
        CastStartMulti(id + 0x10, [(uint)AID.ChaosCondensedParticleBeam, (uint)AID.DiffusiveForceParticleBeam], 7.5f);
        ComponentCondition<Phaser>(id + 0x11, 0.5f, comp => comp.NumCasts > 0, "Adds front/sides")
            .ActivateOnEnter<ChaosCondensedParticleBeam>()
            .ActivateOnEnter<DiffusiveForceParticleBeam>();
        ComponentCondition<Phaser>(id + 0x12, 1.5f, comp => comp.NumCasts >= 6, "Adds sides/front")
            .DeactivateOnExit<Phaser>();
        CastEnd(id + 0x13, 6f);
        Condition(id + 0x20, 0.8f, () => Module.FindComponent<ChaosCondensedParticleBeam>()?.NumCasts > 0 || Module.FindComponent<DiffusiveForceParticleBeam>()?.NumCasts > 0, "Spread/line stacks")
            .DeactivateOnExit<ChaosCondensedParticleBeam>()
            .DeactivateOnExit<DiffusiveForceParticleBeam>(); // TODO: show second wave ...
    }

    private void FloodOfDarkness2(uint id, float delay)
    {
        CastStart(id, (uint)AID.FloodOfDarkness2, delay, "Adds disappear")
            .DeactivateOnExit<StygianShadow>()
            .DeactivateOnExit<Atomos>()
            .DeactivateOnExit<Phase2AIHints>()
            .DeactivateOnExit<DarkEnergyParticleBeam>();
        CastEnd(id + 1, 7f, "Raidwide + arena transition")
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}

