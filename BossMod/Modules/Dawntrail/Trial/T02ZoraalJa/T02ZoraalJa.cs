namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

class SoulOverflow1(BossModule module) : Components.RaidwideCast(module, (uint)AID.SoulOverflow1);
class SoulOverflow2(BossModule module) : Components.RaidwideCast(module, (uint)AID.SoulOverflow2);
class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PatricidalPique);
class CalamitysEdge(BossModule module) : Components.RaidwideCast(module, (uint)AID.CalamitysEdge);
class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 8f);

class VorpalTrail(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.VorpalTrail1, (uint)AID.VorpalTrail2], 2f);

class T02ZoraalJaStates : StateMachineBuilder
{
    public T02ZoraalJaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulOverflow1>()
            .ActivateOnEnter<SoulOverflow2>()
            .ActivateOnEnter<DoubleEdgedSwords>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<CalamitysEdge>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || !Module.PrimaryActor.IsTargetable;
    }
}

public abstract class ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly Angle ArenaRotation = 45f.Degrees();
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f, ArenaRotation);
    public static readonly ArenaBoundsSquare SmallBounds = new(10f, ArenaRotation);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881, SortOrder = 1)]
public class T02ZoraalJa(WorldState ws, Actor primary) : ZoraalJa(ws, primary);
