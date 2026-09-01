namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

[SkipLocalsInit]
sealed class DigThreeGraves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _fires = [];
    private readonly List<Actor> _ices = [];
    private readonly List<Actor> _lightnings = [];
    private readonly List<Mechanic> _elements = [];
    private readonly AOEShapeCircle _circle = new(18f);
    private readonly AOEShapeCross _cross = new(45f, 7.5f);
    private readonly AOEShapeCone _cone = new(60f, 22.5f.Degrees());
    private bool _tracking = true;

    private enum Mechanic : ushort
    {
        None = 0x0,
        Circle = 0x45A,
        Cross = 0x45B,
        Cone = 0x45C
    }

    // after death shroud start, track head AOEs by tether type
    // determine AOE order by tracking OnStatusLose on boss for the 3 extras
    // has an OnStatusGain but replay only shows status losses?
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        if (_elements.Count == 0)
        {
            return [];
        }

        // may show other AOEs for a frame but negligable?
        // try setting risky to 3.5s so AI can attempt to dodge into 1st Severed Current
        var element = _elements[0];
        var max = element == Mechanic.Cone ? 8 : 2;
        max = count > max ? max : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var time = WorldState.CurrentTime;
        var riskyAt = 3.5d;

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref aoes[i];
            aoe.Risky = aoe.Activation.AddSeconds(-riskyAt) <= time;
        }

        return aoes;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.SeveringHead)
        {
            var list = tether.ID switch
            {
                (uint)TetherID.Fire => _fires,
                (uint)TetherID.Ice => _ices,
                (uint)TetherID.Lightning => _lightnings,
                _ => null
            };

            list?.Add(source);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (_tracking && status.ID == (uint)SID.Element && status.Extra is 0x45A or 0x45B or 0x45C)
        {
            var mech = (Mechanic)status.Extra;
            _elements.Add(mech);
            if (_elements.Count == 3)
            {
                _tracking = false;
            }

            AddAOEs(mech);
        }

        void AddAOEs(Mechanic extra)
        {
            var heads = extra switch
            {
                Mechanic.Circle => _fires,
                Mechanic.Cross => _ices,
                Mechanic.Cone => _lightnings,
                _ => []
            };

            AOEShape? shape = extra switch
            {
                Mechanic.Circle => _circle,
                Mechanic.Cross => _cross,
                Mechanic.Cone => _cone,
                _ => null
            };

            if (shape == null)
            {
                return;
            }

            var span = CollectionsMarshal.AsSpan(heads);
            var count = span.Length;

            // how much delay between element resolves?
            // status gains: 151.738 | 153.780 | 155.710 2s between statuses
            // resolve time: 160.711 | 167.254 | 173.841 6.5s between casts
            var interval = 4.5d;
            var activation = WorldState.CurrentTime.AddSeconds(8.9d + interval * (_elements.Count - 1));

            for (var i = 0; i < count; i++)
            {
                ref var head = ref span[i];
                var position = head.Position;
                var rotation = head.Rotation;
                if (shape is AOEShapeCone)
                {
                    // is lightning always in X shape, or can it be also be + shape or any other rotation?
                    for (var j = 0; j < 4; j++)
                    {
                        _aoes.Add(new(shape, position, (45f + 90f * j).Degrees(), activation, default, true, head.InstanceID, shape.Distance(position, rotation)));
                    }
                }
                else
                {
                    _aoes.Add(new(shape, position, rotation, activation, default, true, head.InstanceID, shape.Distance(position, rotation)));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            Mechanic extra = spell.Action.ID switch
            {
                (uint)AID.AncientFireShort => Mechanic.Circle,
                (uint)AID.AncientBlizzardShort => Mechanic.Cross,
                (uint)AID.AncientThunderShort => Mechanic.Cone,
                _ => Mechanic.None
            };

            if (extra != Mechanic.None)
            {
                ++NumCasts;
                _aoes.RemoveAt(0);
                RemoveElement(extra);
            }
        }

        // avoid doing on status loss as loss happens 0.1-0.2s before AOE actually resolves
        void RemoveElement(Mechanic extra)
        {
            if (_elements.Count != 0 && _elements[0] == extra)
            {
                _elements.RemoveAt(0);
            }
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add(string.Join(" -> ", _elements));
    }
#if DEBUG
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var x in _fires)
        {
            Arena.ZoneCircle(x.Position, 1.5f, 0xFF0000FF);
        }
        foreach (var x in _ices)
        {
            Arena.ZoneCircle(x.Position, 1.5f, 0xFFFF0000);
        }
        foreach (var x in _lightnings)
        {
            Arena.ZoneCircle(x.Position, 1.5f, 0xFFFFFF00);
        }
    }
#endif
}

[SkipLocalsInit]
sealed class SeveredDarkCurrent(BossModule module) : Components.GenericAOEs(module)
{
    // is it always N, NW, SW for rotation? does it aim towards a particular element 1st?
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _aoeAll = [];
    private readonly AOEShapeRect _rect = new(30f, 5f, 30f);
    private readonly Angle[] _severedAngles = [-180f.Degrees(), 60f.Degrees(), -60f.Degrees()];
    private bool _added = false;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count == 5 ? 3 : count > 3 ? 4 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes)[..max];
        var isFourAOEs = max == 4;
        var isThreeAOEs = max == 3;

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];

            var shouldBeDanger = isFourAOEs && i < 2 || isThreeAOEs && i == 0;
            var shouldBeRisky = shouldBeDanger || max == 2 && i < 2;

            if (shouldBeDanger)
                aoe.Color = Colors.Danger;

            if (shouldBeRisky)
                aoe.Risky = true;
        }

        return aoes;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        // add all 15 in one go, take 5 per element
        if (!_added && status.ID == (uint)SID.Element)
        {
            _added = true;
            // similar timing to DigThreeGraves, .1-.2 earlier than head AOEs
            var activation = WorldState.CurrentTime.AddSeconds(8.8d);
            var interval = 6.5d;
            var position = Arena.Center;
            var distance = 10f;

            for (var i = 0; i < 3; i++)
            {
                activation = activation.AddSeconds(interval * i);
                var rotation = _severedAngles[i];
                var dir = rotation.ToDirection().OrthoL().Normalized();

                _aoeAll.Add(new(_rect, position, rotation, activation, risky: true));
                for (var j = 1; j <= 2; j++)
                {
                    _aoeAll.Add(new(_rect, position + j * distance * dir, rotation, activation.AddSeconds(2.1d * j), risky: false));
                    _aoeAll.Add(new(_rect, position + j * distance * dir * -1f, rotation, activation.AddSeconds(2.1d * j), risky: false));
                }
            }

            RefillAOEs();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.DarkCurrent3:
                case (uint)AID.DarkCurrent2:
                    ++NumCasts;
                    _aoes.RemoveAt(0);
                    RefillAOEs();
                    break;
            }
        }
    }

    private void RefillAOEs()
    {
        if (_aoes.Count == 0 && NumCasts % 5 == 0 && NumCasts < _aoeAll.Count)
        {
            var all = CollectionsMarshal.AsSpan(_aoeAll);
            var count = all.Length > NumCasts + 5 ? NumCasts + 5 : all.Length;
            for (var i = NumCasts; i < count; i++)
            {
                ref var aoe = ref all[i];
                _aoes.Add(aoe);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // stay near initial cast to move in after
        if (_aoes.Count == 5)
        {
            ref var aoe = ref _aoes.Ref(0);
            hints.GoalZones.Add(AIHints.GoalRectangle(aoe.Origin, aoe.Rotation.ToDirection(), 7f, 60f, 100f));
        }
    }
}
