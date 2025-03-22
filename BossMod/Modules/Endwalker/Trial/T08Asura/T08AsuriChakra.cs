namespace BossMod.Endwalker.Trial.T08Asura;

class AsuriChakra(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AsuriChakra), 5f);
class Chakra1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Chakra1), new AOEShapeDonut(6f, 8f));
class Chakra2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Chakra2), new AOEShapeDonut(9f, 11f));
class Chakra3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Chakra3), new AOEShapeDonut(12f, 14f));
class Chakra4(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(15f, 17f));
class Chakra5(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Chakra4), new AOEShapeDonut(18f, 20f));
