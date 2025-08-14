namespace BossMod.Stormblood.Ultimate.UWU;

// in p3, landslide is baited on a random (?) target (rotation phi for main cast); helpers cast their casts at phi +- 45 and phi +- 135
// if boss is awakened, these 5 landslides are followed by another 5 landslides at phi +- 22.5, phi +- 90 and phi + 180; there is no point predicting them, since corresponding casts start almost immediately (<0.1s)
// in p4, landslides are cast at predetermined angles (ultimate predation, ???)
class Landslide(BossModule module) : Components.GenericAOEs(module)
{
    public bool Awakened;
    public DateTime PredictedActivation;
    protected Actor? PredictedSource;
    private readonly List<AOEInstance> _aoes = new(5);

    public static readonly AOEShapeRect ShapeBoss = new(44.55f, 3f, 4.55f);
    public static readonly AOEShapeRect ShapeHelper = new(40.5f, 3f, 0.5f); // difference is only in hitbox radius

    public bool CastsActive => _aoes.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (PredictedSource != null)
        {
            var aoes = new AOEInstance[5];
            var predPos = PredictedSource.Position;
            var predRot = PredictedSource.Rotation;
            aoes[0] = new(ShapeBoss, predPos, predRot, PredictedActivation);
            aoes[1] = new(ShapeHelper, predPos, predRot + 45f.Degrees(), PredictedActivation);
            aoes[2] = new(ShapeHelper, predPos, predRot - 45f.Degrees(), PredictedActivation);
            aoes[3] = new(ShapeHelper, predPos, predRot + 135f.Degrees(), PredictedActivation);
            aoes[4] = new(ShapeHelper, predPos, predRot - 135f.Degrees(), PredictedActivation);
            return aoes;
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LandslideBoss or (uint)AID.LandslideBossAwakened or (uint)AID.LandslideHelper or (uint)AID.LandslideHelperAwakened or (uint)AID.LandslideUltima or (uint)AID.LandslideUltimaHelper)
        {
            _aoes.Add(new(new AOEShapeRect(40f + caster.HitboxRadius, 3f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            PredictedSource = null;
            if (spell.Action.ID == (uint)AID.LandslideBossAwakened)
                Awakened = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LandslideBoss or (uint)AID.LandslideBossAwakened or (uint)AID.LandslideHelper or (uint)AID.LandslideHelperAwakened or (uint)AID.LandslideUltima or (uint)AID.LandslideUltimaHelper)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
            ++NumCasts;
            if (spell.Action.ID == (uint)AID.LandslideBoss)
                PredictedActivation = WorldState.FutureTime(2d); // used if boss wasn't awakened when it should've been
        }
    }
}

class P3Landslide(BossModule module) : Landslide(module);

class P4Landslide(BossModule module) : Landslide(module)
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Titan && id == 0x1E43)
        {
            PredictedSource = actor;
            PredictedActivation = WorldState.FutureTime(8.1d);
        }
    }
}
