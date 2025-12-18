namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarSkatene;

public enum OID : uint
{
    Boss = 0x2535, //R=4.48
    AltarDeepeye = 0x255E, //R=0.9
    AltarMatanga = 0x2545, // R3.42
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
    AutoAttack2 = 872, // AltarDeepeye->player, no cast, single-target
    AutoAttack3 = 6499, // BossAdd->player, no cast, single-target

    RecklessAbandon = 13311, // Boss->player, 3.0s cast, single-target
    Tornado = 13309, // Boss->location, 3.0s cast, range 6 circle
    VoidCall = 13312, // Boss->self, 3.5s cast, single-target
    Chirp = 13310, // Boss->self, 3.5s cast, range 8+R circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus add disappear
}

sealed class Chirp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Chirp, 12.48f);
sealed class Tornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tornado, 6f);
sealed class VoidCall(BossModule module) : Components.CastHint(module, (uint)AID.VoidCall, "Calls adds");
sealed class RecklessAbandon(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.RecklessAbandon);

sealed class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
sealed class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);
sealed class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

sealed class AltarSkateneStates : StateMachineBuilder
{
    public AltarSkateneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chirp>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<VoidCall>()
            .ActivateOnEnter<RecklessAbandon>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarSkatene.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7587)]
public sealed class AltarSkatene(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [.. bonusAdds, (uint)OID.Boss, (uint)OID.AltarDeepeye];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarDeepeye));
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
                (uint)OID.AltarQueen or (uint)OID.AltarMatanga => 2,
                (uint)OID.AltarDeepeye => 1,
                _ => 0
            };
        }
    }
}
