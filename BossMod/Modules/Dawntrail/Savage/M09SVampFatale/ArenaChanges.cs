namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    // 0F.00020001 -> AOE warning on sides
    // 00.00020001 -> actually remove sides
    // 0F.00080004 -> remove AOE warning on sides
    // indices 0x09 - 0x0C for coffinfiller saws, left to right
    // 00200010 -> saw spawns
    // 04000010 -> charging up AOE and move animation; may line up with cast

    private AOEInstance[] _aoes = [];
    public bool Active = false;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00020001:
                    _aoes = [];
                    Active = true;
                    Arena.Bounds = new ArenaBoundsRect(10f, 20f);
                    break;
                case 0x00080004:
                    Active = false;
                    Arena.Center = new(100f, 100f);
                    Arena.Bounds = new ArenaBoundsSquare(20f);
                    break;
                default:
                    break;
            }
        }

        if (index == 0x0F)
        {
            switch (state)
            {
                case 0x00020001:
                    _aoes = [new(new AOEShapeRect(20f, 5f, 20f), new(85, 100)), new(new AOEShapeRect(20f, 5f, 20f), new(115, 100))];
                    break;
            }
        }

        if (index == 0x10)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsCircle(20);
            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsSquare(20);
        }

        if (index == 0x11)
        {
            switch (state)
            {
                case 0x00020001:
                    Arena.Center = new(100f, 105f);
                    Arena.Bounds = new ArenaBoundsRect(10f, 15f);
                    break;
                case 0x00200010:
                    Arena.Center = new(100f, 110f);
                    Arena.Bounds = new ArenaBoundsRect(10f, 10f);
                    break;
                case 0x00800040:
                    Arena.Center = new(100f, 115f);
                    Arena.Bounds = new ArenaBoundsRect(10f, 5f);
                    break;
            }
        }
    }
}
