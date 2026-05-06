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
        SimplePhase(1u, AddPhase, "Adds")
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
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.LightlessWorldCastbar) ?? false);
        SimplePhase(2u, Phase2, "P2")
            .ActivateOnEnter<ArenaChanges>();
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
        ActorCast(id + 0x03, _module.CastingAdd, (uint)AID.VoidalTurbulenceCastBar, 13.607f, 7f, default, "Cones + Towers (again)");
        SimpleState(id + 0x04, 60f, "Add Enrage?"); // I have no idea when this would actually go off.
    }

    private void Phase2(uint id)
    {
        LightlessWorld(id, 96.84f);
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

    //private void XXX(uint id, float delay)
}
