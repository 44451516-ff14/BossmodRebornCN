namespace BossMod.RealmReborn.Dungeon.D10StoneVigil.D101ChudoYudo;

public enum OID : uint
{
    Boss = 0x5B5 // R4.24
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Rake = 901, // Boss->player, no cast, extra attack on tank
    LionsBreath = 902, // Boss->self, 1.0s cast, range 6+R 120-degree cone
    Swinge = 903 // Boss->self, 4.0s cast, range 40+R 60-degree cone
}

class LionsBreathCleave(BossModule module) : Components.Cleave(module, (uint)AID.LionsBreath, new AOEShapeCone(10.24f, 60.Degrees()), activeWhileCasting: false);
class LionsBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LionsBreath, new AOEShapeCone(10.24f, 60.Degrees()));
class Swinge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swinge, new AOEShapeCone(44.24f, 30.Degrees()));

class D101ChudoYudoStates : StateMachineBuilder
{
    public D101ChudoYudoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LionsBreath>()
            .ActivateOnEnter<LionsBreathCleave>()
            .ActivateOnEnter<Swinge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 11, NameID = 1677)]
public class D101ChudoYudo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Angle a45 = 45.Degrees();
    private static readonly Shape[] difference = [new Square(new(-20, 136), 3, a45), new Square(new(20, 136), 3, a45), new Square(new(-20, 96), 3, a45),
    new Square(new(20, 96), 3, a45), new Rectangle(new(-4.1f, 99), 0.5f, 4), new Circle(new(-4.5f, 96), 0.9f), new Rectangle(new(4.1f, 99), 0.5f, 4),
    new Circle(new(4.5f, 96), 0.9f), new Square(new(7.7f, 95.9f), 0.5f), new Square(new(16.3f, 96.2f), 0.45f, a45), new Square(new(20, 99.8f), 0.5f, a45), new Square(new(20.1f, 108), 0.5f),
    new Square(new(20.1f, 116), 0.5f), new Square(new(20.1f, 124), 0.5f), new Square(new(20, 132.2f), 0.5f, a45), new Square(new(16.2f, 136), 0.5f, a45), new Square(new(7.6f, 136.2f), 0.5f),
    new Square(new(-8, 136.2f), 0.5f), new Square(new(-16.2f, 136), 0.5f, a45), new Square(new(-20, 132.2f), 0.5f, a45), new Square(new(-20.1f, 124), 0.5f), new Square(new(-20.1f, 116), 0.5f),
    new Square(new(-20.1f, 108), 0.5f), new Square(new(-16.3f, 96.2f), 0.45f, a45), new Square(new(-20, 99.8f), 0.5f, a45), new Square(new(-7.7f, 95.9f), 0.5f)];
    public static readonly ArenaBoundsCustom arena = new([new Square(new(0, 116), 19.7f)], difference);
}
