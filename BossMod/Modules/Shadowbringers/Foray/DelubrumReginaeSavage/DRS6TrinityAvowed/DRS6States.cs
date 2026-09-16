namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

sealed class DRS6TrinityAvowedStates : StateMachineBuilder
{
    public DRS6TrinityAvowedStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<AllegiantArsenal>();
    }

    private void SinglePhase(uint id)
    {
        WrathOfBozja(id, 7.4f, false);
        GloryOfBozja(id + 0x10000u, 3.2f);
        AllegiantArsenalAOE(id + 0x20000u, 6.3f);
        AllegiantArsenalAOE(id + 0x30000u, 5.2f);

        Dictionary<AllegiantArsenal.Order, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [AllegiantArsenal.Order.StaffSwordBow] = ((id >> 24) + 1u, ForkStaffSwordBow),
            [AllegiantArsenal.Order.BowSwordStaff] = ((id >> 24) + 2u, ForkBowSwordStaff),
            [AllegiantArsenal.Order.SwordBowStaff] = ((id >> 24) + 3u, ForkSwordBowStaff),
            [AllegiantArsenal.Order.StaffBowSword] = ((id >> 24) + 4u, ForkStaffBowSword),
            [AllegiantArsenal.Order.SwordStaffBow] = ((id >> 24) + 5u, ForkSwordStaffBow),
            [AllegiantArsenal.Order.BowStaffSword] = ((id >> 24) + 6u, ForkBowStaffSword)
        };
        ComponentConditionFork<AllegiantArsenal, AllegiantArsenal.Order>(id + 0x40000u, 0f, _ => true, static comp => comp.Mechanics, dispatch);
    }

    private void ForkStaffSwordBow(uint id)
    {
        // TODO: no idea about timings here
        Staff1(id, 8f);
        Sword1(id + 0x100000u, 8f);
        Bow1(id + 0x200000u, 8f);
        Staff2(id + 0x300000u, 8f);
        Sword2(id + 0x400000u, 8f);
        Bow2(id + 0x500000u, 8f);
        Enrage(id + 0x600000u, 16f);
    }

    private void ForkBowSwordStaff(uint id)
    {
        Bow1(id, 6.0f);
        Sword1(id + 0x100000u, 7.4f);
        Staff1(id + 0x200000u, 7.5f);
        Bow2(id + 0x300000u, 9.6f);
        Sword2(id + 0x400000u, 8f);
        Staff2(id + 0x500000u, 8f);
        Enrage(id + 0x600000u, 16f); // TODO: timing
    }

    private void ForkSwordBowStaff(uint id)
    {
        Sword1(id, 5.3f);
        Bow1(id + 0x100000u, 8.6f);
        Staff1(id + 0x200000u, 8f); // note: very high variance here...
        Sword2(id + 0x300000u, 7.4f);
        Bow2(id + 0x400000u, 8.6f);
        Staff2(id + 0x500000u, 8.4f);
        Enrage(id + 0x600000u, 16f); // TODO: timing
    }

    private void ForkStaffBowSword(uint id)
    {
        Staff1(id, 5.3f);
        Bow1(id + 0x100000u, 8.7f);
        Sword1(id + 0x200000u, 10.4f);
        Staff2(id + 0x300000u, 7.4f);
        Bow2(id + 0x400000u, 9.1f);
        Sword2(id + 0x500000u, 7.9f);
        Enrage(id + 0x600000u, 16f); // TODO: timing
    }

    private void ForkSwordStaffBow(uint id)
    {
        Sword1(id, 5.2f);
        Staff1(id + 0x100000u, 7.6f);
        Bow1(id + 0x200000u, 8.9f);
        Sword2(id + 0x300000u, 7.7f);
        Staff2(id + 0x400000u, 7.6f);
        Bow2(id + 0x500000u, 9.7f);
        Enrage(id + 0x600000u, 15.8f);
    }

    private void ForkBowStaffSword(uint id)
    {
        Bow1(id, 6f); // note: very high variance here...
        Staff1(id + 0x100000u, 7.9f); // note: very high variance here...
        Sword1(id + 0x200000u, 7.5f);
        Bow2(id + 0x300000u, 8.5f);
        Staff2(id + 0x400000u, 8f); // note: very high variance here...
        Sword2(id + 0x500000u, 7.5f);
        Enrage(id + 0x600000u, 20.5f);
    }

    private void Sword1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.HotAndColdSword, 4.5f, 3f); // note: large variance
        // +1.1s: temperature statuses
        Cast(id + 0x10010u, AID.UnwaveringApparition, 6f, 3f);
        Targetable(id + 0x10020u, false, 5.7f, "Disappear"); // note: large variance
        BladeOfEntropy(id + 0x10030u, 0.1f, "Sword 1");
        BladeOfEntropy(id + 0x10040u, 4.0f, "Sword 2"); // note: large variance
        Targetable(id + 0x10050u, true, 3.1f, "Reappear");

        GloryOfBozja(id + 0x20000u, 6.5f);
    }

    private void Bow1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.QuickMarchBow, 3.1f, 3f)
            .ActivateOnEnter<FlamesOfBozja1>()
            .ActivateOnEnter<QuickMarchBow1>(); // debuffs are applied ~1s after cast end
        WrathOfBozja(id + 0x10010u, 3.1f, true);
        Cast(id + 0x10020u, AID.FlamesOfBozja, 3.2f, 3f);
        // +1.1s: flames of bozja aoe cast start
        ComponentCondition<QuickMarch>(id + 0x10030u, 5.7f, static comp => comp.NumActiveForcedMarches > 0, "Forced march start");
        ComponentCondition<FlamesOfBozja>(id + 0x10040u, 4.5f, static comp => comp.NumCasts > 0, "Single safe row")
            .DeactivateOnExit<QuickMarch>();

        Cast(id + 0x20000u, AID.HotAndColdBow, 0.4f, 3f);
        // +1.1s: temperature statuses
        Cast(id + 0x20010u, AID.ShimmeringShot, 5f, 3f);
        ComponentCondition<ShimmeringShot>(id + 0x20020u, 16.2f, static comp => comp.NumCasts > 0, "Arrows hit")
            .ActivateOnEnter<ShimmeringShot1>() // env controls happen ~15.2s before resolve, arrows spawn ~12.8s before resolve
            .DeactivateOnExit<ShimmeringShot>();
        ComponentCondition<FlamesOfBozja>(id + 0x20030u, 2.2f, static comp => comp.AOE.Length == 0, "Bow 1 resolve")
            .DeactivateOnExit<FlamesOfBozja>();

        GloryOfBozja(id + 0x30000u, 5.3f); // TODO: this seems to have slightly different timings depending on forks...
    }

    private void Staff1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.HotAndColdStaff, 3.1f, 3f);
        // +1.1s: temperature statuses
        Cast(id + 0x10010u, AID.QuickMarchStaff, 4.1f, 3f);
        // +1.0s: march debuffs (but we start showing hints only after proximity)
        Cast(id + 0x10020u, AID.FreedomOfBozja, 3.2f, 3f);

        // +1.3s: orb spawn
        // +2.2s: impact visual cast start
        ComponentCondition<ElementalImpact>(id + 0x10030u, 7.5f, static comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<ElementalImpact>()
            .DeactivateOnExit<ElementalImpact>();
        // +2.0s: blast cast starts

        ComponentCondition<QuickMarch>(id + 0x20000u, 6.5f, static comp => comp.NumActiveForcedMarches > 0, "Forced march start")
            .ActivateOnEnter<FreedomOfBozja1>()
            .ActivateOnEnter<QuickMarchStaff1>();
        ComponentCondition<FreedomOfBozja>(id + 0x20010u, 3.3f, static comp => comp.NumCasts > 0, "Orbs hit")
            .DeactivateOnExit<QuickMarch>()
            .DeactivateOnExit<FreedomOfBozja>();

        GloryOfBozja(id + 0x30000u, 7.9f); // TODO: this seems to have slightly different timings depending on forks...
    }

    private void Sword2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.HotAndColdSword, 4.4f, 3f); // note: large variance
        Cast(id + 0x10010u, AID.ElementalBrandSword, 4.1f, 3f);
        Cast(id + 0x10020u, AID.UnwaveringApparition, 3.2f, 3f);
        Targetable(id + 0x10030u, false, 6.0f, "Disappear");
        BladeOfEntropy(id + 0x10040u, 0.1f, "Sword 1");
        BladeOfEntropy(id + 0x10050u, 4.0f, "Sword 2");
        Targetable(id + 0x10060u, true, 3.1f, "Reappear");

        GloryOfBozja(id + 0x20000u, 6.5f);
    }

    private void Bow2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.UnseenEyeBow, 3.1f, 3f);
        Cast(id + 0x10010u, AID.FlamesOfBozja, 3.1f, 3f)
            .ActivateOnEnter<GleamingArrow>(); // PATE events happen together with cast-start, actual casts start ~2.1s later - if we want to rely on former, need to activate earlier
        ComponentCondition<GleamingArrow>(id + 0x10020u, 5.1f, static comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<GleamingArrow>();
        ComponentCondition<FlamesOfBozja>(id + 0x10030u, 5f, static comp => comp.NumCasts > 0, "Single safe row")
            .ActivateOnEnter<FlamesOfBozja2>(); // activate late, since criss-cross have to be resolved first

        Cast(id + 0x20000u, AID.HotAndColdBow, 0f, 3f); // note: very high variance, sometimes even starts slightly beofre flames of bozja end...
        Cast(id + 0x20010u, AID.ElementalBrandBow, 4.2f, 3f);
        Cast(id + 0x20020u, AID.QuickMarchBow, 3.2f, 3f);
        Cast(id + 0x20030u, AID.ShimmeringShot, 3.9f, 3f);
        ComponentCondition<QuickMarch>(id + 0x20040u, 13.2f, static comp => comp.NumActiveForcedMarches > 0, "Forced march start")
            .ActivateOnEnter<ShimmeringShot2>() // env controls happen ~0.9s after cast end, arrows spawn ~3.2s after cast end
            .ActivateOnEnter<QuickMarchBow2>();
        ComponentCondition<ShimmeringShot>(id + 0x20050u, 4f, static comp => comp.NumCasts > 0, "Arrows hit")
            .DeactivateOnExit<QuickMarch>()
            .DeactivateOnExit<ShimmeringShot>();
        ComponentCondition<FlamesOfBozja>(id + 0x20060u, 2.2f, static comp => comp.AOE.Length == 0, "Bow 2 resolve")
            .DeactivateOnExit<FlamesOfBozja>();

        GloryOfBozja(id + 0x30000u, 5.3f);
        WrathOfBozja(id + 0x40000u, 3.2f, false);
    }

    private void Staff2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000u, AID.HotAndColdStaff, 3.1f, 3f);
        Cast(id + 0x10010u, AID.ElementalBrandStaff, 4.1f, 3f);
        Cast(id + 0x10020u, AID.FreedomOfBozja, 3.2f, 3f)
            .ActivateOnEnter<ElementalImpact>();
        Cast(id + 0x10030u, AID.UnseenEyeStaff, 3.1f, 3f);
        ComponentCondition<ElementalImpact>(id + 0x10040u, 1.0f, static comp => comp.NumCasts > 0, "Proximity", 10f)
            .DeactivateOnExit<ElementalImpact>();

        ComponentCondition<FreedomOfBozja>(id + 0x20000u, 10f, static comp => comp.NumCasts > 0, "Orbs + criss-cross", 10f)
            .ActivateOnEnter<FreedomOfBozja2>()
            .ActivateOnEnter<GleamingArrow>()
            .DeactivateOnExit<FreedomOfBozja>()
            .DeactivateOnExit<GleamingArrow>();

        GloryOfBozja(id + 0x30000u, 8f);
    }

    private void WrathOfBozja(uint id, float delay, bool bow)
    {
        Cast(id, bow ? AID.WrathOfBozjaBow : AID.WrathOfBozja, delay, 5f, "Tankbuster")
            .ActivateOnEnter<WrathOfBozja>(!bow)
            .ActivateOnEnter<WrathOfBozjaBow>(bow)
            .DeactivateOnExit<WrathOfBozja>(!bow)
            .DeactivateOnExit<WrathOfBozjaBow>(bow)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void GloryOfBozja(uint id, float delay)
    {
        Cast(id, AID.GloryOfBozja, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        // note: second half of raid is hit 0.6s later, fuck that...
    }

    private void AllegiantArsenalAOE(uint id, float delay)
    {
        CastMulti(id, [AID.AllegiantArsenalSword, AID.AllegiantArsenalBow, AID.AllegiantArsenalStaff], delay, 3f);
        ComponentCondition<AllegiantArsenal>(id + 0x10u, 5.2f, static comp => !comp.Active, "Weapon aoe");
    }

    private void BladeOfEntropy(uint id, float delay, string name)
    {
        CastMulti(id, [AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21, AID.BladeOfEntropyAC11, AID.BladeOfEntropyAC21, AID.BladeOfEntropyAH11, AID.BladeOfEntropyAH21], delay, 10f, name)
            .ActivateOnEnter<BladeOfEntropy>()
            .DeactivateOnExit<BladeOfEntropy>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.Enrage, delay, 12f, "Enrage"); // boss becomes untargetable at the end of the cast
    }
}
