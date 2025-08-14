namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class FTB2DeadStarsStates : StateMachineBuilder
{
    private readonly FTB2DeadStars _module;

    public FTB2DeadStarsStates(FTB2DeadStars module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "")
            .Raw.Update = () => _module.PrimaryActor.IsDestroyed || (_module.DeathWall?.IsDestroyed ?? false) || (_module.FindComponent<PhaseChange>()?.PhaseChanged ?? false);
        SimplePhase(1u, Phase2, "")
            .Raw.Update = () => _module.BossDeadStars?.HPMP.CurHP <= 1u || _module.DeathWall!.IsDestroyed;
        SimplePhase(2u, Wait, "")
            .OnEnter(() => module.Arena.Bounds = FTB2DeadStars.DefaultArena)
            .Raw.Update = () => _module.DeathWall!.IsDestroyed;
    }

    private void Phase1(uint id)
    {
        DecisiveBattle(id, 16.6f);
        SliceNDice(id + 0x10000u, 9.4f);
        ThreeBodyProblem1(id + 0x20000u, 14.8f);
        VengefulBioIIIBlizzardIIIFireIII(id + 0x30000u, 14.4f);
        DeltaAttack(id + 0x40000u, 13.1f);
        FireStrike1(id + 0x50000u, 4.7f);
        ThreeBodyProblem2(id + 0x60000u, 14.4f);
        VengefulBioIIIBlizzardIIIFireIII(id + 0x70000u, 14.3f);
        DeltaAttack(id + 0x80000u, 13.2f);
        FireStrike2(id + 0x90000u, 4.7f);
        ThreeBodyProblem3(id + 0xA0000u, 15.5f);
    }

    private void Phase2(uint id)
    {
        VengefulBioIIIBlizzardIIIFireIII(id, 12f, true);
        SixHandedFistfight(id + 0x10000u, 24.9f);
        SimpleState(id + 0x20000u, 15f, "Enrage");
    }

    private void Wait(uint id)
    {
        SimpleState(id, 15f, "Wait for deathwall to disappear!");
    }

    private void DecisiveBattle(uint id, float delay)
    {
        ComponentCondition<DecisiveBattleAOEs>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DecisiveBattleAOEs>()
            .ActivateOnEnter<DecisiveBattleStatus>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<DecisiveBattleAOEs>();
        ComponentCondition<DecisiveBattleStatus>(id + 0x10u, 0.9f, comp => comp.Active, "Assign targets");
    }

    private void SliceNDice(uint id, float delay)
    {
        ComponentCondition<SliceNDice>(id, delay, comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<SliceNDice>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<SliceNDice>();
    }

    private void ThreeBodyProblem1(uint id, float delay)
    {
        ComponentCondition<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>(id, delay, comp => comp.NumCasts != 0, "Circle AOEs")
            .ActivateOnEnter<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .DeactivateOnExit<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .DeactivateOnExit<DecisiveBattleStatus>();
        ComponentCondition<PrimordialChaosRaidwide>(id + 0x10u, 13.5f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<PrimordialChaosRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<PrimordialChaosRaidwide>();
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + 0x20u + (uint)((i - 1) * 0x10u);
            var casts = i;
            var condition = ComponentCondition<PrimordialChaos>(offset, i == 1 ? 4f : 2.5f, comp => comp.NumTelegraphCasts == casts, $"Color AOE telegraphs {i}");
            if (i == 1)
            {
                condition
                    .DeactivateOnExit<ArenaChange>()
                    .ActivateOnEnter<PrimordialChaos>();
            }
        }

        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + 0x50u + (uint)(i * 0x10u);
            var casts = i * 4;
            var condition = ComponentCondition<PrimordialChaos>(offset, i == 1 ? 4f : 5.6f, comp => comp.NumCasts == casts, $"Color AOEs {i}");
            if (i == 4)
            {
                condition.DeactivateOnExit<PrimordialChaos>();
            }
        }

        ComponentCondition<NoxiousNova>(id + 0xA0u, 10f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<NoxiousNova>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<NoxiousNova>();
    }

    private void VengefulBioIIIBlizzardIIIFireIII(uint id, float delay, bool last = false)
    {
        ComponentCondition<VengefulBioIIIBlizzardIIIFireIII>(id, delay, comp => comp.NumCasts != 0, "Cone AOEs")
            .ActivateOnEnter<VengefulBioIIIBlizzardIIIFireIII>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<VengefulBioIIIBlizzardIIIFireIII>();
        if (!last)
        {
            ComponentCondition<DecisiveBattleStatus>(id + 0x10u, 4.6f, comp => comp.Active, "Assign targets")
                .ActivateOnEnter<DecisiveBattleStatus>();
        }
    }

    private void DeltaAttack(uint id, float delay)
    {
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i * 2;
            var condition = ComponentCondition<DeltaAttack>(offset, i == 1 ? delay : 1.1f, comp => comp.NumCasts == casts, $"Raidwide {i}")
                .SetHint(StateMachine.StateHint.Raidwide);
            if (i == 1)
            {
                condition
                    .ActivateOnEnter<DeltaAttackRaidwide>()
                    .ActivateOnEnter<DeltaAttack>();
            }
            else if (i == 3)
            {
                condition
                    .DeactivateOnExit<DeltaAttackRaidwide>()
                    .DeactivateOnExit<DeltaAttack>();
            }
        }
    }

    private void FireStrike1(uint id, float delay)
    {
        ComponentCondition<Firestrike1>(id, delay, comp => comp.CurrentBaits.Count == 3, "Line stacks appear")
            .ActivateOnEnter<Firestrike1>();
        ComponentCondition<Firestrike1>(id + 0x10u, 5.2f, comp => comp.NumCasts == 3, "Line stacks resolve")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<Firestrike1>();
    }

    private void FireStrike2(uint id, float delay)
    {
        ComponentCondition<Firestrike2>(id, delay, comp => comp.CurrentBaits.Count == 3, "Line stacks appear")
            .ActivateOnEnter<Firestrike2>();
        ComponentCondition<SliceNDice>(id + 0x10u, 5f, comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<SliceNDice>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<SliceNDice>();
        ComponentCondition<Firestrike2>(id + 0x20u, 1.2f, comp => comp.NumCasts == 3, "Line stacks resolve")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<Firestrike2>();
    }

    private void ThreeBodyProblem2(uint id, float delay)
    {
        ComponentCondition<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>(id, delay, comp => comp.NumCasts != 0, "Circle AOEs")
            .ActivateOnEnter<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .ActivateOnExit<IceboundBuffoonery>()
            .DeactivateOnExit<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .DeactivateOnExit<DecisiveBattleStatus>();
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + 0x10u + (uint)((i - 1) * 0x10u);
            var casts = i * 2;
            var condition = ComponentCondition<Snowboulder>(offset, i == 1 ? 14.5f : 2.6f, comp => comp.NumCasts == casts, $"Wild charges {i}");
            if (i == 1)
            {
                condition
                    .ActivateOnEnter<Snowboulder>()
                    .ActivateOnEnter<SnowBoulderKnockback>();
            }
            else if (i == 3)
            {
                condition
                    .ActivateOnEnter<Avalaunch>()
                    .ActivateOnEnter<AvalaunchTether>()
                    .DeactivateOnExit<SnowBoulderKnockback>()
                    .DeactivateOnExit<Snowboulder>();
            }
        }
        ComponentCondition<ChillingCollision>(id + 0x40u, 6.1f, comp => comp.NumCasts != 0, "Knockback")
            .ActivateOnEnter<ChillingCollision>()
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<ChillingCollision>();
        ComponentCondition<Avalaunch>(id + 0x50u, 1.6f, comp => comp.NumFinishedStacks != 0, "Stacks resolve")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<IceboundBuffoonery>()
            .DeactivateOnExit<AvalaunchTether>()
            .DeactivateOnExit<Avalaunch>();
        ActorCastStart(id + 0x60u, _module.BossNereid, (uint)AID.ToTheWinds1, 5.5f, true, "Snowball enrage start")
            .ActivateOnEnter<SelfDestruct>();
        ActorCastEnd(id + 0x70u, _module.BossNereid, 13f, true, "Snowball enrage end")
            .DeactivateOnExit<SelfDestruct>();
    }

    private void ThreeBodyProblem3(uint id, float delay)
    {
        ComponentCondition<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>(id, delay, comp => comp.NumCasts != 0, "Circle AOEs")
            .ActivateOnEnter<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .ActivateOnEnter<PhaseChange>()
            .DeactivateOnExit<NoisomeNuisanceIceboundBuffoonBlazingBelligerent>()
            .DeactivateOnExit<DecisiveBattleStatus>();

        ElementalImpact(id + 0x10u, 2, 14.1f, 1);
        FireSpread(id + 0x20u, 4, 0.5f, 1);
        ElementalImpact(id + 0x30u, 4, 3.6f, 2);
        FireSpread(id + 0x40u, 8, 0.5f, 2);
        ElementalImpact(id + 0x50u, 6, 3.6f, 3);
        FireSpread(id + 0x60u, 12, 0.5f, 3);
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + 0x70u + (uint)((i - 1) * 0x10);
            var casts = 8 * i;
            var cond = ComponentCondition<GeothermalRupture>(offset, (i == 1) ? 7.7f : 2f, comp => comp.NumCasts == casts, $"Baited circles {i}");
            if (i == 1)
                cond
                    .ActivateOnEnter<FlameThrower>()
                    .ActivateOnEnter<GeothermalRupture>();
            else if (i == 3)
                cond.DeactivateOnExit<GeothermalRupture>();
        }
        ComponentCondition<FlameThrower>(id + 0xA0u, 0.1f, comp => comp.NumCasts != 0, "Line stacks")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<FlameThrower>();
        ElementalImpact(id + 0xB0u, 8, 5f, 4);
        FireSpread(id + 0xC0u, 16, 0.5f, 4);
        ElementalImpact(id + 0xD0u, 10, 3.6f, 5);
        FireSpread(id + 0xE0u, 20, 0.5f, 5);
        ElementalImpact(id + 0xF0u, 12, 3.6f, 6);
        FireSpread(id + 0x100u, 24, 0.5f, 6);
        CastStart(id + 0x110u, (uint)AID.ToTheWinds2, 4.8f, "Fireball enrage start")
            .ActivateOnEnter<SelfDestruct>();
        CastEnd(id + 0x120u, 7f, "Fireball enrage end")
            .DeactivateOnExit<SelfDestruct>();

        void ElementalImpact(uint id, int castCount, float delay, int index)
        {
            var cond = ComponentCondition<ElementalImpact>(id, delay, comp => comp.NumCasts == castCount, $"Towers {index}");
            if (index == 1)
            {
                cond
                .ActivateOnEnter<FireSpread>()
                .ActivateOnEnter<ElementalImpact>();
            }
            else if (index == 6)
            {
                cond
                .DeactivateOnExit<ElementalImpact>();
            }
        }

        void FireSpread(uint id, int castCount, float delay, int index)
        {
            var cond = ComponentCondition<FireSpread>(id, delay, comp => comp.NumCasts == castCount, $"Baited cones {index}");
            if (index == 6)
            {
                cond
                .DeactivateOnExit<FireSpread>();
            }
        }
    }

    private void SixHandedFistfight(uint id, float delay)
    {
        ComponentCondition<SixHandedFistfightRaidwide>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<SixHandedFistfightRaidwide>()
            .ActivateOnEnter<SixHandedFistfightArenaChange>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<SixHandedFistfightArenaChange>()
            .DeactivateOnExit<SixHandedFistfightRaidwide>();
        CollateralDamage(id + 0x10u);
        CollateralDamage(id + 0x90u, false);

        void CollateralDamage(uint id, bool first = true)
        {
            for (var i = 1; i <= 2; ++i)
            {
                var offset = id + (uint)((i - 1) * 0x10u);
                var casts = i * 3;
                var condition = ComponentCondition<CollateralColdGasHeatJet>(offset, i == 1 ? first ? 11.6f : 10f : 2f, comp => comp.NumCasts == casts, $"Cone AOEs {i + (first ? 0 : 2)}");
                if (i == 1)
                {
                    condition
                        .ActivateOnEnter<CollateralDamage>()
                        .ActivateOnEnter<CollateralColdGasHeatJet>();
                }
                else if (i == 2)
                {
                    condition
                        .DeactivateOnExit<CollateralColdGasHeatJet>();
                }
            }
            for (var i = 1; i <= 6; ++i)
            {
                var offset = id + 0x20u + (uint)((i - 1) * 0x10u);
                var casts = i <= 5 ? i * 8 : 41;
                var condition = ComponentCondition<CollateralDamage>(offset, i == 1 ? 5.2f : 1f, comp => comp.NumCasts >= casts, $"Spreads {i + (first ? 0 : 6)}");
                if (i == 6)
                {
                    condition
                        .DeactivateOnExit<CollateralDamage>();
                }
            }
        }
    }
}
