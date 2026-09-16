namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

sealed class Ex1ZodiarkStates : StateMachineBuilder
{
    public Ex1ZodiarkStates(BossModule module) : base(module)
    {
        DeathPhase(0u, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID.Kokytos, 6.1f, 4f, "Kokytos");
        Paradeigma1(id + 0x010000u, 7.2f);
        Ania(id + 0x020000u, 2.7f);
        Exoterikos1(id + 0x030000u, 4.1f);
        Paradeigma2(id + 0x040000u, 9.2f);
        Phobos(id + 0x050000u, 7.2f);
        Paradeigma3(id + 0x060000u, 7.2f);
        Ania(id + 0x070000u, 3f);
        Paradeigma4(id + 0x080000u, 3.2f);

        Intermission(id + 0x100000u, 9.5f);
        AstralEclipse(id + 0x110000u, 6.1f, true);

        Paradeigma5(id + 0x200000u, 10.1f);
        Ania(id + 0x210000u, 9f);
        Exoterikos4(id + 0x220000u, 6.2f);
        Paradeigma6(id + 0x230000u, 10.2f);
        TrimorphosExoterikos(id + 0x240000u, 0.6f, true);
        AstralEclipse(id + 0x250000u, 8.5f, false);

        Ania(id + 0x300000u, 7.2f);
        Paradeigma7(id + 0x310000u, 6.2f);
        Exoterikos6(id + 0x320000u, 2.5f);
        Paradeigma8(id + 0x330000u, 5.1f);
        Phobos(id + 0x340000u, 4.9f);
        TrimorphosExoterikos(id + 0x350000u, 10.2f, false);
        Styx(id + 0x360000u, 3.2f, 9);
        Paradeigma9(id + 0x370000u, 0.4f);
        Cast(id + 0x380000u, AID.Enrage, 3.5f, 8f, "Enrage");
    }

    private void Ania(uint id, float delay)
    {
        Cast(id, AID.Ania, delay, 4f)
            .ActivateOnEnter<Ania>();
        ComponentCondition<Ania>(id + 2u, 1f, static comp => comp.Done, "Tankbuster")
            .DeactivateOnExit<Ania>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Phobos(uint id, float delay)
    {
        Cast(id, AID.Phobos, delay, 4f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State Algedon(uint id, float delay, bool setPosFlags)
    {
        CastStartMulti(id, [AID.AlgedonTL, AID.AlgedonTR], delay)
            .SetHint(StateMachine.StateHint.PositioningStart, setPosFlags);
        CastEnd(id + 1u, 7f)
            .ActivateOnEnter<Algedon>();
        return ComponentCondition<Algedon>(id + 2u, 1f, static comp => comp.Done, "Diagonal")
            .DeactivateOnExit<Algedon>()
            .SetHint(StateMachine.StateHint.PositioningEnd, setPosFlags);
    }

    private State Adikia(uint id, float delay)
    {
        Cast(id, AID.Adikia, delay, 6f)
            .ActivateOnEnter<Adikia>();
        return ComponentCondition<Adikia>(id + 0x10u, 1.7f, static comp => comp.Done, "SideSmash")
            .DeactivateOnExit<Adikia>();
    }

    private State Styx(uint id, float delay, int numHits)
    {
        CastStart(id, AID.Styx, delay)
            .ActivateOnEnter<Styx>();
        CastEnd(id + 1u, 5f, "Stack");
        return ComponentCondition<Styx>(id + 0x10u, 1.1f * numHits - 0.1f, comp => comp.NumCasts >= numHits, "Stack resolve", 2)
            .DeactivateOnExit<Styx>();
    }

    // note that exoterikos component is optionally activated, but unconditionally deactivated
    private State TripleEsotericRay(uint id, float delay, bool startExo, bool setPosFlags)
    {
        Cast(id, AID.TripleEsotericRay, delay, 7f, "TripleRay")
            .ActivateOnEnter<Exoterikos>(startExo)
            .SetHint(StateMachine.StateHint.PositioningStart, setPosFlags);
        return ComponentCondition<Exoterikos>(id + 0x10u, 3.1f, static comp => comp.Done, "TripleRay resolve")
            .DeactivateOnExit<Exoterikos>()
            .SetHint(StateMachine.StateHint.PositioningEnd, setPosFlags);
    }

    // this is used by various paradeigma states; the state activates component
    private void ParadeigmaStart(uint id, float delay, string name)
    {
        Cast(id, AID.Paradeigma, delay, 3f, name)
            .ActivateOnEnter<Paradeigma>();
    }

    // this is used by various paradeigma states; automatically deactivates paradeigma component
    private State AstralFlow(uint id, float delay)
    {
        CastStartMulti(id, [AID.AstralFlowCW, AID.AstralFlowCCW], delay);
        CastEnd(id + 1u, 10f, "Rotate")
            .SetHint(StateMachine.StateHint.PositioningStart);
        return Condition(id + 0x10u, 6.2f, () =>
        {
            var raid = Module.WorldState.Party.WithoutSlot(false, true, true);
            var len = raid.Length;
            for (var i = 0; i < len; ++i)
            {
                if (raid[i].FindStatus((uint)SID.TenebrousGrasp) != null)
                {
                    return false;
                }
            }
            return true;
        }, "Rotate resolve", 5f, 1f)
            .DeactivateOnExit<Paradeigma>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // this is used by various exoterikos states; the state activates component
    private void ExoterikosStart(uint id, float delay, string name)
    {
        Cast(id, AID.ExoterikosGeneric, delay, 5f, name)
            .ActivateOnEnter<Exoterikos>();
    }

    private void Paradeigma1(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para1 (4 birds)");
        Styx(id + 0x1000u, 11.2f, 6)
            .DeactivateOnEnter<Paradeigma>(); // TODO: paradeigma should hide itself when done, then we can deactivate it on state exit...
    }

    private void Exoterikos1(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo1 (side tri)");
        Cast(id + 0x1000u, AID.ExoterikosFront, 2.1f, 7, "Exo2 (front)")
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma2(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para2 (birds/behemoths)");
        Algedon(id + 0x1000u, 5.2f, true)
            .DeactivateOnExit<Paradeigma>();
    }

    private void Paradeigma3(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para3 (snakes)");
        ExoterikosStart(id + 0x1000u, 2.1f, "Exo3 (side)");
        AstralFlow(id + 0x2000u, 2.2f)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma4(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para4 (snakes side)");
        Adikia(id + 0x1000u, 4.1f)
            .DeactivateOnExit<Paradeigma>();
    }

    private void Paradeigma5(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para5 (birds/behemoths)");
        AstralFlow(id + 0x1000u, 5.2f);
    }

    private void Exoterikos4(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo4 (side sq)");
        Algedon(id + 0x1000u, 2.1f, true)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma6(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para6 (4 birds + snakes)");
        AstralFlow(id + 0x1000u, 5.2f);
        Styx(id + 0x2000u, 0f, 7); // note: cast starts slightly before flow resolve
    }

    private void TrimorphosExoterikos(uint id, float delay, bool first)
    {
        Cast(id, AID.TrimorphosExoterikos, delay, 13f, "TriExo")
            .ActivateOnEnter<Exoterikos>();

        var followup = first ? Adikia(id + 0x1000u, 6.2f) : Algedon(id + 0x1000u, 5.2f, true);
        followup.DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma7(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para7 (snakes)");
        ExoterikosStart(id + 0x1000u, 2.1f, "Exo5 (side)");
        AstralFlow(id + 0x2000u, 2.2f)
            .DeactivateOnExit<Exoterikos>();
        Cast(id + 0x3000u, AID.Phlegeton, 0f, 2.9f, "Puddles") // note: 3s cast starts ~0.1s before flow resolve...
            .ActivateOnEnter<Phlegethon>();
        Styx(id + 0x4000u, 2.2f, 8)
            .DeactivateOnExit<Phlegethon>(); // resolve happens mid cast
    }

    private void Exoterikos6(uint id, float delay)
    {
        ExoterikosStart(id, delay, "Exo6 (side)");
        TripleEsotericRay(id + 0x1000u, 2.1f, false, true);
    }

    private void Paradeigma8(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para8 (birds/behemoths)");
        ExoterikosStart(id + 0x1000u, 2.1f, "Exo7 (back sq)");
        AstralFlow(id + 0x2000u, 2.1f)
            .DeactivateOnExit<Exoterikos>();
    }

    private void Paradeigma9(uint id, float delay)
    {
        ParadeigmaStart(id, delay, "Para9 (4 birds + snakes)");
        ExoterikosStart(id + 0x1000u, 2.1f, "Exo8 (side/back?)");
        AstralFlow(id + 0x2000u, 2.1f)
            .DeactivateOnExit<Exoterikos>();
        Styx(id + 0x3000u, 0f, 9); // note: cast starts right as flow resolve happens
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Intermission start")
            .ClearHint(StateMachine.StateHint.DowntimeStart); // adds appear almost immediately, so there is no downtime
        CastStartMulti(id + 0x1000u, [AID.AddsEndFail, AID.AddsEndSuccess], 40f, "Add enrage")
            .ActivateOnEnter<Exoterikos>()
            .DeactivateOnExit<Exoterikos>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        CastEnd(id + 0x2000u, 1.1f);
        ComponentCondition<Apomnemoneumata>(id + 0x3000u, 11.5f, static comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Apomnemoneumata>()
            .DeactivateOnExit<Apomnemoneumata>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 0x4000u, true, 10.6f, "Intermission end");
    }

    private void AstralEclipse(uint id, float delay, bool first)
    {
        Cast(id, AID.AstralEclipse, delay, 5f, "Eclipse")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        Targetable(id + 0x1000u, true, 12.1f, "Boss reappear", 1f)
            .ActivateOnEnter<AstralEclipse>()
            .SetHint(StateMachine.StateHint.PositioningStart);

        //  5.1s first explosion
        //  8.2s triple ray cast start
        //  9.2s second explosion
        // 13.2s third explosion
        // 15.2s triple ray cast end
        // 15.3s ray 1
        // 18.3s ray 2
        // -or-
        //  5.1s first explosion
        //  9.2s second explosion
        // 10.7s algedon cast start
        // 13.2s third explosion
        // 17.7s algedon cast end
        // 18.7s algedon aoe
        var followup = first ? TripleEsotericRay(id + 0x2000u, 8.2f, true, false) : Algedon(id + 0x2000u, 10.6f, false);
        followup.DeactivateOnExit<AstralEclipse>();
        followup.SetHint(StateMachine.StateHint.PositioningEnd);
    }
}
