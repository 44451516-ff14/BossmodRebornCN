namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretBasket;

public enum OID : uint
{
    Boss = 0x302D, //R=2.34
    SecretEchivore = 0x302E, //R=1.05
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
    AutoAttack = 872, // Boss/SecretEchivore/Mandragoras/KeeperOfKeys->player, no cast, single-target

    HeavyStrikeVisual1 = 21698, // Boss->self, no cast, single-target
    HeavyStrikeVisual2 = 21723, // Boss->self, no cast, single-target
    HeavyStrike1 = 21724, // Helper->self, 4.0s cast, range 6+R 270-degree cone
    HeavyStrike2 = 21725, // Helper->self, 4.0s cast, range 12+R 270-degree donut segment
    HeavyStrike3 = 21726, // Helper->self, 4.9s cast, range 18+R 270-degree donut segment

    PollenCorona = 21722, // Boss->self, 3.0s cast, range 8 circle
    StraightPunch = 21721, // Boss->player, 4.0s cast, single-target
    Leafcutter = 21732, // SecretEchivore->self, 3.0s cast, range 15 width 4 rect
    EarthCrusher = 21727, // Boss->self, 3.0s cast, single-target
    EarthCrusher2 = 21728, // Helper->self, 4.0s cast, range 10-20 donut
    SomersaultSlash = 21731, // SecretEchivore->player, no cast, single-target
    EarthquakeVisual = 21729, // Boss->self, 4.0s cast, single-target
    Earthquake = 21730, // Helper->self, no cast, range 20 circle

    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // SecretGarlic->self, 3.5s cast, range 6+R circle
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Telega = 9630 // Mandragoras->self, no cast, single-target, bonus adds disappear
}

class Earthquake(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.EarthquakeVisual, (uint)AID.Earthquake, 1.2f);

class HeavyStrike(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly Angle a135 = 135f.Degrees();
    private static readonly AOEShape[] _shapes = [new AOEShapeCone(6.5f, a135), new AOEShapeDonutSector(6.5f, 12.5f, a135), new AOEShapeDonutSector(12.5f, 18.5f, a135)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyStrike1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.HeavyStrike1 => 0,
                (uint)AID.HeavyStrike2 => 1,
                (uint)AID.HeavyStrike3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.1d), spell.Rotation);
        }
    }
}

class PollenCorona(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PollenCorona, 8f);
class StraightPunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StraightPunch);
class Leafcutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Leafcutter, new AOEShapeRect(15f, 2f));
class EarthCrusher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthCrusher2, new AOEShapeDonut(10f, 20f));

abstract class Mandragoras(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6.84f);
class PluckAndPrune(BossModule module) : Mandragoras(module, (uint)AID.PluckAndPrune);
class TearyTwirl(BossModule module) : Mandragoras(module, (uint)AID.TearyTwirl);
class HeirloomScream(BossModule module) : Mandragoras(module, (uint)AID.HeirloomScream);
class PungentPirouette(BossModule module) : Mandragoras(module, (uint)AID.PungentPirouette);
class Pollen(BossModule module) : Mandragoras(module, (uint)AID.Pollen);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class SecretBasketStates : StateMachineBuilder
{
    public SecretBasketStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<HeavyStrike>()
            .ActivateOnEnter<PollenCorona>()
            .ActivateOnEnter<StraightPunch>()
            .ActivateOnEnter<Leafcutter>()
            .ActivateOnEnter<EarthCrusher>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretBasket.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9784)]
public class SecretBasket(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SecretEchivore, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SecretEchivore));
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
                (uint)OID.SecretOnion => 6,
                (uint)OID.SecretEgg => 5,
                (uint)OID.SecretGarlic => 4,
                (uint)OID.SecretTomato or (uint)OID.FuathTrickster => 3,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretEchivore => 1,
                _ => 0
            };
        }
    }
}
