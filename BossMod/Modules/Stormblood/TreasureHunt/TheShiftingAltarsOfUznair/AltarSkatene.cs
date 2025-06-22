namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarSkatene;

public enum OID : uint
{
    Boss = 0x2535, //R=4.48
    AltarDeepeye = 0x255E, //R=0.9
    AltarMatanga = 0x2545, // R3.42
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
    Telega = 963 // AltarMatanga->self, no cast, single-target, bonus adds disappear
}

class Chirp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Chirp, 12.48f);
class Tornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tornado, 6f);
class VoidCall(BossModule module) : Components.CastHint(module, (uint)AID.VoidCall, "Calls adds");
class RecklessAbandon(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.RecklessAbandon);

class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);
class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));

class AltarSkateneStates : StateMachineBuilder
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
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(AltarSkatene.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7587)]
public class AltarSkatene(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarDeepeye, (uint)OID.AltarMatanga];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarDeepeye));
        Arena.Actors(Enemies((uint)OID.AltarMatanga), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AltarMatanga => 2,
                (uint)OID.AltarDeepeye => 1,
                _ => 0
            };
        }
    }
}
