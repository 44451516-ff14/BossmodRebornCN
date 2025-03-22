namespace BossMod.Dawntrail.Hunt.RankA.Heshuala;

public enum OID : uint
{
    Boss = 0x416E // R6.6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    HigherPower1 = 39095, // Boss->self, 2.0s cast, single-target, spin x3
    HigherPower2 = 39096, // Boss->self, 2.0s cast, single-target, spin x4
    HigherPower3 = 39097, // Boss->self, 2.0s cast, single-target, spin x5
    HigherPower4 = 39098, // Boss->self, 2.0s cast, single-target, spin x6

    SpinshockFirstCCW = 39100, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockFirstCW = 39099, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockRest = 39101, // Boss->self, 0.7s cast, range 50 90-degree cone
    ShockingCross = 39102, // Boss->self, 1.7s cast, range 50 width 10 cross, last spin rotation + 45°
    XMarksTheShock = 39103, // Boss->self, 1.7s cast, range 50 width 10 cross, last spin rotation + 90°
    LightningBolt = 39104, // Boss->location, 2.0s cast, range 5 circle
    ElectricalOverload = 39105, // Boss->self, 4.0s cast, range 50 circle
}

public enum SID : uint
{
    ElectricalCharge = 3979, // Boss->Boss, extra=0x4/0x3/0x2/0x1/0x6/0x5
    ShockingCross = 3977, // Boss->Boss, extra=0x0
    XMarksTheShock = 3978, // Boss->Boss, extra=0x0
    NumbingCurrent = 3980, // Boss->player, extra=0x0
}

class SpinShock(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(50f, 45f.Degrees());
    public int Spins;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var spincount = spell.Action.ID switch
        {
            (uint)AID.HigherPower1 => 3,
            (uint)AID.HigherPower2 => 4,
            (uint)AID.HigherPower3 => 5,
            (uint)AID.HigherPower4 => 6,
            _ => 0
        };
        if (spincount != 0)
            Spins = spincount;
        switch (spell.Action.ID)
        {
            case (uint)AID.SpinshockFirstCCW:
                AddSequence(90f.Degrees());
                break;
            case (uint)AID.SpinshockFirstCW:
                AddSequence(-90f.Degrees());
                break;
        }
        void AddSequence(Angle increment) => Sequences.Add(new(cone, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 2.7f, Spins));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpinshockFirstCCW or (uint)AID.SpinshockFirstCW or (uint)AID.SpinshockRest)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
            --Spins;
        }
    }
}

class ShockingCrossXMarksTheShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly SpinShock _rotation = module.FindComponent<SpinShock>()!;
    private bool currentShape; // false = intercards, true cardinal
    private static readonly AOEShapeCross _cross = new(50f, 5f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is AOEInstance aoe && _rotation.Spins < 3)
            return new AOEInstance[1] { aoe };
        else
            return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.XMarksTheShock:
                currentShape = false;
                break;
            case (uint)SID.ShockingCross:
                currentShape = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.XMarksTheShock or (uint)AID.ShockingCross)
        {
            _aoe = null;
        }
    }

    public override void Update()
    {
        if (_aoe == null && _rotation.Sequences.Count > 0)
        {
            var sequence = _rotation.Sequences[0];
            var rotationOffset = currentShape ? default : 45f.Degrees();
            var activation = WorldState.FutureTime(11.5d + _rotation.Spins * 2d);
            _aoe = new(_cross, sequence.Origin, sequence.Rotation + rotationOffset, activation);
        }
    }
}

class LightningBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightningBolt), 5f);
class ElectricalOverload(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ElectricalOverload));

class HeshualaStates : StateMachineBuilder
{
    public HeshualaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpinShock>()
            .ActivateOnEnter<ShockingCrossXMarksTheShock>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<ElectricalOverload>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13157)]
public class Heshuala(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
