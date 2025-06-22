namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class M06NSugarRiotStates : StateMachineBuilder
{
    public M06NSugarRiotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<SingleDoubleStyle>()
            .ActivateOnEnter<MousseMural>()
            .ActivateOnEnter<SprayPain>()
            .ActivateOnEnter<WarmBomb>()
            .ActivateOnEnter<CoolBomb>()
            .ActivateOnEnter<PuddingParty>()
            .ActivateOnEnter<MousseTouchUp>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<TasteOfFire>()
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<Highlightning>();
    }
}
