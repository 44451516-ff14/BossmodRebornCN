namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

sealed class DRN4PhantomStates : StateMachineBuilder
{
    public DRN4PhantomStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<MaledictionOfAgony>()
            .ActivateOnEnter<SwirlingMiasma>()
            .ActivateOnEnter<UndyingHatred>()
            .ActivateOnEnter<VileWave>()
            .ActivateOnEnter<CreepingMiasma>();
    }
}
