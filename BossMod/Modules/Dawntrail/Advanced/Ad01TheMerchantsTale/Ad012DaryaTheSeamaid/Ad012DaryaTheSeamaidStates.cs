namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad012DaryaTheSeamaid;

[SkipLocalsInit]
sealed class Ad012DaryaTheSeamaidStates : StateMachineBuilder
{
    public Ad012DaryaTheSeamaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<NearFarTide>()
            .ActivateOnEnter<EchoedSerenade>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<SeaShackles>()
            .ActivateOnEnter<AquaSpear>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<HydrobulletSpread>()
            .ActivateOnEnter<TidalSpout>()
            .ActivateOnEnter<AlluringOrder>();
    }
}
