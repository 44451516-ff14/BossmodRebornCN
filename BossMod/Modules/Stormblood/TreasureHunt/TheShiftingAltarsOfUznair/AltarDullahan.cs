namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDullahan;

public enum OID : uint
{
    Boss = 0x2533, //R=3.8
    AltarVodoriga = 0x2563, //R=1.8
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.420
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga/Mandragoras->player, no cast, single-target
    AutoAttack3 = 6497, // AltarVodoriga->player, no cast, single-target

    IronJustice = 13316, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    Cloudcover = 13477, // Boss->location, 3.0s cast, range 6 circle
    TerrorEye = 13644, // AltarVodoriga->location, 3.5s cast, range 6 circle
    StygianRelease = 13314, // Boss->self, 3.5s cast, range 50+R circle, small raidwide dmg, knockback 20 from source
    VillainousRebuke = 13315, // Boss->players, 4.5s cast, range 6 circle

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/AltarMatanga->self, no cast, single-target, bonus add disappear
}

class IronJustice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice, new AOEShapeCone(11.8f, 60.Degrees()));
class Cloudcover(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cloudcover, 6f);
class TerrorEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrorEye, 6f);
class VillainousRebuke(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VillainousRebuke, 6f, 8, 8);
class StygianRelease(BossModule module) : Components.RaidwideCast(module, (uint)AID.StygianRelease);
class StygianReleaseKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StygianRelease, 20f, stopAtWall: true)
{
    private readonly TerrorEye _aoe = module.FindComponent<TerrorEye>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _aoe.Casters.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class AltarDullahanStates : StateMachineBuilder
{
    public AltarDullahanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .ActivateOnEnter<Cloudcover>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<VillainousRebuke>()
            .ActivateOnEnter<StygianRelease>()
            .ActivateOnEnter<StygianReleaseKB>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(AltarDullahan.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7585)]
public class AltarDullahan(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarVodoriga, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarVodoriga));
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
                (uint)OID.AltarQueen or (uint)OID.AltarMatanga => 2,
                (uint)OID.AltarVodoriga => 1,
                _ => 0
            };
        }
    }
}
