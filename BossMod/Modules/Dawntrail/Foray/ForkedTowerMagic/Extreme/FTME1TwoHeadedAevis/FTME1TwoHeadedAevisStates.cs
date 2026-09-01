namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME1TwoHeadedAevis;

[SkipLocalsInit]
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
        FugueBreath(id + 0x10000, 4f);
        ThunderfrostTempest(id + 0x20000, 3.9f);
        CrossBlazeLoop(id + 0x30000, 5.2f);
        ArchaeoFury(id + 0x40000, 4.1f);
        FugueBreath(id + 0x50000, 8.2f);
        ThunderfrostTempest(id + 0x60000, 3.2f);
        ArcaneTerror(id + 0x70000, 9.2f);
        BreathyDuet(id + 0x80000, 7.1f);
        ArcaneFugue(id + 0x90000, 7.8f);
        ThunderfrostTempest(id + 0xA0000, 2.3f);
        ArchaeoFury(id + 0xB0000, 3.1f);
        HissingResonance(id + 0xC0000, 8.2f);
        ThunderfrostTempest(id + 0xD0000, 4.1f);
        ArcaneTerror(id + 0xE0000, 9.1f);
        ArcaneFugue(id + 0xF0000, 5.2f);
        ThunderfrostTempest(id + 0x100000, 4.4f);
        BreathyDuet(id + 0x110000, 7.3f);
        ArchaeoFury(id + 0xB0000, 5f); // unsure actual time
        //Enrage
    }

    private void Buffet(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.Buffet, delay, 5f, true, "Assigning boss")
            .ActivateOnEnter<Buffet>();
    }

    private void FugueBreath(uint id, float delay)
    {
        Condition(id, delay, () => _module.GreenHead()!.CastInfo != null || _module.BlueHead()!.CastInfo != null, "")
            .ActivateOnEnter<FreezingFugue>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<FulgurousFugue>()
            .ActivateOnEnter<PoisonBreath>();
        Condition(id + 0x1000, 8.9f, () => _module.GreenHead()!.CastInfo == null || _module.BlueHead()!.CastInfo == null, "Breath + Fugue")
            .DeactivateOnExit<PoisonBreath>()
            .DeactivateOnExit<FulgurousFugue>()
            .DeactivateOnExit<StormsBreath>()
            .DeactivateOnExit<FreezingFugue>();
    }

    private void ThunderfrostTempest(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.ThunderfrostTempest, delay, 5f, true, "Raidwide")
            .ActivateOnEnter<ThunderfrostTempest>()
            .DeactivateOnExit<ThunderfrostTempest>();
    }

    private void ArchaeoFury(uint id, float delay)
    {
        ActorCastStart(id, _module.GreenHead, (uint)AID.ArchaeofuryCast, delay)
            .ActivateOnEnter<Archaeofury>();
        ComponentCondition<Archaeofury>(id + 0x1000, 0f, comp => comp.ActiveSpreads.Count != 0, "");
        ComponentCondition<Archaeofury>(id + 0x2000, 5f, comp => comp.ActiveSpreads.Count == 0, "Tankbuster spread")
            .DeactivateOnExit<Archaeofury>();
    }

    private void CrossBlazeLoop(uint id, float delay)
    {
        Condition(id, delay, () => _module.Green1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast || _module.Blue1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast, "")
            .ActivateOnEnter<CrossBlazeLoop>();
        ComponentCondition<CrossBlazeLoop>(id + 0x1000, 25.1f, comp => comp.ActiveCasters.Length == 0, "CrossBlazeLoop")
            .DeactivateOnExit<CrossBlazeLoop>();
    }

    private void ArcaneTerror(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.ArcaneRevelation, delay, 3f, true, "")
            .ActivateOnEnter<ArcaneRevelation>();
        ActorCastMulti(id + 0x1000, _module.GreenHead, [(uint)AID.TwoTerrors1, (uint)AID.TwoTerrors2], 3.1f, 7f, true, "Arcane + Two Terrors")
            .ActivateOnEnter<TwoTerrorsThin>()
            .ActivateOnEnter<TwoTerrorsWide>()
            .DeactivateOnExit<TwoTerrorsWide>()
            .DeactivateOnExit<TwoTerrorsThin>()
            .DeactivateOnExit<ArcaneRevelation>();
    }

    private void BreathyDuet(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.Summon, delay, 3f, true)
            .ActivateOnEnter<BreathyDuet>();
        ComponentCondition<BreathyDuet>(id + 0x1000, 31.5f, comp => comp.NumCasts >= 4, "Breathy Duet")
            .DeactivateOnExit<BreathyDuet>();
    }

    private void ArcaneFugue(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.ArcaneRevelation, delay, 3f, true, "")
            .ActivateOnEnter<ArcaneRevelation>()
            .ActivateOnEnter<FreezingFulgurousFugue>();
        ComponentCondition<ArcaneRevelation>(id + 0x1000, 18.2f, comp => comp.NumCasts >= 16, "Arcane + Circle/Donut")
            .DeactivateOnExit<ArcaneRevelation>()
            .DeactivateOnExit<FreezingFulgurousFugue>();
    }

    private void HissingResonance(uint id, float delay)
    {
        ActorCast(id, _module.GreenHead, (uint)AID.HissingResonance, delay, 3f, true, "")
            .ActivateOnEnter<HissingResonance>();
        Condition(id + 0x1000, 3.1f, () => _module.Green1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast || _module.Blue1()?.CastInfo?.Action.ID == (uint)AID.BlazeFirstCast, "")
            .ActivateOnEnter<CrossBlazeLoop>();
        ComponentCondition<CrossBlazeLoop>(id + 0x1000, 25.1f, comp => comp.ActiveCasters.Length == 0, "KB + CrossBlazeLoop")
            .DeactivateOnExit<HissingResonance>()
            .DeactivateOnExit<CrossBlazeLoop>();
    }
}
