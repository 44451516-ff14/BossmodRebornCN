namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class DiffuseEnergy(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(12f, 60f.Degrees());
    private readonly List<(Actor source, Actor target)> tethers = new(4);
    private readonly List<(Actor source, WDir direction)> sources = new(2);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID != default && tether.ID == (uint)TetherID.ChildsPlay && WorldState.Actors.Find(tether.Target) is Actor t)
        {
            tethers.Add((source, t));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -120f.Degrees(),
            (uint)IconID.RotateCCW => 120f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            var count = tethers.Count;
            var isTethered = false;
            var index = -1;
            for (var i = 0; i < count; ++i)
            {
                if (tethers[i].source == actor)
                {
                    isTethered = true;
                    index = i;
                    break;
                }
            }
            Sequences.Add(new(cone, actor.Position.Quantized(), actor.Rotation, increment, WorldState.FutureTime(5d), 2.7d, isTethered ? 2 : 6, actorID: actor.InstanceID));

            if (!isTethered)
            {
                return;
            }
            // add moving rotations
            var t = tethers[index];
            var countS = sources.Count;
            for (var i = 0; i < countS; ++i)
            {
                var s = sources[i];
                if (t.target == s.source)
                {
                    Sequences.Add(new(cone, (actor.Position + s.direction).Quantized(), actor.Rotation + 2f * increment, increment, WorldState.FutureTime(13.3d), 2.7d, 3, actorID: actor.InstanceID));
                    tethers.RemoveAt(index);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChildsPlay1:
                sources.Add((caster, -16.9f * caster.Rotation.Round(90f).ToDirection()));
                break;
            case (uint)AID.ChildsPlay2:
                sources.Add((caster, 16.9f * (caster.Rotation.Round(90f) + 90f.Degrees()).ToDirection()));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChildsPlay1 or (uint)AID.ChildsPlay2)
        {
            sources.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DiffuseEnergyFirst or (uint)AID.DiffuseEnergyRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, caster.InstanceID, WorldState.CurrentTime);
        }
    }
}
