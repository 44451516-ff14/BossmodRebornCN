namespace BossMod.Heavensward.Dungeon.D02SohmAl.D021Raskovnik;

public enum OID : uint
{
    Boss = 0xE8F, // R3.68
    DravanianHornet = 0x13C2, // R0.4
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    BloodyCaress = 3793, // Boss->self, no cast, range 8+R 120-degree cone, tank cleave
    AcidRainVisual = 3794, // Boss->self, 4.0s cast, single-target
    AcidRain = 3799, // Helper->location, 3.5s cast, range 6 circle
    Phytobeam = 3797, // Boss->self, 3.0s cast, range 45+R width 12 rect
    SweetScent = 5013, // Boss->self, 3.0s cast, single-target
    FloralTrap = 5009, // Boss->self, no cast, range 45+R 45-degree cone, stun + pull 45 between hitboxes
    FlowerDevour = 5010, // Boss->self, 3.0s cast, range 8 circle
    Spit1 = 5011, // Boss->self, 3.0s cast, range 60+R circle
    Spit2 = 5012, // Helper->self, 3.0s cast, ???, spits out devoured players, knockback 10, away from source
    Leafstorm = 3798 // Boss->location, 3.0s cast, range 50 circle
}

public enum IconID : uint
{
    FloralTap = 46 // player
}

class Leafstorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.Leafstorm);
class Phytobeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Phytobeam, new AOEShapeRect(48.68f, 6f));
class AcidRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AcidRain, 6f);
class FloralTap(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(48.68f, 22.5f.Degrees()), (uint)IconID.FloralTap, (uint)AID.FloralTrap, 8.5f);
class FlowerDevour(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlowerDevour, 8f);
class BloodyCaress(BossModule module) : Components.Cleave(module, (uint)AID.BloodyCaress, new AOEShapeCone(11.68f, 60f.Degrees()));

class D021RaskovnikStates : StateMachineBuilder
{
    public D021RaskovnikStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Leafstorm>()
            .ActivateOnEnter<Phytobeam>()
            .ActivateOnEnter<BloodyCaress>()
            .ActivateOnEnter<AcidRain>()
            .ActivateOnEnter<FloralTap>()
            .ActivateOnEnter<FlowerDevour>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 37, NameID = 3791)]
public class D021Raskovnik(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-136.33f, 145.18f), new(-117.95f, 145.43f), new(-117.36f, 145.39f), new(-116.81f, 145.63f), new(-116.27f, 145.82f),
    new(-114.15f, 146.2f), new(-113.69f, 146.41f), new(-112.32f, 147.3f), new(-111.88f, 147.66f), new(-110.25f, 149.21f),
    new(-107.98f, 151.7f), new(-107.55f, 152.13f), new(-106.13f, 153.03f), new(-105.67f, 153.39f), new(-105.03f, 155.54f),
    new(-103.08f, 159.25f), new(-102.87f, 159.73f), new(-101.84f, 163.71f), new(-101.74f, 164.29f), new(-101.45f, 168.35f),
    new(-102.19f, 170.39f), new(-102.18f, 170.95f), new(-101.94f, 172.54f), new(-101.92f, 173.05f), new(-102, 174.75f),
    new(-102.2f, 175.3f), new(-102.88f, 176.87f), new(-103.41f, 177.85f), new(-103.65f, 178.4f), new(-103.62f, 178.99f),
    new(-103.77f, 179.46f), new(-105.66f, 181.56f), new(-107.58f, 184.07f), new(-107.93f, 184.49f), new(-109.36f, 185.92f),
    new(-109.63f, 186.37f), new(-110.43f, 187.87f), new(-110.94f, 188.17f), new(-111.98f, 188.69f), new(-112.51f, 188.99f),
    new(-113.73f, 190.24f), new(-114.23f, 190.55f), new(-115.91f, 190.85f), new(-116.47f, 190.99f), new(-118.26f, 192.36f),
    new(-118.75f, 192.61f), new(-120.17f, 193.12f), new(-120.67f, 193.23f), new(-122.19f, 193.24f), new(-122.77f, 193.3f),
    new(-124.77f, 194.06f), new(-125.32f, 194.07f), new(-126.88f, 194.06f), new(-129.07f, 193.85f), new(-131, 193.03f),
    new(-131.56f, 192.99f), new(-132.51f, 193.45f), new(-133.11f, 193.36f), new(-134.12f, 193.03f), new(-134.65f, 192.79f),
    new(-135.34f, 191.85f), new(-135.95f, 191.77f), new(-147.07f, 183.13f), new(-147.47f, 182.64f), new(-150.13f, 179.6f),
    new(-150.42f, 179.18f), new(-150.61f, 177.05f), new(-151.86f, 173.21f), new(-152, 172.62f), new(-151.63f, 170.92f),
    new(-151.67f, 170.29f), new(-151.87f, 168.57f), new(-151.86f, 168), new(-151.71f, 166.38f), new(-151.74f, 165.86f),
    new(-152.01f, 164.17f), new(-151.94f, 163.62f), new(-151.1f, 160.33f), new(-150.93f, 159.78f), new(-149.85f, 158.01f),
    new(-149.66f, 157.52f), new(-149.19f, 155.81f), new(-148.87f, 155.4f), new(-147.59f, 154.26f), new(-147.22f, 153.83f),
    new(-146.64f, 152.21f), new(-146.28f, 151.85f), new(-144.84f, 150.92f), new(-144.44f, 150.46f), new(-143.48f, 149.01f),
    new(-143.08f, 148.65f), new(-139.69f, 146.45f), new(-137.96f, 146.03f), new(-137.46f, 145.84f), new(-136.61f, 145.13f)];

    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DravanianHornet));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.DravanianHornet => 1,
                _ => 0
            };
        }
    }
}
