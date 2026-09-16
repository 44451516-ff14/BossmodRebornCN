namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P1Fireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, (uint)AID.Fireball, 4f, 5.3d, 4, 4)
{
    private int _neurolinkCount;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly UCOBConfig _config = Service.Config.Get<UCOBConfig>();

    public WPos Destination;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == StackIcon)
        {
            var slot = Raid.FindSlot(actor.InstanceID);

            if (_neurolinkCount == 0)
            {
                BitMask forbidden = default;

                if (_config.P1Fireball1LBHints)
                {
                    var assignments = _prc.AssignmentsPerSlot(Raid);
                    var len = assignments.Length;
                    if (len > 0)
                    {
                        var hAvoid = PartyRolesConfig.Assignment.H1;
                        if (assignments[slot] == hAvoid)
                        {
                            hAvoid = PartyRolesConfig.Assignment.H2;
                        }

                        for (var i = 0; i < len; ++i)
                        {
                            if (assignments[i] is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT || assignments[i] == hAvoid)
                            {
                                forbidden.Set(i);
                            }
                        }
                    }
                }

                AddStack(actor, WorldState.FutureTime(5.3d), forbidden);

                Destination = Module.PrimaryActor.Position + Module.PrimaryActor.DirectionTo(Arena.Center) * (Module.PrimaryActor.HitboxRadius + 3f);
            }
            else
            {
                // stack activation is quite delayed, seen 7-7.5 seconds
                AddStack(actor, WorldState.FutureTime(7d));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Neurolink)
        {
            ++_neurolinkCount;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == StackAction)
        {
            Stacks.Clear();
            Destination = default;
        }
        else if (id == (uint)AID.LiquidHell && Destination == default && _neurolinkCount > 0)
        {
            var bossToPuddle = spell.TargetXZ - Module.PrimaryActor.Position;
            var safeDist = Math.Max(Module.PrimaryActor.HitboxRadius, 7f - bossToPuddle.Length());
            // stack on opposite side of boss from first fireball spawn
            Destination = Module.PrimaryActor.Position + bossToPuddle.Normalized() * -safeDist;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Destination == default)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        if (Stacks.Count == 0)
        {
            return;
        }

        var baiter = Module.FindComponent<P1LiquidHell>()?.Baiter;

        if (EnableHints || baiter != null && baiter != actor)
        {
            ref var stack = ref Stacks.Ref(0);

            if (stack.Target == actor) // stack target shouldn't move around too much, just plant on boss
            {
                hints.AddForbiddenZone(new SDPrecisePosition(Destination, new(0f, 1f), 0.5f, actor.Position, 0.1f), stack.Activation);
            }
            else if (!stack.ForbiddenPlayers[slot])
            {
                hints.AddForbiddenZone(new SDInvertedCircle(Destination, stack.Radius), stack.Activation);
            }
            else
            {
                hints.AddForbiddenZone(new SDCircle(Destination, stack.Radius), stack.Activation);
            }
        }
    }
}