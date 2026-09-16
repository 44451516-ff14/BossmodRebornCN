namespace BossMod.Stormblood.Ultimate.UWU;

// wicked wheel is used in phase 1 (depending on 'woken' status, it can be used with followup wicked tornado - this can happen with low dps late in the phase) - it is triggered by cast start
// it is also used during phase 4 as part of some mechanics (ultimate predation, ???) - in such case we typically want to show it earlier (based on PATE)
abstract class WickedWheelBase(BossModule module) : Components.GenericAOEs(module)
{
    public DateTime AwakenedResolve;
    public List<(Actor source, AOEShape shape, DateTime activation)> Sources = [];

    public readonly AOEShapeCircle ShapeWheel = new(8.7f);
    public readonly AOEShapeDonut ShapeTornado = new(7f, 20f);
    public readonly AOEShapeCircle ShapeSister = new(8.36f);
    public readonly AOEShapeCircle ShapeCombined = new(20f); // wheel+tornado, used when players are expected to outrange both - e.g. during ultimate predation

    public bool Active => Sources.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Sources.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];

        for (var i = 0; i < count; ++i)
        {
            var s = Sources[i];
            aoes[i] = new(s.shape, s.source.Position, s.source.Rotation, s.activation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WickedWheel:
                // if wheel was predicted, keep the shape, but update the activation time
                var predictedIndex = Sources.FindIndex(s => s.source == caster);
                if (predictedIndex >= 0)
                    Sources[predictedIndex] = (caster, Sources[predictedIndex].shape, Module.CastFinishAt(spell));
                else
                    Sources.Add((caster, ShapeWheel, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.WickedWheelSister:
                Sources.Add((caster, ShapeSister, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.WickedWheel:
                Sources.RemoveAll(s => s.source == caster);
                AwakenedResolve = WorldState.FutureTime(2.1f); // for tornado
                if (caster.FindStatus((uint)SID.Woken) != null)
                    Sources.Add((caster, ShapeTornado, AwakenedResolve));
                ++NumCasts;
                break;
            case (uint)AID.WickedTornado:
                Sources.RemoveAll(s => s.shape == ShapeTornado);
                ++NumCasts;
                break;
            case (uint)AID.WickedWheelSister:
                Sources.RemoveAll(s => s.source == caster);
                ++NumCasts;
                break;
        }
    }
}

sealed class WickedWheel(BossModule module) : WickedWheelBase(module);
sealed class P1WickedWheel(BossModule module) : WickedWheelBase(module);

sealed class P4WickedWheel(BossModule module) : WickedWheelBase(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Garuda && id == 0x1E43)
            Sources.Add((actor, ShapeCombined, WorldState.FutureTime(8.1d)));
    }
}
