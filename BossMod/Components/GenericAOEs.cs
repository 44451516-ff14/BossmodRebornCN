namespace BossMod.Components;

// generic component that shows arbitrary shapes representing avoidable aoes
public abstract class GenericAOEs(BossModule module, ActionID aid = default, string warningText = "GTFO from aoe!") : CastCounter(module, aid)
{
    public record struct AOEInstance(AOEShape Shape, WPos Origin, Angle Rotation = default, DateTime Activation = default, uint Color = 0, bool Risky = true, ulong ActorID = default)
    {
        public readonly bool Check(WPos pos) => Shape.Check(pos, Origin, Rotation);
    }

    public string WarningText = warningText;

    public abstract ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Risky && aoe.Check(actor.Position))
            {
                hints.Add(WarningText);
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            if (c.Risky)
                hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var aoes = ActiveAOEs(pcSlot, pc);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color == 0 ? Colors.AOE : c.Color);
        }
    }
}

// For simple AOEs, formerly known as SelfTargetedAOEs and LocationTargetedAOEs, that happens at the end of the cast
public class SimpleAOEs(BossModule module, ActionID aid, AOEShape shape, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = 0d) : GenericAOEs(module, aid)
{
    public SimpleAOEs(BossModule module, ActionID aid, float radius, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = 0d) : this(module, aid, new AOEShapeCircle(radius), maxCasts, riskyWithSecondsLeft) { }
    public readonly AOEShape Shape = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public int? MaxDangerColor;
    public int? MaxRisky; // set a maximum amount of AOEs that are considered risky
    public readonly double RiskyWithSecondsLeft = riskyWithSecondsLeft; // can be used to delay risky status of AOEs, so AI waits longer to dodge, if 0 it will just use the bool Risky

    public readonly List<AOEInstance> Casters = [];

    public ReadOnlySpan<AOEInstance> ActiveCasters
    {
        get
        {
            var count = Casters.Count;
            var max = count > MaxCasts ? MaxCasts : count;
            return CollectionsMarshal.AsSpan(Casters)[..max];
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];

        var time = WorldState.CurrentTime;
        var max = count > MaxCasts ? MaxCasts : count;
        var hasMaxDangerColor = count > MaxDangerColor;

        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            var color = (hasMaxDangerColor && i < MaxDangerColor) ? Colors.Danger : default;
            var risky = Risky && (MaxRisky == null || i < MaxRisky);

            if (RiskyWithSecondsLeft != 0)
                risky &= aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
            aoe.Color = color;
            aoe.Risky = risky;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), ActorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var count = Casters.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (Casters[i].ActorID == id)
                {
                    Casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

// 'charge at location' aoes that happen at the end of the cast
public class ChargeAOEs(BossModule module, ActionID aid, float halfWidth, int maxCasts = int.MaxValue, float riskyWithSecondsLeft = 0) : SimpleAOEs(module, aid, new AOEShapeCircle(default), maxCasts, riskyWithSecondsLeft)
{
    public readonly float HalfWidth = halfWidth;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var dir = spell.LocXZ - caster.Position;
            Casters.Add(new(new AOEShapeRect(dir.Length(), HalfWidth), WPos.ClampToGrid(caster.Position), Angle.FromDirection(dir), Module.CastFinishAt(spell), ActorID: caster.InstanceID));
        }
    }
}
