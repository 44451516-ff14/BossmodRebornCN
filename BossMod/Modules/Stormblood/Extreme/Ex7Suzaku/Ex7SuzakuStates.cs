namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

class Ex7SuzakuStates : StateMachineBuilder
{
    public Ex7SuzakuStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ScreamsOfTheDamned(id, 6.1f);
        PhoenixDown(id + 0x10000, 8.5f);
        LimitBreakPhase(id + 0x20000, 6.7f);
        Phase2Start(id + 0x30000, 6.2f);
        Hotspot1(id + 0x40000, 12.8f);
        Phase2AfterHotspot1(id + 0x50000, 9.9f);
        Hotspot2(id + 0x60000, 13.9f);
        Phase2AfterHotspot2(id + 0x70000, 9.5f);
        Hotspot3(id + 0x80000, 16.9f);
        Phase2AfterHotspot3(id + 0x90000, 16.9f);
        SimpleState(id + 0xA0000, 23.5f, "Enrage");
    }

    private void ScreamsOfTheDamned(uint id, float delay)
    {
        Cast(id, AID.ScreamsOfTheDamned, delay, 3f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<Rout>()
            .ActivateOnEnter<RekindleP1>();
        ComponentCondition<RekindleP1>(id + 0x10, 9.9f, comp => comp.Spreads.Count != 0, "Spreadmarkers appear");
        ComponentCondition<Rout>(id + 0x20, 3.1f, comp => comp.NumCasts != 0, "Rect AOE")
            .DeactivateOnExit<Rout>();
        ComponentCondition<RekindleP1>(id + 0x30, 2f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .ActivateOnExit<FleetingSummer>()
            .DeactivateOnExit<RekindleP1>();
        ComponentCondition<FleetingSummer>(id + 0x40, 8.9f, comp => comp.NumCasts != 0, "Cone AOE")
            .DeactivateOnExit<FleetingSummer>();
        Cast(id + 0x50, AID.Cremate, 4.6f, 3f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void PhoenixDown(uint id, float delay)
    {
        Cast(id, AID.PhoenixDown, delay, 3f, "Feathers spawn");
        Condition(id + 0x10, 1f, () =>
        {
            var feathers = Module.Enemies((uint)OID.ScarletPlume);
            var count = feathers.Count;
            for (var i = 0; i < count; ++i)
            {
                if (feathers[i].IsTargetable)
                    return true;
            }
            return false;
        }, "Feathers targetable")
            .ActivateOnEnter<RekindleP1>()
            .ActivateOnEnter<ScarletPlumeTailFeather>();
        ComponentCondition<RekindleP1>(id + 0x20, 7.4f, comp => comp.Spreads.Count != 0, "Spreadmarkers appear");
        ComponentCondition<RekindleP1>(id + 0x30, 5.1f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<RekindleP1>();
        Cast(id + 0x40, AID.Cremate, 7.9f, 10.9f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ScarletPlumeTailFeather>(id + 0x50, 1.1f, comp => comp.AOEs.Count == 0, "Feathers resolve")
            .DeactivateOnExit<ScarletPlumeTailFeather>();
        Cast(id + 0x60, AID.ScreamsOfTheDamned, 9.4f, 3f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Condition(id + 0x70, 10.1f, () =>
        {
            var birds = Module.Enemies((uint)OID.ScarletLady);
            var count = birds.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!birds[i].IsDead)
                    return false;
            }
            return true;
        }, "Birds enrage");
        Targetable(id + 0x80, false, 1.4f);
        Targetable(id + 0x90, true, 0.7f);
    }

    private void LimitBreakPhase(uint id, float delay)
    {
        Targetable(id, false, delay)
            .ActivateOnEnter<RapturousEchoTowers>()
            .ActivateOnEnter<ScarletMelody>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<RapturousEchoTowers>(id + 0x10, 20.4f, comp => comp.Towers.Count != 0, "DDR start");
        ComponentCondition<RapturousEchoTowers>(id + 0x20, 31.2f, comp => comp.Towers.Count == 0, "DDR end")
            .DeactivateOnExit<RapturousEchoTowers>()
            .DeactivateOnExit<ScarletMelody>();
        ActorCast(id + 0x30, () => Helper(Module), AID.ScarletFever, 4.9f, 7f, true)
            .ActivateOnEnter<ArenaChange>()
            .SetHint(StateMachine.StateHint.Raidwide);
        static Actor? Helper(BossModule module)
        {
            var helpers = module.Enemies((uint)OID.Helper1);
            var count = helpers.Count;
            for (var i = 0; i < count; ++i)
            {
                var h = helpers[i];
                if (h.CastInfo?.IsSpell(AID.ScarletFever) ?? false)
                    return h;
            }
            return null;
        }
        Condition(id + 0x40, 0.7f, () => Module.Arena.Bounds == Ex7Suzaku.Phase2Bounds, "Arena changes")
            .DeactivateOnExit<ArenaChange>();
        Targetable(id + 0x50, true, 4f)
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void Phase2Start(uint id, float delay)
    {
        SouthronStar(id, delay)
            .ActivateOnExit<MesmerizingMelody>();
        ComponentCondition<MesmerizingMelody>(id + 0x10, 11.1f, comp => comp.NumCasts != 0, "Pull")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<MesmerizingMelody>()
            .ActivateOnEnter<RekindleP2>();
        RekindleWellOfFlameScathingNet(id + 0x20, 3.1f);
        PhantomFlurry(id + 0x70, 1f)
            .ActivateOnExit<Hotspot>();
    }

    private void Hotspot1(uint id, float delay)
    {
        ComponentCondition<Hotspot>(id, delay, comp => comp.AOEs.Count == 16, "Panels");
        float[] delays = [11.7f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 10.7f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f];
        for (var i = 0; i < 16; ++i)
        {
            var casts = i + 1;
            var condition = ComponentCondition<Hotspot>(id + (uint)(casts * 0x10), delays[i], comp => comp.NumCasts == casts, $"Hotspot {casts}");
            if (i == 15)
            {
                condition.DeactivateOnExit<Hotspot>();
            }
        }
    }

    private void Hotspot2(uint id, float delay)
    {
        ComponentCondition<Hotspot>(id, delay, comp => comp.AOEs.Count == 16, "Panels");
        MesmerizingMelodyRuthlessRefrain(id + 0x10, 1.6f);
        float[] delays = [6.1f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 5.4f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f, 1.25f];
        for (var i = 0; i < 8; ++i)
        {
            var casts = i + 1;
            ComponentCondition<Hotspot>(id + 0x10 + (uint)(casts * 0x10), delays[i], comp => comp.NumCasts == casts, $"Hotspot {casts}");
        }
        Cast(id + 0xA0, AID.SouthronStar, 1.4f, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        for (var i = 8; i < 16; ++i)
        {
            var casts = i + 1;
            var condition = ComponentCondition<Hotspot>(id + 0xA0 + (uint)((casts - 8) * 0x10), delays[i], comp => comp.NumCasts == casts, $"Hotspot {casts}");
            if (i == 15)
            {
                condition.DeactivateOnExit<Hotspot>();
            }
        }
    }

    private void Hotspot3(uint id, float delay)
    {
        ComponentCondition<Hotspot>(id, delay, comp => comp.AOEs.Count == 8, "Panels")
            .ActivateOnEnter<RekindleP2>();
        ComponentCondition<RekindleP2>(id + 0x10, 2.9f, comp => comp.Spreads.Count != 0, "Spreadmarkers appear")
            .ActivateOnExit<WellOfFlame>();
        ComponentCondition<WellOfFlame>(id + 0x20, 4.1f, comp => comp.NumCasts != 0, "Frontal AOE")
            .DeactivateOnExit<WellOfFlame>();
        ComponentCondition<RekindleP2>(id + 0x30, 1f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<RekindleP2>();
        float[] delays = [6.1f, 1.25f, 1.25f, 1.25f, 0.45f, 1.25f, 1.25f, 0.25f, 5.3f, 1.25f, 1.25f, 1.25f, 0.65f, 1.25f, 1.25f, 1.25f];
        for (var i = 0; i < 4; ++i)
        {
            var casts = i + 1;
            var condition = ComponentCondition<Hotspot>(id + 0x30 + (uint)(casts * 0x10), delays[i], comp => comp.NumCasts == casts, $"Hotspot {casts}");
            if (i == 3)
            {
                condition.ActivateOnExit<PayThePiperHotspotCombo>();
            }
        }
        ComponentCondition<PayThePiperHotspotCombo>(id + 0x80, 5f, comp => comp.State.Count != 0, "Forced march tethers");
        ComponentCondition<PayThePiperHotspotCombo>(id + 0x90, 10.3f, comp => comp.NumActiveForcedMarches != 0, "Forced march starts");
        for (var i = 4; i < 8; ++i)
        {
            var casts = i + 1;
            ComponentCondition<Hotspot>(id + 0x90 + (uint)((casts - 4) * 0x10), delays[i], comp => comp.NumCasts == casts, $"Hotspot {casts}");
            if (i == 6)
            {
                ComponentCondition<PayThePiperHotspotCombo>(id + 0xC1, 1f, comp => comp.NumActiveForcedMarches == 0, "Forced march ends");
            }
        }
        MesmerizingMelodyRuthlessRefrain(id + 0xE0, 5.8f)
            .DeactivateOnEnter<PayThePiperHotspotCombo>();
        for (var i = 8; i < 12; ++i)
        {
            var casts = i + 1;
            ComponentCondition<Hotspot>(id + 0xE0 + (uint)((casts - 8) * 0x10), delays[i], comp => comp.NumCasts + 8 == casts, $"Hotspot {casts}");
        }
        PhantomFlurry(id + 0x130, 3.2f);
        for (var i = 12; i < 16; ++i)
        {
            var casts = i + 1;
            var condition = ComponentCondition<Hotspot>(id + 0x160 + (uint)((casts - 12) * 0x10), delays[i], comp => comp.NumCasts + 8 == casts, $"Hotspot {casts}");
            if (i == 15)
            {
                condition.DeactivateOnExit<Hotspot>();
            }
        }
    }

    private void Phase2AfterHotspot1(uint id, float delay)
    {
        MesmerizingMelodyRuthlessRefrain(id, delay);
        Cast(id + 0x10, AID.CloseQuarterCrescendo, 7.2f, 4f)
            .ActivateOnEnter<PayThePiperRegular>();
        ComponentCondition<PayThePiperRegular>(id + 0x20, 2.5f, comp => comp.State.Count != 0, "Forced march tethers");
        ComponentCondition<PayThePiperRegular>(id + 0x30, 10.3f, comp => comp.NumActiveForcedMarches != 0, "Forced march starts");
        ComponentCondition<PayThePiperRegular>(id + 0x40, 4f, comp => comp.NumActiveForcedMarches == 0, "Forced march ends")
            .DeactivateOnExit<PayThePiperRegular>()
            .ActivateOnExit<RekindleP2>();
        RekindleWellOfFlameScathingNet(id + 0x50, 0.5f);
        PhantomFlurry(id + 0xA0, 4f, 2.6f)
            .ActivateOnExit<Hotspot>();
    }

    private void Phase2AfterHotspot2(uint id, float delay)
    {
        PhantomFlurry(id, delay);
        SouthronStar(id + 0x40, 6.7f)
            .ActivateOnExit<RuthlessRefrain>()
            .ActivateOnExit<IncandescentInterlude>()
            .ActivateOnExit<RekindleP2>();
        ComponentCondition<IncandescentInterlude>(id + 0x50, 14.3f, comp => comp.TowerCache.Count != 0, "Towers appear");
        ComponentCondition<RekindleP2>(id + 0x60, 4.8f, comp => comp.Spreads.Count != 0, "Spreadmarkers appear");
        ComponentCondition<RuthlessRefrain>(id + 0x70, 2.5f, comp => comp.NumCasts != 0, "Knockback")
            .SetHint(StateMachine.StateHint.Knockback)
            .DeactivateOnExit<RuthlessRefrain>();
        ComponentCondition<IncandescentInterlude>(id + 0x80, 2.3f, comp => comp.Towers.Count == 0, "Towers resolve")
            .DeactivateOnExit<IncandescentInterlude>();
        ComponentCondition<RekindleP2>(id + 0x90, 0.25f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .ActivateOnExit<WellOfFlame>()
            .DeactivateOnExit<RekindleP2>();
        ComponentCondition<WellOfFlame>(id + 0xA0, 4.8f, comp => comp.NumCasts != 0, "Frontal AOE")
            .DeactivateOnExit<WellOfFlame>()
            .ActivateOnExit<Hotspot>();
    }

    private void Phase2AfterHotspot3(uint id, float delay)
    {
        SouthronStar(id, delay);
        MesmerizingMelodyRuthlessRefrain(id + 0x10, 9.1f)
            .ActivateOnExit<RekindleP2>();
        RekindleWellOfFlameScathingNet(id + 0x20, 8.2f);
        PhantomFlurry(id + 0x70, 1f);
        SouthronStar(id + 0x80, 5.1f);
        MesmerizingMelodyRuthlessRefrain(id + 0x90, 9.1f);
        SouthronStar(id + 0xA0, 9.2f);
        PhantomFlurry(id + 0xB0, 2.6f);
    }

    private State PhantomFlurry(uint id, float delay, float delay2 = 1.6f)
    {
        Cast(id, AID.PhantomFlurryVisual, delay, 4f)
                .ActivateOnEnter<PhantomFlurryAOE>()
                .ActivateOnEnter<PhantomFlurryTB>();
        ComponentCondition<PhantomFlurryTB>(id + 0x10, 0.1f, comp => comp.NumCasts == 1, "Tank swap")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<PhantomFlurryAOE>(id + 0x20, 6.1f, comp => comp.NumCasts != 0, "Frontal AOE")
            .DeactivateOnExit<PhantomFlurryAOE>();
        return ComponentCondition<PhantomFlurryTB>(id + 0x30, delay2, comp => comp.NumCasts == 2)
            .DeactivateOnExit<PhantomFlurryTB>();
    }

    private State MesmerizingMelodyRuthlessRefrain(uint id, float delay)
    {
        return CastMulti(id, [AID.MesmerizingMelody, AID.RuthlessRefrain], delay, 4f, "Knockback OR Pull")
            .SetHint(StateMachine.StateHint.Knockback)
            .ActivateOnEnter<MesmerizingMelody>()
            .ActivateOnEnter<RuthlessRefrain>()
            .DeactivateOnExit<MesmerizingMelody>()
            .DeactivateOnExit<RuthlessRefrain>();
    }

    private State SouthronStar(uint id, float delay)
    {
        return Cast(id, AID.SouthronStar, delay, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State RekindleWellOfFlameScathingNet(uint id, float delay)
    {
        ComponentCondition<RekindleP2>(id, delay, comp => comp.Spreads.Count != 0, "Spreadmarkers appear")
            .ActivateOnExit<WellOfFlame>();
        ComponentCondition<WellOfFlame>(id + 0x10, 4.2f, comp => comp.NumCasts != 0, "Frontal AOE")
            .DeactivateOnExit<WellOfFlame>();
        ComponentCondition<RekindleP2>(id + 0x20, 0.9f, comp => comp.Spreads.Count == 0, "Spreads resolve")
            .DeactivateOnExit<RekindleP2>()
            .ActivateOnExit<ScathingNet>();
        ComponentCondition<ScathingNet>(id + 0x30, 1.2f, comp => comp.Stacks.Count != 0, "Stackmarker appears");
        return ComponentCondition<ScathingNet>(id + 0x40, 5.1f, comp => comp.Stacks.Count == 0, "Stack resolves")
            .DeactivateOnExit<ScathingNet>();
    }
}
