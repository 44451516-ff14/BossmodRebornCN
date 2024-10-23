namespace BossMod.Dawntrail.Quest.JobQuests.Viper.FangsOfTheViper;

public enum OID : uint
{
    Boss = 0x429F, // R0.5
    FawningPeiste = 0x42A1, // R2.34
    FawningRaptor = 0x42A3, // R1.82
    FawningWivre = 0x42A2, // R3.9
    WanderingGowrow = 0x42A0, // R4.0
    Helper = 0x43A3
}

public enum AID : uint
{
    AutoAttack1 = 6497, // FawningPeiste/FawningRaptor/WanderingGowrow->player, no cast, single-target
    AutoAttack2 = 6498, // FawningWivre->player, no cast, single-target
    Visual1 = 37699, // Helper->self, no cast, single-target
    Visual2 = 37698, // Helper->self, no cast, single-target
    Visual3 = 37697, // WanderingGowrow->self, no cast, single-target

    BurningCyclone = 37703, // FawningPeiste->self, 5.0s cast, range 6 120-degree cone
    FoulBreath = 39263, // FawningRaptor->self, 5.0s cast, range 7 90-degree cone
    BrowHorn = 37704, // FawningWivre->self, 5.0s cast, range 6 width 4 rect
    Firebreathe = 37700, // WanderingGowrow->self, 5.0s cast, range 60 90-degree cone
    RightSidedShockwave = 37701, // WanderingGowrow->self, 4.0s cast, range 20 180-degree cone
    LeftSidedShockwave = 37702 // WanderingGowrow->self, 4.0s cast, range 20 180-degree cone
}

class BurningCyclone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningCyclone), new AOEShapeCone(6, 60.Degrees()));
class FoulBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FoulBreath), new AOEShapeCone(7, 45.Degrees()));
class BrowHorn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrowHorn), new AOEShapeRect(6, 2));
class Firebreathe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Firebreathe), new AOEShapeCone(60, 45.Degrees()));

abstract class Shockwave(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(20, 90.Degrees()));
class RightSidedShockwave(BossModule module) : Shockwave(module, AID.RightSidedShockwave);
class LeftSidedShockwave(BossModule module) : Shockwave(module, AID.LeftSidedShockwave);

class FangsOfTheViperStates : StateMachineBuilder
{
    public FangsOfTheViperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .ActivateOnEnter<FoulBreath>()
            .ActivateOnEnter<BrowHorn>()
            .ActivateOnEnter<Firebreathe>()
            .ActivateOnEnter<RightSidedShockwave>()
            .ActivateOnEnter<LeftSidedShockwave>()
            .Raw.Update = () => module.Enemies(OID.WanderingGowrow).Any(e => e.IsDead) || module.PrimaryActor.IsDeadOrDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70385, NameID = 12825)]
public class FangsOfTheViper(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(264, 480), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.FawningWivre).Concat(Enemies(OID.FawningPeiste)).Concat(Enemies(OID.FawningRaptor)).Concat(Enemies(OID.WanderingGowrow)));
    }

    protected override bool CheckPull() => Raid.WithoutSlot().Any(x => x.InCombat);
}
