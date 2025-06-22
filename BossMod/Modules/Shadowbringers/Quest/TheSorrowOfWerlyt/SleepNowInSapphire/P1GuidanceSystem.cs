using BossMod.QuestBattle.Shadowbringers.SideQuests;

namespace BossMod.Shadowbringers.Quest.SorrowOfWerlyt.SleepNowInSapphire.P1GuidanceSystem;

public enum OID : uint
{
    Boss = 0x2DFF, // R4.025
    Helper = 0x233C
}

public enum AID : uint
{
    AerialBombardmentVisual = 21491, // Boss->self, 3.0s cast, single-target
    AerialBombardment = 21492, // Helper->location, 2.5s cast, range 12 circle
}

class AerialBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AerialBombardment, 12);

class GWarrior(BossModule module) : QuestBattle.RotationModule<SapphireWeapon>(module);

class GuidanceSystemStates : StateMachineBuilder
{
    public GuidanceSystemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GWarrior>()
            .ActivateOnEnter<AerialBombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69431, NameID = 9461)]
public class GuidanceSystem(WorldState ws, Actor primary) : SleepNowInSapphireSharedBounds(ws, primary)
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.FindStatus((uint)Roleplay.SID.PyreticBooster) == null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.PyreticBooster), actor, ActionQueue.Priority.Medium);
    }
}

public abstract class SleepNowInSapphireSharedBounds(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 610), arena)
{
    private static readonly ArenaBoundsSquare arena = new(59.5f);
}
