namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

sealed class CLL3AdrammelechStates : StateMachineBuilder
{
    public CLL3AdrammelechStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<HolyIV>()
            .ActivateOnEnter<WaterIV1>()
            .ActivateOnEnter<WaterIV3>()
            .ActivateOnEnter<BlizzardIV>()
            .ActivateOnEnter<FireIV>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<BurstIITornado>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<WarpedLight>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<StoneIV>();
    }
}
