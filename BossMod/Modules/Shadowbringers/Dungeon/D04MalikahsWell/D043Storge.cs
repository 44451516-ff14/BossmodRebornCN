namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D043Storge;

public enum OID : uint
{
    Boss = 0x267B, // R=5.0
    RhapsodicNail = 0x267C, // R=1.5
    Helper2 = 0x2BD6, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    IntestinalCrank = 15601, // Boss->self, 4.0s cast, range 60 circle
    DeformationVisual1 = 15528, // Boss->self, no cast, single-target
    DeformationVisual2 = 16808, // Boss->self, no cast, single-target
    BreakingWheelWait = 17914, // Helper2->self, 7.5s cast, single-target, before long Breaking Wheel
    BreakingWheel1 = 15605, // Boss->self, 5.0s cast, range 5-60 donut
    BreakingWheel2 = 15610, // RhapsodicNail->self, 9.0s cast, range 5-60 donut
    BreakingWheel3 = 15887, // Boss->self, 29.0s cast, range 5-60 donut
    CrystalNailVisual = 15606, // Boss->self, 2.5s cast, single-target
    CrystalNail = 15607, // RhapsodicNail->self, 2.5s cast, range 5 circle
    Censure1 = 15927, // Boss->self, 3.0s cast, range 60 circle, activates nails for Breaking Wheel
    Censure2 = 15608, // Boss->self, 3.0s cast, range 60 circle, activates nails for Heretics Fork
    HereticForkWait = 17913, // Helper2->self, 6.5s cast, single-target, before long Heretics Fork
    HereticsFork1 = 15602, // Boss->self, 5.0s cast, range 60 width 10 cross
    HereticsFork2 = 15609, // RhapsodicNail->self, 8.0s cast, range 60 width 10 cross
    HereticsFork3 = 15886 // Boss->self, 23.0s cast, range 60 width 10 cross
}

sealed class IntestinalCrank(BossModule module) : Components.RaidwideCast(module, (uint)AID.IntestinalCrank);
sealed class BreakingWheel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreakingWheel1, new AOEShapeDonut(5f, 60f));
sealed class HereticsFork(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HereticsFork1, new AOEShapeCross(60f, 5f));
sealed class CrystalNail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrystalNail, 5f);

sealed class HereticsForkBreakingWheelStreak(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5f, 60f);
    private static readonly AOEShapeCross cross = new(60f, 5f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Sort((x, y) => x.Activation.CompareTo(y.Activation));
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.HereticsFork2:
            case (uint)AID.HereticsFork3:
                AddAOE(cross);
                break;
            case (uint)AID.BreakingWheel2:
            case (uint)AID.BreakingWheel3:
                AddAOE(donut);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HereticsFork2:
            case (uint)AID.BreakingWheel2:
            case (uint)AID.HereticsFork3:
            case (uint)AID.BreakingWheel3:
                if (_aoes.Count != 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }
}

sealed class D043StorgeStates : StateMachineBuilder
{
    public D043StorgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IntestinalCrank>()
            .ActivateOnEnter<CrystalNail>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<BreakingWheel>()
            .ActivateOnEnter<HereticsForkBreakingWheelStreak>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8249)]
public sealed class D043Storge(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Cross(new(196f, -95f), 20f, 14.5f)], AdjustForHitbox: true);
}
