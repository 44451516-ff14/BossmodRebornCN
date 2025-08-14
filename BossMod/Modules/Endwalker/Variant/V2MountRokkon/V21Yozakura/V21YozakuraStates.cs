namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class V21YozakuraStates : StateMachineBuilder
{
    public V21YozakuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            //Right No Dogu
            .ActivateOnEnter<RootArrangement>()
            .ActivateOnEnter<AccursedSeedling>()
            //Right Dogu
            .ActivateOnEnter<Witherwind>()
            //Left Windy
            .ActivateOnEnter<WindblossomWhirl>()
            .ActivateOnEnter<LevinblossomStrike>()
            .ActivateOnEnter<Mudrain>()
            .ActivateOnEnter<DriftingPetals>()
            //Left Rainy
            .ActivateOnEnter<Icebloom>()
            .ActivateOnEnter<Shadowflight>()
            .ActivateOnEnter<MudPie>()
            //Middle Rope Pulled
            .ActivateOnEnter<FireblossomFlare>()
            .ActivateOnEnter<ArtOfTheFluff>()
            //Middle Rope Unpulled
            .ActivateOnEnter<LevinblossomLance>()
            .ActivateOnEnter<TatamiGaeshi>()
            //Standard
            .ActivateOnEnter<GloryNeverlasting>()
            .ActivateOnEnter<KugeRantsuiOkaRanman>()
            .ActivateOnEnter<SealOfRiotousBloom>()
            .ActivateOnEnter<SeasonsOfTheFleeting>()
            .ActivateOnEnter<ArtOfTheFireblossom>()
            .ActivateOnEnter<ArtOfTheWindblossom>();
    }
}
