namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretPorxie;

public enum OID : uint
{
    Boss = 0x3014, //R=1.2
    MagickedBroomHelper = 0x310A, // R=0.5
    MagickedBroom1 = 0x30F3, // R=3.125
    MagickedBroom2 = 0x30F2, // R=3.125
    MagickedBroom3 = 0x30F4, // R=3.125
    MagickedBroom4 = 0x30F1, // R=3.125
    MagickedBroom5 = 0x3015, // R=3.125
    MagickedBroom6 = 0x30F0, // R=3.125
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    SecretQueen = 0x3021, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    SecretGarlic = 0x301F, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    SecretTomato = 0x3020, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    SecretOnion = 0x301D, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    SecretEgg = 0x301E, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/KeeperOfKeys/Mandragoras->player, no cast, single-target

    BrewingStorm = 21670, // Boss->self, 2.5s cast, range 5 60-degree cone, knockback 10 away from source
    HarrowingDream = 21671, // Boss->self, 3.0s cast, range 6 circle, applies sleep
    BecloudingDustVisual = 22935, // Boss->self, 3.0s cast, single-target
    BecloudingDust = 22936, // BossHelper->location, 3.0s cast, range 6 circle
    SweepStart = 22937, // Brooms>self, 4.0s cast, range 6 circle
    SweepRest = 21672, // Brooms->self, no cast, range 6 circle
    SweepVisual = 22508, // Brooms->self, no cast, single-target, visual
    SweepVisual2 = 22509, // Brooms->self, no cast, single-target

    Telega = 9630, // KeeperOfKeys/Mandragoras->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768, // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
    Pollen = 6452, // SecretQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // SecretOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // SecretTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // SecretEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450 // SecretGarlic->self, 3.5s cast, range 6+R circle
}

class BrewingStorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrewingStorm, new AOEShapeCone(5f, 30f.Degrees()));
class HarrowingDream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HarrowingDream, 6f);
class BecloudingDust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BecloudingDust, 6f);

class Sweep(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SweepStart)
            Lines.Add(new(caster.Position, 12f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell, 0.9d), 4.5d, 4, 3));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SweepRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class SecretPorxieStates : StateMachineBuilder
{
    public SecretPorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrewingStorm>()
            .ActivateOnEnter<HarrowingDream>()
            .ActivateOnEnter<BecloudingDust>()
            .ActivateOnEnter<Sweep>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretPorxie.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9795)]
public class SecretPorxie(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.SecretEgg, (uint)OID.SecretGarlic, (uint)OID.SecretOnion, (uint)OID.SecretTomato,
    (uint)OID.SecretQueen, (uint)OID.KeeperOfKeys];
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
                (uint)OID.SecretTomato => 2,
                (uint)OID.SecretQueen or (uint)OID.KeeperOfKeys => 1,
                _ => 0
            };
        }
    }
}
