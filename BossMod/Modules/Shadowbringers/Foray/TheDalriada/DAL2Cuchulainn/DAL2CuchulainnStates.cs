namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

sealed class DAL2CuchulainnStates : StateMachineBuilder
{
    public DAL2CuchulainnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FleshNecromass>()
            .ActivateOnEnter<FleshNecromassJumps>()
            .ActivateOnEnter<FellFlowAOE>()
            .ActivateOnEnter<FellFlowBait>()
            .ActivateOnEnter<GhastlyAura>()
            .ActivateOnEnter<AmbientPulsation>()
            .ActivateOnEnter<NecroticBillow>()
            .ActivateOnEnter<BurgeoningDread>()
            .ActivateOnEnter<MightOfMalice>()
            .ActivateOnEnter<PutrifiedSoulBurgeoningDreadGhastlyAura>();
    }
}
