namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

sealed class BA1OwainStates : StateMachineBuilder
{
    public BA1OwainStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<ElementalMagicks>()
            .ActivateOnEnter<PiercingLight1>()
            .ActivateOnEnter<PiercingLight2>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<Spiritcull>()
            .ActivateOnEnter<IvoryPalm>()
            .ActivateOnEnter<IvoryPalmExplosion>()
            .ActivateOnEnter<EurekanAero>();
    }
    // all timings seem to have upto 1s variation
    private void SinglePhase(uint id)
    {
        Thricecull(id, 10f);
        AcallamNaSenorach(id + 0x10000u, 7.1f);
        Mythcall(id + 0x20000u, 7.9f);
        Thricecull(id + 0x30000u, 10f);
        AcallamNaSenorach(id + 0x40000u, 7f);
        ElementalShift(id + 0x50000u, 7.5f);
        Thricecull(id + 0x60000u, 11f);
        Spiritcull(id + 0x70000u, 8.3f);
        // from now on repeats until wipe or victory, this extends timeline until up around 20min since its theoretically possible to solo it as long as Owain is pulled
        for (var i = 0; i < 12; ++i)
        {
            var pid = (uint)(i * 0x10000u);
            Thricecull(id += 0x80000u + pid, 5.1f);
            AcallamNaSenorach(id += 0x90000u + pid, 6f);
            PiercingLight2(id += 0xA0000u + pid, 6.1f);
            AcallamNaSenorach(id += 0xB0000u + pid, 9.7f);
            AcallamNaSenorach(id += 0xC0000u + pid, 5.1f);
            ElementalShiftSpiritcull(id + 0xD0000u + pid, 8.3f);
            Thricecull(id += 0xE0000u + pid, 9.3f);
            AcallamNaSenorach(id += 0xF0000u + pid, 17.2f);
            IvoryPalmElementalMagicks(id += 0x100000u + pid, 1.2f);
            Spiritcull(id += 0x110000u + pid, 5.1f);
        }
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Thricecull(uint id, float delay)
    {
        Cast(id, (uint)AID.Thricecull, delay, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void AcallamNaSenorach(uint id, float delay)
    {
        Cast(id, (uint)AID.AcallamNaSenorach, delay, 5f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Mythcall(uint id, float delay)
    {
        Cast(id, (uint)AID.Mythcall, delay, 2f, "Spawn spears");
        Cast(id + 0x10u, (uint)AID.ElementalShift1, 2.2f, 2, "Switch elements");
        CastMulti(id + 0x20u, [(uint)AID.ElementalMagicksIceBoss, (uint)AID.ElementalMagicksFireBoss], 6.3f, 5f, "Circle AOEs");
    }

    private void ElementalShift(uint id, float delay)
    {
        Cast(id, (uint)AID.ElementalShift1, delay, 2f, "Switch elements");
        CastMulti(id + 0x10u, [(uint)AID.ElementalMagicksIceBoss, (uint)AID.ElementalMagicksFireBoss], 6.4f, 5f, "Circle AOEs");
    }

    private void Spiritcull(uint id, float delay)
    {
        Cast(id, (uint)AID.Spiritcull, delay, 3f, "Dorito stacks appear")
            .ActivateOnEnter<LegendaryImbas>();
        ComponentCondition<LegendaryImbas>(id + 0x10u, 0.1f, comp => comp.Casters.Count != 0, "Spreads appear")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LegendaryImbas>();
        ComponentCondition<PiercingLight1>(id + 0x20u, 5, comp => comp.Spreads.Count == 0, "Spreads and dorito stacks resolve");
    }

    private void PiercingLight2(uint id, float delay)
    {
        ComponentCondition<PiercingLight2>(id, delay, comp => comp.Spreads.Count != 0, "Spreads appear");
        CastStart(id + 0x10u, (uint)AID.Pitfall, 1f, "Proximity AOE");
        ComponentCondition<PiercingLight2>(id + 0x20u, 3.8f, comp => comp.Spreads.Count == 0, "Spreads resolve");
        CastEnd(id + 0x30u, 1f, "Proximity AOE resolves");
    }

    private void ElementalShiftSpiritcull(uint id, float delay)
    {
        Cast(id, (uint)AID.ElementalShift1, delay, 2f, "Switch elements");
        Cast(id + 0x10u, (uint)AID.Spiritcull, 6.3f, 3f, "Dorito stacks appear")
            .ActivateOnEnter<LegendaryImbas>();
        ComponentCondition<LegendaryImbas>(id + 0x20u, 1.1f, comp => comp.Casters.Count != 0, "Spreads appear")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<LegendaryImbas>();
        CastStartMulti(id + 0x30u, [(uint)AID.ElementalMagicksIceBoss, (uint)AID.ElementalMagicksFireBoss], 2, "Circle AOEs start");
        ComponentCondition<PiercingLight1>(id + 0x40u, 3f, comp => comp.Spreads.Count == 0, "Spreads resolve");
        ComponentCondition<ElementalMagicks>(id + 0x50u, 2f, comp => comp.AOEs.Count == 0, "Circles resolve");
    }

    private void IvoryPalmElementalMagicks(uint id, float delay)
    {
        ComponentCondition<IvoryPalm>(id, delay, comp => comp.Tethers.Count != 0, "Hands spawn");
        Cast(id + 0x10u, (uint)AID.ElementalShift1, 6.4f, 2f, "Switch elements");
        CastMulti(id + 0x20u, [(uint)AID.ElementalMagicksIceBoss, (uint)AID.ElementalMagicksFireBoss], 6.3f, 5f, "Circle AOEs");
        Cast(id + 0x30u, (uint)AID.Thricecull, 6f, 5f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<IvoryPalmExplosion>(id + 0x40u, 5f, comp => comp.Casters.Count == 0, "Hands soft enrage"); // quite some timing variation here since hands could be killed at any time
    }
}
