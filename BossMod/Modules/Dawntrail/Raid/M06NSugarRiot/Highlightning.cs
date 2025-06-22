namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class Highlightning(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(21f);
    private AOEInstance? _aoe;
    private WPos lastPosition;
    private bool active;
    private DateTime nextActivation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TempestPiece)
        {
            active = true;
            _aoe = new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(6.5d));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Highlightning)
        {
            _aoe = null;
            if (++NumCasts == 3)
            {
                active = false;
                lastPosition = default;
                NumCasts = 0;
                return;
            }
            nextActivation = WorldState.FutureTime(10d);
            lastPosition = caster.Position;
        }
    }

    public override void Update()
    {
        if (!active || _aoe != null)
            return;
        var tempest = Module.Enemies((uint)OID.TempestPiece)[0];
        var angle = (int)Angle.FromDirection(tempest.Position - lastPosition).Deg;
        if (angle == 0)
            return; // cloud didn't start moving yet

        WPos next = angle switch
        {
            -149 or -150 or -90 => new(86.992f, 91.997f),
            90 or 146 or 147 => new(114.977f, 91.997f),
            >= -35 and <= -32 or 28 or 29 => new(99.992f, 114.997f),
            _ => default
        };
        if (next != default)
            _aoe = new(circle, next, default, nextActivation);
    }
}
