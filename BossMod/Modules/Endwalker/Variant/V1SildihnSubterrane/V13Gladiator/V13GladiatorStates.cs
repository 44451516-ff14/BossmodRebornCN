namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

sealed class V13GladiatorStates : StateMachineBuilder
{
    public V13GladiatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunderedRemains>()
            .ActivateOnEnter<BitingWindSmall>()
            .ActivateOnEnter<BitingWindUpdraft>()
            .ActivateOnEnter<BitingWindUpdraftVoidzone>()
            .ActivateOnEnter<RingOfMight1>()
            .ActivateOnEnter<RingOfMight2>()
            .ActivateOnEnter<RingOfMight3>()
            .ActivateOnEnter<RackAndRuin>()
            .ActivateOnEnter<RushOfMight>()
            .ActivateOnEnter<ShatteringSteelMeteor>()
            .ActivateOnEnter<FlashOfSteel>()
            .ActivateOnEnter<SculptorsPassion>()
            .ActivateOnEnter<GoldenFlame>()
            .ActivateOnEnter<SilverFlame>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<MightySmite>();
    }
}
