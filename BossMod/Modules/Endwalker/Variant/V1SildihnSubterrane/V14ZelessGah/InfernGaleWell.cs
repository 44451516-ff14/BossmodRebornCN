namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V14ZelessGah;

sealed class InfernGale(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = new Knockback[1];
    private bool kbInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => kbInit ? _kb : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CA)
        {
            _kb = [new(actor.Position.Quantized(), 20f, WorldState.FutureTime(5.6d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfernGale)
        {
            kbInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (kbInit)
        {
            var center = Arena.Center;
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var origin = kb.Origin;
                // rect intentionally slightly smaller to prevent sus knockbacks
                hints.AddForbiddenZone(p =>
                {
                    if ((p + 20f * (p - origin).Normalized()).InRect(center, 14f, 19f))
                    {
                        return 1f;
                    }
                    return default;
                }, act);
            }
        }
    }
}

sealed class InfernWellPull(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = new Knockback[1];
    private bool kbInit;

    private readonly InfernWellAOE _aoe = module.FindComponent<InfernWellAOE>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => kbInit ? _kb : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CB)
        {
            _kb = [new(actor.Position.Quantized(), 15f, WorldState.FutureTime(7.6d), kind: Kind.TowardsOrigin, minDistance: 1f)]; // min distance: actor (0x233C helper) + player hitbox radius
            kbInit = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.InfernWell)
        {
            kbInit = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (kbInit)
        {
            ref var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(kb.Origin, 23f), act); // radius: 15 pull distance + 8 aoe radius
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOEInit)
        {
            ref var aoe = ref _aoe.AOE[0];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}

sealed class InfernWellAOE(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = new AOEInstance[1];
    public bool AOEInit;
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEInit ? AOE : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MechanicStatus && status.Extra == 0x1CB)
        {
            AOE = [new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(9.1d))];
            AOEInit = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InfernWellAOE)
        {
            AOEInit = false;
        }
    }
}
