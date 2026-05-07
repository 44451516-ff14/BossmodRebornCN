namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

[SkipLocalsInit]

sealed class Ex8EnuoStates : StateMachineBuilder
{
    private readonly Ex8Enuo _module;
    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        _module = module;
        SimplePhase(default, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.AllForNaught) ?? false);
        SimplePhase(2u, AddPhase, "Adds")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<LoomingShadowAdd>()
            .ActivateOnEnter<AggressiveShadowAdd>()
            .ActivateOnEnter<SupportShadowAdds>()
            .ActivateOnEnter<LoomingEmptinessKB>()
            .ActivateOnEnter<LoomingEmptinessKillZone>()
            .ActivateOnEnter<EmptyShadowTower>()
            .ActivateOnEnter<VoidalTurbulanceCone>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<DrainTouch>()
            .ActivateOnEnter<WeightofNothing>()
            .ActivateOnEnter<CurseoftheFlesh>()
            .ActivateOnEnter<BeaconAdd>()
            .ActivateOnEnter<Nothingness>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.LightlessWorldCastbar) ?? false);
        SimplePhase(3u, Phase2, "P2")
            .ActivateOnEnter<ArenaChanges>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed;
    }

    private void Phase1(uint id)
    {
        Meteorain(id, 9.16f);
        NaughtGrowsSingle(id + 0x010000, 7.44f);
        NaughtWakesSimple(id + 0x010002, 7.23f);
        Meltdown(id + 0x020000, 2.09f);
        Emptiness(id + 0x030000, 8.34f);
        NaughtGrowsSingle(id + 0x040000, 3.46f);
        GazeOfTheVoid(id + 0x050000, 10.52f);
        Vacuum(id + 0x060000, 24.2f);
        Emptiness(id + 0x070000, 8.65f);
        DeepFreeze(id + 0x080000, 6.12f);
        Meteorain(id + 0x090000, 3.24f);
        SimpleState(id + 0x100000, 8.87f, "Adds");
    }

    private void AddPhase(uint id)
    {
        Cast(id, (uint)AID.AllForNaught, 0f, 5f, "Add Transition");
        ActorCast(id + 0x01, _module.CastingAdd, (uint)AID.LoomingEmptinessKnockback, 14.223f, 5.0f, default, "Knockback");
        ActorCast(id + 0x02, _module.CastingAdd, (uint)AID.VoidalTurbulenceCastBar, 3.229f, 7f, default, "Cones + Towers");
        ActorCast(id + 0x03, _module.CastingAdd, (uint)AID.VoidalTurbulenceCastBar, 13.607f, 7f, default, "Cones + Towers (again)"); // Timing on this one could be 'fun'
        SimpleState(id + 0x04, 60f, "Add Enrage?"); // I have no idea when this would actually go off.
    }

    private void Phase2(uint id)
    {
        LightlessWorld(id, 96.84f);
        Almagest(id + 0x01, 10.18f);
        DoubleNaughtGrows(id + 0x02, 5.82f);
        NaughtWakesActive(id + 0x03, 7.29f);
        ShroudedHoly(id + 0x04, 19.48f);
        DoubleNaughtGrows(id + 0x05, 8.52f);
        DimensionZero(id + 0x06, 7.17f);
        VacuumMeltdown(id + 0x07, 10.97f);
        GazeOfTheVoid(id + 0x09, 13.32f);
        Almagest(id + 0x10, 28.30f);
        NaughtWakesActive(id + 0x11, 8.43f);
        NaughtHunts(id + 0x12, 2.12f);
        Emptiness(id + 0x13, 23.27f);
        NaughtHunts(id + 0x14, 6.26f);
        Emptiness(id + 0x15, 23.24f);
        DimensionZero(id + 0x17, 5.2f);
        Almagest(id + 0x17, 10.1f);
        DoubleNaughtGrows(id + 0x18, 5.7f);
        NaughtWakesActive(id + 0x19, 7.2f);
        DeepFreeze(id + 0x20, 19.4f);
        DoubleNaughtGrows(id + 0x21, 7f); //? Timing unknown
        DimensionZero(id + 0x22, 7f); //? timing unknown
        Meteorain(id + 0x23, 7f); //? timing unknown
        Meteorain(id + 0x24, 7f); //? timing unknown

    }

    private void Meteorain(uint id, float delay)
    {
        Cast(id, (uint)AID.Meteorain, delay, 5f, "Meteorain")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsWildCharge>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsCircle>();
    }

    private void NaughtGrowsSingle(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtGrows, delay, 7f, "Naught Grows (Single)");
    }

    private void NaughtWakesSimple(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtWakes, delay, 2f, "Naught Wakes");
    }

    private void Meltdown(uint id, float delay)
    {
        Cast(id, (uint)AID.Meltdown, delay, 4f, "Meltdown")
            .ActivateOnEnter<MeltdownAoE>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownWait>();
    }

    private void Emptiness(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.AiryEmptiness, (uint)AID.DenseEmptiness], delay, 4f, "Dense/Airy Emptiness")
            .ActivateOnEnter<DenseAiryEmptiness>();
    }

    private void GazeOfTheVoid(uint id, float delay)
    {
        Cast(id, (uint)AID.GazeOfTheVoid, delay, 6f, "Gaze of the Void")
            .ActivateOnEnter<GazeOfTheVoidAOE>()
            .ActivateOnEnter<GazeOfTheVoidSoaks>();
    }

    private void Vacuum(uint id, float delay)
    {
        Cast(id, (uint)AID.Vacuum, delay, 2.0f, "Vacuum")
            .ActivateOnEnter<VacuumAOE>()
            .ActivateOnEnter<VacuumArc1>()
            .ActivateOnEnter<VacuumArc2>()
            .ActivateOnEnter<VacuumArc3>()
            .ActivateOnEnter<VacuumTelegraph>();
    }

    private void DeepFreeze(uint id, float delay)
    {
        Cast(id, (uint)AID.DeepFreezeCastBar, delay, 5f, "Deep Freeze")
            .ActivateOnEnter<DeepFreezeFlares>()
            .ActivateOnEnter<DeepFreeze>();
    }

    private void LightlessWorld(uint id, float delay)
    {
        Cast(id, (uint)AID.LightlessWorldCastbar, delay, 10f, "Lightless World")
            .ActivateOnEnter<LightlessWorld>();
    }
    private void Almagest(uint id, float delay)
    {
        Cast(id, (uint)AID.Almagest, delay, 5f, "Almagest")
            .ActivateOnEnter<NaughtGrowsWildCharge>()
            .ActivateOnEnter<NaughtGrowsCircle>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsBossCircle>()
            .ActivateOnEnter<NaughtGrowsBossDonut>()
            .ActivateOnEnter<Almagest>();
    }

    private void DoubleNaughtGrows(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtGrowsDoubleCast, delay, 8f, "Double Naught Grows");
    }

    private void NaughtWakesActive(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtWakes, delay, 2f, "Naught Wakes AOEs")
            .ActivateOnEnter<PassageOfNaught>()
            .ActivateOnEnter<NaughtHunts>()
            ;
    }

    private void ShroudedHoly(uint id, float delay)
    {
        Cast(id, (uint)AID.ShroudedHolyCastbar, delay, 6f, "Shrouded Holy")
            .ActivateOnEnter<ShroudedHoly>()
            .ActivateOnEnter<DimensionZero>();
    }

    private void DimensionZero(uint id, float delay)
    {
        Cast(id, (uint)AID.DimensionZeroCastbar, delay, 5f, "Dimension Zero");
    }

    private void VacuumMeltdown(uint id, float delay)
    {
        Cast(id, (uint)AID.Vacuum, delay, 3f, "Vacuum/Meltdown")
            .ActivateOnEnter<VacuumAOE>()
            .ActivateOnEnter<VacuumArc1>()
            .ActivateOnEnter<VacuumArc2>()
            .ActivateOnEnter<VacuumArc3>()
            .ActivateOnEnter<VacuumTelegraph>()
            .ActivateOnEnter<MeltdownAoE>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownWait>();
        Cast(id + 0x000001, (uint)AID.Meltdown, 2.21f, 4f, "Meltdown");
    }

    private void NaughtHunts(uint id, float delay)
    {
        Cast(id, (uint)AID.NaughtHunts, delay, 7f, "Naught Hunts");
    }

    //private void XXX(uint id, float delay)
}
