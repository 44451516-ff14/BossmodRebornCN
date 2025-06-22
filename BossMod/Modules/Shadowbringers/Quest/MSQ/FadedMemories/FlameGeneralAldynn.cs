namespace BossMod.Shadowbringers.Quest.MSQ.FadedMemories.Aldynn;

public enum OID : uint
{
    Boss = 0x2F1E, // R0.59
    Lucia = 0x2F20, // R0.5
    Aymeric = 0x2F1F // R0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Lucia/Aymeric/Boss/->player, no cast, single-target
    Teleport = 21092, // Boss->location, no cast, single-target

    FlamingTizonaVisual = 21093, // Boss->self, 4.0s cast, single-target
    FlamingTizona = 21094, // player->location, 4.0s cast, range 6 circle
    HolyBladedance = 21096 // Lucia->self, 4.0s cast, range 5 width 3 rect
}

class FlamingTizona(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingTizona, 6f);
class HolyBladedance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyBladedance, new AOEShapeRect(5f, 1.5f));

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<HolyBladedance>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 4739)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-143f, 357f), new ArenaBoundsCircle(20f))
{
    public static readonly uint[] opponents = [(uint)OID.Boss, (uint)OID.Lucia, (uint)OID.Aymeric];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(opponents));
    }
}

