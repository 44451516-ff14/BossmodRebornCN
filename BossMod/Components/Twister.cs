namespace BossMod.Components;

// generic 'twister' component: a set of aoes that appear under players, but can't be accurately predicted until it's too late
// normally you'd predict them at the end (or slightly before the end) of some cast, or on component creation
public abstract class GenericTwister(BossModule module, float radius, uint oid, uint aid = default, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : GenericAOEs(module, aid, "GTFO from twister!")
{
    private readonly AOEShapeCircle _shape = new(radius);
    private readonly uint _twisterOID = oid;
    protected readonly List<Actor> Twisters = module.Enemies(oid);
    protected DateTime PredictedActivation;
    protected readonly List<WPos> PredictedPositions = [];
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    public ReadOnlySpan<Actor> ActiveTwisters
    {
        get
        {
            var count = Twisters.Count;
            var result = new Actor[count];
            var index = 0;

            for (var i = 0; i < count; ++i)
            {
                var twister = Twisters[i];
                if (twister.EventState != 7 && ArenaProjectionLayerParticipantApplies(twister, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                {
                    result[index++] = twister;
                }
            }
            return result.AsSpan(0, index);
        }
    }

    public bool Active => ActiveTwisters.Length != 0;

    public void AddPredicted(double activationDelay) => AddPredicted(WorldState.FutureTime(activationDelay));

    public void AddPredicted(DateTime activationTime)
    {
        PredictedPositions.Clear();
        var raid = Raid.WithoutSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = raid[i];
            if (ArenaProjectionLayerParticipantApplies(a, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                PredictedPositions.Add(a.Position);
            }
        }

        PredictedActivation = activationTime;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countPredictedPositions = PredictedPositions.Count;
        var active = ActiveTwisters;
        var lenActiveTwisters = active.Length;
        var predictedSpan = CollectionsMarshal.AsSpan(PredictedPositions);

        var count = countPredictedPositions + lenActiveTwisters;
        if (count == 0)
        {
            return [];
        }

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = 0; i < countPredictedPositions; ++i)
        {
            aoes[index++] = new AOEInstance(_shape, predictedSpan[i], default, PredictedActivation,
                arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer);
        }

        for (var i = 0; i < lenActiveTwisters; ++i)
        {
            aoes[index++] = new AOEInstance(_shape, active[i].Position,
                arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer);
        }

        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == _twisterOID)
        {
            PredictedPositions.Clear();
        }
    }
}

// twister that activates on cast end, or slightly before
public abstract class CastTwister(BossModule module, float radius, uint oid, uint aid, double spawnDelay, double predictBeforeSpawn = 0d, int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false)
    : GenericTwister(module, radius, oid, aid, arenaProjectionLayer, restrictToArenaProjectionLayer)
{
    public readonly double SpawnDelay = spawnDelay; // from cast-end to twister spawn
    public readonly double PredictionTime = predictBeforeSpawn;
    private DateTime _predictAt = DateTime.MaxValue;
    private DateTime _spawnAt;

    public override void Update()
    {
        if (PredictedPositions.Count == 0 && Twisters.Count == 0 && WorldState.CurrentTime >= _predictAt)
        {
            AddPredicted(_spawnAt);
            _predictAt = DateTime.MaxValue;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _spawnAt = Module.CastFinishAt(spell, SpawnDelay);
            _predictAt = _spawnAt.AddSeconds(-PredictionTime);
        }
    }
}
