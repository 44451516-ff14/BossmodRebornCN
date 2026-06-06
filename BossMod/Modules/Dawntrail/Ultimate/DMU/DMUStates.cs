namespace BossMod.Dawntrail.Ultimate.DMU;

[SkipLocalsInit]
sealed class KefkaStates : StateMachineBuilder
{
    public KefkaStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Phase1(id, 10.1f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void Phase1(uint id, float delay)
    {
        Cast(id, (uint)AID.RevoltingRuinIII, delay, 5, "1st Tankbuster")
            .ActivateOnEnter<RevoltingRuinIII>();
        ComponentCondition<RevoltingRuinIII>(id + 0x05, 3.25f, o => o.NumCasts > 1, "2nd Tankbuster")
            .DeactivateOnExit<RevoltingRuinIII>();
        Cast(id + 0x10, (uint)AID.GravenImage, 7.7f, 3, "Graven Image")
            .ActivateOnEnter<GravenImage>()
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<StackSpreadOrbs>();
        ComponentCondition<GravenImage>(id + 0x20, 5.8f, o => o.NumCasts > 0, "Knockbacks")
            .DeactivateOnExit<GravenImage>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x30, 2.3f, o => o.NumCasts > 0, "Blizzard safe spots")
            .DeactivateOnExit<BlizzardSafeSpots>();
        ComponentCondition<StackSpreadOrbs>(id + 0x40, 0.8f, o => !o.Active, "Stack / Spread")
            .DeactivateOnExit<StackSpreadOrbs>()
            .ActivateOnExit<WaveCannon>()
            .ActivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnExit<DoubleTroubleTrapStacks>();
        ComponentCondition<WaveCannon>(id + 0x50, 4.2f, o => o.NumCasts > 0, "Wave Cannon Spreads")
            .DeactivateOnExit<WaveCannon>()
            .ActivateOnEnter<WaveCannonTowers>();
        ComponentCondition<WaveCannonTowers>(id + 0x60, 3.6f, o => o.NumCasts > 0, "Towers resolve")
            .DeactivateOnExit<WaveCannonTowers>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x70, 3.8f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnEnter<BlizzardSafeSpots>();
        ComponentCondition<BlizzardSafeSpots>(id + 0x80, 3.8f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();

        Cast(id + 0x90, (uint)AID.LightOfJudgment, 4.0f, 5.0f, "Raidwide")
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x100, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x110, 2.1f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x120, 2.0f, o => o.NumCasts > 2, "3rd Tankbuster")
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
        ComponentCondition<GravitationalWave>(id + 0x170, 0.85f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>();
        ComponentCondition<Gravitas>(id + 0x180, 4.6f, o => o.NumCasts > 4, "Stack");
        ComponentCondition<Gravitas>(id + 0x190, 4.0f, o => !o.Active, "Spreads")
            .DeactivateOnExit<Gravitas>()
            .ActivateOnEnter<GravitationalWave>();
        ComponentCondition<GravitationalWave>(id + 0x200, 4.5f, o => o.NumCasts > 0, "Left/Right Cleave")
            .DeactivateOnExit<GravitationalWave>()
            .ActivateOnEnter<DoubleTroubleTrapKnockback>()
            .ActivateOnEnter<DoubleTroubleTrapStacks>();
        ComponentCondition<DoubleTroubleTrapKnockback>(id + 0x210, 4.0f, o => o.NumCasts > 0, "Stacks + Knockbacks")
            .DeactivateOnExit<DoubleTroubleTrapStacks>()
            .DeactivateOnExit<DoubleTroubleTrapKnockback>();

        Cast(id + 0x220, (uint)AID.LightOfJudgment, 9.1f, 5.0f, "Raidwide")
            .DeactivateOnExit<GravitasPuddles>()
            .ActivateOnEnter<LightOfJudgment>()
            .DeactivateOnExit<LightOfJudgment>()
            .ActivateOnEnter<HyperDrive>();
        ComponentCondition<HyperDrive>(id + 0x215, 3.2f, o => o.NumCasts > 0, "1st Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x220, 2.1f, o => o.NumCasts > 1, "2nd Tankbuster");
        ComponentCondition<HyperDrive>(id + 0x225, 2.1f, o => o.NumCasts > 2, "3rd Tankbuster")
            .DeactivateOnExit<HyperDrive>();

        Cast(id + 0x230, (uint)AID.TeleTrouncing, 6.8f, 5.0f, "TeleTrouncing")
            .ActivateOnEnter<TeleTrouncing>();

        //TODO fix times
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
            .ActivateOnExit<LightningSafeSpots>()
            .ActivateOnExit<StackSpreadOrbs>()
            .ActivateOnExit<Gaze>();
        ComponentCondition<Gaze>(id + 0x280, 13.1f, o => o.NumCasts > 0, "Stack/Spread + Blizzard + Gaze")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<StackSpreadOrbs>()
            .DeactivateOnExit<Gaze>();
    }
}
