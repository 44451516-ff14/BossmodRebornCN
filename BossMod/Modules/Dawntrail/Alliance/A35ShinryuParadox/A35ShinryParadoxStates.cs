namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

[SkipLocalsInit]
sealed class A35ShinryuParadoxStates : StateMachineBuilder
{
    readonly A35ShinryuParadox _module;

    public A35ShinryuParadoxStates(A35ShinryuParadox module) : base(module)
    {
        _module = module;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<FloorAOEs>()
            .ActivateOnEnter<P2ArenaChange>()
            .ActivateOnEnter<StarflareP1>()
            .ActivateOnEnter<VortexStayMove>()
            .ActivateOnEnter<VortexGaze>()
            .ActivateOnEnter<DarkNova>()
            .ActivateOnEnter<CelestialTrail>()
            .ActivateOnEnter<EmptyProclamation>()
            .ActivateOnEnter<Swordscross1>()
            .ActivateOnEnter<Swordscross2>()
            .ActivateOnEnter<TwinBlaze1>()
            .ActivateOnEnter<TwinBlaze2>()
            .ActivateOnEnter<CataclysmicBlade>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<CosmicFlame>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<GyreCharge>()
            .ActivateOnEnter<SuperNova>()
            .ActivateOnEnter<StarflareP2>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.BossP2 == null // wipe in P1
                 || (module.BossP2?.IsDeadOrDestroyed ?? false); // P2 end
    }

    private void SinglePhase(uint id)
    {
        UpDown(id, 7.2f);
        UpDown(id + 0x1000u, 8.7f);
        Twilight1(id + 0x2000u, 3.6f);
        Starflare1(id + 0x3000u, 4.5f);
        Vortex(id + 0x4000u, 5.8f);
        Twilight2(id + 0x5000u, 5.7f);
        Starflare2(id + 0x6000u, 3.5f);
        DarkNova(id + 0x7000u, 3.6f);

        Cast(id + 0x8000u, (uint)AID.AtomicTailVisual1, 6.5f, 6);
        Timeout(id + 0x8010u, 1f, "Ground floor disappears");
        ComponentCondition<GyreCharge>(id + 0x8020, 5.2f, static g => g.NumCasts > 0, "Raidwide + stun")
            .DeactivateOnExit<GyreCharge>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        P2(id + 0x10000u, 52.8f);
    }

    void UpDown(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.CosmicBreathVisual1, (uint)AID.CosmicTailVisual1], delay, 6f)
            .ActivateOnEnter<UpDownCounter>();
        ComponentCondition<UpDownCounter>(id + 0x10u, 1.1f, static d => d.NumCasts > 0, "Up/down")
            .DeactivateOnExit<UpDownCounter>();
    }

    void Twilight1(uint id, float delay)
    {
        Cast(id, (uint)AID.CloakOfTwilight1, delay, 3f);
        ComponentCondition<FloorAOEs>(id + 0x10u, 13.8f, static n => n.NumCasts > 0, "Light/dark")
            .ExecOnExit<FloorAOEs>(static comp => comp.NumCasts = 0);
    }

    void Starflare1(uint id, float delay)
    {
        Cast(id, (uint)AID.StarflareVisual1, delay, 3f)
            .ActivateOnEnter<UpDownCounter>();

        ComponentCondition<StarflareP1>(id + 0x10u, 5.1f, static s => s.NumCasts >= 10, "Lines 1");
        ComponentCondition<StarflareP1>(id + 0x20u, 2f, static s => s.NumCasts >= 20, "Lines 2")
            .ExecOnExit<StarflareP1>(static comp => comp.NumCasts = 0);
        ComponentCondition<UpDownCounter>(id + 0x30u, 4.5f, static c => c.NumCasts > 0, "Up/down")
            .DeactivateOnExit<UpDownCounter>();
    }

    void Vortex(uint id, float delay)
    {
        CastStart(id, (uint)AID.CataclysmicVortexVisual1, delay);
        CastEnd(id + 0x01u, 7u, "Stay/move/gaze");
    }

    void Twilight2(uint id, float delay)
    {
        Cast(id, (uint)AID.CloakOfTwilight1, delay, 3f);
        UpDown(id + 0x100u, 3.8f);
        ComponentCondition<FloorAOEs>(id + 0x200u, 8.7f, static n => n.NumCasts > 0, "Light/dark")
            .ExecOnExit<FloorAOEs>(static comp => comp.NumCasts = 0);
    }

    void Starflare2(uint id, float delay)
    {
        Cast(id, (uint)AID.StarflareVisual1, delay, 3f);
        CastStart(id + 0x10u, (uint)AID.CataclysmicVortexVisual1, 3.6f);

        ComponentCondition<StarflareP1>(id + 0x20u, 1.4f, static s => s.NumCasts >= 10, "Lines 1");
        ComponentCondition<StarflareP1>(id + 0x21u, 2f, static s => s.NumCasts >= 20, "Lines 2")
            .ExecOnExit<StarflareP1>(static comp => comp.NumCasts = 0);

        CastEnd(id + 0x30u, 3.6f, "Stay/move/gaze");
    }

    void DarkNova(uint id, float delay)
    {
        CastStart(id, (uint)AID.DarkNovaVisual1, delay);
        ComponentCondition<DarkNova>(id + 0x10u, 6.2f, static d => d.NumCasts > 0, "Tankbusters");
    }

    void P2(uint id, float delay)
    {
        ActorTargetable(id, _module.BossP2M, true, delay, "Boss reappears")
            .DeactivateOnEnter<FloorAOEs>()
            .DeactivateOnEnter<P2ArenaChange>()
            .DeactivateOnEnter<StarflareP1>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        CelestialTrail(id + 0x100u, 28.9f);
        EmptyProclamation(id + 0x1000u, 32.9f);
        Swordscross(id + 0x10000u, 3.1f);
        TwinBlaze(id + 0x11000u, 5.7f);
        CataclysmicBlade(id + 0x12000u, 6.2f);
        Burst(id + 0x13000u, 8.2f);
        CosmicFlame(id + 0x14000u, 3.2f);
        SuperNova(id + 0x15000u, 7.2f);
        EmptyProclamation(id + 0x16000u, 0.1f);

        Timeout(id + 0x20000u, 10000f, "Repeat mechanics until death");
    }

    void CelestialTrail(uint id, float delay)
    {
        ComponentCondition<CelestialTrail>(id, delay, static c => c.NumCasts >= 8, "Towers 1");
        ComponentCondition<CelestialTrail>(id + 0x10u, 19.3f, static c => c.NumCasts >= 16, "Towers 2")
            .DeactivateOnExit<CelestialTrail>();
    }

    void EmptyProclamation(uint id, float delay)
    {
        ActorCast(id, _module.BossP2M, (uint)AID.EmptyProclamation, delay, 4f, true, "Raidwide");
    }

    void Swordscross(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2M, [(uint)AID.RightSwordscrossVisual, (uint)AID.LeftSwordscrossVisual], delay, 8f, true);
        ComponentCondition<Swordscross1>(id + 0x10u, 1f, static s => s.NumCasts > 0, "Swords");
    }

    State TwinBlaze(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2M, [(uint)AID.TwinBlazeVisual1, (uint)AID.TwinBlazeVisual2], delay, 5f);
        return ComponentCondition<TwinBlaze1>(id + 0x10u, 1f, static t => t.NumCasts > 0, "In/out")
            .ExecOnExit<TwinBlaze1>(static comp => comp.NumCasts = 0);
    }

    State CataclysmicBlade(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2M, (uint)AID.CataclysmicBladeVisual, delay, true);
        return ComponentCondition<CataclysmicBlade>(id + 0x10, 7, static c => c.NumCasts > 0, "Cones + stay/move/gaze")
            .ExecOnExit<CataclysmicBlade>(static comp => comp.NumCasts = 0);
    }

    void Burst(uint id, float delay)
    {
        ActorCast(id, _module.BossP2M, (uint)AID.BurstVisual, delay, 3f);
        TwinBlaze(id + 0x100u, 7.2f);
    }

    void CosmicFlame(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2M, (uint)AID.CosmicFlameVisual, delay);
        ComponentCondition<CosmicFlame>(id + 0x10u, 5f, static f => f.NumCasts > 0, "Exaflares start");
        ActorCast(id + 0x100u, _module.BossP2M, (uint)AID.AtomicRayVisual, 7.2f, 3f, true);
        ComponentCondition<CosmicFlame>(id + 0x110u, 4.2f, static f => f.NumCasts >= 40, "Exaflares end");
        CataclysmicBlade(id + 0x200u, 8f);
    }

    void SuperNova(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2M, (uint)AID.SuperNovaVisual, delay, true);
        ComponentCondition<SuperNova>(id + 0x10u, 6.2f, static s => s.NumCasts > 0, "Stack 1");
        ComponentCondition<SuperNova>(id + 0x20u, 1.9f, static s => s.NumCasts >= 3, "Stack 3");
    }
}
