namespace BossMod.Dawntrail.Trial.T06Arkveld;

[SkipLocalsInit]
sealed class GuardianArkveldStates : StateMachineBuilder
{
    public GuardianArkveldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // Raidwides
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ForgedFury>()
            // Major tells
            .ActivateOnEnter<ChainbladeBlowLines>()
            .ActivateOnEnter<WyvernsRadianceCleave>()
            .ActivateOnEnter<GuardianResonanceRect>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Concentric1>()
            .ActivateOnEnter<Concentric2>()
            .ActivateOnEnter<Concentric3>()
            .ActivateOnEnter<Concentric4>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<SteeltailThrust>()
            // Player mechanics
            .ActivateOnEnter<WildEnergy>()
            .ActivateOnEnter<ChainbladeCharge>()
            .ActivateOnEnter<ResonanceTowerSmall>()
            .ActivateOnEnter<ResonanceTowerLarge>()
            // Arena hazards
            .ActivateOnEnter<CrackedCrystalSmall>()
            .ActivateOnEnter<CrackedCrystalLarge>()
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsWealAOE>()  // the casted rect telegraph
            .ActivateOnEnter<WyvernsWealPulses>()
            .ActivateOnEnter<WyvernsWealIrregularCastLane>();
    }
}
