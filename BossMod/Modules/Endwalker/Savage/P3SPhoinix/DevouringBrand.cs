namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to devouring brand mechanic
class DevouringBrand(BossModule module) : BossComponent(module)
{
    private const float _halfWidth = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var offset = actor.Position - Module.Center;
        if (Math.Abs(offset.X) <= _halfWidth || Math.Abs(offset.Z) <= _halfWidth)
        {
            hints.Add("GTFO from brand!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneRect(Arena.Center, new WDir(1, 0), Arena.Bounds.Radius, Module.Bounds.Radius, _halfWidth, Colors.AOE);
        Arena.ZoneRect(Arena.Center, new WDir(0, +1), Arena.Bounds.Radius, -_halfWidth, _halfWidth, Colors.AOE);
        Arena.ZoneRect(Arena.Center, new WDir(0, -1), Arena.Bounds.Radius, -_halfWidth, _halfWidth, Colors.AOE);
    }
}
