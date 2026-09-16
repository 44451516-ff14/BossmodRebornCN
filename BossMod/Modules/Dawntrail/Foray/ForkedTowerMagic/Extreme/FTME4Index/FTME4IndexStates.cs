namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class FTME4IndexStates : StateMachineBuilder
{
    public FTME4IndexStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<OmniElementPanels>();
    }

    private void SinglePhase(uint id)
    {
        DualCast(id, 9.1f);
        PropulsionProphecy(id + 0x10000u, 11.3f);
        DualCast(id + 0x20000u, 3f);
        AllKnowingFlames(id + 0x30000u, 11.2f);
        OmniElements(id + 0x40000u, 6.1f);
        ElementaryChemistry(id + 0x50000u, 5.3f);
        AllKnowingFlames(id + 0x60000u, 8.1f);
        Summon(id + 0x70000u, 6f);
        QuadrilogyOfImplements2(id + 0x80000u, 13.3f);
        DualCast(id + 0x90000u, 2.9f);
        AllKnowingFlames(id + 0xA0000u, 11.2f);
        OmniElements2(id + 0xB0000u, 6f);
        ElementaryChemistry(id + 0xC0000u, 5.2f);
        AllKnowingFlames(id + 0xD0000u, 8.2f);
        PropulsionProphecy(id + 0xE0000u, 6f);
        DualCast(id + 0xF0000u, 3f);
        Enrage(id + 0x100000u, 15.3f);
    }

    private void DualCast(uint id, float delay)
    {
        Cast(id, AID.Dualcast, delay, 3f);
        Cast(id + 0x1000u, AID.FlareCast, 2.1f, 5f, "Raidwide x2")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void PropulsionProphecy(uint id, float delay)
    {
        Cast(id, AID.PropulsiveProphecy, delay, 3f, "KB + Quadrilogy start")
            .ActivateOnEnter<QuadrilogyOfImplements>()
            .ActivateOnEnter<Shockwave>();
        ComponentCondition<Shockwave>(id + 0x1000u, 17f, static comp => comp.Casters.Count > 0, "");
        ComponentCondition<Shockwave>(id + 0x1001u, 5f, static comp => comp.Casters.Count == 0, "Knockback");
        ComponentCondition<QuadrilogyOfImplements>(id + 0x2000u, 11.6f, static comp => comp.NumCasts >= 10, "Quadrilogy")
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<QuadrilogyOfImplements>();
    }

    private void AllKnowingFlames(uint id, float delay)
    {
        CastStart(id, AID.AllKnowingFlames, delay, "")
            .ActivateOnEnter<AllKnowingFlames>()
            .ActivateOnEnter<AllConsumingFlames>()
            .ActivateOnEnter<AllMightyFlames>();
        ComponentCondition<AllMightyFlames>(id + 0x1000u, 12.1f, static comp => comp.NumFinishedSpreads >= 3, "Spreads")
            .DeactivateOnExit<AllMightyFlames>()
            .DeactivateOnExit<AllConsumingFlames>()
            .DeactivateOnExit<AllKnowingFlames>();
    }
    private void OmniElements(uint id, float delay)
    {
        // mechanic done when both SealedImplements resolved (numcasts >= 4)
        Cast(id, AID.OmniElements, delay, 4f, "Raidwide + Arena change")
            .ActivateOnEnter<ElementIII>()
            .ActivateOnEnter<Predict>()
            .ActivateOnEnter<ElementaryExpansion>()
            .ActivateOnEnter<SealedImplements>();
        // is Sealed Implements always one then the other, or is it possible for Harp x2?
        ComponentCondition<SealedImplements>(id + 0x1000u, 48.8f, static comp => comp.VisualCasts == 2 && comp.ActiveCasters.Length == 0, "Omni-Elements")
            .DeactivateOnExit<SealedImplements>()
            .DeactivateOnExit<ElementaryExpansion>()
            .DeactivateOnExit<Predict>()
            .DeactivateOnExit<ElementIII>()
            .ActivateOnExit<ElementaryChemistry>();
    }

    private void ElementaryChemistry(uint id, float delay)
    {
        Cast(id, AID.ElementaryChemistry, delay, 20f, "Elementary Chemistry")
            .ActivateOnEnter<ElementaryChemistryPlatform>()
            .DeactivateOnExit<ElementaryChemistry>();
        // attack and arena change actually happens 1.4s after cast end
    }

    private void Summon(uint id, float delay)
    {
        Cast(id, AID.Summon, delay, 3f, "Summon adds")
            .ActivateOnEnter<SummonBombs>()
            .ActivateOnEnter<SummonBirds>()
            .ActivateOnEnter<SunderingSpellblade>()
            .ActivateOnEnter<BladeBlitz>();
        Cast(id + 0x1000u, AID.SunderingSpellbladeCast, 3.1f, 5f, "Exaflares start");
        Cast(id + 0x2000u, AID.BladeblitzCast, 7.1f, 5f, "Bladeblitz start");
    }

    private void QuadrilogyOfImplements2(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_QuadrilogyOfImplements2, delay, 13.6f, "Quadrilogy start")
            .ActivateOnEnter<QuadrilogyOfImplements>()
            .DeactivateOnEnter<SunderingSpellblade>();
        ComponentCondition<QuadrilogyOfImplements>(id + 0x1000u, 11.6f, static comp => comp.NumCasts >= 10, "Quadrilogy")
            .DeactivateOnExit<QuadrilogyOfImplements>()
            .DeactivateOnExit<SummonBirds>()
            .DeactivateOnExit<SummonBombs>()
            .DeactivateOnExit<BladeBlitz>();
    }

    private void OmniElements2(uint id, float delay)
    {
        // 2nd time includes Evocation and Propulsive
        // does this have a quadrilogy? or just sealed?
        Cast(id, AID.OmniElements, delay, 4f, "Raidwide + Arena change")
            .ActivateOnEnter<ElementIII>()
            .ActivateOnEnter<Predict>()
            .ActivateOnEnter<ElementaryExpansion>()
            .ActivateOnEnter<ElementaryEvocation>()
            .ActivateOnEnter<SealedImplements>()
            .ActivateOnEnter<Shockwave>();
        ComponentCondition<SealedImplements>(id + 0x1000u, 65.9f, static comp => comp.VisualCasts == 2 && comp.ActiveCasters.Length == 0, "Omni-Elements")
            .DeactivateOnExit<Shockwave>()
            .DeactivateOnExit<SealedImplements>()
            .DeactivateOnExit<ElementaryEvocation>()
            .DeactivateOnExit<ElementaryExpansion>()
            .DeactivateOnExit<Predict>()
            .DeactivateOnExit<ElementIII>()
            .ActivateOnExit<ElementaryChemistry>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.ElementaryChemistryEnrage, delay, 10f, "Enrage");
    }
}
