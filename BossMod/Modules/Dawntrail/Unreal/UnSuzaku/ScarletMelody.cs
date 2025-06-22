namespace BossMod.Dawntrail.Unreal.UnSuzaku;

sealed class RapturousEchoTowers(BossModule module) : Components.GenericTowers(module)
{
    private readonly int party = module.Raid.WithoutSlot(true, false, false).Length;
    private bool done;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.RapturousEchoPlatform && state != 0x00010002 && Towers.Count <= party && !done)
        {
            Towers.Add(new(actor.Position, 1f, 1, 1, default, WorldState.FutureTime(6.7d)));
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000001 && param1 == 0x00000001)
        {
            Towers.Clear();
            done = true;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var isRisky = true;
        for (var i = 0; i < count; ++i)
        {
            var t = Towers[i];
            if (t.IsInside(actor))
            {
                isRisky = false;
                break;
            }
        }
        hints.Add("Stand in a tower and match the arrow direction!", isRisky);
    }
}

sealed class ScarletMelody(BossModule module) : BossComponent(module)
{
    private readonly Dictionary<ulong, (WPos Position, Angle direction, DateTime time)> _towerData = [];
    private static readonly Angle a175 = 175f.Degrees();

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.RapturousEchoPlatform)
            return;
        Angle? direction = state switch
        {
            0x00080004u => new Angle(), // north
            0x02000100u => -90f.Degrees(), // east
            0x00400020u => 180f.Degrees(), // south
            0x10000800u => 90f.Degrees(), // west
            _ => null
        };
        if (direction != null)
            _towerData[actor.InstanceID] = (actor.Position, direction.Value, WorldState.FutureTime(1d));
        else
            _towerData.Remove(actor.InstanceID);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towerData.Count == 0)
            return;

        foreach (var tower in _towerData)
        {
            var value = tower.Value;
            if (actor.Position.InCircle(value.Position, 1f))
            {
                hints.ForbiddenDirections.Add((value.direction, a175, value.time));
                return;
            }
        }
    }
}
