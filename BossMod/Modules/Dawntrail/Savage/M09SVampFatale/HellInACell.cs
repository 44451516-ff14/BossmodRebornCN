using Dalamud.Game.ClientState.JobGauge.Enums;
using TerraFX.Interop.Windows;

namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// draw hints based on hector guide? config file for different strats? let user handle it?
// if soaked 1st tower, hell awaits status lasts enough to prevent soaking 2nd
// need to set up party config roles to determine 1st set of towers
sealed class BloodyBondage(BossModule module) : Components.CastTowers(module, (uint)AID.BloodyBondage, 4f, 1, 1)
{
    private static readonly M09SVampFataleConfig _config = Service.Config.Get<M09SVampFataleConfig>();
    private static readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();
    private BitMask _soaked;
    private readonly string[] _towerLabel = ["T", "H", "M", "R"];

    // (towers[i].Position - Arena.Center).ToAngle().Deg, Util/Angle says S=0, W=-90, E=90, N=-/+180
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, _soaked, Module.CastFinishAt(spell), caster.InstanceID));

            if (Towers.Count == 4)
            {
                Towers.Sort(delegate (Tower x, Tower y)
                {
                    var north = Angle.AnglesCardinals[2];
                    var xAngle = (x.Position - Arena.Center).ToAngle();
                    var yAngle = (y.Position - Arena.Center).ToAngle();

                    var xDeg = xAngle.AlmostEqual(north, 0.01f) ? 180f : xAngle.Deg;
                    var yDeg = yAngle.AlmostEqual(north, 0.01f) ? 180f : yAngle.Deg;

                    return xDeg < yDeg ? 1 : -1;
                });
            }
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
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_config.ShowTowerOrder)
        {
            base.DrawArenaForeground(pcSlot, pc);
            return;
        }

        if (Towers.Count != 4)
            return;

        var index = GetTowerIndex(pcSlot);
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;

        // couldn't get tower index (party not configured or index error)
        if (index == -1)
        {
            //base.DrawArenaForeground(pcSlot, pc);
            for (var i = 0; i < len; i++)
            {
                var isInside = towers[i].IsInside(pc);
                var numInside = towers[i].NumInside(Module);

                if (isInside && numInside == 1)
                {
                    towers[i].Shape.Outline(Arena, towers[i].Position, towers[i].Rotation, Colors.Safe);
                }
                else if (isInside && numInside > 1)
                {
                    towers[i].Shape.Draw(Arena, towers[i].Position, towers[i].Rotation, default);
                }
                else if (numInside == 0)
                {
                    towers[i].Shape.Outline(Arena, towers[i].Position, towers[i].Rotation, default, 2f);
                    Arena.TextWorld(towers[i].Position, _towerLabel[i], Colors.Safe);
                }
            }
        }
        else
        {
            var actorParty = (NumCasts / 4 + 1) == GetLightParty(pcSlot);

            // not in light party that's doing towers
            if (!actorParty)
            {
                for (var i = 0; i < len; i++)
                {
                    towers[i].Shape.Draw(Arena, towers[i].Position, towers[i].Rotation, default);
                }
                return;
            }

            ref readonly var mytower = ref towers[index];
            var isInside = mytower.IsInside(pc);
            mytower.Shape.Outline(Arena, mytower.Position, mytower.Rotation, isInside ? Colors.Safe : default, 2f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_config.ShowTowerOrder)
        {
            base.AddHints(slot, actor, hints);
            return;
        }

        if (Towers.Count != 4)
            return;

        var index = GetTowerIndex(slot);

        // couldn't get tower index (party not configured or index error)
        if (index == -1)
        {
            base.AddHints(slot, actor, hints);
            return;
        }
        else
        {
            var actorParty = (NumCasts / 4 + 1) == GetLightParty(slot);

            // not in light party that's doing towers
            if (!actorParty)
                return;

            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.EnableTowerOrder)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (Towers.Count != 4)
            return;

        var index = GetTowerIndex(slot);
        var towers = CollectionsMarshal.AsSpan(Towers);

        // couldn't get tower index (party not configured or index error)
        if (index == -1)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }
        else
        {
            var actorParty = (NumCasts / 4 + 1) == GetLightParty(slot);

            // not in light party that's doing towers
            if (!actorParty)
            {
                var len = towers.Length;
                for (var i = 0; i < len; i++)
                {
                    hints.AddForbiddenZone(towers[i].Shape, towers[i].Position, default, towers[i].Activation);
                }
            }
            else
            {
                // melee sacrifices uptime if they immediately move to towers
                // inverted forbidden zone an option to delay movement?
                ref readonly var t = ref towers[index];
                hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position, 3f, 9f));
            }
        }
    }

    private int GetTowerIndex(int pcSlot)
    {
        if (_partyConfig.SlotsPerAssignment(Raid).Length == 0)
            return -1;

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        var index = assignment switch
        {
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => 0,
            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.H2 => 1,
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => 2,
            PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2 => 3,
            _ => -1
        };

        return index;
    }

    private int GetLightParty(int pcSlot)
    {
        if (_partyConfig.SlotsPerAssignment(Raid).Length == 0)
            return 0;

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        var lp = assignment switch
        {
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.H1 => 1,
            PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H2 => 2,
            _ => 0
        };

        return lp;
    }
}
// TODO: only draw adds if sharing the same status; set priority based on status; any difference between 3 adds?
// draw circle around cell that player is in; if player isn't inside cell, draw circles around all cells to avoid
sealed class CharnelCells(BossModule module) : Components.AddsMulti(module, [(uint)OID.CharnelCell, (uint)OID.CharnelCell1, (uint)OID.CharnelCell2]);
sealed class BloodyBondageUndeadDeathmatch(BossModule module) : Components.CastTowers(module, (uint)AID.BloodyBondageUndeadDeathmatch, 6f, 4, 4)
{
    private static readonly M09SVampFataleConfig _config = Service.Config.Get<M09SVampFataleConfig>();
    private static readonly PartyRolesConfig _partyConfig = Service.Config.Get<PartyRolesConfig>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_config.EnableDeathmatch)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (Towers.Count != 2)
            return;

        var lightparty = GetLightParty(slot);
        if (lightparty == 0)
            return;

        Tower[] lpTowers = new Tower[2];
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;

        for (var i = 0; i < len; i++)
        {
            var angle = (towers[i].Position - Arena.Center).ToAngle();
            if (angle.AlmostEqual(Angle.AnglesCardinals[0], 0.01f) || angle.AlmostEqual(Angle.AnglesCardinals[2], 0.01f))
            {
                lpTowers[0] = towers[i];
            }
            else if (angle.AlmostEqual(Angle.AnglesCardinals[1], 0.01f) || angle.AlmostEqual(Angle.AnglesCardinals[3], 0.01f))
            {
                lpTowers[1] = towers[i];
            }
        }

        hints.GoalZones.Add(AIHints.GoalSingleTarget(lpTowers[lightparty - 1].Position, 5f, 9f));
    }

    private int GetLightParty(int pcSlot)
    {
        if (_partyConfig.SlotsPerAssignment(Raid).Length == 0)
            return 0;

        var assignment = _partyConfig[Raid.Members[pcSlot].ContentId];
        var lp = assignment switch
        {
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.H1 => 1,
            PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 or PartyRolesConfig.Assignment.H2 => 2,
            _ => 0
        };

        return lp;
    }
}
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
