namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class BA4ProtoOzmaStates : StateMachineBuilder
{
    public BA4ProtoOzmaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<TransitionAttacks>()
            .ActivateOnEnter<AutoAttacksCube>()
            .ActivateOnEnter<AutoAttacksPyramid>()
            .ActivateOnEnter<AutoAttacksStar>()
            .ActivateOnEnter<BlackHole>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<ShootingStar>()
            .ActivateOnEnter<MeteorStack>()
            .ActivateOnEnter<MeteorBait>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<Ozmaspheres>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<Holy>();
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???");
    }
    //TODO: implement
    //private void XXX(uint id, float delay)
}