namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class UnSeiryuStates : StateMachineBuilder
{
    public UnSeiryuStates(BossModule module) : base(module)
    {
        SimplePhase(default, Phase1, "P1")
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.PrimaryActor.IsTargetable;
        DeathPhase(1u, Intermission)
            .OnExit(() => module.Arena.Bounds = Stormblood.Trial.T09Seiryu.Seiryu.Phase2WaterBounds)
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || module.FindComponent<StrengthOfSpirit>() is StrengthOfSpirit raidwide && raidwide.NumCasts != 0;
        DeathPhase(2u, Phase3)
            .ActivateOnEnter<GreatTyphoonCone>()
            .ActivateOnEnter<GreatTyphoonDonut>()
            .ActivateOnEnter<ArenaChanges>();
    }

    private void Phase1(uint id)
    {
        FifthElement(id, 8.1f);
        SerpentAscending1(id + 0x10000u, 9.1f);
        Cursekeeper1(id + 0x20000u, 5.1f);
        SimpleState(id + 0x30000u, 11.2f, "Intermission (timing varies)")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Intermission(uint id)
    {
        IntermissionPart1(id, 5f);
        IntermissionPart2(id + 0x10000u, 1.9f);
        SimpleState(id + 0x20000u, 68.8f, "Enrage or raidwide")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Phase3(uint id)
    {
        Targetable(id, true, 29.7f, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        CoursingRiverHandprintForceOfNature(id + 0x10000u, 18.8f);
        ForbiddenArts(id + 0x20000u, 0.7f);
        OnmyoSerpentEyeSigil(id + 0x30000u, 26.7f);
        FortuneCalamityBladeSigil(id + 0x40000u, 16f);
        OnmyoSerpentEyeSigil(id + 0x50000u, 5.8f);
        FifthElement(id + 0x60000u, 3.6f);
        SerpentAscending2(id + 0x70000u, 16.2f);
        ForbiddenArts(id + 0x70000u, 2f);
        FifthElement(id + 0x80000u, 3.2f);
        Cursekeeper1(id + 0x90000u, 7.2f);
        Cursekeeper2(id + 0xA0000u, 1.1f);
        PseudoAddPhase(id + 0xB0000u, 25.5f);
        CoursingRiverForceOfNature(id + 0xC0000u, 8.6f);
        OnmyoSerpentEyeSigilHandprint(id + 0xD0000u, 8.6f);
        CoursingRiverForceOfNature(id + 0xE0000u, 8.6f);
        ForbiddenArts(id + 0xF0000u, 2.2f);
        FifthElement(id + 0x100000u, 3.2f);
        Cursekeeper1(id + 0x110000u, 12.2f);
        Cursekeeper2(id + 0x120000u, 1.1f);
        OnmyoSerpentEyeSigil(id + 0x130000u, 12.8f);
        SerpentAscending3(id + 0x140000u, 8.5f);
        FifthElement(id + 0x150000u, 0.6f);
        FifthElement(id + 0x160000u, 15.6f);
        FifthElement(id + 0x170000u, 3.2f);
        SimpleState(id + 0x180000u, 22f, "Enrage"); // casts ends ~21.1 seconds later, action effect another ~0.9s later
    }

    private void FifthElement(uint id, float delay)
    {
        Cast(id, (uint)AID.FifthElement, delay, 4f, "Raidwide")
            .ActivateOnEnter<FifthElement>()
            .DeactivateOnExit<FifthElement>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SerpentAscending1(uint id, float delay)
    {
        ComponentCondition<SerpentDescendingSpread>(id, delay, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnExit<FortuneBladeSigil>()
            .ActivateOnEnter<SerpentDescendingSpread>();
        ComponentCondition<SerpentDescendingSpread>(id + 0x10u, 6.1f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<SerpentDescendingSpread>();
        ComponentCondition<FortuneBladeSigil>(id + 0x20u, 1.8f, comp => comp.NumCasts == 9, "Criss-cross AOEs")
            .DeactivateOnExit<FortuneBladeSigil>();
        ComponentCondition<OnmyoSerpentEyeSigil>(id + 0x30u, 7.3f, comp => comp.NumCasts == 1, "Circle AOE")
            .ActivateOnEnter<OnmyoSerpentEyeSigil>()
            .DeactivateOnExit<OnmyoSerpentEyeSigil>();
    }

    private void Cursekeeper1(uint id, float delay)
    {
        ComponentCondition<Cursekeeper>(id, delay, comp => comp.CurrentBaits.Count != 0, "Tank swap")
            .ActivateOnEnter<KarmicCurse>()
            .ActivateOnEnter<Cursekeeper>();
        ComponentCondition<Cursekeeper>(id + 0x10u, 8.1f, comp => comp.NumCasts != 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<Cursekeeper>();
        ComponentCondition<KarmicCurse>(id + 0x20u, 4f, comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnExit<KarmicCurse>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Cursekeeper2(uint id, float delay)
    {
        ComponentCondition<Cursekeeper>(id, delay, comp => comp.CurrentBaits.Count != 0, "Tank swap")
            .ActivateOnEnter<KarmicCurse>()
            .ActivateOnEnter<Cursekeeper>();
        ComponentCondition<Cursekeeper>(id + 0x10u, 8.1f, comp => comp.NumCasts != 0, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<Cursekeeper>();
        ComponentCondition<KarmicCurse>(id + 0x20u, 4f, comp => comp.NumCasts != 0, "Raidwide")
            .DeactivateOnExit<KarmicCurse>()
            .ActivateOnEnter<FifthElement>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<FifthElement>(id + 0x30u, 1.8f, comp => comp.NumCasts != 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<FifthElement>();
    }

    private void IntermissionPart1(uint id, float delay)
    {
        Targetable(id, false, delay, "Boss untargetable");
        ComponentCondition<BlueBolt>(id + 0x10u, 5f, comp => comp.CurrentBaits.Count != 0, "Line stack + baits appear")
            .ActivateOnEnter<BlueBolt>()
            .ActivateOnEnter<BlueBoltStretch>()
            .ActivateOnEnter<RedRush>();
        ComponentCondition<BlueBolt>(id + 0x20u, 6f, comp => comp.CurrentBaits.Count == 0, "Line stack + baits resolve")
            .DeactivateOnExit<BlueBolt>()
            .DeactivateOnExit<BlueBoltStretch>()
            .DeactivateOnExit<RedRush>();
        ComponentCondition<HundredTonzeSwing>(id + 0x30u, 4.8f, comp => comp.NumCasts == 2, "Circle AOEs")
            .ActivateOnEnter<HundredTonzeSwing>()
            .DeactivateOnExit<HundredTonzeSwing>();
        ComponentCondition<Kanabo>(id + 0x40u, 3.1f, comp => comp.Active, "Tank tethers appear")
            .ActivateOnEnter<Kanabo>()
            .ActivateOnEnter<YamaKagura>();
        ComponentCondition<YamaKagura>(id + 0x50u, 3.2f, comp => comp.NumCasts == 4, "Rect AOEs")
            .DeactivateOnExit<YamaKagura>();
        ComponentCondition<Kanabo>(id + 0x60u, 3.1f, comp => comp.NumCasts == 2, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .DeactivateOnExit<Kanabo>();
    }

    private void IntermissionPart2(uint id, float delay)
    {
        ComponentCondition<Adds>(id, delay, comp => comp.Started, "Adds appear")
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<StrengthOfSpirit>()
            .ActivateOnEnter<Stoneskin>();
    }

    private void CoursingRiverHandprintForceOfNature(uint id, float delay)
    {
        ComponentCondition<CoursingRiverForceOfNature>(id, delay, comp => comp.NumCasts != 0, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<CoursingRiverForceOfNature>()
            .DeactivateOnExit<CoursingRiverForceOfNature>();
        ComponentCondition<Handprint>(id + 0x10u, 14.3f, comp => comp.NumCasts != 0, "Half-room cleave 1")
            .ActivateOnEnter<Handprint>();
        ComponentCondition<FifthElement>(id + 0x20u, 4.6f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<FifthElement>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<FifthElement>();
        ComponentCondition<Handprint>(id + 0x30u, 3.6f, comp => comp.NumCasts == 2, "Half-room cleave 2");
        ComponentCondition<Handprint>(id + 0x40u, 8.1f, comp => comp.NumCasts == 3, "Half-room cleave 3")
            .DeactivateOnExit<Handprint>();
        ComponentCondition<ForceOfNatureAOE>(id + 0x50u, 8.6f, comp => comp.NumCasts != 0, "Circle AOE + Knockback 1")
            .ActivateOnEnter<ForceOfNatureAOE>()
            .ActivateOnEnter<CoursingRiverForceOfNature>()
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<ForceOfNatureAOE>();
        ComponentCondition<CoursingRiverForceOfNature>(id + 0x60u, 1.9f, comp => comp.NumCasts == 2, "Knockback 2")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<CoursingRiverForceOfNature>();
    }

    private void ForbiddenArts(uint id, float delay)
    {
        ComponentCondition<ForbiddenArts>(id, delay, comp => comp.CurrentBaits.Count != 0, "Line stacks appear")
            .ActivateOnEnter<ForbiddenArts>();
        ComponentCondition<ForbiddenArts>(id + 0x10u, 5.3f, comp => comp.NumCasts != 0, "Line stack 1");
        ComponentCondition<ForbiddenArts>(id + 0x20u, 2.1f, comp => comp.NumCasts == 2, "Line stack 2")
            .DeactivateOnExit<ForbiddenArts>();
    }

    private void OnmyoSerpentEyeSigil(uint id, float delay)
    {
        ComponentCondition<OnmyoSerpentEyeSigil>(id, delay, comp => comp.NumCasts != 0, "In or out AOE 1")
            .ActivateOnEnter<OnmyoSerpentEyeSigil>();
        ComponentCondition<OnmyoSerpentEyeSigil>(id + 0x10u, 3.2f, comp => comp.NumCasts == 2, "In or out AOE 2")
            .DeactivateOnExit<OnmyoSerpentEyeSigil>();
    }

    private void FortuneCalamityBladeSigil(uint id, float delay)
    {
        ComponentCondition<FortuneBladeSigil>(id, delay, comp => comp.NumCasts != 0, "Criss-cross AOEs 1")
            .ActivateOnEnter<FortuneBladeSigil>();
        ComponentCondition<FortuneBladeSigil>(id + 0x10u, 1.5f, comp => comp.NumCasts == 18, "Criss-cross AOEs 2")
            .DeactivateOnExit<FortuneBladeSigil>();
    }

    private void SerpentAscending2(uint id, float delay)
    {
        ComponentCondition<SerpentDescendingSpread>(id, delay, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<SerpentAscending>()
            .ActivateOnEnter<SerpentDescendingSpread>();
        ComponentCondition<SerpentAscending>(id + 0x10u, 0.3f, comp => comp.Towers.Count != 0, "Towers appear");
        ComponentCondition<SerpentsDescendingAOE>(id + 0x20u, 0.7f, comp => comp.Casters.Count != 0, "Baited circle AOEs appear")
            .ActivateOnEnter<SerpentsDescendingAOE>();
        ComponentCondition<SerpentsDescendingAOE>(id + 0x30u, 3f, comp => comp.Casters.Count == 0, "Circle AOEs resolve")
            .DeactivateOnExit<SerpentsDescendingAOE>();
        ComponentCondition<SerpentDescendingSpread>(id + 0x40u, 2f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<SerpentDescendingSpread>();
        ComponentCondition<SerpentAscending>(id + 0x50u, 2.2f, comp => comp.NumCasts == 4, "Towers resolve")
            .DeactivateOnExit<SerpentAscending>();
    }

    private void PseudoAddPhase(uint id, float delay)
    {
        ComponentCondition<HundredTonzeSwing>(id, delay, comp => comp.NumCasts == 2, "Circle AOEs")
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<OnmyoSerpentEyeSigil>()
            .DeactivateOnExit<HundredTonzeSwing>();
        ComponentCondition<OnmyoSerpentEyeSigil>(id + 0x10u, 0.5f, comp => comp.NumCasts != 0, "In or out AOE 1");
        ComponentCondition<Kanabo>(id + 0x20u, 2.5f, comp => comp.Active, "Tank tethers appear")
            .ActivateOnEnter<Kanabo>();
        ComponentCondition<OnmyoSerpentEyeSigil>(id + 0x30u, 0.6f, comp => comp.NumCasts == 2, "In or out AOE 2")
            .DeactivateOnExit<OnmyoSerpentEyeSigil>();
        ComponentCondition<Kanabo>(id + 0x40u, 5.6f, comp => comp.NumCasts == 2, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<Kanabo>();
        ComponentCondition<BlueBolt>(id + 0x50u, 4f, comp => comp.CurrentBaits.Count != 0, "Line stack + baits appear")
            .ActivateOnEnter<BlueBolt>()
            .ActivateOnEnter<RedRushKnockback>()
            .ActivateOnEnter<BlueBoltStretch>()
            .ActivateOnEnter<RedRush>();
        ComponentCondition<BlueBolt>(id + 0x60u, 6.1f, comp => comp.CurrentBaits.Count == 0, "Line stack + baits resolve")
            .DeactivateOnExit<BlueBolt>()
            .DeactivateOnExit<BlueBoltStretch>()
            .DeactivateOnExit<RedRushKnockback>()
            .DeactivateOnExit<RedRush>();
        ComponentCondition<SerpentDescendingSpread>(id + 0x70u, 0.9f, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<SerpentDescendingSpread>();
        ComponentCondition<SerpentsDescendingAOE>(id + 0x80u, 1.1f, comp => comp.Casters.Count != 0, "Baited circle AOEs appear")
            .ActivateOnExit<FortuneBladeSigil>()
            .ActivateOnEnter<SerpentsDescendingAOE>();
        ComponentCondition<SerpentsDescendingAOE>(id + 0x90u, 3f, comp => comp.Casters.Count == 0, "Circle AOEs resolve")
            .DeactivateOnExit<SerpentsDescendingAOE>();
        ComponentCondition<SerpentDescendingSpread>(id + 0xA0u, 2f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<SerpentDescendingSpread>();
        ComponentCondition<FortuneBladeSigil>(id + 0xB0u, 3.4f, comp => comp.NumCasts != 0, "Criss-cross AOEs 1")
            .ActivateOnExit<Handprint>();
        ComponentCondition<FortuneBladeSigil>(id + 0xC0u, 1.5f, comp => comp.NumCasts == 18, "Criss-cross AOEs 2")
            .DeactivateOnExit<FortuneBladeSigil>();
        ComponentCondition<Handprint>(id + 0xD0u, 2.9f, comp => comp.NumCasts != 0, "Half-room cleave 1");
        ComponentCondition<FifthElement>(id + 0xE0u, 5.3f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<FifthElement>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<FifthElement>();
        ComponentCondition<Handprint>(id + 0xF0u, 2.9f, comp => comp.NumCasts == 2, "Half-room cleave 2");
        ComponentCondition<Handprint>(id + 0x100u, 8.1f, comp => comp.NumCasts == 3, "Half-room cleave 3")
            .DeactivateOnExit<Handprint>();
    }

    private void CoursingRiverForceOfNature(uint id, float delay)
    {
        ComponentCondition<ForceOfNatureAOE>(id, delay, comp => comp.NumCasts != 0, "Circle AOE + Knockback 1")
            .ActivateOnEnter<ForceOfNatureAOE>()
            .ActivateOnEnter<CoursingRiverForceOfNature>()
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<ForceOfNatureAOE>();
        ComponentCondition<CoursingRiverForceOfNature>(id + 0x10u, 2.5f, comp => comp.NumCasts == 2, "Knockback 2")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<CoursingRiverForceOfNature>();
    }

    private void OnmyoSerpentEyeSigilHandprint(uint id, float delay)
    {
        ComponentCondition<OnmyoSerpentEyeSigil>(id, delay, comp => comp.NumCasts != 0, "In or out AOE 1")
            .ActivateOnEnter<Handprint>()
            .ActivateOnEnter<OnmyoSerpentEyeSigil>();
        ComponentCondition<OnmyoSerpentEyeSigil>(id + 0x10u, 3.2f, comp => comp.NumCasts == 2, "In or out AOE 2")
            .DeactivateOnExit<OnmyoSerpentEyeSigil>();
        ComponentCondition<Handprint>(id + 0x20u, 2.7f, comp => comp.NumCasts != 0, "Half-room cleave 1");
        ComponentCondition<Handprint>(id + 0x30u, 8.1f, comp => comp.NumCasts == 2, "Half-room cleave 2");
        ComponentCondition<Handprint>(id + 0x40u, 8.1f, comp => comp.NumCasts == 3, "Half-room cleave 3")
            .DeactivateOnExit<Handprint>();
    }

    private void SerpentAscending3(uint id, float delay)
    {
        ComponentCondition<SerpentDescendingSpread>(id, delay, comp => comp.Spreads.Count != 0, "Spreads appear")
            .ActivateOnEnter<SerpentAscending>()
            .ActivateOnEnter<SerpentDescendingSpread>();
        ComponentCondition<SerpentAscending>(id + 0x10u, 0.3f, comp => comp.Towers.Count != 0, "Towers appear");
        ComponentCondition<SerpentsDescendingAOE>(id + 0x20u, 0.7f, comp => comp.Casters.Count != 0, "Baited circle AOEs appear")
            .ActivateOnEnter<SerpentsDescendingAOE>();
        ComponentCondition<SerpentsDescendingAOE>(id + 0x30u, 3f, comp => comp.Casters.Count == 0, "Circle AOEs resolve")
            .ActivateOnExit<FortuneBladeSigil>()
            .DeactivateOnExit<SerpentsDescendingAOE>();
        ComponentCondition<SerpentDescendingSpread>(id + 0x40u, 2f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<SerpentDescendingSpread>();
        ComponentCondition<SerpentAscending>(id + 0x50u, 2.2f, comp => comp.NumCasts == 4, "Towers resolve")
            .DeactivateOnExit<SerpentAscending>();
        ComponentCondition<FortuneBladeSigil>(id + 0x60u, 2.4f, comp => comp.NumCasts != 0, "Criss-cross AOEs 1");
        ComponentCondition<FortuneBladeSigil>(id + 0x70u, 1.5f, comp => comp.NumCasts == 18, "Criss-cross AOEs 2")
            .DeactivateOnExit<FortuneBladeSigil>();
    }
}
