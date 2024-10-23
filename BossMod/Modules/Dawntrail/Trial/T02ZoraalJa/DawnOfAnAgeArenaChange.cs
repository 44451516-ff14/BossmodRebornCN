namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Square square = new(T02ZoraalJa.ZoraalJa.ArenaCenter, 20, T02ZoraalJa.ZoraalJa.ArenaRotation);
    private static readonly Square smallsquare = new(T02ZoraalJa.ZoraalJa.ArenaCenter, 10, T02ZoraalJa.ZoraalJa.ArenaRotation);
    private static readonly AOEShapeCustom transition = new([square], [smallsquare]);
    private const uint end = 0x00080004;
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x20)
        {
            switch (state)
            {
                case 0x00020001:
                    _aoe = new(transition, T02ZoraalJa.ZoraalJa.ArenaCenter, default, WorldState.FutureTime(8));
                    break;
                case end:
                    _aoe = null;
                    Arena.Bounds = T02ZoraalJa.ZoraalJa.SmallBounds;
                    break;
            }
        }
        else if (index == 0x1B && state == end)
            Arena.Bounds = T02ZoraalJa.ZoraalJa.DefaultBounds;
    }
}
