namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

abstract class C022GoraiStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C022GoraiStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<ArenaChange>();
    }

    private void SinglePhase(uint id)
    {
        Unenlightenment(id, 8.2f);
        SealOfScurryingSparksFlameAndSulphur(id + 0x10000u, 9.1f);
        ImpurePurgation(id + 0x20000u, 3.3f);
        Thundercall(id + 0x30000u, 8.1f);
        TorchingTorment(id + 0x40000u, 5.6f);
        RousingReincarnation(id + 0x50000u, 8.1f);
        Unenlightenment(id + 0x60000u, 6.4f);
        SealOfScurryingSparksCloudToGround(id + 0x70000u, 9.1f);
        FightingSpirits(id + 0x80000u, 10.3f);
        TorchingTorment(id + 0x90000u, 7.1f);
        MalformedReincarnation(id + 0xA0000u, 8.1f);
        SealOfScurryingSparksFlameAndSulphur(id + 0xB0000u, 10.6f);
        Unenlightenment(id + 0xC0000u, 4.4f);
        Cast(id + 0xD0000u, AID.LivingHell, 6.3f, 10f, "Enrage");
    }

    private void Unenlightenment(uint id, float delay)
    {
        Cast(id, AID.Unenlightenment, delay, 5f)
            .ActivateOnEnter<NUnenlightenment>(!_savage)
            .ActivateOnEnter<SUnenlightenment>(_savage);
        ComponentCondition<Unenlightenment>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Unenlightenment>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TorchingTorment(uint id, float delay)
    {
        Cast(id, AID.TorchingTorment, delay, 5f)
            .ActivateOnEnter<TorchingTorment>();
        ComponentCondition<TorchingTorment>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<TorchingTorment>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SealOfScurryingSparksFlameAndSulphur(uint id, float delay)
    {
        Cast(id, AID.SealOfScurryingSparks, delay, 4f)
            .ActivateOnEnter<SealOfScurryingSparks>();
        Cast(id + 0x10u, AID.FlameAndSulphur, 2.4f, 3f);
        // +0.8s: spawn rocks/flames
        CastMulti(id + 0x20u, [AID.BrazenBalladExpanding, AID.BrazenBalladSplitting], 6.4f, 5f)
            .ActivateOnEnter<FlameAndSulphur>();
        ComponentCondition<FlameAndSulphur>(id + 0x30u, 3.1f, static comp => comp.NumCasts > 0, "Expanding/splitting aoes")
            .DeactivateOnExit<FlameAndSulphur>();
        ComponentCondition<SealOfScurryingSparks>(id + 0x31u, 0.4f, static comp => comp.NumMechanics > 0, "Pairs")
            .DeactivateOnExit<SealOfScurryingSparks>();
    }

    private void SealOfScurryingSparksCloudToGround(uint id, float delay)
    {
        Cast(id, AID.SealOfScurryingSparks, delay, 4f)
            .ActivateOnEnter<SealOfScurryingSparks>();
        Cast(id + 0x10u, AID.CloudToGround, 2.4f, 6.2f)
            .ActivateOnEnter<CloudToGround>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<CloudToGround>(id + 0x20u, 0.8f, static comp => comp.NumCasts > 0, "Exaflares start");
        ComponentCondition<SealOfScurryingSparks>(id + 0x30u, 1.9f, static comp => comp.NumMechanics > 0, "Stack/spread");
        ComponentCondition<SealOfScurryingSparks>(id + 0x40u, 5.0f, static comp => comp.NumMechanics > 1, "Spread/stack")
            .DeactivateOnExit<CloudToGround>() // last exaflare ~0.5s before resolve
            .DeactivateOnExit<SealOfScurryingSparks>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ImpurePurgation(uint id, float delay)
    {
        Cast(id, AID.ImpurePurgation, delay, 3.6f)
            .ActivateOnEnter<ImpurePurgationBait>();
        ComponentCondition<ImpurePurgationBait>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Proteans bait")
            .DeactivateOnExit<ImpurePurgationBait>();
        ComponentCondition<ImpurePurgationAOE>(id + 0x20u, 2.0f, static comp => comp.NumCasts > 0, "Proteans resolve")
            .ActivateOnEnter<NImpurePurgationAOE>(!_savage)
            .ActivateOnEnter<SImpurePurgationAOE>(_savage)
            .DeactivateOnExit<ImpurePurgationAOE>();
    }

    private void Thundercall(uint id, float delay)
    {
        Cast(id, AID.Thundercall, delay, 3f);
        // +3.1s: spawn 6 lightning orbs
        Cast(id + 0x10, AID.HumbleHammer, 5.9f, 5f, "Reduce aoe size")
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<Flintlock>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Flintlock>(id + 0x20u, 4.1f, static comp => comp.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<Flintlock>();
        ComponentCondition<Thundercall>(id + 0x21u, 0.1f, static comp => comp.NumCasts > 0, "AOEs")
            .DeactivateOnExit<Thundercall>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void RousingReincarnation(uint id, float delay)
    {
        Cast(id, AID.RousingReincarnation, delay, 5f)
            .ActivateOnEnter<NRousingReincarnation>(!_savage)
            .ActivateOnEnter<SRousingReincarnation>(_savage);
        ComponentCondition<RousingReincarnation>(id + 0x10u, 0.6f, static comp => comp.NumCasts > 0, "Towers/proteans start")
            .DeactivateOnExit<RousingReincarnation>();
        Cast(id + 0x20u, AID.MalformedPrayer, 1.8f, 4f)
            .ActivateOnEnter<MalformedPrayer1>(); // env controls are 2s after cast end, then every 6s; bursts are 10s after corresponding envcontrol
        Cast(id + 0x30u, AID.PointedPurgation, 3.4f, 8f)
            .ActivateOnEnter<PointedPurgation>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<MalformedPrayer1>(id + 0x40u, 0.7f, static comp => comp.NumCasts > 0, "Towers/proteans 1");
        ComponentCondition<MalformedPrayer1>(id + 0x50u, 6f, static comp => comp.NumCasts > 2, "Towers/proteans 2");
        ComponentCondition<MalformedPrayer1>(id + 0x60u, 6f, static comp => comp.NumCasts > 4, "Towers/proteans 3");
        ComponentCondition<MalformedPrayer1>(id + 0x70u, 6f, static comp => comp.NumCasts > 6, "Towers/proteans 4")
            .DeactivateOnExit<MalformedPrayer1>()
            .DeactivateOnExit<PointedPurgation>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void FightingSpirits(uint id, float delay)
    {
        CastStart(id, AID.FightingSpirits, delay)
            .ActivateOnEnter<WorldlyPursuitBait>(); // icons appear right before cast start
        CastEnd(id + 1u, 5f)
            .ActivateOnEnter<NFightingSpirits>(!_savage)
            .ActivateOnEnter<SFightingSpirits>(_savage);
        ComponentCondition<FightingSpirits>(id + 2u, 1.2f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FightingSpirits>();
        Targetable(id + 0x10u, false, 2.7f, "Boss disappears");
        ComponentCondition<WorldlyPursuitBait>(id + 0x20u, 0.7f, static comp => comp.NumCasts > 0, "Jump 1");
        ComponentCondition<WorldlyPursuitBait>(id + 0x30u, 3.1f, static comp => comp.NumCasts > 1, "Jump 2");
        ComponentCondition<WorldlyPursuitBait>(id + 0x40u, 3.1f, static comp => comp.NumCasts > 2, "Jump 3");
        ComponentCondition<WorldlyPursuitBait>(id + 0x50u, 3.1f, static comp => comp.NumCasts > 3, "Jump 4")
            .DeactivateOnExit<WorldlyPursuitBait>();
        ComponentCondition<WorldlyPursuitLast>(id + 0x60u, 3.1f, static comp => comp.NumCasts > 0, "Jump 5")
            .ActivateOnEnter<WorldlyPursuitLast>()
            .DeactivateOnExit<WorldlyPursuitLast>();
        Targetable(id + 0x70u, true, 2.5f, "Boss reappears");
    }

    private void MalformedReincarnation(uint id, float delay)
    {
        Cast(id, AID.MalformedReincarnation, delay, 5f)
            .ActivateOnEnter<NMalformedReincarnation>(!_savage)
            .ActivateOnEnter<SMalformedReincarnation>(_savage);
        ComponentCondition<MalformedReincarnation>(id + 0x10, 0.6f, static comp => comp.NumCasts > 0, "Towers start")
            .DeactivateOnExit<MalformedReincarnation>();
        Cast(id + 0x20u, AID.MalformedPrayer, 1.8f, 4f)
            .ActivateOnEnter<MalformedPrayer2>(); // first env controls are 2s after cast end
        CastStart(id + 0x30u, AID.FlickeringFlame, 10.2f, "Towers drop")
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x31u, 3f)
            .ActivateOnEnter<NFlickeringFlame>(!_savage)
            .ActivateOnEnter<SFlickeringFlame>(_savage);
        ComponentCondition<MalformedPrayer2>(id + 0x40u, 2.0f, static comp => comp.NumCasts > 0, "Towers 1", 0);
        ComponentCondition<MalformedPrayer2>(id + 0x50u, 2.5f, static comp => comp.NumCasts > 4, "Towers 2", 0);
        ComponentCondition<MalformedPrayer2>(id + 0x60u, 2.0f, static comp => comp.NumCasts > 8, "Towers 3", 0)
            .DeactivateOnExit<MalformedPrayer2>();
        ComponentCondition<FlickeringFlame>(id + 0x70u, 0.5f, static comp => comp.NumCasts >= 8, "Criss-cross 1");
        ComponentCondition<FlickeringFlame>(id + 0x80u, 2.1f, static comp => comp.NumCasts >= 16, "Criss-cross 2")
            .DeactivateOnExit<FlickeringFlame>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}

sealed class C022NGoraiStates(BossModule module) : C022GoraiStates(module, false);
sealed class C022SGoraiStates(BossModule module) : C022GoraiStates(module, true);
