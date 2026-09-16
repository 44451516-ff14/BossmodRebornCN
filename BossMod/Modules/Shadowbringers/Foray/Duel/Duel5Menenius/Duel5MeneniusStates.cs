namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

sealed class Duel5MeneniusStates : StateMachineBuilder
{
    public Duel5MeneniusStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ReactiveMunition>()
            .ActivateOnEnter<GunberdShot>()
            .ActivateOnEnter<LargeGigaTempest>()
            .ActivateOnEnter<SmallGigaTempest>()
            .ActivateOnEnter<RuinationExaflare>()
            .ActivateOnEnter<ProactiveMunition>()
            .ActivateOnEnter<MagitekImpetus>()
            .ActivateOnEnter<BlueHiddenMines>()
            .ActivateOnEnter<RedHiddenMines>();
    }

    private void SinglePhase(uint id)
    {
        CallousCrossfire(id, 13f);

        MagitekMinefield(id + 0x10000u, 12f);
        GigaTempest(id + 0x20000u, 11.25f);
        ReadyShot(id + 0x30000u, 15.5f);
        Gunberd(id + 0x40000u, 2f);
        MagitekImpetus(id + 0x50000u, 2.6f);
        MagitekMinefield(id + 0x60000u, 6.2f);
        ReadyShot(id + 0x70000u, 10.25f);
        Gunberd(id + 0x80000u, 2f);
        MagitekMinefield(id + 0x90000u, 5.6f);
        Ruination(id + 0xA0000u, 6.5f);
        SpiralScourge(id + 0xB0000u, 18.25f);

        ProactiveMunition(id + 0x100000u, 7.25f);
        MagitekMinefield(id + 0x110000u, 7.25f);
        ReactiveMunition(id + 0x120000u, 7.25f);
        SenseWeakness(id + 0x130000u, 13f);
        ReadyShot(id + 0x140000u, 12.25f);
        GigaTempest(id + 0x150000u, 2f);
        MagitekImpetus(id + 0x160000u, 3.3f);
        Gunberd(id + 0x170000u, 10f);
        ReactiveMunition(id + 0x180000u, 13.6f);
        Ruination(id + 0x190000f, 4f);
        SenseWeakness(id + 0x1A0000u, 8.25f);
        IndiscriminateDetonation(id + 0x1B0000u, 3.25f);

        ReadyShot(id + 0x200000u, 11.25f);
        ReactiveMunition(id + 0x210000u, 2f);
        MagitekMinefield(id + 0x220000u, 2f);
        MagitekImpetus(id + 0x230000u, 10.3f);
        MagitekMinefield(id + 0x240000u, 8.25f);
        Gunberd(id + 0x250000u, 9.25f);
        MagitekMinefield(id + 0x260000u, 8.6f);
        ReadyShot(id + 0x270000u, 12.5f);
        Ruination(id + 0x280000u, 2.25f);
        MagitekMinefield(id + 0x290000u, 10.3f);
        Gunberd(id + 0x2A0000u, 9.3f);
        ReadyShot(id + 0x2B0000u, 13.8f);
        GigaTempest(id + 0x2C0000u, 2.2f);
        MagitekImpetus(id + 0x2D0000u, 3.5f);
        Gunberd(id + 0x2E0000u, 10.3f);
        ReactiveMunition(id + 0x2F0000u, 13.7f);
        Ruination(id + 0x300000u, 4.5f);
        SenseWeakness(id + 0x310000u, 8.25f);
        IndiscriminateDetonation(id + 0x320000u, 3.2f);

        TeraTempest(id + 0x400000u, 12.9f);
    }

    private void CallousCrossfire(uint id, float delay)
    {
        Cast(id, AID.CallousCrossfire, delay, 4f, "Turret Crossfire")
            .ActivateOnEnter<CallousCrossfire>()
            .DeactivateOnExit<CallousCrossfire>();
    }

    private void MagitekMinefield(uint id, float delay)
    {
        Cast(id, AID.MagitekMinefield, delay, 3f, "Place Mine");
    }

    private void IndiscriminateDetonation(uint id, float delay)
    {
        Cast(id, AID.IndiscriminateDetonation, delay, 4f, "Detonate Mines");
    }

    private void GigaTempest(uint id, float delay)
    {
        Cast(id, AID.GigaTempest, delay, 5f, "Gigatempest");
    }

    private void MagitekImpetus(uint id, float delay)
    {
        Cast(id, AID.MagitekImpetus, delay, 3f, "Place Forced March");
    }

    private void ReadyShot(uint id, float delay)
    {
        CastMulti(id, [AID.DarkShot, AID.WindslicerShot], delay, 4f, "Load Dark/Windslicer Shot");
    }

    private void Gunberd(uint id, float delay)
    {
        CastMulti(id, [AID.GunberdDark, AID.GunberdWindslicer], delay, 4f, "Shoot Dark/Windslicer Shot");
    }

    private void Ruination(uint id, float delay)
    {
        Cast(id, AID.Ruination, delay, 4f, "Ruination")
            .ActivateOnEnter<RuinationCross>()
            .DeactivateOnExit<RuinationCross>();
    }

    private void SpiralScourge(uint id, float delay)
    {
        Cast(id, AID.SpiralScourge, delay, 6f, "Tankbuster")
            .ActivateOnEnter<SpiralScourge>()
            .DeactivateOnExit<SpiralScourge>();
    }
    private void ProactiveMunition(uint id, float delay)
    {
        Cast(id, AID.ProactiveMunition, delay, 5f, "Chasing AOE");
    }

    private void ReactiveMunition(uint id, float delay)
    {
        Cast(id, AID.ReactiveMunition, delay, 3f, "Place Acceleration Bomb");
    }

    private void SenseWeakness(uint id, float delay)
    {
        Cast(id, AID.SenseWeakness, delay, 4.5f, "Move")
            .ActivateOnEnter<SenseWeakness>()
            .DeactivateOnExit<SenseWeakness>();
    }

    private void TeraTempest(uint id, float delay)
    {
        Cast(id, AID.TeraTempest, delay, 25f, "Enrage");
    }
}
