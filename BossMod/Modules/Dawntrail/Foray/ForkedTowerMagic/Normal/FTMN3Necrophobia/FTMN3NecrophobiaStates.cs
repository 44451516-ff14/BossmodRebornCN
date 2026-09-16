namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

sealed class FTMN3NecrophobiaStates : StateMachineBuilder
{
    public FTMN3NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<AncientFire>()
            .ActivateOnEnter<AncientBlizzard>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<AncientThunder>()
            .ActivateOnEnter<DarkCurrent>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<VacuumWave>();
    }
}
