namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P2Cauterize(BossModule module) : Components.GenericAOEs(module)
{
    public struct Assignment(int order, DateTime deadline)
    {
        public int Order = order;
        public DateTime Deadline = deadline;
    }

    public Assignment[] BaitOrder = new Assignment[PartyState.MaxPartySize];
    public int NumBaitsAssigned;
    private int _numHypernovas;
    public List<AOEInstance> Casters = [];
    private readonly List<(Actor actor, int position)> _dragons = []; // position 0 is N, then CW
    private readonly PositionComparer _comparer = new();

    private readonly AOEShapeRect _shape = new(52f, 10f);

    public static readonly WPos[] StandardBaits = [
        new(18.149f, -9.531f),
        new(8, 18.874f),
        new(-17.667f, 10.398f)
    ];

    public WPos[] CurrentBaits = [];
    private readonly ArenaBoundsSquare pathfindHugBorderBounds = new(21f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(Casters);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot].Order >= NextBaitOrder)
        {
            hints.Add($"Bait {BaitOrder[slot].Order}", false);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var boSlot = BaitOrder[slot];
        var bo = boSlot.Order;

        if (bo >= NextBaitOrder)
        {
            var b = CurrentBaits[bo - 1];
            var act = boSlot.Deadline;
            if (_numHypernovas >= Math.Min(4, bo * 2 - 1))
            {
                hints.PathfindMapBounds = pathfindHugBorderBounds;

                hints.AddForbiddenZone(new SDPrecisePosition(b, new(0f, 1f), 0.5f, actor.Position, 0.1f), act);
            }
            else
            {
                hints.AddForbiddenZone(new SDInvertedDonut(b, 5f, 7f), act.AddSeconds(-1d));
            }
        }
        else if (bo == 0)
        {
            // non-baiters should move further away from any active dragons to give the baiters room, in case the next mechanic is spread
            var count = Casters.Count;
            var aoes = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                hints.AddForbiddenZone(new SDRect(aoe.Origin, aoe.Rotation, 52f, 0f, 13f), aoe.Activation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var boSlot = BaitOrder[pcSlot];
        var order = boSlot.Order;
        if (order >= NextBaitOrder)
        {
            var dragons = DragonsForOrder(order);
            var len = dragons.Length;
            for (var i = 0; i < len; ++i)
            {
                var d = dragons[i];
                var pos = d.Position;
                Arena.ActorInsideBounds(pos, d.Rotation, Colors.Object);
                _shape.Outline(Arena, pos, Angle.FromDirection(pc.Position - pos));
            }

            Arena.ZoneCircleOutline(CurrentBaits[order - 1], 0.5f, Colors.Safe);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Firehorn or (uint)OID.Iceclaw or (uint)OID.Thunderwing or (uint)OID.TailOfDarkness or (uint)OID.FangOfLight)
        {
            var dir = 180f.Degrees() - Angle.FromDirection(actor.Position - Arena.Center);
            var pos = (int)MathF.Round(dir.Deg / 45f) & 7;
            _dragons.Add((actor, pos));
            if (_dragons.Count == 5)
            {
                // sort by direction
                RefSort.Sort(CollectionsMarshal.AsSpan(_dragons), _comparer);
            }
        }
    }

    private readonly struct PositionComparer : IRefComparer<(Actor, int position)>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(ref (Actor, int position) a, ref (Actor, int position) b) => a.position.CompareTo(b.position);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Cauterize1 or (uint)AID.Cauterize2 or (uint)AID.Cauterize3 or (uint)AID.Cauterize4 or (uint)AID.Cauterize5)
        {
            Casters.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Cauterize1 or (uint)AID.Cauterize2 or (uint)AID.Cauterize3 or (uint)AID.Cauterize4 or (uint)AID.Cauterize5)
        {
            if (Casters.Count != 0)
            {
                Casters.RemoveAt(0);
            }
            ++NumCasts;
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

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.Cauterize && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            BaitOrder[slot] = new(++NumBaitsAssigned, WorldState.FutureTime(7.2d));

            if (NumBaitsAssigned == 1)
            {
                var count = _dragons.Count;
                var center = Arena.Center;
                var countD = 0;
                var dir = 112.5f.Degrees();
                var halfangle = 90f.Degrees();
                for (var i = 0; i < count; ++i)
                {
                    if (_dragons[i].actor.Position.InCone(center, dir, halfangle))
                    {
                        ++countD;
                    }
                }
                if (countD == 1)
                {
                    // cursed pattern: second dragon is true S; flipping standard baits horizontally will resolve the mechanic hopefully without killing anyone
                    CurrentBaits = [StandardBaits[2], StandardBaits[1], StandardBaits[0]];

                    for (var i = 0; i < 3; ++i)
                    {
                        ref var b = ref CurrentBaits[i];
                        b = new(-b.X, b.Z);
                    }
                }
                else
                {
                    CurrentBaits = StandardBaits;
                }
            }
        }
    }

    private int NextBaitOrder => (Casters.Count + NumCasts) switch
    {
        0 => 1,
        1 or 2 => 2,
        3 => 3,
        _ => 4
    };

    private Actor[] DragonsForOrder(int order)
    {
        if (_dragons.Count != 5)
        {
            return [];
        }
        return order switch
        {
            1 => [
                    _dragons[0].actor,
                    _dragons[1].actor
                 ],
            2 => [_dragons[2].actor],
            3 => [
                    _dragons[3].actor,
                    _dragons[4].actor
                 ],
            _ => [],
        };
    }
}

sealed class P2Hypernova(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Hypernova, GetVoidzones, 1.4f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidzoneHypernova);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
