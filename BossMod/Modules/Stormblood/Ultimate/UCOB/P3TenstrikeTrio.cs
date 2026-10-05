namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3TenstrikeMeteorStream : MeteorStream
{
    public bool HatchAssigned;
    int _numHatches;
    private readonly int[] _order = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public P3TenstrikeMeteorStream(BossModule module) : base(module)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(3.2d));
        var assignments = CollectionsMarshal.AsSpan(Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid));
        var len = assignments.Length;

        for (var i = 0; i < len; ++i)
        {
            var a = assignments[i];
            _order[a.slot] = a.group;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!HatchAssigned)
        {
            var order = _order[slot];
            if (order >= 0)
            {
                var sign = order > 3 ? -1 : 1;

                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + (180f + (22.5f + 45f * (order & 3)) * sign).Degrees().ToDirection() * 9f, 1f));
            }

            return;
        }

        if (Module.FindComponent<Hatch>()?.IsTarget(slot) == true)
            return;

        if (_numHatches >= 6)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 8f));
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.MeteorStream)
        {
            ++NumCasts;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            var len = spreads.Length;
            var tid = spell.MainTargetID;
            for (var i = 0; i < len; ++i)
            {
                if (spreads[i].Target.InstanceID == tid)
                {
                    Spreads.RemoveAt(i);
                    break;
                }
            }

            spreads = CollectionsMarshal.AsSpan(Spreads);
            len = spreads.Length;
            var act = WorldState.FutureTime(1d);

            for (var i = 0; i < len; ++i)
            {
                spreads[i].Activation = act;
            }
        }
        else if (id == (uint)AID.Hatch)
        {
            ++_numHatches;
        }
    }
}
