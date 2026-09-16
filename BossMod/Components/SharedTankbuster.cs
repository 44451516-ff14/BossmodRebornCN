namespace BossMod.Components;

// generic 'shared tankbuster' component; assumes only 1 concurrent cast is active
// TODO: revise and improve (track invuln, ai hints, num stacked tanks?)
public class GenericSharedTankbuster(BossModule module, uint aid, AOEShape shape, bool originAtTarget = false,
    int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : CastCounter(module, aid)
{
    public readonly AOEShape Shape = shape;
    public readonly bool OriginAtTarget = originAtTarget;
    protected Actor? Source;
    protected Actor? Target;
    protected DateTime Activation;
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public bool Active => Source != null;
    protected int? ResolvedArenaProjectionLayer => Module.ResolveTargetArenaProjectionLayer(Target, ArenaProjectionLayer, RestrictToArenaProjectionLayer);

    // circle shapes typically have origin at target
    public GenericSharedTankbuster(BossModule module, uint aid, float radius, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
        : this(module, aid, new AOEShapeCircle(radius), true, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null || !ArenaProjectionLayerParticipantApplies(actor, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        if (Target == actor)
        {
            var otherTanksInAOE = false;
            var party = Raid.WithoutSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var a = ref party[i];
                if (a != actor && a.Role == Role.Tank
                    && ArenaProjectionLayerParticipantApplies(a, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer) && InAOE(a))
                {
                    otherTanksInAOE = true;
                    break;
                }
            }
            hints.Add("Stack with other tanks or press invuln!", !otherTanksInAOE);
        }
        else if (actor.Role == Role.Tank)
        {
            hints.Add("Stack with tank!", !InAOE(actor));
        }
        else
        {
            hints.Add("离开坦克！", InAOE(actor));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ArenaProjectionLayerApplies(actor, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
            return;

        if (Source != null && Target != null && Target != actor)
        {
            if (actor.Role == Role.Tank && ArenaProjectionLayerParticipantApplies(actor, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                hints.AddForbiddenZone(OriginAtTarget ? Shape.InvertedDistance(Target.Position, Target.Rotation) : Shape.InvertedDistance(Source.Position, Angle.FromDirection(Target.Position - Source.Position)), Activation,
                    arenaProjectionLayer: ArenaProjectionLayerForAI(ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
            else
            {
                hints.AddForbiddenZone(OriginAtTarget ? Shape.Distance(Target.Position, Target.Rotation) : Shape.Distance(Source.Position, Angle.FromDirection(Target.Position - Source.Position)), Activation,
                    arenaProjectionLayer: ArenaProjectionLayerForAI(ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
        }
        else if (Source != null && Target != null && Target == actor && Shape is AOEShapeCircle circle)
        {
            var radius = circle.Radius;
            var party = Raid.WithoutSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.Role != Role.Tank && p != Target && ArenaProjectionLayerParticipantApplies(p, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    hints.AddForbiddenZone(new SDCircle(p.Position, radius), Activation, arenaProjectionLayer: ArenaProjectionLayerForAI(ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer));
                }
            }
        }
        if (Source != null)
        {
            BitMask predictedDamage = default;
            var party = Raid.WithSlot();
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var p = ref party[i];
                if (p.Item2.Role == Role.Tank && ArenaProjectionLayerParticipantApplies(p.Item2, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    predictedDamage.Set(p.Item1);
                }
            }
            if (predictedDamage != default)
            {
                hints.AddPredictedDamage(predictedDamage, Activation, AIHints.PredictedDamageType.Tankbuster);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => Target == player && ArenaProjectionLayerApplies(pc, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer)
            && ArenaProjectionLayerParticipantApplies(player, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Source != null && Target != null && pc.Role == Role.Tank
            && ArenaProjectionLayerParticipantApplies(pc, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            using (Arena.WorldProjectionLayer(ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                if (OriginAtTarget)
                {
                    Shape.Outline(Arena, Target, Target == pc ? default : Colors.Safe);
                }
                else
                {
                    Shape.Outline(Arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position), Target == pc ? default : Colors.Safe);
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source != null && Target != null && (pc.Role != Role.Tank || !ArenaProjectionLayerParticipantApplies(pc, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer)))
        {
            using (Arena.WorldProjectionLayer(ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                if (OriginAtTarget)
                {
                    Shape.Draw(Arena, Target);
                }
                else
                {
                    Shape.Draw(Arena, Source.Position, Angle.FromDirection(Target.Position - Source.Position));
                }
            }
        }
    }

    private bool InAOE(Actor actor) => Source != null && Target != null
        && ArenaProjectionLayerParticipantApplies(actor, ResolvedArenaProjectionLayer, RestrictToArenaProjectionLayer)
        && (OriginAtTarget ? Shape.Check(actor.Position, Target) : Shape.Check(actor.Position, Source.Position, Angle.FromDirection(Target.Position - Source.Position)));
}

// shared tankbuster at cast target
public class CastSharedTankbuster(BossModule module, uint aid, AOEShape shape, bool originAtTarget = false,
    int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericSharedTankbuster(module, aid, shape, originAtTarget, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public CastSharedTankbuster(BossModule module, uint aid, float radius, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
        : this(module, aid, new AOEShapeCircle(radius), true, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (caster == Source)
        {
            Source = Target = null;
        }
    }
}

// shared tankbuster at icon
public class IconSharedTankbuster(BossModule module, uint iconId, uint aid, AOEShape shape, double activationDelay = 5.1d, bool originAtTarget = false,
    int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericSharedTankbuster(module, aid, shape, originAtTarget, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public IconSharedTankbuster(BossModule module, uint iconId, uint aid, float radius, double activationDelay = 5.1d,
        int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
        : this(module, iconId, aid, new AOEShapeCircle(radius), activationDelay, true, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

    public virtual Actor? BaitSource(Actor target) => Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == iconId)
        {
            Source = BaitSource(actor);
            Target = actor;
            Activation = WorldState.FutureTime(activationDelay);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            Source = Target = null;
        }
    }
}
