namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class AbsoluteAuthorityPuddles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteAuthorityPuddlesAOE, 8f);

sealed class AbsoluteAuthorityExpansionBoot(BossModule module) : Components.UniformStackSpread(module, 6f, 15f, 4, alwaysShowSpreads: true) // TODO: verify falloff
{
    public int NumCasts;
    private readonly Ex3QueenEternalConfig _config = Service.Config.Get<Ex3QueenEternalConfig>();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AuthoritysExpansion:
                if (!_config.AbsoluteAuthorityIgnoreFlares)
                    AddSpread(actor, status.ExpireAt);
                break;
            case (uint)SID.AuthoritysBoot:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AbsoluteAuthorityExpansion or (uint)AID.AbsoluteAuthorityBoot)
        {
            ++NumCasts;
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

sealed class AbsoluteAuthorityHeel(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AuthoritysHeel && Stacks.Count == 0)
            Stacks.Add(new(actor, 1.5f, 8, 8, activation: WorldState.FutureTime(5.1d)));
    }

    public override void Update()
    {
        if (Stacks.Count != 0)
        {
            var player = Raid.Player()!;
            var actor = Raid.WithoutSlot(false, true, true).Exclude(player).OrderBy(a => (player.Position - a.Position).LengthSq()).First();
            Stacks[0] = Stacks[0] with { Target = actor ?? player };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AbsoluteAuthorityHeel or (uint)AID.AbsoluteAuthorityHeelFail)
        {
            Stacks.Clear();
            ++NumCasts;
        }
    }
}

sealed class AbsoluteAuthorityKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AbsoluteAuthorityKnockback, 30f, kind: Kind.DirForward);
