namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class M11STheTyrantStates : StateMachineBuilder
{
    public M11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        CrownOfArcadia(id, 5.18f, 5.01f, 1);
        Flatliner(id + 0x10, 460f, 6f);
        SimpleState(id + 0x20, 1000f, "Final Phase");
    }
    private void CrownOfArcadia(uint id, float delay, float cast, int seq)
    {
        Cast(id, (uint)AID.CrownOfArcadia, 0, 5f, "Crown of Arcadia")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnExit<CrownOfArcadia>()
            .ActivateOnExit<RawSteelTrophyAxe>()
            .ActivateOnExit<RawSteelTrophyScythe>()
            .ActivateOnExit<AssaultEvolvedSword>()
            .ActivateOnExit<AssaultEvolvedAxeStack>()
            .ActivateOnExit<AssaultEvolved_AxeAOE>()
            .ActivateOnExit<AssaultEvolvedScythe>()
            .ActivateOnExit<Cometite>()
            .ActivateOnExit<Comet>()
            .ActivateOnExit<CrushingComet>()
            .ActivateOnExit<EyeOfTheHurricane>()
            .ActivateOnExit<Explosion>()
            .ActivateOnExit<MaelstromVoidZones>()
            .ActivateOnExit<MaelstromGustCones>()
            .ActivateOnExit<GreatWallOfFire>()
            .ActivateOnExit<GreatWallOfFireExplosion>()
            .ActivateOnExit<OrbitalOmen>()
            .ActivateOnExit<FireAndFury>()
            .ActivateOnExit<MeteorainComets>()
            .ActivateOnExit<FearsomeFireball>()
            .ActivateOnExit<CosmicKiss>()
            .ActivateOnExit<CometTethers>()
            .ActivateOnExit<TripleTyrannhilation>()
            .ActivateOnExit<Flatliner>()
            .ActivateOnExit<FlatlinerKB>();
    }
    private void Flatliner(uint id, float delay, float cast)
    {
        Condition(id, 362f, () => Module.PrimaryActor.CastInfo?.Action.ID == (uint)AID.MajesticMeteor, "Majestic Meteor", 50, 0)
            .DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>()
            .DeactivateOnExit<AssaultEvolvedSword>()
            .DeactivateOnExit<AssaultEvolvedAxeStack>()
            .DeactivateOnExit<AssaultEvolved_AxeAOE>()
            .DeactivateOnExit<AssaultEvolvedScythe>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CrushingComet>()
            .DeactivateOnExit<EyeOfTheHurricane>()
            .DeactivateOnExit<Explosion>()
            .DeactivateOnExit<MaelstromVoidZones>()
            .DeactivateOnExit<MaelstromGustCones>()
            .DeactivateOnExit<FearsomeFireball>()
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<CometTethers>()
            .DeactivateOnExit<TripleTyrannhilation>()
            .DeactivateOnExit<Flatliner>()
            .DeactivateOnExit<FlatlinerKB>()
            .ActivateOnExit<CrownOfArcadia>()
            .ActivateOnExit<GreatWallOfFire>()
            .ActivateOnExit<GreatWallOfFireExplosion>()
            .ActivateOnExit<OrbitalOmen>()
            .ActivateOnExit<FireAndFury>()
            .ActivateOnExit<ExplosionTowers>()
            .ActivateOnExit<ExplosionTowerKnockback>()
            .ActivateOnExit<MajesticMeteor>()
            .ActivateOnExit<Tether1>()
            .ActivateOnExit<Tether2>()
            .ActivateOnExit<FireBreath>()
            .ActivateOnExit<MeteorainPortals>()
            .ActivateOnExit<MassiveMeteor>()
            .ActivateOnExit<ArcadionAvalanche>()
            .ActivateOnExit<ArcadionAvalancheSmash>()
            .ActivateOnExit<MajesticMeteorStorm>()
            .ActivateOnExit<MammothMeteor>()
            .ActivateOnExit<AtomicImpact>()
            .ActivateOnExit<AtomicImpactVoidZones>()
            .ActivateOnExit<CosmicKissTowers>()
            .ActivateOnExit<WeightyImpactTowers>()
            .ActivateOnExit<TwoWayFireball>()
            .ActivateOnExit<FourWayFireball>()
            .ActivateOnExit<HeartBreakerTower>();
    }
}