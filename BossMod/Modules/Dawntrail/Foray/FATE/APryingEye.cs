namespace BossMod.Dawntrail.Foray.FATE.APryingEye;

public enum OID : uint
{
    Boss = 0x47DC, // R2.4
    ObserversEye1 = 0x47DD, // R0.6
    ObserversEye2 = 0x4818, // R0.6
    Helper = 0x47DE
}

public enum AID : uint
{
    AutoAttack = 43367, // Boss->player, no cast, single-target

    Search = 43038, // ObserversEye1->self, no cast, single-target
    MarkOfDeath = 43039, // ObserversEye1->self, no cast, range 6 90-degree cone
    Visual = 42839, // ObserversEye1->self, no cast, single-target
    Stare1 = 43268, // Boss->self, 5.0s cast, range 60 width 8 rect
    Stare2 = 43044, // Boss->self, 5.0s cast, range 60 width 8 rect
    JumpScare = 43041, // Boss->player, no cast, single-target
    Oogle = 43043, // Boss->self, 4.0s cast, range 40 circle, gaze
    VoidThunderII = 43045 // Helper->location, 3.0s cast, range 6 circle
}

sealed class SearchMarkOfDeath(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(9f, 60f.Degrees()); // bigger than MarkOfDeath since its moving and we want a safety margin 

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var observers = Module.Enemies((uint)OID.ObserversEye1);
        var count = observers.Count;
        if (count == 0)
            return [];
        Span<AOEInstance> aoes = new AOEInstance[count];
        var act = WorldState.FutureTime(1d);
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var observer = observers[i];
            if (!observer.IsDead)
            {
                aoes[index++] = new(cone, observer.Position, observer.Rotation, act);
            }
        }
        return aoes[..index];
    }
}

sealed class Stare(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Stare1, (uint)AID.Stare1], new AOEShapeRect(60f, 4f));
sealed class VoidThunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidThunderII, 6f);
sealed class Oogle(BossModule module) : Components.CastGaze(module, (uint)AID.Oogle);

sealed class APryingEyeStates : StateMachineBuilder
{
    public APryingEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SearchMarkOfDeath>()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<VoidThunderII>()
            .ActivateOnEnter<Oogle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 1018, NameID = 1970)]
public sealed class APryingEye(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

