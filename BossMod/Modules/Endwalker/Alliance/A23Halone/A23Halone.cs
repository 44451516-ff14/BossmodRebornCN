namespace BossMod.Endwalker.Alliance.A23Halone;

class RainOfSpearsFirst(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RainOfSpearsFirst));
class RainOfSpearsRest(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RainOfSpearsRest));
class SpearsThree(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SpearsThreeAOE), new AOEShapeCircle(5f), true);
class WrathOfHalone(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WrathOfHaloneAOE), 25f); // TODO: verify falloff
class GlacialSpearSmall(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearSmall);
class GlacialSpearLarge(BossModule module) : Components.Adds(module, (uint)OID.GlacialSpearLarge);
class IceDart(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.IceDart), 6f);
class IceRondel(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.IceRondel), 6f, 8, 8);
class Niphas(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Niphas), 9f);
class FurysAegis(BossModule module) : Components.CastCounterMulti(module, [ActionID.MakeSpell(AID.Shockwave), ActionID.MakeSpell(AID.FurysAegisAOE1),
ActionID.MakeSpell(AID.FurysAegisAOE2), ActionID.MakeSpell(AID.FurysAegisAOE3), ActionID.MakeSpell(AID.FurysAegisAOE4), ActionID.MakeSpell(AID.FurysAegisAOE5),
ActionID.MakeSpell(AID.FurysAegisAOE6)]);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12064, PlanLevel = 90)]
public class A23Halone(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-700f, 600f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(29.5f);
}
