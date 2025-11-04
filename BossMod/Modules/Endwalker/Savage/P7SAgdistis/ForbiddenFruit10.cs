namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: implement!
class ForbiddenFruit10(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.BronzeBellows)
{
    private BitMask _minotaurPlaforms = ValidPlatformsMask;
    private BitMask _bullPlatforms;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        foreach (var (slot, target) in Raid.WithSlot(true, true, true))
        {
            var source = TetherSources[slot];
            if (source != null)
            {
                AOEShape shape = source.OID == (uint)OID.ImmatureMinotaur ? ShapeMinotaurTethered : ShapeRect;
                shape.Draw(Arena, source.Position, Angle.FromDirection(target.Position - source.Position));
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var slot = TryAssignTether(source, tether);
        if (slot < 0)
            return;
        var safe = tether.ID switch
        {
            (uint)TetherID.Bull => _bullPlatforms,
            (uint)TetherID.Bird => ValidPlatformsMask & ~(_minotaurPlaforms | _bullPlatforms),
            _ => _minotaurPlaforms
        };
        SafePlatforms[slot] = safe;
    }

    protected override DateTime? PredictUntetheredCastStart(Actor fruit)
    {
        switch (fruit.OID)
        {
            case (uint)OID.ForbiddenFruitMinotaur:
                // minotaurs spawn on bridges, minotaur platform is adjacent => their opposite platform is never minotaur one
                _minotaurPlaforms[PlatformIDFromOffset(Arena.Center - fruit.Position)] = false;
                break;
            case (uint)OID.ForbiddenFruitBull:
                _bullPlatforms[PlatformIDFromOffset(fruit.Position - Arena.Center)] = true;
                break;
        }
        return null;
    }
}
