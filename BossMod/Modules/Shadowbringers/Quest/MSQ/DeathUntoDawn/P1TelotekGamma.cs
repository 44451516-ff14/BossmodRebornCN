using BossMod.QuestBattle.Shadowbringers.MSQ;

namespace BossMod.Shadowbringers.Quest.MSQ.DeathUntoDawn.P1;

public enum AID : uint
{
    AntiPersonnelMissile = 24845, // 233C->player/321D, 5.0s cast, range 6 circle
    MRVMissile = 24843, // 233C->location, 8.0s cast, range 12 circle
}

enum OID : uint
{
    Boss = 0x3376
}

class AlisaieAI(BossModule module) : QuestBattle.RotationModule<AutoAlisaie>(module);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 6f);
class MRVMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MRVMissile, 12f, 6);

public class TelotekGammaStates : StateMachineBuilder
{
    public TelotekGammaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlisaieAI>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MRVMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10189)]
public class TelotekGamma(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -180f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
}
