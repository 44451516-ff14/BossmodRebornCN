namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretDjinn;

public enum OID : uint
{
    Boss = 0x300F, //R=3.48
    SecretRabbitsTail = 0x3010, //R=1.32
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 23185, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // SecretRabbitsTail/KeeperOfKeys->player, no cast, single-target

    Gust = 21655, // Boss->location, 3.0s cast, range 6 circle
    ChangelessWinds = 21657, // Boss->self, 3.0s cast, range 40 width 8 rect, knockback 10, source forward
    WhirlingGaol = 21654, // Boss->self, 4.0s cast, range 40 circle, knockback 25 away from source
    Whipwind = 21656, // Boss->self, 5.0s cast, range 55 width 40 rect, knockback 25, source forward
    GentleBreeze = 21653, // SecretRabbitsTail->self, 3.0s cast, range 15 width 4 rect

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

class Gust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gust, 6f);
class ChangelessWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChangelessWinds, new AOEShapeRect(40f, 4f));
class Whipwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Whipwind, new AOEShapeRect(55f, 20f));
class GentleBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GentleBreeze, new AOEShapeRect(15f, 2f));
class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhirlingGaol, "Raidwide + Knockback");
class WhirlingGaolKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WhirlingGaol, 25f, stopAtWall: true);

class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class SecretDjinnStates : StateMachineBuilder
{
    public SecretDjinnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<GentleBreeze>()
            .ActivateOnEnter<WhirlingGaol>()
            .ActivateOnEnter<WhirlingGaolKB>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretDjinn.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9788)]
public class SecretDjinn(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SecretRabbitsTail, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SecretRabbitsTail));
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
                (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SecretRabbitsTail => 1,
                _ => 0
            };
        }
    }
}
