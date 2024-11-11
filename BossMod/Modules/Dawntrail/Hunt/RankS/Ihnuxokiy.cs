namespace BossMod.Dawntrail.Hunt.RankS.Ihnuxokiy;

public enum OID : uint
{
    Boss = 0x4582 // R7.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Gapcloser = 39772, // Boss->location, no cast, single-target
    AbyssalSmog1 = 39773, // Boss->self, 8.0s cast, range 40 180-degree cone
    AbyssalSmog2 = 39828, // Boss->self, 10.0s cast, range 40 180-degree cone
    Aetherstock1 = 39778, // Boss->self, 4.0s cast, single-target, thunderspark
    Aetherstock2 = 39779, // Boss->self, 4.0s cast, single-target, cyclonic ring
    CyclonicRing = 39781, // Boss->self, no cast, range 8-40 donut
    ChaoticStorm = 39777, // Boss->self, 5.0s cast, range 40 circle, raidwide, forced march debuffs
    Thunderspark = 39780, // Boss->self, no cast, range 12 circle
    RazorZephyr = 39774, // Boss->self, 3.0s cast, range 50 width 12 rect
    Blade = 39776 // Boss->player, 5.0s cast, single-target, tankbuster
}

public enum SID : uint
{
    Aetherstock = 4136, // Boss->Boss, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForwardMarch = 2161, // Boss->player, extra=0x0
    ForcedMarch = 1257 // Boss->player, extra=0x1/0x2/0x4/0x8
}

class AetherstockAbyssalSmog(BossModule module) : Components.GenericAOEs(module)
{
    private enum Aetherspark { None, Thunderspark, CyclonicRing }
    private Aetherspark currentAetherspark;
    private static readonly AOEShapeCone cone = new(40, 90.Degrees());
    private static readonly AOEShapeDonut donut = new(8, 40);
    private static readonly AOEShapeCircle circle = new(12);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (count > 1)
            yield return _aoes[1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aetherstock1:
                currentAetherspark = Aetherspark.Thunderspark;
                break;
            case AID.Aetherstock2:
                currentAetherspark = Aetherspark.CyclonicRing;
                break;
            case AID.AbyssalSmog1:
            case AID.AbyssalSmog2:
                var position = Module.PrimaryActor.Position;
                AOEShape? shape = currentAetherspark == Aetherspark.Thunderspark ? circle : currentAetherspark == Aetherspark.CyclonicRing ? donut : null;
                _aoes.Add(new(cone, position, spell.Rotation, Module.CastFinishAt(spell)));
                if (shape != null)
                {
                    _aoes.Add(new(shape, position, default, Module.CastFinishAt(spell, 2.2f)));
                    currentAetherspark = Aetherspark.None;
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count == 0)
            return;
        switch ((AID)spell.Action.ID)
        {
            case AID.AbyssalSmog1:
            case AID.AbyssalSmog2:
            case AID.CyclonicRing:
            case AID.Thunderspark:
                _aoes.RemoveAt(0);
                break;
        }
    }
}

class ChaoticStormForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);
class ChaoticStorm(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ChaoticStorm), "Raidwide + forced march debuffs");
class RazorZephyr(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorZephyr), new AOEShapeRect(50, 6));
class Blade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Blade));

class IhnuxokiyStates : StateMachineBuilder
{
    public IhnuxokiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChaoticStorm>()
            .ActivateOnEnter<ChaoticStormForcedMarch>()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<AetherstockAbyssalSmog>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13444)]
public class Ihnuxokiy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
