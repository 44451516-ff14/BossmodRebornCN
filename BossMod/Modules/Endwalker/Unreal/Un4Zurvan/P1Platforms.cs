namespace BossMod.Endwalker.Unreal.Un4Zurvan;

sealed class P1Platforms(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> ForbiddenPlatforms = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(ForbiddenPlatforms);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        var dir = actor.OID switch
        {
            (uint)OID.PlatformE => 90f.Degrees(),
            (uint)OID.PlatformN => 180f.Degrees(),
            (uint)OID.PlatformW => -90f.Degrees(),
            _ => default
        };
        if (dir == default)
            return;

        switch (state)
        {
            case 0x00040008u:
                ForbiddenPlatforms.Add(new(new AOEShapeCone(20f, 45f.Degrees()), Arena.Center, dir, WorldState.FutureTime(5d)));
                break;
            case 0x00100020u:
                ++NumCasts;
                break;
        }
    }
}
