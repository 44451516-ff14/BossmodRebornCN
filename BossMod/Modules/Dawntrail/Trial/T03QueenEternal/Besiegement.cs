namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class Besiegement(BossModule module) : Components.GenericAOEs(module)
{
    private const float L = 60f;
    public readonly List<AOEInstance> AOEs = new(4);
    private static readonly AOEShapeRect[] rects = [new(L, 2f), new(L, 4f), new(L, 5f), new(L, 6f), new(L, 9f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    private void States(uint state, bool check)
    {
        var aoeMap = new Dictionary<uint, (int, float)[]>()
        {
            [0x00200010] = check ? [(3, 114f), (1, 90f), (2, 101f), (0, 82f)] : [(1, 116f), (1, 94f), (2, 105f), (1, 84f)],
            [0x02000100] = check ? [(2, 95f), (4, 111f), (1, 84f)] : [],
            [0x08000400] = check ? [(1, 116f), (4, 101f), (2, 85f)] : [],
            [0x00800040] = check ? [(1, 116f), (2, 105f), (1, 94f), (1, 84f)] : [(3, 114f), (2, 101f), (1, 90f), (0, 82f)]
        };

        if (aoeMap.TryGetValue(state, out var aoes))
        {
            foreach (var (index, x) in aoes)
                AOEs.Add(new(rects[index], new WPos(x, 80f), Angle.AnglesCardinals[1], WorldState.FutureTime(6.6d)));
            ++NumCasts;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x03)
        {
            return;
        }
        States(state, NumCasts != 1);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Besiegement1:
                case (uint)AID.Besiegement2:
                case (uint)AID.Besiegement3:
                case (uint)AID.Besiegement4:
                case (uint)AID.Besiegement5:
                    AOEs.Clear();
                    break;
            }
        }
    }
}
