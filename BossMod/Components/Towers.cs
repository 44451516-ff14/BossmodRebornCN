namespace BossMod.Components;

public class GenericTowers(BossModule module, ActionID aid = default, bool prioritizeInsufficient = false) : CastCounter(module, aid)
{
    public struct Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, BitMask forbiddenSoakers = default, DateTime activation = default)
    {
        public WPos Position = position;
        public float Radius = radius;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public BitMask ForbiddenSoakers = forbiddenSoakers;
        public DateTime Activation = activation;

        public readonly bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);

        public readonly int NumInside(BossModule module)
        {
            var count = 0;
            var party = module.Raid.WithSlot();
            for (var i = 0; i < party.Length; ++i)
            {
                var indexActor = party[i];
                if (!ForbiddenSoakers[indexActor.Item1] && indexActor.Item2.Position.InCircle(Position, Radius))
                    ++count;
            }
            return count;
        }
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public readonly bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSoakers;
        public readonly bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSoakers;
    }

    public List<Tower> Towers = [];
    public readonly bool PrioritizeInsufficient = prioritizeInsufficient; // give priority to towers with more than 0 but less than min soakers

    // default tower styling
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        arena.AddCircle(pos, radius, safe ? Colors.Safe : 0, 2);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var gtfoFromTower = false;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            if (t.ForbiddenSoakers[slot] && t.IsInside(actor))
            {
                gtfoFromTower = true;
                break;
            }
        }

        if (gtfoFromTower)
        {
            hints.Add("GTFO from tower!");
        }
        else // Find index of a tower that is not forbidden and the actor is inside
        {
            var soakedIndex = -1;
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                if (!t.ForbiddenSoakers[slot] && t.IsInside(actor))
                {
                    soakedIndex = i;
                    break;
                }
            }

            if (soakedIndex >= 0) // If a suitable tower is found
            {
                var count2 = Towers[soakedIndex].NumInside(Module);
                if (count2 < Towers[soakedIndex].MinSoakers)
                    hints.Add("Too few soakers in the tower!");
                else if (count2 > Towers[soakedIndex].MaxSoakers)
                    hints.Add("Too many soakers in the tower!");
                else
                    hints.Add("Soak the tower!", false);
            }
            else // Check if any tower has insufficient soakers
            {
                var insufficientSoakers = false;
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (!t.ForbiddenSoakers[slot] && t.InsufficientAmountInside(Module))
                    {
                        insufficientSoakers = true;
                        break;
                    }
                }
                if (insufficientSoakers)
                    hints.Add("Soak the tower!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot] && !t.IsInside(pc) && t.NumInside(Module) < t.MaxSoakers || t.IsInside(pc) && !t.ForbiddenSoakers[pcSlot] && t.NumInside(Module) <= t.MaxSoakers);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var forbiddenInverted = new List<Func<WPos, float>>(count);
        var forbidden = new List<Func<WPos, float>>(count);

        var hasForbiddenSoakers = false;
        for (var i = 0; i < count; ++i)
        {
            if (Towers[i].ForbiddenSoakers[slot])
            {
                hasForbiddenSoakers = true;
                break;
            }
        }
        if (!hasForbiddenSoakers)
        {
            if (PrioritizeInsufficient)
            {
                List<Tower> insufficientTowers = new(count);
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module) && t.NumInside(Module) != 0)
                        insufficientTowers.Add(t);
                }
                Tower? mostRelevantTower = null;

                for (var i = 0; i < insufficientTowers.Count; ++i)
                {
                    var t = insufficientTowers[i];
                    if (mostRelevantTower == null || t.NumInside(Module) > mostRelevantTower.Value.NumInside(Module) || t.NumInside(Module) == mostRelevantTower.Value.NumInside(Module) &&
                        (t.Position - actor.Position).LengthSq() < (mostRelevantTower.Value.Position - actor.Position).LengthSq())
                        mostRelevantTower = t;
                }
                if (mostRelevantTower.HasValue)
                    forbiddenInverted.Add(ShapeDistance.InvertedCircle(mostRelevantTower.Value.Position, mostRelevantTower.Value.Radius));
            }
            var inTower = false;
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                if (t.IsInside(actor) && t.CorrectAmountInside(Module))
                {
                    inTower = true;
                    break;
                }
            }

            var missingSoakers = !inTower;
            if (missingSoakers)
            {
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module))
                    {
                        missingSoakers = true;
                        break;
                    }
                }
            }
            if (forbiddenInverted.Count == 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module) || t.IsInside(actor) && t.CorrectAmountInside(Module))
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                }

                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.TooManyInside(Module) || !t.IsInside(actor) && t.CorrectAmountInside(Module))
                    {
                        forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
                    }
                }
            }
            var ficount = forbiddenInverted.Count;
            var fcount = forbidden.Count;
            if (fcount == 0 || inTower || missingSoakers && ficount != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), Towers[0].Activation);
            }
            else if (fcount != 0 && !inTower)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers[0].Activation);
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }
            var fcount = forbidden.Count;
            if (fcount != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers[0].Activation);
            }
        }
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            var actors = Module.Raid.WithSlot();
            var acount = actors.Length;
            var filteredActors = new List<(int, Actor)>(acount);

            for (var j = 0; j < acount; ++j)
            {
                var indexActor = actors[j];
                if (!t.ForbiddenSoakers[indexActor.Item1] && indexActor.Item2.Position.InCircle(t.Position, t.Radius))
                {
                    filteredActors.Add(indexActor);
                }
            }

            var mask = new BitMask();
            for (var j = 0; j < filteredActors.Count; ++j)
                mask[filteredActors[j].Item1] = true;

            hints.PredictedDamage.Add((mask, t.Activation));
        }
    }
}

public class CastTowers(BossModule module, ActionID aid, float radius, int minSoakers = 1, int maxSoakers = 1) : GenericTowers(module, aid)
{
    public readonly float Radius = radius;
    public readonly int MinSoakers = minSoakers;
    public readonly int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; ++i)
            {
                var tower = Towers[i];
                if (tower.Position == spell.LocXZ)
                {
                    Towers.Remove(tower);
                    break;
                }
            }
        }
    }
}

// for tower mechanics in open world since likely not everyone is in your party
public class GenericTowersOpenWorld(BossModule module, ActionID aid = default, bool prioritizeInsufficient = false, bool prioritizeEmpty = false) : CastCounter(module, aid)
{
    public struct Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, HashSet<Actor>? allowedSoakers = null, DateTime activation = default)
    {
        public WPos Position = position;
        public float Radius = radius;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public HashSet<Actor>? AllowedSoakers = allowedSoakers;
        public DateTime Activation = activation;

        public readonly bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
        public readonly bool IsInside(Actor actor) => IsInside(actor.Position);
        public static HashSet<Actor> Soakers(BossModule module)
        {
            HashSet<Actor> actors = new(module.WorldState.Actors.Actors.Values.Count);
            foreach (var a in module.WorldState.Actors.Actors.Values)
                if (a.OID == 0)
                    actors.Add(a);
            return actors;
        }

        public int NumInside(BossModule module)
        {
            var count = 0;
            var allowedSoakers = AllowedSoakers ??= Soakers(module);
            foreach (var a in allowedSoakers)
            {
                if (a.Position.InCircle(Position, Radius))
                    ++count;
            }
            return count;
        }

        public bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSoakers;
        public bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSoakers;
    }

    public readonly List<Tower> Towers = [];
    public readonly bool PrioritizeInsufficient = prioritizeInsufficient; // give priority to towers with more than 0 but less than min soakers
    public readonly bool PrioritizeEmpty = prioritizeEmpty; // give priority to towers with 0 soakers

    // default tower styling
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        arena.AddCircle(pos, radius, safe ? Colors.Safe : 0, 2);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var gtfoFromTower = false;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            var allowedSoakers = t.AllowedSoakers ??= Tower.Soakers(Module);
            if (!allowedSoakers.Contains(actor) && t.IsInside(actor))
            {
                gtfoFromTower = true;
                break;
            }
        }

        if (gtfoFromTower)
        {
            hints.Add("GTFO from tower!");
        }
        else // Find index of a tower that is not forbidden and the actor is inside
        {
            var soakedIndex = -1;
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                var allowedSoakers = t.AllowedSoakers ??= Tower.Soakers(Module);
                if (allowedSoakers.Contains(actor) && t.IsInside(actor))
                {
                    soakedIndex = i;
                    break;
                }
            }

            if (soakedIndex >= 0) // If a suitable tower is found
            {
                var count2 = Towers[soakedIndex].NumInside(Module);
                if (count2 < Towers[soakedIndex].MinSoakers)
                    hints.Add("Too few soakers in the tower!");
                else if (count2 > Towers[soakedIndex].MaxSoakers)
                    hints.Add("Too many soakers in the tower!");
                else
                    hints.Add("Soak the tower!", false);
            }
            else // Check if any tower has insufficient soakers
            {
                var insufficientSoakers = false;
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    var allowedSoakers = t.AllowedSoakers ??= Tower.Soakers(Module);
                    if (allowedSoakers.Contains(actor) && t.InsufficientAmountInside(Module))
                    {
                        insufficientSoakers = true;
                        break;
                    }
                }
                if (insufficientSoakers)
                    hints.Add("Soak the tower!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            var allowedSoakers = t.AllowedSoakers ??= Tower.Soakers(Module);
            DrawTower(Arena, t.Position, t.Radius, allowedSoakers.Contains(pc) && !t.IsInside(pc) && t.NumInside(Module) < t.MaxSoakers || t.IsInside(pc) && allowedSoakers.Contains(pc) && t.NumInside(Module) <= t.MaxSoakers);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var forbiddenInverted = new List<Func<WPos, float>>(count);
        var forbidden = new List<Func<WPos, float>>(count);

        var hasForbiddenSoakers = false;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            var allowedSoakers = t.AllowedSoakers ??= Tower.Soakers(Module);
            if (!allowedSoakers.Contains(actor))
            {
                hasForbiddenSoakers = true;
                break;
            }
        }
        if (!hasForbiddenSoakers)
        {
            if (PrioritizeEmpty)
            {
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.NumInside(Module) == 0)
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                }
            }
            else if (PrioritizeInsufficient) // less soakers than max
            {
                List<Tower> insufficientTowers = new(count);
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module) && t.NumInside(Module) != 0)
                        insufficientTowers.Add(t);
                }
                Tower? mostRelevantTower = null;

                for (var i = 0; i < insufficientTowers.Count; ++i)
                {
                    var t = insufficientTowers[i];
                    if (mostRelevantTower == null || t.NumInside(Module) > mostRelevantTower.Value.NumInside(Module) || t.NumInside(Module) == mostRelevantTower.Value.NumInside(Module) &&
                        (t.Position - actor.Position).LengthSq() < (mostRelevantTower.Value.Position - actor.Position).LengthSq())
                        mostRelevantTower = t;
                }
                if (mostRelevantTower.HasValue)
                    forbiddenInverted.Add(ShapeDistance.InvertedCircle(mostRelevantTower.Value.Position, mostRelevantTower.Value.Radius));
            }
            var inTower = false;
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                if (t.IsInside(actor) && t.CorrectAmountInside(Module))
                {
                    inTower = true;
                    break;
                }
            }

            var missingSoakers = !inTower;
            if (missingSoakers)
            {
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module))
                    {
                        missingSoakers = true;
                        break;
                    }
                }
            }
            if (forbiddenInverted.Count == 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.InsufficientAmountInside(Module) || t.IsInside(actor) && t.CorrectAmountInside(Module))
                        forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                }

                for (var i = 0; i < count; ++i)
                {
                    var t = Towers[i];
                    if (t.TooManyInside(Module) || !t.IsInside(actor) && t.CorrectAmountInside(Module))
                    {
                        forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
                    }
                }
            }
            var ficount = forbiddenInverted.Count;
            var fcount = forbidden.Count;
            if (fcount == 0 || inTower || missingSoakers && ficount != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection(forbiddenInverted), Towers[0].Activation);
            }
            else if (fcount != 0 && !inTower)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers[0].Activation);
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                var t = Towers[i];
                forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }

            if (forbidden.Count != 0)
            {
                hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Towers[0].Activation);
            }
        }
    }
}

public class CastTowersOpenWorld(BossModule module, ActionID aid, float radius, int minSoakers = 1, int maxSoakers = 1, bool prioritizeInsufficient = false, bool prioritizeEmpty = false) : GenericTowersOpenWorld(module, aid, prioritizeInsufficient, prioritizeEmpty)
{
    public readonly float Radius = radius;
    public readonly int MinSoakers = minSoakers;
    public readonly int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            for (var i = 0; i < Towers.Count; ++i)
            {
                var tower = Towers[i];
                if (tower.Position == spell.LocXZ)
                {
                    Towers.Remove(tower);
                    break;
                }
            }
        }
    }
}
