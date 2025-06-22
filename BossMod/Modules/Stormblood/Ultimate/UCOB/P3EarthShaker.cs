namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker(BossModule module) : Components.GenericBaitAway(module, (uint)AID.EarthShakerAOE)
{
    private List<Bait> _futureBaits = [];

    private static readonly AOEShapeCone _shape = new(60f, 45f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker && Module.Enemies((uint)OID.BahamutPrime)[0] is var source && source != null)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(source, actor, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.EarthShaker)
        {
            CurrentBaits.Clear();
            Utils.Swap(ref CurrentBaits, ref _futureBaits);
        }
    }
}

class P3EarthShakerVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly List<Actor> _voidzones = module.Enemies((uint)OID.VoidzoneEarthShaker);
    private readonly List<AOEInstance> _predicted = [];
    private BitMask _targets;

    private static readonly AOEShapeCircle _shape = new(5); // TODO: verify radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _voidzones.Count;
        var voidzones = new List<Actor>(count);

        for (var i = 0; i < count; ++i)
        {
            var z = _voidzones[i];
            if (z.EventState != 7)
                voidzones.Add(z);
        }
        var countV = voidzones.Count;
        var aoes = new List<AOEInstance>(countV);
        for (var i = 0; i < countV; ++i)
            aoes.Add(new(_shape, voidzones[i].Position));
        aoes.AddRange(_predicted);
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.VoidzoneEarthShaker)
            _predicted.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
            _targets[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EarthShaker)
            foreach (var (_, p) in Raid.WithSlot(false, true, true).IncludedInMask(_targets))
                _predicted.Add(new(_shape, p.Position, default, WorldState.FutureTime(1.4d)));
    }
}
