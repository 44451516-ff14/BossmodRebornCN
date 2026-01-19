namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class Ex7DoomtrainStates : StateMachineBuilder
{
    public Ex7DoomtrainStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .Raw.Update = () => Module.Enemies((uint)OID.Aether).Count != 0;
    }

    private void SinglePhase(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000u, 2.2f);
        Car3p1(id + 0x20000u, 5.8f);
        Intermission(id + 0x30000u, 10f);
        // Car3p2(id + 0x40000u, 2.2f);
        // Car4(id + 0x50000u, 2.2f);
        // Car5(id + 0x60000u, 2.2f);
        // Car6(id + 0x70000u, 2.2f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void Car1(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack")
            .ActivateOnEnter<DeadMansOverdraught>();
        ComponentCondition<DeadMansExpress>(id + 0x10u, 8.1f, static comp => comp.NumCasts != 0, "Knockback")
            .ActivateOnEnter<PlasmaBeam>()
            .ActivateOnEnter<DeadMansExpress>()
            .DeactivateOnExit<DeadMansExpress>()
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.1d))
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<PlasmaBeam>(id + 0x20u, 2f, static comp => comp.NumCasts != 0, "Line AOEs")
            .DeactivateOnExit<PlasmaBeam>();
        ComponentCondition<DeadMansOverdraught>(id + 0x30u, 3.1f, static comp => comp.Counter == 1u, "Spread/stack resolves");
        CastMulti(id + 0x40u, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack");
        ComponentCondition<DeadMansWindpipe>(id + 0x50u, 8.1f, static comp => comp.NumCasts != 0, "Pull")
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ExecOnEnter<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.1d))
            .ActivateOnEnter<DeadMansWindpipe>()
            .DeactivateOnExit<DeadMansWindpipe>()
            .SetHint(StateMachine.StateHint.Knockback);
        ComponentCondition<DeadMansBlastpipe>(id + 0x60u, 2f, static comp => comp.NumCasts != 0, "Rect AOE")
            .DeactivateOnExit<DeadMansBlastpipe>();
        ComponentCondition<DeadMansOverdraught>(id + 0x70u, 2.1f, static comp => comp.Counter == 2u, "Spread/stack resolves")
            .DeactivateOnExit<DeadMansOverdraught>();
        ComponentCondition<UnlimitedExpress>(id + 0x80u, 7.8f, static comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnlimitedExpress>()
            .DeactivateOnExit<UnlimitedExpress>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x90u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Targetable(id + 0xA0u, true, 5.8f, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void Car2(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], delay, 4f, "Select spread/stack 1")
            .ActivateOnEnter<DeadMansOverdraught>();
        ComponentCondition<ElectrayLong>(id + 0x10u, 14.3f, static comp => comp.NumCasts != 0, "Line AOEs 1")
            .ActivateOnEnter<ElectrayShort>()
            .ActivateOnEnter<ElectrayMedium>()
            .ActivateOnEnter<ElectrayLong>()
            .ActivateOnEnter<PlasmaBeam>()
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansExpress>()
            .ExecOnExit<ElectrayLong>(static comp => comp.NumCasts = 0)
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.9d));
        ComponentCondition<DeadMansOverdraught>(id + 0x20u, 5.9f, static comp => comp.Counter == 1u, "Spread/stack resolves 1");
        CastMulti(id + 0x40u, [(uint)AID.DeadMansOverdraughtSpread, (uint)AID.DeadMansOverdraughtStack], 2f, 4f, "Select spread/stack 2")
            .ActivateOnExit<LightningBurst>();
        ComponentCondition<ElectrayLong>(id + 0x30u, 4.6f, static comp => comp.NumCasts != 0, "Line AOEs 2")
            .ExecOnExit<ElectrayLong>(static comp => comp.NumCasts = 0);
        ComponentCondition<LightningBurst>(id + 040u, 2.6f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();
        ComponentCondition<ElectrayLong>(id + 0x50u, 8.7f, static comp => comp.NumCasts != 0, "Line AOEs 3")
            .DeactivateOnExit<ElectrayShort>()
            .DeactivateOnExit<ElectrayMedium>()
            .DeactivateOnExit<ElectrayLong>()
            .ExecOnExit<DeadMansOverdraught>(static comp => comp.AddStackSpread(5.9d));
        ComponentCondition<DeadMansOverdraught>(id + 0x60u, 5.9f, static comp => comp.Counter == 2u, "Spread/stack resolves 2")
            .DeactivateOnExit<DeadMansBlastpipe>()
            .DeactivateOnExit<DeadMansWindpipe>()
            .DeactivateOnExit<DeadMansExpress>()
            .DeactivateOnExit<PlasmaBeam>()
            .DeactivateOnExit<DeadMansOverdraught>();
        ComponentCondition<UnlimitedExpress>(id + 0x70u, 7.8f, static comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<UnlimitedExpress>()
            .DeactivateOnExit<UnlimitedExpress>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x90u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Car3p1(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss targetable")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<LightningBurst>(id + 0x10u, 2.6f, static comp => comp.NumCasts != 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();
        Cast(id + 0x10u, (uint)AID.RunawayTrainVisual1, 8.5f, 5f, "Runaway Train Phase Transition");
        Targetable(id + 0x20u, false, 0.2f, "Boss untargetable")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Intermission(uint id, float delay)
    {

        var Aether = Module.Enemies((uint)OID.Aether);
        // If I'm reading this correctly, this blocks the state until Aether is dead, and then we can transition back once Doomtrain is targetable?
        Condition(id + 0x10u, 120, () => Aether.Any(e => e.IsDead), "Aether down", 10000, 1) // note that time is arbitrary (What?)
            .ActivateOnEnter<AetherocharAetherosote>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnExit<RunawayTrain>()
            .DeactivateOnExit<AetherocharAetherosote>()
            .DeactivateOnExit<AetherialRay>();
        Targetable(id + 0x20u, true, 5.2f, "Intermission end")
            .DeactivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }
}
