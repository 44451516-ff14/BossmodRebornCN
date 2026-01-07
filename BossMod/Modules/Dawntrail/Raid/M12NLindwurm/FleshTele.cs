using TerraFX.Interop.Windows;

namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class FleshTele(BossModule module) : BossComponent(module)
{
    private readonly RavenousReach _reach = module.FindComponent<RavenousReach>()!;
    private readonly Burst _burst = module.FindComponent<Burst>()!;
    private bool _active = false;
    private readonly Dictionary<ulong, bool> _jumps = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _active = false;
            _jumps.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FleshTimer)
            _active = true;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.FleshForward or (uint)SID.FleshBack)
        {
            _jumps[actor.InstanceID] = status.ID is (uint)SID.FleshForward;
        }
    }

    public override void OnActorUntargetable(Actor actor)
    {
        if (actor.OID == Module.PrimaryActor.OID)
        {
            _active = false;
            _jumps.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;
        if (!_jumps.ContainsKey(pc.InstanceID))
            return;

        var tp = GetTeleport(pc, _jumps[pc.InstanceID]);
        Arena.ActorProjected(tp.from, tp.to, tp.rotation, Colors.Danger);
        Arena.AddLine(tp.from, tp.to);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        if (!_jumps.ContainsKey(actor.InstanceID))
            return;

        var tp = GetTeleport(actor, _jumps[actor.InstanceID]);
        if (DestinationUnsafe(tp.to))
            hints.Add("About to get teleported into danger!");
    }

    private bool DestinationUnsafe(WPos pos)
    {
        if (_reach.ActiveCasters.Length > 0)
        {
            foreach(var c in _reach.ActiveCasters)
            {
                if (c.Check(pos))
                    return true;
            }
        }

        if (_burst.ActiveCasters.Length > 0)
        {
            foreach(var c in _burst.ActiveCasters)
            {
                if (c.Check(pos))
                    return true;
            }
        }

        return !Arena.InBounds(pos);
    }

    private (WPos from, WPos to, Angle rotation) GetTeleport(Actor actor, bool isForward)
    {
        var pos = actor.Position;
        var dir = actor.Rotation;

        var projected = pos + ((dir.ToDirection() * 15f) * (isForward ? 1f : -1f));
        return (pos, projected, dir);
    }
}
