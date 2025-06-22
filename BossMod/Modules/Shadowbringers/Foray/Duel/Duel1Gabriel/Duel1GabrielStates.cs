namespace BossMod.Shadowbringers.Foray.Duel.Duel1Gabriel;

sealed class GabrielStates : StateMachineBuilder
{
    public GabrielStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<MagitekCannonVoidzone>()
            .ActivateOnEnter<MagitekMissile>();
    }

    private void SinglePhase(uint id) // boss skips mechanics that are player targeted while reraiser is in action... can cause bugs in timeline
    {
        MagitekCannon(id, 8.2f);
        DynamicSensoryJammer(id + 0x10000u, 8.9f);
        EnhancedMobility(id + 0x20000u, 5.4f);
        MagitekCannon(id + 0x30000u, 9.2f);
        IntegratedScannerEnhancedMobility2x(id + 0x40000u, 5.3f);
        CruiseMissile(id + 0x50000u, 12.8f);
        DynamicSensoryJammer(id + 0x60000u, 0.7f);
        IntegratedScannerEnhancedMobilityFullyAnalyzed(id + 0x70000u, 11.1f);
        MagitekCannon(id + 0x80000u, 10f);
        IntegratedScannerEnhancedMobility3x(id + 0x90000u, 5.3f);
        CruiseMissile(id + 0xA0000u, 12.8f);
        DynamicSensoryJammer(id + 0xB0000u, 0.7f);
        MagitekCannon(id + 0xC0000u, 7.1f);
        IntegratedScannerEnhancedMobility3x(id + 0xD0000u, 5.3f);
        CruiseMissile(id + 0xE0000u, 12.8f);
        DynamicSensoryJammer(id + 0xF0000u, 0.7f);
        MagitekCannon(id + 0x100000u, 7.1f);
        IntegratedScannerEnhancedMobility3x(id + 0x110000u, 5.3f);
        CruiseMissile(id + 0x120000u, 12.8f);
        DynamicSensoryJammer(id + 0x130000u, 0.7f);
        MagitekCannon(id + 0x140000u, 7.1f);
        IntegratedScannerEnhancedMobility3x(id + 0x150000u, 5.3f);
        CruiseMissile(id + 0x160000u, 12.8f);
        SimpleState(id + 0x170000u, 2.3f, "Enrage");
    }

    private void DynamicSensoryJammer(uint id, float delay)
    {
        ComponentCondition<DynamicSensoryJammer>(id, delay, comp => comp.PlayerStates[0] != default, "Extreme caution starts")
            .ActivateOnExit<MissileLauncher>()
            .ActivateOnEnter<DynamicSensoryJammer>();
        ComponentCondition<DynamicSensoryJammer>(id + 0x10u, 5f, comp => comp.PlayerStates[0] == default, "Extreme caution resolves")
            .DeactivateOnExit<DynamicSensoryJammer>();
        ComponentCondition<MissileLauncher>(id + 0x20u, 2.1f, comp => comp.NumCasts != 0, "Circle AOEs")
            .DeactivateOnExit<MissileLauncher>();
    }

    private void EnhancedMobility(uint id, float delay)
    {
        ComponentCondition<EnhancedMobility>(id, delay, comp => comp.NumCasts != default, "Knockback")
            .ActivateOnEnter<EnhancedMobility>()
            .DeactivateOnExit<EnhancedMobility>();
    }

    private void IntegratedScannerEnhancedMobility2x(uint id, float delay)
    {
        Cast(id, (uint)AID.IntegratedScanner, delay, 4f, "Apply weakspot");
        ComponentCondition<EnhancedMobility>(id + 0x10u, 12.1f, comp => comp.NumCasts != default, "Knockback 1")
            .ActivateOnEnter<EnhancedMobility>()
            .ActivateOnEnter<EnhancedMobilityWeakpoint>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<Burst>(id + 0x20u, 1.3f, comp => comp.NumCasts != default, "Tower 1");
        ComponentCondition<EnhancedMobility>(id + 0x30u, 5.1f, comp => comp.NumCasts == 2, "Knockback 2");
        ComponentCondition<Burst>(id + 0x40u, 1.3f, comp => comp.NumCasts == 2, "Tower 2")
            .DeactivateOnExit<EnhancedMobility>()
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<EnhancedMobilityWeakpoint>();
    }

    private void IntegratedScannerEnhancedMobility3x(uint id, float delay)
    {
        Cast(id, (uint)AID.IntegratedScanner, delay, 4f, "Apply weakspot");
        ComponentCondition<EnhancedMobility>(id + 0x10u, 12.1f, comp => comp.NumCasts != default, "Knockback 1")
            .ActivateOnEnter<EnhancedMobility>()
            .ActivateOnEnter<EnhancedMobilityWeakpoint>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<Burst>(id + 0x20u, 1.3f, comp => comp.NumCasts != default, "Tower 1");
        ComponentCondition<EnhancedMobility>(id + 0x30u, 5.1f, comp => comp.NumCasts == 2, "Knockback 2");
        ComponentCondition<Burst>(id + 0x40u, 1.3f, comp => comp.NumCasts == 2, "Tower 2");
        ComponentCondition<EnhancedMobility>(id + 0x50u, 5.1f, comp => comp.NumCasts == 3, "Knockback 3");
        ComponentCondition<Burst>(id + 0x60u, 1.3f, comp => comp.NumCasts == 3, "Tower 3")
            .DeactivateOnExit<EnhancedMobility>()
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<EnhancedMobilityWeakpoint>();
    }

    private void IntegratedScannerEnhancedMobilityFullyAnalyzed(uint id, float delay)
    {
        Cast(id, (uint)AID.IntegratedScanner, delay, 4f, "Apply impossible weakspot");
        ComponentCondition<EnhancedMobility>(id + 0x10u, 12.1f, comp => comp.NumCasts != default, "Knockback")
            .ActivateOnEnter<EnhancedMobility>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<EnhancedMobilityFullyAnalyzedHint>();
        ComponentCondition<Burst>(id + 0x20u, 1.3f, comp => comp.NumCasts != default, "Tower 1")
            .DeactivateOnExit<EnhancedMobility>()
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<EnhancedMobilityFullyAnalyzedHint>();
    }

    private void MagitekCannon(uint id, float delay)
    {
        Cast(id, (uint)AID.MagitekCannon, delay, 2f, "Baited voidzones")
            .ActivateOnExit<MagitekCannonChase>();
        for (var i = 1; i <= 5; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i;
            var desc = $"Baited voidzone {i}";
            var cond = ComponentCondition<MagitekCannonVoidzone>(offset, i == 1 ? 7.1f : 3.3f, comp => comp.NumCasts == casts, desc);
            if (i == 5)
            {
                cond
                    .DeactivateOnExit<MagitekCannonChase>()
                    .ExecOnExit<MagitekCannonVoidzone>(comp => comp.NumCasts = default);
            }
        }
    }

    private void CruiseMissile(uint id, float delay)
    {
        ComponentCondition<InfraredHomingMissileBait>(id, delay, comp => comp.CurrentBaits.Count != 0, "Prey + missiles")
            .ActivateOnEnter<InfraredHomingMissileBait>();
        ComponentCondition<InfraredHomingMissile>(id + 0x10u, 4.3f, comp => comp.Casters.Count != 0, "Proximity AOE appears")
            .ActivateOnEnter<InfraredHomingMissile>()
            .DeactivateOnExit<InfraredHomingMissileBait>();
        ComponentCondition<InfraredHomingMissile>(id + 0x20u, 6f, comp => comp.NumCasts == 1, "Proximity AOE resolves")
            .DeactivateOnExit<InfraredHomingMissile>();
    }
}
