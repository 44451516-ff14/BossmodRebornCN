namespace BossMod.Components;

// generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
// some players can be marked as 'forbidden' - if any of them is baiting, they are warned
// otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
public class GenericBaitAway(BossModule module, uint aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false, bool tankbuster = false, bool onlyShowOutlines = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.None) : CastCounter(module, aid)
{
    public struct Bait(Actor source, Actor target, AOEShape shape, DateTime activation = default, BitMask forbidden = default, Angle? customRotation = null, int maxCasts = 1, WDir offset = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    {
        public Angle? CustomRotation = customRotation;
        public AOEShape Shape = shape;
        public Actor Source = source;
        public Actor Target = target;
        public DateTime Activation = activation;
        public BitMask Forbidden = forbidden;
        public int MaxCasts = maxCasts;
        public WDir Offset = offset;
        public int? ArenaProjectionLayer = arenaProjectionLayer;
        public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

        // Null keeps the target-following default; explicit IDs and all-layer mode take precedence.
        public readonly int? ResolveArenaProjectionLayer(BossModule module)
            => module.ResolveTargetArenaProjectionLayer(Target, ArenaProjectionLayer, RestrictToArenaProjectionLayer);

        public readonly Angle Rotation => CustomRotation ?? (Source != Target ? Angle.FromDirection(Target.Position - Source.Position) : Source.Rotation);

        public Bait(WPos source, Actor target, AOEShape shape, DateTime activation = default, Angle? customRotation = null, BitMask forbidden = default, int maxCasts = 1, WDir offset = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
            : this(new(default, default, default, default, default!, default, default, default, default, source.ToVec4()), target, shape, activation, forbidden, customRotation, maxCasts, offset, arenaProjectionLayer, restrictToArenaProjectionLayer) { }
    }

    public readonly bool AlwaysDrawOtherBaits = alwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
    public readonly bool CenterAtTarget = centerAtTarget; // if true, aoe source is at target
    public bool OnlyShowOutlines = onlyShowOutlines; // if true only show outlines
    public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public bool IgnoreOtherBaits; // if true, don't show hints/aoes for baits by others
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public BitMask ForbiddenPlayers; // these players should avoid baiting
    public List<Bait> CurrentBaits = [];
    public AIHints.PredictedDamageType DamageType = damageType;
    public const string BaitAwayHint = "远离队伍诱导！";

    public List<Bait> ActiveBaits
    {
        get
        {
            var count = CurrentBaits.Count;
            if (count == 0)
            {
                return CurrentBaits;
            }
            List<Bait> activeBaits = [with(count)];
            var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref curBaits[i];
                if (!bait.Source.IsDead)
                {
                    if (AllowDeadTargets || !bait.Target.IsDead)
                    {
                        activeBaits.Add(bait);
                    }
                }
            }
            return activeBaits;
        }
    }

    public List<Bait> ActiveBaitsOn(Actor target)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return CurrentBaits;
        }
        List<Bait> activeBaitsOnTarget = [with(count)];
        var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref curBaits[i];
            if (!bait.Source.IsDead && bait.Target == target && BaitParticipantAppliesToArenaProjectionLayer(target, bait))
            {
                activeBaitsOnTarget.Add(bait);
            }
        }
        return activeBaitsOnTarget;
    }

    public bool IsBaitTarget(Actor target)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return false;
        }

        var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref curBaits[i];
            if (!bait.Source.IsDead && bait.Target == target && BaitParticipantAppliesToArenaProjectionLayer(target, bait))
            {
                return true;
            }
        }
        return false;
    }

    public List<Bait> ActiveBaitsNotOn(Actor target)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return CurrentBaits;
        }
        List<Bait> activeBaitsNotOnTarget = [with(count)];
        var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);

        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref curBaits[i];
            if (!bait.Source.IsDead && bait.Target != target && BaitAppliesToArenaProjectionLayer(target, bait))
            {
                activeBaitsNotOnTarget.Add(bait);
            }
        }
        return activeBaitsNotOnTarget;
    }

    public WPos BaitOrigin(ref Bait bait) => (CenterAtTarget ? bait.Target : bait.Source).Position + bait.Offset;
    public bool IsClippedBy(Actor actor, ref Bait bait) => BaitParticipantAppliesToArenaProjectionLayer(actor, bait) && bait.Shape.Check(actor.Position, BaitOrigin(ref bait), bait.Rotation);
    protected bool BaitAppliesToArenaProjectionLayer(Actor actor, in Bait bait)
        => ArenaProjectionLayerApplies(actor, bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer);
    protected bool BaitParticipantAppliesToArenaProjectionLayer(Actor actor, in Bait bait)
        => ArenaProjectionLayerParticipantApplies(actor, bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer);

    public List<Actor> PlayersClippedBy(ref Bait bait)
    {
        var actors = Raid.WithoutSlot();
        var len = actors.Length;
        List<Actor> result = [with(len)];
        for (var i = 0; i < len; ++i)
        {
            var actor = actors[i];
            if (actor != bait.Target
                && Module.ActorMatchesArenaProjectionLayer(actor, bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer)
                && bait.Shape.Check(actor.Position, BaitOrigin(ref bait), bait.Rotation))
            {
                result.Add(actor);
            }
        }

        return result;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
        {
            return;
        }

        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var count = baits.Length;
        if (count == 0)
        {
            return;
        }

        if (ForbiddenPlayers[slot])
        {
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref baits[i];
                if (bait.Target == actor && BaitAppliesToArenaProjectionLayer(actor, bait) && BaitParticipantAppliesToArenaProjectionLayer(actor, bait))
                {
                    hints.Add("Avoid baiting!");
                    break;
                }
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref baits[i];
                if (bait.Target != actor || !BaitAppliesToArenaProjectionLayer(actor, bait) || !BaitParticipantAppliesToArenaProjectionLayer(actor, bait))
                {
                    continue;
                }

                var clippedPlayers = PlayersClippedBy(ref bait);
                if (clippedPlayers.Count != 0)
                {
                    hints.Add(BaitAwayHint);
                    break;
                }
            }
        }

        if (!IgnoreOtherBaits)
        {
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref baits[i];
                if (bait.Target == actor || !BaitParticipantAppliesToArenaProjectionLayer(actor, bait))
                {
                    continue;
                }

                if (IsClippedBy(actor, ref bait))
                {
                    hints.Add("离开诱导 AOE！");
                    break;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var count = baits.Length;
        if (count == 0)
        {
            return;
        }
        BitMask predictedDamage = default;
        var firstActivation = DateTime.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            ref var bait = ref baits[i];
            if (!BaitAppliesToArenaProjectionLayer(actor, bait))
            {
                continue;
            }
            if (bait.Target != actor || !BaitParticipantAppliesToArenaProjectionLayer(actor, bait))
            {
                hints.AddForbiddenZone(bait.Shape, BaitOrigin(ref bait), bait.Rotation, bait.Activation,
                    arenaProjectionLayer: ArenaProjectionLayerForAI(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer));
            }
            else
            {
                AddTargetSpecificHints(actor, ref bait, hints);
            }
            if (DamageType != AIHints.PredictedDamageType.None
                && Module.ActorMatchesArenaProjectionLayer(bait.Target, bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer))
            {
                predictedDamage.Set(Raid.FindSlot(bait.Target.InstanceID));
                firstActivation = firstActivation < bait.Activation ? firstActivation : bait.Activation;
            }
        }
        if (predictedDamage != default)
        {
            hints.AddPredictedDamage(predictedDamage, firstActivation, DamageType);
        }
    }

    private void AddTargetSpecificHints(Actor actor, ref Bait bait, AIHints hints)
    {
        if (bait.Source == bait.Target) // TODO: think about how to handle source == target baits, eg. vomitting mechanics
        {
            return;
        }
        var raid = Raid.WithoutSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = raid[i];
            if (a == actor || !Module.ActorMatchesArenaProjectionLayer(a, bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer))
            {
                continue;
            }
            switch (bait.Shape)
            {
                case AOEShapeDonut:
                case AOEShapeCircle:
                    hints.AddForbiddenZone(bait.Shape, a.Position - bait.Offset, default, bait.Activation,
                        arenaProjectionLayer: ArenaProjectionLayerForAI(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer));
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(new SDCone(bait.Source.Position, 100f, bait.Source.AngleTo(a), cone.HalfAngle), bait.Activation,
                        arenaProjectionLayer: ArenaProjectionLayerForAI(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer));
                    break;
                case AOEShapeRect rect:
                    hints.AddForbiddenZone(new SDCone(bait.Source.Position, 100f, bait.Source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - bait.Source.Position).Length())), bait.Activation,
                        arenaProjectionLayer: ArenaProjectionLayerForAI(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer));
                    break;
                case AOEShapeCross cross:
                    hints.AddForbiddenZone(cross, a.Position - bait.Offset, bait.Rotation, bait.Activation,
                        arenaProjectionLayer: ArenaProjectionLayerForAI(bait.ResolveArenaProjectionLayer(Module), bait.RestrictToArenaProjectionLayer));
                    break;
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var bait = ref baits[i];
            if (!bait.Source.IsDead && bait.Target == player && BaitAppliesToArenaProjectionLayer(pc, bait)
                && BaitParticipantAppliesToArenaProjectionLayer(player, bait))
            {
                return BaiterPriority;
            }
        }
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
        {
            return;
        }

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (!b.Source.IsDead && b.Target != pc && (AlwaysDrawOtherBaits || IsClippedBy(pc, ref b)))
            {
                using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module), b.RestrictToArenaProjectionLayer))
                    b.Shape.Draw(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            if (!b.Source.IsDead && (OnlyShowOutlines || !OnlyShowOutlines && b.Target == pc))
            {
                using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module), b.RestrictToArenaProjectionLayer))
                    b.Shape.Outline(Arena, BaitOrigin(ref b), b.Rotation);
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (tankbuster && CurrentBaits.Count != 0)
        {
            hints.Add("Tankbuster cleave");
        }
    }

    internal static int IndexOfClosestLayer(ReadOnlySpan<float> values, float y)
    {
        var bestIndex = 0;
        var bestDiff = Math.Abs(values[0] - y);
        var len = values.Length;
        for (var i = 1; i < len; i++)
        {
            var diff = Math.Abs(values[i] - y);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}

// bait on all players, requiring everyone to spread out
public class BaitAwayEveryone : GenericBaitAway
{
    public BaitAwayEveryone(BossModule module, Actor? source, AOEShape shape, uint aid = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : base(module, aid, damageType: AIHints.PredictedDamageType.Raidwide)
    {
        AllowDeadTargets = false;
        if (source != null)
        {
            var party = Raid.WithoutSlot(true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                if (ArenaProjectionLayerParticipantApplies(party[i], arenaProjectionLayer, restrictToArenaProjectionLayer))
                {
                    CurrentBaits.Add(new(source, party[i], shape, arenaProjectionLayer: arenaProjectionLayer, restrictToArenaProjectionLayer: restrictToArenaProjectionLayer));
                }
            }
        }
    }
}

// component for mechanics requiring tether targets to bait their aoe away from raid

public class BaitAwayTethers(BossModule module, AOEShape shape, uint tetherID, uint aid = default, uint enemyOID = default, double activationDelay = default, bool centerAtTarget = false, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public BaitAwayTethers(BossModule module, float radius, uint tetherID, uint aid = default, uint enemyOID = default, double activationDelay = default, bool centerAtTarget = true, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : this(module, new AOEShapeCircle(radius), tetherID, aid, enemyOID, activationDelay, centerAtTarget, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

    public AOEShape Shape = shape;
    public uint TID = tetherID;
    public readonly uint EnemyOID = enemyOID;
    public bool DrawTethers = true;
    public double ActivationDelay = activationDelay;
    protected DateTime activation;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (DrawTethers)
        {
            var baits = ActiveBaits;
            var count = baits.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = baits[i];
                using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module), b.RestrictToArenaProjectionLayer))
                {
                    Arena.AddLine(b.Source.Position, b.Target.Position);
                }
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null && (EnemyOID == default || enemy.OID == EnemyOID))
        {
            if (activation == default)
            {
                activation = WorldState.FutureTime(ActivationDelay); // TODO: not optimal if tethers do not appear at the same time...
            }
            CurrentBaits.Add(new(enemy, player, Shape, activation, arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction && CurrentBaits.Count == 0)
        {
            activation = default;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) // TODO: this is problematic because untethering can happen many frames before the actual cast event, maybe there is a better solution?
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            var count = CurrentBaits.Count;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source == enemy && b.Target == player)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    // we support both player->enemy and enemy->player tethers
    public (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
        {
            return (null, null);
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return (null, null);
        }

        var (player, enemy) = source.Type is ActorType.Player or ActorType.Buddy ? (source, target) : (target, source);
        if (player.Type is not ActorType.Player and not ActorType.Buddy || enemy.Type is ActorType.Player or ActorType.Buddy)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return (null, null);
        }

        return (player, enemy);
    }
}

// component for mechanics requiring icon targets to bait their aoe away from raid

public class BaitAwayIcon(BossModule module, AOEShape shape, uint iconID, uint aid = default, double activationDelay = 5.1d, bool centerAtTarget = false, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
    : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType)
{
    public BaitAwayIcon(BossModule module, float radius, uint iconID, uint aid = default, double activationDelay = 5.1d, bool centerAtTarget = true, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : this(module, new AOEShapeCircle(radius), iconID, aid, activationDelay, centerAtTarget, source, tankbuster, damageType, restrictToArenaProjectionLayer, arenaProjectionLayer) { }

    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public AOEShape Shape = shape;
    public uint IID = iconID;
    public double ActivationDelay = activationDelay;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public virtual Actor? BaitSource(Actor target) => source ?? Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
        {
            CurrentBaits.Add(new(source, WorldState.Actors.Find(targetID) ?? actor, Shape, WorldState.FutureTime(ActivationDelay), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (CurrentBaits.Count != 0 && spell.Action.ID == WatchedAction)
        {
            CurrentBaits.RemoveAt(0);
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            ref var b = ref CurrentBaits.Ref(i);
            if (b.Target.IsDead)
            {
                CurrentBaits.RemoveAt(i);
            }
        }
    }
}

public class BaitAwayIconMulti(BossModule module, AOEShape shape, uint iconID, uint[] aids, double activationDelay = 5.1d, bool centerAtTarget = false, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
    : BaitAwayIcon(module, shape, iconID, activationDelay: activationDelay, source: source, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType, restrictToArenaProjectionLayer: restrictToArenaProjectionLayer, arenaProjectionLayer: arenaProjectionLayer)
{
    public BaitAwayIconMulti(BossModule module, float radius, uint iconID, uint[] aids, double activationDelay = 5.1d, bool centerAtTarget = true, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
        : this(module, new AOEShapeCircle(radius), iconID, aids, activationDelay, centerAtTarget, source, tankbuster, damageType, restrictToArenaProjectionLayer, arenaProjectionLayer) { }

    protected readonly uint[] AIDs = aids;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                if (CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                }
                ++NumCasts;
                return;
            }
        }
    }
}

// component for mechanics requiring cast targets to gtfo from raid (aoe tankbusters etc)

public class BaitAwayCast(BossModule module, uint aid, AOEShape shape, bool centerAtTarget = false, bool endsOnCastEvent = false, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public BaitAwayCast(BossModule module, uint aid, float radius, bool centerAtTarget = true, bool endsOnCastEvent = false, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : this(module, aid, new AOEShapeCircle(radius), centerAtTarget, endsOnCastEvent, tankbuster, damageType, arenaProjectionLayer, restrictToArenaProjectionLayer) { }

    public AOEShape Shape = shape;
    public bool EndsOnCastEvent = endsOnCastEvent;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && !EndsOnCastEvent)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction && EndsOnCastEvent)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        if (EndsOnCastEvent)
        {
            var count = CurrentBaits.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                ref var b = ref CurrentBaits.Ref(i);
                if (b.Source.IsDeadOrDestroyed || b.Target.IsDead)
                {
                    CurrentBaits.RemoveAt(i);
                }
            }
        }
    }
}

// a variation of BaitAwayCast for charges that end at target

public class BaitAwayChargeCast(BossModule module, uint aid, float halfWidth, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true) : GenericBaitAway(module, aid, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    private readonly float HalfWidth = halfWidth;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction && WorldState.Actors.Find(spell.TargetID) is Actor target)
        {
            CurrentBaits.Add(new(caster, target, new AOEShapeRect((target.Position - caster.Position).Length(), HalfWidth), Module.CastFinishAt(spell), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = CurrentBaits.Count;
            var id = caster.InstanceID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Source.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, HalfWidth);
            }
        }
    }
}

// a variation of baits with tethers for charges that end at target

public class BaitAwayChargeTether(BossModule module, float halfWidth, double activationDelay, uint aidGood, uint aidBad = default, uint tetherIDBad = 57u, uint tetherIDGood = 1u, uint enemyOID = default, float minimumDistance = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
: StretchTetherDuo(module, minimumDistance, activationDelay, tetherIDBad, tetherIDGood, new AOEShapeRect(default, halfWidth), default, enemyOID, arenaProjectionLayer: arenaProjectionLayer, restrictToArenaProjectionLayer: restrictToArenaProjectionLayer)
{
    public readonly uint AidGood = aidGood;
    public readonly uint AidBad = aidBad; // supports 2nd AID incase the AID changes between good and bad tethers
    public readonly float HalfWidth = halfWidth;

    public override void Update()
    {
        base.Update();
        var count = CurrentBaits.Count;
        if (count == 0)
        {
            return;
        }

        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, HalfWidth);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == AidGood || spell.Action.ID == AidBad)
        {
            ++NumCasts;
            var count = CurrentBaits.Count;
            var id = spell.MainTargetID;
            var baits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var b = ref baits[i];
                if (b.Target.InstanceID == id)
                {
                    CurrentBaits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaits.Count == 0)
        {
            return;
        }

        base.AddHints(slot, actor, hints);
        var count = CurrentBaits.Count;

        for (var i = 0; i < count; ++i)
        {
            var b = CurrentBaits[i];
            if (b.Target != actor || !BaitParticipantAppliesToArenaProjectionLayer(actor, b))
            {
                continue;
            }
            if (PlayersClippedBy(ref b).Count != 0)
            {
                hints.Add(BaitAwayHint);
                return;
            }
        }
    }
}

public class GenericBaitProximity(BossModule module, bool alwaysDrawOtherBaits = true, bool onlyShowOutlines = false) : CastCounter(module, default)
{
    public struct Bait(WPos source, AOEShape shape, DateTime activation = default, int numTargets = 1, bool nearest = true, bool stack = false, int minStack = 1, int maxStack = 1, bool centerAtTarget = false, bool tankbuster = false, uint caster = default, Role role = Role.None, BitMask forbidden = default, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.None, Angle? customRotation = null, WDir offset = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
    {
        public WPos Position = source;
        public AOEShape Shape = shape;
        public DateTime Activation = activation;
        public bool FromNearest = nearest;
        public int NumTargets = numTargets;
        public bool IsStack = stack;
        public int MinStack = minStack;
        public int MaxStack = maxStack;
        public Role SpecifiedRole = role;
        public BitMask ForbiddenPlayers = forbidden; // these players should avoid baiting
        public bool CenterAtTarget = centerAtTarget; // if true, aoe source is at target
        public WDir Offset = offset;
        public bool IsTankbuster = tankbuster;
        public uint CasterID = caster;
        public AIHints.PredictedDamageType DamageType = damageType;
        public Angle? CustomRotation = customRotation;
        public int? ArenaProjectionLayer = arenaProjectionLayer;
        public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

        public readonly int? ResolveArenaProjectionLayer(BossModule module, Actor target)
            => module.ResolveTargetArenaProjectionLayer(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer);

        public Bait(Actor source, AOEShape shape, DateTime activation = default, int numTargets = 1, bool nearest = true, bool stack = false, int minStack = 1, int maxStack = 1, bool centerAtTarget = false, bool tankbuster = false, uint caster = default, Role role = Role.None, BitMask forbidden = default, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.None, Angle? customRotation = null, WDir offset = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = true)
            : this(source.Position, shape, activation, numTargets, nearest, stack, minStack, maxStack, centerAtTarget, tankbuster, caster, role, forbidden, damageType, customRotation, offset, arenaProjectionLayer, restrictToArenaProjectionLayer) { }
    }

    public readonly bool AlwaysDrawOtherBaits = alwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
    public bool OnlyShowOutlines = onlyShowOutlines; // if true only show outlines
    public bool AllowDeadTargets = false; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public bool IgnoreOtherBaits; // if true, don't show hints/aoes for baits by others
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public List<Bait> CurrentBaits = [];
    public const string BaitAwayHint = "远离队伍诱导！";
    public const string BaitAOEHint = "GTFO from baited AOE!";

    public List<Bait> ActiveBaits
    {
        get
        {
            var count = CurrentBaits.Count;
            if (count == 0)
            {
                return CurrentBaits;
            }
            List<Bait> activeBaits = [with(count)];
            var curBaits = CollectionsMarshal.AsSpan(CurrentBaits);
            for (var i = 0; i < count; ++i)
            {
                ref var bait = ref curBaits[i];
                var casterId = bait.CasterID;
                if (casterId != default)
                {
                    var caster = Module.Enemies(casterId);
                    if (caster.Count == 0)
                    {
                        continue;
                    }

                    if (caster[0].IsDeadOrDestroyed)
                    {
                        continue;
                    }
                }

                var targets = GetTargets(bait);
                var len = targets.Length;

                for (var j = 0; j < len; j++)
                {
                    if (AllowDeadTargets || !targets[j].IsDead)
                    {
                        activeBaits.Add(bait);
                        break;
                    }
                }
            }
            return activeBaits;
        }
    }

    public WPos BaitOrigin(ref Bait bait, Actor target) => (bait.CenterAtTarget ? target.Position : bait.Position) + bait.Offset;
    public Angle BaitRotation(ref Bait bait, Actor target) => Angle.FromDirection(target.Position - bait.Position);
    public bool IsClippedBy(Actor pc, ref Bait bait, Actor target) => ArenaProjectionLayerParticipantApplies(pc, bait.ResolveArenaProjectionLayer(Module, target), bait.RestrictToArenaProjectionLayer)
        && bait.Shape.Check(pc.Position, BaitOrigin(ref bait, target), BaitRotation(ref bait, target));
    protected bool BaitAppliesToArenaProjectionLayer(Actor actor, in Bait bait)
        => ArenaProjectionLayerApplies(actor, bait.ArenaProjectionLayer, bait.RestrictToArenaProjectionLayer);
    protected bool BaitParticipantAppliesToArenaProjectionLayer(Actor actor, in Bait bait)
        => ArenaProjectionLayerParticipantApplies(actor, bait.ArenaProjectionLayer, bait.RestrictToArenaProjectionLayer);

    public List<Actor> PlayersClippedBy(ref Bait bait, Actor target)
    {
        var actors = Raid.WithoutSlot();
        var len = actors.Length;
        List<Actor> result = [with(len)];
        for (var i = 0; i < len; ++i)
        {
            var actor = actors[i];
            if (actor != target
                && Module.ActorMatchesArenaProjectionLayer(actor, bait.ResolveArenaProjectionLayer(Module, target), bait.RestrictToArenaProjectionLayer)
                && bait.Shape.Check(actor.Position, BaitOrigin(ref bait, target), BaitRotation(ref bait, target)))
            {
                result.Add(actor);
            }
        }

        return result;
    }
    public bool IsBaitTarget(ref Bait bait, Actor target)
    {
        var targets = GetTargets(bait);
        var length = targets.Length;

        for (var j = 0; j < length; j++)
        {
            if (targets[j] == target)
            {
                return true;
            }
        }

        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        if (len == 0)
        {
            return;
        }

        for (var i = 0; i < len; i++)
        {
            ref var b = ref baits[i];
            if (!BaitParticipantAppliesToArenaProjectionLayer(actor, b))
            {
                continue;
            }
            var baiter = IsBaitTarget(ref b, actor) ? actor : default;
            if (baiter == default)
            {
                continue;
            }

            var clippedPlayers = PlayersClippedBy(ref b, baiter);
            var clippedCount = clippedPlayers.Count;
            //hints.Add($"Clipped ({clippedCount})", false);

            if (b.IsStack)
            {
                // increment to include player in stack count
                clippedCount++;
                if (clippedCount < b.MinStack)
                {
                    hints.Add("Not enough in stack!");
                    break;
                }
                else if (clippedCount > b.MaxStack)
                {
                    hints.Add("Too many in stack!");
                    break;
                }
            }
            else
            {
                if (clippedPlayers.Count != 0)
                {
                    hints.Add(BaitAwayHint);
                    break;
                }
            }
        }
        if (!IgnoreOtherBaits)
        {
            for (var i = 0; i < len; i++)
            {
                ref var b = ref baits[i];
                if (!BaitParticipantAppliesToArenaProjectionLayer(actor, b))
                {
                    continue;
                }
                var targets = GetTargets(b);
                var tarLen = targets.Length;

                // show all baits, or all baits aside from yourself
                var subTargets = new Actor[tarLen - (IsBaitTarget(ref b, actor) ? 1 : 0)];
                var subCount = 0;
                for (var j = 0; j < tarLen; j++)
                {
                    if (targets[j] != actor)
                    {
                        subTargets[subCount++] = targets[j];
                    }
                }

                for (var j = 0; j < subCount; j++)
                {
                    var target = subTargets[j];
                    if (IsClippedBy(actor, ref b, target))
                    {
                        if (b.IsStack)
                        {
                            var clippedPlayers = PlayersClippedBy(ref b, target);
                            if (clippedPlayers.Count + 1 > b.MaxStack)
                            {
                                hints.Add(BaitAOEHint);
                                return;
                            }
                        }
                        else
                        {
                            hints.Add(BaitAOEHint);
                            return;
                        }
                    }
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (OnlyShowOutlines || IgnoreOtherBaits)
        {
            return;
        }

        BitMask targetted = default;
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var targets = GetTargets(b);
            var tarLen = targets.Length;

            for (var j = 0; j < tarLen; j++)
            {
                var target = targets[j];
                var slot = Raid.FindSlot(target.InstanceID);
                targetted.Set(slot);

                // always draw stacks even if player isn't clipped by it
                if (target != pc && (AlwaysDrawOtherBaits || b.IsStack || IsClippedBy(pc, ref b, target)))
                {
                    using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer))
                        b.Shape.Draw(Arena, BaitOrigin(ref b, target), BaitRotation(ref b, target),
                            b.IsStack && ArenaProjectionLayerParticipantApplies(pc, b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer) ? Colors.Safe : Colors.AOE);
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var b = ref baits[i];
            var targets = GetTargets(b);
            var tarLen = targets.Length;

            for (var j = 0; j < tarLen; j++)
            {
                var target = targets[j];
                if (OnlyShowOutlines || !OnlyShowOutlines && target == pc)
                {
                    using (Arena.WorldProjectionLayer(b.ResolveArenaProjectionLayer(Module, target), b.RestrictToArenaProjectionLayer))
                        b.Shape.Outline(Arena, BaitOrigin(ref b, target), BaitRotation(ref b, target));
                }
            }
        }
    }

    private bool IsActive => CurrentBaits.Count > 0;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        // one bait can have multiple targets
        // just show everyone if there are active baits
        // maybe write so it only shows players that are baiting or getting clipped by bait?
        if (!IsActive)
        {
            return PlayerPriority.Irrelevant;
        }

        var baits = CollectionsMarshal.AsSpan(ActiveBaits);
        var len = baits.Length;

        var haveApplicableBait = false;
        for (var i = 0; i < len; i++)
        {
            ref var bait = ref baits[i];
            foreach (var target in GetTargets(bait))
            {
                if (!ArenaProjectionLayerApplies(pc, bait.ResolveArenaProjectionLayer(Module, target), bait.RestrictToArenaProjectionLayer))
                {
                    continue;
                }
                haveApplicableBait = true;
                if (target == player)
                {
                    return PlayerPriority.Danger;
                }
            }
        }

        return haveApplicableBait ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public ReadOnlySpan<Actor> GetTargets(Bait bait)
    {
        var party = Raid.WithSlot(AllowDeadTargets, true, true);
        if (bait.ForbiddenPlayers.Any())
        {
            party = [.. party.ExcludedFromMask(bait.ForbiddenPlayers)];
        }

        var partyLen = party.Length;

        var partyRoles = new Actor[partyLen];
        var roleLen = 0;
        for (var i = 0; i < partyLen; i++)
        {
            var actor = party[i].Item2;
            if ((bait.SpecifiedRole == Role.None || actor.Role == bait.SpecifiedRole)
                && Module.ActorMatchesArenaProjectionLayer(actor, bait.ArenaProjectionLayer, bait.RestrictToArenaProjectionLayer))
            {
                partyRoles[roleLen++] = actor;
            }
        }

        (Actor actor, float distSq)[] distances = new (Actor, float)[roleLen];
        var result = new Actor[roleLen];

        for (var i = 0; i < roleLen; ++i)
        {
            var p = partyRoles[i];
            var distSq = (p.Position - bait.Position).LengthSq();
            distances[i] = (p, distSq);
        }

        var isNearest = bait.FromNearest;

        var targets = Math.Min(bait.NumTargets, roleLen);
        for (var i = 0; i < targets; ++i)
        {
            var selIdx = i;
            for (var j = i + 1; j < roleLen; ++j)
            {
                if (isNearest && distances[j].distSq < distances[selIdx].distSq || !isNearest && distances[j].distSq > distances[selIdx].distSq)
                {
                    selIdx = j;
                }
            }

            if (selIdx != i)
            {
                (distances[selIdx], distances[i]) = (distances[i], distances[selIdx]);
            }

            result[i] = distances[i].actor;
        }

        return result.AsSpan()[..targets];
    }
}
