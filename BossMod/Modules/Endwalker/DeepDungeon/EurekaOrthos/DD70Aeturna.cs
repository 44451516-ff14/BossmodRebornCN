namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD70Aeturna;

public enum OID : uint
{
    Boss = 0x3D1B, // R5.95
    AllaganCrystal = 0x3D1C, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    FallingRock = 31441, // Helper->self, 2.5s cast, range 3 circle
    Ferocity = 31442, // Boss->self, 5.0s cast, single-target
    FerocityTetherStretchSuccess = 31443, // Boss->player, no cast, single-target
    FerocityTetherStretchFail = 31444, // Boss->player, no cast, single-target
    Impact = 31438, // AllaganCrystal->self, 2.5s cast, range 5 circle
    PreternaturalTurnCircle = 31436, // Boss->self, 6.0s cast, range 15 circle
    PreternaturalTurnDonut = 31437, // Boss->self, 6.0s cast, range 6-30 donut
    Roar = 31435, // Boss->self, 5.0s cast, range 60 circle
    ShatterCircle = 31439, // AllaganCrystal->self, 3.0s cast, range 8 circle
    ShatterCone = 31440, // AllaganCrystal->self, 2.5s cast, range 18+R 150-degree cone
    SteelClaw = 31445, // Boss->player, 5.0s cast, single-target
    Teleport = 31446 // Boss->location, no cast, single-target, boss teleports mid
}

class SteelClaw(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.SteelClaw);
class Ferocity(BossModule module) : Components.StretchTetherDuo(module, 15f, 5.7f);
class PreternaturalTurnCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PreternaturalTurnCircle, 15f);
class PreternaturalTurnDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PreternaturalTurnDonut, new AOEShapeDonut(6f, 30f));

class Shatter(BossModule module) : Components.GenericAOEs(module)
{
    private bool ferocityCasted;
    private readonly List<AOEInstance> _aoes = new(4);

    private static readonly AOEShapeCone cone = new(23.95f, 75.Degrees());
    private static readonly AOEShapeCircle circle = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOEs(AOEShape shape)
        {
            var crystals = Module.Enemies((uint)OID.AllaganCrystal);
            var count = crystals.Count;
            for (var i = 0; i < count; ++i)
            {
                var crystal = crystals[i];
                _aoes.Add(new(shape, crystal.Position.Quantized(), crystal.Rotation, Module.CastFinishAt(spell, 0.5f)));
            }
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.Ferocity:
                ferocityCasted = true;
                break;
            case (uint)AID.PreternaturalTurnCircle when !ferocityCasted:
                AddAOEs(cone);
                break;
            case (uint)AID.PreternaturalTurnDonut when !ferocityCasted:
                AddAOEs(circle);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ShatterCircle or (uint)AID.ShatterCone)
            _aoes.Clear();
        else if (spell.Action.ID is (uint)AID.PreternaturalTurnCircle or (uint)AID.PreternaturalTurnDonut)
            ferocityCasted = false;
    }
}

class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);
class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 3f);
class Impact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impact, 5f);

class DD70AeturnaStates : StateMachineBuilder
{
    public DD70AeturnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SteelClaw>()
            .ActivateOnEnter<Ferocity>()
            .ActivateOnEnter<PreternaturalTurnCircle>()
            .ActivateOnEnter<PreternaturalTurnDonut>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Impact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 903, NameID = 12246)]
public class DD70Aeturna(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -300f), new ArenaBoundsCircle(20f));
