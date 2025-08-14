namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class RunawayRunoff(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RunawayRunoff, 18f)
{
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref var kb = ref Casters.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
                var len = aoes.Length;
                WPos donutPos = default;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Shape == Explosion.Donut)
                    {
                        donutPos = aoe.Origin;
                        break;
                    }
                }
                var loc = kb.Origin;

                var center = Arena.Center;
                hints.AddForbiddenZone(p =>
                {
                    // knockback distance can be greater than distance between source and inner ring of the donut, but we have a few seconds to move after
                    // so we mainly want to ensure getting knocked back into the right direction at least, we can achieve this by inflating the donut hole
                    if ((p + 18f * (p - loc).Normalized()).InCircle(donutPos, 6f))
                    {
                        return 1f;
                    }
                    return default;
                }, act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Shape != Explosion.Donut && aoe.Check(pos)) // allow getting knocked into the donut since there is time to move after kb
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }
}
