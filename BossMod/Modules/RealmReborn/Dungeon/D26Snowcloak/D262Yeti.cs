namespace BossMod.RealmReborn.Dungeon.D26Snowcloak.D262Yeti;

public enum OID : uint
{
    Boss = 0x3977, // R3.8
    Snowball = 0x3978, // R1.0-4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Buffet = 29585, // Boss->self, 3.0s cast, range 12 120-degree cone
    Northerlies = 29582, // Boss->self, 5.0s cast, range 80 circle
    FrozenMist = 29583, // Boss->self, no cast, single-target
    Updrift = 29584, // Boss->self, 4.0s cast, range 80 circle
    SmallSnowballVisual = 29588, // Snowball->self, no cast, single-target
    BigSnowballVisual = 29587, // Snowball->self, no cast, single-target
    HeavySnow = 29589, // Helper->self, 6.5s cast, range 15 circle
    LightSnow = 29590, // Helper->self, 7.0s cast, range 2 circle

    Spin = 29586, // Boss->self, 5.0s cast, range 11 circle
    FrozenCircle = 29591, // Helper->self, 5.0s cast, range 10-40 donut

    FrozenSpikeVisual = 25583, // Boss->self, 4.5+0,5s cast, single-target
    FrozenSpike = 29592 // Helper->player, 5.0s cast, range 6 circle
}

sealed class FrozenSpike(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FrozenSpike, 5f);
sealed class HeavySnow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySnow, 15f);
sealed class LightSnow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightSnow, 2f);
sealed class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeCone(12f, 60f.Degrees()));

sealed class SpinFrozenCircle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(11f), new AOEShapeDonut(10f, 40f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Spin)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Spin => 0,
                (uint)AID.FrozenCircle => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(3d));
        }
    }
}

sealed class Northerlies(BossModule module) : Components.RaidwideCast(module, (uint)AID.Northerlies);

sealed class D262YetiStates : StateMachineBuilder
{
    public D262YetiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrozenSpike>()
            .ActivateOnEnter<HeavySnow>()
            .ActivateOnEnter<LightSnow>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<SpinFrozenCircle>()
            .ActivateOnEnter<Northerlies>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 27, NameID = 3040, SortOrder = 3)]
public sealed class D262Yeti(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom([new(-98.24f, -135.15f), new(-93.57f, -134.53f), new(-88.72f, -132.58f),
    new(-84.62f, -129.45f), new(-81.51f, -125.42f),
    new(-79.55f, -120.71f), new(-78.84f, -115.47f), new(-78.44f, -115.05f), new(-78.5f, -114.39f), new(-82.38f, -104.18f),
    new(-82.93f, -103.86f), new(-84.44f, -101.95f), new(-84.91f, -101.5f), new(-88.37f, -98.85f), new(-88.92f, -98.51f),
    new(-92.98f, -96.83f), new(-93.61f, -96.64f), new(-98.08f, -96.05f), new(-98.74f, -96.04f), new(-100.64f, -96.29f),
    new(-101.90f, -96.28f), new(-113.24f, -103.09f), new(-113.77f, -103.51f), new(-114.06f, -104.03f), new(-115.22f, -105.55f),
    new(-115.55f, -106.11f), new(-117.24f, -110.21f), new(-117.41f, -110.84f), new(-118.00f, -115.27f), new(-117.99f, -115.93f),
    new(-117.41f, -120.35f), new(-117.24f, -120.97f), new(-115.54f, -125.09f), new(-115.22f, -125.64f), new(-112.53f, -129.14f),
    new(-112.08f, -129.61f), new(-108.50f, -132.36f), new(-107.92f, -132.69f), new(-103.82f, -134.38f), new(-103.16f, -134.56f),
    new(-98.69f, -135.15f)])]);
}
