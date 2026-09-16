namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class ForbiddenFruit2(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StymphalianStrike)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(12.5d);
}
