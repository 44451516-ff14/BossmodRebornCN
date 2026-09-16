namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

sealed class BA1ArtStates : StateMachineBuilder
{
    public BA1ArtStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<Thricecull>()
            .ActivateOnEnter<AcallamNaSenorach>()
            .ActivateOnEnter<LegendMythSpinnerCarver>()
            .ActivateOnEnter<DefilersDeserts>()
            .ActivateOnEnter<DefilersDesertsPredict>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<LegendaryGeasAOE>()
            .ActivateOnEnter<LegendaryGeasStay>()
            .ActivateOnEnter<GloryUnearthed>()
            .ActivateOnEnter<PiercingDark>();
    }
    // all timings seem to have upto 1s variation
    private void SinglePhase(uint id)
    {
        Thricecull(id, 10f);
        LegendCarverSpinner(id + 0x10000u);
        AcallamNaSenorach(id + 0x20000u, 4.1f);
        Mythcall(id + 0x30000u, 7f);
        AcallamNaSenorach(id + 0x40000u, 3.2f);
        Thricecull(id + 0x50000u, 3.6f);
        Mythcall(id + 0x60000u, 5f);
        // from now on repeats until wipe or victory, this extends timeline until up around 20min since its theoretically possible to solo it as long as Owain is pulled
        for (var i = 0u; i < 10u; ++i)
        {
            var pid = i * 0x10000u;
            LegendaryGeas(id += 0x70000u + pid, 3.1f);
            AcallamNaSenorach(id += 0x80000u + pid, 4.5f);
            GloryUnearthedPitfall(id += 0x90000u + pid, 4.5f);
            Thricecull(id += 0xA0000u + pid, 5f);
            AcallamNaSenorach(id += 0xB0000u + pid, 3.2f);
            Mythcall(id += 0xC0000u + pid, 7.8f);
            Thricecull(id += 0xD0000u + pid, 3f);
            AcallamNaSenorach(id += 0xE0000u + pid, 3.2f);
            Mythcall2(id += 0xF0000u + pid, 6f);
        }
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Thricecull(uint id, float delay)
    {
        Cast(id, AID.Thricecull, delay, 4f, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void LegendCarverSpinner(uint id)
    {
        CastMulti(id, [AID.Legendcarver, AID.Legendspinner], 3.5f, 4.5f, "In/Out AOE");
        CastMulti(id + 0x10u, [AID.Legendcarver, AID.Legendspinner], 4, 4.5f, "Inverse of previous AOE");
    }

    private void AcallamNaSenorach(uint id, float delay)
    {
        Cast(id, AID.AcallamNaSenorach, delay, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Mythcall(uint id, float delay)
    {
        Cast(id, AID.Mythcall, delay, 2f, "Spawn spears");
        CastMulti(id + 0x10u, [AID.Legendcarver, AID.Legendspinner], 6f, 4.5f, "In/Out AOE");
        ComponentCondition<LegendMythSpinnerCarver>(id + 0x20, 3f, static comp => comp.AOEs.Count == 0, "Spears repeat AOE");
    }

    private void Mythcall2(uint id, float delay)
    {
        Cast(id, AID.Mythcall, delay, 2f, "Spawn spears");
        Cast(id + 0x10u, AID.PiercingDarkVisual, 6.1f, 2.5f, "Spreads");
        CastMulti(id + 0x20u, [AID.Legendcarver, AID.Legendspinner], 1.6f, 4.5f, "In/Out AOE");
        ComponentCondition<PiercingDark>(id + 0x30u, 0.4f, static comp => comp.ActiveSpreads.Count == 0, "Spreads resolve");
        ComponentCondition<LegendMythSpinnerCarver>(id + 0x40u, 3f, static comp => comp.AOEs.Count == 0, "Spears repeat AOE");
    }

    private void LegendaryGeas(uint id, float delay)
    {
        Cast(id, AID.LegendaryGeas, delay, 4f, "Circle AOE + stop moving");
        ComponentCondition<LegendaryGeasStay>(id + 0x10u, 3f, static comp => comp.PlayerStates[0].Requirement == Components.StayMove.Requirement.None, "Can move again + cross");
        ComponentCondition<DefilersDeserts>(id + 0x20u, 0.5f, static comp => comp.Casters.Count != 0, "Crosses");
    }

    private void GloryUnearthedPitfall(uint id, float delay)
    {
        ComponentCondition<GloryUnearthed>(id, delay, static comp => comp.Chasers.Count != 0, "Chasing AOE start");
        CastStart(id + 0x10u, AID.Pitfall, 2.5f, "Proximity AOE start");
        CastEnd(id + 0x20u, 5f, "Proximity AOE resolve");
        ComponentCondition<GloryUnearthed>(id + 0x30u, 3.7f, static comp => comp.Chasers.Count == 0, "Chasing AOE ends");
    }
}
