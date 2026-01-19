namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly List<Rectangle> _rects = new(8);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            // arena break after Bring Down The House
            if (state == 0x00020001u)
            {
                var center = Arena.Center;
                _rects.Add(new(center - new WDir(-15f, 7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(-5f, -7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(5f, 7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(15f, -7.5f), 5f, 7.5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects], Offset: -0.5f);
            }
            // other set of tiles break
            else if (state == 0x02000100u)
            {
                var center = Arena.Center;
                _rects.Add(new(center - new WDir(-15f, -7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(-5f, 7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(5f, -7.5f), 5f, 7.5f));
                _rects.Add(new(center - new WDir(15f, 7.5f), 5f, 7.5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects], Offset: -0.5f);
            }
            // arena partial fix through middle
            else if (state is 0x00200010u or 0x08000400u)
            {
                _rects.Add(new(Arena.Center, 20f, 5f));
                Arena.Bounds = new ArenaBoundsCustom([.. _rects], Offset: -0.5f);
            }
            // arena reset
            else if (state is 0x10000004u or 0x00080004u or 0x00800004u)
            {
                _rects.Clear();
                Arena.Bounds = M12NLindwurm.ArenaBounds;
            }
        }
    }
}
