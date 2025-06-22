namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x60A, // x1
    CorruptedCrystal = 0x60C, // spawn during fight
    VoidPitch = 0x6B4 // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    GrimCleaver = 620, // Boss->player, no cast, single-target, random
    GrimFate = 624, // Boss->self, no cast, range 8+4.6 ?-degree cone cleave
    Desolation = 958, // Boss->self, 2.3s cast, range 55+4.6 width 6 rect aoe
    Hellssend = 1132, // Boss->self, no cast, damage up buff
    AetherialSurge = 1167, // CorruptedCrystal->self, 3.0s cast, range 5+1 circle aoe
    SeaOfPitch = 962 // VoidPitch->location, no cast, range 4 circle
}

public enum SID : uint
{
    Invincibility = 325 // none->Boss, extra=0x0
}

class GrimFate(BossModule module) : Components.Cleave(module, (uint)AID.GrimFate, new AOEShapeCone(12.6f, 60f.Degrees())); // TODO: verify angle
class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(60f, 3f));
class AetherialSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialSurge, 6f);

// note: actor 'dies' immediately after casting
class SeaOfPitch(BossModule module) : Components.Voidzone(module, 4f, GetEnemies)
{
    private static Actor[] GetEnemies(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidPitch);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimFate>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<AetherialSurge>()
            .ActivateOnEnter<SeaOfPitch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1396)]
public class D113Batraal(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly PolygonCustom[] shape = [new([new(69.7f, -150.8f), new(84.3f, -160.6f), new(100.8f, -150.9f),
    new(103.5f, -156.9f), new(102.8f, -163f), new(94.3f, -181.3f), new(84.6f, -178.8f),
    new(82.2f, -185.4f), new(81.7f, -202.9f), new(76.8f, -202.5f), new(72.2f, -203.4f), new(58.4f, -198.1f),
    new(57.6f, -193.1f), new(65.2f, -188.8f), new(73.1f, -183.7f), new(75.1f, -178.7f), new(70.9f, -173.1f),
    new(59.4f, -164.6f), new(60.8f, -159f), new(58.4f, -158.1f), new(56f, -154.1f)])];
    public static readonly ArenaBoundsComplex arena = new(shape);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CorruptedCrystal => 1,
                (uint)OID.Boss when PrimaryActor.FindStatus((uint)SID.Invincibility) != null => AIHints.Enemy.PriorityInvincible,
                _ => 0
            };
        }
    }
}
