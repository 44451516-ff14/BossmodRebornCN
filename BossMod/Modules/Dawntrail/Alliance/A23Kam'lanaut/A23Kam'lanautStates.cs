namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class A23KamlanautStates : StateMachineBuilder
{
    public A23KamlanautStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ElementalBladeWide>()
            .ActivateOnEnter<ElementalBladeNarrow>()
            .ActivateOnEnter<ElementalResonance>()
            .ActivateOnEnter<SublimeElementsWide>()
            .ActivateOnEnter<SublimeElementsNarrow>()
            .ActivateOnEnter<EmpyrealBanishIII>()
            .ActivateOnEnter<EmpyrealBanishIV>()
            .ActivateOnEnter<LightBladeIllumedEstoc>()
            .ActivateOnEnter<ShieldBash>()
            .ActivateOnEnter<SublimeEstoc>()
            .ActivateOnEnter<GreatWheelCircle>()
            .ActivateOnEnter<GreatWheelCone>()
            .ActivateOnEnter<TranscendentUnion>()
            .ActivateOnEnter<EnspiritedSwordplayShockwave>()
            .ActivateOnEnter<PrincelyBlow>()
            .ActivateOnEnter<PrincelyBlowKB>();
    }
}
