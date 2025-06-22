namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

sealed class EndwalkerStates : StateMachineBuilder
{
    public EndwalkerStates(Endwalker module) : base(module)
    {
        DeathPhase(default, id => { SimpleState(id, 10000, "Enrage"); })
            .ActivateOnEnter<Megaflare>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Puddles>()
            .ActivateOnEnter<JudgementBolt>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<StarBeyondStars>()
            .ActivateOnEnter<TheEdgeUnbound>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<NineNightsAvatar>()
            .ActivateOnEnter<NineNightsHelpers>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<Exaflare>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<DeadGaze>()
            .ActivateOnEnter<MortalCoil>()
            .ActivateOnEnter<TidalWave2>();

        SimplePhase(1u, id => { SimpleState(id, 10000, "Enrage"); }, "P2")
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<SilveredEdge>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<SwiftAsShadow>()
            .ActivateOnEnter<Candlewick>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Extinguishment>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<UnmovingDvenadkatik>()
            .ActivateOnEnter<TheEdgeUnbound2>()
            .Raw.Update = () => module.ZenosP2() is var ZenosP2 && ZenosP2 != null && !ZenosP2.IsTargetable && ZenosP2.HPMP.CurHP <= 1u;
    }
}
