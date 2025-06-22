namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheOlderOne;

public enum OID : uint
{
    Boss = 0x253F, //R=5.06
    AltarIdol1 = 0x2572, //R=1.72, untargetable
    AltarIdol2 = 0x2540, //R=1.72, untargetable
    AltarIdol3 = 0x2573, //R=1.72, untargetable
    AltarIdol4 = 0x2574, //R=1.72, untargetable
    GoldWhisker = 0x2544, // R0.54
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga/Mandragoras->player, no cast, single-target
    AutoAttack3 = 13478, // Boss->player, no cast, single-target

    MysticFlash = 13385, // Boss->player, 3.0s cast, single-target
    MysticLight = 13386, // Boss->self, 3.0s cast, range 40+R 60-degree cone
    MysticFlame = 13387, // Boss->self, 3.0s cast, single-target
    MysticFlame2 = 13388, // Helper->location, 3.0s cast, range 7 circle
    MysticLevin = 13389, // Boss->self, 3.0s cast, range 30+R circle
    MysticHeat = 13390, // AltarIdols->self, 3.0s cast, range 40+R width 3 rect
    SelfDetonate = 13391, // AltarIdols->self, 4.0s cast, range 9+R circle

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // AltarMatanga/Mandragoras/GoldWhisker->self, no cast, single-target, bonus add disappear
}

class MysticLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticLight, new AOEShapeCone(45.06f, 30.Degrees()));
class MysticFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticFlame2, 7);
class MysticHeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MysticHeat, new AOEShapeRect(41.72f, 1.5f));
class SelfDetonate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SelfDetonate, 10.72f);
class MysticLevin(BossModule module) : Components.RaidwideCast(module, (uint)AID.MysticLevin);
class MysticFlash(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.MysticFlash);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60.Degrees()), [(uint)OID.AltarMatanga]);

class TheOlderOneStates : StateMachineBuilder
{
    public TheOlderOneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<MysticFlame>()
            .ActivateOnEnter<MysticHeat>()
            .ActivateOnEnter<MysticLevin>()
            .ActivateOnEnter<MysticFlash>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(TheOlderOne.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7597)]
public class TheOlderOne(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen or (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
