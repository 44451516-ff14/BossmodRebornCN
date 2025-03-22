namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class AbsoluteAuthorityCircle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AbsoluteAuthorityCircle), 8f);

class AbsoluteAuthorityFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(12), (uint)IconID.Flare, ActionID.MakeSpell(AID.AbsoluteAuthorityFlare), 6f, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }
}

class AbsoluteAuthorityDorito(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly AbsoluteAuthorityCircle _aoe = module.FindComponent<AbsoluteAuthorityCircle>()!;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DoritoStack && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1d)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var player = Raid.Player()!;
            var party = Raid.WithoutSlot(false, true, true);
            Array.Sort(party, (a, b) =>
            {
                var distA = (player.Position - a.Position).LengthSq();
                var distB = (player.Position - a.Position).LengthSq();
                return distA.CompareTo(distB);
            });
            var len = party.Length;
            Actor? closest = null;
            var aoes = _aoe.ActiveAOEs(default, player);
            var lenaoes = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p == player)
                    continue;
                for (var j = 0; j < lenaoes; ++j)
                {
                    if (!aoes[i].Check(p.Position))
                    {
                        closest = p;
                        break;
                    }
                }
            }
            Stacks[0] = Stacks[0] with { Target = closest ?? player };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AbsoluteAuthorityDoritoStack1 or (uint)AID.AbsoluteAuthorityDoritoStack2)
            Stacks.Clear();
    }
}

class AuthoritysHold(BossModule module) : Components.StayMove(module, 3)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.AuthoritysHold && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class AuthoritysGaze(BossModule module) : Components.GenericGaze(module)
{
    private DateTime _activation;
    private readonly List<Actor> _affected = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _affected.Count;
        if (count == 0 || WorldState.CurrentTime < _activation.AddSeconds(-10d))
            return [];
        var eyes = new Eye[count];
        for (var i = 0; i < count; ++i)
            eyes[i] = new(_affected[i].Position, _activation);
        return eyes;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysGaze)
        {
            _activation = status.ExpireAt;
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AuthoritysGaze)
            _affected.Remove(actor);
    }
}
