namespace BossMod.Stormblood.Trial.T09Seiryu;

sealed class T09SeiryuStates : StateMachineBuilder
{
    public T09SeiryuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<BlueBolt>()
            .ActivateOnEnter<RedRush>()
            .ActivateOnEnter<HundredTonzeSwing>()
            .ActivateOnEnter<Handprint>()
            .ActivateOnEnter<CoursingRiver>()
            .ActivateOnEnter<DragonsWake>()
            .ActivateOnEnter<FifthElement>()
            .ActivateOnEnter<FortuneBladeSigil>()
            .ActivateOnEnter<InfirmSoul>()
            .ActivateOnEnter<KanaboAOE>()
            .ActivateOnEnter<KanaboBait>()
            .ActivateOnEnter<OnmyoSerpentEyeSigil>()
            .ActivateOnEnter<SerpentDescending>()
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<ForbiddenArts>()
            .ActivateOnEnter<SerpentAscending>()
            .ActivateOnEnter<ForceOfNature1>()
            .ActivateOnEnter<ForceOfNature2>();
    }
}
