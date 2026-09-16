namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class P9SKokytosStates : StateMachineBuilder
{
    public P9SKokytosStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void SinglePhase(uint id)
    {
        GluttonysAugur(id, 6.5f);

        Ravening(id + 0x100000u, 8.4f, AID.RaveningMage, false, false);
        DualityOfDeath(id + 0x110000u, 10.0f);
        TripleDualspell(id + 0x120000u, 5.2f);

        Ravening(id + 0x200000u, 16.1f, AID.RaveningMartialist, true, false);
        AscendantFist(id + 0x210000u, 7.8f);
        ArchaicRockbreakerCombination(id + 0x220000u, 7.4f);
        GluttonysAugur(id + 0x230000u, 1.0f);
        AscendantFist(id + 0x240000u, 5.8f);

        Ravening(id + 0x300000u, 17.3f, AID.RaveningChimeric, true, false);
        LevinstrikeSummoning(id + 0x310000u, 2.5f);
        GluttonysAugur(id + 0x320000u, 1.0f);

        Ravening(id + 0x400000u, 13.6f, AID.RaveningBeast, false, true);
        Charibdys(id + 0x410000u, 2.5f);

        Ravening(id + 0x500000u, 14.3f, AID.RaveningChimeric, true, false);
        DualityOfDeath(id + 0x510000u, 7.9f);
        ArchaicRockbreakerDualspell(id + 0x520000u, 1.0f);
        GluttonysAugur(id + 0x530000u, 0.3f);
        ChimericSuccession(id + 0x540000u, 6.9f);
        Dualspell(id + 0x550000u, 3.8f, true);

        Ravening(id + 0x600000u, 11.0f, AID.RaveningMage, false, false);
        Dualspell(id + 0x610000u, 2.5f);
        GluttonysAugur(id + 0x620000u, 0.4f);
        Dualspell(id + 0x630000u, 1.6f);
        DualityOfDeath(id + 0x640000u, 1.3f);
        Dualspell(id + 0x650000u, 1.0f);
        GluttonysAugur(id + 0x660000u, 0.3f);

        Ravening(id + 0x700000u, 16.8f, AID.RaveningChimeric, false, false);
        Cast(id + 0x710000u, AID.Disintegration, 2.5f, 10f, "Enrage");
    }

    private void GluttonysAugur(uint id, float delay)
    {
        Cast(id, AID.GluttonysAugur, delay, 5f);
        ComponentCondition<GluttonysAugur>(id + 2u, 0.5f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<GluttonysAugur>()
            .DeactivateOnExit<GluttonysAugur>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Ravening(uint id, float delay, AID aid, bool withUplift, bool shortCast)
    {
        Cast(id, aid, delay, shortCast ? 3f : 4f)
            .ActivateOnEnter<Uplift>(withUplift);
        ComponentCondition<SoulSurge>(id + 2u, shortCast ? 7.8f : 6.6f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<SoulSurge>()
            .DeactivateOnExit<SoulSurge>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DualityOfDeath(uint id, float delay)
    {
        CastStart(id, AID.DualityOfDeath, delay)
            .ActivateOnEnter<DualityOfDeath>(); // icons appear 0.1s before cast start
        CastEnd(id + 1u, 5f);
        ComponentCondition<DualityOfDeath>(id + 2u, 0.8f, static comp => comp.NumCasts >= 1, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DualityOfDeath>(id + 3u, 2.3f, static comp => comp.NumCasts >= 2, "Tankbuster 2")
            .DeactivateOnExit<DualityOfDeath>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State Dualspell(uint id, float delay, bool isTwoMinds = false)
    {
        CastMulti(id, [isTwoMinds ? AID.TwoMindsIceFire : AID.DualspellIceFire, isTwoMinds ? AID.TwoMindsIceLightning : AID.DualspellIceLightning], delay, isTwoMinds ? 7f : 5f)
            .ActivateOnEnter<DualspellFire>()
            .ActivateOnEnter<DualspellLightning>()
            .ActivateOnEnter<DualspellIce>();
        return ComponentCondition<DualspellIce>(id + 0x10u, isTwoMinds ? 1.2f : 7.8f, static comp => comp.NumCasts > 0, "Dualspell")
            .DeactivateOnExit<DualspellFire>()
            .DeactivateOnExit<DualspellLightning>()
            .DeactivateOnExit<DualspellIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TripleDualspell(uint id, float delay)
    {
        Dualspell(id, delay);
        Dualspell(id + 0x1000u, 0.3f);
        Dualspell(id + 0x2000u, 0.3f);
    }

    // TODO: tankswap component
    private void AscendantFist(uint id, float delay)
    {
        Cast(id, AID.AscendantFist, delay, 5f, "Tankbuster swap")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // keeps lines component active
    private void ArchaicRockbreaker(uint id, float delay)
    {
        Cast(id, AID.ArchaicRockbreakerCenter, delay, 5f)
            .ActivateOnEnter<ArchaicRockbreakerCenter>()
            .ActivateOnEnter<ArchaicRockbreakerShockwave>()
            .ActivateOnEnter<ArchaicRockbreakerPairs>()
            .DeactivateOnExit<ArchaicRockbreakerCenter>();
        ComponentCondition<ArchaicRockbreakerShockwave>(id + 0x10u, 1.5f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<ArchaicRockbreakerShockwave>();
        ComponentCondition<ArchaicRockbreakerPairs>(id + 0x20u, 1.3f, static comp => !comp.Active, "Pairs")
            .ActivateOnEnter<ArchaicRockbreakerLine>() // first 8 casts start together with knockback
            .DeactivateOnExit<ArchaicRockbreakerPairs>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ArchaicRockbreakerCombination(uint id, float delay)
    {
        ArchaicRockbreaker(id, delay);

        CastMulti(id + 0x100u, [AID.FrontCombinationOut, AID.FrontCombinationIn, AID.RearCombinationOut, AID.RearCombinationIn], 0.3f, 6f) // note: second set of lines start casting ~4.4s into cast, overlapping with first
            .ActivateOnEnter<ArchaicRockbreakerCombination>();
        ComponentCondition<ArchaicRockbreakerLine>(id + 0x110u, 0.4f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<ArchaicRockbreakerLine>(); // hide second set for a time
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x111u, 0.6f, static comp => comp.NumCasts >= 1, "In/out");
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x120u, 3.1f, static comp => comp.NumCasts >= 2, "Front/back")
            .ActivateOnEnter<ArchaicRockbreakerLine>(); // start showing lines again
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x130u, 2.9f, static comp => comp.NumCasts >= 3, "Out/in")
            .DeactivateOnExit<ArchaicRockbreakerLine>() // lines end ~0.6s before
            .DeactivateOnExit<ArchaicRockbreakerCombination>();

        Cast(id + 0x200u, AID.ArchaicDemolish, 2.4f, 4)
            .ActivateOnEnter<ArchaicDemolish>();
        ComponentCondition<ArchaicDemolish>(id + 0x210u, 1.2f, static comp => !comp.Active, "Party stacks")
            .DeactivateOnExit<ArchaicDemolish>()
            .DeactivateOnExit<Uplift>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ArchaicRockbreakerDualspell(uint id, float delay)
    {
        ArchaicRockbreaker(id, delay);
        Dualspell(id + 0x100u, 2.3f)
            .DeactivateOnExit<ArchaicRockbreakerLine>()
            .DeactivateOnExit<Uplift>();
    }

    private void LevinstrikeSummoning(uint id, float delay)
    {
        Cast(id, AID.LevinstrikeSummoning, delay, 4f)
            .ActivateOnEnter<LevinstrikeSummoningIcemeld>()
            .ActivateOnEnter<LevinstrikeSummoningFiremeld>()
            .ActivateOnEnter<LevinstrikeSummoningShock>();
        Cast(id + 0x10u, AID.ScrambledSuccession, 2.1f, 10f);
        Targetable(id + 0x20u, false, 0.1f, "Disappear");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x30u, 2.5f, static comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x31u, 2.3f, static comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x32u, 0.2f, static comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x33u, 0.8f, static comp => comp.NumTowers >= 1, "Tower 1");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x40u, 2.4f, static comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x41u, 2.3f, static comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x42u, 0.3f, static comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x43u, 0.8f, static comp => comp.NumTowers >= 2, "Tower 2");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x50u, 2.4f, static comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x51u, 2.3f, static comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x52u, 0.3f, static comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x53u, 0.8f, static comp => comp.NumTowers >= 3, "Tower 3");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x60u, 2.5f, static comp => comp.NumCasts >= 4);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x61u, 2.3f, static comp => comp.NumCasts >= 4)
            .DeactivateOnExit<LevinstrikeSummoningFiremeld>();
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x62u, 0.3f, static comp => comp.NumCasts >= 4)
            .DeactivateOnExit<LevinstrikeSummoningIcemeld>();
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x63u, 0.8f, static comp => comp.NumTowers >= 4, "Tower 4")
            .DeactivateOnExit<LevinstrikeSummoningShock>();
        Targetable(id + 0x70u, true, 1.4f, "Reappear");

        Dualspell(id + 0x1000u, 0.1f, true)
            .DeactivateOnExit<Uplift>();
    }

    private void ChimericSuccession(uint id, float delay)
    {
        Cast(id, AID.ChimericSuccession, delay, 5f);
        CastStartMulti(id + 0x10u, [AID.FrontFirestrikes, AID.RearFirestrikes], 7.5f)
            .ActivateOnEnter<ChimericSuccession>();
        ComponentCondition<ChimericSuccession>(id + 0x20u, 3.3f, static comp => comp.NumCasts >= 1, "Defamation 1");
        ComponentCondition<ChimericSuccession>(id + 0x21u, 3.0f, static comp => comp.NumCasts >= 2, "Defamation 2");
        CastEnd(id + 0x30u, 1.7f);
        ComponentCondition<ChimericSuccession>(id + 0x31u, 0.4f, static comp => !comp.JumpActive, "Baited jump");
        ComponentCondition<ChimericSuccession>(id + 0x40u, 0.9f, static comp => comp.NumCasts >= 3, "Defamation 3");
        CastStartMulti(id + 0x50u, [AID.SwingingKickFront, AID.SwingingKickRear], 1.2f);
        ComponentCondition<ChimericSuccession>(id + 0x60u, 1.8f, static comp => comp.NumCasts >= 4, "Defamation 4")
            .ActivateOnEnter<SwingingKick>()
            .DeactivateOnExit<ChimericSuccession>();
        CastEnd(id + 0x70, 1.2f, "Front/back cleave")
            .DeactivateOnExit<SwingingKick>();
    }

    private void Charibdys(uint id, float delay)
    {
        Cast(id, AID.Charybdis, delay, 3f)
            .ActivateOnEnter<Charibdys>();
        Cast(id + 0x10u, AID.Comet, 2.1f, 5f)
            .ActivateOnEnter<CometImpact>();
        ComponentCondition<CometImpact>(id + 0x20u, 1.1f, static comp => comp.NumCasts > 0, "Proximity")
            .DeactivateOnExit<CometImpact>();

        CastStart(id + 0x100u, AID.BeastlyBile, 3.1f)
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CometBurst>()
            .ActivateOnEnter<BeastlyBile>()
            .ActivateOnEnter<Thunderbolt>();
        CastEnd(id + 0x101u, 5f);
        Cast(id + 0x110u, AID.Thunderbolt, 2.1f, 3f);
        ComponentCondition<Thunderbolt>(id + 0x120u, 1.0f, static comp => comp.NumCasts > 0, "Proteans 1");
        ComponentCondition<BeastlyBile>(id + 0x121u, 0.9f, static comp => comp.NumCasts >= 1, "Stack 1");
        // +0.8s: burst 1 should start
        Cast(id + 0x130u, AID.Thunderbolt, 1.3f, 3f);
        ComponentCondition<Thunderbolt>(id + 0x140u, 1.0f, static comp => comp.NumCasts > 4, "Proteans 2")
            .DeactivateOnExit<Thunderbolt>();
        ComponentCondition<BeastlyBile>(id + 0x141u, 0.9f, static comp => comp.NumCasts >= 2, "Stack 2")
            .DeactivateOnExit<BeastlyBile>();
        // +0.8s: burst 1 should finish, burst 2 should start

        CastStart(id + 0x200u, AID.EclipticMeteor, 2.4f)
            .ActivateOnEnter<EclipticMeteor>();
        // +4.4s: burst 2 should finish
        CastEnd(id + 0x201u, 7f);
        ComponentCondition<EclipticMeteor>(id + 0x210u, 1.9f, static comp => comp.NumCasts > 0, "LOS meteor")
            .DeactivateOnExit<EclipticMeteor>();

        Cast(id + 0x300u, AID.BeastlyFury, 6.7f, 5f);
        ComponentCondition<BeastlyFury>(id + 0x310u, 1.0f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<BeastlyFury>()
            .DeactivateOnExit<BeastlyFury>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CometBurst>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
