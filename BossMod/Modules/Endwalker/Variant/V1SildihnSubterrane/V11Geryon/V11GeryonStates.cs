namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class V11GeryonStates : StateMachineBuilder
{
    public V11GeryonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Gigantomill>()
            .ActivateOnEnter<SubterraneanShudderColossalLaunch>()
            .ActivateOnEnter<ColossalStrike>()
            .ActivateOnEnter<ColossalCharge>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<ColossalSwing>()
            .ActivateOnEnter<RunawaySludge>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Intake>()
            .ActivateOnEnter<RollingBoulder>()
            .ActivateOnEnter<SuddenlySewage>()
            .ActivateOnEnter<RunawayRunoff>();
    }
}
