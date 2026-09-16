namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

sealed class FTME3NecrophobiaStates : StateMachineBuilder
{
    public FTME3NecrophobiaStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        HailOfHellflares(id, 5f);
        DeathShroud(id + 0x10000u, 12.6f);
        HailOfHellflares(id + 0x20000u, 4f);
        DarkCurrent(id + 0x30000u, 11.1f);
        DigThreeGraves(id + 0x40000u, 12.4f);
        HailOfHellflares(id + 0x50000u, 5.3f);
        FertileGround(id + 0x60000u, 7.9f);
        HailOfHellflares(id + 0x70000u, 1.5f);
        DarkCurrent(id + 0x80000u, 11.5f);
        DigThreeGraves(id + 0x90000u, 12.4f);
        HailOfHellflares(id + 0xA0000u, 5.1f);
        Enrage(id + 0xB0000u, 10.2f); // find time
    }

    private void HailOfHellflares(uint id, float delay)
    {
        Cast(id, AID.HailOfHellflares, delay, 5f, "Raidwide")
            .ActivateOnEnter<HailOfHellflares>()
            .DeactivateOnExit<HailOfHellflares>();
    }
    private void DeathShroud(uint id, float delay)
    {
        Cast(id, AID.DeathShroud, delay, 7f, "");
        Cast(id + 0x1000u, AID.HeadsRoll, 2.1f, 3f, "Death Shroud start")
            .ActivateOnEnter<DeathShroud>();
        ComponentCondition<DeathShroud>(id + 0x2000u, 32.2f, static comp => comp.NumCasts >= 18, "Fire / Blizzard / Lightning")
            .DeactivateOnExit<DeathShroud>();
    }
    private void DarkCurrent(uint id, float delay)
    {
        Cast(id, AID.DarkCurrentCast, delay, 2.7f, "")
            .ActivateOnEnter<DarkCurrent>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<VacuumWave>();
        // if Deathly Rays finish before Vacuum Wave cast ends it skips Tankbuster state
        //ComponentCondition<DeathlyRay>(id + 0x1000, 9.6f, static comp => comp.NumCasts >= 16, "Dark Current")
        Cast(id + 0x1000u, AID.VacuumWave, 5.5f, 4f, "Dark Current")
            .DeactivateOnExit<VacuumWave>()
            .DeactivateOnExit<DeathlyRay>()
            .DeactivateOnExit<DarkCurrent>();
        Cast(id + 0x2000u, AID.CorpseMangler, 5f, 5f, "Tankbuster")
            .ActivateOnEnter<CorpseMangler>()
            .DeactivateOnExit<CorpseMangler>();
    }
    private void DigThreeGraves(uint id, float delay)
    {
        Cast(id, AID.DeathShroud, delay, 7f, "")
            .ActivateOnEnter<DigThreeGraves>();
        Cast(id + 0x1000u, AID.HeadsRoll, 2.1f, 3f, "");
        Cast(id + 0x2000u, AID.DigThreeGraves, 10.6f, 3f, "Dig Three Graves start")
            .ActivateOnEnter<SeveredDarkCurrent>()
            .ActivateOnEnter<VacuumWave>();
        ComponentCondition<VacuumWave>(id + 0x3000u, 32f, static comp => comp.NumCasts >= 1, "Dig Three Graves")
            .DeactivateOnExit<SeveredDarkCurrent>()
            .DeactivateOnExit<VacuumWave>()
            .DeactivateOnExit<DigThreeGraves>();
    }
    private void FertileGround(uint id, float delay)
    {
        CastStart(id, AID.FertileGroundCast, delay, "")
            .ActivateOnEnter<FertileGroundRaidwide>();
        ComponentCondition<FertileGroundRaidwide>(id + 0x1000u, 6.8f, static comp => comp.NumCasts >= 1, "Raidwide")
            .ActivateOnEnter<FertileGround>()
            .ActivateOnEnter<DeathShroud>()
            .DeactivateOnExit<FertileGroundRaidwide>();
        Cast(id + 0x2000u, AID.DeathShroud, 8.2f, 7f, "");
        Cast(id + 0x3000u, AID.HeadsRoll, 2.1f, 3f, "");
        Cast(id + 0x4000u, AID.SpellProcession, 11.1f, 5f, "Fertile Ground start")
            .ActivateOnEnter<SpellProcession>()
            .DeactivateOnExit<SpellProcession>();
        ComponentCondition<FertileGround>(id + 0x5000u, 49.8f, static comp => comp.NumCasts >= 32, "Fertile Ground")
            .DeactivateOnExit<DeathShroud>()
            .DeactivateOnExit<FertileGround>();
    }
    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.NihilityCast, delay, 12f);
        Cast(id + 0x1000u, AID.Nihility, 0f, 2f, "Enrage");
    }
}
