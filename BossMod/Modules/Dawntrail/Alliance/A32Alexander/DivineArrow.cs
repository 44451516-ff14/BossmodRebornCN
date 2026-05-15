namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class DivineArrowCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle startrot;
    private readonly AOEShapeCone cone = new(45f, 45.Degrees());
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var inc = -20.Degrees();
        if (iconID is ((uint)IconID.DivineArrowER) or ((uint)IconID.DivineArrowSR) or ((uint)IconID.DivineArrowNR) or ((uint)IconID.DivineArrowWR))
        {
            inc = -inc;
        }
        if (iconID is (uint)IconID.DivineArrowN or ((uint)IconID.DivineArrowNR))
        {
            startrot = 180.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowS or ((uint)IconID.DivineArrowSR))
        {
            startrot = default;
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowE or ((uint)IconID.DivineArrowER))
        {
            startrot = 90.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
        if (iconID is (uint)IconID.DivineArrowW or ((uint)IconID.DivineArrowWR))
        {
            startrot = -90.Degrees();
            Sequences.Add(new(cone, actor.Position, startrot, inc, WorldState.FutureTime(13.3f), 0.67f, 18, maxShownAOEs: 8));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DivineArrowCone or (uint)AID.DivineArrowSpamCone)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

sealed class DivineArrowCircles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DivineArrowClose, (uint)AID.DivineArrowClose2], new AOEShapeCircle(10f), 1)
{
    //protected readonly uint[] CloseAIDs = [(uint)AID.DivineArrowClose, (uint)AID.DivineArrowClose2];
    private readonly uint[] MidAIDs = [(uint)AID.DivineArrowMid, (uint)AID.DivineArrowMid2];
    private readonly AOEShapeDonut MidShape = new(10f, 23f);
    private readonly uint[] FarAIDs = [(uint)AID.DivineArrowFar, (uint)AID.DivineArrowFar2];
    private readonly AOEShapeDonut FarShape = new(23f, 36f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var id = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (id == AIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: Shape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
                return;
            }
        }
        for (var i = 0; i < MidAIDs.Length; i++)
        {
            if (id == MidAIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(MidShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: MidShape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
            }
        }
        for (var i = 0; i < FarAIDs.Length; i++)
        {
            if (id == FarAIDs[i])
            {
                var origin = spell.LocXZ;
                var rotation = spell.Rotation;
                Casters.Add(new(FarShape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: FarShape.Distance(origin, rotation)));
                SortHelpers.SortAOEByActivation(Casters);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Casters.Count;
        var id = caster.InstanceID;
        var aoes = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            if (aoes[i].ActorID == id)
            {
                Casters.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        for (var i = 0; i < len; ++i)
        {
            if (spell.Action.ID == AIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
        for (var i = 0; i < MidAIDs.Length; i++)
        {
            if (spell.Action.ID == MidAIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
        for (var i = 0; i < FarAIDs.Length; i++)
        {
            if (spell.Action.ID == FarAIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}

sealed class DivineArrowLines(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DivineArrowLines, (uint)AID.DivineArrowLines2], new AOEShapeRect(60f, 5f), 6, 6);
