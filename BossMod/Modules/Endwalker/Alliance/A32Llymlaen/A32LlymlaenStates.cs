namespace BossMod.Endwalker.Alliance.A32Llymlaen;

sealed class A32LlymlaenStates : StateMachineBuilder
{
    public A32LlymlaenStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Tempest(id, 7.2f);
        SeafoamSpiral(id + 0x10000u, 5.2f);
        WindRose(id + 0x20000u, 3.2f);
        NavigatorsTridentNormal(id + 0x30000u, 10.6f);
        WindRoseSeafoamSpiral(id + 0x40000u, 6.2f);
        SurgingWaveSeaFoam(id + 0x50000u, 2.1f);
        DeepDiveNormal(id + 0x60000u, 3.2f);
        TorrentialTridents(id + 0x70000u, 9.3f, false);
        StormySeas(id + 0x80000u, 1.3f);
        DenizensOfTheDeep(id + 0x90000u, 9.9f);
        Godsbane(id + 0xA0000u, 3.6f);
        NavigatorsTridentAdds(id + 0xB0000u, 9.5f);
        WindRoseSeafoamSpiral(id + 0xC0000u, 5.1f);
        SurgingWaveToTheLast(id + 0xD0000u, 2.1f);
        DeepDiveHardWater(id + 0xE0000u, 5.3f);
        TorrentialTridents(id + 0xF0000u, 6.2f, true);
        StormySeasMaelstrom(id + 0x100000u, 0.3f);
        Godsbane(id + 0x110000u, 4.3f);
        WindRoseSeafoamSpiral(id + 0x120000u, 11.3f);
        StormySeas(id + 0x130000u, 2.9f);
        LeftRightStrait(id + 0x140000u, 0.3f);
        Tempest(id + 0x150000u, 6.2f);
        NavigatorsTridentAdds(id + 0x160000u, 11.5f);
        WindRoseSeafoamSpiral(id + 0x170000u, 5f);
        SurgingWaveToTheLast(id + 0x180000u, 2.1f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Tempest(uint id, float delay)
    {
        Cast(id, AID.Tempest, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SeafoamSpiral(uint id, float delay)
    {
        Cast(id, AID.SeafoamSpiral, delay, 6, "In")
            .ActivateOnEnter<SeafoamSpiral>()
            .DeactivateOnExit<SeafoamSpiral>();
    }

    private void WindRose(uint id, float delay)
    {
        Cast(id, AID.WindRose, delay, 6f, "Out")
            .ActivateOnEnter<WindRose>()
            .DeactivateOnExit<WindRose>();
    }

    private State WindRoseSeafoamSpiral(uint id, float delay)
    {
        return CastMulti(id, [AID.WindRose, AID.SeafoamSpiral], delay, 6f, "In/out")
            .ActivateOnEnter<WindRose>()
            .ActivateOnEnter<SeafoamSpiral>()
            .DeactivateOnExit<WindRose>()
            .DeactivateOnExit<SeafoamSpiral>();
    }

    private void LeftRightStrait(uint id, float delay)
    {
        CastMulti(id, [AID.LeftStrait, AID.RightStrait], delay, 6f, "Side")
            .ActivateOnEnter<Strait>()
            .DeactivateOnExit<Strait>();
    }

    private void NavigatorsTridentNormal(uint id, float delay)
    {
        Cast(id, AID.NavigatorsTrident, delay, 6.5f)
            .ActivateOnEnter<DireStraits>();
        ComponentCondition<DireStraits>(id + 0x10u, 1.0f, static comp => comp.NumCasts > 0, "Side 1");
        ComponentCondition<DireStraits>(id + 0x11u, 1.8f, static comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<DireStraits>();

        ComponentCondition<NavigatorsTridentAOE>(id + 0x20u, 1.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NavigatorsTridentAOE>()
            .ActivateOnEnter<SurgingWaveFrothingSea>();
        ComponentCondition<NavigatorsTridentAOE>(id + 0x21u, 7f, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<SurgingWaveCorridor>() // envcontrol will happen right after knockback end
            .ActivateOnEnter<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State CorridorBounds(State state)
    {
        return state
            .ExecOnEnter<SurgingWaveCorridor>(comp =>
            {
                var isEast = comp.CorridorDir.X > 0f;
                var center = Module.Arena.Center;
                var arena = new ArenaBoundsCustom([new Rectangle(center, 19f, 29f), new Rectangle(center + new WDir((isEast ? 1f : -1f) * 39f, 0f), 40f, 10f)], ScaleFactor: 1.5f);
                Module.Arena.Center = arena.Center;
                Module.Arena.Bounds = arena;
            });
    }

    private State NormalBounds(State state)
    {
        return state
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (new(0f, -900f), new ArenaBoundsRect(19f, 29f)));
    }

    private void SurgingWaveSeaFoam(uint id, float delay)
    {
        var state = Cast(id, AID.SurgingWave, delay, 9f)
            .ActivateOnEnter<SurgingWaveAOE>()
            .ActivateOnEnter<SurgingWaveShockwave>()
            .ActivateOnEnter<SurgingWaveSeaFoam>();
        CorridorBounds(state);
        ComponentCondition<SurgingWaveAOE>(id + 0x10u, 1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<SurgingWaveAOE>();
        ComponentCondition<SurgingWaveShockwave>(id + 0x11u, 0.2f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SurgingWaveShockwave>();
        ComponentCondition<SurgingWaveFrothingSea>(id + 0x20u, 6.2f, static comp => comp.NumCasts > 0);
        CastStartMulti(id + 0x30u, [AID.LeftStrait, AID.RightStrait], 7.8f);
        ComponentCondition<SurgingWaveFrothingSea>(id + 0x40u, 3.9f, static comp => comp.NumCasts > 12)
            .ActivateOnEnter<Strait>();
        var state2 = ComponentCondition<SurgingWaveCorridor>(id + 0x50u, 0.5f, static comp => comp.CorridorDir == default)
            .DeactivateOnExit<SurgingWaveSeaFoam>()
            .DeactivateOnExit<SurgingWaveCorridor>()
            .DeactivateOnExit<SurgingWaveFrothingSea>();
        NormalBounds(state2);
        CastEnd(id + 0x60u, 1.6f, "Side")
            .DeactivateOnExit<Strait>();
    }

    private void DeepDiveNormal(uint id, float delay)
    {
        Cast(id, AID.DeepDiveNormal, delay, 5f, "Stack")
            .ActivateOnEnter<DeepDiveNormal>()
            .DeactivateOnExit<DeepDiveNormal>();
    }

    private void TorrentialTridents(uint id, float delay, bool longDelay)
    {
        Cast(id, AID.TorrentialTridents, delay, 4f)
            .ActivateOnEnter<TorrentialTridentAOE>();
        ComponentCondition<TorrentialTridentLanding>(id + 0x10u, 2.8f, static comp => comp.NumCasts > 0)
            .ActivateOnEnter<TorrentialTridentLanding>();
        ComponentCondition<TorrentialTridentLanding>(id + 0x20u, 5f, static comp => comp.NumCasts > 5, "Raidwide x6")
            .DeactivateOnExit<TorrentialTridentLanding>();
        ComponentCondition<TorrentialTridentAOE>(id + 0x30u, longDelay ? 4.1f : 2.1f, static comp => comp.AOEs.Count > 0);
        ComponentCondition<TorrentialTridentAOE>(id + 0x40u, longDelay ? 10 : 8, static comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<TorrentialTridentAOE>(id + 0x50u, 5f, static comp => comp.NumCasts > 5, "Explosions end")
            .DeactivateOnExit<TorrentialTridentAOE>();
    }

    private void StormySeas(uint id, float delay)
    {
        ComponentCondition<Stormwhorl>(id, delay, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Stormwhorl>();
        ComponentCondition<Stormwhorl>(id + 0x10u, 4f, static comp => comp.NumCasts > 0, "Puddles")
            .ActivateOnEnter<Stormwinds>();
        ComponentCondition<Stormwhorl>(id + 0x11u, 2f, static comp => comp.NumCasts > 3);
        ComponentCondition<Stormwhorl>(id + 0x12u, 2f, static comp => comp.NumCasts > 6)
            .DeactivateOnExit<Stormwhorl>();
        ComponentCondition<Stormwinds>(id + 0x20u, 1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Stormwinds>();
    }

    private void DenizensOfTheDeep(uint id, float delay)
    {
        Cast(id, AID.DenizensOfTheDeep, delay, 4f);
        ComponentCondition<SerpentsTide>(id + 0x10u, 6.0f, static comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<SerpentsTide>();
        WindRose(id + 0x20u, 2f);
        ComponentCondition<SerpentsTide>(id + 0x30u, 0.2f, static comp => comp.AOEs.Count == 0, "Lines 1");

        ComponentCondition<SerpentsTide>(id + 0x100u, 3.1f, static comp => comp.AOEs.Count > 0);
        CastStartMulti(id + 0x110u, [AID.LeftStrait, AID.RightStrait], 7);
        ComponentCondition<SerpentsTide>(id + 0x120u, 1.2f, static comp => comp.AOEs.Count == 0, "Lines 2")
            .ActivateOnEnter<Strait>();
        ComponentCondition<SerpentsTide>(id + 0x130u, 2.8f, static comp => comp.AOEs.Count > 0);
        CastEnd(id + 0x140u, 2f, "Side")
            .DeactivateOnExit<Strait>();
        ComponentCondition<SerpentsTide>(id + 0x150u, 6.2f, static comp => comp.AOEs.Count == 0, "Lines 3")
            .DeactivateOnExit<SerpentsTide>();

        ComponentCondition<Maelstrom>(id + 0x200u, 0.6f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Maelstrom>();
        ComponentCondition<Maelstrom>(id + 0x201u, 4f, static comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Maelstrom>();
    }

    private void Godsbane(uint id, float delay)
    {
        Cast(id, AID.Godsbane, delay, 5f);
        ComponentCondition<Godsbane>(id + 2u, 2f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Godsbane>()
            .DeactivateOnExit<Godsbane>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void NavigatorsTridentAdds(uint id, float delay)
    {
        Cast(id, AID.NavigatorsTrident, delay, 6.5f)
            .ActivateOnEnter<DireStraits>();
        ComponentCondition<DireStraits>(id + 0x10u, 1.0f, static comp => comp.NumCasts > 0, "Side 1");
        ComponentCondition<DireStraits>(id + 0x11u, 1.8f, static comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<DireStraits>();

        ComponentCondition<NavigatorsTridentAOE>(id + 0x20u, 1.8f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NavigatorsTridentAOE>()
            .ActivateOnEnter<SurgingWaveFrothingSea>();
        ComponentCondition<NavigatorsTridentAOE>(id + 0x21u, 7f, static comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<SurgingWaveCorridor>() // envcontrol will happen right after knockback end
            .ActivateOnEnter<SerpentsTide>()
            .ActivateOnEnter<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SerpentsTide>(id + 0x30u, 1.2f, static comp => comp.AOEs.Count == 0, "Lines")
            .DeactivateOnExit<SerpentsTide>();
    }

    private void SurgingWaveToTheLast(uint id, float delay)
    {
        var state = Cast(id, AID.SurgingWave, delay, 9f)
            .ActivateOnEnter<SurgingWaveAOE>()
            .ActivateOnEnter<SurgingWaveShockwave>();
        CorridorBounds(state);
        ComponentCondition<SurgingWaveAOE>(id + 0x10u, 1f, static comp => comp.NumCasts > 0)
            .DeactivateOnExit<SurgingWaveAOE>();
        ComponentCondition<SurgingWaveShockwave>(id + 0x11u, 0.2f, static comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SurgingWaveShockwave>();

        CastStart(id + 0x20u, AID.ToTheLast, 3.0f);
        ComponentCondition<ToTheLast>(id + 0x30u, 6.0f, static comp => comp.NumCasts > 0, "Side 1")
            .ActivateOnEnter<ToTheLast>();
        ComponentCondition<ToTheLast>(id + 0x40u, 2.0f, static comp => comp.NumCasts > 1, "Side 2");
        ComponentCondition<ToTheLast>(id + 0x50u, 2.0f, static comp => comp.NumCasts > 2, "Side 3")
            .DeactivateOnExit<ToTheLast>();
        CastStartMulti(id + 0x60u, [AID.LeftStrait, AID.RightStrait], 1.4f);
        var state2 = ComponentCondition<SurgingWaveCorridor>(id + 0x70u, 4.0f, static comp => comp.CorridorDir == default)
            .ActivateOnEnter<Strait>()
            .DeactivateOnExit<SurgingWaveCorridor>()
            .DeactivateOnExit<SurgingWaveFrothingSea>();
        NormalBounds(state2);
        CastEnd(id + 0x80, 2.0f, "Side")
            .DeactivateOnExit<Strait>();
    }

    private void DeepDiveHardWater(uint id, float delay)
    {
        Cast(id, AID.DeepDiveHardWater, delay, 9u, "Stack x3")
            .ActivateOnEnter<DeepDiveHardWater>()
            .DeactivateOnExit<DeepDiveHardWater>();
    }

    private void StormySeasMaelstrom(uint id, float delay)
    {
        ComponentCondition<Stormwhorl>(id, delay, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Stormwhorl>();
        ComponentCondition<Stormwhorl>(id + 0x10u, 4f, static comp => comp.NumCasts > 0, "Puddles")
            .ActivateOnEnter<Stormwinds>();
        ComponentCondition<Stormwhorl>(id + 0x11u, 2f, static comp => comp.NumCasts > 3)
            .ActivateOnEnter<SerpentsTide>();
        ComponentCondition<Stormwhorl>(id + 0x12u, 2f, static comp => comp.NumCasts > 6)
            .DeactivateOnExit<Stormwhorl>();
        ComponentCondition<Stormwinds>(id + 0x20u, 1f, static comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Stormwinds>();
        ComponentCondition<SerpentsTide>(id + 0x30u, 4.4f, static comp => comp.AOEs.Count == 0, "Lines")
            .DeactivateOnExit<SerpentsTide>();

        ComponentCondition<Maelstrom>(id + 0x100u, 0.6f, static comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Maelstrom>();
        WindRoseSeafoamSpiral(id + 0x110u, 2.3f)
            .DeactivateOnExit<Maelstrom>();
    }
}
