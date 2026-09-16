namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class Predict(BossModule module) : Components.GenericAOEs(module)
{
    // spawns 3x ForetoldPhenomenon and 3x 0x1EC00F
    // all actor creation + status gain happening on same timestamp in replay
    // other one looks like final position of ForetoldPhenomenon, 15.5f from center
    // does it always travel CW?
    // don't add AI hint for donut in EX; in some cases better to stand between the 2 circles
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<(Actor Actor, ushort Extra)> _predicts = [];
    private readonly List<Actor> _debugs = [];
    private readonly float _distance = 15.5f;
    private readonly AOEShapeDonut _donut = new(4f, 15f);
    private readonly AOEShapeCircle _circle = new(10f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ForetoldMarker)
        {
            _debugs.Add(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Predict)
        {
            _predicts.Add((actor, status.Extra));

            var position = actor.Position;
            var center = Module.PrimaryActor.Position;
            var direction = (position - center).Normalized();
            var rotate = direction.Rotate(-60f.Degrees());
            var dir = rotate * _distance;
            var activation = WorldState.CurrentTime.AddSeconds(9.8d);
            _aoes.Add(new(status.Extra == 0x44C ? _donut : _circle, center + dir, activation: activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Starfall or (uint)AID.Cleansing)
        {
            _aoes.RemoveAt(0);
            _debugs.Clear();
            _predicts.Clear();
        }
    }
    /*
#if DEBUG
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var dcount = _debugs.Count;
        var pcount = _predicts.Count;

        for (var i = 0; i < dcount; i++)
        {
            var act = _debugs[i];
            Arena.ZoneCircle(act.Position, 1.5f, 0xFF888800);
        }

        for (var i = 0; i < pcount; i++)
        {
            var act = _predicts[i];
            if (act.Extra == 0x44C)
            {
                Arena.ZoneDonut(act.Actor.Position, 4f, 15f, default);
            }
            else
            {
                Arena.ZoneCircle(act.Actor.Position, 10f, default);
            }
        }
    }
#endif
    */
}
