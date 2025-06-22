namespace BossMod.Stormblood.Ultimate.UWU;

// predict puddles under all players until actual casts start
class P1FeatherRain(BossModule module) : Components.GenericAOEs(module, (uint)AID.FeatherRain)
{
    private readonly List<WPos> _predicted = new(8);
    private readonly List<AOEInstance> _aoes = new(8);
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(3f);

    public bool CastsPredicted => _predicted.Count > 0;
    public bool CastsActive => _aoes.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var predictedCount = _predicted.Count;
        var aoesCount = _aoes.Count;
        var totalCount = predictedCount + aoesCount;

        if (totalCount == 0)
            return [];

        var aoes = new AOEInstance[totalCount];
        var index = 0;

        for (var i = 0; i < predictedCount; ++i)
        {
            var p = _predicted[i];
            aoes[index++] = new(_shape, p, default, _activation);
        }

        for (var i = 0; i < aoesCount; ++i)
            aoes[index++] = _aoes[i];
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E3A && actor.OID is (uint)OID.Garuda or (uint)OID.GarudaSister)
        {
            _predicted.Clear();
            var party = Raid.WithoutSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
                _predicted.Add(party[i].Position);
            _activation = WorldState.FutureTime(2.5d);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _predicted.Clear();
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoes.RemoveAt(0);
        }
    }
}
