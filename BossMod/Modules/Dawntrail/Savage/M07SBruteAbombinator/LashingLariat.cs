namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class LashingLariat(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rectPrediction = new(25f, 70f, 7f);
    private static readonly AOEShapeRect rect = new(70f, 16f);
    private Actor? tetheredWall;
    private Angle angle;
    private AOEInstance[] _aoe = [];
    private readonly M07SBruteAbombinatorConfig _config = Service.Config.Get<M07SBruteAbombinatorConfig>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.Wall && source.OID == (uint)OID.Wall)
        {
            tetheredWall = source;
            InitIfReady();
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID != (uint)SID.Stoneringer)
            return;
        angle = status.Extra switch
        {
            0x388 or 0x38A => -90f.Degrees(),
            0x389 or 0x38B => 90.Degrees(),
            _ => default
        };
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_config.EnableLariatPrediction && tetheredWall != null && angle != default)
        {
            _aoe = [new(rectPrediction, tetheredWall.Position, tetheredWall.Rotation + angle, WorldState.FutureTime(13.8d))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID) // update prediction to ensure pixel perfectness
        {
            case (uint)AID.LashingLariat1:
            case (uint)AID.LashingLariat2:
                _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LashingLariat1:
            case (uint)AID.LashingLariat2:
                ++NumCasts;
                break;
        }
    }
}
