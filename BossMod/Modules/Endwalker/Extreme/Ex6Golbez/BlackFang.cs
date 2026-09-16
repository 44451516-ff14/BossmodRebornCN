namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class BlackFang(BossModule module) : Components.CastCounterMulti(module,
    [(uint)AID.BlackFangAOE1, (uint)AID.BlackFangAOE2, (uint)AID.BlackFangAOE3, (uint)AID.BlackFangEnrageAOE1, (uint)AID.BlackFangEnrageAOE2, (uint)AID.BlackFangEnrageAOE3]);