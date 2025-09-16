namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.Hati;

public enum OID : uint
{
    Boss = 0x2538, //R=5.4
    AltarKatasharin = 0x2569, //R=3.0
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras->player, no cast, single-target
    AutoAttack3 = 6499, // AltarKatasharin->player, no cast, single-target

    GlassyNova = 13362, // Boss->self, 3.0s cast, range 40+R width 8 rect
    Hellstorm = 13359, // Boss->self, 3.0s cast, single-target
    Hellstorm2 = 13363, // Helper->location, 3.5s cast, range 10 circle
    Netherwind = 13741, // AltarKatasharin->self, 3.0s cast, range 15+R width 4 rect
    BrainFreeze = 13361, // Boss->self, 4.0s cast, range 10+R circle, turns player into Imp
    PolarRoar = 13360, // Boss->self, 3.0s cast, range 9-40 donut

    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class PolarRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PolarRoar, new AOEShapeDonut(9f, 40f));
class Hellstorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hellstorm2, 10f);
class Netherwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Netherwind, new AOEShapeRect(18f, 2f));
class GlassyNova(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlassyNova, new AOEShapeRect(45.4f, 4f));
class BrainFreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrainFreeze, 15.4f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class HatiStates : StateMachineBuilder
{
    public HatiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PolarRoar>()
            .ActivateOnEnter<Hellstorm>()
            .ActivateOnEnter<Netherwind>()
            .ActivateOnEnter<BrainFreeze>()
            .ActivateOnEnter<GlassyNova>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(Hati.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7590)]
public class Hati(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarKatasharin, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarKatasharin));
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
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen => 2,
                (uint)OID.AltarKatasharin => 1,
                _ => 0
            };
        }
    }
}
