namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V22Moko;

abstract class V22MokoStates : StateMachineBuilder
{
    public V22MokoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 1
            .ActivateOnEnter<Unsheathing>()
            .ActivateOnEnter<VeilSever>()
            // Route 2
            .ActivateOnEnter<ScarletAuspice>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<Explosion>()
            // Route 3
            .ActivateOnEnter<GhastlyGrasp>()
            .ActivateOnEnter<YamaKagura>()
            // Route 4
            .ActivateOnEnter<Spiritflame>()
            .ActivateOnEnter<Spiritflames>()
            // Standard
            .ActivateOnEnter<SpearmanOrdersFast>()
            .ActivateOnEnter<SpearmanOrdersSlow>()
            .ActivateOnEnter<KenkiReleaseMoonlessNight>()
            .ActivateOnEnter<IronRain>()
            .ActivateOnEnter<Giri>()
            .ActivateOnEnter<AzureAuspice>()
            .ActivateOnEnter<BoundlessScarletAzure>()
            .ActivateOnEnter<UpwellFirst>()
            .ActivateOnEnter<UpwellRest>();
    }
}

sealed class V22MokoPath2States(BossModule module) : V22MokoStates(module) { }
sealed class V22MokoOtherPathsStates(BossModule module) : V22MokoStates(module) { }
