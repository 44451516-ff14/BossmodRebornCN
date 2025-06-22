namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit1(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StaticMoon)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => WorldState.FutureTime(12.5d);
}
