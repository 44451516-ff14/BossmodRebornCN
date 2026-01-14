namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// draw hints based on hector guide? config file for different strats? let user handle it?
// if soaked 1st tower, hell awaits status lasts enough to prevent soaking 2nd
// need to set up party config roles to determine 1st set of towers
sealed class BloodyBondage(BossModule module) : Components.CastTowers(module, (uint)AID.BloodyBondage, 4f)
{
    private BitMask _soaked;
    //FromDirection center - tower -> 0 = north, pos = left, neg = right?

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, _soaked, Module.CastFinishAt(spell), caster.InstanceID));
        }
    }
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HellAwaits)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _soaked.Set(slot);
        }
    }
}
// TODO: only draw adds if sharing the same status; set priority based on status; any difference between 3 adds?
sealed class CharnelCells(BossModule module) : Components.AddsMulti(module, [(uint)OID.CharnelCell, (uint)OID.CharnelCell1, (uint)OID.CharnelCell2]);
sealed class BloodyBondageUndeadDeathmatch(BossModule module) : Components.CastTowers(module, (uint)AID.BloodyBondageUndeadDeathmatch, 6f, 4, 4);
// add hint on where to stand as tank (middle of long side)
sealed class UltrasonicSpreadTank(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltrasonicSpreadTank)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UltrasonicSpreadCast)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            for (var i = 0; i < len; i++)
            {
                if (party[i].Role == Role.Tank)
                {
                    // 1st party has .5s or so of status left when next baits happen
                    var status = party[i].FindStatus((uint)SID.HellAwaits);
                    if (status == null || status.Value.ExpireAt.AddSeconds(-1) < WorldState.CurrentTime)
                    {
                        CurrentBaits.Add(new(caster, party[i], new AOEShapeCone(40f, 50f.Degrees())));
                        return;
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
// TODO: DPS stay together, make it cleaner, maybe separate component to track party slots currently in cells
// add hint on where to stand if healer/dps
sealed class UltrasonicSpreadRest(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltrasonicSpreadRest)
{
    private bool _healer;
    private bool _dps;
    private readonly AOEShapeCone _cone = new(40f, 22.5f.Degrees());
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UltrasonicSpreadCast)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            for (var i = 0; i < len; i++)
            {
                var status = party[i].FindStatus((uint)SID.HellAwaits);
                if (status == null || status.Value.ExpireAt.AddSeconds(-1) < WorldState.CurrentTime)
                {
                    if (party[i].Role == Role.Healer && !_healer)
                    {
                        CurrentBaits.Add(new(caster, party[i], _cone));
                        _healer = true;
                        if (_healer && _dps)
                            return;
                    }
                    else if (party[i].Role is Role.Melee or Role.Ranged && !_dps)
                    {
                        CurrentBaits.Add(new(caster, party[i], _cone));
                        _dps = true;
                        if (_healer && _dps)
                            return;
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _healer = false;
            _dps = false;
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

// TODO: stack for non-cell party, bait away from cells, default to tank for now
// add hint for non-cell party where to stand (middle of long side)
sealed class UltrasonicAmp(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltrasonicAmp)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.UltrasonicAmpCast)
        {
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;

            for (var i = 0; i < len; i++)
            {
                if (party[i].Role == Role.Tank)
                {
                    if (party[i].FindStatus((uint)SID.HellAwaits) == null)
                    {
                        CurrentBaits.Add(new(caster, party[i], new AOEShapeCone(40f, 50f.Degrees())));
                        return;
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}
