namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class DAL1GauntletStates : StateMachineBuilder
{
    public DAL1GauntletStates(DAL1Gauntlet module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<NihilitysSong>()
            .ActivateOnEnter<BallisticImpact>()
            .ActivateOnEnter<Stormcall>()
            .ActivateOnEnter<PainStorm>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<PainfulGust>()
            .ActivateOnEnter<BroadsideBarrage>()
            .ActivateOnEnter<NorthSouthwind>()
            .ActivateOnEnter<Pyroclysm>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.FourthLegionHoplomachus);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDeadOrDestroyed)
                        return false;
                }
                return module.PrimaryActor.IsDestroyed && (module.BossAugur()?.IsDestroyed ?? true) && (module.BossAlkonost()?.IsDeadOrDestroyed ?? true) && (module.BossCrow()?.IsDeadOrDestroyed ?? true);
            };
    }
}
