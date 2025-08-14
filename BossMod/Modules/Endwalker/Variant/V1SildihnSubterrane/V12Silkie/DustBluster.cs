namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class DustBluster(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DustBluster, 16f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var center = Arena.Center;
                var vzs = Module.Enemies((uint)OID.WaterVoidzone);
                var count = vzs.Count;
                var forbidden = new Func<WPos, float>[count + 1];

                // square intentionally slightly smaller to prevent sus knockback
                forbidden[count] = p =>
                {
                    if ((p + 16f * (p - center).Normalized()).InSquare(center, 19f))
                    {
                        return 1f;
                    }
                    return default;
                };

                for (var i = 0; i < count; ++i)
                {
                    var a = vzs[i].Position;
                    forbidden[i] = ShapeDistance.Cone(center, 100f, Angle.FromDirection(a - center), Angle.Asin(5f / (a - center).Length()));
                }
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), act);
            }
        }
    }
}
