namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class DRS8QueenStates : StateMachineBuilder
{
    private readonly DRS8Queen _module;

    public DRS8QueenStates(DRS8Queen module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "P1")
            .ActivateOnEnter<ArenaChange>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HPMP.CurHP <= 1u || (Module.PrimaryActor.CastInfo?.IsSpell(AID.GodsSaveTheQueen) ?? false);
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
        Cast(id, (uint)AID.EmpyreanIniquity, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CleansingSlash(uint id, float delay)
    {
        Cast(id, (uint)AID.CleansingSlashFirst, delay, 5, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<CleansingSlashSecond>(id + 2, 3.1f, comp => comp.NumCasts > 0, "Tankbuster 2")
            .ActivateOnEnter<CleansingSlashSecond>()
            .DeactivateOnExit<CleansingSlashSecond>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void QueensWill(uint id, float delay)
    {
        // right before cast start: ENVC 19.00200010, guards gain 2056 status with extra 0xE1 + PATE 1E43
        Cast(id, (uint)AID.QueensWill, delay, 5)
            .ActivateOnEnter<QueensWill>(); // statuses appear ~0.7s after cast end
        Cast(id + 0x10, (uint)AID.NorthswainsGlow, 3.2f, 3)
            .ActivateOnEnter<NorthswainsGlow>(); // aoe casts start ~0.8s after visual cast end
        Cast(id + 0x20, (uint)AID.BeckAndCallToArmsWillKW, 3.1f, 5);
        ComponentCondition<NorthswainsGlow>(id + 0x30, 2.6f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<NorthswainsGlow>();

        CastStart(id + 0x40, (uint)AID.BeckAndCallToArmsWillSG, 0.5f);
        ComponentCondition<QueensWill>(id + 0x41, 1.1f, comp => comp.NumCasts >= 2, "Easy chess 1");
        CastEnd(id + 0x42, 3.9f);
        ComponentCondition<QueensWill>(id + 0x43, 4.3f, comp => comp.NumCasts >= 4, "Easy chess 2")
            .DeactivateOnExit<QueensWill>();
    }

    private void QueensEdict(uint id, float delay)
    {
        Cast(id, (uint)AID.QueensEdict, delay, 5)
            .ActivateOnEnter<QueensEdict>(); // safezone envcontrol, statuses on guards and players appear ~0.8s after cast end
        Targetable(id + 0x10, false, 3.1f, "Disappear");
        Cast(id + 0x20, (uint)AID.BeckAndCallToArmsEdictKW, 0.1f, 16.3f);

        CastStart(id + 0x30, (uint)AID.BeckAndCallToArmsEdictSG, 3.2f);
        ComponentCondition<QueensEdict>(id + 0x31, 1.3f, comp => comp.NumCasts >= 2, "Super chess rows"); // 1st edict movement starts ~0.2s before this
        CastEnd(id + 0x32, 7.4f);

        ComponentCondition<QueensEdict>(id + 0x40, 2.4f, comp => comp.NumStuns > 0, "Super chess columns");
        ComponentCondition<QueensEdict>(id + 0x41, 1.9f, comp => comp.NumCasts >= 4);

        CastStart(id + 0x50, (uint)AID.GunnhildrsBlades, 2.8f);
        ComponentCondition<QueensEdict>(id + 0x51, 1.3f, comp => comp.NumStuns == 0); // 2nd edict movement starts ~1.0s before this
        ComponentCondition<QueensEdict>(id + 0x52, 9, comp => comp.NumStuns > 0, "Super chess safespot");
        CastEnd(id + 0x53, 3.7f)
            .DeactivateOnExit<QueensEdict>();

        Targetable(id + 0x60, true, 3.1f, "Reappear");
    }

    private void GodsSaveTheQueen(uint id, float delay)
    {
        Cast(id, (uint)AID.GodsSaveTheQueen, delay, 5);
        ComponentCondition<GodsSaveTheQueen>(id + 0x10, 2.1f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<GodsSaveTheQueen>()
            .DeactivateOnExit<GodsSaveTheQueen>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MaelstromsBolt(uint id, float delay)
    {
        ComponentCondition<MaelstromsBolt>(id, delay, comp => comp.NumCasts > 0, "Reflect/raidwide")
            .ActivateOnEnter<MaelstromsBolt>()
            .DeactivateOnExit<MaelstromsBolt>();
    }

    private void RelentlessPlay1(uint id, float delay)
    {
        Cast(id, (uint)AID.RelentlessPlay, delay, 5);

        // +3.0s: tethers/icons
        ActorCast(id + 0x10, _module.Warrior, (uint)AID.ReversalOfForces, 3.1f, 4); // gunner casts automatic turret - starts 1s later, ends at the same time

        // +1.0s: tethers are replaced with statuses
        CastStart(id + 0x20, (uint)AID.NorthswainsGlow, 1.1f);
        // +0.1s: turrets spawn
        ActorCastStart(id + 0x21, _module.Gunner, (uint)AID.Reading, 2.1f);
        CastEnd(id + 0x22, 0.9f);
        ActorCastEnd(id + 0x23, _module.Gunner, 2.1f)
            .ActivateOnEnter<NorthswainsGlow>(); // aoes start ~0.8s after visual cast end
        // +1.0s: unseen statuses

        ActorCastStart(id + 0x30, _module.Warrior, (uint)AID.WindsOfWeight, 3.0f);
        ActorCastStart(id + 0x31, _module.Gunner, (uint)AID.QueensShot, 0.2f)
            .ActivateOnEnter<WindsOfWeight>();
        ActorCastEnd(id + 0x32, _module.Warrior, 5.8f, false, "Wind/gravity")
            .ActivateOnEnter<QueensShot>()
            .DeactivateOnExit<WindsOfWeight>()
            .DeactivateOnExit<NorthswainsGlow>();
        ActorCastEnd(id + 0x33, _module.Gunner, 1.2f, false, "Face gunner")
            .DeactivateOnExit<QueensShot>();

        // +0.5s: turrets start their casts
        ComponentCondition<TurretsTourUnseen>(id + 0x40, 3.5f, comp => comp.NumCasts > 0, "Face turret")
            .ActivateOnEnter<TurretsTourUnseen>()
            .DeactivateOnExit<TurretsTourUnseen>();
    }

    private void RelentlessPlay2(uint id, float delay)
    {
        Cast(id, (uint)AID.RelentlessPlay, delay, 5);
        ActorCast(id + 0x10, _module.Knight, (uint)AID.ShieldOmen, 3.2f, 3);

        ActorCastStart(id + 0x20, _module.Soldier, (uint)AID.DoubleGambit, 4.5f);
        Targetable(id + 0x21, false, 0.4f, "Disappear");
        ActorCastStart(id + 0x22, _module.Knight, (uint)AID.OptimalOffensive, 1.4f);
        CastStartMulti(id + 0x23, [(uint)AID.JudgmentBladeR, (uint)AID.JudgmentBladeL], 1.9f)
            .ActivateOnEnter<OptimalOffensive>()
            .ActivateOnEnter<OptimalOffensiveKnockback>()
            .ActivateOnEnter<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x24, _module.Soldier, 1.3f)
            .ActivateOnEnter<JudgmentBlade>();
        ActorCastStart(id + 0x25, _module.Soldier, (uint)AID.SecretsRevealed, 3.2f); // right before cast start, 2 unsafe avatars are tethered to caster
        ActorCastEnd(id + 0x26, _module.Knight, 0.6f, false, "Charge + Knockback")
            .DeactivateOnExit<OptimalOffensive>()
            .DeactivateOnExit<OptimalOffensiveKnockback>();
        CastEnd(id + 0x27, 1.9f);
        ComponentCondition<JudgmentBlade>(id + 0x28, 0.3f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<UnluckyLotAetherialSphere>(id + 0x29, 0.5f, comp => comp.NumCasts > 0, "Sphere explosion")
            .DeactivateOnExit<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x2A, _module.Soldier, 1.7f);

        CastMulti(id + 0x30, [(uint)AID.JudgmentBladeR, (uint)AID.JudgmentBladeL], 2.2f, 7)
            .ActivateOnEnter<JudgmentBlade>()
            .ActivateOnEnter<PawnOff>(); // cast starts ~2.7s after judgment blade; we could show hints much earlier based on tethers
        ComponentCondition<JudgmentBlade>(id + 0x32, 0.3f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<PawnOff>(id + 0x33, 2.4f, comp => comp.NumCasts > 0, "Real/fake aoes")
            .DeactivateOnExit<PawnOff>();

        Targetable(id + 0x40, true, 2.0f, "Reappear");
    }

    private void RelentlessPlay3(uint id, float delay)
    {
        Cast(id, (uint)AID.RelentlessPlay, delay, 5);
        ActorCastMulti(id + 0x10, _module.Knight, [(uint)AID.SwordOmen, (uint)AID.ShieldOmen], 3.1f, 3);

        // note: gunner starts automatic turret visual together with optimal play
        ActorCastMulti(id + 0x20, _module.Knight, [(uint)AID.OptimalPlaySword, (uint)AID.OptimalPlayShield], 6.5f, 5, false, "Cone + circle/donut")
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<OptimalPlayCone>()
            .DeactivateOnExit<OptimalPlaySword>()
            .DeactivateOnExit<OptimalPlayShield>()
            .DeactivateOnExit<OptimalPlayCone>();

        ActorCast(id + 0x30, _module.Gunner, (uint)AID.TurretsTour, 1, 5, false, "Turrets start")
            .ActivateOnEnter<TurretsTour>();
        ComponentCondition<TurretsTour>(id + 0x40, 1.7f, comp => comp.NumCasts >= 4, "Turrets resolve")
            .DeactivateOnExit<TurretsTour>();
    }

    private void RelentlessPlay4(uint id, float delay)
    {
        Cast(id, (uint)AID.RelentlessPlay, delay, 5);
        ActorCast(id + 0x10, _module.Warrior, (uint)AID.Bombslinger, 3.1f, 3);
        // +0.9s: bombs spawn

        ActorCastStart(id + 0x20, _module.Warrior, (uint)AID.ReversalOfForces, 3.2f); // icons/tethers appear ~0.1s before cast start
        CastStart(id + 0x21, (uint)AID.HeavensWrath, 2.9f);
        ActorCastEnd(id + 0x22, _module.Warrior, 1.1f);
        CastEnd(id + 0x23, 1.9f);

        ComponentCondition<HeavensWrathAOE>(id + 0x30, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<HeavensWrathAOE>();
        ActorCastStartMulti(id + 0x31, _module.Soldier, [(uint)AID.FieryPortent, (uint)AID.IcyPortent], 2.2f)
            .ActivateOnEnter<HeavensWrathKnockback>()
            .ActivateOnEnter<AboveBoard>();
        ComponentCondition<HeavensWrathAOE>(id + 0x32, 2.8f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<FieryIcyPortent>()
            .DeactivateOnExit<HeavensWrathAOE>()
            .DeactivateOnExit<HeavensWrathKnockback>();
        ActorCastStart(id + 0x33, _module.Warrior, (uint)AID.AboveBoard, 0.5f);
        Targetable(id + 0x34, false, 1.8f, "Boss disappears");
        ActorCastEnd(id + 0x35, _module.Soldier, 0.9f, false, "Move/stay")
            .DeactivateOnExit<FieryIcyPortent>();
        ActorCastEnd(id + 0x36, _module.Soldier, 3.3f);

        ComponentCondition<AboveBoard>(id + 0x40, 4.2f, comp => comp.CurState == AboveBoard.State.ThrowUpDone, "Throw up");
        ComponentCondition<AboveBoard>(id + 0x41, 2.0f, comp => comp.CurState == AboveBoard.State.ShortExplosionsDone, "Bombs 1");
        ComponentCondition<AboveBoard>(id + 0x42, 4.2f, comp => comp.CurState == AboveBoard.State.LongExplosionsDone, "Bombs 2")
            .DeactivateOnExit<AboveBoard>();

        Targetable(id + 0x50, true, 3.0f, "Boss reappears");
    }

    private void RelentlessPlay5(uint id, float delay)
    {
        Cast(id, (uint)AID.RelentlessPlay, delay, 5);

        ActorCastStart(id + 0x10, _module.Warrior, (uint)AID.SoftEnrageW, 3.1f);
        ActorCastStart(id + 0x11, _module.Soldier, (uint)AID.SoftEnrageS, 3);
        ActorCastEnd(id + 0x12, _module.Warrior, 2, false, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x13, _module.Soldier, 3, false, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x20, _module.Warrior, (uint)AID.SoftEnrageW, 0.1f);
        ActorCastStart(id + 0x21, _module.Soldier, (uint)AID.SoftEnrageS, 3);
        ActorCastEnd(id + 0x22, _module.Warrior, 2, false, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x23, (uint)AID.EmpyreanIniquity, 0.8f);
        ActorCastEnd(id + 0x24, _module.Soldier, 2.2f, false, "Raidwide 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastEnd(id + 0x25, 2.8f, "Raidwide 5")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x30, _module.Knight, (uint)AID.SoftEnrageK, 1.3f);
        ActorCastStart(id + 0x31, _module.Warrior, (uint)AID.SoftEnrageW, 3);
        ActorCastEnd(id + 0x32, _module.Knight, 2, false, "Raidwide 6")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x33, _module.Soldier, (uint)AID.SoftEnrageS, 1);
        ActorCastEnd(id + 0x34, _module.Warrior, 2, false, "Raidwide 7")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x35, _module.Gunner, (uint)AID.SoftEnrageG, 1);
        ActorCastEnd(id + 0x36, _module.Soldier, 2, false, "Raidwide 8")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x37, _module.Gunner, 3, false, "Raidwide 9")
           .SetHint(StateMachine.StateHint.Raidwide);
    }
}
