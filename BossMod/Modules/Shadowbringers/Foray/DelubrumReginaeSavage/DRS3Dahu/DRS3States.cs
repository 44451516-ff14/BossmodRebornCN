namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

sealed class DRS3DahuStates : StateMachineBuilder
{
    public DRS3DahuStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ReverberatingRoarHotChargeFirebreathe(id, 10.3f);
        HeadDownSpitFlameShockwaveFeralHowl(id + 0x10000u, 14.5f);
        FirebreatheRotating(id + 0x20000u, 1.6f);
        CrownedMarchosias(id + 0x30000u, 7.4f);
        ReverberatingRoarHeadDownShockwaveSpitFlameHystericAssault(id + 0x40000u, 37f);
        FirebreatheRotating(id + 0x50000u, 6.1f, true);
        // TODO: shockwave? -> hot charge + firebreathe -> ...
        SimpleState(id + 0xFF0000u, 100f, "???");
    }

    private void Shockwave(uint id, float delay)
    {
        CastMulti(id, [AID.LeftSidedShockwaveFirst, AID.RightSidedShockwaveFirst], delay, 3f, "Shockwave 1")
            .ActivateOnEnter<Shockwave>();
        CastMulti(id + 0x10u, [AID.LeftSidedShockwaveSecond, AID.RightSidedShockwaveSecond], 1.6f, 1f, "Shockwave 2")
            .DeactivateOnExit<Shockwave>();
    }

    private State SpitFlame(uint id, float delay)
    {
        CastStart(id, AID.SpitFlame, delay)
            .ActivateOnEnter<SpitFlame>(); // first icon appears right before cast start
        CastEnd(id + 1u, 8f);
        return ComponentCondition<SpitFlame>(id + 0x10u, 3.7f, static comp => !comp.Active, "Spits resolve")
            .DeactivateOnExit<SpitFlame>();
    }

    private void ReverberatingRoarHotChargeFirebreathe(uint id, float delay)
    {
        ComponentCondition<FallingRock>(id, delay, static comp => comp.Casters.Count > 0, "Rocks 1 bait")
            .ActivateOnEnter<FallingRock>();
        Cast(id + 0x10u, AID.HotCharge, 9.5f, 3f, "Charge 1") // note: large variance
            .ActivateOnEnter<HotCharge>()
            .DeactivateOnExit<HotCharge>();
        Cast(id + 0x20u, AID.HotCharge, 1.8f, 3f, "Charge 2")
            .ActivateOnEnter<HotCharge>()
            .DeactivateOnExit<HotCharge>()
            .DeactivateOnExit<FallingRock>(); // last rock ends ~1.1s before cast start
        Cast(id + 0x30u, AID.Firebreathe, 1.8f, 5f, "Cone")
            .ActivateOnEnter<Firebreathe>()
            .DeactivateOnExit<Firebreathe>();
    }

    private void HeadDownSpitFlameShockwaveFeralHowl(uint id, float delay)
    {
        ComponentCondition<HeadDown>(id, delay, static comp => comp.Casters.Count > 0, "Add charges begin")
            .ActivateOnEnter<HeadDown>();
        SpitFlame(id + 0x100u, 4.8f);
        Shockwave(id + 0x200u, 3.2f);
        ComponentCondition<HeadDown>(id + 0x300u, 1f, static comp => comp.Casters.Count == 0, "Add charges resolve", 5) // if one of the spit flame target dies, shockwave happens a bit earlier
            .DeactivateOnExit<HeadDown>();

        Cast(id + 0x400u, AID.FeralHowl, 1.6f, 5f)
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<FeralHowl>();
        ComponentCondition<FeralHowl>(id + 0x410u, 2.1f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FeralHowl>();
        ComponentCondition<HuntersClaw>(id + 0x420u, 1.4f, static comp => comp.NumCasts > 0, "Add aoes")
            .DeactivateOnExit<HuntersClaw>();
    }

    private void FirebreatheRotating(uint id, float delay, bool withHeadDown = false)
    {
        CastStart(id, AID.FirebreatheRotating, delay)
            .ActivateOnEnter<FirebreatheRotating>(); // icon appears just before cast start
        CastEnd(id + 1u, 5f)
            .ActivateOnEnter<HeadDown>(withHeadDown);
        ComponentCondition<FirebreatheRotating>(id + 0x10u, 0.7f, static comp => comp.NumCasts > 0, "Cone 1");
        ComponentCondition<FirebreatheRotating>(id + 0x11u, 2f, static comp => comp.NumCasts > 1);
        ComponentCondition<FirebreatheRotating>(id + 0x12u, 2f, static comp => comp.NumCasts > 2);
        ComponentCondition<FirebreatheRotating>(id + 0x13u, 2f, static comp => comp.NumCasts > 3);
        ComponentCondition<FirebreatheRotating>(id + 0x14u, 2f, static comp => comp.NumCasts > 4, "Cone 5")
            .DeactivateOnExit<FirebreatheRotating>()
            .DeactivateOnExit<HeadDown>(withHeadDown);
    }

    private void CrownedMarchosias(uint id, float delay)
    {
        Condition(id, delay, () =>
        {
            var enemies = Module.Enemies((uint)OID.CrownedMarchosias);
            var count = enemies.Count;
            for (var i = 0; i < count; ++i)
            {
                if (enemies[i].IsTargetable)
                {
                    return true;
                }
            }
            return false;
        }, "Adds appear");
        // +5.2s: second set
        // +10.0s: third set, first set gets damage up
        // +14.1s: first set gets second stack of damage up
        // and so on...
    }

    private void ReverberatingRoarHeadDownShockwaveSpitFlameHystericAssault(uint id, float delay)
    {
        ComponentCondition<FallingRock>(id, delay, static comp => comp.Casters.Count > 0, "Rocks 1 bait")
            .ActivateOnEnter<FallingRock>();
        ComponentCondition<HeadDown>(id + 1u, 0.2f, static comp => comp.Casters.Count > 0, "Add charges begin")
            .ActivateOnEnter<HeadDown>();
        Shockwave(id + 0x100u, 3.9f);
        SpitFlame(id + 0x200u, 2.5f)
            .DeactivateOnExit<FallingRock>() // last rocks end ~1.1s before cast start
            .DeactivateOnExit<HeadDown>(); // last charges end ~2.5s after cast start

        ComponentCondition<Burn>(id + 0x300u, 1.0f, static comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Burn>();

        Cast(id + 0x400u, AID.HystericAssault, 0.1f, 5f)
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<HystericAssault>();
        ComponentCondition<HystericAssault>(id + 0x310u, 0.9f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<HystericAssault>();
        ComponentCondition<Burn>(id + 0x320u, 2.2f, static comp => comp.NumCasts > 0, "Flares")
            .DeactivateOnExit<Burn>();
        ComponentCondition<HuntersClaw>(id + 0x330u, 0.4f, static comp => comp.NumCasts > 0, "Add aoes")
            .DeactivateOnExit<HuntersClaw>();
    }
}
