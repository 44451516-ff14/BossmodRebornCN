namespace BossMod.Endwalker.Ultimate.DSW1;

class PureOfHeartBrightwing(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _shape = new(18, 15.Degrees()); // TODO: verify angle

    public override void Update()
    {
        CurrentBaits.Clear();
        if (NumCasts < 8)
            foreach (var source in Module.Enemies(OID.SerCharibert))
                foreach (var target in Raid.WithoutSlot(false, true, true).SortedByRange(source.Position).Take(2))
                    CurrentBaits.Add(new(source, target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Brightwing)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

class PureOfHeartSkyblindBait(BossModule module) : BossComponent(module)
{
    private BitMask _baiters;

    private const float _radius = 3;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baiters[slot] && Raid.WithSlot(false, true, true).ExcludedFromMask(_baiters).InRadius(actor.Position, _radius).Any())
            hints.Add("GTFO from raid!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (_, player) in Raid.WithSlot(false, true, true).IncludedInMask(_baiters))
            Arena.AddCircle(player.Position, _radius, Colors.Danger);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Skyblind)
            _baiters.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Skyblind)
            _baiters.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class PureOfHeartSkyblind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skyblind, 3);
