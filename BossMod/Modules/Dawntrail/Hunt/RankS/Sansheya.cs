namespace BossMod.Dawntrail.Hunt.RankS.Sansheya;

public enum OID : uint
{
    Boss = 0x43DD // R4.0
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/4379->player, no cast, single-target

    CullingBlade = 39295, // Boss->self, 4.0s cast, range 80 circle
    PyreOfRebirth = 39288, // Boss->self, 4.0s cast, range 32 circle, status effect boiling, turns into pyretic
    FiresDomain = 39285, // Boss->players, 5.0s cast, width 6 rect charge
    TwinscorchedVeil1 = 39292, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then circle
    TwinscorchedVeil2 = 39290, // Boss->self, 5.0s cast, range 40 180-degree cone, visual left then right then circle
    TwinscorchedHalo1 = 39289, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, left then right then donut
    TwinscorchedHalo2 = 39291, // Boss->self, 5.0s cast, range 40 180-degree cone, visual, right then left then donut
    Twinscorch1 = 39601, // Boss->self, 4.0s cast, range 40 180-degree cone, left then right
    Twinscorch2 = 39602, // Boss->self, 4.0s cast, range 40 180-degree cone, right then left
    ScorchingLeft1 = 39528, // Boss->self, 1.0s cast, range 40 180-degree cone
    ScorchingLeft2 = 39557, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight1 = 39558, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRight2 = 39529, // Boss->self, 1.0s cast, range 40 180-degree cone
    VeilOfHeat1 = 39283, // Boss->self, 5.0s cast, range 15 circle
    VeilOfHeat2 = 39293, // Boss->self, 1.0s cast, range 15 circle
    CaptiveBolt = 39296, // Boss->players, 5.0s cast, range 6 circle, stack
    HaloOfHeat1 = 39284, // Boss->self, 5.0s cast, range 10-40 donut
    HaloOfHeat2 = 39294 // Boss->self, 1.0s cast, range 10-40 donut
}

public enum SID : uint
{
    Boiling = 4140, // Boss->player, extra=0x0
    Pyretic = 960 // none->player, extra=0x0
}

sealed class Boiling(BossModule module) : Components.StayMove(module)
{
    private BitMask _boiling;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Boiling && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _boiling[Raid.FindSlot(actor.InstanceID)] = true;
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            if (status.ID == (uint)SID.Boiling)
                _boiling[Raid.FindSlot(actor.InstanceID)] = false;
            else if (status.ID == (uint)SID.Pyretic)
                PlayerStates[slot] = default;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_boiling[slot])
        {
            var remaining = (PlayerStates[slot].Activation - WorldState.CurrentTime).TotalSeconds;
            hints.Add($"Boiling on you in {remaining:f1}s. (Pyretic!)", remaining < 3d);
        }
    }
}

sealed class TwinscorchedHaloVeil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(10f, 40f);
    private static readonly AOEShapeCircle circle = new(15f);
    private readonly List<AOEInstance> _aoes = new(3);

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
            else if (aoe.Shape == aoes[0].Shape)
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwinscorchedVeil1:
            case (uint)AID.TwinscorchedVeil2:
                AddAOEs(circle);
                break;
            case (uint)AID.TwinscorchedHalo1:
            case (uint)AID.TwinscorchedHalo2:
                AddAOEs(donut);
                break;
            case (uint)AID.Twinscorch1:
            case (uint)AID.Twinscorch2:
                AddAOEs(null);
                break;
        }
        void AddAOEs(AOEShape? secondaryShape)
        {
            var position = spell.LocXZ;
            var rotation = spell.Rotation;
            AddAOE(cone);
            AddAOE(cone, 180f.Degrees(), 2.3f);
            if (secondaryShape != null)
                AddAOE(secondaryShape, default, 4.5f);
            void AddAOE(AOEShape shape, Angle offset = default, float delay = default) => _aoes.Add(new(shape, position, rotation + offset, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.HaloOfHeat2:
                case (uint)AID.VeilOfHeat2:
                case (uint)AID.ScorchingLeft1:
                case (uint)AID.ScorchingLeft2:
                case (uint)AID.ScorchingRight1:
                case (uint)AID.ScorchingRight2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

sealed class HaloOfHeat1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HaloOfHeat1, new AOEShapeDonut(10f, 40f));
sealed class VeilOfHeat1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeilOfHeat1, 15f);
sealed class FiresDomain(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.FiresDomain, 3f);
sealed class CaptiveBolt(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.CaptiveBolt, 6f, 8);
sealed class PyreOfRebirthCullingBlade(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PyreOfRebirth, (uint)AID.CullingBlade]);

sealed class SansheyaStates : StateMachineBuilder
{
    public SansheyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HaloOfHeat1>()
            .ActivateOnEnter<VeilOfHeat1>()
            .ActivateOnEnter<TwinscorchedHaloVeil>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<PyreOfRebirthCullingBlade>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<Boiling>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13399)]
public sealed class Sansheya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
