namespace BossMod.Shadowbringers.Alliance.A34XunZiMengZi;

sealed class Energy(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 1f, Length = 3f;
    private static readonly AOEShapeCapsule capsule = new(Radius, Length);
    private readonly List<Actor> _energy = new(23);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            aoes[i] = new(capsule, h.Position, h.Rotation);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Energy)
        {
            if (id == 0x11D2)
            {
                _energy.Add(actor);
            }
            else if (id == 0x11E7)
            {
                _energy.Remove(actor);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyBomb)
        {
            _energy.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _energy.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenImminent = new Func<WPos, float>[count];
        var forbiddenFuture = new Func<WPos, float>[count];
        for (var i = 0; i < count; ++i)
        {
            var h = _energy[i];
            forbiddenFuture[i] = ShapeDistance.Capsule(h.Position, h.Rotation, Length, 1.5f);
            forbiddenImminent[i] = ShapeDistance.Circle(h.Position, Radius);
        }
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenFuture), WorldState.FutureTime(1.1d));
        hints.AddForbiddenZone(ShapeDistance.Union(forbiddenImminent));
    }
}
