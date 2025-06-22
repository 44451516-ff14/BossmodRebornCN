namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: implement!
class ForbiddenFruit9(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StymphalianStrike)
{
    protected override DateTime? PredictUntetheredCastStart(Actor fruit) => fruit.OID == (uint)OID.ForbiddenFruitBird ? WorldState.FutureTime(12.5d) : null;
}
