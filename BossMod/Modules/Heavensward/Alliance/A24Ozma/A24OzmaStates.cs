namespace BossMod.Heavensward.Alliance.A24Ozma;

[SkipLocalsInit]
sealed class A24OzmaStates : StateMachineBuilder
{
    public A24OzmaStates(BossModule module) : base(module)
    {
        bool IsWipedOrLeftRaid() => module.Raid.Player()!.Position is var p && !(p.InSquare(new(300f, 265f), 80f) || p.InSquare(new(280f, -404.5f), 40f))
        || module.WorldState.CurrentCFCID != 168u;
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<HolyKB>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<ExecrationAOE>()
            .ActivateOnEnter<AccelerationBomb>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed && IsWipedOrLeftRaid();
    }
}
