namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarMandragora;

public enum OID : uint
{
    Boss = 0x2542, //R=2.85
    AltarKorrigan = 0x255C, //R=0.84
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Mandragoras->player, no cast, single-target
    AutoAttack2 = 6499, // AltarKorrigan->player, no cast, single-target

    OpticalIntrusion = 13367, // Boss->player, 3.0s cast, single-target
    LeafDagger = 13369, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 13370, // Boss->self, 3.0s cast, single-target
    Hypnotize = 13368, // Boss->self, 2.5s cast, range 20+R 90-degree cone, gaze, paralysis

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus add disappear
}

class OpticalIntrusion(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.OpticalIntrusion);
class Hypnotize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hypnotize, new AOEShapeCone(22.85f, 45f.Degrees()));
class SaibaiMandragora(BossModule module) : Components.CastHint(module, (uint)AID.SaibaiMandragora, "Calls adds");
class LeafDagger(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LeafDagger, 3f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class AltarMandragoraStates : StateMachineBuilder
{
    public AltarMandragoraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OpticalIntrusion>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(AltarMandragora.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7600)]
public class AltarMandragora(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarKorrigan, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarKorrigan));
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
                (uint)OID.AltarQueen => 2,
                (uint)OID.AltarKorrigan => 1,
                _ => 0
            };
        }
    }
}
