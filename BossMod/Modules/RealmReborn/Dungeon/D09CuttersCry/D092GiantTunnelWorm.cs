namespace BossMod.RealmReborn.Dungeon.D09CuttersCry.D092GiantTunnelWorm;

public enum OID : uint
{
    GiantTunnelWorm = 0x48FF, // R4.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // GiantTunnelWorm->player, no cast, single-target

    Sandstorm = 44235, // GiantTunnelWorm->player, no cast, single-target
    SandCyclone = 44236, // GiantTunnelWorm->player, no cast, single-target, applies sludge
    SandPillar = 44232, // Helper->self, no cast, range 4 circle
    Earthbreak = 44233, // GiantTunnelWorm->self, no cast, range 10 circle
    BottomlessDesert = 44234, // Helper->self, no cast, range 65 circle, pull 32 between hitboxes, raidwide
}

public enum SID : uint
{
    Sludge = 270 // GiantTunnelWorm->player, extra=0x0

}

sealed class Sludge(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Sludge, "Sludge", "muddied");

sealed class SandPillarEarthbreak(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle circleSmall = new(4f), circleBig = new(10f);
    private readonly List<AOEInstance> _aoes = new(7);
    private static readonly (WPos initialPos, Angle rotation)[] positions =
    [
        (new(-135.623f, 147.871f), -45f.Degrees()),
        (new(-142.695f, 144.942f), default),
        (new(-149.766f, 162.013f), 135f.Degrees()),
        (new(-142.695f, 164.942f), 180f.Degrees()),
        (new(-152.695f, 154.942f), 90f.Degrees()),
        (new(-135.623f, 162.013f), -135f.Degrees()),
        (new(-149.766f, 147.871f), 45f.Degrees()),
        (new(-132.694f, 154.942f), -90f.Degrees()),
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        if (_aoes.Count <= 1)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe = ref aoes[0];
        aoe.Color = Colors.Danger;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SandPillar:
                var count = _aoes.Count;
                if (count == 0)
                {
                    var cPos = caster.Position;
                    for (var i = 0; i < 8; ++i)
                    {
                        ref readonly var pos = ref positions[i];
                        var initialPosition = pos.initialPos;
                        if (initialPosition.AlmostEqual(cPos, 1f))
                        {
                            var dir = 4f * pos.rotation.ToDirection();
                            for (var j = 1; j < 8; ++j)
                            {
                                _aoes.Add(new(j != 7 ? circleSmall : circleBig, (initialPosition + j * dir).Quantized(), default, WorldState.FutureTime(0.9d * j)));
                            }
                            return;
                        }
                    }
                }
                else
                {
                    _aoes.RemoveAt(0);
                }
                break;
            case (uint)AID.Earthbreak:
                _aoes.Clear();
                break;
            case (uint)AID.BottomlessDesert:
                _aoes.Add(new(circleBig, caster.Position.Quantized(), default, WorldState.FutureTime(3.1d)));
                break;
        }
    }
}

sealed class D092GiantTunnelWormStates : StateMachineBuilder
{
    public D092GiantTunnelWormStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Sludge>()
            .ActivateOnEnter<SandPillarEarthbreak>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.GiantTunnelWorm, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 12u, NameID = 1589u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.RealmReborn, SortOrder = 2)]
public sealed class D092GiantTunnelWorm(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom([new(-152.42f, 127.49f), new(-152.01f, 127.81f),
    new(-151.04f, 128.76f), new(-150.49f, 128.97f), new(-150.04f, 129.24f),
    new(-149.53f, 129.72f), new(-148.43f, 130.37f), new(-147.83f, 130.49f), new(-147.34f, 130.23f), new(-146.91f, 129.77f),
    new(-145.89f, 128.91f), new(-145.29f, 128.62f), new(-144.64f, 128.5f), new(-143.06f, 128.46f), new(-142.42f, 128.59f),
    new(-141.78f, 128.78f), new(-140.75f, 129.67f), new(-140.41f, 130.16f), new(-139.92f, 130.63f), new(-139.39f, 130.62f),
    new(-138.69f, 130.38f), new(-138.24f, 130.86f), new(-137.62f, 131.16f), new(-137.02f, 131.01f), new(-136.37f, 130.95f),
    new(-134.29f, 131.24f), new(-133.92f, 130.6f), new(-133.55f, 130.26f), new(-133.04f, 129.9f), new(-132.42f, 130.03f),
    new(-130.86f, 131.94f), new(-130.38f, 132.3f), new(-129.76f, 132.47f), new(-127.15f, 132.38f), new(-125.27f, 132.7f),
    new(-124.59f, 132.9f), new(-123.15f, 134.26f), new(-122.68f, 134.84f), new(-122.04f, 136.61f), new(-121.75f, 137.79f),
    new(-120.71f, 138.45f), new(-120.18f, 138.96f), new(-119.75f, 139.52f), new(-118.46f, 142.06f), new(-117.85f, 144.61f),
    new(-117.96f, 145.96f), new(-117.81f, 146.54f), new(-117.44f, 146.99f), new(-116.93f, 146.92f), new(-115.62f, 146.9f),
    new(-115.7f, 147.55f), new(-117.3f, 148.8f), new(-117.77f, 149.26f), new(-118.03f, 149.83f), new(-118.11f, 150.47f),
    new(-117.91f, 151.08f), new(-117.35f, 152.27f), new(-117.18f, 152.75f), new(-117.58f, 153.84f), new(-118.04f, 154.36f),
    new(-118.4f, 154.88f), new(-118.49f, 155.49f), new(-118.21f, 156.01f), new(-117.77f, 156.47f), new(-116.69f, 157.27f),
    new(-116.49f, 157.73f), new(-116.63f, 158.4f), new(-117.71f, 160.77f), new(-118.75f, 161.67f), new(-119.11f, 162.16f),
    new(-118.94f, 163.41f), new(-118.97f, 164.11f), new(-119.16f, 165.38f), new(-119.37f, 166.03f), new(-119.79f, 166.52f),
    new(-120.35f, 166.9f), new(-121.05f, 166.87f), new(-121.68f, 166.66f), new(-122.84f, 166.11f), new(-123.41f, 165.99f),
    new(-127.02f, 167.29f), new(-128.27f, 168.87f), new(-128.58f, 169.4f), new(-128.34f, 171.92f), new(-128.41f, 172.55f),
    new(-128.61f, 173.19f), new(-128.88f, 173.8f), new(-129.14f, 174.23f), new(-130.47f, 174.31f), new(-131.06f, 174.52f),
    new(-131.14f, 175.05f), new(-130.84f, 175.64f), new(-130.79f, 176.3f), new(-132.03f, 178.66f), new(-132.52f, 179.15f),
    new(-133.08f, 179.57f), new(-133.51f, 180.05f), new(-134.02f, 180.51f), new(-135.19f, 181.09f), new(-135.8f, 181.17f),
    new(-136.42f, 181.39f), new(-138.11f, 182.43f), new(-138.73f, 182.27f), new(-139.32f, 182.37f), new(-148.32f, 182.55f),
    new(-148.89f, 182.23f), new(-149.43f, 181.8f), new(-149.71f, 181.34f), new(-151.22f, 181.05f), new(-151.84f, 181.06f),
    new(-152.49f, 180.97f), new(-153.09f, 180.71f), new(-153.73f, 180.63f), new(-154.39f, 180.38f), new(-155.54f, 179.77f),
    new(-155.61f, 179.09f), new(-155.41f, 178.42f), new(-155.05f, 177.82f), new(-154.94f, 177.12f), new(-155.07f, 176.6f),
    new(-157.42f, 175.56f), new(-157.95f, 175.2f), new(-158.44f, 174.74f), new(-159.73f, 172.52f), new(-160.3f, 172.17f),
    new(-162.25f, 172.07f), new(-162.26f, 171.39f), new(-162.17f, 170.74f), new(-161.92f, 170.09f), new(-161.95f, 169.42f),
    new(-162.35f, 169.01f), new(-162.96f, 168.85f), new(-163.48f, 169.16f), new(-164.11f, 169.41f), new(-164.76f, 169.56f),
    new(-165.27f, 169.51f), new(-166.39f, 169.27f), new(-166.97f, 169.25f), new(-167.46f, 169.12f), new(-167.54f, 168.49f),
    new(-168.15f, 167.34f), new(-168.3f, 166.73f), new(-168.49f, 165.44f), new(-168.23f, 164.11f), new(-167.77f, 163.59f),
    new(-167.13f, 163.24f), new(-166.61f, 162.8f), new(-166.31f, 162.22f), new(-167.23f, 160.62f), new(-167.73f, 159.4f),
    new(-167.9f, 158.74f), new(-167.97f, 158.1f), new(-168.23f, 157.64f), new(-171.29f, 155.25f), new(-171.36f, 154.54f),
    new(-171.03f, 153.95f), new(-170.57f, 153.45f), new(-168.4f, 151.96f), new(-167.85f, 151.69f), new(-167.65f, 151.15f),
    new(-167.32f, 149.94f), new(-167.07f, 149.33f), new(-166.28f, 148.24f), new(-165.3f, 147.32f), new(-164.62f, 147.2f),
    new(-163.94f, 147.42f), new(-163.25f, 147.48f), new(-162.57f, 147.35f), new(-162.06f, 147.04f), new(-161.76f, 145.78f),
    new(-161.54f, 145.15f), new(-160.58f, 143.39f), new(-160.17f, 142.92f), new(-160.53f, 141.02f), new(-160.58f, 139.67f),
    new(-159.62f, 138.68f), new(-158.96f, 138.44f), new(-158.28f, 138.47f), new(-157.65f, 138.25f), new(-157.16f, 137.78f),
    new(-156.85f, 137.23f), new(-157.01f, 136.72f), new(-157.29f, 136.13f), new(-157.48f, 135.52f), new(-157.56f, 134.84f),
    new(-157.26f, 133.59f), new(-157.03f, 132.89f), new(-156.77f, 132.3f), new(-155.87f, 131.23f), new(-155.11f, 129.42f),
    new(-154.53f, 128.98f), new(-153.87f, 128.71f), new(-153.35f, 128.36f), new(-152.99f, 127.81f), new(-152.42f, 127.49f)])]);
}
