namespace BossMod.Heavensward.Extreme.Ex3Thordan;

sealed class Ex3ThordanStates : StateMachineBuilder
{
    private readonly Ex3Thordan _module;

    public Ex3ThordanStates(Ex3Thordan module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "Phase 1")
            .OnExit(() => Module.Arena.Bounds = Ex3Thordan.DefaultBounds)
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1u, AddPhase, "Add phase")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (_module.BossJanlenoux()?.IsDeadOrDestroyed ?? false) && (_module.BossAdelphel()?.IsDeadOrDestroyed ?? false);
        DeathPhase(2u, Phase2);
    }

    private void Phase1(uint id)
    {
        // first 'phase'
        AscalonsMight(id, 6.1f);
        Meteorain(id + 0x10000u, 7.1f);
        AscalonsMercy(id + 0x20000u, 2.0f);
        AscalonsMight(id + 0x30000u, 3.5f);
        DragonsEyeGaze(id + 0x40000u, 2.1f);
        AscalonsMight(id + 0x50000u, 5.2f);
        LightningStorm(id + 0x60000u, 5.1f);
        DragonsRage(id + 0x70000u, 1.4f);
        AncientQuaga(id + 0x80000u, 2.2f);
        AscalonsMight(id + 0x90000u, 6.2f);
        HeavenlyHeel(id + 0xA0000u, 2.2f);
        AscalonsMight(id + 0xB0000u, 2.1f, true);
        Targetable(id + 0xC0000u, false, 2f, "Boss disappears");
    }

    private void AddPhase(uint id)
    {
        // intermission part 1
        HeavensflameChainsConviction(id, 18.3f); // note: quite large variance
        SacredCrossSpiralThrust(id + 0x10000u, 3f);
        AdelphelJanlenoux(id + 0x20000u, 7f);
    }

    private void Phase2(uint id)
    {
        // intermission part 2
        SpiralPierceDimensionalCollapseHiemalStormComets(id, 5f);
        LightOfAscalonUltimateEnd(id + 0x10000u, 6.3f);
        Targetable(id + 0x20000u, true, 4.5f, "Boss targetable");

        // second 'phase'
        KnightsOfTheRound1(id + 0x30000u, 3.2f);
        AncientQuaga(id + 0x40000u, 9.2f);
        KnightsOfTheRound2(id + 0x50000u, 2.2f);
        AscalonsMight(id + 0x60000u, 5.2f); // note: sometimes it's 3.2s instead
        KnightsOfTheRound3(id + 0x70000u, 6.2f);
        HeavenlyHeel(id + 0x80000u, 7.2f);
        AscalonsMight(id + 0x90000u, 2.1f);
        KnightsOfTheRound4(id + 0xA0000u, 7.2f);
        KnightsOfTheRound5(id + 0xB0000u, 2.2f);
        AncientQuaga(id + 0xC0000u, 4.3f);
        HeavenlyHeel(id + 0xD0000u, 5.2f);
        AscalonsMightEnrage(id + 0xE0000u, 2.1f);
        for (var i = 0u; i < 100u; ++i)
        {
            Cast(id + 0xF0000u + 2u * i, (uint)AID.Enrage, 2.1f, 10f, "Enrage"); // enrage only does 99999 damage and can potentiall repeat until duty timer runs out
        }
    }

    private void AscalonsMight(uint id, float delay, bool arena = false)
    {
        var condition = ComponentCondition<AscalonsMight>(id, delay, comp => comp.NumCasts > 0, "Cleave")
            .ActivateOnEnter<AscalonsMight>()
            .DeactivateOnExit<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        if (arena)
            condition = condition.ActivateOnEnter<ArenaChange>();
    }

    private void AscalonsMightEnrage(uint id, float delay)
    {
        ComponentCondition<AscalonsMight>(id, delay, comp => comp.NumCasts >= 1, "Cleave 1")
            .ActivateOnEnter<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 1u, 5.2f, comp => comp.NumCasts >= 2, "Cleave 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 2u, 3.1f, comp => comp.NumCasts >= 3, "Cleave 3")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 3u, 3.1f, comp => comp.NumCasts >= 4, "Cleave 4")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 4u, 3.1f, comp => comp.NumCasts >= 5, "Cleave 5")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AscalonsMight>(id + 5u, 3.1f, comp => comp.NumCasts >= 6, "Cleave 6")
            .DeactivateOnExit<AscalonsMight>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Meteorain(uint id, float delay)
    {
        CastStart(id, (uint)AID.Meteorain, delay, "Puddles bait");
        CastEnd(id + 1u, 2.7f)
            .ActivateOnEnter<Meteorain>();
        ComponentCondition<Meteorain>(id + 2u, 0.3f, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<Meteorain>();
    }

    private State AscalonsMercy(uint id, float delay)
    {
        return Cast(id, (uint)AID.AscalonsMercy, delay, 3f, "Cones fan")
            .ActivateOnEnter<AscalonsMercy>()
            .ActivateOnEnter<AscalonsMercyHelper>()
            .DeactivateOnExit<AscalonsMercy>()
            .DeactivateOnExit<AscalonsMercyHelper>();
    }

    private void DragonsEyeGaze(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f);
        Cast(id + 0x10u, (uint)AID.DragonsGaze, 7.2f, 3f, "Gaze")
            .ActivateOnEnter<DragonsGaze>()
            .DeactivateOnExit<DragonsGaze>();
    }

    private void LightningStorm(uint id, float delay)
    {
        CastStart(id, (uint)AID.LightningStorm, delay)
            .ActivateOnEnter<LightningStorm>();
        CastEnd(id + 1u, 3.5f);
        ComponentCondition<LightningStorm>(id + 2u, 0.7f, comp => !comp.Active, "Spread")
            .DeactivateOnExit<LightningStorm>();
    }

    private void DragonsRage(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsRage, delay, 5f, "Stack")
            .ActivateOnEnter<DragonsRage>()
            .DeactivateOnExit<DragonsRage>();
    }

    private State AncientQuaga(uint id, float delay)
    {
        return Cast(id, (uint)AID.AncientQuaga, delay, 3f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HeavenlyHeel(uint id, float delay)
    {
        Cast(id, (uint)AID.HeavenlyHeel, delay, 4f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeavensflameChainsConviction(uint id, float delay)
    {
        ComponentCondition<Heavensflame>(id, delay, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Heavensflame>();
        ComponentCondition<Conviction>(id + 0x10u, 2f, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<Conviction>();
        // +0.1s: holy chain cast?..
        ComponentCondition<Heavensflame>(id + 0x20u, 4f, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<Heavensflame>()
            .DeactivateOnExit<BurningChains>(); // note: this resolves much earlier...
        ComponentCondition<Conviction>(id + 0x30u, 3f, comp => comp.Towers.Count == 0, "Towers resolve")
            .DeactivateOnExit<Conviction>();
    }

    private void SacredCrossSpiralThrust(uint id, float delay)
    {
        ComponentCondition<SerZephirin>(id, delay, comp => comp.ActiveActors.Count != 0, "Add appears")
            .ActivateOnEnter<SerZephirin>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<SerZephirin>(id + 0x10u, 20.1f, comp => comp.ActiveActors.Count == 0, "DPS check")
            .ActivateOnEnter<SpiralThrust1>()
            .DeactivateOnExit<SerZephirin>()
            .SetHint(StateMachine.StateHint.DowntimeStart | StateMachine.StateHint.Raidwide);
        ComponentCondition<SpiralThrust>(id + 0x20u, 6, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<SpiralThrust>();
    }

    private void AdelphelJanlenoux(uint id, float delay)
    {
        ComponentCondition<SwordShieldOfTheHeavens>(id, delay, comp => comp.Active, "Adds appear")
            .ActivateOnEnter<SwordShieldOfTheHeavens>()
            .ActivateOnEnter<HeavenlySlashAdelphel>()
            .ActivateOnEnter<HeavenlySlashJanlenoux>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        // note: kill times vary wildly, so mechanics could be skipped - to avoid creating phases, we just activate all components at once and don't create intermediate states
        // +7.2s: divine right 1 start
        // +10.2s: divine right 1 finish, buffs appear - adds should be split
        // +15.3s: holy bladedance 1 start
        // +19.3s: holy bladedance 1 finish
        // +29.2s: leap icon 1
        // +32.2s: leap icon 2
        // +34.3s: divine right 2 start
        // +35.2s: leap icon 3, leap cast 1
        // +37.3s: divine right 2 finish
        // +38.2s: leap cast 2
        // +42.3s: leap cast 3, cleaves
        // +49.5s: holiest of holy 1 start
        // +52.5s: holiest of holy 1 finish
        // ...: then holy bladedance 2 > divine right 3 > holiest of holy 2 > cleaves > divine right 4 > holy bladedance 3
        ComponentCondition<SwordShieldOfTheHeavens>(id + 0x1000u, 300f, comp => !comp.Active, "Adds enrage")
            .ActivateOnEnter<HoliestOfHoly>()
            .ActivateOnEnter<SkywardLeap>()
            .DeactivateOnExit<HoliestOfHoly>()
            .DeactivateOnExit<SkywardLeap>()
            .DeactivateOnExit<HeavenlySlashAdelphel>()
            .DeactivateOnExit<HeavenlySlashJanlenoux>()
            .DeactivateOnExit<SwordShieldOfTheHeavens>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void SpiralPierceDimensionalCollapseHiemalStormComets(uint id, float delay)
    {
        ComponentCondition<SpiralPierce>(id, delay, comp => comp.CurrentBaits.Count > 0, maxOverdue: 100f)
            .ActivateOnEnter<SpiralPierce>();
        ComponentCondition<DimensionalCollapse>(id + 0x10u, 2.1f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<DimensionalCollapse>();
        ComponentCondition<HiemalStormSpread>(id + 0x20u, 0.9f, comp => comp.Active)
            .ActivateOnEnter<HiemalStormSpread>();
        ComponentCondition<SpiralPierce>(id + 0x30u, 3.2f, comp => comp.NumCasts > 0, "Spreads + Charges")
            .ActivateOnEnter<HiemalStormVoidzone>()
            .DeactivateOnExit<HiemalStormSpread>() // spreads resolve slightly before charges, with large variance
            .ActivateOnExit<FaithUnmoving>()
            .DeactivateOnExit<SpiralPierce>();
        ComponentCondition<DimensionalCollapse>(id + 0x40u, 1.8f, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<DimensionalCollapse>();
        ComponentCondition<FaithUnmoving>(id + 0x50u, 3.1f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FaithUnmoving>();

        ComponentCondition<MeteorCircle>(id + 0x1000u, 3.4f, comp => comp.ActiveActors.Count != 0, "Comets appear") // note: quite large variance
            .ActivateOnEnter<CometCircle>()
            .ActivateOnEnter<MeteorCircle>()
            .ActivateOnEnter<HeavyImpact>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        // +3.4s: prey icons, first aoe after 4.1s, then every 1.1s
        // +29.9s: all live comets cast raidwides
        // TODO: proper small/large enrage deadlines
        ComponentCondition<MeteorCircle>(id + 0x1010u, 52.2f, comp => comp.ActiveActors.Count == 0, "Large comet enrage", 100f);
        ComponentCondition<MeteorCircle>(id + 0x1020u, 10f, comp =>
        {
            var comets = Module.Enemies((uint)OID.CometCircle);
            var countC = comets.Count;
            for (var i = 0; i < countC; ++i)
            {
                if (!comets[i].IsDeadOrDestroyed)
                    return false;
            }
            var meteors = Module.Enemies((uint)OID.CometCircle);
            var countM = meteors.Count;
            for (var i = 0; i < countM; ++i)
            {
                if (!meteors[i].IsDeadOrDestroyed)
                    return false;
            }
            return true;
        })
            .DeactivateOnExit<HeavyImpact>()
            .DeactivateOnExit<CometCircle>()
            .DeactivateOnExit<MeteorCircle>()
            .DeactivateOnExit<HiemalStormVoidzone>() // voidzones disappear slightly after comets appear
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void LightOfAscalonUltimateEnd(uint id, float delay)
    {
        ComponentCondition<BossReappear>(id, delay, comp => comp.NumCasts > 0, "Boss reappears")
            .ActivateOnEnter<BossReappear>()
            .ActivateOnEnter<LightOfAscalon>();
        ComponentCondition<LightOfAscalon>(id + 0x10u, 10.7f, comp => comp.NumCasts > 0, "Short knockback 1")
            .DeactivateOnExit<BossReappear>();
        ComponentCondition<LightOfAscalon>(id + 0x20u, 8.2f, comp => comp.NumCasts >= 7, "Short knockback 7") // note: i've seen 6 casts in one of the logs...
            .DeactivateOnExit<LightOfAscalon>();
        ComponentCondition<UltimateEnd>(id + 0x30u, 9.6f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<UltimateEnd>()
            .DeactivateOnExit<UltimateEnd>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnightsOfTheRound1(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f, "Trio 1 start (eye N)")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10u, (uint)AID.KnightsOfTheRound, 7.2f, 3);
        AscalonsMight(id + 0x20u, 5.2f);
        ComponentCondition<HolyShieldBash>(id + 0x30u, 6.0f, comp => comp.NumCasts > 0, "Stun healer")
            .ActivateOnEnter<HolyShieldBash>();
        ComponentCondition<HolyShieldBash>(id + 0x40u, 3.1f, comp => comp.Source != null);
        CastStart(id + 0x41u, (uint)AID.HeavenlyHeel, 5f);
        ComponentCondition<HolyShieldBash>(id + 0x42u, 1f, comp => comp.NumCasts > 1, "Wild charge")
            .DeactivateOnExit<HolyShieldBash>();
        CastEnd(id + 0x43u, 3f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x50u, (uint)AID.DragonsGaze, 2.1f, 3f, "Gaze")
            .DeactivateOnExit<DragonsGaze>();
    }

    private void KnightsOfTheRound2(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f, "Trio 2 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10u, (uint)AID.KnightsOfTheRound, 7.2f, 3f);
        ComponentCondition<Conviction>(id + 0x20, 7.2f, comp => comp.Towers.Count > 0)
            .ActivateOnEnter<Conviction>();
        Cast(id + 0x30u, (uint)AID.DragonsGaze, 1f, 3f, "Gaze")
            .ActivateOnEnter<HeavyImpact>() // starts ~1s into cast
            .DeactivateOnExit<DragonsGaze>();
        ComponentCondition<HeavyImpact>(id + 0x40u, 1.1f, comp => comp.NumCasts > 0, "Ring 1");
        ComponentCondition<HeavyImpact>(id + 0x50u, 2f, comp => comp.NumCasts > 1, "Towers + Ring 2")
            .DeactivateOnExit<Conviction>(); // happen at the same time
        ComponentCondition<DimensionalCollapse>(id + 0x60u, 1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<DimensionalCollapse>();
        ComponentCondition<HeavyImpact>(id + 0x70u, 1f, comp => comp.NumCasts > 2, "Ring 3");
        ComponentCondition<HeavyImpact>(id + 0x80u, 2f, comp => comp.NumCasts > 3, "Ring 4")
            .DeactivateOnExit<HeavyImpact>()
            .ActivateOnEnter<FaithUnmoving>();
        ComponentCondition<DimensionalCollapse>(id + 0x90u, 3f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<DimensionalCollapse>();

        CastStart(id + 0xA0u, (uint)AID.DragonsRage, 0.2f)
            .ActivateOnEnter<DragonsRage>();
        ComponentCondition<FaithUnmoving>(id + 0xA1u, 3f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FaithUnmoving>();
        CastEnd(id + 0xA2u, 2f, "Stack")
            .DeactivateOnExit<DragonsRage>();
    }

    private void KnightsOfTheRound3(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f, "Trio 3 start (eye ignore)");
        Cast(id + 0x10u, (uint)AID.KnightsOfTheRound, 7.2f, 3f)
            .ActivateOnEnter<SpiralThrust2>();
        LightningStorm(id + 0x20u, 3.2f);
        ComponentCondition<SpiralPierce>(id + 0x30u, 1.9f, comp => comp.CurrentBaits.Count > 0) // pierce & thrust start at the same time
            .ActivateOnEnter<SpiralPierce>()
            .ActivateOnEnter<SkywardLeap>(); // starts slightly later than pierce
        CastStart(id + 0x31u, (uint)AID.DragonsRage, 2.9f);
        ComponentCondition<SpiralPierce>(id + 0x32u, 3.1f, comp => comp.NumCasts > 0, "Baits")
            .ActivateOnEnter<DragonsRage>()
            .DeactivateOnExit<SpiralThrust2>()
            .DeactivateOnExit<SpiralPierce>()
            .DeactivateOnExit<SkywardLeap>(); // resolves slightly later, but whatever
        CastEnd(id + 0x33u, 1.9f, "Stack")
            .DeactivateOnExit<DragonsRage>();
    }

    private void KnightsOfTheRound4(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f, "Trio 4 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10u, (uint)AID.KnightsOfTheRound, 7.2f, 3);
        AscalonsMight(id + 0x20u, 6.2f);
        CastStart(id + 0x30u, (uint)AID.DragonsGaze, 2.1f);
        ComponentCondition<Heavensflame>(id + 0x31u, 1f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Heavensflame>();
        CastEnd(id + 0x32u, 2f, "Gaze")
            .ActivateOnEnter<BurningChains>() // chains appear together with first puddles
            .DeactivateOnExit<DragonsGaze>();
        // +0.1s: chains resolve?..
        ComponentCondition<HiemalStormSpread>(id + 0x40u, 0.9f, comp => comp.Active)
            .ActivateOnEnter<HiemalStormSpread>()
            .DeactivateOnExit<BurningChains>(); // TODO: i think it resolves earlier...
        AscalonsMercy(id + 0x50u, 1.4f)
            .ActivateOnEnter<HiemalStormVoidzone>()
            .DeactivateOnExit<Heavensflame>() // last puddle ends ~0.5s into cast
            .DeactivateOnExit<HiemalStormSpread>(); // note: this happens mid cast, with staggers...

        AncientQuaga(id + 0x1000u, 5.1f);
        HeavenlyHeel(id + 0x2000u, 2.1f);
        AncientQuaga(id + 0x3000u, 2.1f)
            .DeactivateOnExit<HiemalStormVoidzone>();
    }

    private void KnightsOfTheRound5(uint id, float delay)
    {
        Cast(id, (uint)AID.DragonsEye, delay, 3f, "Trio 5 start")
            .ActivateOnEnter<DragonsGaze>(); // objanim happens ~0.8s after cast end
        Cast(id + 0x10u, (uint)AID.KnightsOfTheRound, 7.2f, 3f);
        AscalonsMight(id + 0x20u, 5.2f);
        ComponentCondition<HoliestOfHoly>(id + 0x30u, 5, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<HoliestOfHoly>()
            .DeactivateOnExit<HoliestOfHoly>()
            .SetHint(StateMachine.StateHint.Raidwide);
        AscalonsMight(id + 0x40u, 5.2f);
        ComponentCondition<HeavenswardLeap>(id + 0x50u, 6.1f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<HeavenswardLeap>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HeavenswardLeap>(id + 0x51u, 3f, comp => comp.NumCasts > 1, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HeavenswardLeap>(id + 0x52u, 3f, comp => comp.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<HeavenswardLeap>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<SerZephirin>(id + 0x1000u, 7.7f, comp => comp.ActiveActors.Count != 0, "Boss invuln")
            .ActivateOnEnter<SerZephirin>();
        // +0.1s: zephirin starts 25s cast
        ComponentCondition<PureOfSoul>(id + 0x1010u, 6.1f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<PureOfSoul>()
            .DeactivateOnExit<PureOfSoul>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AbsoluteConviction>(id + 0x1020u, 10.9f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<AbsoluteConviction>()
            .DeactivateOnExit<AbsoluteConviction>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x1030, (uint)AID.DragonsGaze, 2.3f, 3f, "Gaze")
            .DeactivateOnExit<DragonsGaze>();
        ComponentCondition<SerZephirin>(id + 0x1040u, 2.8f, comp => comp.ActiveActors.Count == 0, "Add enrage")
            .DeactivateOnExit<SerZephirin>();
    }
}
