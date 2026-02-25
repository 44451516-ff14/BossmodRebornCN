/*
namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<AssaultWeaponTimeline>()
            .ActivateOnEnter<AssaultFloorPredictor>();
    }

    private void SinglePhase(uint id)
    {
        CrownOfArcadia(id, 5.18f); //00:05
        RawSteelTrophy(id + 0x1000, 5.1f); //00:15-00:33
        TrophyWeapons(id + 0x2000, 12.4f);  //00:33-01:17
        VoidStardust(id + 0x3000, 8.1f); //01:17-01:48 - observed entering state at 60 seconds since pull
        CrownOfArcadia(id + 0x4000, 5f); // 01:58
        DanceOfDomination(id + 0x5000, 60f); //02:14-02:35
        RawSteelTrophy(id + 0x6000, 60f); //02:35-02:55
        Charybdistopia(id + 0x7000, 60f); //02:55-03:03
        UltimateTrophyWeapons(id + 0x8000, 60f); //03:03-03:53
        OneAndOnly(id + 0x9000, 60f); //03:53
        Flatliner(id + 0x10000, 460f); //05:58
        EclipticMeteor(id + 0x20000, 300f);
        SimpleState(id + 0xFF0000u, 10000f, "Report Me");
    }
    private void CrownOfArcadia(uint id, float delay) //0, 0x4000
    {
        Cast(id, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }
    private void RawSteelTrophy(uint id, float delay) //0x1000, 0x6000
    {
        CastMulti(id, [(uint)AID.RawSteelTrophyAxe, (uint)AID.RawSteelTrophyScythe], delay, 2f, "Select Weapon")
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>();

        Condition(id + 0x10, 6.3f, () =>
            {
                var axe = Module.FindComponent<RawSteelTrophyAxe>();
                if (axe != null && axe.NumCasts > 0)
                    return true;

                var scythe = Module.FindComponent<RawSteelTrophyScythe>();
                return scythe != null && scythe.NumCasts > 0;
            }, "Raw Steel Trophy resolves", 2f)
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>();
    }
    private void TrophyWeapons(uint id, float delay) //0x2000
    {
        Cast(id, (uint)AID.TrophyWeapons, delay, 2f, "Trophy Weapons")
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>();

        Condition(id + 0x10, 30f, TrophyWeaponsResolved(), "Trophy Weapons Complete", 10f, 10f)
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>();
    }
    private void VoidStardust(uint id, float delay) //0x3000
    {
        Cast(id, (uint)AID.TrophyWeapons, delay, 2.7f, "Stack/Spread, 3 Weapons, Spread/Stack")
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>();

        Cast(id + 0x10, (uint)AID.VoidStardust, delay, 4f, "Bait Puddles")
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CrushingComet>();

        Condition(id + 0x20, 30f, TrophyWeaponsResolved(), "Trophy Weapons Complete", 10f, 10f)
                .DeactivateOnExit<AssaultEvolvedSword>()
                .DeactivateOnExit<AssaultEvolvedScythe>()
                .DeactivateOnExit<AssaultEvolvedAxeStack>();

        ComponentCondition<Comet>(id + 0x30, 20f, comp => comp.NumFinishedSpreads != 0)
            .DeactivateOnExit<Comet>();
        ComponentCondition<CrushingComet>(id + 0x40, 20f, comp => comp.NumFinishedStacks != 0)
            .DeactivateOnExit<CrushingComet>();
        ComponentCondition<Cometite>(id + 0x60, 20f, comp => comp.NumCasts >= 48) // it's 48 casts if everyone is alive...
            .DeactivateOnExit<Cometite>();
    }
    private void DanceOfDomination(uint id, float delay) //0x5000
    {
        Cast(id, (uint)AID.DanceOfDominationTrophy, delay, 2f, "Multi-hit Raidwide")
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<EyeOfTheHurricane>();
        ComponentCondition<EyeOfTheHurricane>(id + 0x10, 20f, comp => comp.NumFinishedStacks != 0)
            .DeactivateOnExit<DanceOfDomination>()
            .DeactivateOnExit<Explosion>()
            .DeactivateOnExit<EyeOfTheHurricane>();
    }
    private void Charybdistopia(uint id, float delay) //0x7000
    {
        Cast(id, (uint)AID.Charybdistopia, delay, 2f, "Sets HP to 0")
            .ActivateOnEnter<Charybdistopia>()
            .DeactivateOnExit<Charybdistopia>();
    }
    private void UltimateTrophyWeapons(uint id, float delay) //0x8000
    {
        Cast(id, (uint)AID.UltimateTrophyWeapons, delay, 2f, "Ultimate Trophy Weapons")
            .ActivateOnEnter<AssaultEvolvedSword>()
            .ActivateOnEnter<AssaultEvolvedScythe>()
            .ActivateOnEnter<AssaultEvolvedAxeStack>()
            .ActivateOnEnter<MaelstromVoidZones>()
            .ActivateOnEnter<MaelstromGustCones>();

        ComponentCondition<MaelstromGustCones>(id + 0x20, 60f, comp => comp._resolved, "Bait Tornadoes", 10f, 10f)
            .DeactivateOnExit<MaelstromVoidZones>()
            .DeactivateOnExit<MaelstromGustCones>()
            .DeactivateOnExit<AssaultWeaponTimeline>()
            .DeactivateOnExit<AssaultFloorPredictor>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>();
        Cast(id + 0x30, (uint)AID.ImmortalReign, delay, 2f, "Immortal Reign");
    }
    private void OneAndOnly(uint id, float delay) //0x9000
    {
        Cast(id, (uint)AID.OneAndOnly, delay, 9f)
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<OneAndOnly>()
            .ActivateOnEnter<GreatWallOfFire>()
            .ActivateOnEnter<GreatWallOfFireExplosion>()
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .ActivateOnEnter<MeteorainComets>()
            .ActivateOnEnter<FearsomeFireball>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<CometTethers>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<TripleTyrannhilation>()
            .DeactivateOnEnter<AssaultWeaponTimeline>()
            .DeactivateOnEnter<AssaultFloorPredictor>()
            .DeactivateOnExit<OneAndOnly>();
        ComponentCondition<GreatWallOfFireExplosion>(id + 0x10, 30f, comp => comp.NumCasts >= 2, "Shared Tank Cleaves", 10f, 10f)
            .SetHint(StateMachine.StateHint.Tankbuster);

        ComponentCondition<OrbitalOmen>(id + 0x20, 60f, comp => comp.NumCasts >= 8, "Orbital Omen", 10f, 10f)
            .DeactivateOnExit<GreatWallOfFire>()
            .DeactivateOnExit<GreatWallOfFireExplosion>()
            .DeactivateOnExit<FireAndFury>()
            .DeactivateOnExit<OrbitalOmen>();
        ComponentCondition<TripleTyrannhilation>(id + 0x30, 60f, comp => !comp._active, "Hide!", 100f)
            .DeactivateOnExit<MeteorainComets>()
            .DeactivateOnExit<FearsomeFireball>()
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<CometTethers>()
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<TripleTyrannhilation>();
    }
    private void Flatliner(uint id, float delay) //0x10000
    {
        Cast(id, (uint)AID.Flatliner, delay, 6)
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerKB>()
            .ActivateOnEnter<ExplosionTowers>()
            .ActivateOnEnter<ExplosionTowerKnockback>()
            .ActivateOnEnter<MajesticMeteor>()
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .ActivateOnEnter<MeteorainPortals>()
            .ActivateOnEnter<MeteorMechanicHints>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<MajesticMeteorStorm>()
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheSmash>()
            .DeactivateOnExit<Flatliner>();
        ComponentCondition<FlatlinerKB>(id + 0x10, 10f, comp => comp.NumCasts != 0)
            .DeactivateOnExit<FlatlinerKB>();
        CastMulti(id + 0x30, [(uint)AID.ArcadionAvalanche_Pick1, (uint)AID.ArcadionAvalanche_Pick2, (uint)AID.ArcadionAvalanche_Pick3, (uint)AID.ArcadionAvalanche_Pick4], delay, 15f, "Get to safe platform")
            .DeactivateOnExit<Tether1>()
            .DeactivateOnExit<Tether2>()
            .DeactivateOnExit<MajesticMeteor>()
            .DeactivateOnExit<MeteorainPortals>()
            .DeactivateOnExit<MeteorMechanicHints>()
            .DeactivateOnExit<MassiveMeteor>()
            .DeactivateOnExit<MajesticMeteorStorm>()
            .DeactivateOnExit<ArcadionAvalanche>();
    }
    private void EclipticMeteor(uint id, float delay) //0x20000
    {
        Cast(id, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia")
            .DeactivateOnEnter<ExplosionTowers>()
            .DeactivateOnEnter<ExplosionTowerKnockback>()
            .DeactivateOnEnter<ArcadionAvalanche>()
            .DeactivateOnEnter<ArcadionAvalancheSmash>()
            .ActivateOnExit<GreatWallOfFire>()
            .ActivateOnExit<GreatWallOfFireExplosion>()
            .ActivateOnExit<OrbitalOmen>()
            .ActivateOnExit<FireAndFury>();

        Cast(id + 0x10, (uint)AID.GreatWallOfFire, delay, 5f, "Great Wall Of Fire");
        Cast(id + 0x20, (uint)AID.OrbitalOmen, delay, 5f, "Orbital Omen")
            .DeactivateOnExit<GreatWallOfFire>()
            .DeactivateOnExit<GreatWallOfFireExplosion>();
        Cast(id + 0x30, (uint)AID.FireAndFury, delay, 4f, "Fire and Fury")
            .DeactivateOnExit<FireAndFury>();
        Cast(id + 0x40, (uint)AID.CrownOfArcadia, delay, 5f, "Crown of Arcadia");
        Cast(id + 0x50, (uint)AID.EclipticStampede, delay, 5f, "Ecliptic Stampede")
            .ActivateOnEnter<Tether1>()
            .ActivateOnEnter<Tether2>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<AtomicImpact>()
            .ActivateOnEnter<AtomicImpactVoidZones>()
            .ActivateOnEnter<CosmicKissTowers>()
            .ActivateOnEnter<WeightyImpactTowers>()
            .ActivateOnEnter<TwoWayFireball>()
            .ActivateOnEnter<FourWayFireball>()
            .ActivateOnEnter<HeartBreakerTower>();
    }
    private Func<bool> TrophyWeaponsResolved()
    {
        AssaultEvolvedSword? sword = null;
        AssaultEvolvedScythe? scythe = null;
        AssaultEvolvedAxeStack? axe = null;

        return () =>
        {
            sword ??= Module.FindComponent<AssaultEvolvedSword>();
            scythe ??= Module.FindComponent<AssaultEvolvedScythe>();
            axe ??= Module.FindComponent<AssaultEvolvedAxeStack>();

            return sword!.NumCasts > 0
                && scythe!.NumCasts > 0
                && axe!.NumCasts > 0;
        };
    }
}
*/
