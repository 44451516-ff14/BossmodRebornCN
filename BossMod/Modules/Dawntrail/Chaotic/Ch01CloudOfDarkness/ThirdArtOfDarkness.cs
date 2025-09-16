namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class ThirdArtOfDarknessCleave(BossModule module) : Components.GenericAOEs(module)
{
    public enum Mechanic { None, Left, Right, Stack, Spread }

    public readonly Dictionary<Actor, List<(Mechanic mechanic, DateTime activation)>> Mechanics = [];
    public BitMask PlatformPlayers;

    private static readonly AOEShapeCone _shape = new(15f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Mechanics.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var index = 0;
        foreach (var (caster, m) in Mechanics)
        {
            var dir = m.Count == 0 ? default : m[0].mechanic switch
            {
                Mechanic.Left => 90f.Degrees(),
                Mechanic.Right => -90f.Degrees(),
                _ => default
            };
            if (dir != default)
                aoes[index++] = new(_shape, caster.Position.Quantized(), caster.Rotation + dir, m[0].activation);
        }
        return aoes.AsSpan()[..index];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (PlatformPlayers[slot])
        {
            var playerSide = actor.PosRot.X - Arena.Center.X;

            Actor? matchingActor = null;
            List<(Mechanic mechanic, DateTime activation)>? matchingMechanics = null;

            foreach (var kv in Mechanics)
            {
                var actorSide = kv.Key.PosRot.X - Arena.Center.X;
                if (actorSide * playerSide > 0)
                {
                    matchingActor = kv.Key;
                    matchingMechanics = kv.Value;
                    break;
                }
            }

            if (matchingActor != null && matchingMechanics != null && matchingMechanics.Count > 0)
            {
                var order = "";
                var count = matchingMechanics.Count;
                var mechs = CollectionsMarshal.AsSpan(matchingMechanics);
                for (var i = 0; i < count; ++i)
                {
                    ref var m = ref mechs[i];
                    if (i > 0)
                        order += " > ";
                    order += m.mechanic.ToString();
                }
                hints.Add($"Order: {order}", false);
            }
        }

        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.OuterDarkness)
            PlatformPlayers.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.OuterDarkness)
            PlatformPlayers.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID == (uint)OID.StygianShadow)
        {
            var mechanic = iconID switch
            {
                (uint)IconID.ThirdArtOfDarknessLeft => Mechanic.Left,
                (uint)IconID.ThirdArtOfDarknessRight => Mechanic.Right,
                (uint)IconID.ThirdArtOfDarknessStack => Mechanic.Stack,
                (uint)IconID.ThirdArtOfDarknessSpread => Mechanic.Spread,
                _ => Mechanic.None
            };
            if (mechanic != Mechanic.None)
                Mechanics.GetOrAdd(actor).Add((mechanic, WorldState.FutureTime(9.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var mechanic = spell.Action.ID switch
        {
            (uint)AID.ArtOfDarknessAOEL => Mechanic.Left,
            (uint)AID.ArtOfDarknessAOER => Mechanic.Right,
            (uint)AID.HyperFocusedParticleBeamAOE => Mechanic.Spread,
            (uint)AID.MultiProngedParticleBeamAOE => Mechanic.Stack,
            _ => Mechanic.None
        };
        if (mechanic != Mechanic.None)
        {
            var (a, m) = Mechanics.FirstOrDefault(kv => kv.Key.Position.AlmostEqual(caster.Position, 1f) && kv.Value.Count > 0 && kv.Value[0].mechanic == mechanic);
            if (a != null)
            {
                ++NumCasts;
                m.RemoveAt(0);
                if (m.Count == 0)
                    Mechanics.Remove(a);
            }
        }
    }
}

sealed class ThirdArtOfDarknessHyperFocusedParticleBeam(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly ThirdArtOfDarknessCleave? _main = module.FindComponent<ThirdArtOfDarknessCleave>();

    private static readonly AOEShapeRect _shape = new(22, 2.5f);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_main != null)
            foreach (var (a, m) in _main.Mechanics)
                if (m.Count > 0 && m[0].mechanic == ThirdArtOfDarknessCleave.Mechanic.Spread)
                    foreach (var p in Raid.WithoutSlot(false, false, true).SortedByRange(a.Position).Take(6))
                        CurrentBaits.Add(new(a, p, _shape, m[0].activation));
    }
}

sealed class ThirdArtOfDarknessMultiProngedParticleBeam(BossModule module) : Components.UniformStackSpread(module, 3f, default, 2)
{
    private readonly ThirdArtOfDarknessCleave? _main = module.FindComponent<ThirdArtOfDarknessCleave>();

    public override void Update()
    {
        Stacks.Clear();
        if (_main != null)
            foreach (var (a, m) in _main.Mechanics)
                if (m.Count > 0 && m[0].mechanic == ThirdArtOfDarknessCleave.Mechanic.Stack)
                    foreach (var p in Raid.WithoutSlot(false, false, true).SortedByRange(a.Position).Take(3))
                        AddStack(p, m[0].activation, ~_main.PlatformPlayers);
        base.Update();
    }
}
