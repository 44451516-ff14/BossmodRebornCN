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

class FrozenSpike(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FrozenSpike, 5);
class HeavySnow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySnow, 15);
class LightSnow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightSnow, 2);
class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeCone(12, 60.Degrees()));

class SpinFrozenCircle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(11), new AOEShapeDonut(10, 40)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Spin)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.Spin => 0,
                AID.FrozenCircle => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(3));
        }
    }
}

class Northerlies(BossModule module) : Components.RaidwideCast(module, (uint)AID.Northerlies);

class D262YetiStates : StateMachineBuilder
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
public class D262Yeti(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-93.88f, -134.57f), new(-93.31f, -134.48f), new(-88.91f, -132.66f), new(-88.47f, -132.41f), new(-84.85f, -129.63f),
    new(-84.41f, -129.2f), new(-81.76f, -125.74f), new(-81.45f, -125.31f), new(-79.64f, -120.94f), new(-79.49f, -120.38f),
    new(-78.85f, -115.5f), new(-78.45f, -115.08f), new(-78.48f, -114.5f), new(-82.26f, -104.43f), new(-82.6f, -103.92f),
    new(-83.07f, -103.72f), new(-84.41f, -101.97f), new(-84.82f, -101.56f), new(-88.37f, -98.83f), new(-88.85f, -98.53f),
    new(-92.93f, -96.85f), new(-93.46f, -96.65f), new(-98.26f, -96.02f), new(-98.85f, -96.04f), new(-100.54f, -96.27f),
    new(-101.13f, -96.33f), new(-101.69f, -96.15f), new(-113.46f, -103.21f), new(-115.39f, -105.75f), new(-117.29f, -110.32f),
    new(-117.42f, -110.91f), new(-118, -115.26f), new(-118.02f, -115.76f), new(-117.4f, -120.4f), new(-117.25f, -120.96f),
    new(-115.59f, -124.96f), new(-115.31f, -125.52f), new(-112.33f, -129.4f), new(-108.27f, -132.53f), new(-103.85f, -134.37f),
    new(-103.33f, -134.53f), new(-98.95f, -135.11f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
