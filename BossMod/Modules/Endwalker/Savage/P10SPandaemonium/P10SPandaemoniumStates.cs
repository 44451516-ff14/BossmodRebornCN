namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class P10SPandaemoniumStates : StateMachineBuilder
{
    public P10SPandaemoniumStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<Border>(); // keep active throught the fight, since it overlaps with subsequent mechanics
    }

    private void SinglePhase(uint id)
    {
        Ultima(id, 8.4f);
        SoulGrasp(id + 0x10000u, 4.2f);
        DividingWings1(id + 0x20000u, 2.8f);
        WickedStepPandaemoniacPillars(id + 0x30000u, 2.7f);
        Silkspit(id + 0x40000u, 2.0f);
        DaemoniacBondsPandaemoniacMeltdownTouchdown(id + 0x50000u, 1.4f);
        Ultima(id + 0x60000u, 5.9f);
        SoulGrasp(id + 0x70000u, 4.2f);
        DaemoniacBondsPandaemoniacTurrets(id + 0x80000u, 7.5f);
        Ultima(id + 0x90000u, 6.6f);
        SoulGrasp(id + 0xA0000u, 4.2f);
        WickedStepSilkspit(id + 0xB0000u, 7.5f);
        DividingWings2(id + 0xC0000u, 1.4f);
        SoulGrasp(id + 0xD0000u, 3.6f);
        DividingWings3(id + 0xE0000u, 11.6f);
        Ultima(id + 0xF0000u, 13.2f);
        SoulGrasp(id + 0x100000u, 5.2f);
        WickedStepEntanglingWeb(id + 0x110000u, 10.7f);
        PartedPlumesPandaemoniacRay(id + 0x120000u, 6.7f);
        Silkspit(id + 0x130000u, 3.2f);
        PandaemoniacPillarsTurrets(id + 0x140000u, 2.4f, AID.PandaemoniacPillars);
        CirclesHolyPillars(id + 0x150000u, 1.5f);
        PandaemoniacMeltdown(id + 0x160000u, 2.0f);
        HarrowingHell(id + 0x170000u, 17.1f, true);
    }

    private void Ultima(uint id, float delay)
    {
        Cast(id, AID.Ultima, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SoulGrasp(uint id, float delay)
    {
        CastStart(id, AID.SoulGrasp, delay)
            .ActivateOnEnter<SoulGrasp>(); // icon appears right before cast start
        CastEnd(id + 1u, 5f);
        ComponentCondition<SoulGrasp>(id + 0x10u, 0.8f, static comp => comp.NumCasts >= 1, "Tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<SoulGrasp>(id + 0x11u, 1.6f, static comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<SoulGrasp>(id + 0x12u, 1.6f, static comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<SoulGrasp>(id + 0x13u, 1.6f, static comp => comp.NumCasts >= 4, "Tankbuster hit 4")
            .DeactivateOnExit<SoulGrasp>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State CirclesHoly(uint id, float delay)
    {
        return CastMulti(id, [AID.PandaemonsHoly, AID.CirclesOfPandaemonium], delay, 4f, "In/out")
            .ActivateOnEnter<PandaemonsHoly>()
            .ActivateOnEnter<CirclesOfPandaemonium>()
            .DeactivateOnExit<PandaemonsHoly>()
            .DeactivateOnExit<CirclesOfPandaemonium>();
    }

    private void CirclesHolyPillars(uint id, float delay)
    {
        CirclesHoly(id, delay)
            .ActivateOnEnter<Imprisonment>() // note: these start ~1.1s later, theoretically we can predict that by looking at previous bury casts...
            .ActivateOnEnter<Cannonspawn>()
            .ActivateOnEnter<PealOfDamnation>();
        ComponentCondition<Imprisonment>(id + 0x10u, 0.7f, static comp => comp.NumCasts > 0, "Pillars")
            .DeactivateOnExit<PealOfDamnation>() // this ends ~0.5s earlier, but who cares
            .DeactivateOnExit<Imprisonment>()
            .DeactivateOnExit<Cannonspawn>();
    }

    private void WickedStep(uint id, float delay)
    {
        Cast(id, AID.WickedStep, delay, 6f)
            .ActivateOnEnter<WickedStep>();
        ComponentCondition<WickedStep>(id + 0x10u, 1.1f, static comp => comp.NumCasts >= 1, "Tower L")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<WickedStep>(id + 0x20u, 2.4f, static comp => comp.NumCasts >= 2, "Tower R")
            .DeactivateOnExit<WickedStep>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void EntanglingWeb(uint id, float delay)
    {
        Cast(id, AID.EntanglingWeb, delay, 3f);
        ComponentCondition<EntanglingWebAOE>(id + 0x10u, 8f, static comp => comp.Casters.Count > 0, "Web bait")
            .ActivateOnEnter<EntanglingWebHints>()
            .ActivateOnEnter<EntanglingWebAOE>()
            .DeactivateOnExit<EntanglingWebHints>();
        ComponentCondition<EntanglingWebAOE>(id + 0x20u, 3f, static comp => comp.NumCasts > 0, "Web resolve")
            .DeactivateOnExit<EntanglingWebAOE>();
    }

    private void PandaemoniacPillarsTurrets(uint id, float delay, AID aid)
    {
        Cast(id, aid, delay, 5f)
            .ActivateOnEnter<PandaemoniacPillars>();
        ComponentCondition<PandaemoniacPillars>(id + 0x10u, 1.2f, static comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<PandaemoniacPillars>();
    }

    private State PandaemoniacMeltdown(uint id, float delay)
    {
        CastStart(id, AID.PandaemoniacMeltdown, delay)
            .ActivateOnEnter<PandaemoniacMeltdown>(); // icons appear right before cast start
        CastEnd(id + 1u, 5f);
        return ComponentCondition<PandaemoniacMeltdown>(id + 2u, 0.6f, static comp => comp.NumCasts > 0, "Line stack/spread")
            .DeactivateOnExit<PandaemoniacMeltdown>();
    }

    private void Touchdown(uint id, float delay)
    {
        Cast(id, AID.Touchdown, delay, 8f)
            .ActivateOnEnter<Touchdown>();
        ComponentCondition<Touchdown>(id + 2u, 1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<Touchdown>();
    }

    private void Silkspit(uint id, float delay)
    {
        Cast(id, AID.Silkspit, delay, 3f);
        ComponentCondition<Silkspit>(id + 0x10u, 1.9f, static comp => comp.Active)
            .ActivateOnEnter<Silkspit>();
        ComponentCondition<Silkspit>(id + 0x20u, 8.1f, static comp => !comp.Active, "Silkspit")
            .DeactivateOnExit<Silkspit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DaemoniacBondsCast(uint id, float delay)
    {
        Cast(id, AID.DaemoniacBonds, delay, 3f)
            .ActivateOnEnter<DaemoniacBonds>(); // statuses appear ~0.7s after cast end
    }

    private void DaemoniacBondsResolve(uint id, float delay)
    {
        ComponentCondition<DaemoniacBonds>(id, delay, static comp => comp.NumMechanics >= 1, "Stack/spread");
        ComponentCondition<DaemoniacBonds>(id + 1u, 3f, static comp => comp.NumMechanics >= 2, "Spread/stack")
            .DeactivateOnExit<DaemoniacBonds>();
    }

    private void PandaemoniacRay(uint id, float delay)
    {
        CastMulti(id, [AID.PandaemoniacRayL, AID.PandaemoniacRayR], delay, 5f)
            .ActivateOnEnter<PandaemoniacRay>();
        ComponentCondition<PandaemoniacRay>(id + 2u, 0.2f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .DeactivateOnExit<PandaemoniacRay>();
        ComponentCondition<JadePassage>(id + 0x10u, 3.6f, static comp => comp.NumCasts > 0, "Lines")
            .ActivateOnEnter<JadePassage>()
            .DeactivateOnExit<JadePassage>();
    }

    private void PartedPlumesPandaemoniacRay(uint id, float delay)
    {
        // overlaps with ray & circle/holy
        Cast(id, AID.PartedPlumes, delay, 3f);
        CastStartMulti(id + 0x10u, [AID.PandaemoniacRayL, AID.PandaemoniacRayR], 7.3f)
            .ActivateOnEnter<PartedPlumes>() // first aoe cast start 3.8s after previous cast end, individual aoes are 0.3s apart
            .ActivateOnEnter<PartedPlumesVoidzone>();
        ComponentCondition<PartedPlumes>(id + 0x20u, 0.5f, static comp => comp.NumCasts > 0, "Plumes start")
            .ActivateOnEnter<PandaemoniacRay>();
        ComponentCondition<PartedPlumes>(id + 0x30u, 2.4f, static comp => comp.Casters.Count == 0, "Plumes end")
            .DeactivateOnExit<PartedPlumes>()
            .DeactivateOnExit<PartedPlumesVoidzone>();
        CastEnd(id + 0x40u, 2.1f);
        ComponentCondition<PandaemoniacRay>(id + 0x41u, 0.2f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .DeactivateOnExit<PandaemoniacRay>();
        CastStartMulti(id + 0x50u, [AID.PandaemonsHoly, AID.CirclesOfPandaemonium], 3)
            .ActivateOnEnter<JadePassage>();
        ComponentCondition<JadePassage>(id + 0x60u, 0.6f, static comp => comp.NumCasts > 0, "Lines")
            .ActivateOnEnter<PandaemonsHoly>()
            .ActivateOnEnter<CirclesOfPandaemonium>()
            .DeactivateOnExit<JadePassage>();
        CastEnd(id + 0x70u, 3.3f, "In/out")
            .DeactivateOnExit<PandaemonsHoly>()
            .DeactivateOnExit<CirclesOfPandaemonium>();
    }

    private void WickedStepEntanglingWeb(uint id, float delay)
    {
        WickedStep(id, delay);
        EntanglingWeb(id + 0x1000u, 3.6f);
    }

    private void WickedStepPandaemoniacPillars(uint id, float delay)
    {
        // these happen in quick succession, makes sense to group them together
        WickedStep(id, delay);
        EntanglingWeb(id + 0x1000u, 4.7f);
        PandaemoniacPillarsTurrets(id + 0x2000u, 1.4f, AID.PandaemoniacPillars);
        CirclesHolyPillars(id + 0x3000u, 1.5f);
    }

    private void WickedStepSilkspit(uint id, float delay)
    {
        WickedStep(id, delay);

        // overlap of entangling web, silkspit and daemoniac bonds mechanics
        Cast(id + 0x1000u, AID.EntanglingWeb, 4.7f, 3);
        CastStart(id + 0x1010u, AID.Silkspit, 7.3f)
            .ActivateOnEnter<EntanglingWebHints>()
            .ActivateOnEnter<EntanglingWebAOE>();
        ComponentCondition<EntanglingWebAOE>(id + 0x1011u, 0.7f, static comp => comp.Casters.Count > 0, "Web bait")
            .DeactivateOnExit<EntanglingWebHints>();
        CastEnd(id + 0x1012u, 2.3f);
        ComponentCondition<EntanglingWebAOE>(id + 0x1013u, 0.7f, static comp => comp.NumCasts > 0, "Web resolve")
            .DeactivateOnExit<EntanglingWebAOE>();
        ComponentCondition<Silkspit>(id + 0x1020u, 1.2f, static comp => comp.Active)
            .ActivateOnEnter<Silkspit>();
        DaemoniacBondsCast(id + 0x1030u, 4.2f);
        ComponentCondition<Silkspit>(id + 0x1040u, 0.8f, static comp => !comp.Active, "Silkspit")
            .DeactivateOnExit<Silkspit>()
            .SetHint(StateMachine.StateHint.Raidwide);

        PandaemoniacPillarsTurrets(id + 0x2000u, 2.3f, AID.PandaemoniacPillars);
        CirclesHolyPillars(id + 0x3000u, 1.5f);

        // overlap of pandaemoniac ray and daemoniac bonds resolve
        CastStartMulti(id + 0x4000u, [AID.PandaemoniacRayL, AID.PandaemoniacRayR], 2.0f)
            .ExecOnEnter<DaemoniacBonds>(static comp => comp.Show());
        ComponentCondition<DaemoniacBonds>(id + 0x4010u, 4.4f, static comp => comp.NumMechanics >= 1, "Stack/spread")
            .ActivateOnEnter<PandaemoniacRay>();
        CastEnd(id + 0x4020u, 0.6f);
        ComponentCondition<PandaemoniacRay>(id + 0x4030u, 0.2f, static comp => comp.NumCasts > 0, "Half-room cleave")
            .DeactivateOnExit<PandaemoniacRay>();
        ComponentCondition<DaemoniacBonds>(id + 0x4040u, 3.2f, static comp => comp.NumMechanics >= 2, "Spread/stack")
            .ActivateOnEnter<JadePassage>()
            .DeactivateOnExit<DaemoniacBonds>();
        ComponentCondition<JadePassage>(id + 0x4050u, 0.4f, static comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<JadePassage>();
    }

    private State HarrowingHell(uint id, float delay, bool isEnrage)
    {
        Cast(id, AID.HarrowingHell, delay, 5f);
        ComponentCondition<HarrowingHell>(id + 0x11u, 1.0f, static comp => comp.NumCasts >= 1, "Hell start")
            .ActivateOnEnter<HarrowingHell>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x12u, 1.9f, static comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x13u, 1.7f, static comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x14u, 1.7f, static comp => comp.NumCasts >= 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x15u, 1.7f, static comp => comp.NumCasts >= 5)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x16u, 1.5f, static comp => comp.NumCasts >= 6)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x17u, 1.4f, static comp => comp.NumCasts >= 7)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<HarrowingHell>(id + 0x18u, 1.3f, static comp => comp.NumCasts >= 8)
            .SetHint(StateMachine.StateHint.Raidwide);
        return ComponentCondition<HarrowingHell>(id + 0x20u, 3.9f, static comp => comp.NumCasts >= 9, isEnrage ? "Enrage" : "Hell resolve")
            .DeactivateOnExit<HarrowingHell>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DividingWings1(uint id, float delay)
    {
        Cast(id, AID.DividingWings, delay, 3f);
        Cast(id + 0x10u, AID.SteelWeb, 3.2f, 3f);
        ComponentCondition<DividingWings>(id + 0x20u, 7.7f, static comp => comp.NumCasts > 0, "Tethers")
            .ActivateOnEnter<DividingWings>()
            .ActivateOnEnter<SteelWebStack>()
            .DeactivateOnExit<DividingWings>();
        ComponentCondition<SteelWebStack>(id + 0x30u, 0.3f, static comp => !comp.Active, "Web")
            .ActivateOnEnter<SteelWebTethers>()
            .DeactivateOnExit<SteelWebStack>();
        CirclesHoly(id + 0x1000u, 6.1f)
            .DeactivateOnExit<SteelWebTethers>();
    }

    private void DividingWings2(uint id, float delay)
    {
        Cast(id, AID.DividingWings, delay, 3f)
            .ActivateOnEnter<DividingWings>();
        Cast(id + 0x10u, AID.SteelWeb, 3.2f, 3f)
            .ActivateOnEnter<SteelWebStack>();
        Cast(id + 0x20u, AID.Touchdown, 4.1f, 8f)
            .ActivateOnEnter<Touchdown>();
        ComponentCondition<DividingWings>(id + 0x30u, 0.6f, static comp => comp.NumCasts > 0, "Tethers")
            .DeactivateOnExit<DividingWings>();
        ComponentCondition<SteelWebStack>(id + 0x40u, 0.3f, static comp => !comp.Active, "Web")
            .ActivateOnEnter<SteelWebTethers>()
            .DeactivateOnExit<SteelWebStack>();
        ComponentCondition<Touchdown>(id + 0x50u, 0.1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<Touchdown>();
        PandaemoniacMeltdown(id + 0x1000u, 12.5f)
            .DeactivateOnExit<SteelWebTethers>();
    }

    private void DividingWings3(uint id, float delay)
    {
        Cast(id, AID.DividingWings, delay, 3f);
        Cast(id + 0x10u, AID.PandaemoniacWeb, 3.2f, 3f);

        CastStart(id + 0x100u, AID.PandaemonsHoly, 4.1f)
            .ActivateOnEnter<DividingWings>()
            .ActivateOnEnter<SteelWebStack>()
            .ActivateOnEnter<EntanglingWebHints>()
            .ActivateOnEnter<EntanglingWebAOE>();
        ComponentCondition<DividingWings>(id + 0x110u, 3.5f, static comp => comp.NumCasts > 0, "Tethers")
            .ActivateOnEnter<PandaemonsHoly>()
            .DeactivateOnExit<DividingWings>();
        ComponentCondition<SteelWebStack>(id + 0x120u, 0.4f, static comp => !comp.Active, "Web stack & bait 1") // steel web resolve + entangling web 1 baits
            .ActivateOnEnter<SteelWebTethers>()
            .DeactivateOnExit<SteelWebStack>();
        CastEnd(id + 0x130u, 0.1f)
            .DeactivateOnExit<PandaemonsHoly>();
        ComponentCondition<EntanglingWebAOE>(id + 0x140u, 2.8f, static comp => comp.NumCasts > 0);

        CastStart(id + 0x200u, AID.DaemoniacBonds, 1.3f);
        ComponentCondition<EntanglingWebAOE>(id + 0x210u, 1.7f, static comp => comp.Casters.Count > 0, "Web bait 2")
            .DeactivateOnExit<EntanglingWebHints>();
        CastEnd(id + 0x220u, 1.3f);
        ComponentCondition<EntanglingWebAOE>(id + 0x230u, 1.7f, static comp => comp.Casters.Count == 0)
            .ActivateOnEnter<DaemoniacBonds>()
            .DeactivateOnExit<EntanglingWebAOE>();

        HarrowingHell(id + 0x300u, 4.6f, false)
            .DeactivateOnExit<SteelWebTethers>() // TODO: this can be deactivated much earlier?
            .ExecOnExit<DaemoniacBonds>(static comp => comp.Show());

        DaemoniacBondsResolve(id + 0x400u, 5.4f);
    }

    private void DaemoniacBondsPandaemoniacMeltdownTouchdown(uint id, float delay)
    {
        DaemoniacBondsCast(id, delay);
        PandaemoniacMeltdown(id + 0x100u, 4.2f)
            .ExecOnExit<DaemoniacBonds>(static comp => comp.Show());
        Touchdown(id + 0x200u, 3.6f);
        DaemoniacBondsResolve(id + 0x300u, 0.5f);
    }

    private void DaemoniacBondsPandaemoniacTurrets(uint id, float delay)
    {
        DaemoniacBondsCast(id, delay);
        PandaemoniacPillarsTurrets(id + 0x100u, 4.2f, AID.PandaemoniacTurrets);
        ComponentCondition<Turrets>(id + 0x200u, 10.7f, static comp => comp.NumCasts > 0, "Knockback 1")
            .ActivateOnEnter<Turrets>();
        ComponentCondition<Turrets>(id + 0x201u, 4.5f, static comp => comp.NumCasts > 2, "Knockback 2");
        ComponentCondition<Turrets>(id + 0x202u, 4.5f, static comp => comp.NumCasts > 4, "Knockback 3");
        ComponentCondition<Turrets>(id + 0x203u, 4.5f, static comp => comp.NumCasts > 6, "Knockback 4")
            .DeactivateOnExit<Turrets>()
            .ExecOnExit<DaemoniacBonds>(static comp => comp.Show());
        DaemoniacBondsResolve(id + 0x300u, 4.3f);
        PandaemoniacRay(id + 0x400u, 1.8f);
    }
}
