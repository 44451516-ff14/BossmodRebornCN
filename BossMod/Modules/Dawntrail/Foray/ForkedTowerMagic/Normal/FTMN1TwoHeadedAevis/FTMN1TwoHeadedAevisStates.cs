namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[SkipLocalsInit]
sealed class TwoHeadedAevisStates : StateMachineBuilder
{
    public TwoHeadedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<LightningIcePoison>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<HypothermalCombustionShock>()
            .ActivateOnEnter<HissingReprise>()
            .ActivateOnEnter<BlazeLoop>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<Archaeofury1>()
            .ActivateOnEnter<Archaeofury2>();
    }
}
