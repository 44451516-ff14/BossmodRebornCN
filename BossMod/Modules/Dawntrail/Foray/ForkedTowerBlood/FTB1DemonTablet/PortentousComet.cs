namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class PortentousCometeor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PortentousCometeor, 43f);

sealed class PortentousCometeorBait(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true)
{
    private static readonly AOEShapeCircle circle = new(43f);
    private Actor? meteor;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PortentousCometeor)
        {
            meteor = actor;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.CraterLater)
        {
            CurrentBaits.Add(new(actor, actor, circle, WorldState.FutureTime(12d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PortentousCometeor)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0 && meteor != null)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(meteor.Position, 1f), CurrentBaits[0].Activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.Add("Take bait to side with meteor!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (ActiveBaitsOn(pc).Count != 0 && meteor != null)
        {
            Arena.AddCircle(meteor.Position, 2f, Colors.Vulnerable, 2f);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (ActiveBaitsOn(actor).Count != 0 && meteor != null)
        {
            movementHints.Add(actor.Position, meteor.Position, Colors.Safe);
        }
    }
}

sealed class PortentousCometKnockback(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<(Actor target, Angle dir)> targets = new(4);
    private DateTime activation;
    private readonly LandingKnockback _kb = module.FindComponent<LandingKnockback>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kb.Casters.Count != 0)
            return [];
        var count = targets.Count;
        if (count == 0)
            return [];
        var pos = actor.Position;
        var center = Arena.Center;
        var dirRect = new WDir(default, 1f);
        var isTarget = false;
        for (var i = 0; i < count; ++i)
        {
            if (targets[i].target == actor)
            {
                isTarget = true;
                break;
            }
        }
        for (var i = 0; i < count; ++i)
        {
            var kb = targets[i];
            if (isTarget && kb.target == actor || !isTarget && pos.InCircle(kb.target.Position, 4f)) // only draw one knockback since they cant be chained, give priority to actor's own knockback
            {
                Span<Knockback> knockback = new Knockback[1];
                // the knockback range for knockbacks away from meteor side does not seem very consistent. Theory: if the knockback ends up inside the demon tablet, it gets extended to land 3y behind the wall
                knockback[0] = new Knockback(kb.target.Position, 13f, activation, circle, kb.dir, Kind: Kind.DirForward);
                var dir = kb.dir.ToDirection();
                if ((pos + 13f * dir).InRect(center, dirRect, 3f, 3f, 15f))
                {
                    knockback[0].Distance = (center + 6f * dir - pos).Length();
                }
                return knockback;
            }
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2 && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            targets.Add((target, id == (uint)AID.PortentousComet1 ? 180f.Degrees() : default));
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PortentousComet1 or (uint)AID.PortentousComet2 && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            var count = targets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (targets[i].target == target)
                {
                    targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class PortentousComet1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PortentousComet1, 4f, 12);
sealed class PortentousComet2(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.PortentousComet2, 4f, 12);
