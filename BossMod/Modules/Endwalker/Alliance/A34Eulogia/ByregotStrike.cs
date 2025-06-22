namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ByregotStrikeJump(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ByregotStrike, 8f);
class ByregotStrikeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ByregotStrikeKnockback, 20f);
class ByregotStrikeCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ByregotStrikeAOE, new AOEShapeCone(90f, 22.5f.Degrees()));
