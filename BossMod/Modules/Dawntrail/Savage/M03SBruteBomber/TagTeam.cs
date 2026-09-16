namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

sealed class TagTeamLariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(2)];
    private readonly Actor?[] _tetherSource = new Actor?[PartyState.MaxPartySize];
    private readonly AOEInstance[][] _safespot = new AOEInstance[PartyState.MaxPartySize][];
    private readonly AOEShapeRect rect = new(70f, 17f);
    private ConeHA[] cone = [];
    private bool waitforcone;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_tetherSource[slot] != null)
        {
            if (_safespot[slot] != null)
            {
                return _safespot[slot];
            }

            var safespot = AddSafespots(slot);
            if (safespot != null)
            {
                return _safespot[slot] = safespot;
            }
            return [];
        }
        return CollectionsMarshal.AsSpan(AOEs);
    }

    private AOEInstance[]? AddSafespots(int slot)
    {
        var count = AOEs.Count;
        if (AOEs.Count == 2 && waitforcone == (cone.Length != 0))
        {
            var safeShapes = new RectangleSE[1];
            var dangerShapes = new RectangleSE[1];
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                var origin = aoe.Origin;
                var isSafe = _tetherSource[slot]!.Position.AlmostEqual(origin, 25f);
                var shape = new RectangleSE(origin, origin + rect.LengthFront * aoe.Rotation.ToDirection(), rect.HalfWidth);

                if (isSafe)
                {
                    safeShapes[0] = shape;
                }
                else
                {
                    dangerShapes[0] = shape;
                }
            }

            ref var aoe0 = ref aoes[0];
            var center = Arena.Center;
            var aoeShape = new AOEShapeCustom(center, safeShapes, dangerShapes, cone, cone.Length != 0 ? OperandType.Intersection : OperandType.Union, invertForbiddenZone: true);
            return [new(aoeShape, center, default, aoe0.Activation, Colors.SafeFromAOE, shapeDistance: aoeShape.InvertedDistance(center, default))];
        }
        return null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tetherSource[slot] == null)
        {
            base.AddHints(slot, actor, hints);
        }
        else if (_safespot[slot] is AOEInstance[] aoes)
        {
            ref var aoe = ref aoes[0];
            hints.Add("Go to correct spot!", !aoe.Check(actor.Position));
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainDeathmatch && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            _tetherSource[slot] = source;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
                AddAOE();
                break;
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
                AddAOE();
                waitforcone = true;
                break;
            case (uint)AID.FusesOfFuryMurderousMist:
                cone = [new(spell.LocXZ, 40f, spell.Rotation, 135f.Degrees())];
                break;
        }
        void AddAOE() => AOEs.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.TagTeamLariatComboFirstRAOE:
            case (uint)AID.TagTeamLariatComboFirstLAOE:
            case (uint)AID.TagTeamLariatComboSecondRAOE:
            case (uint)AID.TagTeamLariatComboSecondLAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstRAOE:
            case (uint)AID.FusesOfFuryLariatComboFirstLAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondRAOE:
            case (uint)AID.FusesOfFuryLariatComboSecondLAOE:
                ++NumCasts;
                new Span<Actor?>(_tetherSource, 0, PartyState.MaxPartySize).Clear();
                var index = -1;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                var len = aoes.Length;
                var pos = spell.LocXZ;
                for (var i = 0; i < len; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Origin.AlmostEqual(pos, 1f))
                    {
                        index = i;
                        break;
                    }
                }
                if (index < 0)
                {
                    ReportError($"Failed to find AOE for {spell.LocXZ}");
                }
                else if (id is (uint)AID.TagTeamLariatComboFirstRAOE or (uint)AID.TagTeamLariatComboFirstLAOE or (uint)AID.FusesOfFuryLariatComboFirstRAOE or (uint)AID.FusesOfFuryLariatComboFirstLAOE)
                {
                    ref var aoe = ref aoes[index];
                    var center = Arena.Center;
                    aoe.Origin = (center - (aoe.Origin - center)).Quantized();
                    aoe.Rotation += 180f.Degrees();
                    aoe.Activation = WorldState.FutureTime(4.3d);
                }
                else
                {
                    AOEs.RemoveAt(index);
                }
                break;
        }
    }
}
