namespace BossMod.Dawntrail.Savage.M01SBlackCat;

// same component covers normal, leaping and leaping clone versions
sealed class QuadrupleCrossingProtean(BossModule module) : Components.GenericBaitAway(module)
{
    public Actor? Origin;
    private DateTime _activation;
    private Actor? _clone;
    private Angle _jumpDirection;

    private static readonly AOEShapeCone _shape = new(100f, 22.5f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (Origin != null && _activation != default)
        {
            var party = Raid.WithoutSlot(false, true, true);
            Array.Sort(party, (a, b) =>
                {
                    var distA = (a.Position - Origin.Position).LengthSq();
                    var distB = (b.Position - Origin.Position).LengthSq();
                    return distA.CompareTo(distB);
                });

            var len = party.Length;
            var max = len > 4 ? 4 : len;
            for (var i = 0; i < max; ++i)
            {
                ref readonly var p = ref party[i];
                CurrentBaits.Add(new(Origin, p, _shape, _activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_clone != null && CurrentBaits.Count == 0)
            Arena.Actor(_clone.Position + 10f * (_clone.Rotation + _jumpDirection).ToDirection(), _clone.Rotation, Colors.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        // note: tether target is created after boss is tethered...
        if (actor.OID == (uint)OID.LeapTarget && Module.PrimaryActor.Tether.Target == actor.InstanceID)
        {
            Origin = actor;
            _jumpDirection = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position) - (Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuadrupleCrossingFirst:
                Origin = caster;
                _activation = Module.CastFinishAt(spell, 0.8f);
                break;
            case (uint)AID.LeapingQuadrupleCrossingBossL:
            case (uint)AID.LeapingQuadrupleCrossingBossR:
                // origin will be set to leap target when it's created
                _activation = Module.CastFinishAt(spell, 1.8f);
                break;
            case (uint)AID.NailchipperAOE:
                if (NumCasts == 8)
                    ForbiddenPlayers[Raid.FindSlot(spell.TargetID)] = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.QuadrupleCrossingProtean or (uint)AID.LeapingQuadrupleCrossingBossProtean or (uint)AID.LeapingQuadrupleCrossingShadeProtean)
        {
            if (NumCasts == 8)
            {
                ForbiddenPlayers = default; // third set => clear nailchippers
            }

            _activation = WorldState.FutureTime(3d);
            var count = spell.Targets.Count;
            for (var i = 0; i < count; ++i)
                ForbiddenPlayers[Raid.FindSlot(spell.Targets[i].ID)] = true;

            if (++NumCasts is 8 or 16)
            {
                Origin = null;
                ForbiddenPlayers = default;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (NumCasts < 8 || tether.ID != (uint)TetherID.Soulshade)
            return; // not relevant tether

        if (_clone == null)
        {
            _clone = source;
        }
        else if (_clone == source)
        {
            var origin = source.Position + 10f * (source.Rotation + _jumpDirection).ToDirection();
            Origin = new(0, 0, -1, "", 0, ActorType.None, Class.None, 0, new(origin.X, source.PosRot.Y, origin.Z, source.PosRot.W));
            _activation = WorldState.FutureTime(17d);
        }
    }
}

sealed class QuadrupleCrossingAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private bool ready;
    private static readonly AOEShapeCone _shape = new(100f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!ready)
            return [];
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i < 4)
            {
                if (count == 8)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.QuadrupleCrossingProtean:
            case (uint)AID.LeapingQuadrupleCrossingBossProtean:
            case (uint)AID.LeapingQuadrupleCrossingShadeProtean:
                _aoes.Add(new(_shape, WPos.ClampToGrid(caster.Position), caster.Rotation, WorldState.FutureTime(5.9d)));
                break;
            case (uint)AID.QuadrupleCrossingAOE:
            case (uint)AID.LeapingQuadrupleCrossingBossAOE:
            case (uint)AID.LeapingQuadrupleCrossingShadeAOE:
                ++NumCasts;
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
        if (_aoes.Count == 8)
            ready = true;
    }
}
