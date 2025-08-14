namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 18f, stopAfterWall: true)
{
    private RelSimplifiedComplexPolygon polygon = new();
    private bool polygonInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                if (!polygonInit)
                {
                    if (Arena.Bounds is ArenaBoundsComplex arena)
                    {
                        polygon = arena.poly.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                        polygonInit = true;
                    }
                }
                var center = Arena.Center;
                var poly = polygon;
                hints.AddForbiddenZone(p =>
                {
                    // while doing a point in polygon test and intersection test seems like double the work, the intersection test is actually a lot slower than the PiP test, so this is a net positive to filter out some cells beforehand
                    var offset = p - center;
                    var dir = offset.Normalized();
                    if (poly.Contains(offset + 18f * dir) && Intersect.RayPolygon(offset, dir, poly) > 18f)
                        return 1f;
                    return default;
                }, act);
            }
        }
    }
}
