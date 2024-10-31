namespace BossMod.Components;

public class GenericTowers(BossModule module, ActionID aid = default) : CastCounter(module, aid)
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
        public readonly int NumInside(BossModule module) => module.Raid.WithSlot().ExcludedFromMask(ForbiddenSoakers).InRadius(Position, Radius).Count();
        public readonly bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public readonly bool InsufficientAmountInside(BossModule module) => NumInside(module) is var count && count < MaxSoakers;
        public readonly bool TooManyInside(BossModule module) => NumInside(module) is var count && count > MaxSoakers;
    }

    public List<Tower> Towers = [];

    // default tower styling
    public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
    {
        arena.AddCircle(pos, radius, safe ? Colors.Safe : 0, 2);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Towers.Any(t => t.ForbiddenSoakers[slot] && t.IsInside(actor)))
        {
            hints.Add("GTFO from tower!");
        }
        else if (Towers.FindIndex(t => !t.ForbiddenSoakers[slot] && t.IsInside(actor)) is var soakedIndex && soakedIndex >= 0) // note: this assumes towers don't overlap
        {
            var count = Towers[soakedIndex].NumInside(Module);
            if (count < Towers[soakedIndex].MinSoakers)
                hints.Add("Too few soakers in the tower!");
            else if (count > Towers[soakedIndex].MaxSoakers)
                hints.Add("Too many soakers in the tower!");
            else
                hints.Add("Soak the tower!", false);
        }
        else if (Towers.Any(t => !t.ForbiddenSoakers[slot] && t.InsufficientAmountInside(Module)))
        {
            hints.Add("Soak the tower!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in Towers)
            DrawTower(Arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot] && !t.IsInside(pc) && t.NumInside(Module) < t.MaxSoakers || t.IsInside(pc) && !t.ForbiddenSoakers[pcSlot] && t.NumInside(Module) <= t.MaxSoakers);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count == 0)
            return;
        var forbiddenInverted = new List<Func<WPos, float>>();
        var forbidden = new List<Func<WPos, float>>();
        if (!Towers.Any(x => x.ForbiddenSoakers[slot]))
        {
            foreach (var t in Towers.Where(x => !x.IsInside(actor) && x.InsufficientAmountInside(Module) && x.NumInside(Module) > 0))
                forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
            var inTower = Towers.Any(x => x.IsInside(actor) && x.CorrectAmountInside(Module));
            var missingSoakers = !inTower && Towers.Any(x => x.InsufficientAmountInside(Module));
            if (forbiddenInverted.Count == 0)
            {
                foreach (var t in Towers.Where(x => x.InsufficientAmountInside(Module) || x.IsInside(actor) && x.CorrectAmountInside(Module)))
                    forbiddenInverted.Add(ShapeDistance.InvertedCircle(t.Position, t.Radius));
                foreach (var t in Towers.Where(x => x.TooManyInside(Module) || !x.IsInside(actor) && x.CorrectAmountInside(Module)))
                    forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
            }
            if (forbidden.Count == 0 || inTower || missingSoakers && forbiddenInverted.Count > 0)
                hints.AddForbiddenZone(p => forbiddenInverted.Max(f => f(p)), Towers[0].Activation);
            else if (forbidden.Count > 0 && !inTower)
                hints.AddForbiddenZone(p => forbidden.Min(f => f(p)), Towers[0].Activation);
        }
        else if (Towers.Any(x => x.ForbiddenSoakers[slot]))
        {
            foreach (var t in Towers)
                forbidden.Add(ShapeDistance.Circle(t.Position, t.Radius));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Min(f => f(p)), Towers[0].Activation);
        }
        foreach (var t in Towers)
            hints.PredictedDamage.Add((Module.Raid.WithSlot().ExcludedFromMask(t.ForbiddenSoakers).InRadius(t.Position, t.Radius).Mask(), t.Activation));
    }
}

public class CastTowers(BossModule module, ActionID aid, float radius, int minSoakers = 1, int maxSoakers = 1) : GenericTowers(module, aid)
{
    public float Radius = radius;
    public int MinSoakers = minSoakers;
    public int MaxSoakers = maxSoakers;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Towers.Add(new(DeterminePosition(caster, spell), Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var pos = DeterminePosition(caster, spell);
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
        }
    }

    private WPos DeterminePosition(Actor caster, ActorCastInfo spell) => spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
}
