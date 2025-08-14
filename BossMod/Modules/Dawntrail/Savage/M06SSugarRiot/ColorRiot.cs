namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class ColorRiot(BossModule module) : Components.GenericBaitAway(module, default, true, true, true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCircle circle = new(4);
    private bool? warmClose;
    private DateTime activation;
    private BitMask warmPlayers;
    private BitMask coolPlayers;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (warmClose == null)
            return;
        var (closest, furthest) = GetTargets();
        if (closest != null)
            CurrentBaits.Add(new(Module.PrimaryActor, closest, circle, activation));

        if (furthest != null)
            CurrentBaits.Add(new(Module.PrimaryActor, furthest, circle, activation));
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (warmClose != null)
            hints.Add($"Proximity tankbusters");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (warmClose is not bool isWarmClose)
            return;

        var (closest, furthest) = GetTargets();
        base.AddHints(slot, actor, hints);

        if (actor.Role != Role.Tank)
        {
            if (actor == closest)
            {
                hints.Add("Move further away from boss!");
                return;
            }
            if (actor == furthest)
            {
                hints.Add("Move closer to boss!");
                return;
            }
            return;
        }

        if (!warmPlayers.Any() && !coolPlayers.Any())
        {
            hints.Add("Bait close or far!", actor != closest && actor != furthest);
            return;
        }

        var isWarm = warmPlayers[slot];
        var isCool = coolPlayers[slot];
        var isClosest = actor == closest;
        var isFurthest = actor == furthest;

        if (closest == null || furthest == null)
            return;

        if (isWarm)
        {
            hints.Add(isWarmClose ? "Bait far!" : "Bait close!", isWarmClose ? !isFurthest : !isClosest);
        }
        else if (isCool)
        {
            hints.Add(isWarmClose ? "Bait close!" : "Bait far!", isWarmClose ? !isClosest : !isFurthest);
        }
        else
        {
            hints.Add("Go opposite distance of co-tank!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ColorRiot1 or (uint)AID.ColorRiot2)
        {
            warmClose = spell.Action.ID == (uint)AID.ColorRiot2;
            activation = WorldState.FutureTime(7.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WarmBomb or (uint)AID.CoolBomb)
        {
            warmClose = null;
            ++NumCasts;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CoolTint)
        {
            coolPlayers[Raid.FindSlot(actor.InstanceID)] = true;
        }
        else if (status.ID == (uint)SID.WarmTint)
        {
            warmPlayers[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CoolTint)
        {
            coolPlayers[Raid.FindSlot(actor.InstanceID)] = false;
        }
        else if (status.ID == (uint)SID.WarmTint)
        {
            warmPlayers[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }

    private (Actor?, Actor?) GetTargets()
    {
        var party = Raid.WithoutSlot(false, true, true);
        var source = Module.PrimaryActor;
        var len = party.Length;

        Actor? closest = null;
        Actor? furthest = null;
        var closestDistSq = float.MaxValue;
        var furthestDistSq = float.MinValue;

        for (var j = 0; j < len; ++j)
        {
            ref readonly var player = ref party[j];

            var distSq = (player.Position - source.Position).LengthSq();

            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closest = player;
            }

            if (distSq > furthestDistSq)
            {
                furthestDistSq = distSq;
                furthest = player;
            }
        }
        return (closest, furthest);
    }
}
