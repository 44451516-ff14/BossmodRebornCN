namespace BossMod.Components;

// generic 'stack/spread' mechanic has some players that have to spread away from raid, some other players that other players need to stack with
// there are various variants (e.g. everyone should spread, or everyone should stack in one or more groups, or some combination of that)
public abstract class GenericStackSpread(BossModule module, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false) : BossComponent(module)
{
    public struct Stack(Actor target, float radius, int minSize = 2, int maxSize = int.MaxValue, DateTime activation = default, BitMask forbiddenPlayers = default)
    {
        public Actor Target = target;
        public float Radius = radius;
        public int MinSize = minSize;
        public int MaxSize = maxSize;
        public DateTime Activation = activation;
        public BitMask ForbiddenPlayers = forbiddenPlayers; // raid members that aren't allowed to participate in the stack

        public readonly int NumInside(BossModule module) => module.Raid.WithSlot().ExcludedFromMask(ForbiddenPlayers).InRadius(Target.Position, Radius).Count();
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSize && count <= MaxSize;
        public readonly bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSize;
        public readonly bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSize;
        public readonly bool IsInside(WPos pos) => pos.InCircle(Target.Position, Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);
    }

    public record struct Spread(
        Actor Target,
        float Radius,
        DateTime Activation = default
    );

    public bool AlwaysShowSpreads = alwaysShowSpreads; // if false, we only shown own spread radius for spread targets - this reduces visual clutter
    public bool RaidwideOnResolve = raidwideOnResolve; // if true, assume even if mechanic is correctly resolved everyone will still take damage
    public bool IncludeDeadTargets = includeDeadTargets; // if false, stacks & spreads with dead targets are ignored
    public List<Stack> Stacks = [];
    public List<Spread> Spreads = [];
    public const string StackHint = "Stack!";

    public bool Active => Stacks.Count + Spreads.Count > 0;
    public IEnumerable<Stack> ActiveStacks => IncludeDeadTargets ? Stacks : Stacks.Where(s => !s.Target.IsDead);
    public IEnumerable<Spread> ActiveSpreads => IncludeDeadTargets ? Spreads : Spreads.Where(s => !s.Target.IsDead);

    public bool IsStackTarget(Actor actor) => Stacks.Any(s => s.Target == actor);
    public bool IsSpreadTarget(Actor actor) => Spreads.Any(s => s.Target == actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.FindIndex(s => s.Target == actor) is var iSpread && iSpread >= 0)
        {
            hints.Add("Spread!", Raid.WithoutSlot().InRadiusExcluding(actor, Spreads[iSpread].Radius).Any());
        }
        else if (Stacks.FindIndex(s => s.Target == actor) is var iStack && iStack >= 0)
        {
            var stack = Stacks[iStack];
            var numStacked = 1; // always stacked with self
            var stackedWithOtherStackOrAvoid = false;
            foreach (var (j, other) in Raid.WithSlot().InRadiusExcluding(actor, stack.Radius))
            {
                ++numStacked;
                stackedWithOtherStackOrAvoid |= stack.ForbiddenPlayers[j] || IsStackTarget(other);
            }
            hints.Add(StackHint, stackedWithOtherStackOrAvoid || numStacked < stack.MinSize || numStacked > stack.MaxSize);
        }
        else
        {
            var numParticipatingStacks = 0;
            var numUnsatisfiedStacks = 0;
            foreach (var s in ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]))
            {
                if (actor.Position.InCircle(s.Target.Position, s.Radius))
                    ++numParticipatingStacks;
                else if (Raid.WithoutSlot().InRadiusExcluding(s.Target, s.Radius).Count() + 1 < s.MinSize)
                    ++numUnsatisfiedStacks;
            }

            if (numParticipatingStacks > 1)
                hints.Add(StackHint);
            else if (numParticipatingStacks == 1)
                hints.Add(StackHint, false);
            else if (numUnsatisfiedStacks > 0)
                hints.Add(StackHint);
            // else: don't show anything, all potential stacks are already satisfied without a player
            //hints.Add("Stack!", ActiveStacks.Count(s => !s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)) != 1);
        }

        if (ActiveSpreads.Any(s => s.Target != actor && actor.Position.InCircle(s.Target.Position, s.Radius)))
        {
            hints.Add("GTFO from spreads!");
        }
        else if (ActiveStacks.Any(s => s.Target != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)))
        {
            hints.Add("GTFO from forbidden stacks!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // forbid standing next to spread markers
        // TODO: think how to improve this, current implementation works, but isn't particularly good - e.g. nearby players tend to move to same spot, turn around, etc.
        // ideally we should provide per-mechanic spread spots, but for simple cases we should try to let melee spread close and healers/rdd spread far from main target...
        foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(spreadFrom.Target.Position, spreadFrom.Radius + 1), spreadFrom.Activation);
        foreach (var spreadFrom in ActiveSpreads.Where(s => s.Target == actor))
            foreach (var x in Raid.WithoutSlot())
                if (!ActiveSpreads.Any(s => s.Target == x))
                    hints.AddForbiddenZone(ShapeDistance.Circle(x.Position, spreadFrom.Radius + 1), spreadFrom.Activation);
        foreach (var avoid in ActiveStacks.Where(s => s.Target != actor && (s.ForbiddenPlayers[slot] || !s.IsInside(actor) && (s.CorrectAmountInside(Module) || s.TooManyInside(Module)) || s.IsInside(actor) && s.TooManyInside(Module))))
            hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Target.Position, avoid.Radius), avoid.Activation);

        if (Stacks.FirstOrDefault(s => s.Target == actor) is var actorStack && actorStack.Target != null)
        {
            // forbid standing next to other stack markers or overlapping them
            foreach (var stackWith in ActiveStacks.Where(s => s.Target != actor))
                hints.AddForbiddenZone(ShapeDistance.Circle(stackWith.Target.Position, stackWith.Radius * 2), stackWith.Activation);
            // if player got stackmarker and is playing with NPCs, go to a NPC to stack with them since they will likely not come to you
            if (Raid.WithoutSlot().Any(x => x.Type == ActorType.Buddy))
            {
                var forbidden = new List<Func<WPos, float>>();
                foreach (var stackWith in ActiveStacks.Where(s => s.Target == actor))
                    forbidden.Add(ShapeDistance.InvertedCircle(Raid.WithoutSlot().FirstOrDefault(x => !x.IsDead && !IsSpreadTarget(x) && !IsStackTarget(x))!.Position, actorStack.Radius / 3));
                if (forbidden.Count > 0)
                    hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), actorStack.Activation);
            }
        }
        else if (!IsSpreadTarget(actor) && !IsStackTarget(actor))
        {
            var forbidden = new List<Func<WPos, float>>();
            foreach (var s in ActiveStacks.Where(x => !x.ForbiddenPlayers[slot] && (x.IsInside(actor) && !x.TooManyInside(Module)
            || !x.IsInside(actor) && x.InsufficientAmountInside(Module))))
                forbidden.Add(ShapeDistance.InvertedCircle(s.Target.Position, s.Radius - 0.25f));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), ActiveStacks.FirstOrDefault().Activation);
        }

        if (RaidwideOnResolve)
        {
            var firstActivation = DateTime.MaxValue;
            BitMask damageMask = new();
            foreach (var s in ActiveSpreads)
            {
                damageMask.Set(Raid.FindSlot(s.Target.InstanceID));
                firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
            }
            foreach (var s in ActiveStacks)
            {
                damageMask |= Raid.WithSlot().Mask() & ~s.ForbiddenPlayers; // assume everyone will take damage except forbidden players (so-so assumption really...)
                firstActivation = firstActivation < s.Activation ? firstActivation : s.Activation;
            }

            if (damageMask.Any())
                hints.PredictedDamage.Add((damageMask, firstActivation));
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var shouldSpread = IsSpreadTarget(player);
        var shouldStack = IsStackTarget(player);
        var shouldAvoid = !shouldSpread && !shouldStack && ActiveStacks.Any(s => s.ForbiddenPlayers[playerSlot]);
        if (shouldAvoid)
            customColor = Colors.Vulnerable;
        return shouldAvoid || shouldSpread ? PlayerPriority.Danger
            : shouldStack ? PlayerPriority.Interesting
            : Active ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!AlwaysShowSpreads && Spreads.FindIndex(s => s.Target == pc) is var iSpread && iSpread >= 0)
        {
            // Draw only own circle if spreading; no one should be inside.
            Arena.AddCircle(pc.Position, Spreads[iSpread].Radius);
        }
        else
        {
            void DrawCircle(WPos position, float radius, uint color = 0) => Arena.AddCircle(position, radius, color);
            // Handle safe stack circles
            foreach (var s in ActiveStacks.Where(x => x.Target == pc || !x.ForbiddenPlayers[pcSlot]
                    && !IsSpreadTarget(pc) && !IsStackTarget(pc) && (x.IsInside(pc)
                    && !x.TooManyInside(Module) || !x.IsInside(pc) && x.InsufficientAmountInside(Module))))
                DrawCircle(s.Target.Position, s.Radius, Colors.Safe);

            // Handle dangerous stack circles
            foreach (var s in ActiveStacks.Where(x => x.Target != pc && (IsStackTarget(pc) || x.ForbiddenPlayers[pcSlot] || IsSpreadTarget(pc) ||
                !x.IsInside(pc) && (x.CorrectAmountInside(Module) || x.TooManyInside(Module)) ||
                x.IsInside(pc) && x.TooManyInside(Module))))
                DrawCircle(s.Target.Position, s.Radius);

            // Handle spread circles
            foreach (var s in ActiveSpreads)
                DrawCircle(s.Target.Position, s.Radius);
        }
    }
}

// stack/spread with same properties for all stacks and all spreads (most common variant)
public abstract class UniformStackSpread(BossModule module, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : GenericStackSpread(module, alwaysShowSpreads, raidwideOnResolve, includeDeadTargets)
{
    public float StackRadius = stackRadius;
    public float SpreadRadius = spreadRadius;
    public int MinStackSize = minStackSize;
    public int MaxStackSize = maxStackSize;

    public IEnumerable<Actor> ActiveStackTargets => ActiveStacks.Select(s => s.Target);
    public IEnumerable<Actor> ActiveSpreadTargets => ActiveSpreads.Select(s => s.Target);

    public void AddStack(Actor target, DateTime activation = default, BitMask forbiddenPlayers = default) => Stacks.Add(new(target, StackRadius, MinStackSize, MaxStackSize, activation, forbiddenPlayers));
    public void AddStacks(IEnumerable<Actor> targets, DateTime activation = default) => Stacks.AddRange(targets.Select(target => new Stack(target, StackRadius, MinStackSize, MaxStackSize, activation)));
    public void AddSpread(Actor target, DateTime activation = default) => Spreads.Add(new(target, SpreadRadius, activation));
    public void AddSpreads(IEnumerable<Actor> targets, DateTime activation = default) => Spreads.AddRange(targets.Select(target => new Spread(target, SpreadRadius, activation)));
}

// spread/stack mechanic that selects targets by casts
public class CastStackSpread(BossModule module, ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public ActionID StackAction { get; init; } = stackAID;
    public ActionID SpreadAction { get; init; } = spreadAID;
    public int NumFinishedStacks { get; protected set; }
    public int NumFinishedSpreads { get; protected set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == StackAction && WorldState.Actors.Find(spell.TargetID) is var stackTarget && stackTarget != null)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell));
        }
        else if (spell.Action == SpreadAction && WorldState.Actors.Find(spell.TargetID) is var spreadTarget && spreadTarget != null)
        {
            AddSpread(spreadTarget, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == StackAction)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
            ++NumFinishedStacks;
        }
        else if (spell.Action == SpreadAction)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
            ++NumFinishedSpreads;
        }
    }
}

// generic 'spread from targets of specific cast' mechanic
public class SpreadFromCastTargets(BossModule module, ActionID aid, float radius, bool drawAllSpreads = true) : CastStackSpread(module, default, aid, 0, radius, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with targets of specific cast' mechanic
public class StackWithCastTargets(BossModule module, ActionID aid, float radius, int minStackSize = 2, int maxStackSize = int.MaxValue) : CastStackSpread(module, aid, default, radius, 0, minStackSize, maxStackSize);

// spread/stack mechanic that selects targets by icon and finishes by cast event
public class IconStackSpread(BossModule module, uint stackIcon, uint spreadIcon, ActionID stackAID, ActionID spreadAID, float stackRadius, float spreadRadius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, bool alwaysShowSpreads = false, int maxCasts = 1)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, alwaysShowSpreads)
{
    public uint StackIcon { get; init; } = stackIcon;
    public uint SpreadIcon { get; init; } = spreadIcon;
    public ActionID StackAction { get; init; } = stackAID;
    public ActionID SpreadAction { get; init; } = spreadAID;
    public float ActivationDelay { get; init; } = activationDelay;
    public int NumFinishedStacks { get; protected set; }
    public int NumFinishedSpreads { get; protected set; }
    public int MaxCasts { get; init; } = maxCasts; // for stacks where the final AID hits multiple times
    private int castCounter;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == StackIcon)
        {
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
        }
        else if (iconID == SpreadIcon)
        {
            AddSpread(actor, WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == StackAction)
        {
            if (Stacks.Count == 1 && Stacks.Any(x => x.Target.InstanceID != spell.MainTargetID))
                Stacks[0] = Stacks[0] with { Target = WorldState.Actors.Find(spell.MainTargetID)! };
            if (++castCounter == MaxCasts)
            {
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                ++NumFinishedStacks;
                castCounter = 0;
            }
        }
        else if (spell.Action == SpreadAction)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedSpreads;
        }
    }
}

// generic 'spread from actors with specific icon' mechanic
public class SpreadFromIcon(BossModule module, uint icon, ActionID aid, float radius, float activationDelay, bool drawAllSpreads = true) : IconStackSpread(module, 0, icon, default, aid, 0, radius, activationDelay, alwaysShowSpreads: drawAllSpreads);

// generic 'stack with actors with specific icon' mechanic
public class StackWithIcon(BossModule module, uint icon, ActionID aid, float radius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue, int maxCasts = 1) : IconStackSpread(module, icon, 0, aid, default, radius, 0, activationDelay, minStackSize, maxStackSize, false, maxCasts);

// generic single hit "line stack" component, usually do not have an iconID, instead players get marked by cast event
// usually these have 50 range and 4 halfWidth, but it can be modified
public class LineStack(BossModule module, ActionID aidMarker, ActionID aidResolve, float activationDelay, float range = 50, float halfWidth = 4, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true) : GenericBaitAway(module)
{
    // TODO: add forbidden slots logic?
    // TODO: add logic for min and max stack size
    public ActionID AidMarker { get; init; } = aidMarker;
    public ActionID AidResolve { get; init; } = aidResolve;
    public float ActionDelay { get; init; } = activationDelay;
    public float Range { get; init; } = range;
    public float HalfWidth { get; init; } = halfWidth;
    public int MaxStackSize { get; init; } = maxStackSize;
    public int MinStackSize { get; init; } = minStackSize;
    public int MaxCasts { get; init; } = maxCasts; // for stacks where the final AID hits multiple times
    public bool MarkerIsFinalTarget { get; init; } = markerIsFinalTarget; // rarely the marked player is not the target of the line stack
    public HashSet<Actor> ForbiddenActors { get; init; } = [];
    private int castCounter;
    public const string HintStack = "Stack!";
    public const string HintAvoidOther = "GTFO from other line stacks!";
    public const string HintAvoid = "GTFO from line stacks!";

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == AidMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.MainTargetID)!, new AOEShapeRect(Range, HalfWidth), WorldState.FutureTime(ActionDelay)));
        else if (spell.Action == AidResolve && CurrentBaits.Count > 0)
        {
            if (MarkerIsFinalTarget)
            {
                if (CurrentBaits.Count == 1 && CurrentBaits.Any(x => x.Target.InstanceID != spell.MainTargetID))
                    CurrentBaits[0] = CurrentBaits[0] with { Target = WorldState.Actors.Find(spell.MainTargetID)! };
                if (++castCounter == MaxCasts)
                {
                    CurrentBaits.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                    castCounter = 0;
                    ++NumCasts;
                }
            }
            else
            {
                if (++castCounter == MaxCasts)
                {
                    CurrentBaits.RemoveAt(0);
                    castCounter = 0;
                    ++NumCasts;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ActiveBaits.Any())
            return;
        var isBaitTarget = ActiveBaits.Any(x => x.Target == actor);
        var isBaitNotTarget = ActiveBaits.Any(x => x.Target != actor);
        var forbiddenInverted = new List<Func<WPos, float>>();
        var forbidden = new List<Func<WPos, float>>();
        var forbiddenActors = ForbiddenActors.Contains(actor);
        // if line stack target and NPCs in party, go stack with them. usually they won't come to you
        if (Raid.WithoutSlot().Any(x => x.Type == ActorType.Buddy) && ActiveBaits.Any(x => x.Target == actor))
        {
            var closestAlly = Raid.WithoutSlot().Exclude(actor).Closest(actor.Position)!;
            forbiddenInverted.Add(ShapeDistance.InvertedCircle(closestAlly.Position, 2));
        }
        if (isBaitNotTarget && !isBaitTarget && !forbiddenActors)
            foreach (var b in ActiveBaits.Where(x => x.Target != actor))
                forbiddenInverted.Add(ShapeDistance.InvertedRect(b.Source.Position, b.Rotation, Range, 0, HalfWidth));
        // prevent overlapping if there are multiple line stacks, or if an actor is forbidden to enter
        if (isBaitNotTarget && isBaitTarget || forbiddenActors)
            foreach (var b in ActiveBaits.Where(x => x.Target != actor))
                forbidden.Add(ShapeDistance.Rect(b.Source.Position, b.Rotation, Range, 0, 2 * HalfWidth));
        if (forbiddenInverted.Count > 0)
            hints.AddForbiddenZone(p => forbiddenInverted.Max(f => f(p)), ActiveBaits.FirstOrDefault().Activation);
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Min(f => f(p)), ActiveBaits.FirstOrDefault().Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ActiveBaits.Any())
            return;

        var isBaitTarget = ActiveBaits.Any(x => x.Target == actor);
        var isBaitNotTarget = ActiveBaits.Any(x => x.Target != actor);
        var isInBaitShape = ActiveBaits.Any(x => actor.Position.InRect(x.Source.Position, x.Rotation, Range, 0, HalfWidth));

        if (isBaitNotTarget && !isBaitTarget && !isInBaitShape)
            hints.Add(HintStack);
        else if ((isBaitNotTarget || isBaitTarget) && isInBaitShape)
            hints.Add(HintStack, false);

        if (ActiveBaits.Count() > 1 && isBaitTarget)
        {
            var isInOtherBaitShape = ActiveBaits.Any(x => x.Target != actor && actor.Position.InRect(x.Source.Position, x.Rotation, Range, 0, 2 * HalfWidth));
            if (isInOtherBaitShape)
                hints.Add(HintAvoidOther);
        }

        if (ForbiddenActors.Contains(actor) && isInBaitShape)
            hints.Add(HintAvoid);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!ActiveBaits.Any())
            return;

        var isBaitTarget = ActiveBaits.Any(x => x.Target == pc);
        var isBaitNotTarget = ActiveBaits.Any(x => x.Target != pc);

        foreach (var bait in ActiveBaits)
        {
            var color = isBaitTarget && bait.Target == pc || !isBaitTarget && bait.Target != pc ? Colors.SafeFromAOE : Colors.AOE;
            bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation, color);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

// generic 'donut stack' mechanic
public class DonutStack(BossModule module, ActionID aid, uint icon, float innerRadius, float outerRadius, float activationDelay, int minStackSize = 2, int maxStackSize = int.MaxValue) : UniformStackSpread(module, innerRadius / 3, default, minStackSize, maxStackSize)
{
    // this is a donut targeted on each player, it is best solved by stacking
    // regular stack component won't work because this is self targeted
    public AOEShapeDonut Donut { get; init; } = new(innerRadius, outerRadius);
    public float ActivationDelay { get; init; } = activationDelay;
    public uint Icon { get; init; } = icon;
    public ActionID Aid { get; init; } = aid;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == Icon)
            AddStack(actor, WorldState.FutureTime(ActivationDelay));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == Aid)
            Stacks.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ActiveStacks.Any())
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var c in Raid.WithoutSlot().Where(x => ActiveStacks.Any(y => y.Target == x)).Exclude(actor))
            forbidden.Add(ShapeDistance.InvertedCircle(c.Position, Donut.InnerRadius * 0.25f));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), ActiveStacks.FirstOrDefault().Activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in ActiveStacks)
            Donut.Draw(Arena, c.Target.Position, default, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}
