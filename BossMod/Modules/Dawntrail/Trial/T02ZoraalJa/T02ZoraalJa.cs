namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

class SoulOverflow1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SoulOverflow1));
class SoulOverflow2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SoulOverflow2));
class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PatricidalPique));
class CalamitysEdge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CalamitysEdge));
class Burst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), new AOEShapeCircle(8));

abstract class VorpalTrail(BossModule module, AID aid) : Components.ChargeAOEs(module, ActionID.MakeSpell(aid), 2);
class VorpalTrail1(BossModule module) : VorpalTrail(module, AID.VorpalTrail1);
class VorpalTrail2(BossModule module) : VorpalTrail(module, AID.VorpalTrail2);

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
            .ActivateOnEnter<VorpalTrail1>()
            .ActivateOnEnter<VorpalTrail2>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || !Module.PrimaryActor.IsTargetable;
    }
}

public abstract class ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly Angle ArenaRotation = 45.Degrees();
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20, ArenaRotation);
    public static readonly ArenaBoundsSquare SmallBounds = new(10, ArenaRotation);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881, SortOrder = 1)]
public class T02ZoraalJa(WorldState ws, Actor primary) : ZoraalJa(ws, primary) { }
