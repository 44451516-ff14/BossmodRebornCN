namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class P5SStates : StateMachineBuilder
{
    public P5SStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SonicHowl(id, 9.2f);
        RubyGlowTopazStones(id + 0x10000u, 2.1f);
        VenomousMassToxicCrunch(id + 0x20000u, 8.2f, true);
        VenomTowers(id + 0x30000u, 8.1f);
        VenomousMassToxicCrunch(id + 0x40000u, 4.1f);
        RubyGlowTopazStonesDoubleRush(id + 0x50000u, 9.1f);
        SonicHowl(id + 0x60000u, 4.7f, true);
        RubyGlowTopazCluster(id + 0x70000u, 8.2f);
        VenomousMassToxicCrunch(id + 0x80000u, 3.6f);
        VenomSquallSurge(id + 0x90000u, 6.1f);
        ClawTail(id + 0xA0000u, 2.4f);
        StarvingStampede(id + 0xB0000u, 19.6f); // this delay is sometimes 0.6s less (claw/tail?)
        SonicHowl(id + 0xC0000u, 8.3f);
        RubyGlowTopazStonesVenomPoolRayClaw(id + 0xD0000u, 2.1f);
        VenomousMassToxicCrunch(id + 0xE0000u, 9.3f, true);
        RubyGlowTopazStonesVenomSquall(id + 0xF0000u, 8.2f);
        VenomTowersClawTail(id + 0x100000u, 0.3f);
        VenomousMassToxicCrunch(id + 0x110000u, 2.8f); // 2.7-3.4 range (claw/tail?)
        SonicHowl(id + 0x120000u, 6.1f);
        RubyGlowTopazStonesVenomPoolDoubleRush(id + 0x130000u, 2.2f);
        VenomousMassToxicCrunch(id + 0x140000u, 3.2f, true);
        SonicHowl(id + 0x150000u, 8.2f);
        VenomSquallSurge(id + 0x160000u, 3.2f);
        ClawTail(id + 0x170000u, 0.5f);
        VenomousMassToxicCrunch(id + 0x180000u, 4.4f); // this delay is sometimes 0.6s less (claw/tail?)
        SonicShatter(id + 0x190000u, 9.1f);
        Cast(id + 0x1A0000u, AID.AcidicSlaver, 15.3f, 5f, "Enrage");
    }

    private void VenomousMassToxicCrunch(uint id, float delay, bool endRubyGlow = false)
    {
        Cast(id, AID.VenomousMass, delay, 5f)
            .ActivateOnEnter<VenomousMass>()
            .DeactivateOnExit<RubyGlowCommon>(endRubyGlow);
        ComponentCondition<VenomousMass>(id + 2u, 0.8f, static comp => comp.NumCasts > 0, "Tankbuster 1")
            .DeactivateOnExit<VenomousMass>();

        Cast(id + 0x1000u, AID.ToxicCrunch, 1.3f, 5f)
            .ActivateOnEnter<ToxicCrunch>();
        ComponentCondition<ToxicCrunch>(id + 0x1002u, 0.3f, static comp => comp.NumCasts > 0, "Tankbuster 2")
            .DeactivateOnExit<ToxicCrunch>();
    }

    private void SonicHowl(uint id, float delay, bool endRubyGlow = false)
    {
        Cast(id, AID.SonicHowl, delay, 5f, "Raidwide")
            .DeactivateOnExit<RubyGlowCommon>(endRubyGlow)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State RubyGlow(uint id, float delay)
    {
        return Cast(id, AID.RubyGlow, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void RubyGlowTopazStones(uint id, float delay)
    {
        // ruby glow 1: 2x2 cells, 2 magic, 2 poison - need to find a safespot
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow1>();
        Cast(id + 0x1000, AID.TopazStones, 3.2f, 4f);
        ComponentCondition<RubyGlow1>(id + 0x1010u, 0.5f, static comp => comp.MagicStones.Count != 0);
        ComponentCondition<RubyGlow1>(id + 0x1020u, 13.5f, static comp => comp.MagicStones.Count == 0, "Cells");
        // note: poison disappears later, during next mechanic...
    }

    private void RubyGlowTopazStonesDoubleRush(uint id, float delay)
    {
        // ruby glow 2: diagonal line, 1 magic, 1 poison, charge - need to avoid first charge and then avoid magic
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow2>();
        Cast(id + 0x1000u, AID.TopazStones, 3.2f, 4f);
        DoubleRush(id + 0x1010u, 4.5f);
        ComponentCondition<RubyGlow2>(id + 0x1020u, 1.5f, static comp => comp.MagicStones.Count == 0, "Cells");
        // note: poison disappears later, during next mechanic...
    }

    private void RubyGlowTopazCluster(uint id, float delay)
    {
        // ruby glow 3: 2x2 cells, 2+2+3+3 magic - need to move between safespots
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow3>();
        Cast(id + 0x1000u, AID.TopazCluster, 2.1f, 4f);
        ComponentCondition<RubyGlow3>(id + 0x1010u, 11.1f, static comp => comp.NumCasts >= 2, "Cells 1");
        ComponentCondition<RubyGlow3>(id + 0x1011u, 2.5f, static comp => comp.NumCasts >= 4, "Cells 2");
        ComponentCondition<RubyGlow3>(id + 0x1012u, 2.5f, static comp => comp.NumCasts >= 7, "Cells 3");
        ComponentCondition<RubyGlow3>(id + 0x1013u, 2.5f, static comp => comp.NumCasts >= 10, "Cells 4")
            .DeactivateOnExit<RubyGlow3>();
    }

    private void RubyGlowTopazStonesVenomPoolRayClaw(uint id, float delay)
    {
        // ruby glow 4: diagonal line, 2+3 magic, 2 venom pools - need to recolor 2 magic to poison and then avoid ray/claw while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow4>();
        Cast(id + 0x1000u, AID.TopazStones, 2.1f, 4f);
        Cast(id + 0x1010u, AID.VenomPoolRecolor, 2.1f, 5f);
        ComponentCondition<RubyGlow4>(id + 0x1020u, 3.9f, static comp => comp.NumCasts > 0, "Recolor");
        ComponentCondition<RubyGlow4>(id + 0x1030u, 3f, static comp => comp.MagicStones.Count == 0, "Cells");
        CastMulti(id + 0x2000u, [AID.SearingRay, AID.RagingClaw], 0.2f, 5f, "Searing ray / Raging claw");
        // note: poison disappears later, during next mechanic...
        // note: raging claw continues hitting for ~2.5s, searing ray resolves immediately - next mechanic is fixed relative to cast end
    }

    private void RubyGlowTopazStonesVenomSquall(uint id, float delay)
    {
        // ruby glow 5: 2x2 cells, 2 magic, 2 poison, spread - need to avoid magic and then spread while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow5>();
        Cast(id + 0x1000u, AID.TopazStones, 3.1f, 4f);
        ComponentCondition<RubyGlow5>(id + 0x1010u, 0.5f, static comp => comp.MagicStones.Count != 0);
        // note: we next part is same as venom squall/surge, except that order is fixed; magic explosion happens ~0.1s before squall cast end
        CastStart(id + 0x1020u, AID.VenomSquall, 8.6f);
        ComponentCondition<RubyGlow5>(id + 0x1021u, 4.9f, static comp => comp.MagicStones.Count == 0, "Cells");
        CastEnd(id + 0x1022u, 0.1f)
            .ActivateOnEnter<VenomSquallSurge>(); // note: activating only after cells resolve to reduce visual clutter
        ComponentCondition<VenomSquallSurge>(id + 0x1023u, 3.8f, static comp => comp.Progress > 0, "Spread");
        ComponentCondition<VenomSquallSurge>(id + 0x1024u, 3f, static comp => comp.Progress > 1, "Mid bait");
        ComponentCondition<VenomDrops>(id + 0x1025u, 3f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<VenomDrops>()
            .DeactivateOnExit<VenomDrops>()
            .DeactivateOnExit<RubyGlow5>(); // poison disappears ~2.5s into cast
        ComponentCondition<VenomSquallSurge>(id + 0x1026u, 3f, static comp => comp.Progress > 2, "Stack")
            .DeactivateOnExit<VenomSquallSurge>();
    }

    private void RubyGlowTopazStonesVenomPoolDoubleRush(uint id, float delay)
    {
        // ruby glow 6: 2x2 cells, 3+2+2+2 magic, venom pools - need to recolor 2 magic to poison and then avoid charges while avoiding poison
        RubyGlow(id, delay)
            .ActivateOnEnter<RubyGlow6>();
        Cast(id + 0x1000u, AID.TopazStones, 2.1f, 4f);
        Cast(id + 0x1010u, AID.VenomPoolRecolor, 2.1f, 5f);
        ComponentCondition<RubyGlow6>(id + 0x1020u, 3.9f, static comp => comp.NumCasts > 0, "Recolor");
        ComponentCondition<RubyGlow6>(id + 0x1030u, 3f, static comp => comp.MagicStones.Count == 0, "Cells");
        DoubleRush(id + 0x1040u, 4f);
        // note: poison disappears later, during next mechanic...
    }

    private void DoubleRush(uint id, float delay)
    {
        Cast(id, AID.DoubleRush, delay, 6f, "Charge 1")
            .ActivateOnEnter<DoubleRush>()
            .DeactivateOnExit<DoubleRush>();
        ComponentCondition<DoubleRushReturn>(id + 2u, 2.1f, static comp => comp.NumCasts > 0, "Charge 2")
            .ActivateOnEnter<DoubleRushReturn>()
            .DeactivateOnExit<DoubleRushReturn>();
    }

    private void VenomSquallSurge(uint id, float delay)
    {
        CastMulti(id, [AID.VenomSquall, AID.VenomSurge], delay, 5f)
            .ActivateOnEnter<VenomSquallSurge>();
        ComponentCondition<VenomSquallSurge>(id + 2u, 3.8f, static comp => comp.Progress > 0, "Spread/stack");
        ComponentCondition<VenomSquallSurge>(id + 3u, 3f, static comp => comp.Progress > 1, "Mid bait");
        ComponentCondition<VenomDrops>(id + 4u, 3f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<VenomDrops>()
            .DeactivateOnExit<VenomDrops>();
        ComponentCondition<VenomSquallSurge>(id + 5u, 3f, static comp => comp.Progress > 2, "Stack/spread")
            .DeactivateOnExit<VenomSquallSurge>();
    }

    private void VenomTowers(uint id, float delay)
    {
        ComponentCondition<VenomTowers>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        ComponentCondition<VenomTowers>(id + 0x10u, 13f, static comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
    }

    private void ClawTail(uint id, float delay)
    {
        // note: tail to claw is ~0.5s shorter (and next state is longer), but other timings are unaffected
        CastStartMulti(id, [AID.ClawToTail, AID.TailToClaw], delay);
        ComponentCondition<ClawTail>(id + 1u, 6f, static comp => comp.Progress > 0, "Claw/tail hit 1")
            .ActivateOnEnter<ClawTail>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        ComponentCondition<ClawTail>(id + 0x100u, 4.2f, static comp => comp.Progress >= 8, "Claw/tail end")
            .DeactivateOnExit<ClawTail>();
    }

    private void VenomTowersClawTail(uint id, float delay)
    {
        ComponentCondition<VenomTowers>(id, delay, static comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        CastStartMulti(id + 1u, [AID.ClawToTail, AID.TailToClaw], 12.2f);
        ComponentCondition<VenomTowers>(id + 2u, 0.8f, static comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
        ComponentCondition<ClawTail>(id + 3u, 5.2f, static comp => comp.Progress > 0, "Claw/tail hit 1")
            .ActivateOnEnter<ClawTail>()
            .SetHint(StateMachine.StateHint.BossCastEnd);
        ComponentCondition<ClawTail>(id + 0x100u, 4.2f, static comp => comp.Progress >= 8, "Claw/tail end")
            .DeactivateOnExit<ClawTail>();
    }

    private void StarvingStampede(uint id, float delay)
    {
        Targetable(id, false, delay, "Jumps disappear")
            .ActivateOnEnter<StarvingStampede>();
        ComponentCondition<VenomTowers>(id + 1u, 0.9f, static comp => comp.Active)
            .ActivateOnEnter<VenomTowers>();
        Targetable(id + 2u, true, 9.7f, "Jumps reappear")
            .DeactivateOnExit<StarvingStampede>();
        ComponentCondition<VenomTowers>(id + 3u, 3.3f, static comp => !comp.Active, "Towers")
            .DeactivateOnExit<VenomTowers>();
        // note: if any players have failed the mechanic, this will be extended while he is eating people...
        ComponentCondition<DevourBait>(id + 0x10u, 2.3f, static comp => comp.NumCasts > 0, "Devour", 20f)
            .ActivateOnEnter<DevourBait>()
            .DeactivateOnExit<DevourBait>();
    }

    private void SonicShatter(uint id, float delay)
    {
        Cast(id, AID.SonicShatter, delay, 5f, "Raidwide hit 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 2u, 3.1f, static comp => comp.NumCasts >= 1, "Hit 2")
            .ActivateOnEnter<SonicShatter>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 3u, 3.1f, static comp => comp.NumCasts >= 2, "Hit 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 4u, 3.1f, static comp => comp.NumCasts >= 3, "Hit 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SonicShatter>(id + 5u, 3.1f, static comp => comp.NumCasts >= 4, "Hit 5")
            .DeactivateOnExit<SonicShatter>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
