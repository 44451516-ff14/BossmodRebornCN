namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretKorrigan;

public enum OID : uint
{
    Boss = 0x3022, //R=2.85
    SecretMandragora = 0x301C, //R=0.84
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/SecretMandragora/Mandragoras->player, no cast, single-target

    Hypnotize = 21674, // Boss->self, 4.0s cast, range 40 circle
    LeafDagger = 21675, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 21676, // Boss->self, 3.0s cast, single-target
    Ram = 21673, // Boss->player, 3.0s cast, single-target

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class Hypnotize(BossModule module) : Components.CastGaze(module, (uint)AID.Hypnotize);
class Ram(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Ram);
class SaibaiMandragora(BossModule module) : Components.CastHint(module, (uint)AID.SaibaiMandragora, "Calls adds");
class LeafDagger(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LeafDagger, 3f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class SecretKorriganStates : StateMachineBuilder
{
    public SecretKorriganStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<Ram>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(SecretKorrigan.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9806)]
public class SecretKorrigan(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SecretMandragora, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SecretMandragora));
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
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato => 3,
                (uint)OID.SecretQueen => 2,
                (uint)OID.SecretMandragora => 1,
                _ => 0
            };
        }
    }
}
