namespace BossMod.Endwalker.Alliance.A23Halone;

class RainOfSpearsFirst(BossModule module) : Components.CastCounter(module, (uint)AID.RainOfSpearsFirst);
class RainOfSpearsRest(BossModule module) : Components.CastCounter(module, (uint)AID.RainOfSpearsRest);
class SpearsThree(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SpearsThreeAOE, 5f);
class WrathOfHalone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WrathOfHaloneAOE, 25f); // TODO: verify falloff
class GlacialSpearSmall(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearSmall);
class GlacialSpearLarge(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearLarge);
class IceDart(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.IceDart, 6f);
class IceRondel(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.IceRondel, 6f, 8, 8);
class Niphas(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Niphas, 9f);
class FurysAegis(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.Shockwave, (uint)AID.FurysAegisAOE1,
(uint)AID.FurysAegisAOE2, (uint)AID.FurysAegisAOE3, (uint)AID.FurysAegisAOE4, (uint)AID.FurysAegisAOE5,
(uint)AID.FurysAegisAOE6]);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12064, PlanLevel = 90)]
public class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-700f, 600f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(29.5f);
}
