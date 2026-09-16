namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3BlackfireTrio(BossModule module) : Components.CastCounter(module, (uint)AID.BlackfireTrio)
{
    private Actor? _nael;
    public DateTime BaitAt = module.WorldState.FutureTime(8.5d);
    public Angle RelativeNorth;
    private readonly P3BahamutPositioning _positioning = module.FindComponent<P3BahamutPositioning>()!;
    public bool Active => _nael != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_nael, Colors.Object, true);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.NaelDeusDarnus && id == 0x1E43 && NumCasts > 0)
        {
            _nael = actor;
            RelativeNorth = (actor.Position - Arena.Center).ToAngle();
            _positioning.DesiredRotation = RelativeNorth;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MegaflareDive)
        {
            BaitAt = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (BaitAt != default)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 1f), BaitAt);
        }
    }
}

sealed class P3ThermionicBeam : Components.UniformStackSpread
{
    public P3ThermionicBeam(BossModule module) : base(module, 4f, 0f, 8, 8)
    {
        var target = Raid.Player(); // note: target is random
        if (target != null)
        {
            AddStack(target, WorldState.FutureTime(5.3d)); // assume it is activated right when downtime starts
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThermionicBeam)
        {
            Stacks.Clear();
        }
    }

    // we don't make any effort to stack with the party, since the standard BFT dodge should force everyone to stand together; only add damage prediction
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Stacks.Count;
        if (count == 0)
        {
            return;
        }
        var raid = Raid.WithSlot(false, true, true);
        var lenR = raid.Length;
        var pos = Stacks.Ref(0).Target.Position.Quantized();
        BitMask mask = default;
        for (var i = 0; i < lenR; ++i)
        {
            var p = raid[i];
            if (p.Item2.Position.InCircle(pos, 4f))
            {
                mask.Set(p.Item1);
            }
        }
    }
}

sealed class P3BlackfireLiquidHell(BossModule module) : LiquidHellBase(module)
{
    private bool _arenaSplitHints = true;
    private readonly P3BlackfireTrio _blackfire = module.FindComponent<P3BlackfireTrio>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!_arenaSplitHints)
        {
            return;
        }

        var numSources = NumSources + _predictedByEvent.Count;

        if (numSources < 5)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, _blackfire.RelativeNorth, 60f, 0f, 1f));
        }
        else if (numSources == 5)
        {
            // force pathfinder to move out of puddle, sometimes casters can get stuck for some reason
            if (actor.Position.InRect(Arena.Center, _blackfire.RelativeNorth, 60f, 60f, 6f))
            {
                var relN = _blackfire.RelativeNorth.ToDirection();
                var safety = actor.Class.IsDD() ? relN.OrthoL() : relN.OrthoR();
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + relN * 17f + safety * 8f, 2f));
            }
            else
            {
                // once we're out of puddles, move toward arena center; hint disappears once enum marker appears
                hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, _blackfire.RelativeNorth, 2f, 2f, 50f), DateTime.MaxValue);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            _arenaSplitHints = false;
        }
    }
}

sealed class P3MegaflareTower(BossModule module) : Components.CastTowers(module, (uint)AID.MegaflareTower, 3f)
{
    BitMask _stackTargets;
    bool _assigned;
    int _numHypernovas;
    private P3BlackfireTrio? _blackfire;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction && Towers.Count == 4)
        {
            if (_blackfire == null)
            {
                var comp = Module.FindComponent<P3BlackfireTrio>();
                if (comp != null)
                {
                    _blackfire = comp;
                }
                else
                {
                    return;
                }
            }
            var dirN = _blackfire.RelativeNorth.ToDirection();
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            var raid = Raid.WithSlot(true, true, true);
            var lenR = raid.Length;

            for (var i = 0; i < count; i++)
            {
                ref var t = ref towers[i];
                var toTower = t.Position - Arena.Center;
                var towerN = dirN.Dot(toTower) > 0f;
                var towerE = dirN.OrthoR().Dot(toTower) > 0f;
                BitMask mask = default;
                if (towerE)
                {
                    var allowedRole = towerN ? Role.Tank : Role.Healer;
                    for (var j = 0; j < lenR; ++j)
                    {
                        var p = raid[j];
                        if (p.Item2.Role != allowedRole)
                        {
                            mask.Set(p.Item1);
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < lenR; ++j)
                    {
                        var p = raid[j];
                        if (p.Item2.Class.IsSupport())
                        {
                            mask.Set(p.Item1);
                        }
                    }
                }
                t.ForbiddenSoakers = mask;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_stackTargets.Any() && _blackfire != null)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, _blackfire.RelativeNorth, 3f, 3f, 30f), DateTime.MaxValue);
            return;
        }

        var count = Towers.Count;
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            if (t.ForbiddenSoakers[slot])
            {
                hints.AddForbiddenZone(new SDCircle(t.Position, 3f + (_numHypernovas < 2 ? 2 : 0)), t.Activation);
            }
            else if (_numHypernovas < 2)
            {
                hints.AddForbiddenZone(new SDInvertedDonut(t.Position, 5f, 8f), t.Activation); // FIXME hypernova spawn
            }
            else
            {
                hints.AddForbiddenZone(new SDInvertedCircle(t.Position, 3f), t.Activation);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _stackTargets.Set(slot);
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                towers[i].ForbiddenSoakers.Set(slot);
            }
            if (_blackfire == null)
            {
                var comp = Module.FindComponent<P3BlackfireTrio>();
                if (comp != null)
                {
                    _blackfire = comp;
                }
                else
                {
                    return;
                }
            }
            AssignTowers();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.Hypernova)
        {
            ++_numHypernovas;
        }
    }

    void AssignTowers()
    {
        if (_assigned || _stackTargets.NumSetBits() != 4)
        {
            return;
        }

        var relativeN = _blackfire!.RelativeNorth.ToDirection();
        var relativeW = relativeN.OrthoL();

        var towers = CollectionsMarshal.AsSpan(Towers);
        RefSort.Sort(towers, new TowerComparer(relativeN, relativeW, Arena.Center));

        var ti = 0;
        var si = 0;

        var assignments = _prc.SlotsPerAssignment(Raid);
        var len = assignments.Length;
        for (var i = 0; i < len; ++i)
        {
            var s = assignments[i];
            if (++si <= 4 || _stackTargets[s])
            {
                continue;
            }

            ref var t = ref towers[ti++];
            if ((t.Position - Arena.Center).Dot(relativeW) <= 0f)
            {
                break;
            }

            t.ForbiddenSoakers = ~BitMask.Build(s);
        }

        _assigned = true;
    }

    private readonly struct TowerComparer(WDir relativeN, WDir relativeW, WPos center) : IRefComparer<Tower>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref Tower a, ref Tower b)
        {
            var apos = a.Position - center;
            var bpos = b.Position - center;

            var aRelevant = apos.Dot(relativeW) > 0f;
            var bRelevant = bpos.Dot(relativeW) > 0f;

            if (aRelevant != bRelevant)
            {
                return aRelevant ? -1 : 1;
            }

            return bpos.Dot(relativeN).CompareTo(apos.Dot(relativeN));
        }
    }
}

sealed class P3MegaflareStack(BossModule module) : Components.UniformStackSpread(module, 5f, 0f, 4, 4)
{
    private readonly P3BlackfireTrio _blackfire = module.FindComponent<P3BlackfireTrio>()!;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            if (Stacks.Count == 0)
            {
                AddStack(actor, WorldState.FutureTime(5d), new(0xff));
            }
            Stacks.Ref(0).ForbiddenPlayers.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MegaflareStack)
        {
            Stacks.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Stacks.Count;
        if (count == 0)
        {
            return;
        }

        ref var stack = ref Stacks.Ref(0);
        // avoid all stack targets
        if (stack.ForbiddenPlayers[slot])
        {
            var raid = Raid.WithSlot(false, true, true);
            var len = raid.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = raid[i];
                if (!stack.ForbiddenPlayers[p.Item1])
                {
                    hints.AddForbiddenZone(new SDCircle(p.Item2.Position, StackRadius), stack.Activation);
                }
            }
        }
        else // bft: stack spot is relative south of puddles
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + (_blackfire.RelativeNorth + 180f.Degrees()).ToDirection() * 8f, 2.5f), stack.Activation);
        }
    }
}
