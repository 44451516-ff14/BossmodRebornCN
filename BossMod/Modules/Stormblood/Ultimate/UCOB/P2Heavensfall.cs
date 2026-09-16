using static BossMod.PartyRolesConfig;

namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class Heavensfall(BossModule module) : Components.GenericKnockback(module, (uint)AID.Heavensfall)
{
    public DateTime Activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 11f, Activation, ignoreImmunes: true) };
    }
}

sealed class P2Heavensfall(BossModule module) : Heavensfall(module)
{
    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new SDPrecisePosition(new WPos(0, 9), new(0, 1), 0.5f, actor.Position, 0.1f), Activation);
    }
}

sealed class P3Heavensfall(BossModule module) : Heavensfall(module)
{
    public bool EnableHints;

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        if (EnableHints)
        {
            hints.AddForbiddenZone(new SDInvertedDonut(Arena.Center, 8.5f, 10), Activation);
        }
    }
}

sealed class P2HeavensfallPillar(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private readonly AOEShapeRect _shape = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.EventHelper)
        {
            return;
        }
        switch (state)
        {
            case 0x00040008u: // appear
                _aoe = [new(_shape, actor.Position, actor.Rotation)];
                break;
            // 0x00100020: ? 0.5s after appear
            // 0x00400080: ? 4.0s after appear
            // 0x01000200: ? 5.8s after appear
            // 0x04000800: ? 7.5s after appear
            // 0x10002000: ? 9.4s after appear
            case 0x40008000u: // disappear (11.1s after appear)
                _aoe = [];
                break;
        }
    }
}

sealed class P2ThermionicBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermionicBurst, new AOEShapeCone(24.5f, 11.25f.Degrees()));

sealed class P2MeteorStream : Components.UniformStackSpread
{
    public int NumCasts;

    public P2MeteorStream(BossModule module) : base(module, default, 4f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(5.6d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorStream)
        {
            ++NumCasts;

            var count = Spreads.Count;
            var id = spell.MainTargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
            // update activation time for second set
            if (NumCasts == 4)
            {
                spreads = CollectionsMarshal.AsSpan(Spreads);
                var act = WorldState.FutureTime(3.1d);
                for (var i = 0; i < 4; ++i)
                {
                    spreads[i].Activation = act;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        if (Spreads.Count == 8)
        {
            var (dist, angle) = assignment switch
            {
                Assignment.MT => (9f, -11.25f.Degrees()),
                Assignment.OT => (9f, 11.25f.Degrees()),
                Assignment.H1 => (18f, -11.25f.Degrees()),
                Assignment.H2 => (18f, 11.25f.Degrees()),
                Assignment.M1 => (9f, -56.25f.Degrees()),
                Assignment.M2 => (9f, 56.25f.Degrees()),
                Assignment.R1 => (18f, -56.25f.Degrees()),
                Assignment.R2 => (18f, 56.25f.Degrees()),
                _ => default
            };

            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + angle.ToDirection() * dist, 2f), Spreads.Ref(0).Activation);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class P2HeavensfallDalamudDive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DalamudDive, true, true)
{
    private readonly Actor? _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);

    private readonly AOEShapeCircle _shape = new(5f);

    public void Show()
    {
        if (_target != null)
        {
            CurrentBaits.Add(new(_target, _target, _shape));
        }
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var isTarget = false;
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target == actor)
            {
                isTarget = true;
                break;
            }
        }
        if (!isTarget)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        // preposition close to nael
        if (actor.Role is Role.Melee or Role.Tank)
        {
            for (var i = 0; i < len; ++i)
            {
                var t = baits[i].Target;
                if (t != actor)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position, 6f));
                }
            }
        }
    }
}
