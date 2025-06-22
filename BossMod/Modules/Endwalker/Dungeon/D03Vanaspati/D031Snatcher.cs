namespace BossMod.Endwalker.Dungeon.D03Vanaspati.D031Snatcher;

public enum OID : uint
{
    Boss = 0x33E8, // R=3.99
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    LastGasp = 25141, // Boss->player, 5.0s cast, single-target
    LostHope = 25143, // Boss->self, 4.0s cast, range 20 circle, applies temporary misdirection
    MouthOff = 25137, // Boss->self, 3.0s cast, single-target
    NoteOfDespair = 25144, // Boss->self, 5.0s cast, range 40 circle
    Vitriol = 25138, // Helper->self, 9.0s cast, range 13 circle
    Wallow = 25142, // Helper->player, 5.0s cast, range 6 circle
    WhatIsLeft = 25140, // Boss->self, 8.0s cast, range 20 180-degree cone
    WhatIsRight = 25139 // Boss->self, 8.0s cast, range 20 180-degree cone
}

class Cleave(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 90f.Degrees()));
class WhatIsLeft(BossModule module) : Cleave(module, (uint)AID.WhatIsLeft);
class WhatIsRight(BossModule module) : Cleave(module, (uint)AID.WhatIsRight);

class LostHope(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.LostHope);
class Vitriol(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Vitriol, 13f);
class NoteOfDespair(BossModule module) : Components.RaidwideCast(module, (uint)AID.NoteOfDespair);
class Wallow(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Wallow, 6f);
class LastGasp(BossModule module) : Components.SingleTargetCast(module, (uint)AID.LastGasp);

class D031SnatcherStates : StateMachineBuilder
{
    public D031SnatcherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhatIsLeft>()
            .ActivateOnEnter<WhatIsRight>()
            .ActivateOnEnter<LostHope>()
            .ActivateOnEnter<Vitriol>()
            .ActivateOnEnter<NoteOfDespair>()
            .ActivateOnEnter<Wallow>()
            .ActivateOnEnter<LastGasp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS, Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 789, NameID = 10717)]
public class D031Snatcher(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-375f, 85f), 19.5f * CosPI.Pi36th, 36)],
    [new Rectangle(new(-375f, 105f), 20f, 1.2f), new Rectangle(new(-375f, 61f), 20f, 2f, -30f.Degrees())]);
}
