namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class T05NecronStates : StateMachineBuilder
{
    private Wipe? wipe;

    public T05NecronStates(BossModule module) : base(module)
    {
        bool IsWipedOrLeftRaid()
        {
            wipe ??= module.FindComponent<Wipe>();
            return (wipe?.Wiped ?? false) || module.WorldState.CurrentCFCID != 1061u;
        }
        TrivialPhase()
            .ActivateOnEnter<Wipe>()
            .ActivateOnEnter<Prisons>()
            .ActivateOnEnter<FearOfDeathGrandCross>()
            .ActivateOnEnter<ChokingGrasp>()
            .ActivateOnEnter<FearOfDeathAOE>()
            .ActivateOnEnter<MementoMori>()
            .ActivateOnEnter<DarknessOfEternity>()
            .ActivateOnEnter<ColdGripExistentialDread>()
            .ActivateOnEnter<Aetherblight>()
            .ActivateOnEnter<FearOfDeathAOE2>()
            .ActivateOnEnter<GrandCrossArenaChange>()
            .ActivateOnEnter<NeutronRing>()
            .ActivateOnEnter<GrandCrossBait>()
            .ActivateOnEnter<GrandCrossRect>()
            .ActivateOnEnter<GrandCrossProx>()
            .ActivateOnEnter<BlueShockwave>()
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<MassMacabre>()
            .ActivateOnEnter<SpreadingFear>()
            .Raw.Update = () => module.PrimaryActor.IsDead || IsWipedOrLeftRaid();
    }
}
