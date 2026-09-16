namespace BossMod.Stormblood.Ultimate.UCOB;

sealed class P3EarthShaker(UCOB module) : Components.GenericBaitAway(module, (uint)AID.EarthShakerAOE)
{
    private List<Bait> _futureBaits = [];
    private readonly Actor _bahamut = module.BahamutPrime()!;
    private P3QuickmarchTrio? _trio;

    private readonly AOEShapeCone _shape = new(60f, 45f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(_bahamut, actor, _shape));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count > 0)
        {
            if (_trio == null)
            {
                var comp = Module.FindComponent<P3QuickmarchTrio>();
                if (comp != null)
                {
                    _trio = comp;
                }
                else
                {
                    return;
                }
            }
            var dirNorth = (_trio.RelativeNorth - Arena.Center).ToAngle();
            var baitIndex = -1;

            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            var len = baits.Length;
            for (var i = 0; i < len; ++i)
            {
                if (baits[i].Target == actor)
                {
                    baitIndex = i;
                    break;
                }
            }

            if (baitIndex >= 0)
            {
                ref var bait = ref baits[baitIndex];

                var safeDir = assignment switch
                {
                    PartyRolesConfig.Assignment.H1 => dirNorth + 45f.Degrees(),
                    PartyRolesConfig.Assignment.H2 => dirNorth - 45f.Degrees(),
                    _ => dirNorth - 135f.Degrees()
                };

                var act = bait.Activation;
                var center = Arena.Center;
                hints.AddForbiddenZone(new SDInvertedRect(center, safeDir, 60f, 0f, 1f), act);
                hints.AddForbiddenZone(new SDCircle(center, 6f), act);

                // healers should move closer to arena center to be in range of the whole party,
                // in case i.e. R2 gets hit by megaflare
                hints.GoalZones.Add(AIHints.GoalSingleTarget(center, 10f, 0.5f));
            }
            else if (actor.Role != Role.Tank)
            {
                hints.AddForbiddenZone(new SDInvertedRect(Arena.Center, dirNorth + 135f.Degrees(), 60f, 0f, 1f), baits[0].Activation);
            }

            BitMask damage = default;
            for (var i = 0; i < len; ++i)
            {
                damage.Set(Raid.FindSlot(baits[i].Target.InstanceID));
            }

            if (damage.Any())
            {
                hints.AddPredictedDamage(damage, baits[0].Activation);
            }

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.EarthShaker)
        {
            CurrentBaits.Clear();
            Utils.Swap(ref CurrentBaits, ref _futureBaits);
        }
    }
}

sealed class P3EarthShakerVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 4f, (uint)AID.EarthShakerAOE, GetVoidzones, 1.4d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidzoneEarthShaker);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                voidzones[index++] = z;
            }
        }
        return voidzones[..index];
    }

    readonly List<Actor> Targets = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
        {
            Targets.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction && Targets.Count > 0)
        {
            _predictedByEvent.Add((Targets[0].Position, WorldState.FutureTime(CastEventToSpawn)));
            Targets.RemoveAt(0);
        }
    }
}
