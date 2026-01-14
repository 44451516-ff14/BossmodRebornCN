namespace BossMod.Stormblood.Raid.O1NAlteRoite;

sealed class O1NAlteRoiteStates : StateMachineBuilder
{
    public O1NAlteRoiteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ThinIce>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<TwinBoltTetheredBuster>()
            .ActivateOnEnter<BreathwingKB>()
            .ActivateOnEnter<ClampAOE>()
            .ActivateOnEnter<ClampKB>()
            .ActivateOnEnter<DownburstKB>()
            .ActivateOnEnter<DownburstTornado>()
            .ActivateOnEnter<FireOrbsTimedFollowAOE>()
            .ActivateOnEnter<TwinBoltAOE>()
            .ActivateOnEnter<BlazeLevinStackSpread>()
;
    }
}
