namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

sealed class NostalgiaDimensionalSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NostalgiaDimensionalSurge, 5f);

sealed class Nostalgia(BossModule module) : Components.CastCounterMulti(module,
    [(uint)AID.NostalgiaBury1, (uint)AID.NostalgiaBury2, (uint)AID.NostalgiaBury3, (uint)AID.NostalgiaBury4, (uint)AID.NostalgiaRoar1, (uint)AID.NostalgiaRoar2, (uint)AID.NostalgiaPrimalRoar]);
