namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class AutoAttacksCube(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<Actor> targets = new(3);
    private static readonly AOEShapeRect rect = new(40.5f, 2f);

    // todo: this is a hack, ideally we need to determine who has the current highest enmity on each platform
    // the hack just assumes that people with highest enmity for cube auto attack never changes
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AutoAttackSphere:
                if (targets.Count == 3)
                    targets.Clear();
                targets.Add(WorldState.Actors.Find(spell.MainTargetID)!);
                break;
            case (uint)AID.FlareStarVisual:
                if (caster == Module.PrimaryActor)
                {
                    var activation = WorldState.FutureTime(4.3d);
                    AddBaits(ref activation);
                }
                break;
            case (uint)AID.AutoAttackCube:
                ++NumCasts;
                if (NumCasts % 3 == 0)
                {
                    CurrentBaits.Clear();
                    if (NumCasts <= 42)
                    {
                        var activation = WorldState.FutureTime(2.6d); // time varies wildly depending on current mechanic, taking lowest
                        AddBaits(ref activation);
                    }
                }
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                NumCasts = 0;
                break;
        }
        void AddBaits(ref DateTime activation)
        {
            var count = targets.Count;
            var s = Module.PrimaryActor;
            for (var i = 0; i < count; ++i)
                CurrentBaits.Add(new(s, targets[i], rect, activation));
        }
    }
}

sealed class AutoAttacksPyramid(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<Actor> players = [];
    private bool active;

    // this is just an estimation, targets quickly look random if not in predetermined spots behind platform black hole buffers...
    public override void Update()
    {
        if (active)
        {
            List<Actor>[] platformActors = [new(20), new(20), new(20)];
            CurrentBaits.Clear();

            var primaryPos = Module.PrimaryActor.Position;
            var countP = players.Count;
            if (countP == 0)
            {
                foreach (var a in Module.WorldState.Actors.Actors.Values)
                    if (a.OID == default)
                        players.Add(a);
            }

            for (var i = 0; i < countP; ++i)
            {
                var a = players[i];
                if (a.IsDead)
                    continue;
                for (var j = 0; j < 3; j++)
                {
                    if (a.Position.InRect(primaryPos, BA4ProtoOzma.Directions[j], 100f, default, 5f))
                    {
                        platformActors[j].Add(a);
                        break;
                    }
                }
            }

            for (var i = 0; i < 3; ++i)
            {
                Actor? closest = null;
                var minDistSq = float.MaxValue;
                var rect = platformActors[i];
                var count = rect.Count;
                for (var j = 0; j < count; ++j)
                {
                    var actor = rect[j];
                    var distSq = (actor.Position - primaryPos).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closest = actor;
                    }
                }
                if (closest != null)
                {
                    CurrentBaits.Add(new(Module.PrimaryActor, closest, circle));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExecrationVisual:
                if (caster == Module.PrimaryActor)
                    active = true;
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                active = false;
                CurrentBaits.Clear();
                break;
        }
    }
}

class AutoAttacksStar(BossModule module) : Components.GenericStackSpread(module)
{
    private bool active;
    private readonly List<Actor> players = [];

    public override void Update()
    {
        if (active)
        {
            List<Actor>[] platformActors = [new(20), new(20), new(20)];
            Stacks.Clear();

            var primaryPos = Module.PrimaryActor.Position;
            var countP = players.Count;
            if (countP == 0)
            {
                foreach (var a in Module.WorldState.Actors.Actors.Values)
                    if (a.OID == default)
                        players.Add(a);
            }

            for (var i = 0; i < countP; ++i)
            {
                var a = players[i];
                if (a.IsDead)
                    continue;
                for (var j = 0; j < 3; j++)
                {
                    if (a.Position.InRect(primaryPos, BA4ProtoOzma.Directions[j], 100f, default, 5f))
                    {
                        platformActors[j].Add(a);
                        break;
                    }
                }
            }

            for (var i = 0; i < 3; ++i)
            {
                Actor? closest = null;
                var minDistSq = float.MaxValue;
                var rect = platformActors[i];
                var count = rect.Count;
                for (var j = 0; j < count; ++j)
                {
                    var actor = rect[j];
                    var distSq = (actor.Position - primaryPos).LengthSq();
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        closest = actor;
                    }
                }
                if (closest != null)
                {
                    Stacks.Add(new(closest, 6f, 8, 24));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MourningStarVisual:
                if (caster == Module.PrimaryActor)
                    active = true;
                break;
            case (uint)AID.TransfigurationSphere1:
            case (uint)AID.TransfigurationSphere2:
            case (uint)AID.TransfigurationSphere3:
                active = false;
                Stacks.Clear();
                break;
        }
    }
}
