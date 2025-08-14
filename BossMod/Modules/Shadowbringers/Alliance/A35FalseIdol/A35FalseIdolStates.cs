namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class A35FalseIdolStates : StateMachineBuilder
{
    public A35FalseIdolStates(A35FalseIdol module) : base(module)
    {
        TrivialPhase(default)
            .ActivateOnEnter<MagicalInterference>()
            .ActivateOnEnter<LighterNoteBait>()
            .ActivateOnEnter<LighterNoteExaflare>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<ScreamingScoreEminence>()
            .ActivateOnEnter<ScatteredMagic>()
            .ActivateOnEnter<DarkerNote>();
        TrivialPhase(1u)
            .OnEnter(() => module.Arena.Bounds = A35FalseIdol.ArenaP2)
            .ActivateOnEnter<MagicalInterference>() // not keeping on phase change because mechanic gets cancelled by phase change
            .ActivateOnEnter<LighterNoteBait>() // same as above
            .ActivateOnEnter<LighterNoteExaflare>() // same as above
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Distortion>()
            .ActivateOnEnter<UnevenFooting>()
            .ActivateOnEnter<Crash>()
            .ActivateOnEnter<HeavyArms1>()
            .ActivateOnEnter<HeavyArms2>()
            .ActivateOnEnter<ShockwaveKB>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<Energy>()
            .Raw.Update = () => module.BossBossP2?.IsDeadOrDestroyed ?? true;
    }
}
