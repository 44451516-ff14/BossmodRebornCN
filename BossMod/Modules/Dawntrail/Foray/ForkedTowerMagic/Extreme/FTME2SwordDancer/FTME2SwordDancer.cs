namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME2SwordDancer;

[SkipLocalsInit]
sealed class SwordStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.SwordStorm);
[SkipLocalsInit]
sealed class Rush(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.Rush1, (uint)AID.Rush2, (uint)AID.Rush3, (uint)AID.Rush4], 3.5f, 2, 2);
[SkipLocalsInit]
sealed class RushLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RushLong, new AOEShapeRect(30f, 3f), 8);
[SkipLocalsInit]
sealed class TurnInner(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Turn1], new AOEShapeDonutSector(9f, 14f, 45f.Degrees()));
[SkipLocalsInit]
sealed class TurnOuter(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Turn2, (uint)AID.Turnabout2], new AOEShapeDonutSector(19f, 24f, 45f.Degrees()));
sealed class TurnMiddle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Turn3, (uint)AID.Turn4, (uint)AID.Turnabout1], new AOEShapeDonutSector(14f, 19f, 45f.Degrees()));
[SkipLocalsInit]
sealed class MartialMystique(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MartialMystique, new AOEShapeRect(48f, 48f));
[SkipLocalsInit]
sealed class Pierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);
[SkipLocalsInit]
sealed class SwordDance(BossModule module) : Components.GenericAOEs(module)
{
    // do sword markers rotate like in normal mode or can it be different?
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        ref var aoe = ref aoes[0];
        aoe.Color = Colors.Danger;

        return aoes[..max];
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.SwordDanceMarker && state == 0x00010002)
        {
            // 8.8s between 1st mark and 1st cast
            // 1s between eanims, 2.4s-ish between actual cast
            var count = _aoes.Count;
            var act = WorldState.FutureTime(8.8d + 2.4d * count);
            _aoes.Add(new(new AOEShapeRect(30f, 10f, 30f), actor.Position, actor.Rotation, act));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.SwordDance)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(FTME2SwordDancerStates),
    ConfigType = null, // replace null with typeof(FTME2SwordDancerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.SwordDancer,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.TheForkedTowerMagic,
    GroupID = 1114u,
    NameID = 14820u,
    SortOrder = 2,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME2SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, 704f), new ArenaBoundsCircle(24f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 24f);
}
