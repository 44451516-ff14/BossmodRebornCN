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
            .Raw.Update = () => _module.BossP2() is { IsTargetable: false, HPRatio: < 1 };
        SimplePhase(2, Phase3, "P3")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.ChaosP3()?.IsDeadOrDestroyed == true && _module.ExdeathP3()?.IsDeadOrDestroyed == true;
        SimplePhase(3, Phase4, "P4")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.KefkaP4()?.IsDeadOrDestroyed == true;
        SimplePhase(4, Phase5, "P5")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.KefkaP5()?.IsDeadOrDestroyed == true;
    }

    private void Phase5(uint id) {
        ActorTargetable(id, _module.KefkaP5, true, 0.1f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x10, _module.KefkaP5, (uint)AID.UltimaRepeaterCast, 3.0f, 5.0f, true, "Ultima Repeater")
            .ActivateOnEnter<UltimaRepeater>()
            .DeactivateOnExit<UltimaRepeater>()
            .ActivateOnEnter<FellForces>()
            .ExecOnEnter<FellForces>(o => o.active = true);

        ComponentCondition<FellForces>(id + 0x20, 6.0f, o => o.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x25, 3.1f, o => o.NumCasts > 3, "2nd Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x30, 3.1f, o => o.NumCasts > 6, "3rd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x40, _module.KefkaP5, (uint)AID.ChaoticFlood, 0.3f, 5.0f, true, "Chaotic Flood")
            .ActivateOnEnter<ChaoticFlood>()
            .ActivateOnEnter<ChaoticFloodStack>();

        ComponentCondition<ChaoticFloodStack>(id + 0x45, 1.0f, o => o.NumCasts > 0, "1st Stack");
        ComponentCondition<ChaoticFlood>(id + 0x50, 0.1f, o => o.NumCasts > 0, "1st Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x55, 1.0f, o => o.NumCasts > 1, "2nd Stack");
        ComponentCondition<ChaoticFlood>(id + 0x60, 0.1f, o => o.NumCasts > 2, "2nd Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x65, 1.0f, o => o.NumCasts > 2, "3rd Stack");
        ComponentCondition<ChaoticFlood>(id + 0x70, 0.1f, o => o.NumCasts > 4, "3rd Flood");
        ComponentCondition<ChaoticFloodStack>(id + 0x75, 1.0f, o => o.NumCasts > 3, "4th Stack");
        ComponentCondition<ChaoticFlood>(id + 0x80, 0.1f, o => o.NumCasts > 6, "4th Flood")
            .DeactivateOnExit<ChaoticFloodStack>()
            .DeactivateOnExit<ChaoticFlood>()
            .ActivateOnExit<MaddeningOrchestra>();

        ComponentCondition<MaddeningOrchestra>(id + 0x90, 9.9f, o => o.NumCasts > 0, "1st Baits Resolve")
            .ActivateOnExit<ChaoticFlareTB>()
            .ExecOnExit<ChaoticFlareTB>(a => a.active = true);

        ComponentCondition<MaddeningOrchestra>(id + 0x100, 3.2f, o => o.NumCasts > 5, "2nd Baits + TB Resolve")
            .DeactivateOnExit<ChaoticFlareTB>()
            .DeactivateOnExit<MaddeningOrchestra>()
            .ActivateOnExit<ChaoticHolyFlareDiffusion>();

        ComponentCondition<ChaoticHolyFlareDiffusion>(id + 0x110, 3.5f, o => o.NumCasts >= 2, "Tank Baits Resolve")
            .DeactivateOnExit<ChaoticHolyFlareDiffusion>()
            .ActivateOnEnter<FellForces>()
            .ExecOnExit<FellForces>(a => a.active = true)
            .ExecOnExit<FellForces>(a => a.expectedCasts = 6);

        ComponentCondition<FellForces>(id + 0x120, 4.6f, o => o.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x130, 3.1f, o => o.NumCasts > 3, "2nd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x140, _module.KefkaP5, (uint)AID.Celestriad, 1.4f, 5.0f, true, "Celestraid")
            .ActivateOnEnter<Celestriad>()
            .ActivateOnEnter<CatastrophicChoice>();

        ComponentCondition<Celestriad>(id + 0x150, 9.1f, o => o.NumCasts > 0, "1st Tower Set");
        ComponentCondition<CatastrophicChoice>(id + 0x155, 0.2f, o => o.NumCasts > 0, "In/Out");
        ComponentCondition<Celestriad>(id + 0x160, 5.8f, o => o.NumCasts > 4, "2nd Tower Set");
        ComponentCondition<Celestriad>(id + 0x170, 6.0f, o => o.NumCasts > 8, "3rd Tower Set");
        ComponentCondition<CatastrophicChoice>(id + 0x175, 0.2f, o => o.NumCasts > 1, "2nd In/Out");

        ActorCast(id + 0x180, _module.KefkaP5, (uint)AID.UltimaRepeaterCast, 4.0f, 4.0f, true, "Ultima Repeater")
            .ActivateOnEnter<UltimaRepeater>()
            .DeactivateOnExit<UltimaRepeater>()
            .ActivateOnEnter<FellForces>()
            .ExecOnEnter<FellForces>(o => o.active = true)
            .ExecOnEnter<FellForces>(o => o.expectedCasts = 6);

        ComponentCondition<FellForces>(id + 0x190, 6.0f, o => o.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x200, 3.1f, o => o.NumCasts > 3, "2nd Auto Attack Stack")
            .DeactivateOnExit<FellForces>();

        ActorCast(id + 0x210, _module.KefkaP5, (uint)AID.StrayApocalypseCast, 1.5f, 5.0f, true, "Exa-flares start")
            .ActivateOnEnter<StrayApocalypse>()
            .ActivateOnEnter<StrayEntropy>();

        ComponentCondition<StrayApocalypse>(id + 0x220, 15.8f, o => o.NumCasts >= 84, "Exa-Flares end");
        ComponentCondition<StrayEntropy>(id + 0x230, 2.3f, o => !o.Active, "Spreads")
            .DeactivateOnExit<StrayApocalypse>()
            .DeactivateOnExit<StrayEntropy>();

        ActorCast(id + 0x240, _module.KefkaP5, (uint)AID.MaddeningOrchestra, 3.3f, 5.0f, true, "Maddening Orchestra")
            .ActivateOnEnter<MaddeningOrchestra>();

        ComponentCondition<MaddeningOrchestra>(id + 0x250, 0.9f, o => o.NumCasts > 0, "1st Baits Resolve")
            .ActivateOnExit<ChaoticFlareTB>()
            .ExecOnExit<ChaoticFlareTB>(a => a.active = true);

        ComponentCondition<MaddeningOrchestra>(id + 0x260, 3.2f, o => o.NumCasts > 5, "2nd Baits + TB Resolve")
            .DeactivateOnExit<ChaoticFlareTB>()
            .DeactivateOnExit<MaddeningOrchestra>()
            .ActivateOnExit<ChaoticHolyFlareDiffusion>();

        ComponentCondition<ChaoticHolyFlareDiffusion>(id + 0x270, 3.5f, o => o.NumCasts > 0, "Tank Baits Resolve")
            .DeactivateOnExit<ChaoticHolyFlareDiffusion>()
            .ActivateOnExit<FellForces>()
            .ExecOnExit<FellForces>(a => a.active = true);

        ComponentCondition<FellForces>(id + 0x280, 4.7f, o => o.NumCasts > 0, "1st Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x290, 3.1f, o => o.NumCasts > 3, "2nd Auto Attack Stack");
        ComponentCondition<FellForces>(id + 0x300, 3.1f, o => o.NumCasts > 6, "3rd Auto Attack Stack")
            .DeactivateOnExit<FellForces>()
            .ActivateOnEnter<P5ForsakenBait>()
            .ActivateOnEnter<P5ForsakenRaidWide>()
            .ActivateOnEnter<P5ForsakenGround>()
            .ActivateOnEnter<P5ForsakenStack>();

        Timeout(id + 0x500000, 30.0f, "P5 Unknown");
    }

    // TODO update raidwides to actually use actors instead since it should now be fixed and able to find the boss correctly
    private void Phase4(uint id) {
        ActorTargetable(id, _module.KefkaP4, true, 2.1f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x05, _module.KefkaP4, (uint)AID.KefkaSays, 5.2f, 5.0f, true, "Other bosses spawn")
            .ActivateOnEnter<GrandCrossOrder>()
            .ActivateOnEnter<TsunamiInfernoOrder>();

        ActorCastStart(id + 0x10, _module.KefkaP4, (uint)AID.MysteryMagic, 4.6f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<LightningSafeSpots>();

        ComponentCondition<BlizzardSafeSpots>(id + 0x20, 5.0f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();

        ComponentCondition<GrandCrossOrder>(id + 0x30, 4.4f, o => o.currentCast > 0, "Raidwide (1st Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>();
        ComponentCondition<TsunamiInfernoOrder>(id + 0x40, 5.1f, o => o.currentCast > 0, "Raidwide (1st Tsunami Inferno)")
            .ActivateOnEnter<TsunamiRaidwide>()
            .ActivateOnEnter<InfernoRaidwide>()
            .DeactivateOnExit<TsunamiRaidwide>()
            .DeactivateOnExit<InfernoRaidwide>();

        ActorCastStart(id + 0x50, _module.KefkaP4, (uint)AID.MysteryMagic, 0.5f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<LightningSafeSpots>();

        ComponentCondition<BlizzardSafeSpots>(id + 0x60, 5.0f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();

        ComponentCondition<GrandCrossOrder>(id + 0x70, 4.2f, o => o.currentCast > 1, "Raidwide (2nd Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>();
        ComponentCondition<TsunamiInfernoOrder>(id + 0x80, 5.2f, o => o.currentCast > 1, "Raidwide (2nd Tsunami Inferno)")
            .ActivateOnEnter<TsunamiRaidwide>()
            .ActivateOnEnter<InfernoRaidwide>()
            .DeactivateOnExit<TsunamiRaidwide>()
            .DeactivateOnExit<InfernoRaidwide>();

        ActorCastStart(id + 0x90, _module.KefkaP4, (uint)AID.MysteryMagic, 0.7f, true, "Mystery Magic")
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<LightningSafeSpots>();

        ComponentCondition<BlizzardSafeSpots>(id + 0x100, 5.0f, o => o.NumCasts > 0, "Blizzard + Lightning safe spots")
            .DeactivateOnExit<LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>();

        ComponentCondition<GrandCrossOrder>(id + 0x110, 3.9f, o => o.currentCast > 2, "Raidwide (3rd Grand Cross)")
            .ActivateOnEnter<GrandCrossRaidwide>()
            .DeactivateOnExit<GrandCrossRaidwide>()
            .ActivateOnExit<AntiLight>()
            .ActivateOnExit<EdgeOfDeath>();

        ComponentCondition<AntiLight>(id + 0x120, 12.6f, o => o.NumCasts >= 4, "Antilight + Edge of Death")
            .DeactivateOnExit<AntiLight>()
            .DeactivateOnExit<EdgeOfDeath>()
            .ActivateOnExit<ForkedWater>()
            .ActivateOnExit<AccelerationBomb>();

        Condition(id + 0x130, 8.4f, () => Module.FindComponent<AccelerationBomb>()!.NumCasts >= 4 &&
                                          Module.FindComponent<ForkedWater>()!.NumCasts >= 4,
            "Spreads + Stacks + Acceleration Bombs Resolve")
            .DeactivateOnExit<ForkedWater>()
            .DeactivateOnExit<AccelerationBomb>()
            .ActivateOnEnter<LightningSafeSpots>()
            .ActivateOnExit<CursedShriek>()
            .ActivateOnExit<KefkaOrder>();

        ComponentCondition<LightningSafeSpots>(id + 0x140, 8.1f, o => o.NumCasts > 0, "Lightning Safe Spots");

        ComponentCondition<CursedShriek>(id + 0x150, 0.8f, o => o.NumCasts > 0, "Gazes Resolve")
            .DeactivateOnExit<CursedShriek>()
            .DeactivateOnExit<LightningSafeSpots>()
            .ActivateOnExit<Inferno>()
            .ActivateOnExit<InfernoBaits>();

        ActorCastStart(id + 0x160, _module.KefkaP4, (uint)AID.UltimaUpsurge, 4.3f, true, "Ultima Upsurge")
            .ActivateOnEnter<UltimaUpsurge>()
            .ActivateOnEnter<ForkedWater>()
            .ActivateOnEnter<AccelerationBomb>()
            .ExecOnEnter<ForkedWater>(o => o.active = false);

        ComponentCondition<Inferno>(id + 0x165, 2.5f, o => o.active == true, "Baits dropped")
            .ExecOnExit<ForkedWater>(o => o.active = true);

        ComponentCondition<UltimaUpsurge>(id + 0x166, 2.5f, o => o.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<UltimaUpsurge>();

        ComponentCondition<InfernoBaits>(id + 0x170, 2.5f, o => o.NumCasts >= 8, "Inferno Baits")
            .DeactivateOnExit<Inferno>()
            .DeactivateOnExit<InfernoBaits>();

        Condition(id + 0x180, 4.2f, () => Module.FindComponent<AccelerationBomb>()!.NumCasts >= 4 &&
                                          Module.FindComponent<ForkedWater>()!.NumCasts >= 4,
                "Spreads + Stacks + Acceleration Bombs Resolve")
            .DeactivateOnExit<ForkedWater>()
            .DeactivateOnExit<AccelerationBomb>()
            .ActivateOnEnter<BlizzardSafeSpots>();

        ComponentCondition<BlizzardSafeSpots>(id + 0x190, 1.5f, o => o.NumCasts > 0, "Blizzard Safe spots")
            .DeactivateOnExit<BlizzardSafeSpots>()
            .ActivateOnExit<CursedShriek>();

        ComponentCondition<CursedShriek>(id + 0x200, 7.1f, o => o.NumCasts > 0, "Gazes Resolve")
            .DeactivateOnExit<CursedShriek>()
            .ActivateOnExit<Tsunami>()
            .ActivateOnExit<TsunamiBaits>();

        ComponentCondition<TsunamiBaits>(id + 0x210, 10.7f, o => o.NumCasts >= 8, "Tsunami Baits")
            .DeactivateOnExit<Tsunami>()
            .DeactivateOnExit<TsunamiBaits>()
            .ActivateOnEnter<BlizzardSafeSpots>()
            .ActivateOnEnter<P4LightningSafeSpots>();

        ComponentCondition<LightningSafeSpots>(id + 0x220, 0.5f, o => o.NumCasts > 0, "Blizzard + Lightning Safe Spots")
            .DeactivateOnExit<P4LightningSafeSpots>()
            .DeactivateOnExit<BlizzardSafeSpots>()
            .DeactivateOnExit<GrandCrossOrder>()
            .DeactivateOnExit<TsunamiInfernoOrder>()
            .DeactivateOnExit<KefkaOrder>();

        ActorCast(id + 0x40000, _module.KefkaP4, (uint)AID.UltimaUpsurge, 4.2f, 5.0f, true, "Enrage")
            .ActivateOnEnter<UltimaUpsurge>()
            .DeactivateOnExit<UltimaUpsurge>();

        Timeout(id + 0x40010, 31.0f, "Downtime");
    }

    private void Phase3(uint id) {
        ActorCast(id, _module.BossP3, (uint)AID.AeroIIIAssault, 2.4f, 3, false, "Knockback")
            .ActivateOnEnter<AeroIIIAssault>()
            .DeactivateOnExit<AeroIIIAssault>();

        ActorCast(id + 0x10, _module.BossP3, (uint)AID.DefinitionOfInsanity, 33.7f, 4);
        ActorTargetable(id + 0x20, _module.ExdeathP3, true, 3.1f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x30, _module.ChaosP3, (uint)AID.TheDecisiveBattle, 0.2f, 3.0f, true)
            .ActivateOnEnter<TheDecisiveBattle>()
            .DeactivateOnExit<TheDecisiveBattle>();

        ActorCast(id + 0x40, _module.ChaosP3, (uint)AID.BowelsOfAgony, 14.4f, 5.0f, true, "Raidwide")
            .ActivateOnEnter<BowelsOfAgony>()
            .DeactivateOnExit<BowelsOfAgony>()
            .ActivateOnExit<Crystals>();

        ActorCast(id + 0x50, _module.ExdeathP3, (uint)AID.ThunderIII, 12.3f, 7.0f, true, "Thunder + element 1")
            .ActivateOnEnter<ThunderIII>()
            .DeactivateOnExit<ThunderIII>()
            .ExecOnEnter<Crystals>(c => {
                if (c.nextElement == Crystals.Element.Fire) {
                    Module.ActivateComponent<FireCrystal>();
                }

                if (c.nextElement == Crystals.Element.Water) {
                    Module.ActivateComponent<WaterCrystal>();
                }
            });

        Condition(id + 0x60, 0.8f, () => Module.FindComponent<FireCrystal>()?.NumCasts > 0 || Module.FindComponent<WaterCrystal>()?.NumCasts > 0, "1st Crystal")
            .ExecOnExit<Crystals>(c => {
                if (c.nextElement == Crystals.Element.Fire) {
                    Module.DeactivateComponent<FireCrystal>();
                }

                if (c.nextElement == Crystals.Element.Water) {
                    Module.DeactivateComponent<WaterCrystal>();
                }
            });

        ActorCast(id + 0x70, _module.ExdeathP3, (uint)AID.ThunderIII, 3.4f, 5.0f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<ThunderIIITB>(id + 0x75, 0.1f, o => o.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x80, 3.0f, o => o.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        ActorCastStartMulti(id + 0x90, _module.ChaosP3, [(uint)AID.LongitudinalImplosion, (uint)AID.LatitudinalImplosion], 4.8f, true)
            .ActivateOnEnter<LongitudinalLatitudinalImplosion>()
            .ExecOnEnter<Crystals>(c => {
                if (c.nextElement == Crystals.Element.Fire) {
                    Module.ActivateComponent<FireCrystal>();
                }

                if (c.nextElement == Crystals.Element.Water) {
                    Module.ActivateComponent<WaterCrystal>();
                }
            });

        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x100, 5.6f, o => o.NumCasts > 0, "Front/sides 1st");
        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x110, 2.0f, o => o.NumCasts > 2, "Front/sides 2nd")
            .DeactivateOnExit<LongitudinalLatitudinalImplosion>();

        Condition(id + 0x120, 3.1f,
                () => Module.FindComponent<WaterCrystal>()?.NumCasts > 0 ||
                      Module.FindComponent<FireCrystal>()?.NumCasts > 0, "2nd Crystal")
            .ExecOnExit<Crystals>(c =>
            {
                if (c.nextElement == Crystals.Element.Fire)
                {
                    Module.DeactivateComponent<FireCrystal>();
                }

                if (c.nextElement == Crystals.Element.Water)
                {
                    Module.DeactivateComponent<WaterCrystal>();
                }
            })
            .ActivateOnExit<UmbraSmash>()
            .ActivateOnEnter<UltimaBlaster>();

        ActorCastStart(id + 0x130, _module.ChaosP3, (uint)AID.UmbraSmash, 9.3f, true, "UmbraSmash bait")
            .ActivateOnEnter<UltimaBlasterLimitCut>();

        ComponentCondition<UltimaBlaster>(id + 0x140, 0.8f, o => o.NumCasts > 0, "Raidwides start");

        ComponentCondition<UmbraSmash>(id + 0x150, 4.1f, o => o.NumCasts > 0, "UmbraSmash bait resolves")
            .ActivateOnEnter<HeadTailWind>()
            .ActivateOnEnter<Cyclone>()
            .DeactivateOnExit<UmbraSmash>();

        ComponentCondition<HeadTailWind>(id + 0x160, 3.1f, o => o.NumCasts > 0, "Knockback");
        ComponentCondition<Cyclone>(id + 0x170, 3.9f, o => o.NumCasts == 8, "Wind stacks")
            .DeactivateOnExit<HeadTailWind>()
            .DeactivateOnExit<Cyclone>()
            .DeactivateOnExit<Crystals>();

        ComponentCondition<UltimaBlasterLimitCut>(id + 0x180, 11.0f, o => o.NumCasts > 0, "Limit cut starts");
        ComponentCondition<UltimaBlasterLimitCut>(id + 0x190, 1.6f, o => o.NumCasts == 8, "Limit cut ends")
            .DeactivateOnExit<UltimaBlaster>()
            .DeactivateOnExit<UltimaBlasterLimitCut>();

        ActorCast(id + 0x200, _module.ExdeathP3, (uint)AID.ThunderIII, 1.0f, 5.0f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<ThunderIIITB>(id + 0x205, 0.1f, o => o.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x210, 3.1f, o => o.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        ActorCast(id + 0x220, _module.ChaosP3, (uint)AID.TheDecisiveBattle, 1.9f, 3.0f, true)
            .ActivateOnEnter<TheDecisiveBattle>()
            .DeactivateOnExit<TheDecisiveBattle>()
            .ActivateOnExit<ThunderIIITB>();

        ComponentCondition<ThunderIIITB>(id + 0x235, 9.2f, o => o.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x240, 3.1f, o => o.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>()
            .ActivateOnEnter<EarthquakeRaidwide>();

        ComponentCondition<EarthquakeRaidwide>(id + 0x250, 1.8f, o => o.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<EarthquakeRaidwide>()
            .ActivateOnEnter<KefkaMax>()
            .ActivateOnExit<SlapHappy>()
            .ActivateOnExit<SlapHappyBaits>()
            .ActivateOnExit<BlackHole>()
            .ActivateOnExit<BlackHoleActors>();

        ComponentCondition<SlapHappy>(id + 0x260, 18.8f, o => o.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x270, 7.4f, o => o.NumCasts == 1, "Tethers set 1-1");

        ActorCastStart(id + 0x280, _module.ExdeathP3, (uint)AID.ThunderIII, 5.3f, true, "Tankbuster cast")
            .ActivateOnEnter<ThunderIIITB>();
        ComponentCondition<BlackHole>(id + 0x290, 1.8f, o => o.NumCasts > 1, "Tethers set 1-2");
        ComponentCondition<ThunderIIITB>(id + 0x295, 3.2f, o => o.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x300, 3.1f, o => o.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>()
            .ActivateOnEnter<DamningEdict>();

        ComponentCondition<DamningEdict>(id + 0x310, 5.0f, o => o.NumCasts > 0, "Frontal")
            .DeactivateOnExit<DamningEdict>()
            .ActivateOnEnter<SlapHappy>()
            .ActivateOnEnter<SlapHappyBaits>();

        ComponentCondition<SlapHappy>(id + 0x320, 4.7f, o => o.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x340, 7.5f, o => o.NumCasts > 3, "Tethers set 2-1");
        ComponentCondition<BlackHole>(id + 0x350, 5.0f, o => o.NumCasts > 6, "Tethers set 2-2");
        ComponentCondition<BlackHole>(id + 0x360, 5.1f, o => o.NumCasts > 9, "Tethers set 2-3")
            .ActivateOnEnter<DamningEdict>();

        ComponentCondition<DamningEdict>(id + 0x370, 4.9f, o => o.NumCasts > 0, "Frontal")
            .DeactivateOnExit<DamningEdict>()
            .ActivateOnEnter<LookUponMeAndDespairAOE>();

        ComponentCondition<LookUponMeAndDespairAOE>(id + 0x380, 1.4f, o => o.NumCasts > 0, "Middle line AOE")
            .DeactivateOnExit<LookUponMeAndDespairAOE>()
            .ActivateOnEnter<ThunderIIITB>();

        ComponentCondition<ThunderIIITB>(id + 0x395, 4.5f, o => o.NumCasts > 0, "Tankbuster 1st hit");
        ComponentCondition<ThunderIIITB>(id + 0x400, 3.1f, o => o.NumCasts > 1, "Tankbuster 2nd hit")
            .DeactivateOnExit<ThunderIIITB>();

        ComponentCondition<BlackHole>(id + 0x410, 10.1f, o => o.NumCasts > 12, "Tethers set 3-1");
        ComponentCondition<BlackHole>(id + 0x420, 5.0f, o => o.NumCasts > 15, "Tethers set 3-2");
        ComponentCondition<BlackHole>(id + 0x430, 5.1f, o => o.NumCasts > 18, "Tethers set 3-3")
            .ActivateOnEnter<WhiteHole>();

        ComponentCondition<WhiteHole>(id + 0x440, 11.0f, o => o.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<WhiteHole>()
            .ActivateOnEnter<LongitudinalLatitudinalImplosion>()
            .ActivateOnEnter<SlapHappy>()
            .ActivateOnEnter<SlapHappyBaits>();

        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x450, 0.7f, o => o.NumCasts > 0, "Front/sides 1st");
        ComponentCondition<LongitudinalLatitudinalImplosion>(id + 0x460, 2.0f, o => o.NumCasts > 2, "Front/sides 2nd")
            .DeactivateOnExit<LongitudinalLatitudinalImplosion>();

        ComponentCondition<SlapHappy>(id + 0x470, 2.2f, o => o.NumCasts == 4, "SlapHappy AOEs resolve + Baits")
            .DeactivateOnExit<SlapHappy>()
            .DeactivateOnExit<SlapHappyBaits>();

        ComponentCondition<BlackHole>(id + 0x480, 7.3f, o => o.NumCasts > 21, "Tethers set 4-1")
            .ActivateOnEnter<LookUponMeAndDespairAOE>();
        ComponentCondition<BlackHole>(id + 0x490, 7.0f, o => o.NumCasts > 23, "Tethers set 4-2 + Middle line AOE")
            .DeactivateOnExit<LookUponMeAndDespairAOE>()
            .ActivateOnExit<P3Blizzard>();

        ActorCast(id + 0x500, _module.ExdeathP3, (uint)AID.BlizzardIIICast, 5.3f, 3.0f, true, "1st Blizzard Baits")
            .DeactivateOnEnter<KefkaMax>()
            .DeactivateOnEnter<BlackHoleActors>()
            .DeactivateOnEnter<BlackHole>()
            .ActivateOnEnter<P3BlizzardBaits>()
            .ActivateOnEnter<KnockDown>()
            .ActivateOnEnter<StompAMole>();

        ComponentCondition<P3Blizzard>(id + 0x510, 0.1f, o => o.NumCasts > 0, "1st Blizzard Baits");
        ComponentCondition<P3BlizzardBaits>(id + 0x520, 3.0f, o => o.NumCasts > 0, "1st Blizzard Baits Resolve");
        ComponentCondition<P3Blizzard>(id + 0x530, 0.1f, o => o.NumCasts > 8, "2nd Blizzard Baits");
        ComponentCondition<StompAMole>(id + 0x540, 2.2f, o => o.NumCasts > 0, "1st Tower");
        ComponentCondition<P3BlizzardBaits>(id + 0x550, 0.8f, o => o.NumCasts > 8, "2nd Blizzard Baits Resolve")
            .DeactivateOnExit<P3Blizzard>()
            .DeactivateOnExit<P3BlizzardBaits>();
        ComponentCondition<StompAMole>(id + 0x570, 0.5f, o => o.NumCasts > 1, "2nd Tower");
        ComponentCondition<StompAMole>(id + 0x580, 1.3f, o => o.NumCasts > 2, "3rd Tower");
        ComponentCondition<StompAMole>(id + 0x590, 1.3f, o => o.NumCasts > 3, "4th Tower");
        ComponentCondition<KnockDown>(id + 0x600, 1.8f, o => !o.Active, "2nd Stack")
            .DeactivateOnExit<StompAMole>()
            .ActivateOnEnter<BigBang>()
            .ActivateOnEnter<P3BlizzardMove>();
        ComponentCondition<P3BlizzardMove>(id + 0x610, 3.5f, o => o.NumCasts > 0, "Blizzard Raidwide")
            .DeactivateOnExit<P3BlizzardMove>();
        ComponentCondition<BigBang>(id + 0x620, 1.2f, o => o.NumCasts > 1, "Stack AOEs resolve")
            .DeactivateOnExit<BigBang>()
            .DeactivateOnExit<KnockDown>()
            .ActivateOnEnter<P3Enrage>();
        ComponentCondition<P3Enrage>(id + 0x640, 17.6f, comp => !comp.enrage, "Enrage");
    }

    private void Phase2(uint id) {
        ActorTargetable(id, _module.BossP2, true, 10.3f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ActorCast(id + 0x10, _module.BossP2, (uint)AID.UltimateEmbrace, 7.2f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();
        ActorCast(id + 0x20, _module.BossP2, (uint)AID.Forsaken, 8.2f, 7.0f, true, "Raidwide")
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
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(o => o.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x50, 5.7f, o => o.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        // Tower set 3
        Condition(id + 0x60, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 3, "3rd Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 4
        ComponentCondition<ForsakenShapes>(id + 0x70, 10.0f, o => o.currentTowerSet > 4, "4th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(o => o.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x80, 5.7f, o => o.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        // Tower set 5
        Condition(id + 0x90, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 5, "5th Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 6
        ComponentCondition<ForsakenShapes>(id + 0x100, 10.0f, o => o.currentTowerSet > 6, "6th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnExit<ForsakenSolverSet1>()
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.AOE)
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnExit<AllThingsEnding>(o => o.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x110, 5.7f, o => o.aoesLocked, "Boss/Clones baits")
            .ExecOnExit<ForsakenSolverSet1>(s => s.colourCircle = Colors.Safe);

        // Tower set 7
        Condition(id + 0x120, 5.4f, () => Module.FindComponent<AllThingsEndingCasts>()!.NumCasts >= 4 &&
                                         Module.FindComponent<ForsakenShapes>()!.currentTowerSet > 7, "7th Tower Set + baits")
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>()
            .DeactivateOnExit<ForsakenSolverSet1>()
            .ActivateOnExit<ForsakenSolverSet2>();

        // Tower set 8
        ComponentCondition<ForsakenShapes>(id + 0x130, 10.0f, o => o.currentTowerSet > 8, "8th Tower Set")
            .DeactivateOnExit<ForsakenSolverSet2>()
            .ActivateOnEnter<AllThingsEnding>()
            .ActivateOnEnter<AllThingsEndingCasts>()
            .ExecOnEnter<AllThingsEnding>(o => o.aoesLocked = false);

        // Clones baits
        ComponentCondition<AllThingsEnding>(id + 0x140, 5.4f, o => o.aoesLocked, "Boss/Clones baits");

        ComponentCondition<AllThingsEndingCasts>(id + 0x145, 5.2f, o => o.NumCasts >= 4, "Boss/Clones baits Resolve")
            .DeactivateOnExit<ForsakenShapes>()
            .DeactivateOnExit<ForsakenBaitsSpreadStacks>()
            .DeactivateOnExit<ForsakenBaitsCone>()
            .DeactivateOnExit<ForsakenBaitsBossClones>()
            .DeactivateOnExit<AllThingsEnding>()
            .DeactivateOnExit<AllThingsEndingCasts>();

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
        ActorCast(id + 0x220, _module.BossP2, (uint)AID.UltimateEmbrace, 2.1f, 5.0f, true, "Tankbuster")
            .ActivateOnEnter<UltimateEmbrace>()
            .DeactivateOnExit<UltimateEmbrace>();

        ActorTargetable(id + 0x30000, _module.BossP2, false, 4.1f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
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
            .ActivateOnExit<GravenImage3>();
        ComponentCondition<GravenImage3>(id + 0x270, 5.6f, o => !o.Active, "Sleeps + Confusion Spreads")
            .DeactivateOnExit<GravenImage3>()
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
