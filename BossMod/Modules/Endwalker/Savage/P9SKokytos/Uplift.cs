namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, new AOEShapeRect(4f, 8f));

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly Rectangle[] rectsCard = [new(new(100, 119.5f), 8f, 2f), new(new(80.5f, 100f), 2f, 8f),
    new(new(119.5f, 100), 2f, 8f), new(new(100, 80.5f), 8f, 2f)];
    public bool? Cardinal;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00080004u && index is 2 or 3)
        {
            Module.Arena.Bounds = new ArenaBoundsCircle(20f);
        }
        else if (state == 0x00020001u)
        {
            if (index == 2)
            {
                Cardinal = true;
                Module.Arena.Bounds = new ArenaBoundsCustom(P9SKokytos.GetDefaultCircle(), rectsCard);
            }
            else if (index == 3)
            {
                Cardinal = false;
                Rectangle[] rectsIntercard = [RotatedRectangle(new WPos(100f, 119.5f), -45f.Degrees()),
                    RotatedRectangle(new WPos(80.5f, 100f), -135f.Degrees()), RotatedRectangle(new WPos(119.5f, 100f), 45f.Degrees()),
                    RotatedRectangle(new WPos(100f, 80.5f), 135f.Degrees())];
                Module.Arena.Bounds = new ArenaBoundsCustom(P9SKokytos.GetDefaultCircle(), rectsIntercard);
            }
        }

        Rectangle RotatedRectangle(WPos position, Angle rotation)
        {
            var rotatedPosition = WPos.RotateAroundOrigin(45f, Arena.Center, position);
            return new(rotatedPosition, 8f, 2f, rotation);
        }
    }
}
