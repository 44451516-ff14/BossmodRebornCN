namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class HuntersHarvestBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HuntersHarvest)
{
    public static readonly AOEShapeCone Cone = new(40f, 105f.Degrees());
    public BitMask Bind;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind && actor.Role == Role.Tank)
        {
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Role == Role.Tank && p != actor)
                {
                    CurrentBaits.Add(new(Module.Enemies((uint)OID.BossP2)[0], p, Cone, WorldState.FutureTime(10.3d)));
                }
            }
        }
    }

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        if (len == 0)
            return;
        ref var b = ref baits[0];
        for (var i = 0; i < 5; ++i)
        {
            var center = ArenaChanges.EndArenaPlatforms[i].Center;
            if (b.Target.Position.InCircle(center, 8f))
            {
                b.CustomRotation = ArenaChanges.PlatformAngles[i];
                return;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Bind)
            Bind[Raid.FindSlot(actor.InstanceID)] = true;
    }
}

sealed class HuntersHarvest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersHarvest, HuntersHarvestBait.Cone);

sealed class GeotemporalBlast(BossModule module) : Components.GenericBaitAway(module, (uint)AID.GeotemporalBlast, tankbuster: true)
{
    private static readonly AOEShapeCircle circle = new(16);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind && actor.Role == Role.Tank)
        {
            CurrentBaits.Add(new(actor, actor, circle, WorldState.FutureTime(10.3d)));
        }
    }
}

sealed class AerotemporalBlast(BossModule module) : Components.GenericBaitStack(module, (uint)AID.AerotemporalBlast)
{
    private static readonly AOEShapeCircle circle = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StalkingStoneWind)
        {
            if (actor.Role == Role.Healer)
            {
                var party = Raid.WithSlot(true, true, true);
                var len = party.Length;
                BitMask forbidden = default;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var p = ref party[i];
                    if (p.Item2.Role == Role.Tank)
                        forbidden[p.Item1] = true;
                }
                CurrentBaits.Add(new(actor, actor, circle, WorldState.FutureTime(10.3d), forbidden));
            }
        }
    }
}
