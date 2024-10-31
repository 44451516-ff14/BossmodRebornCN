namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class ProsecutionOfWar(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ProsecutionOfWar));
class VirtualShift1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtualShift1));
class VirtualShift2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtualShift2));
class VirtualShift3(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtualShift3));
class BrutalCrown(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrutalCrown), new AOEShapeDonut(5, 60));
class RoyalDomain(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RoyalDomain));
class DynasticDiadem(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DynasticDiadem), new AOEShapeDonut(6, 70));

abstract class RaidwideMulti(BossModule module, AID aid) : Components.RaidwideCast(module, ActionID.MakeSpell(aid), "multiple Raidwides");
class RoyalBanishmentRaidwide(BossModule module) : RaidwideMulti(module, AID.RoyalBanishmentVisual);
class AbsoluteAuthorityRaidwide(BossModule module) : RaidwideMulti(module, AID.AbsoluteAuthorityRaidwide1);

class T03QueenEternalStates : StateMachineBuilder
{
    public T03QueenEternalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Besiegement>()
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<Aethertithe>()
            .ActivateOnEnter<WaltzOfTheRegalia>()
            .ActivateOnEnter<WaltzOfTheRegaliaBait>()
            .ActivateOnEnter<RuthlessRegalia>()
            .ActivateOnEnter<ProsecutionOfWar>()
            .ActivateOnEnter<VirtualShift1>()
            .ActivateOnEnter<VirtualShift2>()
            .ActivateOnEnter<VirtualShift3>()
            .ActivateOnEnter<AbsoluteAuthorityRaidwide>()
            .ActivateOnEnter<DownburstKB>()
            .ActivateOnEnter<DownburstRaidwide>()
            .ActivateOnEnter<BrutalCrown>()
            .ActivateOnEnter<PowerfulGustKB>()
            .ActivateOnEnter<PowerfulGustRaidwide>()
            .ActivateOnEnter<RoyalDomain>()
            .ActivateOnEnter<AbsoluteAuthorityCircle>()
            .ActivateOnEnter<AuthoritysGaze>()
            .ActivateOnEnter<AuthoritysHold>()
            .ActivateOnEnter<AbsoluteAuthorityDorito>()
            .ActivateOnEnter<AbsoluteAuthorityFlare>()
            .ActivateOnEnter<DynasticDiadem>()
            .ActivateOnEnter<DivideAndConquer>()
            .ActivateOnEnter<RoyalBanishment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 984, NameID = 13029)]
public class T03QueenEternal(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly WPos FinalCenter = new(100, 105);
    public static readonly WPos LeftSplitCenter = new(108, 94);
    public static readonly WPos RightSplitCenter = new(92, 94);
    public static readonly ArenaBoundsRect FinalBounds = new(20, 15);
    public static readonly ArenaBoundsRect SplitGravityBounds = new(12, 8);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    public static readonly ArenaBoundsComplex XArena = new([new Rectangle(new(100, 82.5f), 12.5f, 2.5f), new Rectangle(new(100, 102.5f), 12.5f, 2.5f), new Cross(new(100, 92.5f), 15, 2.5f, 45.Degrees())], Offset: -0.5f);
    public static readonly ArenaBoundsComplex SplitArena = new([new Rectangle(LeftSplitCenter, 4, 8), new Rectangle(RightSplitCenter, 4, 8)]);
}
