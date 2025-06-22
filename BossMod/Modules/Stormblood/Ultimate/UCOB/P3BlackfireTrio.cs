namespace BossMod.Stormblood.Ultimate.UCOB;

class P3BlackfireTrio(BossModule module) : BossComponent(module)
{
    private Actor? _nael;

    public bool Active => _nael != null;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_nael, Colors.Object, true);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
        }
    }
}

class P3ThermionicBeam : Components.UniformStackSpread
{
    public P3ThermionicBeam(BossModule module) : base(module, 4, 0, 8)
    {
        var target = Raid.Player(); // note: target is random
        if (target != null)
            AddStack(target, WorldState.FutureTime(5.3d)); // assume it is activated right when downtime starts
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThermionicBeam)
            Stacks.Clear();
    }
}

class P3MegaflareTower(BossModule module) : Components.CastTowers(module, (uint)AID.MegaflareTower, 3f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(slot);
            // TODO: consider making per-tower assignments
        }
    }
}

class P3MegaflareStack(BossModule module) : Components.UniformStackSpread(module, 5f, default, 4, 4)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MegaflareStack)
        {
            if (Stacks.Count == 0)
                AddStack(actor, WorldState.FutureTime(5d), new(0xff));
            Stacks.Ref(0).ForbiddenPlayers[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MegaflareStack)
            Stacks.Clear();
    }
}
