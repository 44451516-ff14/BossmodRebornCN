namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D021AencThon;

public enum OID : uint
{
    Boss = 0x3F2, // R=2.0
    GeyserHelper1 = 0x1EAAA1, // controls animations for 2 geysers
    GeyserHelper2 = 0x1EAAA2, // controls animations for 3 geysers
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    CandyCane = 8857, // Boss->player, 4.0s cast, single-target
    HydrofallVisual = 8871, // Boss->self, 3.0s cast, single-target
    Hydrofall = 8893, // Helper->location, 3.0s cast, range 6 circle
    LaughingLeapAOE = 8852, // Boss->location, 4.0s cast, range 4 circle
    LaughingLeapStack = 8840, // Boss->players, no cast, range 4 circle
    LandsbloodFirst = 7822, // Boss->self, 3.0s cast, range 40 circle
    LandsbloodRepeat = 7899, // Boss->self, no cast, range 40 circle
    Geyser = 8800 // Helper->self, no cast, range 6 circle
}

public enum IconID : uint
{
    Stackmarker = 62 // player
}

class Landsblood(BossModule module) : Components.RaidwideCast(module, (uint)AID.LandsbloodFirst, "Raidwides + Geysers");
class CandyCane(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CandyCane);
class Hydrofall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrofall, 6f);
class LaughingLeap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaughingLeapAOE, 4f);
class LaughingLeapStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.LaughingLeapStack, 4f, 5.1f, 4, 4);

class Geyser(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = new(14);

    private readonly WDir[] geysers1 = [new(-9f, 15f), new(default, -16f)];
    private readonly WDir[] geysers2 = [new(-9f, -15f), new(default, 5f), new(7f, -7f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);
        var isNotLastSet = aoes[^1].Activation > deadline;
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation < deadline)
            {
                if (isNotLastSet)
                    aoe.Color = color;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020u)
        {
            var positions = actor.OID switch
            {
                (uint)OID.GeyserHelper1 => geysers1,
                (uint)OID.GeyserHelper2 => geysers2,
                _ => []
            };
            var len = positions.Length;
            var rot = actor.Rotation;
            var origin = actor.Position;
            var activation = WorldState.FutureTime(5.1d);
            for (var i = 0; i < len; ++i)
            {
                var pos = positions[i].Rotate(rot) + origin;
                _aoes.Add(new(circle, pos.Quantized(), default, activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Geyser)
            _aoes.RemoveAt(0);
    }
}

class D021AencThonStates : StateMachineBuilder
{
    public D021AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CandyCane>()
            .ActivateOnEnter<Landsblood>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<LaughingLeap>()
            .ActivateOnEnter<LaughingLeapStack>()
            .ActivateOnEnter<Geyser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8141)]
public class D021AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(default, 30f), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(default, 50f), 20f, 1f), new Rectangle(new(default, 10f), 20f, 1.4f)]);
}
