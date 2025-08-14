namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Owain;

sealed class IvoryPalm(BossModule module) : Components.GenericGaze(module)
{
    public readonly List<(Actor target, Actor source)> Tethers = new(2);

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Tethers.Count;
        if (count == 0)
            return [];

        for (var i = 0; i < count; ++i)
        {
            var tether = Tethers[i];
            if (tether.target == actor && !tether.source.IsDead) // apparently tethers don't get removed immediately upon death
            {
                return new Eye[1] { new(tether.source.Position, inverted: true) };
            }
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (HitByEye(ref actor, eye) != eye.Inverted)
            {
                hints.Add("Face the hand to petrify it!");
                break;
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        // only tethers in this fight are from this mechanic, so no need to check tether IDs
        Tethers.Add((WorldState.Actors.Find(tether.Target)!, source));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        Tethers.Remove((WorldState.Actors.Find(tether.Target)!, source));
    }
}

sealed class IvoryPalmExplosion(BossModule module) : Components.CastHint(module, (uint)AID.Explosion, "Ivory Palm is enraging!", true);

sealed class EurekanAero(BossModule module) : Components.Cleave(module, (uint)AID.EurekanAero, new AOEShapeCone(6f, 60f.Degrees()), [(uint)OID.IvoryPalm])
{
    public override List<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        var enemies = Module.Enemies(EnemyOID);
        var count = enemies.Count;
        List<(Actor, Actor, Angle)> origins = new(count);
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.IsDead || enemy.FindStatus((uint)SID.Petrification) != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
                origins.Add(new(OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position)));
        }
        return origins;
    }
}
