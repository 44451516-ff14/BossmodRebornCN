namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D054ForgivenRevelry;

public enum OID : uint
{
    Boss = 0x28F3, //R=7.5
    Brightsphere = 0x2947, //R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 16246, // Boss->player, no cast, single-target

    LeftPalmVisual = 16249, // Boss->self, no cast, single-target
    LeftPalm = 16250, // Helper->self, 4.5s cast, range 30 width 15 rect
    RightPalmVisual = 16247, // Boss->self, no cast, single-target
    RightPalm = 16248, // Helper->self, 4.5s cast, range 30 width 15 rect

    LightShot = 16251 // Brightsphere->self, 4.0s cast, range 40 width 4 rect
}

abstract class Palm(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(30f, 7.5f));
class LeftPalm(BossModule module) : Palm(module, (uint)AID.LeftPalm);
class RightPalm(BossModule module) : Palm(module, (uint)AID.RightPalm);

class LightShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightShot, new AOEShapeRect(40f, 2f));

class D054ForgivenRevelryStates : StateMachineBuilder
{
    public D054ForgivenRevelryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftPalm>()
            .ActivateOnEnter<RightPalm>()
            .ActivateOnEnter<LightShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8270)]
public class D054ForgivenRevelry(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, 176.3f), new ArenaBoundsRect(14.65f, 14.4f));
