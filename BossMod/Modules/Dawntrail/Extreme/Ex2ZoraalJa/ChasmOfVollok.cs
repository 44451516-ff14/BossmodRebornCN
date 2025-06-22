namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class ChasmOfVollokFangSmall(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokFangSmallAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    private const float platformOffset = 21.2132f;
    private static readonly AOEShapeRect _shape = new(5f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChasmOfVollokFangSmall)
        {
            // the visual cast happens on one of the side platforms at intercardinals, offset by 30
            var pos = spell.LocXZ;
            var offset = new WDir(pos.X > Arena.Center.X ? -platformOffset : +platformOffset, pos.Z > Arena.Center.Z ? -platformOffset : +platformOffset);
            AOEs.Add(new(_shape, WPos.ClampToGrid(pos + offset), spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

// note: we can start showing aoes earlier, right when fang actors spawn
sealed class ChasmOfVollokFangLarge(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokFangLargeAOE)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeRect _shape = new(10, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VollokLargeAOE)
        {
            AOEs.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            var pos = spell.LocXZ;
            var mainOffset = Trial.T02ZoraalJa.ZoraalJa.ArenaCenter - Arena.Center;
            var fangOffset = pos - Arena.Center;
            var mirrorOffset = fangOffset.Dot(mainOffset) > 0 ? -2 * mainOffset : 2 * mainOffset;
            AOEs.Add(new(_shape, WPos.ClampToGrid(pos + mirrorOffset), spell.Rotation, Module.CastFinishAt(spell)));
        }
    }
}

sealed class ChasmOfVollokPlayer(BossModule module) : Components.GenericAOEs(module, (uint)AID.ChasmOfVollokPlayer, "GTFO from occupied cell!")
{
    public bool Active;
    private readonly List<Actor> _targets = new(8);
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(2.5f, 2.5f, 2.5f);
    private static readonly WDir _localX = (-135f).Degrees().ToDirection();
    private static readonly WDir _localZ = 135f.Degrees().ToDirection();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            return [];
        var aoes = new List<AOEInstance>();
        var platformOffset = 2f * (Arena.Center - Trial.T02ZoraalJa.ZoraalJa.ArenaCenter);
        var count = _targets.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = _targets[i];
            if (t == actor)
                continue;
            var playerOffset = t.Position - Trial.T02ZoraalJa.ZoraalJa.ArenaCenter;
            var playerX = _localX.Dot(playerOffset);
            var playerZ = _localZ.Dot(playerOffset);
            if (Math.Abs(playerX) >= 15f || Math.Abs(playerZ) >= 15f)
            {
                playerOffset -= platformOffset;
                playerX = _localX.Dot(playerOffset);
                playerZ = _localZ.Dot(playerOffset);
            }
            var cellX = CoordinateToCell(playerX);
            var cellZ = CoordinateToCell(playerZ);
            var cellCenter = Trial.T02ZoraalJa.ZoraalJa.ArenaCenter + _localX * CellCenterCoordinate(cellX) + _localZ * CellCenterCoordinate(cellZ);

            aoes.Add(new(_shape, cellCenter, 45f.Degrees(), _activation));
            if (platformOffset != default)
                aoes.Add(new(_shape, cellCenter + platformOffset, 45f.Degrees(), _activation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void Update()
    {
        // assume that if player dies, he won't participate in the mechanic
        var count = _targets.Count;
        if (count == 0)
            return;
        for (var i = count - 1; i >= 0; --i)
        {
            if (_targets[i].IsDead)
                _targets.RemoveAt(i);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChasmOfVollok)
        {
            _targets.Add(actor);
            _activation = WorldState.FutureTime(6.1d);
        }
    }

    private static int CoordinateToCell(float x) => x switch
    {
        < -5f => 0,
        < 0f => 1,
        < 5f => 2,
        _ => 3
    };

    private static float CellCenterCoordinate(int c) => -7.5f + c * 5f;
}
