namespace BossMod.Modules.Dawntrail.Extreme.Ex8Enuo;

[SkipLocalsInit]

sealed class Ex8EnuoStates : StateMachineBuilder
{
    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        SimplePhase(default, Phase1, "P1")
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
    }

    private void Meteorain(uint id, float delay)
    {
        Cast(id, (uint)AID.Meteorain, delay, 5f, "Meteorain")
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
            .ActivateOnEnter<GazeOfTheVoidAOE>();
    }

    private void Vacuum(uint id, float delay)
    {
        Cast(id, (uint)AID.Vacuum, delay, 2.0f, "Vacuum");
    }

    private void DeepFreeze(uint id, float delay)
    {
        Cast(id, (uint)AID.DeepFreeze, delay, 5f, "Deep Freeze");
    }

    //private void XXX(uint id, float delay)
}
