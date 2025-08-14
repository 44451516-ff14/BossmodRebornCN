namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class BrutalSmash(BossModule module) : Components.GenericSharedTankbuster(module, default, 6f)
{
    private bool close;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Source != null)
            hints.Add($"Proximity shared tankbuster");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Target == null)
            return;
        if (Target == actor)
        {
            if (actor.Role == Role.Tank)
            {
                var otherTanksInAOE = false;
                var party = Raid.WithoutSlot(false, true, true);
                var len = party.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var a = ref party[i];
                    if (a != actor && a.Role == Role.Tank && InAOE(a))
                    {
                        otherTanksInAOE = true;
                        break;
                    }
                }
                hints.Add("Stack with other tanks or press invuln!", !otherTanksInAOE);
            }
            else
            {
                hints.Add(close ? "Move further away from boss!" : "Move closer to boss!");
            }
        }
        else if (actor.Role == Role.Tank)
        {
            if (Target.Role != Role.Tank)
            {
                hints.Add(!close ? "Move further away from boss!" : "Move closer to boss!");
            }
            else
                hints.Add("Stack with tank!", !InAOE(actor));
        }
        else if (Target.Role == Role.Tank)
        {
            hints.Add("GTFO from tank!", InAOE(actor));
        }
        bool InAOE(Actor actor) => Source != null && Target != null && Shape.Check(actor.Position, Target);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BrutalSmash1 or (uint)AID.BrutalSmash2)
        {
            Source = null;
            ++NumCasts;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SmashHere or (uint)AID.SmashThere)
        {
            Source = caster;
            close = spell.Action.ID == (uint)AID.SmashHere;
            Activation = Module.CastFinishAt(spell, 2.1d);
        }
    }

    public override void Update()
    {
        if (Source != null)
            Target = FindActorByDistance(close);
    }

    private Actor? FindActorByDistance(bool findClosest)
    {
        var party = Raid.WithoutSlot(false, true, true);
        var source = Module.PrimaryActor;
        Actor? selected = null;

        var bestDistance = findClosest ? float.MaxValue : float.MinValue;
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            var distSq = (p.Position - source.Position).LengthSq();
            var isBetter = findClosest ? distSq < bestDistance : distSq > bestDistance;

            if (isBetter)
            {
                bestDistance = distSq;
                selected = p;
            }
        }
        return selected;
    }
}
