namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D083TheCurator;

public enum OID : uint
{
    Boss = 0x1018, // R4.5
    AetherochemicalMine = 0x101A, // R1.0
    ClockworkAlarum = 0x1019, // R2.25
    Helper = 0xD25
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Sanctification = 3977, // Boss->self, no cast, range 12+R 120-degree cone
    Unholy = 3978, // Boss->self, no cast, range 80+R circle
    AetherochemicalExplosive = 3979, // Boss->self, 3.0s cast, ???, apply Aetherochemical Bomb status (should be cleansed)
    AetherochemicalExplosionStatus = 3980, // Helper->location, no cast, ???
    BrokenGlass = 3982, // Helper->self, no cast, ???
    TheEducator = 3981, // Boss->self, 6.0s cast, ???
    AetherochemicalMine = 3983, // Helper->self, no cast, single-target
    AetherochemicalExplosionMine = 3984, // AetherochemicalMine->self, no cast, range 12 circle, knockback 10, away from source
    TheEducatorBootSequence = 3986, // ClockworkAlarum->self, 3.0s cast, single-target
    SeedOfTheRivers = 3985, // Helper->location, 3.0s cast, range 5 circle
}

public enum SID : uint
{
    AetherochemicalBomb = 723, // none->player, extra=0x0
}

class SeedOfTheRivers(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedOfTheRivers, 5f);
class Sanctification(BossModule module) : Components.Cleave(module, (uint)AID.Sanctification, new AOEShapeCone(16.5f, 60f.Degrees()), activeWhileCasting: false);

class Educator(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect square = new(5f, 5f, 5f);
    private BitMask activeCells;
    private static readonly Square[] defaultSquare = [new(D083TheCurator.ArenaCenter, 19.5f)];
    public static readonly Square[] Tiles = GenerateTiles();
    private readonly List<AOEInstance> _aoes = new(16);

    private static Square[] GenerateTiles()
    {
        var squares = new Square[16];
        for (var i = 0; i < 16; ++i)
            squares[i] = new Square(CellCenter(i), 5f);
        return squares;
    }

    public static int CellIndex(WPos pos)
    {
        var off = pos - D083TheCurator.ArenaCenter;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private static int CoordinateIndex(float coord) => coord switch
    {
        < -10f => 0,
        < 0f => 1,
        < 10f => 2,
        _ => 3
    };

    public static WPos CellCenter(int index)
    {
        var x = -15f + 10f * (index & 3);
        var z = -15f + 10f * (index >> 2);
        return D083TheCurator.ArenaCenter + new WDir(x, z);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheEducator)
        {
            var centerIndex = CellIndex(spell.LocXZ);
            var rowStart = centerIndex & 3;
            var columnStart = centerIndex >> 2;

            for (var i = 0; i < 4; ++i)
            {
                activeCells[(columnStart << 2) | i] = true;
                activeCells[(i << 2) | rowStart] = true;
            }
            UpdateArenaBounds();
        }
        else if (spell.Action.ID == (uint)AID.TheEducatorBootSequence)
        {
            _aoes.Add(new(square, CellCenter(CellIndex(spell.LocXZ)), default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheEducatorBootSequence)
        {
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin == CellCenter(CellIndex(spell.LocXZ)))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TheEducatorBootSequence)
        {
            var index = CellIndex(caster.Position);
            activeCells[index] = true;
            UpdateArenaBounds();
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000004 || activeCells == default)
            return;
        activeCells = default;
        Arena.Bounds = D083TheCurator.DefaultArena;
        Arena.Center = D083TheCurator.ArenaCenter;
    }

    private void UpdateArenaBounds()
    {
        List<Square> brokenTilesList = [];

        var len = Tiles.Length;
        for (var i = 0; i < len; ++i)
        {
            if (activeCells[i])
                brokenTilesList.Add(Tiles[i]);
        }

        Square[] brokenTiles = [.. brokenTilesList];
        if (brokenTiles.Length == 16) // prevents empty sequence incase all tiles are active
            brokenTiles = [];
        var arena = new ArenaBoundsCustom(defaultSquare, brokenTiles);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}

class AetherochemicalMine(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f);
    private readonly List<AOEInstance> _aoes = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AetherochemicalMine)
            _aoes.Add(new(circle, caster.Position));
        else if (spell.Action.ID == (uint)AID.AetherochemicalExplosionMine)
            RemoveAOE(caster.Position);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherochemicalMine)
            RemoveAOE(actor.Position);
    }

    private void RemoveAOE(WPos pos)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].Origin == pos)
            {
                _aoes.RemoveAt(i);
                break;
            }
        }
    }
}

class AetherochemicalBombStatus(BossModule module) : Components.CleansableDebuff(module, (uint)SID.AetherochemicalBomb, "Bomb", "targeted");

class AetherochemicalBomb(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AetherochemicalBomb)
            Spreads.Add(new(actor, 8f, WorldState.FutureTime(6d))); // status effect hits every 6 seconds unless cleansed/times out, radius is either 7 or 8
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AetherochemicalBomb)
            Spreads.Clear();
    }
}

class D083TheCuratorStates : StateMachineBuilder
{
    public D083TheCuratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SeedOfTheRivers>()
            .ActivateOnEnter<Educator>()
            .ActivateOnEnter<Sanctification>()
            .ActivateOnEnter<AetherochemicalMine>()
            .ActivateOnEnter<AetherochemicalBombStatus>()
            .ActivateOnEnter<AetherochemicalBomb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 3434, SortOrder = 9)]
public class D083TheCurator(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(default, -350f);
    public static readonly ArenaBoundsSquare DefaultArena = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ClockworkAlarum));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ClockworkAlarum => 1,
                _ => 0
            };
        }
    }
}
