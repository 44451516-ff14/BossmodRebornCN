namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

sealed class FTME1TwoHeadedAevisStates : StateMachineBuilder
{
    private readonly FTME1TwoHeadedAevis _module;
    public FTME1TwoHeadedAevisStates(FTME1TwoHeadedAevis module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Buffet(id, 10.1f);
        FugueBreath(id + 0x10000u, 4f);
        ThunderfrostTempest(id + 0x20000u, 3.9f);
        CrossBlazeLoop(id + 0x30000u, 5.2f);
        ArchaeoFury(id + 0x40000u, 4.1f);
        FugueBreath(id + 0x50000u, 8.2f);
        ThunderfrostTempest(id + 0x60000u, 3.2f);
        ArcaneTerror(id + 0x70000u, 9.2f);
        BreathyDuet(id + 0x80000u, 7.1f);
        ArcaneFugue(id + 0x90000u, 7.8f);
        ThunderfrostTempest(id + 0xA0000u, 2.3f);
        ArchaeoFury(id + 0xB0000u, 3.1f);
        HissingResonance(id + 0xC0000u, 8.2f);
        ThunderfrostTempest(id + 0xD0000u, 4.1f);
        ArcaneTerror(id + 0xE0000u, 9.1f);
        ArcaneFugue(id + 0xF0000u, 5.2f);
        ThunderfrostTempest(id + 0x100000u, 4.4f);
        BreathyDuet(id + 0x110000u, 7.3f);
        ArchaeoFury(id + 0xB0000u, 5f); // unsure actual time
        //Enrage
    }

    private void Buffet(uint id, float delay)
    {
        Cast(id, AID.Buffet, delay, 5f, "Assigning boss")
            .ActivateOnEnter<Buffet>();
    }

    private void FugueBreath(uint id, float delay)
    {
        Condition(id, delay, () => _module.PrimaryActor.CastInfo != null || _module.BlueHead()!.CastInfo != null, "")
            .ActivateOnEnter<FreezingFugue>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<FulgurousFugue>()
            .ActivateOnEnter<PoisonBreath>();
        Condition(id + 0x1000u, 8.9f, () => _module.PrimaryActor.CastInfo == null || _module.BlueHead()!.CastInfo == null, "Breath + Fugue")
            .DeactivateOnExit<PoisonBreath>()
            .DeactivateOnExit<FulgurousFugue>()
            .DeactivateOnExit<StormsBreath>()
            .DeactivateOnExit<FreezingFugue>();
    }

    private void ThunderfrostTempest(uint id, float delay)
    {
        Cast(id, AID.ThunderfrostTempest, delay, 5f, "Raidwide")
            .ActivateOnEnter<ThunderfrostTempest>()
            .DeactivateOnExit<ThunderfrostTempest>();
    }

    private void ArchaeoFury(uint id, float delay)
    {
        CastStart(id, AID.ArchaeofuryCast, delay)
            .ActivateOnEnter<Archaeofury>();
        ComponentCondition<Archaeofury>(id + 0x1000u, 0f, static comp => comp.ActiveSpreads.Count != 0, "");
        ComponentCondition<Archaeofury>(id + 0x2000u, 5f, static comp => comp.ActiveSpreads.Count == 0, "Tankbuster spread")
            .DeactivateOnExit<Archaeofury>();
    }

    private void CrossBlazeLoop(uint id, float delay)
    {
        Condition(id, delay, () => _module.Green1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast || _module.Blue1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast, "")
            .ActivateOnEnter<CrossBlazeLoop>();
        ComponentCondition<CrossBlazeLoop>(id + 0x1000u, 25.1f, static comp => comp.ActiveCasters.Length == 0, "CrossBlazeLoop")
            .DeactivateOnExit<CrossBlazeLoop>();
    }

    private void ArcaneTerror(uint id, float delay)
    {
        Cast(id, AID.ArcaneRevelation, delay, 3f)
            .ActivateOnEnter<ArcaneRevelation>();
        CastMulti(id + 0x1000u, [AID.TwoTerrors1, AID.TwoTerrors2], 3.1f, 7f, "Arcane + Two Terrors")
            .ActivateOnEnter<TwoTerrorsThin>()
            .ActivateOnEnter<TwoTerrorsWide>()
            .DeactivateOnExit<TwoTerrorsWide>()
            .DeactivateOnExit<TwoTerrorsThin>()
            .DeactivateOnExit<ArcaneRevelation>();
    }

    private void BreathyDuet(uint id, float delay)
    {
        Cast(id, AID.Summon, delay, 3f)
            .ActivateOnEnter<BreathyDuet>();
        ComponentCondition<BreathyDuet>(id + 0x1000u, 31.5f, static comp => comp.NumCasts >= 4, "Breathy Duet")
            .DeactivateOnExit<BreathyDuet>();
    }

    private void ArcaneFugue(uint id, float delay)
    {
        Cast(id, AID.ArcaneRevelation, delay, 3f)
            .ActivateOnEnter<ArcaneRevelation>()
            .ActivateOnEnter<FreezingFulgurousFugue>();
        ComponentCondition<ArcaneRevelation>(id + 0x1000u, 18.2f, static comp => comp.NumCasts >= 16, "Arcane + Circle/Donut")
            .DeactivateOnExit<ArcaneRevelation>()
            .DeactivateOnExit<FreezingFulgurousFugue>();
    }

    private void HissingResonance(uint id, float delay)
    {
        Cast(id, AID.HissingResonance, delay, 3f)
            .ActivateOnEnter<HissingResonance>();
        Condition(id + 0x1000u, 3.1f, () => _module.Green1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast || _module.Blue1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast, "")
            .ActivateOnEnter<CrossBlazeLoop>();
        ComponentCondition<CrossBlazeLoop>(id + 0x1000u, 25.1f, static comp => comp.ActiveCasters.Length == 0, "KB + CrossBlazeLoop")
            .DeactivateOnExit<HissingResonance>()
            .DeactivateOnExit<CrossBlazeLoop>();
    }
}
