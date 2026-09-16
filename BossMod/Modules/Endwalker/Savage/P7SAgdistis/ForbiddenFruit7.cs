namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class ForbiddenFruit7(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StymphalianStrike)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(16.5d);
}
