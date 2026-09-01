namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
abstract class DragonsGaze(BossModule module, uint bossOID, double activationDelay) : Components.GenericGaze(module, (uint)AID.DragonsGazeAOE)
{
    public bool EnableHints;
    private readonly uint _bossOID = bossOID;
    private Actor? _boss;
    private DateTime _activation;
    private Eye[] _eyes = [];
    public bool Active => _boss != null;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (NumCasts != 0 || !EnableHints || _eyes.Length == 0)
        {
            return [];
        }
        return _eyes;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // seen indices: 2 = E, 5 = SW, 6 = W => inferring 0=N, 1=NE, ... cw order
        if (index <= 0x07 && state == 0x00020001u)
        {
            if (_activation == default)
            {
                _activation = WorldState.FutureTime(activationDelay);
            }
            _boss = Module.Enemies(_bossOID)[0];
            var bossLoc = _boss.Position.Quantized();
            var eyePosition = (Arena.Center + 40f * (180f - index * 45f).Degrees().ToDirection()).Quantized();
            _eyes = new Eye[2];
            _eyes[0] = new(eyePosition, _activation, eyeCenter: IndicatorWorldPos(eyePosition));
            _eyes[1] = new(bossLoc, _activation, eyeCenter: IndicatorWorldPos(bossLoc));
        }
    }
}
