namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class DRS8QueenStates : StateMachineBuilder
{
    private readonly DRS8Queen _module;

    public DRS8QueenStates(DRS8Queen module) : base(module)
    {
        _module = module;
        SimplePhase(0u, Phase1, "P1")
            .ActivateOnEnter<ArenaChange>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1u || (Module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.GodsSaveTheQueen;
        DeathPhase(1u, Phase2);
    }

    private void Phase1(uint id)
    {
        EmpyreanIniquity(id, 10.2f);
        QueensWill(id + 0x10000u, 13.7f);
        CleansingSlash(id + 0x20000u, 10.9f);
        EmpyreanIniquity(id + 0x30000u, 4.2f);
        QueensEdict(id + 0x40000u, 11.5f);
        SimpleState(id + 0x50000u, 12.5f, "Second phase");
    }

    private void Phase2(uint id)
    {
        GodsSaveTheQueen(id, default);
        MaelstromsBolt(id + 0x10000u, 31.7f);
        RelentlessPlay1(id + 0x20000u, 7.3f);
        CleansingSlash(id + 0x30000u, 4.4f);
        RelentlessPlay2(id + 0x40000u, 8.2f);
        EmpyreanIniquity(id + 0x50000u, 5.1f);
        QueensEdict(id + 0x60000u, 11.5f);
        CleansingSlash(id + 0x70000u, 2.1f);
        RelentlessPlay3(id + 0x80000u, 10.2f);
        MaelstromsBolt(id + 0x90000u, 16.3f);
        EmpyreanIniquity(id + 0xA0000u, 6.2f);
        RelentlessPlay4(id + 0xB0000u, 8.2f);
        RelentlessPlay5(id + 0xC0000u, 0.1f);
        // TODO: boss gains damage up at +6.6, then presumably would start some enrage cast...
        SimpleState(id + 0xD0000, 20.8f, "Enrage");
    }

    private void EmpyreanIniquity(uint id, float delay)
    {
        Cast(id, AID.EmpyreanIniquity, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CleansingSlash(uint id, float delay)
    {
        Cast(id, AID.CleansingSlashFirst, delay, 5f, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<CleansingSlashSecond>(id + 2u, 3.1f, static comp => comp.NumCasts > 0, "Tankbuster 2")
            .ActivateOnEnter<CleansingSlashSecond>()
            .DeactivateOnExit<CleansingSlashSecond>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void QueensWill(uint id, float delay)
    {
        // right before cast start: ENVC 19.00200010, guards gain 2056 status with extra 0xE1 + PATE 1E43
        Cast(id, AID.QueensWill, delay, 5f)
            .ActivateOnEnter<QueensWill>(); // statuses appear ~0.7s after cast end
        Cast(id + 0x10u, AID.NorthswainsGlow, 3.2f, 3f)
            .ActivateOnEnter<NorthswainsGlow>(); // aoe casts start ~0.8s after visual cast end
        Cast(id + 0x20u, AID.BeckAndCallToArmsWillKW, 3.1f, 5f);
        ComponentCondition<NorthswainsGlow>(id + 0x30, 2.6f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<NorthswainsGlow>();

        CastStart(id + 0x40u, AID.BeckAndCallToArmsWillSG, 0.5f);
        ComponentCondition<QueensWill>(id + 0x41, 1.1f, static comp => comp.NumCasts >= 2, "Easy chess 1");
        CastEnd(id + 0x42u, 3.9f);
        ComponentCondition<QueensWill>(id + 0x43, 4.3f, static comp => comp.NumCasts >= 4, "Easy chess 2")
            .DeactivateOnExit<QueensWill>();
    }

    private void QueensEdict(uint id, float delay)
    {
        Cast(id, AID.QueensEdict, delay, 5f)
            .ActivateOnEnter<QueensEdict>(); // safezone envcontrol, statuses on guards and players appear ~0.8s after cast end
        Targetable(id + 0x10u, false, 3.1f, "Disappear");
        Cast(id + 0x20u, AID.BeckAndCallToArmsEdictKW, 0.1f, 16.3f);

        CastStart(id + 0x30u, AID.BeckAndCallToArmsEdictSG, 3.2f);
        ComponentCondition<QueensEdict>(id + 0x31u, 1.3f, static comp => comp.NumCasts >= 2, "Super chess rows"); // 1st edict movement starts ~0.2s before this
        CastEnd(id + 0x32u, 7.4f);

        ComponentCondition<QueensEdict>(id + 0x40u, 2.4f, static comp => comp.NumStuns > 0, "Super chess columns");
        ComponentCondition<QueensEdict>(id + 0x41u, 1.9f, static comp => comp.NumCasts >= 4);

        CastStart(id + 0x50u, AID.GunnhildrsBlades, 2.8f);
        ComponentCondition<QueensEdict>(id + 0x51u, 1.3f, static comp => comp.NumStuns == 0); // 2nd edict movement starts ~1.0s before this
        ComponentCondition<QueensEdict>(id + 0x52u, 9f, static comp => comp.NumStuns > 0, "Super chess safespot");
        CastEnd(id + 0x53u, 3.7f)
            .DeactivateOnExit<QueensEdict>();

        Targetable(id + 0x60u, true, 3.1f, "Reappear");
    }

    private void GodsSaveTheQueen(uint id, float delay)
    {
        Cast(id, AID.GodsSaveTheQueen, delay, 5f);
        ComponentCondition<GodsSaveTheQueen>(id + 0x10u, 2.1f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<GodsSaveTheQueen>()
            .DeactivateOnExit<GodsSaveTheQueen>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MaelstromsBolt(uint id, float delay)
    {
        ComponentCondition<MaelstromsBolt>(id, delay, static comp => comp.NumCasts > 0, "Reflect/raidwide")
            .ActivateOnEnter<MaelstromsBolt>()
            .DeactivateOnExit<MaelstromsBolt>();
    }

    private void RelentlessPlay1(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5f);

        // +3.0s: tethers/icons
        ActorCast(id + 0x10u, _module.Warrior, AID.ReversalOfForces, 3.1f, 4f); // gunner casts automatic turret - starts 1s later, ends at the same time

        // +1.0s: tethers are replaced with statuses
        CastStart(id + 0x20u, AID.NorthswainsGlow, 1.1f);
        // +0.1s: turrets spawn
        ActorCastStart(id + 0x21u, _module.Gunner, AID.Reading, 2.1f);
        CastEnd(id + 0x22u, 0.9f);
        ActorCastEnd(id + 0x23u, _module.Gunner, 2.1f)
            .ActivateOnEnter<NorthswainsGlow>(); // aoes start ~0.8s after visual cast end
        // +1.0s: unseen statuses

        ActorCastStart(id + 0x30u, _module.Warrior, AID.WindsOfWeight, 3.0f);
        ActorCastStart(id + 0x31u, _module.Gunner, AID.QueensShot, 0.2f)
            .ActivateOnEnter<WindsOfWeight>();
        ActorCastEnd(id + 0x32u, _module.Warrior, 5.8f, false, "Wind/gravity")
            .ActivateOnEnter<QueensShot>()
            .DeactivateOnExit<WindsOfWeight>()
            .DeactivateOnExit<NorthswainsGlow>();
        ActorCastEnd(id + 0x33u, _module.Gunner, 1.2f, false, "Face gunner")
            .DeactivateOnExit<QueensShot>();

        // +0.5s: turrets start their casts
        ComponentCondition<TurretsTourUnseen>(id + 0x40u, 3.5f, static comp => comp.NumCasts > 0, "Face turret")
            .ActivateOnEnter<TurretsTourUnseen>()
            .DeactivateOnExit<TurretsTourUnseen>();
    }

    private void RelentlessPlay2(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5f);
        ActorCast(id + 0x10u, _module.Knight, AID.ShieldOmen, 3.2f, 3f);

        ActorCastStart(id + 0x20u, _module.Soldier, AID.DoubleGambit, 4.5f);
        Targetable(id + 0x21u, false, 0.4f, "Disappear");
        ActorCastStart(id + 0x22u, _module.Knight, AID.OptimalOffensive, 1.4f);
        CastStartMulti(id + 0x23u, [AID.JudgmentBladeR, AID.JudgmentBladeL], 1.9f)
            .ActivateOnEnter<OptimalOffensive>()
            .ActivateOnEnter<OptimalOffensiveKnockback>()
            .ActivateOnEnter<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x24u, _module.Soldier, 1.3f)
            .ActivateOnEnter<JudgmentBlade>();
        ActorCastStart(id + 0x25u, _module.Soldier, AID.SecretsRevealed, 3.2f); // right before cast start, 2 unsafe avatars are tethered to caster
        ActorCastEnd(id + 0x26u, _module.Knight, 0.6f, false, "Charge + Knockback")
            .DeactivateOnExit<OptimalOffensive>()
            .DeactivateOnExit<OptimalOffensiveKnockback>();
        CastEnd(id + 0x27u, 1.9f);
        ComponentCondition<JudgmentBlade>(id + 0x28u, 0.3f, static comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<UnluckyLotAetherialSphere>(id + 0x29u, 0.5f, static comp => comp.NumCasts > 0, "Sphere explosion")
            .DeactivateOnExit<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x2Au, _module.Soldier, 1.7f);

        CastMulti(id + 0x30u, [AID.JudgmentBladeR, AID.JudgmentBladeL], 2.2f, 7f)
            .ActivateOnEnter<JudgmentBlade>()
            .ActivateOnEnter<PawnOff>(); // cast starts ~2.7s after judgment blade; we could show hints much earlier based on tethers
        ComponentCondition<JudgmentBlade>(id + 0x32u, 0.3f, static comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<PawnOff>(id + 0x33u, 2.4f, static comp => comp.NumCasts > 0, "Real/fake aoes")
            .DeactivateOnExit<PawnOff>();

        Targetable(id + 0x40u, true, 2.0f, "Reappear");
    }

    private void RelentlessPlay3(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5f);
        ActorCastMulti(id + 0x10u, _module.Knight, [AID.SwordOmen, AID.ShieldOmen], 3.1f, 3f);

        // note: gunner starts automatic turret visual together with optimal play
        ActorCastMulti(id + 0x20u, _module.Knight, [AID.OptimalPlaySword, AID.OptimalPlayShield], 6.5f, 5f, false, "Cone + circle/donut")
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<OptimalPlayCone>()
            .DeactivateOnExit<OptimalPlaySword>()
            .DeactivateOnExit<OptimalPlayShield>()
            .DeactivateOnExit<OptimalPlayCone>();

        ActorCast(id + 0x30u, _module.Gunner, AID.TurretsTour, 1f, 5f, false, "Turrets start")
            .ActivateOnEnter<TurretsTour>();
        ComponentCondition<TurretsTour>(id + 0x40u, 1.7f, static comp => comp.NumCasts >= 4, "Turrets resolve")
            .DeactivateOnExit<TurretsTour>();
    }

    private void RelentlessPlay4(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5f);
        ActorCast(id + 0x10u, _module.Warrior, AID.Bombslinger, 3.1f, 3f);
        // +0.9s: bombs spawn

        ActorCastStart(id + 0x20u, _module.Warrior, AID.ReversalOfForces, 3.2f); // icons/tethers appear ~0.1s before cast start
        CastStart(id + 0x21u, AID.HeavensWrath, 2.9f);
        ActorCastEnd(id + 0x22u, _module.Warrior, 1.1f);
        CastEnd(id + 0x23u, 1.9f);

        ComponentCondition<HeavensWrathAOE>(id + 0x30u, 0.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<HeavensWrathAOE>();
        ActorCastStartMulti(id + 0x31u, _module.Soldier, [AID.FieryPortent, AID.IcyPortent], 2.2f)
            .ActivateOnEnter<HeavensWrathKnockback>()
            .ActivateOnEnter<AboveBoard>();
        ComponentCondition<HeavensWrathAOE>(id + 0x32u, 2.8f, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<FieryIcyPortent>()
            .DeactivateOnExit<HeavensWrathAOE>()
            .DeactivateOnExit<HeavensWrathKnockback>();
        ActorCastStart(id + 0x33u, _module.Warrior, AID.AboveBoard, 0.5f);
        Targetable(id + 0x34u, false, 1.8f, "Boss disappears");
        ActorCastEnd(id + 0x35u, _module.Soldier, 0.9f, false, "Move/stay")
            .DeactivateOnExit<FieryIcyPortent>();
        ActorCastEnd(id + 0x36u, _module.Soldier, 3.3f);

        ComponentCondition<AboveBoard>(id + 0x40u, 4.2f, static comp => comp.CurState == AboveBoard.State.ThrowUpDone, "Throw up");
        ComponentCondition<AboveBoard>(id + 0x41u, 2.0f, static comp => comp.CurState == AboveBoard.State.ShortExplosionsDone, "Bombs 1");
        ComponentCondition<AboveBoard>(id + 0x42u, 4.2f, static comp => comp.CurState == AboveBoard.State.LongExplosionsDone, "Bombs 2")
            .DeactivateOnExit<AboveBoard>();

        Targetable(id + 0x50u, true, 3.0f, "Boss reappears");
    }

    private void RelentlessPlay5(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5f);

        ActorCastStart(id + 0x10u, _module.Warrior, AID.SoftEnrageW, 3.1f);
        ActorCastStart(id + 0x11u, _module.Soldier, AID.SoftEnrageS, 3f);
        ActorCastEnd(id + 0x12u, _module.Warrior, 2f, false, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x13u, _module.Soldier, 3f, false, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x20u, _module.Warrior, AID.SoftEnrageW, 0.1f);
        ActorCastStart(id + 0x21u, _module.Soldier, AID.SoftEnrageS, 3f);
        ActorCastEnd(id + 0x22u, _module.Warrior, 2, false, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x23u, AID.EmpyreanIniquity, 0.8f);
        ActorCastEnd(id + 0x24u, _module.Soldier, 2.2f, false, "Raidwide 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastEnd(id + 0x25u, 2.8f, "Raidwide 5")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x30u, _module.Knight, AID.SoftEnrageK, 1.3f);
        ActorCastStart(id + 0x31u, _module.Warrior, AID.SoftEnrageW, 3f);
        ActorCastEnd(id + 0x32u, _module.Knight, 2f, false, "Raidwide 6")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x33u, _module.Soldier, AID.SoftEnrageS, 1f);
        ActorCastEnd(id + 0x34u, _module.Warrior, 2f, false, "Raidwide 7")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x35u, _module.Gunner, AID.SoftEnrageG, 1f);
        ActorCastEnd(id + 0x36u, _module.Soldier, 2f, false, "Raidwide 8")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x37u, _module.Gunner, 3f, false, "Raidwide 9")
           .SetHint(StateMachine.StateHint.Raidwide);
    }
}
