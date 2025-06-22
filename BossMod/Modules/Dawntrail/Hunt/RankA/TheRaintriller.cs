namespace BossMod.Dawntrail.Hunt.RankA.TheRaintriller;

public enum OID : uint
{
    Boss = 0x457F, // R4.800, x1
}

public enum AID : uint
{
    // Do Re Misery has multiple AIDs with variant casts, 1 -> One Yell, 2 -> 2 Yells, 3 -> 3 Yells.
    // Croakdown/Ribbitygibbet/ChirpOfTheWisp are NOT what they correspond to in-game...
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 39804, // Boss->location, no cast, single-target (I think this corresponds to yells??)
    DoReMisery1 = 39758, // Boss->self, 5.0s cast, single-target
    Croakdown = 39750, // Boss->self, 1.0s cast, range 12 circle "Chirp!"
    DoReMisery2 = 39751, // Boss->self, 6.2s cast, single-target
    Ribbitygibbet = 39752, // Boss->self, 1.0s cast, range 10-40 donut "Ribbit!"
    DoReMisery3 = 39749, // Boss->self, 7.5s cast, single-target
    ChirpOTheWisp = 39753, // Boss->self, 1.0s cast, range 40 135-degree cone "Croak!"
    DropOfVenom = 39754, // Boss->player, 5.0s cast, range 6 circle
}

public enum NPCYell : ushort
{
    Chirp = 17815, // circle
    ChirpRibbit = 17719, // circle, donut
    RibbitChirpCroak = 17720, // donut, circle, cone
    ChirpRibbitCroak = 17721, // circle, donut, cone
    CroakRibbitChirp = 17722, // cone, donut, circle
    RibbitCroakChirp = 17723, // donut, cone, circle
    CroakChirpRibbit = 17724, // cone, circle, donut
    ChirpCroakRibbit = 17725, // circle, cone, donut
    ChirpCroakChirp = 17734, // circle, cone, circle
    RibbitRibbitChirp = 17793 // donut, donut, circle
}

sealed class DoReMisery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private AOEShape[] shapes = [];
    private static readonly AOEShapeCircle circle = new(12f);
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCone cone = new(40f, 135f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else if (aoe.Shape != cone)
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = _aoes.Count;
        if (count > 0)
        {
            var sb = new StringBuilder(18);
            for (var i = 0; i < count; i++)
            {
                var shapeHint = _aoes[i].Shape switch
                {
                    AOEShapeCircle => "Out",
                    AOEShapeDonut => "In",
                    AOEShapeCone => "Back",
                    _ => ""
                };
                sb.Append(shapeHint);

                if (i < count - 1)
                    sb.Append(" -> ");
            }
            hints.Add(sb.ToString());
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DoReMisery1 or (uint)AID.DoReMisery2 or (uint)AID.DoReMisery3)
        {
            var len = shapes.Length;
            for (var i = 0; i < len; ++i)
                AddAOE(shapes[i], i * 3.2f);
            shapes = [];
            void AddAOE(AOEShape shape, float delay)
            => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 1.2f + delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Croakdown or (uint)AID.ChirpOTheWisp or (uint)AID.Ribbitygibbet)
            _aoes.RemoveAt(0);
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        shapes = id switch
        {
            (ushort)NPCYell.Chirp => [circle],
            (ushort)NPCYell.ChirpRibbit => [circle, donut],
            (ushort)NPCYell.ChirpCroakRibbit => [circle, cone, donut],
            (ushort)NPCYell.ChirpRibbitCroak => [circle, donut, cone],
            (ushort)NPCYell.ChirpCroakChirp => [circle, cone, circle],
            (ushort)NPCYell.RibbitChirpCroak => [donut, circle, cone],
            (ushort)NPCYell.RibbitCroakChirp => [donut, cone, circle],
            (ushort)NPCYell.RibbitRibbitChirp => [donut, donut, circle],
            (ushort)NPCYell.CroakRibbitChirp => [cone, donut, circle],
            (ushort)NPCYell.CroakChirpRibbit => [cone, circle, donut],
            _ => []
        };
    }
}

sealed class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DropOfVenom, 6f);

sealed class TheRaintrillerStates : StateMachineBuilder
{
    public TheRaintrillerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoReMisery>()
            .ActivateOnEnter<DropOfVenom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13442)]
public sealed class TheRaintriller(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
