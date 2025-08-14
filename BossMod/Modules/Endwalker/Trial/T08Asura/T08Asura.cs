namespace BossMod.Endwalker.Trial.T08Asura;

class LowerRealm(BossModule module) : Components.RaidwideCast(module, (uint)AID.LowerRealm);
class Ephemerality(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ephemerality);

class CuttingJewel(BossModule module) : Components.BaitAwayCast(module, (uint)AID.CuttingJewel, 4f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class IconographyPedestalPurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IconographyPedestalPurge, 10f);
class PedestalPurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PedestalPurge, 60f);
class IconographyWheelOfDeincarnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IconographyWheelOfDeincarnation, new AOEShapeDonut(8f, 40f));
class WheelOfDeincarnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WheelOfDeincarnation, new AOEShapeDonut(48f, 96f));
class IconographyBladewise(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IconographyBladewise, new AOEShapeRect(50f, 3f));
class Bladewise(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bladewise, new AOEShapeRect(100f, 14f));
class Scattering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scattering, new AOEShapeRect(20f, 3f));
class OrderedChaos(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.OrderedChaos, 5f);
class MyriadAspects(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MyriadAspects1, (uint)AID.MyriadAspects2], new AOEShapeCone(40f, 15f.Degrees()), 6, 12);

class T08AsuraStates : StateMachineBuilder
{
    public T08AsuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Ephemerality>()
            .ActivateOnEnter<LowerRealm>()
            .ActivateOnEnter<AsuriChakra>()
            .ActivateOnEnter<Chakra1>()
            .ActivateOnEnter<Chakra2>()
            .ActivateOnEnter<Chakra3>()
            .ActivateOnEnter<Chakra4>()
            .ActivateOnEnter<Chakra5>()
            .ActivateOnEnter<CuttingJewel>()
            .ActivateOnEnter<Laceration>()
            .ActivateOnEnter<IconographyPedestalPurge>()
            .ActivateOnEnter<PedestalPurge>()
            .ActivateOnEnter<IconographyWheelOfDeincarnation>()
            .ActivateOnEnter<WheelOfDeincarnation>()
            .ActivateOnEnter<IconographyBladewise>()
            .ActivateOnEnter<Bladewise>()
            .ActivateOnEnter<SixBladedKhadga>()
            .ActivateOnEnter<MyriadAspects>()
            .ActivateOnEnter<Scattering>()
            .ActivateOnEnter<OrderedChaos>()
            .ActivateOnEnter<ManyFaces>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 944, NameID = 12351)]
public class T08Asura(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, StartingArena)
{
    private static readonly WPos arenaCenter = new(100, 100);
    public static readonly ArenaBoundsComplex StartingArena = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi32th, 32)]);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(arenaCenter, 19.165f, 32)]);
}
