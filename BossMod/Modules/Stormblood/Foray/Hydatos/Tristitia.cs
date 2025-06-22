namespace BossMod.Stormblood.Foray.Hydatos.Tristitia;

public enum OID : uint
{
    Boss = 0x2741, // R4.0
    Tristitia = 0x2740, // R6.0
}

public enum AID : uint
{
    AutoAttack1 = 14923, // Boss->player, no cast, single-target
    AutoAttack2 = 14919, // Tristitia->player, no cast, single-target

    WatergaIII = 14922, // Tristitia->location, 3.0s cast, range 8 circle
    SpineLash1 = 14924, // Boss->self, 3.0s cast, range 5+R 90-degree cone
    SpineLash2 = 14921, // Tristitia->self, 3.0s cast, range 5+R 90-degree cone
    ShockSpikes = 14920, // Tristitia->self, 5.0s cast, single-target, applies shock spike buff
    TornadoII1 = 15914, // Tristitia->self, 5.0s cast, range 5-40 donut
    TornadoII2 = 15915, // Tristitia->self, no cast, range 5-40 donut
    AerogaIV1 = 15912, // Tristitia->self, 3.5s cast, range 10 circle
    AerogaIV2 = 15913, // Tristitia->self, no cast, range 10 circle
    MightyStrikes = 15910, // Tristitia->self, 3.0s cast, single-target, applies critical strikes buff
    Meteor = 15911, // Tristitia->self, 5.0s cast, range 40 circle
    Dualcast = 15909 // Tristitia->self, 2.0s cast, single-target
}

public enum SID : uint
{
    ShockSpikes = 199, // Boss->Boss, extra=0x64
    CriticalStrikes = 1797 // Boss->Boss, extra=0x0
}

class WatergaIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WatergaIII, 8f);
class SpineLash1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpineLash1, new AOEShapeCone(9f, 45f.Degrees()));
class SpineLash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpineLash2, new AOEShapeCone(11f, 45f.Degrees()));
class ShockSpikes(BossModule module) : Components.Dispel(module, (uint)SID.ShockSpikes);
class MightyStrikes(BossModule module) : Components.Dispel(module, (uint)SID.CriticalStrikes);

class Meteor(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteor);

class TornadoIIAerogaIVDualCast(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private bool dualCast;
    public readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
                aoe.Risky = false;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape, float delay = default) => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        void AddAOEs(AOEShape shape1, AOEShape shape2)
        {
            AddAOE(shape1);
            AddAOE(shape2, 3.1f);
            dualCast = false;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.Dualcast:
                dualCast = true;
                break;
            case (uint)AID.TornadoII1:
                if (!dualCast)
                    AddAOE(donut);
                else
                    AddAOEs(donut, circle);
                break;
            case (uint)AID.AerogaIV1:
                if (!dualCast)
                    AddAOE(circle);
                else
                    AddAOEs(circle, donut);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.TornadoII1:
                case (uint)AID.TornadoII2:
                case (uint)AID.AerogaIV1:
                case (uint)AID.AerogaIV2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

class PhuaboStates : StateMachineBuilder
{
    public PhuaboStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpineLash1>()
            .Raw.Update = () =>
            {
                var adds = module.Enemies((uint)OID.Boss);
                var countadds = adds.Count;
                var areAddsDestroyed = true;
                for (var i = 0; i < countadds; ++i)
                {
                    if (!adds[i].IsDestroyed)
                        areAddsDestroyed = false;
                }
                var boss = module.Enemies((uint)OID.Tristitia);
                var countBoss = boss.Count;
                var isBossTargetable = false;
                for (var i = 0; i < countBoss; ++i)
                {
                    if (boss[i].IsTargetable)
                        isBossTargetable = true;
                }
                return areAddsDestroyed || isBossTargetable;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1422, SortOrder = 12)]
public class Phuabo(WorldState ws, Actor primary) : BASupportFate(ws, primary);

class TristitiaStates : StateMachineBuilder
{
    public TristitiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShockSpikes>()
            .ActivateOnEnter<MightyStrikes>()
            .ActivateOnEnter<TornadoIIAerogaIVDualCast>()
            .ActivateOnEnter<WatergaIII>()
            .ActivateOnEnter<TornadoIIAerogaIVDualCast>()
            .ActivateOnEnter<SpineLash2>()
            .ActivateOnEnter<Meteor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.EurekaNM, GroupID = 639, NameID = 1422, PrimaryActorOID = (uint)OID.Tristitia, SortOrder = 13)]
public class Tristitia(WorldState ws, Actor primary) : BASupportFate(ws, primary);

public abstract class BASupportFate(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.Tristitia];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(All));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss or (uint)OID.Tristitia => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
