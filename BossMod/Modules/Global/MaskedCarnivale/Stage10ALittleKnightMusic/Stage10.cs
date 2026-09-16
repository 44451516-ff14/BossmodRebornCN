namespace BossMod.Global.MaskedCarnivale.Stage10;

public enum OID : uint
{
    Boss = 0x2717, // R1.0/1.5/2.0/2.5 (radius increases with amount of successful King's Will casts)
}

public enum AID : uint
{
    IronJustice1 = 14725, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Cloudcover1 = 14722, // Boss->location, 3.0s cast, range 6 circle
    KingsWill1 = 14719, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice2 = 14726, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    KingsWill2 = 14720, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice3 = 14727, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    KingsWill3 = 14721, // Boss->self, 6.0s cast, single-target, interruptible buff
    IronJustice4 = 14728, // Boss->self, 2.5s cast, range 8+R 120-degree cone
    Cloudcover2 = 14723, // Boss->player, no cast, range 6 circle
    BlackNebula = 14724, // Boss->self, 6.0s cast, range 50+R circle, interruptible enrage after 3 King's Will casts
}

sealed class IronJustice1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice1, new AOEShapeCone(9f, 60f.Degrees()));
sealed class IronJustice2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice2, new AOEShapeCone(9.5f, 60f.Degrees()));
sealed class IronJustice3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice3, new AOEShapeCone(10f, 60f.Degrees()));
sealed class IronJustice4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice4, new AOEShapeCone(10.5f, 60f.Degrees()));
sealed class BlackNebula(BossModule module) : Components.CastInterruptHint(module, (uint)AID.BlackNebula);
sealed class Cloudcover1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cloudcover1, 6f);
sealed class KingsWill1(BossModule module) : Components.CastInterruptHint(module, (uint)AID.KingsWill1);
sealed class KingsWill2(BossModule module) : Components.CastInterruptHint(module, (uint)AID.KingsWill2);
sealed class KingsWill3(BossModule module) : Components.CastInterruptHint(module, (uint)AID.KingsWill3);

sealed class Stage10States : StateMachineBuilder
{
    public Stage10States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice1>()
            .ActivateOnEnter<IronJustice2>()
            .ActivateOnEnter<IronJustice3>()
            .ActivateOnEnter<IronJustice4>()
            .ActivateOnEnter<Cloudcover1>()
            .ActivateOnEnter<BlackNebula>()
            .ActivateOnEnter<KingsWill1>()
            .ActivateOnEnter<KingsWill2>()
            .ActivateOnEnter<KingsWill3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 620u, NameID = 8100u)]
public sealed class Stage10 : BossModule
{
    public Stage10(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        var name = PrimaryActor.Name;
        _prePullHints =
        [
            $"{name} will cast King's Will during the fight. Interrupt it with Flying Sardine or he will become stronger each time. After 3 casts he starts using the interruptible enrage Black Nebula.",
            $"Requirement for achievement: Let {name} cast King's Will 3 times."
        ];
    }

    private readonly string[] _prePullHints;

    public override string[] PrePullHints => _prePullHints;
}
