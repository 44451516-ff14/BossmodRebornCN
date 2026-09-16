namespace BossMod.Endwalker.Alliance.A33Oschon;

sealed class A33OschonStates : StateMachineBuilder
{
    private readonly A33Oschon _module;

    public A33OschonStates(A33Oschon module) : base(module)
    {
        _module = module;
        SimplePhase(0u, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.LoftyPeaks;
        SimplePhase(1u, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BossP2()?.HPMP.CurHP ?? 0u) <= 1u;
    }

    private void Phase1(uint id)
    {
        P1SuddenDownpour(id, 10.2f);
        P1TrekShot(id + 0x10000u, 4.4f);
        P1Reproduce(id + 0x20000u, 5.8f);
        P1Reproduce(id + 0x30000u, 2.4f);
        P1FlintedFoehn(id + 0x40000u, 2.3f);
        P1SoaringMinuet(id + 0x50000u, 1.4f);
        P1Arrow(id + 0x60000u, 3.2f);
        P1Reproduce(id + 0x70000u, 4.2f);
        P1SuddenDownpour(id + 0x80000u, 0.1f);
        P1DownhillClimbingShot(id + 0x90000u, 4.4f);
        P1FlintedFoehn(id + 0xA0000u, 3.2f);
        P1TrekShot(id + 0xB0000u, 1.3f);
        P1SuddenDownpour(id + 0xC0000u, 2.6f);
        P1SuddenDownpour(id + 0xD0000u, 1.1f);
        P1Reproduce(id + 0xE0000u, 7.4f);
        P1SuddenDownpour(id + 0xF0000u, 0.1f);
        P1DownhillClimbingShot(id + 0x100000u, 4.4f);
        P1FlintedFoehn(id + 0x110000u, 3.1f);
        P1TrekShot(id + 0x120000u, 1.4f);
        P1SuddenDownpour(id + 0x130000u, 2.6f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Phase2(uint id)
    {
        P2LoftyPeaks(id, 0f);
        P2PitonPull(id + 0x10000u, 5.2f);
        P2PitonPull(id + 0x20000u, 3.7f);
        P2Altitude(id + 0x30000u, 5.7f);
        P2FlintedFoehn(id + 0x40000u, 5.2f);
        P2WanderingShot(id + 0x50000u, 3.3f);
        P2WanderingShot(id + 0x60000u, 3.5f);
        P2Arrow(id + 0x70000u, 3.5f);
        P2ArrowTrail(id + 0x80000u, 5.2f);
        P2WanderingVolley(id + 0x90000u, 6.7f);
        P2FlintedFoehn(id + 0xA0000u, 5.7f);
        P2Arrow(id + 0xB0000u, 1.1f);
        P2Altitude(id + 0xC0000u, 2.2f);
        P2SuddenDownpour(id + 0xD0000u, 7.2f);
        P2SuddenDownpour(id + 0xE0000u, 5.1f);
        P2ArrowTrail(id + 0xF0000u, 7.2f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void P1SuddenDownpour(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.SuddenDownpour, delay, 4f, true);
        ComponentCondition<P1SuddenDownpour>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P1SuddenDownpour>()
            .DeactivateOnExit<P1SuddenDownpour>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1TrekShot(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP1, [AID.TrekShotN, AID.TrekShotS], delay, 6f, true)
            .ActivateOnEnter<P1TrekShot>();
        ComponentCondition<P1TrekShot>(id + 0x2u, 3.5f, static comp => comp.NumCasts > 0, "Cone")
            .DeactivateOnExit<P1TrekShot>();
    }

    private void P1Reproduce(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.Reproduce, delay, 3f, true);
        ComponentCondition<P1SwingingDraw>(id + 0x10u, 2.9f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<P1SwingingDraw>();
        ComponentCondition<P1SwingingDraw>(id + 0x20u, 13.2f, static comp => comp.NumCasts > 0, "Cone")
            .DeactivateOnExit<P1SwingingDraw>();
    }

    private void P1FlintedFoehn(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP1, AID.FlintedFoehnP1, delay, true)
            .ActivateOnEnter<P1FlintedFoehn>();
        ActorCastEnd(id + 1u, _module.BossP1, 4.5f, true);
        ComponentCondition<P1FlintedFoehn>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<P1FlintedFoehn>(id + 0x20u, 5.3f, static comp => comp.NumCasts > 5, "Stack 6")
            .DeactivateOnExit<P1FlintedFoehn>();
    }

    private void P1SoaringMinuet(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.SoaringMinuet1, delay, 5f, true, "Wide cone")
            .ActivateOnEnter<P1SoaringMinuet1>()
            .DeactivateOnExit<P1SoaringMinuet1>();
    }

    private void P1Arrow(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.ArrowP1, delay, 4f, true)
            .ActivateOnEnter<P1Arrow>();
        ComponentCondition<P1Arrow>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P1Arrow>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1DownhillClimbingShot(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.DownhillP1, delay, 3f, true);
        ComponentCondition<P1Downhill>(id + 0x10f, 0.4f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1Downhill>();
        ActorCastMulti(id + 0x20f, _module.BossP1, [AID.ClimbingShot1, AID.ClimbingShot2, AID.ClimbingShot3, AID.ClimbingShot4], 1.7f, 5f, true, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<P1ClimbingShot>()
            .DeactivateOnExit<P1ClimbingShot>();
        ComponentCondition<P1Downhill>(id + 0x30f, 1.8f, static comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P1Downhill>();
        ActorCast(id + 0x100f, _module.BossP1, AID.SoaringMinuet2, 1.3f, 5f, true, "Wide cone")
            .ActivateOnEnter<P1SoaringMinuet2>()
            .DeactivateOnExit<P1SoaringMinuet2>();
    }

    private void P2LoftyPeaks(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.LoftyPeaks, delay, 5f, true, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2MovingMountains>(id + 0x10u, 1.1f, static comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<P2MovingMountains>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2MovingMountains>(id + 0x11u, 1.5f, static comp => comp.NumCasts > 1, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2MovingMountains>(id + 0x12u, 1.5f, static comp => comp.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<P2MovingMountains>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2PeakPeril>(id + 0x13u, 3.9f, static comp => comp.NumCasts > 0, "Raidwide 4")
            .ActivateOnEnter<P2PeakPeril>()
            .DeactivateOnExit<P2PeakPeril>()
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsSquare(20f) { Y = 130f, BorderY = 130f })
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2Shockwave>(id + 0x20u, 15.7f, static comp => comp.NumCasts > 0, "Raidwide 5")
            .ActivateOnEnter<P2Shockwave>()
            .DeactivateOnExit<P2Shockwave>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x30u, _module.BossP2, true, 2f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P2PitonPull(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], delay, 8f, true)
            .ActivateOnEnter<P2PitonPull>();
        ComponentCondition<P2PitonPull>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }

    private void P2Altitude(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.Altitude, delay, 6f, true)
            .ActivateOnEnter<P2Altitude>();
        ComponentCondition<P2Altitude>(id + 2u, 1, static comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P2Altitude>();
    }

    private void P2FlintedFoehn(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.FlintedFoehnP2, delay, true)
            .ActivateOnEnter<P2FlintedFoehn>();
        ActorCastEnd(id + 1u, _module.BossP2, 4.5f, true);
        ComponentCondition<P2FlintedFoehn>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<P2FlintedFoehn>(id + 0x20u, 5.3f, static comp => comp.NumCasts > 5, "Stack 6")
            .DeactivateOnExit<P2FlintedFoehn>();
    }

    private void P2WanderingShot(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.WanderingShotN, AID.WanderingShotS], delay, 7, true)
            .ActivateOnEnter<P2WanderingShot>();
        ComponentCondition<P2WanderingShot>(id + 0x10u, 3.6f, static comp => comp.NumCasts > 0, "N/S circle")
            .DeactivateOnExit<P2WanderingShot>();
    }

    private void P2Arrow(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.ArrowP2, delay, 6f, true)
            .ActivateOnEnter<P2Arrow>();
        ComponentCondition<P2Arrow>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P2Arrow>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2ArrowTrail(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.ArrowTrail, delay, 3f, true);
        ComponentCondition<P2ArrowTrail>(id + 0x10f, 2.4f, static comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<P2ArrowTrail>();

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x20u, 5.7f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2DownhillArrowTrailDownhill>();
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x21u, 3f, static comp => comp.NumCasts > 0, "Puddles 1");

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x30u, 1f, static comp => comp.Casters.Count > 0);
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x31u, 3f, static comp => comp.NumCasts > 3, "Puddles 2");

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x40u, 1f, static comp => comp.Casters.Count > 0);
        ActorCastStartMulti(id + 0x41u, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], 2.1f, true);
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x42u, 0.9f, static comp => comp.NumCasts > 6, "Puddles 3")
            .ActivateOnEnter<P2PitonPull>()
            .DeactivateOnExit<P2DownhillArrowTrailDownhill>();
        ComponentCondition<P2ArrowTrail>(id + 0x43u, 0.8f, static comp => comp.NumCasts >= 64, "Exaflares resolve")
            .DeactivateOnExit<P2ArrowTrail>();
        ActorCastEnd(id + 0x44u, _module.BossP2, 6.2f, true);
        ComponentCondition<P2PitonPull>(id + 0x45u, 0.5f, static comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }

    private void P2WanderingVolley(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.WanderingVolleyDownhill, delay, 3f, true);
        ComponentCondition<P2WanderingVolleyDownhill>(id + 0x10u, 0.5f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2WanderingVolleyDownhill>();
        ActorCastMulti(id + 0x20u, _module.BossP2, [AID.WanderingVolleyN, AID.WanderingVolleyS], 2.7f, 10f, true, "Knockback sides")
            .ActivateOnEnter<P2WanderingVolleyKnockback>()
            .ActivateOnEnter<P2WanderingVolleyAOE>()
            .ActivateOnEnter<P2WanderingShot>()
            .DeactivateOnExit<P2WanderingVolleyKnockback>()
            .DeactivateOnExit<P2WanderingVolleyAOE>();
        ComponentCondition<P2WanderingVolleyDownhill>(id + 0x30u, 1.3f, static comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P2WanderingVolleyDownhill>();
        ActorCastStartMulti(id + 0x40u, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], 1.9f, true);
        ComponentCondition<P2WanderingShot>(id + 0x41u, 0.5f, static comp => comp.NumCasts > 0, "N/S circle")
            .ActivateOnEnter<P2PitonPull>()
            .DeactivateOnExit<P2WanderingShot>();
        ActorCastEnd(id + 0x42u, _module.BossP2, 7.5f, true);
        ComponentCondition<P2PitonPull>(id + 0x43u, 0.5f, static comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }

    private void P2SuddenDownpour(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.P2SuddenDownpour, delay, 4f, true);
        ComponentCondition<P2SuddenDownpour>(id + 2u, 1f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P2SuddenDownpour>()
            .DeactivateOnExit<P2SuddenDownpour>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
