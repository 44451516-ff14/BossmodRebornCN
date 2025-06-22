using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

public enum Direction
{
    North,
    South,
    East,
    West
}

// neat feature of deep dungeons - there is only one path from any room to any other room (no loops) and the grid is so small that brute forcing is basically free!
internal sealed class FloorPathfind(ReadOnlySpan<RoomFlags> Map)
{
    public readonly RoomFlags[] Map = Map.ToArray();

    private readonly bool[] Explored = new bool[25];

    private readonly Queue<List<int>> Queue = [];

    public List<int> Pathfind(int startRoom, int destRoom)
    {
        if (startRoom == destRoom)
            return [];

        Explored[startRoom] = true;
        Queue.Enqueue([startRoom]);
        while (Queue.TryDequeue(out var v))
        {
            var v1 = v[^1];
            if (v1 == destRoom)
            {
                v.RemoveAt(0);
                return v;
            }
            var edges = CollectionsMarshal.AsSpan(Edges(v1));
            var len = edges.Length;
            for (var i = 0; i < len; ++i)
            {
                var w = edges[i];
                if (!Explored[w])
                {
                    Explored[w] = true;
                    Queue.Enqueue([.. v, w]);
                }
            }
        }

        return [];
    }

    private List<int> Edges(int roomIndex)
    {
        var md = Map[roomIndex];
        var edges = new List<int>(4);
        if (md.HasFlag(RoomFlags.ConnectionN))
            edges.Add(roomIndex - 5);
        if (md.HasFlag(RoomFlags.ConnectionS))
            edges.Add(roomIndex + 5);
        if (md.HasFlag(RoomFlags.ConnectionW))
            edges.Add(roomIndex - 1);
        if (md.HasFlag(RoomFlags.ConnectionE))
            edges.Add(roomIndex + 1);
        return edges;
    }
}
