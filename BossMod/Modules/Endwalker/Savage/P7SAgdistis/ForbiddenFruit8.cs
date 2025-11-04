namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit8(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.StymphalianStrike)
{
    private BitMask _noBirdsPlatforms = ValidPlatformsMask;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var slot = TryAssignTether(source, tether);
        if (slot < 0)
            return;
        var safe = ValidPlatformsMask & ~_noBirdsPlatforms;
        safe.Clear(PlatformIDFromOffset(source.Position - Arena.Center));
        SafePlatforms[slot] = safe;
    }

    protected override DateTime? PredictUntetheredCastStart(Actor fruit)
    {
        if (fruit.OID != (uint)OID.ForbiddenFruitBird)
            return null;

        _noBirdsPlatforms.Clear(PlatformIDFromOffset(fruit.Position - Arena.Center));
        Array.Fill(SafePlatforms, _noBirdsPlatforms);
        return WorldState.FutureTime(12.5f);
    }
}
