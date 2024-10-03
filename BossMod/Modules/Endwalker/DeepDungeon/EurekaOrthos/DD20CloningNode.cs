namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD20CloningNode;

public enum OID : uint
{
    Boss = 0x3E77, // R6.0
    ClonedShieldDragon = 0x3E78, // R3.445
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32817, // Boss->player, no cast, single-target
    Teleport = 32554, // ClonedShieldDragon->location, no cast, single-target

    OffensiveCommand = 32543, // Boss->self, 5.0s cast, single-target
    FlameBreathVisual1 = 32544, // ClonedShieldDragon->self, 2.0+1,5s cast, single-target
    FlameBreathVisual2 = 32553, // ClonedShieldDragon->self, no cast, single-target
    FlameBreathVisual3 = 32552, // ClonedShieldDragon->self, no cast, single-target
    FlameBreath = 32864, // Helper->self, 3.5s cast, range 50 30-degree cone
    PiercingLaser = 32547, // Boss->self, 2.5s cast, range 40 width 5 rect
    OrderRelay = 32545 // Boss->self, 8.0s cast, single-target
}

class FlameBreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(50, 15.Degrees());
    private static readonly float intercardinalDistance = 16 * MathF.Sqrt(2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            foreach (var a in _aoes)
                if ((a.Activation - _aoes[0].Activation).TotalSeconds <= 1)
                    yield return a;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlameBreathVisual3)
        {
            var activation = WorldState.FutureTime(9.6f);

            if ((caster.Position - Arena.Center).LengthSq() > 625)
                _aoes.Add(new(cone, CalculatePosition(caster), caster.Rotation, activation));
            else
                _aoes.Add(new(cone, RoundPosition(caster.Position), caster.Rotation, activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.FlameBreath)
            _aoes.RemoveAt(0);
    }

    private static WPos CalculatePosition(Actor caster)
    {
        var isIntercardinal = IsCasterIntercardinal(caster);
        var distance = isIntercardinal ? intercardinalDistance : 22;
        var position = caster.Position + distance * caster.Rotation.ToDirection();
        return RoundPosition(position);
    }

    private static bool IsCasterIntercardinal(Actor caster)
    {
        foreach (var angle in Helpers.AnglesIntercardinals)
            if (caster.Rotation.AlmostEqual(angle, Angle.DegToRad))
                return true;
        return false;
    }

    private static WPos RoundPosition(WPos position) => new(MathF.Round(position.X * 2) * 0.5f, MathF.Round(position.Z * 2) * 0.5f);
}

class PiercingLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PiercingLaser), new AOEShapeRect(40, 2.5f));

class DD20CloningNodeStates : StateMachineBuilder
{
    public DD20CloningNodeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlameBreath>()
            .ActivateOnEnter<PiercingLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 898, NameID = 12261)]
public class DD20CloningNode(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(19.5f));
