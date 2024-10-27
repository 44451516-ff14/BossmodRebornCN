namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class Besiegement(BossModule module) : Components.GenericAOEs(module)
{
    private const int Z = 80;
    private const int L = 60;
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeRect[] rects = [new(L, 2), new(L, 4), new(L, 5), new(L, 6), new(L, 9)];
    private static readonly HashSet<AID> casts = [AID.Besiegement1, AID.Besiegement2, AID.Besiegement3, AID.Besiegement4,
    AID.Besiegement5];
    private static readonly Angle angle = -0.003f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    private void States(uint state, bool check)
    {
        var aoeMap = new Dictionary<uint, (int, int)[]>()
        {
            [0x00200010] = check ? [(3, 114), (1, 90), (2, 101), (0, 82)] : [(1, 116), (1, 94), (2, 105), (1, 84)],
            [0x02000100] = check ? [(2, 95), (4, 111), (1, 84)] : [],
            [0x08000400] = check ? [(1, 116), (4, 101), (2, 85)] : [],
            [0x00800040] = check ? [(1, 116), (2, 105), (1, 94), (1, 84)] : [(3, 114), (2, 101), (1, 90), (0, 82)]
        };

        if (aoeMap.TryGetValue(state, out var aoes))
        {
            foreach (var (index, x) in aoes)
                AOEs.Add(new(rects[index], new WPos(x, Z), angle, WorldState.FutureTime(6.6f)));
            ++NumCasts;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x03)
            return;
        States(state, NumCasts != 1);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (casts.Contains((AID)spell.Action.ID))
            AOEs.Clear();
    }
}
