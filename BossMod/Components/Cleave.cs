namespace BossMod.Components;

// generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
// enemy OID == 0 means 'primary actor'
public class Cleave(BossModule module, ActionID aid, AOEShape shape, uint enemyOID = 0, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true) : CastCounter(module, aid)
{
    public AOEShape Shape { get; init; } = shape;
    public bool ActiveForUntargetable { get; init; } = activeForUntargetable;
    public bool ActiveWhileCasting { get; init; } = activeWhileCasting;
    public bool OriginAtTarget { get; init; } = originAtTarget;
    public DateTime NextExpected;
    private readonly IReadOnlyList<Actor> _enemies = module.Enemies(enemyOID != 0 ? enemyOID : module.PrimaryActor.OID);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OriginsAndTargets().Any(e => e.target != actor && Shape.Check(actor.Position, e.origin.Position, e.angle)))
        {
            hints.Add("GTFO from cleave!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!OriginsAndTargets().Any())
            return;

        foreach (var (origin, target, angle) in OriginsAndTargets())
            if (actor != target)
                hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
            else
                AddTargetSpecificHints(actor, origin, angle, hints);
    }

    private void AddTargetSpecificHints(Actor actor, Actor source, Angle angle, AIHints hints)
    {
        foreach (var a in Raid.WithoutSlot().Exclude(actor))
            switch (Shape)
            {
                case AOEShapeCircle circle:
                    hints.AddForbiddenZone(circle, a.Position);
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(ShapeDistance.Cone(source.Position, 100, source.AngleTo(a), cone.HalfAngle));
                    break;
                case AOEShapeRect rect:
                    hints.AddForbiddenZone(ShapeDistance.Cone(source.Position, 100, source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - source.Position).Length())));
                    break;
            }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var e in OriginsAndTargets())
        {
            Shape.Outline(Arena, e.origin.Position, e.angle);
        }
    }

    private IEnumerable<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.IsDead)
                continue;

            if (!ActiveForUntargetable && !enemy.IsTargetable)
                continue;

            if (!ActiveWhileCasting && enemy.CastInfo != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
            {
                yield return (OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position));
            }
        }
    }
}
