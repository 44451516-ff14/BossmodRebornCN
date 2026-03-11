namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V12PariOfPlenty;

class V12PariOfPlentyStates : StateMachineBuilder
{
    public V12PariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<CarpetRide>()
            .ActivateOnEnter<LeftRightFireflight>()
            .ActivateOnEnter<WheelOfFireflight>()
            .ActivateOnEnter<BurningBleam>()
            .ActivateOnEnter<GaleForce>()
            .ActivateOnEnter<PredatorySwoop>()
            .ActivateOnEnter<TranscendentFlight>()
            .ActivateOnEnter<StrongWind>()
            .ActivateOnEnter<ThievesWeaves>()
            .ActivateOnEnter<SpurningFlames>()
            .ActivateOnEnter<ImpassionedSparks>()
            .ActivateOnEnter<ScouringScorn>();
    }
}
