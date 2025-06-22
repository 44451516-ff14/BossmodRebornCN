namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class ImitationBlizzard(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(20f);
    private static readonly AOEShapeCross cross = new(60f, 8f);
    private readonly List<AOEInstance> _aoes = new(8);
    private Actor? referenceIcewind;
    private bool isPattern1;
    public bool IsRain4;
    private bool aoesAdded;
    private BitMask wickedWater;
    private BitMask gelidGaol;
    public bool WickedWaterActive; // separate bool incase none of the players in your party get the debuff, not sure if possible, so just in case
    public bool Show;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Show)
        {
            return [];
        }
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var act0 = aoes[0].Activation;
        var deadline1 = act0.AddSeconds(5d);
        var deadline2 = act0.AddSeconds(1d);

        var index = 0;
        while (index < count && aoes[index].Activation < deadline1)
        {
            ++index;
            if (!IsRain4 && aoes[index - 1].Activation < deadline2)
            {
                aoes[index - 1].Color = wickedWater[slot] ? Colors.SafeFromAOE : Colors.Danger;
            }
        }
        return aoes[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.DraconiformMotion1)
        {
            Show = true; // don't draw this stuff before baited cones appear so people don't panic and bait it to an unfortunate direction
        }
        else if (id == (uint)AID.FrigidDive)
        {
            var crossPuddle = Module.Enemies((uint)OID.WaterPuddleCross);
            if (crossPuddle.Count != 0)
            {
                var crossP = crossPuddle[0];
                _aoes.Add(new(cross, WPos.ClampToGrid(crossP.Position), crossP.Rotation, Module.CastFinishAt(spell, 4.1d)));
            }
        }
        else if (id == (uint)AID.ImitationIcicle)
        {
            var pos = caster.Position;
            if (pos.Z == 161f) // there is probably a way to calculate this stuff, but with only 4 possible patterns we might as well hardcode it for easy maintainability
            {
                (AOEShape, WPos)[] aoes = [];
                switch (pos.X)
                {
                    case -319f:
                        aoes =
                        [
                            (cross, new(-343f, 141f)),
                            (circle, new(-319f, 173f)),
                            (cross, new(-331f, 173f)),
                            (circle, new(-331f, 141f)),
                            (circle, new(-355f, 141f)),
                            (circle, new(-343f, 173f)),
                            (circle, new(-319f, 141f)),
                            (circle, new(-355f, 173f))
                        ];
                        break;
                    case -331f:
                        aoes =
                        [
                            (circle, new(-331f, 173f)),
                            (circle, new(-355f, 141f)),
                            (circle, new(-319f, 173f)),
                            (circle, new(-343f, 141f)),
                            (cross, new(-343f, 173f)),
                            (cross, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (circle, new(-319f, 141f))
                        ];
                        break;
                    case -343f:
                        aoes =
                        [
                            (circle, new(-343f, 173f)),
                            (circle, new(-319f, 141f)),
                            (cross, new(-331f, 173f)),
                            (circle, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (cross, new(-343f, 141f)),
                            (circle, new(-319f, 173f)),
                            (circle, new(-355f, 141f))
                        ];
                        break;
                    case -355f:
                        aoes =
                        [
                            (cross, new(-331f, 141f)),
                            (circle, new(-355f, 173f)),
                            (cross, new(-343f, 173f)),
                            (circle, new(-343f, 141f)),
                            (circle, new(-319f, 141f)),
                            (circle, new(-331f, 173f)),
                            (circle, new(-355f, 141f)),
                            (circle, new(-331f, 173f))
                        ];
                        break;

                }
                var act = Module.CastFinishAt(spell, 4.6d);
                for (var i = 0; i < 8; ++i)
                {
                    ref readonly var aoe = ref aoes[i];
                    var delay = i switch
                    {
                        < 2 => default,
                        < 5 => 1f,
                        < 7 => 2f,
                        _ => 3f
                    };
                    _aoes.Add(new(aoe.Item1, WPos.ClampToGrid(aoe.Item2), Angle.AnglesCardinals[1], act.AddSeconds(delay)));
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        var id = actor.OID;
        if (id == (uint)OID.Icewind && referenceIcewind == null)
        {
            referenceIcewind = actor;
        }
        else if (id == (uint)OID.WaterPuddleCross && actor.Position is var pos && (pos == new WPos(-321f, 141f) || pos == new WPos(-359f, 157f)))
        {
            isPattern1 = true;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var id = status.ID;
        if (id == (uint)SID.WickedWater)
        {
            wickedWater[Raid.FindSlot(actor.InstanceID)] = true;
            WickedWaterActive = true;
        }
        else if (id == (uint)SID.GelidGaol)
        {
            gelidGaol[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var id = status.ID;
        if (id == (uint)SID.WickedWater)
        {
            wickedWater[Raid.FindSlot(actor.InstanceID)] = false;
        }
        else if (id == (uint)SID.GelidGaol)
        {
            gelidGaol[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void Update()
    {
        if (!aoesAdded && Show && referenceIcewind != null && referenceIcewind.LastFrameMovement.LengthSq() > 1e-5f) // it is possible for the wind to move such a tiny amount that floating point errors result in incorrect direction detection
        {
            var isCCW = (referenceIcewind.Position - Arena.Center).OrthoR().Dot(referenceIcewind.LastFrameMovement) < 0f;
            if (!IsRain4)
            {
                // with only 4 possible patterns we might as well hardcode it for easy maintainability
                (AOEShape, WPos)[] aoes = [];
                var activationPattern241 = false;
                switch (isPattern1, isCCW)
                {
                    case (true, true):
                        aoes =
                        [
                            (cross, new(-353f, 173f)),
                            (cross, new(-321f, 141f)),
                            (circle, new(-337f, 133f)),
                            (circle, new(-337f, 181f)),
                            (circle, new(-321f, 173f)),
                            (circle, new(-353f, 141f)),
                            (cross, new(-337f, 157f))
                        ];
                        activationPattern241 = true;
                        break;
                    case (true, false):
                        aoes =
                        [
                            (circle, new(-353f, 141f)),
                            (circle, new(-321f, 173f)),
                            (circle, new(-337f, 133f)),
                            (circle, new(-337f, 181f)),
                            (cross, new(-337f, 157f)),
                            (cross, new(-353f, 173f)),
                            (cross, new(-321f, 141f))
                        ];
                        break;
                    case (false, true):
                        aoes =
                        [
                            (circle, new(-353f, 173f)),
                            (circle, new(-321f, 141f)),
                            (cross, new(-337f, 157f)),
                            (circle, new(-337f, 133f)),
                            (circle, new(-337f, 181f)),
                            (cross, new(-321f, 173f)),
                            (cross, new(-353f, 141f))
                        ];
                        break;
                    case (false, false):
                        aoes =
                        [
                            (cross, new(-321f, 173f)),
                            (cross, new(-353f, 141f)),
                            (circle, new(-337f, 181f)),
                            (circle, new(-337f, 133f)),
                            (circle, new(-321f, 141f)),
                            (circle, new(-353f, 173f)),
                            (cross, new(-337f, 157f))
                        ];
                        activationPattern241 = true;
                        break;
                }
                var act = WorldState.FutureTime(5.8d);
                for (var i = 0; i < 7; ++i)
                {
                    ref readonly var aoe = ref aoes[i];
                    var delay = i switch
                    {
                        < 2 => default,
                        < 6 when activationPattern241 => 4d,
                        < 5 => 4d,
                        _ => 8d
                    };
                    _aoes.Add(new(aoe.Item1, WPos.ClampToGrid(aoe.Item2), Angle.AnglesCardinals[1], act.AddSeconds(delay)));
                }
                aoesAdded = true;
            }
            else
            {
                // with only 2 possible patterns we might as well hardcode it for easy maintainability
                (AOEShape, WPos)[] aoes = [];
                switch (isPattern1, isCCW) // bool in tuple checks if wind is moving counterclockwise
                {
                    case (true, true):
                    case (false, false):
                        aoes =
                        [
                            (circle, new(-337f, 179f)),
                            (circle, new(-337f, 135f)),
                            (cross, new(-359f, 157f)),
                            (cross, new(-315f, 157f)),
                        ];
                        break;
                    case (true, false):
                    case (false, true):
                        aoes =
                        [
                            (cross, new(-337f, 179f)),
                            (cross, new(-337f, 135f)),
                            (circle, new(-359f, 157f)),
                            (circle, new(-315f, 157f)),
                        ];
                        break;
                }
                var act = WorldState.FutureTime(5.5d);
                for (var i = 0; i < 4; ++i)
                {
                    ref readonly var aoe = ref aoes[i];
                    _aoes.Add(new(aoe.Item1, WPos.ClampToGrid(aoe.Item2), Angle.AnglesCardinals[1], act.AddSeconds(i < 2 ? default : 10d)));
                }
                aoesAdded = true;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ImitationBlizzardCircle or (uint)AID.ImitationBlizzardCross)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (gelidGaol[slot])
        {
            return;
        }
        if (wickedWater[slot])
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var forbidden = new List<Func<WPos, float>>(len);
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Color == Colors.SafeFromAOE)
                {
                    forbidden.Add(aoe.Shape.InvertedDistance(aoe.Origin, aoe.Rotation));
                }
                else
                {
                    break;
                }
            }
            if (forbidden.Count != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbidden), aoes[0].Activation);
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (gelidGaol[slot])
        {
            return;
        }
        if (wickedWater[slot])
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isInside = false;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Check(actor.Position))
                {
                    isInside = true;
                    break;
                }
            }
            hints.Add("Get hit by an ice AOE!", !isInside);
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class ImitationBlizzardTowers(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    public override ReadOnlySpan<Tower> ActiveTowers(int slot, Actor actor)
    {
        var count = Towers.Count;
        if (count == 0)
        {
            return [];
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        var deadline = towers[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count && towers[index].Activation < deadline)
            ++index;

        return towers[..index];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FrigidDive)
        {
            var towerPuddles = Module.Enemies((uint)OID.WaterPuddleTower);
            var count = towerPuddles.Count - 1;
            var origin = spell.LocXZ;
            var dir = spell.Rotation;
            var act0 = Module.CastFinishAt(spell, 4.1d);
            var act1 = act0.AddSeconds(4d);
            Towers.Capacity = 6;
            for (var i = count; i >= 0; --i)
            {
                var t = towerPuddles[i].Position;
                var posClamp = WPos.ClampToGrid(t);
                if (t.InRect(origin, dir, 60f, default, 10f))
                {
                    Towers.Insert(0, new(posClamp, 4f, 4, 8, null, act0));
                }
                else
                {
                    Towers.Add(new(posClamp, 4f, 4, 8, null, act1));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.ImitationBlizzardTower)
        {
            ++NumCasts;
            if (Towers.Count != 0)
            {
                Towers.RemoveAt(0);
            }
        }
        else if (id == (uint)AID.DraconiformMotion1)
        {
            var towerPuddles = Module.Enemies((uint)OID.WaterPuddleTower);
            var count = towerPuddles.Count - 1;
            var act = Module.CastFinishAt(spell, 5.7d);
            Towers.Capacity = 6;
            for (var i = count; i >= 0; --i)
            {
                Towers.Add(new(WPos.ClampToGrid(towerPuddles[i].Position), 4f, 4, 8, null, act));
            }
        }
    }
}

sealed class BallOfIce(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circleBig = new(8f), circleSmall = new(4f);
    private readonly List<AOEInstance> _aoes = new(10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        var circle = actor.OID switch
        {
            (uint)OID.WaterPuddleCircle or (uint)OID.WaterPuddleCross => circleBig,
            (uint)OID.WaterPuddleTower => circleSmall,
            _ => null
        };
        if (circle != null)
        {
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(12.6d))); // activation time depends on mechanic, this is only the lowest possible
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BallOfIce or (uint)AID.BallOfIceTower)
        {
            var count = _aoes.Count;
            var pos = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
