namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class AbsoluteAuthorityCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbsoluteAuthorityCircle), new AOEShapeCircle(8));

class AbsoluteAuthorityFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(12), (uint)IconID.Flare, ActionID.MakeSpell(AID.AbsoluteAuthorityFlare), 6, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }
}

class AbsoluteAuthorityDorito(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.DoritoStack && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1f)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var player = Raid.Player()!;
            var sort = Raid.WithoutSlot().Exclude(player).OrderBy(a => (player.Position - a.Position).LengthSq());
            var actor = sort.FirstOrDefault(x => !(Module.FindComponent<AbsoluteAuthorityCircle>()?.ActiveAOEs(0, x).Any(z => z.Shape.Check(x.Position, z.Origin, z.Rotation) && z.Risky) ?? false));
            Stacks[0] = Stacks[0] with { Target = actor ?? player };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbsoluteAuthorityDoritoStack1 or AID.AbsoluteAuthorityDoritoStack2)
            Stacks.Clear();
    }
}

class AuthoritysHold(BossModule module) : Components.StayMove(module, 3)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class AuthoritysGaze(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        foreach (var a in _affected)
            if (_affected.Count > 0 && WorldState.CurrentTime > _activation.AddSeconds(-10))
                yield return new(a.Position, _activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AuthoritysGaze)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AuthoritysGaze)
            _affected.Remove(actor);
    }
}
