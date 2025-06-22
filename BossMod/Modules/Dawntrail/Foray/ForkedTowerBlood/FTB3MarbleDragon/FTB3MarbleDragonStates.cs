namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class FTB3MarbleDragonStates : StateMachineBuilder
{
    private readonly FTB3MarbleDragon _module;

    public FTB3MarbleDragonStates(FTB3MarbleDragon module) : base(module)
    {
        _module = module;
        DeathPhase(default, SinglePhase)
            .ActivateOnEnter<ArenaChange>();
    }

    private void SinglePhase(uint id)
    {
        ImitationStar(id, 13f);
        DraconiformMotion(id + 0x10000u, 13.5f);
        ImitationRain1(id + 0x20000u, 5f);
        DreadDeluge(id + 0x30000u, 13.1f);
        ImitationRain2(id + 0x40000u, 5.8f);
        DreadDeluge(id + 0x50000u, 8.6f, true);
        IceGolems(id + 0x60000u, 16.5f);
        ImitationRain3(id + 0x70000u, 0.6f);
        ImitationRain3(id + 0x80000u, 4.6f, false);
        LifelessLegacy(id + 0x90000u, 6.9f);
        ImitationRain4(id + 0xA0000u, 13.4f);
        ImitationStar(id + 0xB0000u, 17.9f);
        ImitationRain5(id + 0xC0000u, 12f);
        ImitationRain5(id + 0xD0000u, 2.2f, false);
        ImitationStar(id + 0xE0000u, 14.6f, true);
        DreadDeluge(id + 0xF0000u, 11.3f);
        SimpleState(id + 0x100000u, 34f, "Enrage");
    }

    private void ImitationStar(uint id, float delay, bool unloadIceWind = false)
    {
        var cond = ComponentCondition<ImitationStar>(id, delay, comp => comp.NumCasts != 0, "Raidwide + bleed")
            .ActivateOnEnter<ImitationStar>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationStar>();
        if (unloadIceWind)
        {
            cond
                .DeactivateOnExit<FrigidTwister>();
        }
    }

    private void DraconiformMotion(uint id, float delay)
    {
        ComponentCondition<DraconiformMotion>(id, delay, comp => comp.NumCasts == 2, "Front/back cone AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .ActivateOnEnter<DraconiformMotionBait>()
            .ExecOnEnter<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(8.6d))
            .DeactivateOnExit<ArenaChange>()
            .ActivateOnExit<ImitationRain>()
            .DeactivateOnExit<DraconiformMotionBait>()
            .DeactivateOnExit<DraconiformMotion>();
    }

    private void ImitationRain1(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnExit<BallOfIce>()
            .ActivateOnExit<DraconiformMotionBait>()
            .ExecOnExit<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(11.2d))
            .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(4.9d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationRain>();
        ComponentCondition<ImitationIcicle>(id + 0x10u, 15.6f, comp => comp.NumCasts == 2, "Circle AOEs")
            .ActivateOnEnter<ImitationBlizzard>()
            .ActivateOnEnter<DraconiformMotion>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .ActivateOnEnter<ImitationIcicle>()
            .DeactivateOnExit<ImitationIcicle>();
        ComponentCondition<DraconiformMotion>(id + 0x20u, 0.4f, comp => comp.NumCasts == 2, "Front/back cone AOEs")
            .DeactivateOnExit<DraconiformMotionBait>()
            .DeactivateOnExit<DraconiformMotion>();
        ImitationBlizzard(id + 0x30u);
    }

    private void DreadDeluge(uint id, float delay, bool unloadIceWind = false)
    {
        var cond = ComponentCondition<DreadDeluge>(id, delay, comp => comp.NumCasts != 0, "Tankbusters")
            .ActivateOnEnter<DreadDeluge>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DreadDeluge>();
        if (unloadIceWind)
        {
            cond
                .DeactivateOnExit<FrigidTwister>();
        }
    }

    private void ImitationRain2(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnExit<DraconiformMotionBait>()
            .ExecOnExit<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(9.8d))
            .ActivateOnExit<BallOfIce>()
            .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(5.8d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<ImitationRain>();
        ComponentCondition<FrigidTwister>(id + 0x10u, 7.3f, comp => comp.ActiveAOEs(0, default!).Length != 0, "Icewinds appear")
            .ActivateOnEnter<FrigidTwister>()
            .ActivateOnEnter<ImitationBlizzard>();
        ComponentCondition<DraconiformMotion>(id + 0x20u, 7.4f, comp => comp.NumCasts == 2, "Front/back cone AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .DeactivateOnExit<DraconiformMotionBait>()
            .DeactivateOnExit<DraconiformMotion>();
        for (var i = 1; i <= 3; ++i)
        {
            var offset = id + 0x30u + (uint)((i - 1) * 0x10u);
            var casts = i switch
            {
                1 => 2,
                2 => 3,
                _ => 7
            };
            var condition = ComponentCondition<ImitationBlizzard>(offset, i == 1 ? 3.7f : 4f, comp => comp.NumCasts >= casts, $"Ice puddle AOEs {i}");
            if (i == 3)
            {
                condition
                    .DeactivateOnExit<BallOfIce>()
                    .DeactivateOnExit<ImitationBlizzard>();
            }
        }
    }

    private void IceGolems(uint id, float delay)
    {
        ComponentCondition<IceGolems>(id, delay, comp => comp.ActiveActors.Count != 0, "Adds targetable (Ice Golems)")
            .ActivateOnEnter<VulnerabilityDown>()
            .ActivateOnEnter<WitheringEternity>()
            .ActivateOnEnter<IceGolems>();
    }

    private void ImitationRain3(uint id, float delay, bool first = true)
    {
        if (!first)
        {
            ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
                .ActivateOnEnter<ImitationRain>()
                .ActivateOnExit<BallOfIce>()
                .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(4.6d))
                .SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<ImitationRain>();
        }
        else
        {
            ComponentCondition<WitheringEternity>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
                .ActivateOnExit<BallOfIce>()
                .SetHint(StateMachine.StateHint.Raidwide)
                .DeactivateOnExit<WitheringEternity>();
        }
        ComponentCondition<FrigidDive>(id + 0x10u, first ? 12.2f : 11.4f, comp => comp.NumCasts != 0, "Rect AOE")
            .ActivateOnEnter<FrigidDive>()
            .ActivateOnEnter<ImitationBlizzard>()
            .ActivateOnEnter<ImitationBlizzardTowers>()
            .DeactivateOnExit<FrigidDive>();
        ComponentCondition<ImitationBlizzardTowers>(id + 0x20u, 4.1f, comp => comp.NumCasts != 0, "Towers 1 + cross AOE");
        ComponentCondition<ImitationBlizzardTowers>(id + 0x30u, 4f, comp => comp.NumCasts == 6, "Towers 2")
            .DeactivateOnExit<ImitationBlizzard>()
            .DeactivateOnExit<BallOfIce>()
            .DeactivateOnExit<ImitationBlizzardTowers>();
    }

    private void LifelessLegacy(uint id, float delay)
    {
        ComponentCondition<IceSprite>(id, delay, comp => comp.ActiveActors.Count != 0, "Adds targetable (Ice Sprites)")
            .ActivateOnEnter<LifelessLegacy>()
            .ActivateOnEnter<DamageUp>()
            .ActivateOnEnter<IceSprite>();
        ComponentCondition<LifelessLegacy>(id + 0x10u, 36.8f, comp => comp.NumCasts != 0, "Raidwide or enrage")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LifelessLegacy>()
            .DeactivateOnExit<IceSprite>()
            .DeactivateOnExit<DamageUp>()
            .DeactivateOnExit<IceGolems>()
            .DeactivateOnExit<VulnerabilityDown>();
    }

    private void ImitationRain4(uint id, float delay)
    {
        ComponentCondition<ImitationBlizzard>(id, delay, comp => comp.WickedWaterActive, "Water debuffs appear")
            .ActivateOnEnter<BallOfIce>()
            .ActivateOnEnter<ImitationBlizzard>()
            .ActivateOnEnter<WickedWater>();
        ComponentCondition<WickedWater>(id + 0x10u, 1.8f, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnExit<DraconiformMotionBait>()
            .ExecOnExit<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(11d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<WickedWater>();
        ComponentCondition<ImitationIcicle>(id + 0x20u, 15.4f, comp => comp.NumCasts == 2, "Circle AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .ActivateOnEnter<ImitationIcicle>()
            .DeactivateOnExit<ImitationIcicle>();
        ComponentCondition<DraconiformMotion>(id + 0x30u, 0.4f, comp => comp.NumCasts == 2, "Front/back cone AOEs")
            .ActivateOnEnter<GelidGaol>()
            .DeactivateOnExit<DraconiformMotionBait>()
            .DeactivateOnExit<DraconiformMotion>();
        ImitationBlizzard(id + 0x40u);
    }

    private void ImitationRain5(uint id, float delay, bool first = true)
    {
        var cond = ComponentCondition<ImitationRain>(id, delay, comp => comp.NumCasts != 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnExit<BallOfIce>()
            .ExecOnEnter<ImitationRain>(comp => comp.Activation = _module.WorldState.FutureTime(first ? 12d : 2.1d))
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnExit<ImitationBlizzard>()
            .ExecOnExit<ImitationBlizzard>(comp => comp.IsRain4 = true)
            .DeactivateOnExit<ImitationRain>();
        if (first)
        {
            cond
                .ActivateOnExit<DraconiformMotionBait>()
                .ExecOnExit<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(9.7d));
            ComponentCondition<FrigidTwister>(id + 0x10u, 7.4f, comp => comp.ActiveAOEs(0, default!).Length != 0, "Icewinds appear")
                .ActivateOnEnter<FrigidTwister>();
        }
        else
        {
            cond
                .ActivateOnEnter<DraconiformMotionBait>()
                .ExecOnEnter<DraconiformMotionBait>(comp => comp.Activation = _module.WorldState.FutureTime(3.8d));
        }
        ComponentCondition<DraconiformMotion>(id + (first ? 0x20u : 0x10u), first ? 7.2f : 6.4f, comp => comp.NumCasts == 2, "Front/back cone AOEs")
            .ActivateOnEnter<DraconiformMotion>()
            .ActivateOnEnter<ImitationBlizzardTowers>()
            .ExecOnEnter<DraconiformMotion>(comp => comp.Color = Colors.Danger)
            .DeactivateOnExit<DraconiformMotionBait>()
            .DeactivateOnExit<DraconiformMotion>();
        ComponentCondition<ImitationBlizzard>(id + (first ? 0x30u : 0x20u), first ? 3.7f : 1.7f, comp => comp.NumCasts == 2, $"Ice puddle AOEs 1")
            .ExecOnExit<ImitationBlizzard>(comp => comp.Show = false);
        ComponentCondition<ImitationBlizzardTowers>(id + (first ? 0x40u : 0x30u), 4f, comp => comp.NumCasts == 6, $"Towers")
            .ExecOnExit<ImitationBlizzard>(comp => comp.Show = true)
            .DeactivateOnExit<ImitationBlizzardTowers>();
        ComponentCondition<ImitationBlizzard>(id + (first ? 0x50u : 0x40u), 6f, comp => comp.NumCasts == 4, $"Ice puddle AOEs 2")
            .DeactivateOnExit<BallOfIce>()
            .DeactivateOnExit<ImitationBlizzard>();
    }

    private void ImitationBlizzard(uint id)
    {
        for (var i = 1; i <= 4; ++i)
        {
            var offset = id + (uint)((i - 1) * 0x10u);
            var casts = i switch
            {
                1 => 2,
                2 => 5,
                3 => 7,
                _ => 8
            };
            var condition = ComponentCondition<ImitationBlizzard>(offset, i == 1 ? 4.2f : 1f, comp => comp.NumCasts == casts, $"Ice puddle AOEs {i}");
            if (i == 4)
            {
                condition
                    .DeactivateOnExit<BallOfIce>()
                    .DeactivateOnExit<ImitationBlizzard>();
            }
        }
    }
}
