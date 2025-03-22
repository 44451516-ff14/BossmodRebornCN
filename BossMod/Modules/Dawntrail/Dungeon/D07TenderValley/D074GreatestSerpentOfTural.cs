namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D074GreatestSerpentOfTural;

public enum OID : uint
{
    Boss = 0x4164, // R4.5
    LesserSerpentOfTural = 0x41DE, // R2.812
    GreatSerpentOfTural = 0x41E0, // R1.152-3.84
    SludgeVoidzone1 = 0x1EBA86, // R0.5
    SludgeVoidzone2 = 0x1EBA87, // R0.5
    SludgeVoidzone3 = 0x1EBA88, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 36747, // Boss->location, no cast, single-target

    DubiousTulidisaster = 36748, // Boss->self, 5.0s cast, range 40 circle

    BouncyCouncil = 36746, // Boss->self, 3.0s cast, single-target, spawns clones

    MisplacedMystery = 36750, // LesserSerpentOfTural->self, 7.0s cast, range 52 width 5 rect
    ExaltedWobble = 36749, // LesserSerpentOfTural->self, 7.0s cast, range 9 circle

    ScreesOfFuryVisual = 36744, // Boss->self, 4.5+0.5s cast, single-target, AOE tankbuster
    ScreesOfFury = 36757, // Helper->player, no cast, range 3 circle 

    GreatestLabyrinth = 36745, // Boss->self, 4.0s cast, range 40 circle

    MoistSummoning = 36743, // Boss->self, 3.0s cast, single-target, spawns great serpent of tural
    MightyBlorpVisual1 = 36753, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual2 = 36752, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorpVisual3 = 36751, // GreatSerpentOfTural->self, 4.5+0.5s cast, single-target, stack
    MightyBlorp1 = 39983, // GreatSerpentOfTural->players, no cast, range 6 circle
    MightyBlorp2 = 39982, // GreatSerpentOfTural->players, no cast, range 5 circle
    MightyBlorp3 = 39981, // GreatSerpentOfTural->players, no cast, range 4 circle

    GreatestFloodVisual = 36742, // Boss->self, 5.0s cast, single-target
    GreatestFlood = 36756, // Helper->self, 6.0s cast, range 40 circle, knockback 15, away from source

    GreatTorrentVisual = 36741, // Boss->self, 3.0s cast, single-target
    GreatTorrentAOE = 36754, // Helper->location, 6.0s cast, range 6 circle 
    GreatTorrentSpread = 36755 // Helper->player, no cast, range 6 circle
}

public enum IconID : uint
{
    Tankbuster = 341, // player
    LabyrinthFail = 504, // player
    LabyrinthSuccess = 503, // player
    Stackmarker1 = 62, // player
    Stackmarker2 = 542, // player
    Stackmarker3 = 543, // player
    Spreadmarker = 139 // player
}

class DubiousTulidisasterArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom square = new([new Square(D074GreatestSerpentOfTural.ArenaCenter, 15f)], [new Square(D074GreatestSerpentOfTural.ArenaCenter, 12f)]);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DubiousTulidisaster && Arena.Bounds == D074GreatestSerpentOfTural.StartingBounds)
            _aoe = new(square, Arena.Center, default, Module.CastFinishAt(spell, 4.8f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Arena.Bounds = D074GreatestSerpentOfTural.DefaultBounds;
            _aoe = null;
        }
    }
}

class ScreesOfFury(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(3f), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.ScreesOfFury), 5.3f, true, tankbuster: true);

class GreatestFlood(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.GreatestFlood), 15f)
{
    private static readonly Angle a45 = 45f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(source.Position, 4f, source.Rotation, a45), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class GreatestLabyrinth(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly Square middle = new(D074GreatestSerpentOfTural.ArenaCenter, 4f);
    private const float Radius = 2f;

    private static readonly (Square correctTile, Square goalTile)[] tilePairs = [
        (new(new(-124f, -552f), Radius), new(new(-140f, -564f), Radius)),
        (new(new(-128f, -560f), Radius), new(new(-120f, -544f), Radius)),
        (new(new(-132f, -548f), Radius), new(new(-120f, -564f), Radius)),
        (new(new(-136f, -556f), Radius), new(new(-140f, -544f), Radius))];

    private static readonly Shape[] wholeArena = [new Square(D074GreatestSerpentOfTural.ArenaCenter, 12f)];

    private static readonly AOEShapeCustom[] forbiddenShapes = InitializeShapes();
    private static readonly AOEShapeCustom[] safeShapes = InitializeShapes(true);

    private static AOEShapeCustom[] InitializeShapes(bool invert = false)
    {
        var shapes = new AOEShapeCustom[4];
        for (var i = 0; i < 4; ++i)
        {
            var tp = tilePairs[i];
            shapes[i] = !invert ? new AOEShapeCustom(wholeArena, [middle, tp.correctTile, tp.goalTile]) : new AOEShapeCustom([tp.correctTile, tp.goalTile], InvertForbiddenZone: true);
        }
        return shapes;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 0x01)
            return;

        void AddAOEs(int index)
        {
            _aoes.Add(new(forbiddenShapes[index], Arena.Center));
            _aoes.Add(new(safeShapes[index], Arena.Center, default, WorldState.FutureTime(10d), Colors.SafeFromAOE));
        }
        switch (state)
        {
            case 0x01000080:
                AddAOEs(0);
                break;
            case 0x04000200:
                AddAOEs(1);
                break;
            case 0x10000800:
                AddAOEs(2);
                break;
            case 0x00020001:
                AddAOEs(3);
                break;
            case 0x00100004 or 0x00200004 or 0x00400004 or 0x00080004:
                _aoes.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoes.Count == 0)
            return;
        hints.Add("Walk onto safe square!", !_aoes[1].Check(actor.Position));
    }
}

class MightyBlorp(BossModule module, uint iconID, AID aid, float radius) : Components.StackWithIcon(module, iconID, ActionID.MakeSpell(aid), radius, 4.6f, 4, 4);
class MightyBlorp1(BossModule module) : MightyBlorp(module, (uint)IconID.Stackmarker1, AID.MightyBlorp1, 6f);
class MightyBlorp2(BossModule module) : MightyBlorp(module, (uint)IconID.Stackmarker2, AID.MightyBlorp2, 5f);
class MightyBlorp3(BossModule module) : MightyBlorp(module, (uint)IconID.Stackmarker3, AID.MightyBlorp3, 4f);

class SludgeVoidzone(BossModule module, float radius, uint oid) : Components.Voidzone(module, radius, m => GetVoidzones(m, oid))
{
    private static Actor[] GetVoidzones(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        if (enemies.Count != 0 && enemies[0].EventState != 7)
            return [enemies[0]];
        return [];
    }
}

class SludgeVoidzone1(BossModule module) : SludgeVoidzone(module, 6f, (uint)OID.SludgeVoidzone1);
class SludgeVoidzone2(BossModule module) : SludgeVoidzone(module, 5f, (uint)OID.SludgeVoidzone2);
class SludgeVoidzone3(BossModule module) : SludgeVoidzone(module, 4f, (uint)OID.SludgeVoidzone3);

class DubiousTulidisaster(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DubiousTulidisaster));
class GreatestLabyrinthRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatestLabyrinth));
class GreatestFloodRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GreatestFlood));
class ExaltedWobble(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ExaltedWobble), 9f);
class MisplacedMystery(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MisplacedMystery), new AOEShapeRect(52f, 2.5f));
class GreatTorrent(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.GreatTorrentAOE), 6f, 10);
class GreatTorrentSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.GreatTorrentSpread), 6f, 5.1f);

class D074GreatestSerpentOfTuralStates : StateMachineBuilder
{
    public D074GreatestSerpentOfTuralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DubiousTulidisasterArenaChange>()
            .ActivateOnEnter<DubiousTulidisaster>()
            .ActivateOnEnter<ScreesOfFury>()
            .ActivateOnEnter<MightyBlorp1>()
            .ActivateOnEnter<MightyBlorp2>()
            .ActivateOnEnter<MightyBlorp3>()
            .ActivateOnEnter<SludgeVoidzone1>()
            .ActivateOnEnter<SludgeVoidzone2>()
            .ActivateOnEnter<SludgeVoidzone3>()
            .ActivateOnEnter<GreatestFlood>()
            .ActivateOnEnter<GreatestFloodRaidwide>()
            .ActivateOnEnter<GreatestLabyrinth>()
            .ActivateOnEnter<GreatestLabyrinthRaidwide>()
            .ActivateOnEnter<ExaltedWobble>()
            .ActivateOnEnter<MisplacedMystery>()
            .ActivateOnEnter<GreatTorrent>()
            .ActivateOnEnter<GreatTorrentSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12709)]
public class D074GreatestSerpentOfTural(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-130f, -554f);
    public static readonly ArenaBoundsSquare StartingBounds = new(14.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(12f);
}
