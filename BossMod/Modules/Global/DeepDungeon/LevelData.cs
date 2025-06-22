namespace BossMod.Global.DeepDungeon;

public sealed record class Floor<T>(uint DungeonId, uint Floorset, Tileset<T> RoomsA, Tileset<T> RoomsB)
{
    public Floor<M> Map<M>(Func<T, M> Mapping) => new(DungeonId, Floorset, RoomsA.Map(Mapping), RoomsB.Map(Mapping));
}

public sealed record class Tileset<T>
{
    private readonly RoomData<T>[] _rooms;

    public Tileset(RoomData<T>[] rooms)
    {
        _rooms = rooms;
    }

    public Tileset<M> Map<M>(Func<T, M> Mapping)
    {
        var len = _rooms.Length;
        var mappedRooms = new RoomData<M>[len];
        for (var i = 0; i < len; ++i)
        {
            mappedRooms[i] = _rooms[i].Map(Mapping);
        }
        return new Tileset<M>(mappedRooms);
    }

    public RoomData<T> this[int index] => _rooms[index];

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Tileset { Rooms = [");
        var len = _rooms.Length;
        for (var i = 0; i < len; ++i)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }
            sb.Append(_rooms[i].ToString());
        }
        sb.Append("] }");
        return sb.ToString();
    }
}

public sealed record class RoomData<T>(T Center, T North, T South, T West, T East)
{
    public RoomData<M> Map<M>(Func<T, M> F) => new(F(Center), F(North), F(South), F(West), F(East));
}

public readonly record struct Wall(WPos Position, float Depth);
