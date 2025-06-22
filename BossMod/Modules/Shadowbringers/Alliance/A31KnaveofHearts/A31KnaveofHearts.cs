namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);

abstract class ColossalImpact(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(61, 10));
class ColossalImpact1(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpact1);
class ColossalImpact2(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpact2);
class ColossalImpact3(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpact3);
class ColossalImpactLeft(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpactLeft);
class ColossalImpactRight(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpactRight);
class ColossalImpactCenter(BossModule module) : ColossalImpact(module, (uint)AID.ColossalImpactCenter);

class MagicArtilleryBeta(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MagicArtilleryBeta, 3);
class MagicArtilleryAlpha(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MagicArtilleryAlpha, 5);
class LightLeap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightLeap, 25);
class BoxSpawn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoxSpawn, new AOEShapeRect(8, 4));
class MagicBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagicBarrage, new AOEShapeRect(61, 2.5f));
class Lunge(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Lunge, 60, stopAtWall: true, kind: Kind.DirForward);
class Energy(BossModule module) : Components.Voidzone(module, 1, m => m.Enemies(OID.Energy).Where(z => z.EventState != 7));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9955)]
public class A31KnaveofHearts(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -724.4f), new ArenaBoundsSquare(29.5f));
