namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL1Brionac4thLegionHelldiver;

sealed class CLL1Brionac4thLegionHelldiverStates : StateMachineBuilder
{
    public CLL1Brionac4thLegionHelldiverStates(CLL1Brionac4thLegionHelldiver module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BossHealths>()
            .ActivateOnEnter<ChainCannon>()
            .ActivateOnEnter<OrbsAOE>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<ElectricAnvil>()
            .ActivateOnEnter<Voltstream>()
            .ActivateOnEnter<FalseThunder>()
            .ActivateOnEnter<WildCharges>()
            .ActivateOnEnter<LightningShower>()
            .ActivateOnEnter<CommandSuppressiveFormation>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<MRVMissile>()
            .ActivateOnEnter<MagitekMissiles>()
            .ActivateOnEnter<InfraredBlast>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.FourthLegionSkyArmor);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDeadOrDestroyed)
                        return false;
                }
                return module.PrimaryActor.IsDeadOrDestroyed && (module.BossHellDiver?.IsDeadOrDestroyed ?? true);
            };
    }
}
