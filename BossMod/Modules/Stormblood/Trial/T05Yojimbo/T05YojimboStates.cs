namespace BossMod.Stormblood.Trial.T05Yojimbo;

class T05YojimboStates : StateMachineBuilder
{
    public T05YojimboStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fragility>()
            .ActivateOnEnter<MettaGiri>()
            .ActivateOnEnter<Yukikaze2>()
            .ActivateOnEnter<Gekko2>()
            .ActivateOnEnter<Kasha2>()
            .ActivateOnEnter<TinySong>()
            .ActivateOnEnter<DragonsLair>()
            .ActivateOnEnter<BitterEnd2>()
            .ActivateOnEnter<DragonNight>()
            .ActivateOnEnter<GigaJump>()
            .ActivateOnEnter<AmeNoMurakumo>()
            .ActivateOnEnter<ElectrogeneticForce2>()
            .ActivateOnEnter<Enchain>()
            .ActivateOnEnter<HellsGate>()
            .ActivateOnEnter<Masamune>()
            .ActivateOnEnter<ZanmaZanmai>()
            .ActivateOnEnter<EpicStormsplitter>();
    }
}
