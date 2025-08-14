namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class T01ValigarmandaStates : StateMachineBuilder
{
    public T01ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SlitheringStrike>()
            .ActivateOnEnter<ArcaneLightning>()
            .ActivateOnEnter<StranglingCoilSusurrantBreath>()
            .ActivateOnEnter<SkyruinHailOfFeathersDisasterZoneRuinForetold>()
            .ActivateOnEnter<RuinfallTower>()
            .ActivateOnEnter<RuinfallKB>()
            .ActivateOnEnter<RuinfallAOE>()
            .ActivateOnEnter<ChillingCataclysm>()
            .ActivateOnEnter<NorthernCross>()
            .ActivateOnEnter<FreezingDust>()
            .ActivateOnEnter<CalamitousEcho>()
            .ActivateOnEnter<CalamitousCry1>()
            .ActivateOnEnter<CalamitousCry2>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<IceTalon>()
            .ActivateOnEnter<Tulidisaster1>()
            .ActivateOnEnter<Tulidisaster2>()
            .ActivateOnEnter<Tulidisaster3>()
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<BlightedBolt1>()
            .ActivateOnEnter<BlightedBolt2>();
    }
}
