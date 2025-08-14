namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class DAL4DiabloArmamentStates : StateMachineBuilder
{
    public DAL4DiabloArmamentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<RuinousPseudoomen>()
            .ActivateOnEnter<AethericBoomExplosionDiabolicGateVoidSystemsOverload>()
            .ActivateOnEnter<AdvancedNox>()
            .ActivateOnEnter<Aetheroplasm>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<DeadlyDealingKB>()
            .ActivateOnEnter<DeadlyDealingAOE>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<LightPseudopillar>()
            .ActivateOnEnter<PillarOfShamash>()
            .ActivateOnEnter<UltimatePseudoterror>()
            .ActivateOnEnter<AdvancedDeathIV>()
            .ActivateOnEnter<PillarOfShamashBait>()
            .ActivateOnEnter<PillarOfShamashStack>()
            .ActivateOnEnter<AccelerationBomb>();
    }
}
