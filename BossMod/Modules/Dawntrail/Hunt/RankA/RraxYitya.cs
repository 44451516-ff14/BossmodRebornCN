namespace BossMod.Dawntrail.Hunt.RankA.RraxYitya;

public enum OID : uint
{
    Boss = 0x4232 // R5.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    RightWingblade1 = 37164, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade1 = 37165, // Boss->self, 3.0s cast, range 25 90 degree cone
    RightWingblade2 = 37166, // Boss->self, 3.0s cast, range 25 90 degree cone
    LeftWingblade2 = 37167, // Boss->self, 3.0s cast, range 25 90 degree cone
    LaughingLeap = 37372, // Boss->self, 4.0s cast, range 15 width 5 rect
    TriplicateReflex = 37170, // Boss->self, 5.0s cast, single-target
    RightWingbladeRepeat = 37171, // Boss->self, no cast, range 25 90 degree cone
    LeftWingbladeRepeat = 37172 // Boss->self, no cast, range 25 90 degree cone
}

class LaughingLeap(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LaughingLeap), new AOEShapeRect(15f, 2.5f));

class Wingblade(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly List<Angle> offsets = new(3);
    private static readonly Angle a180 = 180f.Degrees(), a90 = 90f.Degrees();
    private static readonly AOEShapeCone cone = new(25f, a90);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = offsets.Count;
        if (count != 0)
        {
            var sequenceBuilder = new StringBuilder("Sequence: ", 33);
            for (var i = 0; i < count; ++i)
            {
                if (i > 0)
                    sequenceBuilder.Append(" -> ");
                sequenceBuilder.Append(offsets[i].Rad < 0 ? "Right" : "Left");
            }
            hints.Add(sequenceBuilder.ToString());
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(Angle offset = default, float delay = default) => _aoes.Add(new(cone, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        switch (spell.Action.ID)
        {
            case (uint)AID.RightWingblade1:
                offsets.Clear();
                offsets.Add(-a90);
                AddAOE();
                break;
            case (uint)AID.LeftWingblade1:
                offsets.Clear();
                offsets.Add(a90);
                AddAOE();
                break;
            case (uint)AID.RightWingblade2:
                offsets.Add(-a90);
                AddAOE();
                break;
            case (uint)AID.LeftWingblade2:
                offsets.Add(a90);
                AddAOE();
                break;
            case (uint)AID.TriplicateReflex:
                var count = offsets.Count;
                for (var i = 0; i < count; ++i)
                    AddAOE(offsets[i], 0.4f + i * 2f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.RightWingblade1:
                case (uint)AID.LeftWingblade1:
                case (uint)AID.LeftWingblade2:
                case (uint)AID.RightWingblade2:
                    _aoes.RemoveAt(0);
                    break;
                case (uint)AID.RightWingbladeRepeat:
                case (uint)AID.LeftWingbladeRepeat:
                    _aoes.RemoveAt(0);
                    if (offsets.Count != 0)
                        offsets.RemoveAt(0);
                    break;
            }
    }
}

class RraxYityaStates : StateMachineBuilder
{
    public RraxYityaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Wingblade>()
            .ActivateOnEnter<LaughingLeap>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12753)]
public class RraxYitya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
