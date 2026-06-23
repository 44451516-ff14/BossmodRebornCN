namespace BossMod.Dawntrail.Ultimate.DMU;

[SkipLocalsInit]
sealed class DMUStates : StateMachineBuilder {
    private readonly DMU _module;

    public DMUStates(DMU module) : base(module) {
        _module = module;

        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, Phase2, "P2")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.BossP2()?.IsDeadOrDestroyed == true;
    }

    private void Phase2(uint id) {
        ActorTargetable(id, _module.BossP2, true, 10.3f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x10, _module.BossP2, (uint)AID.UltimateEmbrace, 7.0f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();
        ActorCast(id + 0x20, _module.BossP2, (uint)AID.Forsaken, 8.0f, 7.0f, true, "Raidwide")
            .ActivateOnEnter<Forsaken>()
            .DeactivateOnExit<Forsaken>()
            .ActivateOnEnter<ForsakenShapes>()
            .ActivateOnEnter<PathOfLight>()
            .ActivateOnEnter<ForsakenBaitsSpreadStacks>()
            .ActivateOnExit<ForsakenBaitsBossClones>()
            .ActivateOnEnter<ForsakenBaitsCone>()
            .ActivateOnEnter<ForsakenSolverSet1>();

        // Tower set 1
        ComponentCondition<ForsakenShapes>(id + 0x30, 13.2f, o => o.currentTowerSet > 1, "1st Tower Set")
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 2
        ComponentCondition<ForsakenShapes>(id + 0x40, 10.0f, o => o.currentTowerSet > 2, "2nd Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>();

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x50, 5.7f, o => o.NumCasts > 0, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        ComponentCondition<AllThingsEnding>(id + 0x55, 5.0f, o => o.NumCasts > 4, "Boss/Clones baits Resolve")
            .DeactivateOnExit<AllThingsEnding>();

        // Tower set 3
        ComponentCondition<ForsakenShapes>(id + 0x60, 0.4f, o => o.currentTowerSet > 3, "3rd Tower Set")
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 4
        ComponentCondition<ForsakenShapes>(id + 0x70, 10.0f, o => o.currentTowerSet > 4, "4th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>();

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x80, 5.7f, o => o.NumCasts > 0, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        ComponentCondition<AllThingsEnding>(id + 0x85, 5.0f, o => o.NumCasts > 4, "Boss/Clones baits Resolve")
            .DeactivateOnExit<AllThingsEnding>();

        // Tower set 5
        ComponentCondition<ForsakenShapes>(id + 0x90, 0.4f, o => o.currentTowerSet > 5, "5th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 6
        ComponentCondition<ForsakenShapes>(id + 0x100, 10.0f, o => o.currentTowerSet > 6, "6th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>();

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x110, 5.7f, o => o.NumCasts > 0, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        ComponentCondition<AllThingsEnding>(id + 0x115, 5.0f, o => o.NumCasts > 4, "Boss/Clones baits Resolve")
            .DeactivateOnExit<AllThingsEnding>();

        // Tower set 7
        ComponentCondition<ForsakenShapes>(id + 0x120, 0.4f, o => o.currentTowerSet > 7, "7th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 8
        ComponentCondition<ForsakenShapes>(id + 0x130, 10.0f, o => o.currentTowerSet > 8, "8th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnEnter<AllThingsEnding>();

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x140, 5.4f, o => o.NumCasts > 0, "Boss/Clones baits");

        ComponentCondition<AllThingsEnding>(id + 0x145, 5.0f, o => o.NumCasts > 7, "Boss/Clones baits Resolve")
            .DeactivateOnExit<ForsakenShapes>()
            .DeactivateOnExit<ForsakenBaitsSpreadStacks>()
            .DeactivateOnExit<ForsakenBaitsCone>()
            .DeactivateOnExit<ForsakenBaitsBossClones>()
            .DeactivateOnExit<AllThingsEnding>();

        ActorCast(id + 0x150, _module.BossP2, (uint)AID.LightOfJudgmentP2, 4.1f, 5.0f, true, "Raidwide")
            .ActivateOnEnter<LightOfJudgmentP2>()
            .DeactivateOnExit<LightOfJudgmentP2>();

        ActorCast(id + 0x160, _module.BossP2, (uint)AID.Trine, 8.2f, 3.0f, true, "Trine")
            .ActivateOnEnter<Trine>();

        ActorCastMulti(id + 0x170, _module.BossP2, [(uint)AID.WingsOfDestructionLeft, (uint)AID.WingsOfDestructionRight], 3.1f, 4.0f, true, "Left / Right")
            .ActivateOnEnter<WingsOfDestructionLeftRight>()
            .DeactivateOnExit<WingsOfDestructionLeftRight>();
        ComponentCondition<Trine>(id + 0x180, 5.7f, o => o.NumCasts == 10, "Trine 1 Explosions");
        ActorCastStart(id + 0x190, _module.BossP2, (uint)AID.WingsOfDestructionTB, 0.6f, true)
            .ActivateOnEnter<WingsOfDestructionTB>();
        ComponentCondition<Trine>(id + 0x200, 1.5f, o => o.NumCasts == 13, "Trine 2 Explosions");
        ComponentCondition<Trine>(id + 0x210, 2.0f, o => o.NumCasts == 22, "Trine 3 Explosions")
            .DeactivateOnExit<Trine>();
        ComponentCondition<WingsOfDestructionTB>(id + 0x220, 0.6f, o => o.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<WingsOfDestructionTB>();
        ActorCast(id + 0x220, _module.BossP2, (uint)AID.UltimateEmbrace, 2.0f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();

        Timeout(id + 0xFF0000, 10000, "???");
    }

    private void Phase1(uint id) {
        Phase1RevoltingRuinIII(id, 10.1f);
        Phase1GravenImage(id + 0x1000, 7.7f);
        Phase1Gravitas(id + 0x2000, 4.0f);
        Phase1TeleTrouncing(id + 0x3000, 6.8f);
    }

    void Phase1RevoltingRuinIII(uint id, float delay) {
        Cast(id, (uint)AID.RevoltingRuinIII, delay, 5, "1st Tankbuster")
            .ActivateOnEnter<RevoltingRuinIII>();
        ComponentCondition<RevoltingRuinIII>(id + 0x05, 3.25f, o => o.NumCasts > 1, "2nd Tankbuster")
            .DeactivateOnExit<RevoltingRuinIII>();
    }

    void Phase1GravenImage(uint id, float delay) {
        Cast(id + 0x10, (uint)AID.GravenImage, delay, 3.0f, "Graven Image")
            .ActivateOnEnter<GravenImage>()
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<StackSpreadOrbs>();
        CastStart(id + 0x20, (uint)AID.MysteryMagic, 3.2f);
        ComponentCondition<GravenImage>(id + 0x30, 2.6f, o => o.NumCasts > 0, "Knockbacks")
            .DeactivateOnExit<GravenImage>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x40, 2.3f, o => o.NumCasts > 0, "Blizzard safe spots")
            .DeactivateOnExit<BlizzardSafeSpots>();

        ComponentCondition<StackSpreadOrbs>(id + 0x40, 0.8f, o => !o.Active, "Stack / Spread")
            .DeactivateOnExit<StackSpreadOrbs>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnExit<DoubleTroubleTrapStacks>();

        ComponentCondition<WaveCannon>(id + 0x50, 4.2f, o => o.NumCasts > 0, "Wave Cannon Spreads")
            .DeactivateOnExit<WaveCannon>()
            .ActivateOnEnter<WaveCannonTowers>();

        ComponentCondition<WaveCannonTowers>(id + 0x60, 3.6f, o => o.NumCasts > 0, "Towers resolve")
            .DeactivateOnExit<WaveCannonTowers>();

        CastStart(id + 0x70, (uint)AID.MysteryMagic, 2.7f)
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnEnter<BlizzardSafeSpots>();

        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x80, 1.0f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>();

        ComponentCondition<BlizzardSafeSpots>(id + 0x90, 3.9f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();
    }

    void Phase1Gravitas(uint id, float delay) {
        Cast(id + 0x90, (uint)AID.LightOfJudgment, delay, 5.0f, "Raidwide")
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x100, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x110, 2.1f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x120, 2.1f, o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();

        Cast(id + 0x130, (uint)AID.GravenImage, 7.3f, 3, "Graven Image")
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<Gravitas>()
            .ActivateOnEnter<GravitasPuddles>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x140, 7.15f, o => o.NumCasts > 0, "Blizzard safe spots + Stack")
            .DeactivateOnExit<BlizzardSafeSpots>();
        ComponentCondition<Gravitas>(id + 0x150, 4.1f, o => !o.Active, "Spreads")
            .ActivateOnEnter<RevoltingRuinIII>()
            .ActivateOnEnter<GravitationalWave>();
        Cast(id + 0x160, (uint)AID.RevoltingRuinIII, 0.7f, 5, "1st Tankbuster");
        ComponentCondition<RevoltingRuinIII>(id + 0x165, 3.25f, o => o.NumCasts > 1, "2nd Tankbuster")
            .DeactivateOnExit<RevoltingRuinIII>();
        ComponentCondition<GravitationalWave>(id + 0x170, 0.80f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>();
        ComponentCondition<Gravitas>(id + 0x180, 4.6f, o => o.NumCasts > 4, "Stack");
        ComponentCondition<Gravitas>(id + 0x190, 4.0f, o => !o.Active, "Spreads")
            .DeactivateOnExit<Gravitas>()
            .ActivateOnEnter<GravitationalWave>();
        ComponentCondition<GravitationalWave>(id + 0x200, 4.5f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>()
            .ActivateOnEnter<DoubleTroubleTrapKnockback>()
            .ActivateOnEnter<DoubleTroubleTrapStacks>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x210, 3.8f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>();
        Cast(id + 0x220, (uint)AID.LightOfJudgment, 9.2f, 5.0f, "Raidwide")
            .DeactivateOnExit<GravitasPuddles>()
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x215, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x220, 2.1f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x225, 2.1f, o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();
    }

    void Phase1TeleTrouncing(uint id, float delay) {
        Cast(id + 0x230, (uint)AID.TeleTrouncing, delay, 5.0f, "TeleTrouncing")
            .ActivateOnEnter<TeleTrouncing>();
        ComponentCondition<TeleTrouncing>(id + 0x240, 7.8f, o => o.NumCasts > 0, "First Arrows");
        ComponentCondition<TeleTrouncing>(id + 0x250, 3.0f, o => o.NumCasts > 9, "Second Arrows")
            .DeactivateOnExit<TeleTrouncing>()
            .ActivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnExit<DoubleTroubleTrapStacks>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x260, 5.4f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnExit<GravenImage2>();
        ComponentCondition<GravenImage2>(id + 0x270, 5.6f, o => !o.Active, "Sleeps + Confusion Spreads")
            .DeactivateOnExit<GravenImage2>()
            .ActivateOnExit<Gaze>();

        CastStart(id + 0x280, (uint)AID.MysteryMagic, 7.9f)
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnEnter<StackSpreadOrbs>();

        Condition(id + 0x290, 5.0f, () => Module.FindComponent<LightningSafeSpots>()!.NumCasts > 0 && Module.FindComponent<Gaze>()!.NumCasts > 0, "Lightning + Gaze")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<StackSpreadOrbs>()
            .DeactivateOnExit<Gaze>();

        Targetable(id + 0x1000, false, 11.0f, "Boss disappears");
    }
}
