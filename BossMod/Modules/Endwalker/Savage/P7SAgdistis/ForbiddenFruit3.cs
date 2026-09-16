namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class ForbiddenFruit3(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StaticMoon)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(10.5d);
}
