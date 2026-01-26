namespace BossMod.Dawntrail.Savage.M10STheXtremes;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnMapEffect(byte index, uint state)
    {
        // 01.00020001 -> regular arena
        // 01.00200010 -> donut aoe marker
        // 01.00800040 -> donut kill zone, arena turns into 20f circle instead of square
        if (index == 0x01)
        {
            switch (state)
            {
                case 0x00020001:
                    Arena.Bounds = M10STheXtremes.ArenaBounds;
                    break;
                case 0x00200010:
                    _aoes = [new(new AOEShapeDonut(20f, 30f), Arena.Center, activation: WorldState.FutureTime(5d))];
                    break;
                case 0x00800040:
                    _aoes = [];
                    Arena.Bounds = new ArenaBoundsCircle(20f);
                    break;
            }
        }
    }
}
