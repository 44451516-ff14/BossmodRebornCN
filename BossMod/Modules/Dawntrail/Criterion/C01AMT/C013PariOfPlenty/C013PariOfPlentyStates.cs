namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

[SkipLocalsInit]
sealed class PariOfPlentyStates : StateMachineBuilder {
    public PariOfPlentyStates(BossModule module) : base(module) {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id) {
        FireFlight(id, 7.1f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void FireFlight(uint id, float delay) {
        Cast(id, (uint)AID.HeatBurst, delay, 5, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>()
            .ActivateOnEnter<Fireflight>()
            .ActivateOnEnter<SunCirclet>()
            .ActivateOnEnter<FireflightStackSpread>()
            .ActivateOnEnter<WheelOfFableFlight>()
            .ActivateOnEnter<WheelofFableFlightStackSpread>()
            .ActivateOnEnter<FireOfVictory>()
            .ActivateOnEnter<WitchHunt>()
            .ActivateOnEnter<WitchHuntStack>()
            .ActivateOnEnter<FireFlightFourLongNight>();
    }
}