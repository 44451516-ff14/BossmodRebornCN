namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSatyros;

public enum OID : uint
{
    Boss = 0x3D2D, //R=7.5
    GymnasiouElaphos = 0x3D2E, //R=2.08
    StormsGrip = 0x3D2F, //R=1.0
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasiouLampas = 0x3D4D, //R=2.001
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/GymnasiouElaphos->player, no cast, single-target
    AutoAttack2 = 870, // GymnasiouLyssa->player, no cast, single-target

    StormWingVisual1 = 32220, // Boss->self, 5.0s cast, single-target
    StormWingVisual2 = 32219, // Boss->self, 5.0s cast, single-target
    StormWing = 32221, // Helper->self, 5.0s cast, range 40 90-degree cone
    DreadDive = 32218, // Boss->player, 5.0s cast, single-target
    FlashGale = 32222, // Boss->location, 3.0s cast, range 6 circle
    StormsGripActivate = 32199, // StormsGrip->self, no cast, single-target
    WindCutter = 32227, // StormsGrip->self, no cast, range 4 circle
    BigHorn = 32226, // GymnasiouElaphos->player, no cast, single-target
    WingblowVisual = 32224, // Boss->self, 4.0s cast, single-target
    Wingblow = 32225, // Helper->self, 4.0s cast, range 15 circle

    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

class StormWing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormWing, new AOEShapeCone(40f, 45f.Degrees()));
class FlashGale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlashGale, 6);
class WindCutter(BossModule module) : Components.Voidzone(module, 4f, GetVoidzones)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.StormsGrip);
}
class Wingblow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wingblow, 15f);
class DreadDive(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DreadDive);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouSatyrosStates : StateMachineBuilder
{
    public GymnasiouSatyrosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StormWing>()
            .ActivateOnEnter<FlashGale>()
            .ActivateOnEnter<WindCutter>()
            .ActivateOnEnter<Wingblow>()
            .ActivateOnEnter<DreadDive>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => AllDeadOrDestroyed(GymnasiouSatyros.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12003)]
public class GymnasiouSatyros(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasiouLampas, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouElaphos, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouElaphos));
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GymnasiouLampas => 3,
                (uint)OID.GymnasiouLyssa => 2,
                (uint)OID.GymnasiouElaphos => 1,
                _ => 0
            };
        }
    }
}
