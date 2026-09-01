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

sealed class SeedOfTheRivers(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedOfTheRivers, 5f);
sealed class Sanctification(BossModule module) : Components.Cleave(module, (uint)AID.Sanctification, new AOEShapeCone(16.5f, 60f.Degrees()), activeWhileCasting: false);

sealed class Educator(BossModule module) : Components.GenericAOEs(module)
{
    private readonly WPos center = new(0f, -350f);
    private readonly AOEShapeRect square = new(5f, 5f, 5f);
    private BitMask activeCells;
    private readonly List<AOEInstance> _aoes = [with(16)];

    public int CellIndex(WPos pos)
    {
        var off = pos - center;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private int CoordinateIndex(float coord) => coord switch
    {
        < -10f => 0,
        < 0f => 1,
        < 10f => 2,
        _ => 3
    };

    public WPos CellCenter(int index)
    {
        var x = -15f + 10f * (index & 3);
        var z = -15f + 10f * (index >> 2);
        return center + new WDir(x, z);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.TheEducator)
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
        else if (id == (uint)AID.TheEducatorBootSequence)
        {
            _aoes.Add(new(square, CellCenter(CellIndex(spell.LocXZ)), default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheEducatorBootSequence)
        {
            var count = _aoes.Count;
            var loc = spell.LocXZ;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Origin == CellCenter(CellIndex(loc)))
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
        {
            return;
        }
        activeCells = default;
        Arena.Bounds = new ArenaBoundsSquare(19.5f);
        Arena.Center = center;
    }

    private void UpdateArenaBounds()
    {
        var brokenTiles = new Square[activeCells.NumSetBits()];
        var index = 0;
        for (var i = 0; i < 16; ++i)
        {
            if (activeCells[i])
            {
                brokenTiles[index++] = new Square(CellCenter(i), 5f);
            }
        }

        if (brokenTiles.Length == 16) // prevents empty sequence incase all tiles are active
        {
            brokenTiles = [];
        }
        var arena = new ArenaBoundsCustom([new Square(center, 19.5f)], brokenTiles);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}

sealed class AetherochemicalMine(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circle = new(5f);
    private readonly List<AOEInstance> _aoes = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.AetherochemicalMine)
        {
            _aoes.Add(new(circle, caster.Position));
        }
        else if (id == (uint)AID.AetherochemicalExplosionMine)
        {
            RemoveAOE(caster.Position);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherochemicalMine)
        {
            RemoveAOE(actor.Position);
        }
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

sealed class AetherochemicalBombStatus(BossModule module) : Components.CleansableDebuff(module, (uint)SID.AetherochemicalBomb, "Bomb", "targeted");

sealed class AetherochemicalBomb(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AetherochemicalBomb)
        {
            Spreads.Add(new(actor, 8f, WorldState.FutureTime(6d))); // status effect hits every 6 seconds unless cleansed/times out, radius is either 7 or 8
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AetherochemicalBomb)
        {
            Spreads.Clear();
        }
    }
}

sealed class D083TheCuratorStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35u, NameID = 3434u, SortOrder = 9)]
public sealed class D083TheCurator(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -350f), new ArenaBoundsSquare(19.5f))
{
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
