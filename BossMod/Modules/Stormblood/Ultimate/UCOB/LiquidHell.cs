namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class LiquidHellBase(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.LiquidHell, m => m.Enemies((uint)OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3d) // 1.8
{
    public int NumSources => Sources(Module).Count();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // we only add hints for spawned fireballs since the activation is so delayed
        // this helps party not kill themselves during blackfire trio, and gives ranged lots of extra room in p1
        foreach (var z in Sources(Module))
            hints.AddForbiddenZone(Shape, z.Position); // activation: spawn.AddSeconds(ActivationDelay)
    }
}

sealed class LiquidHell(BossModule module) : LiquidHellBase(module);

sealed class P1LiquidHell(BossModule module) : LiquidHellBase(module)
{
    public override bool KeepOnPhaseChange => true;

    public enum BaitMode
    {
        None,
        Proximity,
        Random
    }

    BaitMode Mode;
    DateTime NextCast;
    private readonly PartyRolesConfig _config = Service.Config.Get<PartyRolesConfig>();
    private readonly Hatch _hatch = module.FindComponent<Hatch>()!;
    private P1Fireball? _fireball;
    private readonly List<Actor> _neurolinks = module.Enemies((uint)OID.Neurolink);

    public Actor? Baiter;

    public void Reset(double delay, BaitMode mode)
    {
        Mode = mode;
        NextCast = WorldState.FutureTime(delay);
        NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == WatchedAction)
        {
            NextCast = WorldState.FutureTime(1.2d);

            if (Mode == BaitMode.Random && (Baiter == null || Baiter.IsDead))
            {
                Baiter = Raid.WithoutSlot().Closest(spell.TargetXZ);
            }
        }

        if (NumCasts >= 5)
        {
            NextCast = default;
            Mode = BaitMode.None;
            Baiter = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Mode == BaitMode.Proximity)
        {
            var assignments = _config.SlotsPerAssignment(Raid);

            if (assignments.Length == 0)
            {
                return;
            }

            bool isBaiter;

            var slotR1 = assignments[(int)PartyRolesConfig.Assignment.R1];
            if (_hatch.IsTarget(slotR1))
            {
                isBaiter = assignment == PartyRolesConfig.Assignment.H1;
            }
            else
            {
                isBaiter = assignment == PartyRolesConfig.Assignment.R1;
            }

            if (isBaiter)
            {
                hints.AddForbiddenZone(new SDCircle(Module.PrimaryActor.Position, 18f), NextCast);
                var center = Arena.Center;
                // encourage baiter to stay on the opposite half of the arena, because it tends to walk itself into a corner otherwise
                hints.AddForbiddenZone(new SDInvertedCone(Module.PrimaryActor.Position, 50f, Module.PrimaryActor.DirectionTo(center).ToAngle(), 45f.Degrees()), DateTime.MaxValue);

                // encourage baiter to stay on arena edge if possible
                hints.GoalZones.Add(p => p.InDonut(center, 18, 22) ? 0.1f : 0);

                // don't drop on neurolinks
                var countN = _neurolinks.Count;
                for (var i = 0; i < countN; ++i)
                {
                    hints.AddForbiddenZone(new SDCircle(_neurolinks[i].Position, 7f), NextCast);
                }
            }
            else
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.PrimaryActor.Position, 16f, 0.1f));
            }
        }

        if (Mode == BaitMode.Random)
        {
            if (NumCasts == 0 && Module.PrimaryActor.TargetID != actor.InstanceID && !_hatch.IsTarget(slot) && assignment is not (PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.MT))
            {
                hints.AddForbiddenZone(new SDCircle(Module.PrimaryActor.Position, 6f), NextCast);

                var raid = Raid.WithoutSlot(false, true, true);
                var lenR = raid.Length;
                for (var i = 0; i < lenR; ++i)
                {
                    var p = raid[i];
                    if (p == actor)
                    {
                        continue;
                    }
                    hints.AddForbiddenZone(new SDCircle(p.Position, 1f), DateTime.MaxValue);
                }
            }
            if (_fireball == null)
            {
                var comp = Module.FindComponent<P1Fireball>();
                if (comp != null)
                {
                    _fireball = comp;
                }
                else
                {
                    return;
                }
            }
            if (actor == Baiter && _fireball.Destination is var dest && dest != default)
            {
                hints.AddForbiddenZone(new SDInvertedCircle(dest, 11f), NextCast.AddSeconds(1.2d * (4 - NumCasts)));
                hints.AddForbiddenZone(new SDCircle(dest, 7f), NextCast);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => player == Baiter ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void Update()
    {
        base.Update();

        if (Mode == BaitMode.Proximity)
            Baiter = Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position);
    }
}
