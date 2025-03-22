namespace BossMod.Endwalker.Ultimate.DSW2;

class P3DarkdragonDiveCounter(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var numSoakers = spell.Action.ID switch
        {
            (uint)AID.DarkdragonDive1 => 1,
            (uint)AID.DarkdragonDive2 => 2,
            (uint)AID.DarkdragonDive3 => 3,
            (uint)AID.DarkdragonDive4 => 4,
            _ => 0
        };
        if (numSoakers == 0)
            return;

        Towers.Add(new(spell.LocXZ, 5f, numSoakers, numSoakers));
        if (Towers.Count == 4)
            InitAssignments();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkdragonDive1 or (uint)AID.DarkdragonDive2 or (uint)AID.DarkdragonDive3 or (uint)AID.DarkdragonDive4)
        {
            var count = Towers.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var tower = Towers[i];
                if (tower.Position.AlmostEqual(pos, 1f))
                {
                    Towers.Remove(tower);
                    break;
                }
            }
        }
    }

    // 0 = NW, then CW order
    private int ClassifyTower(WPos tower)
    {
        var offset = tower - Arena.Center;
        return offset.Z > 0f ? (offset.X > 0f ? 2 : 3) : (offset.X > 0f ? 1 : 0);
    }

    private void InitAssignments()
    {
        int[] towerIndices = [-1, -1, -1, -1];
        var count = Towers.Count;
        for (var i = 0; i < count; ++i)
            towerIndices[ClassifyTower(Towers[i].Position)] = i;

        var config = Service.Config.Get<DSW2Config>();
        var assign = config.P3DarkdragonDiveCounterGroups.Resolve(Raid);
        foreach (var (slot, group) in assign)
        {
            var pos = group & 3;
            if (group < 4 && Towers[towerIndices[pos]].MinSoakers == 1)
            {
                // flex
                pos = FlexOrder(pos, config.P3DarkdragonDiveCounterPreferCCWFlex).First(p => Towers[towerIndices[p]].MinSoakers > 2);
            }

            ref var tower = ref Towers.AsSpan()[towerIndices[pos]];
            if (tower.ForbiddenSoakers.None())
                tower.ForbiddenSoakers = new(0xff);
            tower.ForbiddenSoakers.Clear(slot);
        }
    }

    private int[] FlexOrder(int starting, bool preferCCW)
    {
        var order = new int[3];
        if (preferCCW)
        {
            order[0] = (starting + 3) & 3;
            order[1] = (starting + 1) & 3;
        }
        else
        {
            order[0] = (starting + 1) & 3;
            order[1] = (starting + 3) & 3;
        }
        order[2] = (starting + 2) & 3;
        return order;
    }
}
