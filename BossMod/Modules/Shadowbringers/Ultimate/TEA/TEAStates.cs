namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class TEAStates : StateMachineBuilder
{
    private readonly TEA _module;

    public TEAStates(TEA module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1LivingLiquid, "P1: Living Liquid")
            .ActivateOnEnter<P1HandOfPain>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed; // phase 1 ends either with wipe (everything destroyed) or success (boss dies, then is destroyed few seconds into next phase)
        SimplePhase(1u, Phase2BruteJusticeCruiseChaser, "P2: BJ+CC")
            .Raw.Update = () =>
            {
                var bj = _module.BruteJustice();
                var cc = _module.CruiseChaser();
                if (bj == null || cc == null)
                    return true; // shouldn't happen - BJ & CC should exist from the start
                if (Module.PrimaryActor.IsDestroyed && bj.IsDestroyed && cc.IsDestroyed)
                    return true; // wipe in p1/p2 => all actors are destroyed; BJ/CC are not destroyed otherwise
                // BJ/CC start untargetable, then both become targetable after intermission and stay targetable until the end
                // when either reaches 1hp, it becomes untargetable, and the remaining one starts casting enrage
                // some time after becoming untargetable, BJ/CC get healed to full - so the pass condition is: both untargetable and at least one at non-full hp (this prevents triggering during intermission)
                return !bj.IsTargetable && !cc.IsTargetable && (bj.HPMP.CurHP < bj.HPMP.MaxHP || cc.HPMP.CurHP < cc.HPMP.MaxHP);
            };
        SimplePhase(2u, Phase3AlexanderPrime, "P3: Alex Prime")
            .Raw.Update = () =>
            {
                var alex = _module.AlexPrime();
                if (alex == null)
                    return true; // shouldn't happen - alex should exist from the start
                if (Module.PrimaryActor.IsDestroyed && alex.IsDestroyed)
                    return true; // wipe in p1/p2/p3 => all actors are destroyed; alex is not destroyed otherwise
                // alex swaps between targetable and untargetable state; consider that state transition happens at 1hp when enrage cast is interrupted
                return alex.HPMP.CurHP <= 1 && alex.CastInfo == null;
            };
        SimplePhase(3u, Phase4PerfectAlexander, "P4: Perfect Alex")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.PerfectAlex()?.IsDestroyed ?? true) || (_module.PerfectAlex()?.IsDead ?? false); // perfect alex should not be destroyed until either wipe or kill
    }

    private void Phase1LivingLiquid(uint id)
    {
        P1FluidSwing(id, 10.2f);
        P1Cascade(id + 0x10000u, 4.1f);
        P1HandsProteansDollsCleaves(id + 0x20000u, 11.1f);
        P1ProteansBoss(id + 0x30000u, 10.2f);
        P1SplashDrainageCascade(id + 0x40000u, 6.1f);
        P1Throttle(id + 0x50000u, 4.6f);
        P1ProteansBoth(id + 0x60000u, 7.7f);
        P1PainSplashCleaves(id + 0x70000u, 5.1f);
        ActorCast(id + 0x80000u, _module.BossP1, (uint)AID.Enrage, 3.1f, 4f, true, "Enrage");
    }

    private void P1FluidSwing(uint id, float delay)
    {
        ComponentCondition<P1FluidSwing>(id, delay, comp => comp.NumCasts > 0, "Cleave")
            .ActivateOnEnter<P1FluidSwing>()
            .DeactivateOnExit<P1FluidSwing>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // keeps cascade component active
    private void P1Cascade(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, (uint)AID.Cascade, delay, 4f, true, "Raidwide")
            .ActivateOnEnter<P1Cascade>()
            .ActivateOnEnter<P1Embolus>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State P1HandOfPain(uint id, float delay, int seq)
    {
        return ComponentCondition<P1HandOfPain>(id, delay, comp => comp.NumCasts >= seq, $"HP check {seq}");
    }

    private void P1HandsProteansDollsCleaves(uint id, float delay)
    {
        Condition(id, delay, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
        ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 1, 3.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1HandOfPartingPrayer>()
            .ActivateOnEnter<P1ProteanWaveTornadoVisBait>()
            .ActivateOnEnter<P1ProteanWaveTornadoVisCast>()
            .DeactivateOnExit<P1ProteanWaveTornadoVisBait>();
        ComponentCondition<P1JagdDolls>(id + 2u, 1f, comp => comp.Active)
            .ActivateOnEnter<P1JagdDolls>();
        ComponentCondition<P1HandOfPartingPrayer>(id + 3u, 1f, comp => comp.Resolved, "Resolve")
            .DeactivateOnExit<P1HandOfPartingPrayer>();
        ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x10u, 1f, comp => comp.Casters.Count == 0)
            .ActivateOnEnter<P1FluidStrike>()
            .ActivateOnEnter<P1FluidSwing>()
            .DeactivateOnExit<P1ProteanWaveTornadoVisCast>();
        ComponentCondition<P1FluidSwing>(id + 0x20u, 1.1f, comp => comp.NumCasts > 0, "Cleaves")
            .ActivateOnEnter<P1ProteanWaveTornadoInvis>()
            .DeactivateOnExit<P1FluidStrike>()
            .DeactivateOnExit<P1FluidSwing>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P1ProteanWaveTornadoInvis>(id + 0x30u, 1f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<P1ProteanWaveTornadoInvis>();
        ComponentCondition<P1JagdDolls>(id + 0x100u, 1f, comp => comp.NumExhausts > 0, "Exhaust 1");
        // +0.1s: hand of pain start
        // +2.0s: pressurize
        // +2.6s: embolus spawn
        P1HandOfPain(id + 0x200u, 3.1f, 1);
        ComponentCondition<P1JagdDolls>(id + 0x300u, 7.5f, comp => comp.NumExhausts > 1, "Exhaust 2", 0.1f);
        ComponentCondition<P1FluidSwing>(id + 0x400u, 6.4f, comp => comp.NumCasts > 0, "Cleaves")
            .ActivateOnEnter<P1FluidStrike>()
            .ActivateOnEnter<P1FluidSwing>()
            .DeactivateOnExit<P1FluidStrike>()
            .DeactivateOnExit<P1FluidSwing>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1ProteansBoss(uint id, float delay)
    {
        ActorCast(id + 1u, _module.BossP1, (uint)AID.ProteanWaveLiquidVisBoss, delay, 3f, true, "Protean baited")
            .ActivateOnEnter<P1ProteanWaveLiquid>()
            .DeactivateOnExit<P1ProteanWaveLiquid>();
        P1HandOfPain(id + 0x10u, 0.2f, 2)
            .ActivateOnEnter<P1ProteanWaveLiquidInvisFixed>()
            .ActivateOnEnter<P1ProteanWaveLiquidInvisBaited>()
            .ActivateOnEnter<P1Sluice>();
        ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x20u, 1.9f, comp => comp.NumCasts > 0, "Protean 1");
        ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x30u, 3.0f, comp => comp.NumCasts > 1, "Protean 2")
            .DeactivateOnExit<P1ProteanWaveLiquidInvisFixed>()
            .DeactivateOnExit<P1ProteanWaveLiquidInvisBaited>()
            .DeactivateOnExit<P1Sluice>();
    }

    private void P1SplashDrainageCascade(uint id, float delay)
    {
        ComponentCondition<P1Splash>(id, delay, comp => comp.NumCasts > 0, "Splash start")
            .ActivateOnEnter<P1Splash>()
            .ActivateOnEnter<P1Drainage>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 1u, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 2u, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 3u, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 4u, 1.1f, comp => comp.NumCasts > 4).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 5u, 1.1f, comp => comp.NumCasts > 5) // drainage resolves almost at the same time
            .DeactivateOnExit<P1Splash>()
            .DeactivateOnExit<P1Drainage>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x100u, _module.BossP1, (uint)AID.Cascade, 1.1f, true);
        P1HandOfPain(id + 0x101u, 1.3f, 3);
        ActorCastEnd(id + 0x102u, _module.BossP1, 2.7f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1Throttle(uint id, float delay)
    {
        ComponentCondition<P1Throttle>(id, delay, comp => comp.Applied, "Debuffs")
            .ActivateOnEnter<P1Throttle>()
            .DeactivateOnExit<P1Throttle>();
    }

    private void P1ProteansBoth(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, (uint)AID.ProteanWaveLiquidVisBoss, delay, 3f, true, "Protean baited")
            .ActivateOnEnter<P1ProteanWaveLiquid>()
            .DeactivateOnExit<P1ProteanWaveLiquid>();
        P1HandOfPain(id + 0x10, 2, 4)
            .ActivateOnEnter<P1ProteanWaveLiquidInvisFixed>()
            .ActivateOnEnter<P1ProteanWaveLiquidInvisBaited>()
            .ActivateOnEnter<P1Sluice>()
            .ActivateOnEnter<P1ProteanWaveTornadoVisBait>()
            .ActivateOnEnter<P1ProteanWaveTornadoVisCast>();
        ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x11u, 0.1f, comp => comp.NumCasts > 0, "Protean 1");
        ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x12u, 0.9f, comp => comp.Casters.Count > 0)
            .DeactivateOnExit<P1ProteanWaveTornadoVisBait>();
        ComponentCondition<P1ProteanWaveLiquidInvisFixed>(id + 0x20u, 2.1f, comp => comp.NumCasts > 1, "Protean 2")
            .DeactivateOnExit<P1ProteanWaveLiquidInvisFixed>()
            .DeactivateOnExit<P1ProteanWaveLiquidInvisBaited>()
            .DeactivateOnExit<P1Sluice>();
        ComponentCondition<P1ProteanWaveTornadoVisCast>(id + 0x21u, 0.8f, comp => comp.Casters.Count == 0)
            .DeactivateOnExit<P1ProteanWaveTornadoVisCast>();
        ComponentCondition<P1ProteanWaveTornadoInvis>(id + 0x30u, 2.1f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<P1ProteanWaveTornadoInvis>()
            .DeactivateOnExit<P1ProteanWaveTornadoInvis>();
        // +3.9s: pressurize
        // +4.5s: embolus spawn
        Condition(id + 0x40u, 3.9f, () => (_module.LiquidHand()?.ModelState.ModelState ?? 0) != 0, "Hand of parting/prayer bait");
        ComponentCondition<P1HandOfPartingPrayer>(id + 0x41u, 5.1f, comp => comp.Resolved, "Resolve")
            .ActivateOnEnter<P1HandOfPartingPrayer>()
            .DeactivateOnExit<P1HandOfPartingPrayer>();
    }

    private void P1PainSplashCleaves(uint id, float delay)
    {
        P1HandOfPain(id + 1u, delay, 5)
            .DeactivateOnExit<P1HandOfPain>();
        ComponentCondition<P1Splash>(id + 0x10u, 0.3f, comp => comp.NumCasts > 0, "Splash start")
            .ActivateOnEnter<P1Splash>()
            .ActivateOnEnter<P1FluidSwing>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 0x11u, 1.1f, comp => comp.NumCasts > 1).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 0x12u, 1.1f, comp => comp.NumCasts > 2).SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1Splash>(id + 0x13u, 1.1f, comp => comp.NumCasts > 3).SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<P1Splash>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P1FluidSwing>(id + 0x20u, 1.2f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<P1FluidSwing>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Phase2BruteJusticeCruiseChaser(uint id)
    {
        P2Intermission(id, 0f);
        P2WhirlwindDebuffs(id + 0x10000u, 5.2f);
        P2ChakramOpticalSightPhoton(id + 0x20000u, 6f);
        P2SpinCrusherCompressedWaterLightning(id + 0x30000u, 6.9f);
        P2MissileCommand(id + 0x40000u, 4.3f);
        P2VerdictGavel(id + 0x50000u, 1.4f);
        P2PhotonDoubleRocketPunch(id + 0x60000u, 5.5f);
        P2SuperJumpApocalypticRay(id + 0x70000u, 3.2f);
        P2Whirlwind(id + 0x80000u, 1.5f);
        P2Whirlwind(id + 0x90000u, 4.2f);
        SimpleState(id + 0xA0000u, 10, "Enrage"); // TODO: timing; i think there are no more mechanics here?
    }

    private void P2Intermission(uint id, float delay)
    {
        Timeout(id, delay)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2IntermissionHawkBlaster>(id + 0x10u, 5.2f, comp => comp.NumCasts > 0, "First aoe")
            .ActivateOnEnter<P2IntermissionLimitCut>()
            .ActivateOnEnter<P2IntermissionHawkBlaster>();
        ComponentCondition<P2IntermissionLimitCut>(id + 0x20u, 7.4f, comp => comp.NumCasts >= 1, "Hit 1");
        ComponentCondition<P2IntermissionLimitCut>(id + 0x30u, 4.6f, comp => comp.NumCasts >= 3, "Hit 3");
        ComponentCondition<P2IntermissionLimitCut>(id + 0x40u, 4.6f, comp => comp.NumCasts >= 5, "Hit 5");
        ComponentCondition<P2IntermissionLimitCut>(id + 0x50u, 4.6f, comp => comp.NumCasts >= 7, "Hit 7");
        ComponentCondition<P2JKick>(id + 0x100u, 4.6f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<P2JKick>()
            .DeactivateOnExit<P2IntermissionHawkBlaster>()
            .DeactivateOnExit<P2IntermissionLimitCut>()
            .DeactivateOnExit<P2JKick>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x101u, _module.BruteJustice, true, 3f, "Intermission end")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    // keeps nisi component active
    private void P2WhirlwindDebuffs(uint id, float delay)
    {
        ActorCastStart(id, _module.CruiseChaser, (uint)AID.Whirlwind, delay);
        ActorCastStart(id + 1u, _module.BruteJustice, (uint)AID.JudgmentNisi, 3f);
        ActorCastEnd(id + 2u, _module.CruiseChaser, 1f, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 3u, _module.BruteJustice, 3f, false, "Nisi")
            .ActivateOnEnter<P2Nisi>(); // debuffs are applied ~0.8s after cast end
        ActorCast(id + 0x10u, _module.BruteJustice, (uint)AID.LinkUp, 3.2f, 3f, false, "Debuffs")
            .ActivateOnEnter<P2CompressedWaterLightning>(); // debuffs & icons are applied ~0.8s after cast end
    }

    private void P2ChakramOpticalSightPhoton(uint id, float delay)
    {
        ActorCastStart(id, _module.CruiseChaser, (uint)AID.OpticalSight, delay)
            .ActivateOnEnter<P2EyeOfTheChakram>();
        ActorCastEnd(id + 1u, _module.CruiseChaser, 2f);
        ComponentCondition<P2EyeOfTheChakram>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Chakrams")
            .ActivateOnEnter<P2HawkBlasterOpticalSight>()
            .DeactivateOnExit<P2EyeOfTheChakram>();
        ActorCastStart(id + 0x10u, _module.CruiseChaser, (uint)AID.Photon, 3.3f);
        ComponentCondition<P2HawkBlasterOpticalSight>(id + 0x11u, 1.1f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P2HawkBlasterOpticalSight>();
        ActorCastEnd(id + 0x12u, _module.CruiseChaser, 1.9f)
            .ExecOnEnter<P2Nisi>(comp => comp.ShowPassHint = 1) // first nisi pass should happen around photon cast end
            .ExecOnExit<P2CompressedWaterLightning>(comp => comp.ResolveImminent = true); // should start moving to debuff stacks after nisi pass
        ComponentCondition<P2Photon>(id + 0x13u, 0.3f, comp => comp.NumCasts > 0, "Photon")
            .ActivateOnEnter<P2Photon>()
            .DeactivateOnExit<P2Photon>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    // keeps tornado component active
    private void P2SpinCrusherCompressedWaterLightning(uint id, float delay)
    {
        ActorCast(id, _module.CruiseChaser, (uint)AID.SpinCrusher, delay, 3f, false, "Baited cleave")
            .ActivateOnEnter<P2SpinCrusher>()
            .DeactivateOnExit<P2SpinCrusher>();
        ComponentCondition<P2CompressedWaterLightning>(id + 0x10u, 4.6f, comp => !comp.ResolveImminent, "Water/lightning 1")
            .ActivateOnEnter<P2Drainage>(); // tornado spawns ~1s after resolve
        // +0.6s: vulns applied to previous stack targets
        // +0.7s: icons/debuffs applied to next stack targets
        // +0.8s: first sets of nisis expire
    }

    private void P2MissileCommand(uint id, float delay)
    {
        ActorCast(id, _module.BruteJustice, (uint)AID.MissileCommand, delay, 3f);
        ComponentCondition<P2EarthMissileBaited>(id + 0x10u, 1.2f, comp => comp.HaveCasters, "Bait missiles")
            .ActivateOnEnter<P2EarthMissileBaited>();
        ComponentCondition<P2Enumeration>(id + 0x20u, 1.9f, comp => comp.Active)
            .ActivateOnEnter<P2Enumeration>();
        ComponentCondition<P2HiddenMinefield>(id + 0x30u, 0.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2HiddenMinefield>();
        ComponentCondition<P2EarthMissileBaited>(id + 0x40u, 1.0f, comp => !comp.HaveCasters); // voidzones appear at cast positions with a slight delay
        ComponentCondition<P2HiddenMinefield>(id + 0x50u, 2.0f, comp => comp.Casters.Count == 0);
        ComponentCondition<P2Enumeration>(id + 0x60u, 2.1f, comp => !comp.Active, "Enumerations + Ice")
            .ActivateOnEnter<P2EarthMissileIce>()
            .DeactivateOnExit<P2Enumeration>();
        ComponentCondition<P2EarthMissileIce>(id + 0x70u, 0.8f, comp => comp.Sources(Module).Any());
        // +4.0s: ice voidzone grows
        // +5.6s: gelid gaol spawns where tornado is (assuming it is in ice voidzone)
        // +6.3s: tornado is destroyed
        // +6.3s: if any mine is not soaked, they explode now
        // +6.8s: smaller ice voidzone disappears (eventstate 7)
        // +7.7s: fire voidzones disappear (eventstate 7)
        ComponentCondition<P2EarthMissileIce>(id + 0x80u, 9.8f, comp => !comp.Sources(Module).Any(), "Voidzones disappear")
            .ExecOnEnter<P2Nisi>(comp => comp.ShowPassHint = 2) // second nisi pass should happen after enumerations are resolved
            .ExecOnEnter<P2CompressedWaterLightning>(comp => comp.ResolveImminent = true) // should start moving to debuff stacks after nisi pass
            .DeactivateOnExit<P2Drainage>()
            .DeactivateOnExit<P2EarthMissileBaited>()
            .DeactivateOnExit<P2HiddenMinefield>()
            .DeactivateOnExit<P2EarthMissileIce>();
    }

    private void P2VerdictGavel(uint id, float delay)
    {
        ActorCastStart(id, _module.BruteJustice, (uint)AID.Verdict, delay);
        ComponentCondition<P2CompressedWaterLightning>(id + 0x10u, 2.2f, comp => !comp.ResolveImminent, "Water/lightning 2")
            .ActivateOnEnter<P2Drainage>();
        ActorCastEnd(id + 0x20u, _module.BruteJustice, 1.8f); // judgment debuffs appear ~0.8s after cast end

        ActorCast(id + 0x100u, _module.CruiseChaser, (uint)AID.LimitCutP2, 3.2f, 2f, false, "CC invuln") // note: BJ starts flarethrower cast together with CC; invuln is applied ~0.6s after cast end
            .ActivateOnEnter<P2Flarethrower>()
            .ActivateOnEnter<P2CCInvincible>();
        ActorCastEnd(id + 0x102u, _module.BruteJustice, 1.9f)
            .ActivateOnEnter<P2PlasmaShield>();
        ComponentCondition<P2Flarethrower>(id + 0x103u, 0.3f, comp => comp.NumCasts > 0, "Baited flamethrower")
            .DeactivateOnExit<P2Flarethrower>() // note: tornado is normally destroyed by a flarethrower, failing to do that will cause tornado to wipe the raid later
            .ExecOnExit<P2Nisi>(comp => comp.ShowPassHint = 3) // third nisi pass should happen after flarethrower bait
            .ExecOnExit<P2CompressedWaterLightning>(comp => comp.ResolveImminent = true); // resolve stacks after nisi pass
        ActorCast(id + 0x110u, _module.CruiseChaser, (uint)AID.Whirlwind, 8.0f, 4f, false, "Raidwide")
            .DeactivateOnExit<P2PlasmaShield>() // it's a wipe if shield is not dealth with in time
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<P2CompressedWaterLightning>(id + 0x200u, 8.7f, comp => !comp.ResolveImminent, "Water/lightning 3")
            .DeactivateOnExit<P2CompressedWaterLightning>()
            .DeactivateOnExit<P2CCInvincible>()
            .ExecOnExit<P2Nisi>(comp => comp.ShowPassHint = 4); // fourth nisi pass should happen after last stacks, while resolving propeller wind

        ActorCastStart(id + 0x300u, _module.CruiseChaser, (uint)AID.PropellerWind, 12.5f);
        ActorCastStart(id + 0x301u, _module.BruteJustice, (uint)AID.Gavel, 3f)
            .ActivateOnEnter<P2PropellerWind>();
        ActorCastEnd(id + 0x302u, _module.CruiseChaser, 3f, false, "LOS")
            .DeactivateOnExit<P2PropellerWind>();
        ActorCastEnd(id + 0x303u, _module.BruteJustice, 2f, false, "Gavel");
        ComponentCondition<P2Nisi>(id + 0x304u, 2f, comp => comp.NumActiveNisi == 0, "Nisi resolve")
            .DeactivateOnExit<P2Nisi>()
            .DeactivateOnExit<P2Drainage>();
    }

    private void P2PhotonDoubleRocketPunch(uint id, float delay)
    {
        ActorCast(id, _module.CruiseChaser, (uint)AID.Photon, delay, 3f);
        ComponentCondition<P2Photon>(id + 2u, 0.3f, comp => comp.NumCasts > 0, "Photon")
            .ActivateOnEnter<P2Photon>()
            .DeactivateOnExit<P2Photon>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCast(id + 0x100u, _module.BruteJustice, (uint)AID.DoubleRocketPunch, 3.4f, 4f, false, "Shared tankbuster")
            .ActivateOnEnter<P2DoubleRocketPunch>()
            .DeactivateOnExit<P2DoubleRocketPunch>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2SuperJumpApocalypticRay(uint id, float delay)
    {
        ActorCast(id, _module.BruteJustice, (uint)AID.SuperJump, delay, 3.9f)
            .ActivateOnEnter<P2SuperJump>();
        ComponentCondition<P2SuperJump>(id + 2u, 0.4f, comp => comp.NumCasts > 0, "Jump")
            .DeactivateOnExit<P2SuperJump>();

        ComponentCondition<P2ApocalypticRay>(id + 0x10u, 2.3f, comp => comp.Source != null)
            .ActivateOnEnter<P2ApocalypticRay>();
        ComponentCondition<P2ApocalypticRay>(id + 0x15u, 4.9f, comp => comp.NumCasts >= 5, "AOE end")
            .DeactivateOnExit<P2ApocalypticRay>();
    }

    private void P2Whirlwind(uint id, float delay)
    {
        ActorCast(id, _module.CruiseChaser, (uint)AID.Whirlwind, delay, 4f, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Phase3AlexanderPrime(uint id)
    {
        P3TemporalStasis(id, 0f);
        P3ChasteningHeat(id + 0x10000u, 5.1f, false);
        P3InceptionFormation(id + 0x20000u, 5.3f);
        P3ChasteningHeat(id + 0x30000u, 5.1f, true);
        P3WormholeFormation(id + 0x40000u, 5.2f);
        P3MegaHoly(id + 0x50000u, 4.3f);
        P3SummonAlexander(id + 0x60000u, 6.4f);
    }

    private void P3TemporalStasis(uint id, float delay)
    {
        ActorTargetable(id, _module.AlexPrime, false, delay)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorCast(id + 0x10u, _module.AlexPrime, (uint)AID.TemporalStasis, 7.5f, 8f, true)
            .ActivateOnEnter<P3TemporalStasis>();
        ComponentCondition<P3TemporalStasis>(id + 0x20u, 1.2f, comp => comp.Frozen, "Temporal stasis");
        ComponentCondition<P3TemporalStasis>(id + 0x30u, 6.4f, comp => comp.NumCasts >= 2)
            .DeactivateOnExit<P3TemporalStasis>();
        ActorTargetable(id + 0x40u, _module.AlexPrime, true, 3.7f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P3ChasteningHeat(uint id, float delay, bool longDivineSpearDelay)
    {
        ActorCast(id, _module.AlexPrime, (uint)AID.ChasteningHeat, delay, 5f, true, "Tankbuster (vuln)")
            .ActivateOnEnter<P3ChasteningHeat>()
            .DeactivateOnExit<P3ChasteningHeat>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3DivineSpear>(id + 0x10u, longDivineSpearDelay ? 5.2f : 3.2f, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<P3DivineSpear>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3DivineSpear>(id + 0x11u, 2.1f, comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3DivineSpear>(id + 0x12u, 2.1f, comp => comp.NumCasts >= 3, "Tankbuster (cones)")
            .DeactivateOnExit<P3DivineSpear>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P3InceptionFormation(uint id, float delay)
    {
        ActorCast(id, _module.AlexPrime, (uint)AID.InceptionFormation, delay, 4f, true);
        ActorTargetable(id + 0x10u, _module.AlexPrime, false, 3.1f, "Inception formation")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P3Inception1>(id + 0x20u, 4.2f, comp => comp.AllSpheresSpawned)
            .ActivateOnEnter<P3Inception1>()
            .ActivateOnEnter<P3Tetrashatter>()
            .ActivateOnEnter<P3Inception2>(); // note: activated early, since spheres potentially could be intercepted early, and we track their casts to start showing hints
        ActorCast(id + 0x30u, _module.AlexPrime, (uint)AID.JudgmentCrystal, 4.2f, 3f, true);
        // +0.7s: remaining 4 players get icon 96
        ComponentCondition<P3Inception1>(id + 0x40u, 5.8f, comp => comp.CrystalsDone, "Crystals");
        ActorTargetable(id + 0x50u, _module.TrueHeart, true, 0.7f)
            .ActivateOnEnter<P3TrueHeart>();

        ComponentCondition<P3Inception2>(id + 0x100u, 10.6f, comp => comp.NumCasts >= 1, "Bait 1")
            .DeactivateOnExit<P3Inception1>();
        ComponentCondition<P3Inception2>(id + 0x101u, 2.2f, comp => comp.NumCasts >= 2, "Bait 2");
        ComponentCondition<P3Inception2>(id + 0x102u, 2.2f, comp => comp.NumCasts >= 3, "Bait 3")
            .DeactivateOnExit<P3Tetrashatter>()
            .DeactivateOnExit<P3Inception2>();

        // debuffs (restraining x2, aggravated x2, shared) appear right before cast start
        ActorCast(id + 0x200u, _module.AlexPrime, (uint)AID.Inception, 2.2f, 5f, true)
            .ActivateOnEnter<P3Inception3Sacrament>()
            .ActivateOnEnter<P3Inception3EarlyHints>()
            .ActivateOnEnter<P3Inception4Hints>(); // component self-activates when all debuffs are applied
        Condition(id + 0x208u, 4.0f, () => _module.TrueHeart()?.IsDead ?? true, "Heart disappears")
            .DeactivateOnExit<P3TrueHeart>();
        ComponentCondition<P3Inception3Sacrament>(id + 0x210u, 4.3f, comp => comp.NumCasts > 0, "Shared sentence")
            .ActivateOnEnter<P3Inception3Debuffs>()
            .DeactivateOnExit<P3Inception3Debuffs>() // note: debuffs resolve ~0.3s before sacrament
            .DeactivateOnExit<P3Inception3Sacrament>()
            .DeactivateOnExit<P3Inception3EarlyHints>();

        ActorCastStart(id + 0x300u, _module.BruteJustice, (uint)AID.SuperJump, 5.1f)
            .ActivateOnEnter<P2SuperJump>()
            .ActivateOnEnter<P3Inception4Cleaves>();
        ComponentCondition<P3Inception4Cleaves>(id + 0x301u, 0.9f, comp => comp.NumCasts >= 1)
            .DeactivateOnExit<P3Inception4Hints>();
        ComponentCondition<P3Inception4Cleaves>(id + 0x302u, 1.1f, comp => comp.NumCasts >= 2);
        ComponentCondition<P3Inception4Cleaves>(id + 0x303u, 1.1f, comp => comp.NumCasts >= 3)
            .DeactivateOnExit<P3Inception4Cleaves>();
        ActorCastEnd(id + 0x304u, _module.BruteJustice, 0.8f);
        ComponentCondition<P2SuperJump>(id + 0x305u, 0.3f, comp => comp.NumCasts > 0, "Jump")
            .DeactivateOnExit<P2SuperJump>();

        ActorTargetable(id + 0x400u, _module.AlexPrime, true, 2.1f, "Inception resolve")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P3WormholeFormation(uint id, float delay)
    {
        ActorCast(id, _module.AlexPrime, (uint)AID.WormholeFormation, delay, 4f);
        ActorTargetable(id + 0x10u, _module.AlexPrime, false, 3.1f, "Wormhole formation")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        // +2.2s: PATE 1E43 on alex & bj & cc in their spots
        // +3.4s: CC starts casting limit cut (to 5.4)
        // +5.4s: BJ starts casting link-up (to 8.4)
        ActorCast(id + 0x100u, _module.AlexPrime, (uint)AID.VoidOfRepentance, 8.4f, 3f, true)
            .ActivateOnEnter<P3WormholeLimitCut>() // icons appear ~0.7s after cast start
            .ActivateOnEnter<P3WormholeRepentance>()
            .ActivateOnEnter<P2EyeOfTheChakram>(); // chakrams start casts around cast end
        // +1.0s: wormholes appear
        // +1.8s: eobjanim 00010002 on wormholes
        ActorCastStart(id + 0x110u, _module.BruteJustice, (uint)AID.SuperJump, 3.2f);
        ComponentCondition<P2EyeOfTheChakram>(id + 0x120u, 2.7f, comp => comp.NumCasts > 0, "Chakrams")
            .ActivateOnEnter<P2SuperJump>() // TODO: reconsider activation time?..
            .DeactivateOnExit<P2EyeOfTheChakram>();
        ActorCastEnd(id + 0x130u, _module.BruteJustice, 1.1f);
        // +0.2s: alpha sword @ 1
        ComponentCondition<P2SuperJump>(id + 0x140u, 0.3f, comp => comp.NumCasts > 0, "Jump")
            .DeactivateOnExit<P2SuperJump>();

        ActorCastStart(id + 0x200u, _module.AlexPrime, (uint)AID.SacramentWormhole, 0.8f, true)
            .ActivateOnEnter<P3ApocalypticRay>();
        // +0.6s: blasty charge @ 2
        ComponentCondition<P3ApocalypticRay>(id + 0x210u, 1.6f, comp => comp.Source != null)
            .ActivateOnEnter<P3WormholeSacrament>();
        // +1.8s: alpha sword @ 3
        ComponentCondition<P3WormholeRepentance>(id + 0x220u, 2.1f, comp => comp.NumSoaks >= 1, "Wormhole 1");
        // +1.2s: blasty charge @ 4
        ActorCastEnd(id + 0x230u, _module.AlexPrime, 2.3f, true)
            .DeactivateOnExit<P3WormholeSacrament>();
        ComponentCondition<P3ApocalypticRay>(id + 0x240u, 0.5f, comp => comp.NumCasts >= 5)
            .DeactivateOnExit<P3ApocalypticRay>();
        // +1.2s: alpha sword @ 5
        ComponentCondition<P3WormholeRepentance>(id + 0x250u, 1.5f, comp => comp.NumSoaks >= 2, "Wormhole 2");
        // +1.2s: blasty charge @ 6
        // +3.8s: BJ starts missile command
        // +3.9s: alpha sword @ 7
        ComponentCondition<P3WormholeRepentance>(id + 0x260u, 4.2f, comp => comp.NumSoaks >= 3, "Wormhole 3")
            .DeactivateOnExit<P3WormholeRepentance>();

        ActorCastStart(id + 0x300u, _module.AlexPrime, (uint)AID.IncineratingHeat, 1.0f, true);
        ComponentCondition<P3WormholeLimitCut>(id + 0x310u, 0.1f, comp => comp.NumCasts >= 8)
            .DeactivateOnExit<P3WormholeLimitCut>();
        // +1.4s: BJ ends missile command
        ActorCastEnd(id + 0x320u, _module.AlexPrime, 4.9f, true, "Stack")
            .ActivateOnEnter<P3WormholeIncineratingHeat>()
            .ActivateOnEnter<P3WormholeEnumeration>() // note: icons appear ~0.3s before cast end
            .DeactivateOnExit<P3WormholeIncineratingHeat>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x330u, _module.AlexPrime, true, 4.2f)
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<P3WormholeEnumeration>(id + 0x340u, 0.5f, comp => !comp.Active, "Enumerations")
            .DeactivateOnExit<P3WormholeEnumeration>();
    }

    private void P3MegaHoly(uint id, float delay)
    {
        ActorCast(id, _module.AlexPrime, (uint)AID.MegaHoly, delay, 4f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x10u, _module.AlexPrime, (uint)AID.MegaHoly, 3.2f, 4f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P3SummonAlexander(uint id, float delay)
    {
        ActorCast(id, _module.AlexPrime, (uint)AID.SummonAlexander, delay, 3f, true);
        ActorTargetable(id + 0x10u, _module.AlexPrime, false, 6.2f, "Alex disappers");
        // +0.2s: j storm
        ActorCast(id + 0x20u, _module.AlexPrime, (uint)AID.DivineJudgment, 1.3f, 75f, true, "Enrage");
        // +0.9s after cast start: bj targetable
        // +4.1s after cast start: j wave 1, then every 3s
    }

    private void Phase4PerfectAlexander(uint id)
    {
        P4Intermission(id, 2.1f);
        P4FinalWord(id + 0x10000u, 10.1f);
        P4DoubleOpticalSight(id + 0x20000u, 7.2f);
        P4FateCalibrationAlpha(id + 0x30000u, 7.9f);
        P4OrdainedPunishment(id + 0x40000u, 5.1f);
        P4FateCalibrationBeta(id + 0x50000u, 10.1f);
        P4OrdainedPunishment(id + 0x60000u, 5.2f);
        P4AlmightyJudgmentIrresistibleGrace(id + 0x70000u, 7.1f);
        P4OrdainedPunishment(id + 0x80000u, 7.1f);
        P4AlmightyJudgmentIrresistibleGrace(id + 0x90000u, 6.9f);
        P4TemporalInterference(id + 0xA0000u, 10.0f);
    }

    private void P4Intermission(uint id, float delay)
    {
        ActorTargetable(id, _module.AlexPrime, false, delay)
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P3DivineJudgmentRaidwide>(id + 1u, 13.5f, comp => comp.NumCasts > 0, "Tank LB")
            .ActivateOnEnter<P3DivineJudgmentRaidwide>()
            .DeactivateOnExit<P3DivineJudgmentRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 2u, _module.PerfectAlex, true, 57.8f, "Alex appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private State P4OrdainedMotionStillness(uint id, float delay)
    {
        return ActorCastMulti(id, _module.PerfectAlex, [(uint)AID.OrdainedMotion, (uint)AID.OrdainedStillness], delay, 4f, true, "Stay/move");
    }

    private void P4FinalWord(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.FinalWord, delay, 4, true)
            .ActivateOnEnter<P4FinalWordDebuffs>(); // debuffs appear ~0.8s after cast
        P4OrdainedMotionStillness(id + 0x10u, 3.1f)
            .ActivateOnEnter<P4FinalWordStillnessMotion>();
        ComponentCondition<P4FinalWordDebuffs>(id + 0x20u, 3.5f, comp => comp.Done, "Forced march");
        P4OrdainedMotionStillness(id + 0x30u, 3.6f)
            .DeactivateOnExit<P4FinalWordDebuffs>() // forced movement resolve is ~1.4 after cast start
            .DeactivateOnExit<P4FinalWordStillnessMotion>();
    }

    private void P4OpticalSight(uint id, float delay)
    {
        ActorCastStartMulti(id, _module.PerfectAlex, [(uint)AID.OpticalSightSpread, (uint)AID.OpticalSightStack], delay, true)
            .ActivateOnEnter<P4OpticalSight>(); // icons appear right before cast start
        ActorCastEnd(id + 1u, _module.PerfectAlex, 3f, true);
        ComponentCondition<P4OpticalSight>(id + 2u, 2.1f, comp => !comp.Active, "Spread/stack")
            .DeactivateOnExit<P4OpticalSight>();
    }

    private void P4DoubleOpticalSight(uint id, float delay)
    {
        P4OpticalSight(id, delay);
        P4OpticalSight(id + 0x10u, 1.1f);
    }

    private void P4OrdainedPunishment(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.OrdainedCapitalPunishment, delay, 3, true)
            .ActivateOnEnter<P4OrdainedCapitalPunishment>();
        ComponentCondition<P4OrdainedCapitalPunishment>(id + 0x10u, 3.1f, comp => comp.NumCasts >= 1, "Shared tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCastStart(id + 0x20u, _module.PerfectAlex, (uint)AID.OrdainedPunishment, 1.1f, true)
            .SetHint(StateMachine.StateHint.Tankbuster); // second shared hit happens at the same time as cast start
        ComponentCondition<P4OrdainedCapitalPunishment>(id + 0x30u, 1.0f, comp => comp.NumCasts >= 3, "Shared tankbuster 3")
            .DeactivateOnExit<P4OrdainedCapitalPunishment>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCastEnd(id + 0x40u, _module.PerfectAlex, 4f, true, "Solo tankbuster")
            .ActivateOnEnter<P4OrdainedPunishment>()
            .DeactivateOnExit<P4OrdainedPunishment>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P4FateCalibrationAlpha(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.FateProjectionAlpha, delay, 5f, true)
            .ActivateOnEnter<P4FateProjection>(); // tethers appear ~1.2s after cast end
        ActorCast(id + 0x10u, _module.PerfectAlex, (uint)AID.FateCalibrationAlpha, 3.2f, 22f, true)
            .ActivateOnEnter<P4FateCalibrationAlphaStillnessMotion>()
            .ActivateOnEnter<P4FateCalibrationAlphaDebuffs>()
            .ActivateOnEnter<P4FateCalibrationAlphaSacrament>();
        // timeline during fate calibration cast:
        // +5.1s: 1s-long stillness/motion cast
        // +6.2s: stillness or motion castevents on projections
        // +12.1s: defamation on helper near projection
        // +12.6s: shared sentence on projection
        // +13.1s: stillness/motion cast start, 3x aggravated assault on projections
        // +14.1s: stillness/motion cast end
        // +14.9s: stillness or motion castevents on remaining projections
        // +15.1s: sacrament 1
        // +15.7s: sacrament 2
        // +16.2s: sacrament 3

        ActorTargetable(id + 0x100u, _module.PerfectAlex, false, 3f, "Fate calibration alpha")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P4FateCalibrationAlphaStillnessMotion>(id + 0x110u, 4.1f, comp => comp.NumCasts >= 1, "Stay/move")
            .ExecOnEnter<P4FateCalibrationAlphaStillnessMotion>(comp => comp.ApplyNextRequirement());
        // +3.0s: defamation
        // +3.5s: shared sentence
        ComponentCondition<P4FateCalibrationAlphaStillnessMotion>(id + 0x140u, 4.1f, comp => comp.NumCasts >= 2, "Stay/move") // also aggravated resolve & sacrament 1
            .DeactivateOnExit<P4FateCalibrationAlphaDebuffs>()
            .DeactivateOnExit<P4FateCalibrationAlphaStillnessMotion>();
        // +0.5s: sacrament 2
        ComponentCondition<P4FateCalibrationAlphaSacrament>(id + 0x160u, 1.1f, comp => comp.NumCasts >= 3)
            .DeactivateOnExit<P4FateCalibrationAlphaSacrament>()
            .DeactivateOnExit<P4FateProjection>();

        ActorTargetable(id + 0x200u, _module.PerfectAlex, true, 5.1f, "Boss reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P4FateCalibrationBeta(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.FateProjectionBeta, delay, 5f, true)
            .ActivateOnEnter<P4FateProjection>(); // tethers appear ~1.2s after cast end
        ActorCast(id + 0x10u, _module.PerfectAlex, (uint)AID.FateCalibrationBeta, 3.2f, 35f, true)
            .ActivateOnEnter<P4FateCalibrationBetaDebuffs>()
            .ActivateOnEnter<P4FateCalibrationBetaJJump>()
            .ActivateOnEnter<P4FateCalibrationBetaOpticalSight>()
            .ActivateOnEnter<P4FateCalibrationBetaRadiantSacrament>();
        // timeline during fate calibration cast:
        // +10.0s: tethers + status 2195 (extra 0x84/85)
        // +11.8-13.1s: kill 4 tethered projections
        // +14.1s: shared sentence on untethered light projection
        // +15.1s: jumps
        // +20.7s: stack/spread effect
        // +22.7s: kill beacons with stack/spread
        // +26.2s: donut

        ActorTargetable(id + 0x100u, _module.PerfectAlex, false, 3.1f, "Fate calibration beta")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P4FateCalibrationBetaDebuffs>(id + 0x110u, 2f, comp => comp.Done); // forced march apply
        ComponentCondition<P4FateCalibrationBetaJJump>(id + 0x120u, 8.1f, comp => comp.NumCasts > 0, "Jumps")
            .ExecOnEnter<P4FateCalibrationBetaJJump>(comp => comp.Show())
            .DeactivateOnExit<P4FateCalibrationBetaDebuffs>() // tethers & shared sentence resolve ~1s before jumps
            .DeactivateOnExit<P4FateCalibrationBetaJJump>();
        ComponentCondition<P4FateCalibrationBetaOpticalSight>(id + 0x130u, 6f, comp => comp.Done, "Stack/spread")
            .ExecOnEnter<P4FateCalibrationBetaOpticalSight>(comp => comp.Show())
            .DeactivateOnExit<P4FateCalibrationBetaOpticalSight>();
        ComponentCondition<P4FateCalibrationBetaRadiantSacrament>(id + 0x140u, 5f, comp => comp.NumCasts > 0, "Donut")
            .ExecOnEnter<P4FateCalibrationBetaRadiantSacrament>(comp => comp.Show())
            .DeactivateOnExit<P4FateCalibrationBetaRadiantSacrament>()
            .DeactivateOnExit<P4FateProjection>();

        ActorTargetable(id + 0x200u, _module.PerfectAlex, true, 5.2f, "Boss reappear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P4AlmightyJudgmentIrresistibleGrace(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.AlmightyJudgment, delay, 3f, true);
        ActorCastStart(id + 0x10u, _module.PerfectAlex, (uint)AID.IrresistibleGrace, 11.1f, true, "Exatrines 1") // first exatrine aoe happens together with cast start
            .ActivateOnEnter<P4IrresistibleGrace>()
            .ActivateOnEnter<P4AlmightyJudgment>();
        ComponentCondition<P4AlmightyJudgment>(id + 0x20u, 4f, comp => !comp.Active, "Exatrines 3")
            .DeactivateOnExit<P4AlmightyJudgment>();
        ActorCastEnd(id + 0x30u, _module.PerfectAlex, 1f, true, "Stack")
            .DeactivateOnExit<P4IrresistibleGrace>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P4TemporalInterference(uint id, float delay)
    {
        ActorCast(id, _module.PerfectAlex, (uint)AID.TemporalInterference, delay, 5f, true)
            .ActivateOnEnter<P4TemporalPrison>();
        ActorCastStart(id + 0x10u, _module.PerfectAlex, (uint)AID.TemporalPrison, 3.2f, true);
        ComponentCondition<P4TemporalPrison>(id + 0x20u, 10f, comp => comp.NumPrisons >= 1, "Prison 1");
        ComponentCondition<P4TemporalPrison>(id + 0x21u, 5f, comp => comp.NumPrisons >= 2, "Prison 2");
        ComponentCondition<P4TemporalPrison>(id + 0x22u, 5f, comp => comp.NumPrisons >= 3, "Prison 3");
        ComponentCondition<P4TemporalPrison>(id + 0x23u, 5f, comp => comp.NumPrisons >= 4, "Prison 4");
        ComponentCondition<P4TemporalPrison>(id + 0x24u, 5f, comp => comp.NumPrisons >= 5, "Prison 5");
        ComponentCondition<P4TemporalPrison>(id + 0x25u, 5f, comp => comp.NumPrisons >= 6, "Prison 6");
        ComponentCondition<P4TemporalPrison>(id + 0x26u, 5f, comp => comp.NumPrisons >= 7, "Prison 7");
        ActorCastEnd(id + 0x30u, _module.PerfectAlex, 2, true, "Enrage");
        SimpleState(id + 0x40u, 3f, "???");
    }
}
