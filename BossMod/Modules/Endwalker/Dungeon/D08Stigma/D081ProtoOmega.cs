namespace BossMod.Endwalker.Dungeon.D08Stigma.D081ProtoOmega;

public enum OID : uint
{
    Boss = 0x3417, // R=8.99
    MarkIIGuidedMissile = 0x3418, // R1.000, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Burn = 25385, // Helper->player, no cast, range 6 circle
    ChemicalMissile = 25384, // Boss->self, 3.0s cast, single-target
    ElectricSlide = 25386, // Boss->players, 5.0s cast, range 6 circle //Stack+Knockback
    GuidedMissile = 25382, // Boss->self, 3.0s cast, single-target //Tethered bait away
    IronKiss = 25383, // MarkIIGuidedMissile->self, no cast, range 3 circle 
    MustardBomb = 25387, // Boss->player, 5.0s cast, range 5 circle
    SideCannons1 = 25376, // Boss->self, 7.0s cast, range 60 180-degree cone
    SideCannons2 = 25377 // Boss->self, 7.0s cast, range 60 180-degree cone
}

class ElectricSlideKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ElectricSlide, 15f, stopAtWall: true);
class ElectricSlide(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ElectricSlide, 6f, 4, 4);
class IronKiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronKiss, 3f);

class SideCannons(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SideCannons1, (uint)AID.SideCannons2], new AOEShapeCone(60f, 90f.Degrees()));

class MustardBomb(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MustardBomb);

class D081ProtoOmegaStates : StateMachineBuilder
{
    public D081ProtoOmegaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricSlide>()
            .ActivateOnEnter<ElectricSlideKnockback>()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<SideCannons>()
            .ActivateOnEnter<MustardBomb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10401)]
public class D081ProtoOmega(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144, -136), new ArenaBoundsSquare(20));
