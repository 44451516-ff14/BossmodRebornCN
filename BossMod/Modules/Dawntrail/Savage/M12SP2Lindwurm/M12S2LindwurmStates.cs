namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

class M12S2LindwurmStates : StateMachineBuilder
{
    public M12S2LindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Replication1(id, 10.2f);
        Replication2(id + 0x10000, 11.2f);
        BloodMana(id + 0x20000, 15);
        IdyllicDream(id + 0x30000, 11.2f);
        DoubleSobat(id + 0x40000, 3.2f);
        ArcadianHell(id + 0x50000, 8.4f);
    }

    void Replication1(uint id, float delay)
    {
        Cast(id, AID.ArcadiaAflame, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<ArcadiaAflame>()
            .DeactivateOnExit<ArcadiaAflame>();

        Cast(id + 0x100u, AID.Replication, 9.5f, 3f)
            .ActivateOnEnter<Replication1SecondBait>()
            .ActivateOnEnter<Replication1Guidance>()
            .ActivateOnEnter<WingedScourge>()
            .ActivateOnEnter<WingedScourgeSecond>()
            .ActivateOnEnter<MightyMagicTopTierSlamFirstBait>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(static comp => comp.Risky = false);

        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x110u, 11.6f, static comp => comp.NumFire > 0, "Fire bait");
        ComponentCondition<WingedScourge>(id + 0x111u, 0.8f, static comp => comp.NumCasts > 0, "Cones");
        ComponentCondition<MightyMagicTopTierSlamFirstBait>(id + 0x112u, 0.3f, static comp => comp.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamFirstBait>()
            .ExecOnExit<SnakingKick>(static comp => comp.Risky = true);

        ComponentCondition<SnakingKick>(id + 0x120u, 4.6f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<SnakingKick>();

        ComponentCondition<WingedScourge>(id + 0x130u, 12.6f, static comp => comp.Casters.Count > 0)
            .DeactivateOnExit<WingedScourgeSecond>();

        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x140u, 3.2f, static comp => comp.NumFire > 0, "Fire baits");
        ComponentCondition<WingedScourge>(id + 0x141u, 0.8f, static comp => comp.NumCasts > 4, "Cones");
        ComponentCondition<MightyMagicTopTierSlamSecondBait>(id + 0x142u, 0.3f, static comp => comp.NumDark > 0, "Dark baits")
            .DeactivateOnExit<MightyMagicTopTierSlamSecondBait>()
            .DeactivateOnExit<Replication1SecondBait>()
            .DeactivateOnExit<Replication1Guidance>()
            .DeactivateOnExit<WingedScourge>();

        DoubleSobat(id + 0x200u, 2.4f);
    }

    void DoubleSobat(uint id, float delay)
    {
        CastStart(id, AID.DoubleSobatBoss1, delay)
            .ActivateOnEnter<DoubleSobatBuster>()
            .ActivateOnEnter<DoubleSobatRepeat>();
        ComponentCondition<DoubleSobatBuster>(id + 1u, 5.6f, static comp => comp.NumCasts > 0, "Half-room buster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DoubleSobatBuster>();
        ComponentCondition<DoubleSobatRepeat>(id + 0x10u, 4.6f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .ActivateOnEnter<EsotericFinisher>()
            .DeactivateOnExit<DoubleSobatRepeat>()
            .ExecOnEnter<EsotericFinisher>(static comp => comp.EnableHints = false)
            .ExecOnExit<EsotericFinisher>(static comp => comp.EnableHints = true);

        ComponentCondition<EsotericFinisher>(id + 0x20u, 2.5f, static comp => comp.NumCasts > 0, "Double tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<EsotericFinisher>();
    }

    void Replication2(uint id, float delay)
    {
        Cast(id, AID.Staging, delay, 3f)
            .ActivateOnEnter<Replication2Staging>();
        ComponentCondition<Replication2Staging>(id + 3u, 7.9f, static comp => comp.PlayersAssigned, "Player clones appear");
        Cast(id + 0x10u, AID.Replication, 3.3f, 3f);

        ComponentCondition<Replication2Staging>(id + 0x100u, 16.5f, static comp => comp.WurmsAssigned, "Clone tethers");
        CastStart(id + 0x101u, AID.FirefallSplashCast, 0.1f)
            .ActivateOnEnter<Replication2FirefallSplash>()
            .ActivateOnEnter<Replication2ScaldingWaves>()
            .ActivateOnEnter<Replication2ManaBurst>()
            .ActivateOnEnter<SnakingKick>()
            .ExecOnEnter<SnakingKick>(static comp => comp.Risky = false);

        ComponentCondition<Replication2FirefallSplash>(id + 0x110u, 5.9f, static comp => comp.NumCasts > 0, "Boss jumps (spread)")
            .DeactivateOnExit<Replication2FirefallSplash>();
        ComponentCondition<Replication2ScaldingWaves>(id + 0x111u, 0.6f, static comp => comp.NumCasts > 0, "Proteans");
        //.DeactivateOnExit<Replication2ScaldingWaves>(); // keep activated since we need to track targets
        ComponentCondition<Replication2ManaBurst>(id + 0x112u, 1.4f, static comp => comp.NumCasts > 0, "Defamations")
            .ActivateOnEnter<Replication2HeavySlam>()
            .ActivateOnEnter<Replication2HemorrhagicProjection>()
            .DeactivateOnExit<Replication2ManaBurst>();
        ComponentCondition<Replication2HeavySlam>(id + 0x113u, 5.5f, static comp => comp.NumCasts > 0, "Stacks")
            //.ExecOnEnter<Replication2HeavySlam>(static comp => comp.EnableHints = true)
            .DeactivateOnExit<Replication2HeavySlam>();
        ComponentCondition<Replication2HemorrhagicProjection>(id + 0x114u, 1.8f, static comp => comp.NumCasts > 0, "Cones")
            .ExecOnEnter<Replication2HemorrhagicProjection>(static comp => comp.EnableHints = true)
            .DeactivateOnExit<Replication2HemorrhagicProjection>();

        ComponentCondition<SnakingKick>(id + 0x120u, 3.8f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .ExecOnEnter<SnakingKick>(static comp => comp.Risky = true)
            .DeactivateOnExit<SnakingKick>();

        Cast(id + 0x200u, AID.Reenactment, 7.2f, 3f)
            .ActivateOnEnter<Replication2ReenactmentOrder>()
            .ActivateOnEnter<Replication2ReenactmentAOEs>()
            .ActivateOnEnter<Replication2ReenactmentScaldingWaves>()
            .ActivateOnEnter<Replication2ReenactmentTowers>();

        CastMulti(id + 0x210u, [AID.NetherwrathNear, AID.NetherwrathFar], 3.2f, 5f)
            .ActivateOnEnter<Replication2TimelessSpite>();
        ComponentCondition<Replication2TimelessSpite>(id + 0x220u, 1.2f, static comp => comp.NumCasts > 0, "Close/far stack + reenactment start")
            .DeactivateOnExit<Replication2ScaldingWaves>()
            .DeactivateOnExit<Replication2TimelessSpite>()
            .DeactivateOnExit<Replication2ReenactmentOrder>();
    }

    void BloodMana(uint id, float delay)
    {
        // reenactment casts are 4s apart, but actual AOEs trigger later, varying by mechanic
        // if we put this at the end of rep2 instead of here, it fucks up state transition text
        Timeout(id, delay)
            .DeactivateOnExit<Replication2ReenactmentTowers>()
            .DeactivateOnExit<Replication2ReenactmentScaldingWaves>()
            .DeactivateOnExit<Replication2ReenactmentAOEs>()
            .DeactivateOnExit<Replication2Staging>();

        Cast(id + 1u, AID.MutatingCells, 5.1f, 3f)
            .ActivateOnEnter<ManaSphere>();
        ComponentCondition<ManaSphere>(id + 3u, 1.8f, static comp => comp.HaveDebuff, "Alpha/beta debuffs");

        ComponentCondition<ManaSphere>(id + 0x10u, 8.2f, static comp => comp.Spheres.Count > 0, "Shapes appear");
        ComponentCondition<ManaSphere>(id + 0x11u, 8.7f, static comp => comp.SwapDone, "Debuffs swap");

        Cast(id + 0x100u, AID.BloodWakening, 11.7f, 3f)
            .ActivateOnEnter<BloodWakeningReplay>();
        ComponentCondition<BloodWakeningReplay>(id + 0x110u, 1.7f, static comp => comp.NumCasts > 0, "AOEs 1");
        ComponentCondition<BloodWakeningReplay>(id + 0x111u, 5.1f, static comp => comp.NumCasts >= 12, "AOEs 2")
            .DeactivateOnExit<BloodWakeningReplay>();

        CastMulti(id + 0x200u, [AID.NetherworldNear, AID.NetherworldFar], 0, 4.3f)
            .ActivateOnEnter<Netherworld>();
        ComponentCondition<Netherworld>(id + 0x210u, 1.3f, static comp => comp.NumCasts > 0, "Close/far stack")
            .DeactivateOnExit<ManaSphere>()
            .DeactivateOnExit<Netherworld>();

        Cast(id + 0x300u, AID.ArcadiaAflame, 1.8f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<ArcadiaAflame>()
            .DeactivateOnExit<ArcadiaAflame>();

        DoubleSobat(id + 0x400u, 4.3f);
    }

    void IdyllicDream(uint id, float delay)
    {
        Cast(id, AID.IdyllicDream, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<IdyllicDreamRaidwide>()
            .ActivateOnEnter<IdyllicDreamStaging>()
            .ActivateOnEnter<IdyllicDreamArena>()
            .DeactivateOnExit<IdyllicDreamRaidwide>()
            .ExecOnExit<IdyllicDreamStaging>(static comp => comp.WatchSpawns = false); // N->S clones spawn before 8x staging set does

        Cast(id + 0x10u, AID.Staging, 3.2f, 3f);

        ComponentCondition<IdyllicDreamStaging>(id + 0x20u, 5.4f, static comp => comp.PlayersAssigned, "Player clones appear");

        // red arena -> blue arena
        Cast(id + 0x100u, AID.TwistedVision, 5.8f, 4f);

        // 3x boss clones spawn along X=0, store casts for later
        Cast(id + 0x110u, AID.Replication, 3.1f, 3f)
            .ActivateOnEnter<IdyllicDreamPowerGusherSnakingKick>();

        // blue arena -> red arena, boss clones disappear before finishing cast
        Cast(id + 0x120u, AID.TwistedVision, 8.5f, 4f);

        // 8x boss clones spawn in clockwise paired order
        Cast(id + 0x130u, AID.Replication, 3.2f, 3f)
            .ExecOnEnter<IdyllicDreamStaging>(static comp => comp.WatchSpawns = true);

        // twisted vision resummons stored AOEs
        // clones pick tethers during cast
        CastStart(id + 0x140u, AID.TwistedVision, 15.5f)
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(static comp => comp.Visible = true);
        ComponentCondition<IdyllicDreamStaging>(id + 0x141u, 3.9f, static comp => comp.WurmsAssigned, "Clone tethers")
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(static comp => comp.Risky = true);
        CastEnd(id + 0x142u, 0.1f);

        CastStart(id + 0x150u, AID.LindwurmsMeteor, 3.5f)
            .ActivateOnEnter<LindwurmsMeteor>();
        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x151u, 0.9f, static comp => comp.NumCasts > 0, "Stored AOEs")
            .ExecOnExit<IdyllicDreamArena>(static comp => comp.Predict(7.8d));
        CastEnd(id + 0x152u, 4.1f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LindwurmsMeteor>();

        // platform transform during cast, towers appear on platforms at cast end
        CastStart(id + 0x160u, AID.Downfall, 3.1f)
            .ActivateOnEnter<IdyllicDreamElementalMeteor>();
        ComponentCondition<IdyllicDreamArena>(id + 0x161u, 0.9f, static comp => comp.State == 1, "Platforms appear");
        CastEnd(id + 0x162u, 2.2f);

        Cast(id + 0x200u, AID.ArcadianArcanumCast, 3.1f, 3f)
            .ActivateOnEnter<ArcadianArcanum>();
        ComponentCondition<ArcadianArcanum>(id + 0x202u, 1.3f, static comp => comp.NumCasts > 0, "Random spreads")
            .DeactivateOnExit<ArcadianArcanum>();

        // circle transform, clone mechanics trigger after a delay
        // 359.88 (cast start) -> 370.37 (first) -> 375.32 (second)
        CastStart(id + 0x210u, AID.TwistedVision, 2.8f)
            .ActivateOnEnter<IdyllicDreamWurmStackSpread>();
        ComponentCondition<IdyllicDreamArena>(id + 0x211u, 5.2f, static comp => comp.State == 0, "Platforms disappear");
        //.ExecOnExit<IdyllicDreamWurmStackSpread>(static comp => comp.EnableHints = true);

        ComponentCondition<IdyllicDreamWurmStackSpread>(id + 0x220u, 5.2f, static comp => comp.NumCasts == 2, "Clone mechanics start");
        ComponentCondition<IdyllicDreamWurmStackSpread>(id + 0x221u, 15f, static comp => comp.NumCasts == 8, "Clone mechanics end")
            .ExecOnExit<IdyllicDreamStaging>(static comp => comp.WurmsFinished = true);

        Timeout(id + 0x222u, 1.5f).DeactivateOnExit<IdyllicDreamWurmStackSpread>()
            .ExecOnExit<IdyllicDreamArena>(static comp => comp.Predict(8.8d))
            .ExecOnExit<IdyllicDreamElementalMeteor>(static comp => comp.CreateTowers());

        // platform transform, towers appear and activate
        Cast(id + 0x230u, AID.TwistedVision, 3.6f, 4f)
            .ActivateOnEnter<IdyllicDreamSharedState>()
            .ActivateOnEnter<IdyllicDreamLindwurmsDarkII>()
            .ActivateOnEnter<IdyllicDreamWindTower>()
            .ActivateOnEnter<IdyllicDreamHotBlooded>()
            .ActivateOnEnter<IdyllicDreamDoom>()
            .ActivateOnEnter<LindwurmsStoneIII>();

        ComponentCondition<IdyllicDreamArena>(id + 0x232u, 1.3f, static comp => comp.State == 1, "Platforms appear");
        ComponentCondition<IdyllicDreamElementalMeteor>(id + 0x233u, 3.1f, static comp => comp.NumCasts > 0, "Towers")
            .ExecOnEnter<IdyllicDreamLindwurmsDarkII>(static comp => comp.EnableHints = true)
            .DeactivateOnExit<IdyllicDreamElementalMeteor>();
        ComponentCondition<LindwurmsStoneIII>(id + 0x234u, 5.7f, static comp => comp.NumCasts > 0, "Delayed puddles")
            .ActivateOnEnter<LindwurmsPortent>()
            //.DeactivateOnExit<IdyllicDreamHotBlooded>() // keep enabled, in case the status lingers for some reason
            .DeactivateOnExit<IdyllicDreamLindwurmsDarkII>()
            .DeactivateOnExit<IdyllicDreamWindTower>()
            .DeactivateOnExit<LindwurmsStoneIII>();

        ComponentCondition<LindwurmsPortent>(id + 0x240u, 5f, static comp => comp.NumCasts > 0, "Proximity baits")
            .DeactivateOnExit<LindwurmsPortent>()
            .DeactivateOnExit<IdyllicDreamHotBlooded>()
            .DeactivateOnExit<IdyllicDreamDoom>()
            .DeactivateOnExit<IdyllicDreamSharedState>()
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(static comp => comp.WatchTeleport = true);

        // black hole appears and absorbs one clone; remaining clones jump
        Cast(id + 0x300u, AID.TemporalCurtain, 5.4f, 3f);

        // circle transform
        CastStart(id + 0x310u, AID.TwistedVision, 8.7f);
        ComponentCondition<IdyllicDreamArena>(id + 0x311u, 5.2f, static comp => comp.State == 0, "Platforms disappear")
            .ActivateOnEnter<IdyllicDreamManaBurstPlayer>()
            .ActivateOnEnter<IdyllicDreamHeavySlamPlayer>()
            .ActivateOnEnter<IdyllicDreamPlayerCastCounter>()
            .ExecOnEnter<IdyllicDreamManaBurstPlayer>(static comp => comp.Predict(0))
            .ExecOnEnter<IdyllicDreamHeavySlamPlayer>(static comp => comp.Predict(0));

        // clones replay stack/spread
        Cast(id + 0x320u, AID.Reenactment, 1.9f, 3f)
            .ExecOnEnter<IdyllicDreamManaBurstPlayer>(static comp => comp.Risky = true);
        ComponentCondition<IdyllicDreamPlayerCastCounter>(id + 0x322u, 3.6f, static comp => comp.NumCasts == 4, "Reenactment 1")
            .ExecOnExit<IdyllicDreamArena>(static comp => comp.Predict(6.7d));
        // platform transform, jumpy clones
        Cast(id + 0x330u, AID.TwistedVision, 1.5f, 4f)
            // TODO: fix aoe activation time, im tired
            .ExecOnEnter<IdyllicDreamPowerGusherSnakingKick>(static comp =>
            {
                comp.Visible = true;
                comp.Reset();
            });
        ComponentCondition<IdyllicDreamArena>(id + 0x332u, 1.3f, static comp => comp.State == 1, "Platforms appear");

        CastStart(id + 0x340u, AID.TwistedVision, 2.3f);
        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x341u, 0.9f, static comp => comp.NumCasts > 0, "Safe platform")
            .ExecOnExit<IdyllicDreamManaBurstPlayer>(static comp => comp.Predict(1))
            .ExecOnExit<IdyllicDreamHeavySlamPlayer>(static comp => comp.Predict(1));

        ComponentCondition<IdyllicDreamArena>(id + 0x350u, 4.3f, static comp => comp.State == 0, "Platforms disappear")
            .ExecOnExit<IdyllicDreamManaBurstPlayer>(static comp => comp.Risky = true);
        ComponentCondition<IdyllicDreamPlayerCastCounter>(id + 0x351, 6.6f, static comp => comp.NumCasts == 8, "Reenactment 2")
            .DeactivateOnExit<IdyllicDreamPlayerCastCounter>()
            .DeactivateOnExit<IdyllicDreamManaBurstPlayer>()
            .DeactivateOnExit<IdyllicDreamHeavySlamPlayer>()
            .DeactivateOnExit<IdyllicDreamStaging>()
            .ExecOnExit<IdyllicDreamPowerGusherSnakingKick>(static comp =>
            {
                comp.Visible = true;
                comp.Reset();
            });

        ComponentCondition<IdyllicDreamPowerGusherSnakingKick>(id + 0x352u, 4.8f, static comp => comp.NumCasts > 0, "Stored AOE")
            .DeactivateOnExit<IdyllicDreamPowerGusherSnakingKick>();

        Cast(id + 0x400u, AID.IdyllicDream, 1f, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<IdyllicDreamRaidwide>()
            .DeactivateOnExit<IdyllicDreamRaidwide>()
            .DeactivateOnExit<IdyllicDreamArena>();
    }

    void ArcadianHell(uint id, float delay)
    {
        Cast(id, AID.ReplicationHell, delay, 5f)
            .ActivateOnEnter<ArcadianHell5x>()
            .ActivateOnEnter<ArcadianHell9x>();

        Cast(id + 0x10u, AID.ArcadianHellRaidwide, 8.5f, 5f, "Raidwide x5")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x20u, AID.ArcadianHellRaidwide, 11.3f, 5f, "Raidwide x9")
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x100u, AID.ArcadianHellEnrage, 12.9f, 10f, "Enrage");
    }
}
