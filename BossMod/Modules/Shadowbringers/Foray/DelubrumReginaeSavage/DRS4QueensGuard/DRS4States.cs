namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

sealed class DRS4QueensGuardStates : StateMachineBuilder
{
    readonly DRS4QueensGuard _module;

    public DRS4QueensGuardStates(DRS4QueensGuard module) : base(module)
    {
        _module = module;
        SimplePhase(0u, Phase0, "P0: 4 guards")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !(_module.Knight()?.IsTargetable ?? true) && !(_module.Warrior()?.IsTargetable ?? true) && !(_module.Soldier()?.IsTargetable ?? true) && !(_module.Gunner()?.IsTargetable ?? true);
        SimplePhase(1u, Phase1, "P1: knight+warrior")
            .ActivateOnEnter<SpellforgeSteelstingHint>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !(_module.Knight()?.IsTargetable ?? true) && module.Knight()?.HPMP.CurHP <= 1u && !(_module.Warrior()?.IsTargetable ?? true) && module.Warrior()?.HPMP.CurHP <= 1u;
        SimplePhase(2u, Phase2, "P2: soldier+gunner")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !(_module.Gunner()?.IsTargetable ?? true) && module.Gunner()?.HPMP.CurHP <= 1u && !(_module.Soldier()?.IsTargetable ?? true) && module.Soldier()?.HPMP.CurHP <= 1u;
        SimplePhase(3u, Phase3, "P3: wards")
            .ActivateOnEnter<CoatOfArms>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
    }

    private void Phase0(uint id)
    {
        SimpleState(id, 0f, "Raidwides until 80%")
            .ActivateOnEnter<ArenaChange>();
    }

    private void Phase1(uint id)
    {
        ActorTargetable(id, _module.Knight, true, 3.3f, "Warrior + Knight appear")
            .DeactivateOnEnter<ArenaChange>();
        Phase1Repeat(id + 0x100000u, 8.1f);
        Phase1Repeat(id + 0x200000u, 8.1f);
        // TODO: enrage
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void Phase1Repeat(uint id, float delay)
    {
        P1SelectVulnerability(id, delay);
        P1OptimalOffensiveAboveBoard(id + 0x10000u, 9.1f);
        P1WindsOfWeightOptimalPlay(id + 0x20000u, 10.3f);
        P1RapidSever(id + 0x30000u, 4.2f);
    }

    private void Phase2(uint id)
    {
        ActorTargetable(id, _module.Soldier, true, 3.3f, "Soldier + Gunner appear");
        P2FoolsGambit(id + 0x10000u, 8.2f);
        P2RapidSever(id + 0x20000u, 5.6f);
        P2BloodAndBone(id + 0x30000u, 4.2f);
        P2GunTurret(id + 0x40000u, 10.2f);
        P2DoubleGambit(id + 0x50000u, 10.3f);
        P2BloodAndBone(id + 0x60000u, 5.7f);
        P2RapidSever(id + 0x70000u, 4.1f);
        P2AutomaticTurret(id + 0x80000u, 10.2f);
        P2BloodAndBone(id + 0x90000u, 5.1f);
        P2RapidSever(id + 0xA0000u, 4.1f);
        SimpleState(id + 0xB0000u, 15f, "Enrage");  // TODO: not sure about the timing, no log yet
    }

    private void Phase3(uint id)
    {
        ActorCast(id, _module.Knight, AID.StrongpointDefense, 3.4f, 5f); // warrior also casts spiteful spirit at the same time
        ComponentCondition<CoatOfArms>(id + 0x10u, 1.2f, static comp => comp.ActiveActors.Count != 0, "Wards + spheres 1");
        // +1.8s: enrage casts start
        // +2.0s: spiritual spheres become targetable (don't really care...)
        // +4.7s: sprite check & coat of arms cast start

        ComponentCondition<CoatOfArms>(id + 0x20u, 9.0f, static comp => comp.Active, "Shields 1")
            .ActivateOnEnter<Fracture>();
        ComponentCondition<Fracture>(id + 0x21u, 0.9f, static comp => comp.NumCasts > 0, "Reflect 1")
            .DeactivateOnExit<Fracture>();
        ComponentCondition<CoatOfArms>(id + 0x22u, 12.1f, static comp => !comp.Active);

        ComponentCondition<CoatOfArms>(id + 0x30u, 4.1f, static comp => comp.Active, "Shields 2");
        ComponentCondition<CoatOfArms>(id + 0x31u, 13f, static comp => !comp.Active)
            .ActivateOnEnter<Fracture>();
        ComponentCondition<Fracture>(id + 0x32u, 0.8f, static comp => comp.NumCasts > 0, "Reflect 2")
            .DeactivateOnExit<Fracture>();

        ComponentCondition<CoatOfArms>(id + 0x40u, 3.3f, static comp => comp.Active, "Shields 3");
        ComponentCondition<CoatOfArms>(id + 0x41u, 13f, static comp => !comp.Active);

        ComponentCondition<CoatOfArms>(id + 0x50u, 4.1f, static comp => comp.Active, "Shields 4");
        SimpleState(id + 0x60u, 11.5f, "Enrage");
    }

    private void P1SelectVulnerability(uint id, float delay)
    {
        ComponentCondition<SpellforgeSteelstingHint>(id, delay, static comp => comp.Active, "Spellforge/steelsting");
    }

    private void P1BloodAndBone(uint id, float delay)
    {
        // both knight and warrior cast their raidwides, unless one of them is killed
        Condition(id, delay, () => (_module.Knight()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.BloodAndBoneKnight || (_module.Warrior()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.BloodAndBoneWarrior);
        Condition(id + 1u, 5f, () => _module.Knight()?.CastInfo == null && _module.Warrior()?.CastInfo == null, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1RapidSever(uint id, float delay)
    {
        // both knight and warrior cast their tankbusters, unless one of them is killed
        Condition(id, delay, () => (_module.Knight()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.RapidSeverKnight || (_module.Warrior()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.RapidSeverWarrior);
        Condition(id + 1u, 5f, () => _module.Knight()?.CastInfo == null && _module.Warrior()?.CastInfo == null, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1OptimalOffensiveAboveBoard(uint id, float delay)
    {
        ActorCast(id, _module.Knight, AID.RelentlessBatteryKnight, delay, 5); // both knight and warrior cast their visual

        ActorCastStartMulti(id + 0x10u, _module.Knight, [AID.SwordOmen, AID.ShieldOmen], 3.2f);
        ActorCastStart(id + 0x11u, _module.Warrior, AID.Bombslinger, 1);
        ActorCastEnd(id + 0x12u, _module.Knight, 2);
        ActorCastEnd(id + 0x13u, _module.Warrior, 1);
        // +1.0s: create 2x6 bombs (aetherial bolt/burst)

        ActorCastStart(id + 0x20u, _module.Warrior, AID.ReversalOfForces, 3.2f); // tethers/icons for reversal appear ~0.1s before cast start
        ActorTargetable(id + 0x21u, _module.Knight, false, 3f, "Knight disappear");
        ActorCastEnd(id + 0x22u, _module.Warrior, 1f);
        // +0.7s: create aetherial sphere in center
        // +0.9s: replace tethers with statuses
        // +2.2s: tether sphere to knight

        // main cast is a charge aoe from knight's position (at border) to cast location (center)
        // for shield, together with this cast, we have two raidwide knockback casts and sphere-move cast - they have identical cast times
        ActorCastStartMulti(id + 0x30u, _module.Knight, [AID.OptimalOffensiveSword, AID.OptimalOffensiveShield], 2.3f);
        ActorCastStart(id + 0x31u, _module.Warrior, AID.AboveBoard, 2.9f)
            .ActivateOnEnter<OptimalOffensiveSword>()
            .ActivateOnEnter<OptimalOffensiveShield>()
            .ActivateOnEnter<OptimalOffensiveShieldKnockback>()
            .ActivateOnEnter<UnluckyLot>() // start showing hints immediately; actual cast starts 0.6s after sword cast or 1.6s after shield cast and lasts for 1s
            .ActivateOnEnter<AboveBoard>(); // start showing hints immediately
        ActorCastEnd(id + 0x32u, _module.Knight, 4.1f, false, "Charge")
            .DeactivateOnExit<OptimalOffensiveSword>()
            .DeactivateOnExit<OptimalOffensiveShield>()
            .DeactivateOnExit<OptimalOffensiveShieldKnockback>();
        ActorCastEnd(id + 0x33u, _module.Warrior, 1.9f);

        ComponentCondition<AboveBoard>(id + 0x40u, 0.9f, static comp => comp.CurState == AboveBoard.State.ThrowUpDone, "Throw up")
            .DeactivateOnExit<UnluckyLot>(); // cast finishes 0.2s or 1.2s before that
        ComponentCondition<AboveBoard>(id + 0x50u, 2.1f, static comp => comp.CurState == AboveBoard.State.ShortExplosionsDone, "Bombs 1");
        ActorTargetable(id + 0x60u, _module.Knight, true, 1.4f, "Knight reappear");
        ComponentCondition<AboveBoard>(id + 0x70u, 2.8f, static comp => comp.CurState == AboveBoard.State.LongExplosionsDone, "Bombs 2")
            .DeactivateOnExit<AboveBoard>();

        ActorCast(id + 0x1000u, _module.Warrior, AID.Boost, 5.0f, 4f, false, "Damage up");
        P1BloodAndBone(id + 0x1010u, 3.1f);
    }

    private void P1WindsOfWeightOptimalPlay(uint id, float delay)
    {
        ActorCast(id, _module.Warrior, AID.RelentlessBatteryWarrior, delay, 5f); // both knight and warrior cast their visual
        ActorCast(id + 0x10u, _module.Warrior, AID.ReversalOfForces, 3.2f, 4f); // tethers/icons for reversal appear ~0.1s before cast start
        // +0.9s: replace tethers with statuses

        ActorCastStart(id + 0x20u, _module.Warrior, AID.WindsOfWeight, 3.2f);
        ActorCastStartMulti(id + 0x21u, _module.Knight, [AID.SwordOmen, AID.ShieldOmen], 1.9f)
            .ActivateOnEnter<WindsOfWeight>();
        ActorCastEnd(id + 0x22u, _module.Knight, 3f);
        ActorCastEnd(id + 0x23u, _module.Warrior, 1.1f, false, "Wind/gravity")
            .DeactivateOnExit<WindsOfWeight>();

        ActorCastStartMulti(id + 0x30u, _module.Knight, [AID.OptimalPlaySword, AID.OptimalPlayShield], 2.1f);
        ActorCastStart(id + 0x31u, _module.Warrior, AID.Boost, 3f)
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<OptimalPlayCone>();
        ActorCastEnd(id + 0x32u, _module.Knight, 2f, false, "Cone + circle/donut")
            .DeactivateOnExit<OptimalPlaySword>()
            .DeactivateOnExit<OptimalPlayShield>()
            .DeactivateOnExit<OptimalPlayCone>();
        ActorCastEnd(id + 0x33u, _module.Warrior, 2f, false, "Damage up");

        P1BloodAndBone(id + 0x40u, 3.2f);
    }

    private void P2BloodAndBone(uint id, float delay, string startName = "")
    {
        // both soldier and gunner cast cast their raidwides, unless one of them is killed
        Condition(id, delay, () => (_module.Soldier()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.BloodAndBoneSoldier || (_module.Gunner()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.QueensShotRaidwide, startName);
        Condition(id + 1u, 5f, () => _module.Soldier()?.CastInfo == null && _module.Gunner()?.CastInfo == null, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2RapidSever(uint id, float delay)
    {
        // both soldier and gunner cast their tankbusters, unless one of them is killed
        Condition(id, delay, () => (_module.Soldier()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.RapidSeverSoldier || (_module.Gunner()?.CastInfo?.Action.ID ?? 0u) == (uint)AID.ShotInTheDark);
        Condition(id + 1, 5, () => _module.Soldier()?.CastInfo == null && _module.Gunner()?.CastInfo == null, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2FoolsGambit(uint id, float delay)
    {
        ActorCast(id, _module.Soldier, AID.RelentlessBatterySoldier, delay, 5f); // both soldier and gunner cast their visual
        ActorCast(id + 0x10u, _module.Soldier, AID.GreatBallOfFire, 3.2f, 3f);
        // +0.8s: create 2 big and 2 small flame actors

        ActorCastStart(id + 0x20u, _module.Soldier, AID.FoolsGambit, 3.2f);
        ActorCastStart(id + 0x21u, _module.Gunner, AID.AutomaticTurretGambit, 5.8f);
        ActorCastEnd(id + 0x22u, _module.Soldier, 0.2f);
        // +0.7s: flames gain transfiguration statuses
        ActorCastEnd(id + 0x23u, _module.Gunner, 2.8f);
        // +1.2s: create 10 turret actors

        ActorCast(id + 0x30u, _module.Gunner, AID.Reading, 3.2f, 3f);
        // +1.0s: players get unseen statuses
        // +3.0s: flames lose transfiguration statuses

        ActorCastStart(id + 0x40u, _module.Gunner, AID.QueensShotUnseen, 3.1f)
            .ActivateOnEnter<GreatBallOfFire>(); // activating at this point would show flames in final positions; no point activating earlier, since they'll be moving
        ComponentCondition<GreatBallOfFire>(id + 0x50u, 3.5f, static comp => comp.NumCasts > 0, "Flames")
            .ActivateOnEnter<QueensShotUnseen>()
            .DeactivateOnExit<GreatBallOfFire>();
        ActorCastEnd(id + 0x60u, _module.Gunner, 3.5f, false, "Face gunner")
            .DeactivateOnExit<QueensShotUnseen>();

        // +0.5s: turrets start their casts
        ComponentCondition<TurretsTourUnseen>(id + 0x70u, 3.5f, static comp => comp.NumCasts > 0, "Face turret")
            .ActivateOnEnter<TurretsTourUnseen>()
            .DeactivateOnExit<TurretsTourUnseen>();
    }

    private void P2GunTurret(uint id, float delay)
    {
        ActorCast(id, _module.Gunner, AID.RelentlessBatteryGunner, delay, 5f); // both soldier and gunner cast their visual
        ActorCast(id + 0x10u, _module.Gunner, AID.GunTurret, 3.2f, 3f);
        Condition(id + 0x20u, 1.3f, () =>
        {
            var turrets = _module.GunTurrets;
            var count = turrets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (turrets[i].IsTargetable)
                {
                    return true;
                }
            }
            return false;
        }, "Spawn turrets");
        ActorCast(id + 0x30u, _module.Gunner, AID.HigherPower, 1.9f, 4f);

        // sniper shots happen around raidwide start (~0.1s after); tricky to make a state for it, because both turrets could be killed before allowing to finish
        P2BloodAndBone(id + 0x40u, 6.9f, "Turret tankbusters");

        // note: deadline to kill both turrets (explosion cast end) is ~2s into next cast
        ActorCastMulti(id + 0x50u, _module.Soldier, [AID.FieryPortent, AID.IcyPortent], 11.2f, 6f, false, "Move/stay")
            .ActivateOnEnter<FieryPortent>()
            .ActivateOnEnter<IcyPortent>()
            .DeactivateOnExit<FieryPortent>()
            .DeactivateOnExit<IcyPortent>();
    }

    private void P2DoubleGambit(uint id, float delay)
    {
        ActorCast(id, _module.Soldier, AID.RelentlessBatterySoldier, delay, 5f); // both soldier and gunner cast their visual
        ActorCast(id + 0x10u, _module.Soldier, AID.DoubleGambit, 3.2f, 3f);

        ActorCastStart(id + 0x20u, _module.Soldier, AID.SecretsRevealed, 3.2f); // note: tethers appear right before cast start
        ActorCastStart(id + 0x21u, _module.Gunner, AID.AutomaticTurretGambit, 1.8f);
        ActorCastEnd(id + 0x22u, _module.Gunner, 3f);
        ActorCastEnd(id + 0x23u, _module.Soldier, 0.2f);

        ActorCast(id + 0x30u, _module.Gunner, AID.Reading, 3.0f, 3f);

        ActorCast(id + 0x40u, _module.Gunner, AID.QueensShotUnseen, 3.1f, 7f, false, "Face gunner")
            .ActivateOnEnter<QueensShotUnseen>()
            .ActivateOnEnter<PawnOff>() // casts start ~0.4s into gunner's cast
            .DeactivateOnExit<QueensShotUnseen>();

        // +0.5s: turrets start their casts
        ComponentCondition<TurretsTourUnseen>(id + 0x50u, 3.5f, static comp => comp.NumCasts > 0, "Face turret")
            .ActivateOnEnter<TurretsTourUnseen>()
            .DeactivateOnExit<TurretsTourUnseen>()
            .DeactivateOnExit<PawnOff>();
    }

    private void P2AutomaticTurret(uint id, float delay)
    {
        ActorCast(id, _module.Gunner, AID.RelentlessBatteryGunner, delay, 5f); // both soldier and gunner cast their visual
        ActorCast(id + 0x10u, _module.Gunner, AID.AutomaticTurretNormal, 3.2f, 3f);
        ActorCast(id + 0x20u, _module.Gunner, AID.TurretsTourNormal, 3.2f, 5f, false, "Turrets start")
            .ActivateOnEnter<TurretsTour>();
        ActorCastStartMulti(id + 0x30u, _module.Soldier, [AID.FieryPortent, AID.IcyPortent], 0.9f);
        ComponentCondition<TurretsTour>(id + 0x31u, 0.8f, static comp => comp.NumCasts >= 4, "Turrets resolve")
            .ActivateOnEnter<FieryPortent>()
            .ActivateOnEnter<IcyPortent>()
            .DeactivateOnExit<TurretsTour>();
        ActorCastEnd(id + 0x32u, _module.Soldier, 5.2f, false, "Move/stay")
            .DeactivateOnExit<FieryPortent>()
            .DeactivateOnExit<IcyPortent>();
    }
}
