namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class WashOut(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WashOut, 35f, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var vzs = Module.Enemies((uint)OID.WaterVoidzone);
                var count = vzs.Count;
                var forbidden = new Func<WPos, float>[count + 1];
                var dir = kb.Direction.ToDirection();
                var dirAdj = 35f * dir;
                var center = Arena.Center;
                // square intentionally slightly smaller to prevent sus knockback
                forbidden[count] = p =>
                {
                    if ((p + dirAdj).InSquare(center, 19f))
                        return 1f;
                    return default;
                };

                for (var i = 0; i < count; ++i)
                {
                    var a = vzs[i].Position;
                    forbidden[i] = ShapeDistance.Rect(a, dir, 40f, 40f, 5f);
                }
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), act);
            }
        }
    }
}
