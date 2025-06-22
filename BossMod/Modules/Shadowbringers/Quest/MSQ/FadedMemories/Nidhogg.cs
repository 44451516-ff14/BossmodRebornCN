namespace BossMod.Shadowbringers.Quest.MSQ.FadedMemories.Nidhogg;

public enum OID : uint
{
    Boss = 0x2F21, // R2.7
    Clone = 0x2F22 // R2.7
}

public enum AID : uint
{
    AutoAttack = 6498, // Boss->player, no cast, single-target

    HighJumpVisual = 21099, // Clone/Boss->self, 4.0s cast, single-target
    HighJump = 21299, // player->self, 4.0s cast, range 8 circle
    Geirskogul = 21098 // Clone/Boss->self, 4.0s cast, range 62 width 8 rect
}

class HighJump(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HighJump, 8f);
class Geirskogul(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Geirskogul, new AOEShapeRect(62f, 4f));

class NidhoggStates : StateMachineBuilder
{
    public NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighJump>()
            .ActivateOnEnter<Geirskogul>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 3458)]
public class Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(-242, 436.5f), new ArenaBoundsCircle(20));
