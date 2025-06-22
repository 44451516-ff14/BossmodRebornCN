namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheGreatGoldWhisker;

public enum OID : uint
{
    Boss = 0x2541, //R=2.4
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GoldWhisker->player, no cast, single-target

    TripleTrident = 13364, // Boss->players, 3.0s cast, single-target
    Tingle = 13365, // Boss->self, 4.0s cast, range 10+R circle
    FishOutOfWater = 13366, // Boss->self, 3.0s cast, single-target
    Telega = 9630 // GoldWhisker->self, no cast, single-target
}

class TripleTrident(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TripleTrident);
class FishOutOfWater(BossModule module) : Components.CastHint(module, (uint)AID.FishOutOfWater, "Spawns adds");
class Tingle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tingle, 12.4f);

class TheGreatGoldWhiskerStates : StateMachineBuilder
{
    public TheGreatGoldWhiskerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleTrident>()
            .ActivateOnEnter<FishOutOfWater>()
            .ActivateOnEnter<Tingle>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(TheGreatGoldWhisker.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7599)]
public class TheGreatGoldWhisker(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GoldWhisker];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GoldWhisker), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GoldWhisker => 1,
                _ => 0
            };
        }
    }
}
