namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.GreedyPixie;

public enum OID : uint
{
    Boss = 0x3018, //R=1.6
    SecretMorpho = 0x3019, //R=1.8
    PixieDouble1 = 0x304C, //R=1.6
    PixieDouble2 = 0x304D, //R=1.6
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23185, // Boss/Mandragoras->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys/SecretMorpho->player, no cast, single-target

    WindRune = 21686, // Boss->self, 3.0s cast, range 40 width 8 rect
    SongRune = 21684, // Boss->location, 3.0s cast, range 6 circle
    StormRune = 21682, // Boss->self, 4.0s cast, range 40 circle
    BushBash1 = 22779, // PixieDouble2->self, 7.0s cast, range 12 circle
    BushBash2 = 21683, // Boss->self, 5.0s cast, range 12 circle
    NatureCall1 = 22780, // PixieDouble1->self, 7.0s cast, range 30 120-degree cone, turns player into a plant
    NatureCall2 = 21685, // Boss->self, 5.0s cast, range 30 120-degree cone, turns player into a plant

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Windrune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindRune, new AOEShapeRect(40f, 4f));
class SongRune(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SongRune, 6f);
class StormRune(BossModule module) : Components.RaidwideCast(module, (uint)AID.StormRune);

abstract class BushBash(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 12f);
class BushBash1(BossModule module) : BushBash(module, (uint)AID.BushBash1);
class BushBash2(BossModule module) : BushBash(module, (uint)AID.BushBash2);

abstract class NatureCall(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 60f.Degrees()));
class NatureCall1(BossModule module) : NatureCall(module, (uint)AID.NatureCall1);
class NatureCall2(BossModule module) : NatureCall(module, (uint)AID.NatureCall2);

abstract class Mandragoras(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6.84f);
class PluckAndPrune(BossModule module) : Mandragoras(module, (uint)AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, (uint)AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, (uint)AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, (uint)AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, (uint)AID.Pollen);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class GreedyPixieStates : StateMachineBuilder
{
    public GreedyPixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Windrune>()
            .ActivateOnEnter<StormRune>()
            .ActivateOnEnter<SongRune>()
            .ActivateOnEnter<BushBash1>()
            .ActivateOnEnter<BushBash2>()
            .ActivateOnEnter<NatureCall1>()
            .ActivateOnEnter<NatureCall2>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () => AllDeadOrDestroyed(GreedyPixie.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9797)]
public class GreedyPixie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SecretMorpho, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SecretMorpho));
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
                (uint)OID.SecretTomato or (uint)OID.FuathTrickster => 3,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretMorpho => 1,
                _ => 0
            };
        }
    }
}
