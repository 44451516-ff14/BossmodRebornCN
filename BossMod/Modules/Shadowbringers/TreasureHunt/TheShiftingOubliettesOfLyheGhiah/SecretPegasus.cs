namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPegasus;

public enum OID : uint
{
    Boss = 0x3016, //R=2.5
    Thunderhead = 0x3017, //R=1.0
    KeeperOfKeys = 0x3034, // R3.23
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/KeeperOfKeys->player, no cast, single-target

    BurningBright = 21667, // Boss->self, 3.0s cast, range 47 width 6 rect
    Nicker = 21668, // Boss->self, 4.0s cast, range 12 circle
    CloudCall = 21666, // Boss->self, 3.0s cast, single-target, calls clouds
    Gallop = 21665, // Boss->players, no cast, width 10 rect charge, seems to target random player 5-6s after CloudCall
    LightningBolt = 21669, // Thunderhead->self, 3.0s cast, range 8 circle

    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Telega = 9630 // KeeperOfKeys->self, no cast, single-target, bonus adds disappear
}

class BurningBright(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningBright, new AOEShapeRect(47f, 3f));
class Nicker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nicker, 12f);
class CloudCall(BossModule module) : Components.CastHint(module, (uint)AID.CloudCall, "Calls thunderclouds");
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 8f);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class SecretPegasusStates : StateMachineBuilder
{
    public SecretPegasusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningBright>()
            .ActivateOnEnter<Nicker>()
            .ActivateOnEnter<CloudCall>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretPegasus.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9793)]
public class SecretPegasus(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys, (uint)OID.FuathTrickster];
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
                (uint)OID.SecretOnion => 5,
                (uint)OID.SecretEgg => 4,
                (uint)OID.SecretGarlic => 3,
                (uint)OID.SecretTomato or (uint)OID.FuathTrickster => 2,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
