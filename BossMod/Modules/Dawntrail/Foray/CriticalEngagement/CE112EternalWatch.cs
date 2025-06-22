namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE112EternalWatch;

public enum OID : uint
{
    Boss = 0x46CC, // R7.42
    HolySphere = 0x46CD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    RadiantWaveVisual = 41279, // Boss->self, 5.0s cast, single-target
    RadiantWave = 41345, // Helper->self, no cast, ???
    AncientHolyVisual = 41282, // Boss->self, no cast, single-target

    AncientHoly1 = 41284, // Helper->location, 11.0s cast, ???, proximity AOE, multiple casters to circumvent AOE limit
    AncientHoly2 = 41395, // Helper->location, 11.0s cast, ???

    AetherialExchange = 41280, // Boss->self, 3.0+1,0s cast, single-target
    AetherialExchangeStone = 41276, // Helper->HolySphere, 1.0s cast, single-target
    AetherialExchangeWind = 41281, // Helper->HolySphere, 1.0s cast, single-target

    AncientStoneIIIVisual = 41288, // Boss->self, 4.0+1,0s cast, single-target
    AncientStoneIII1 = 41289, // Helper->self, 5.0s cast, range 40 60-degree cone
    AncientStoneIII2 = 41293, // Helper->self, 12.0s cast, range 40 60-degree cone
    AncientAeroIIIVisual = 41286, // Boss->self, 4.0+1,0s cast, single-target
    AncientAeroIII1 = 41287, // Helper->self, 5.0s cast, range 40 60-degree cone
    AncientAeroIII2 = 41292, // Helper->self, 12.0s cast, range 40 60-degree cone

    SandSurge = 41296, // HolySphere->self, 2.0s cast, range 15 circle
    WindSurge = 41295, // HolySphere->self, 2.0s cast, range 15 circle
    LightSurge = 41294, // HolySphere->self, 2.0s cast, range 15 circle

    HolyBlaze = 41297, // Boss->self, 4.0s cast, range 60 width 5 rect
    Scratch = 41301, // Boss->player, 5.0s cast, single-target, tankbuster
    Fissure = 41283, // Boss->self, no cast, single-target

    DoubleCast1 = 41291, // Boss->self, 11.5+0,5s cast, single-target
    DoubleCast2 = 41290 // Boss->self, 11.5+0,5s cast, single-target
}

public enum SID : uint
{
    SphereStatus = 2536 // Helper->HolySphere, extra=0x225/0x224, 224 wind, 225 stone
}

sealed class AncientStoneAncientAeroIIISlow : Components.SimpleAOEGroups
{
    public AncientStoneAncientAeroIIISlow(BossModule module) : base(module, [(uint)AID.AncientAeroIII2, (uint)AID.AncientStoneIII2], WindStoneSurge.Cone, 4)
    {
        MaxDangerColor = 2;
    }
}
sealed class AncientStoneAncientAeroIIIFast(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AncientAeroIII1, (uint)AID.AncientStoneIII1], WindStoneSurge.Cone);
sealed class AncientHoly(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientHoly1, 26f);
sealed class RadiantWave(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.RadiantWaveVisual, (uint)AID.RadiantWave, 1.2f);
sealed class Scratch(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Scratch);
sealed class HolyBlaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyBlaze, new AOEShapeRect(60f, 2.5f));

sealed class LightSurge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(15f);
    private readonly List<Actor> spheres = new(12);
    private readonly AncientHoly _aoe = module.FindComponent<AncientHoly>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AncientHoly1 && _aoes.Count == 0 && spheres.Count == 4)
        {
            var count = spheres.Count;
            var act = Module.CastFinishAt(spell, 2.4f);
            for (var i = 0; i < count; ++i)
            {
                var sphere = spheres[i];
                _aoes.Add(new(circle, WPos.ClampToGrid(sphere.Position), default, act, ActorID: sphere.InstanceID));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightSurge)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.HolySphere)
        {
            spheres.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.HolySphere)
        {
            var count = spheres.Count;
            for (var i = 0; i < count; ++i)
            {
                if (spheres[i] == actor)
                {
                    spheres.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.SphereStatus)
        {
            var count = spheres.Count;
            for (var i = 0; i < count; ++i)
            {
                if (spheres[i] == actor)
                {
                    spheres.RemoveAt(i);
                    if (spheres.Count == 4 && _aoe.Casters.Count != 0)
                    {
                        var countS = spheres.Count;
                        for (var j = 0; j < countS; ++j)
                        {
                            var sphere = spheres[j];
                            _aoes.Add(new(circle, WPos.ClampToGrid(sphere.Position), default, _aoe.Casters[0].Activation, ActorID: sphere.InstanceID));
                        }
                    }
                    return;
                }
            }
        }
    }
}

sealed class WindStoneSurge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeCircle circle = new(15f);
    public static readonly AOEShapeCone Cone = new(40f, 30f.Degrees());
    private readonly List<Actor> spheresStone = new(6);
    private readonly List<Actor> spheresWind = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 4 ? 4 : count;
        if (count > 3 && aoes[0].Activation != aoes[3].Activation)
        {
            for (var i = 0; i < 2; ++i)
            {
                aoes[i].Color = Colors.Danger;
            }
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.AncientAeroIII1 or (uint)AID.AncientAeroIII2)
        {
            var count = spheresWind.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                var sphere = spheresWind[i];
                if (Cone.Check(sphere.Position, spell.LocXZ, spell.Rotation))
                {
                    AddAOE(sphere.Position, sphere.InstanceID);
                    spheresWind.RemoveAt(i);
                }
            }
        }
        else if (id is (uint)AID.AncientStoneIII1 or (uint)AID.AncientStoneIII2)
        {
            var count = spheresStone.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                var sphere = spheresStone[i];
                if (Cone.Check(sphere.Position, spell.LocXZ, spell.Rotation))
                {
                    AddAOE(sphere.Position, sphere.InstanceID);
                    spheresStone.RemoveAt(i);
                }
            }
        }
        void AddAOE(WPos position, ulong instanceID) => _aoes.Add(new(circle, WPos.ClampToGrid(position), default, Module.CastFinishAt(spell, 2.6f), ActorID: instanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WindSurge or (uint)AID.SandSurge)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.SphereStatus)
        {
            var extra = status.Extra;
            if (extra == 0x224u)
            {
                spheresWind.Add(actor);
            }
            else if (extra == 0x225u)
            {
                spheresStone.Add(actor);
            }
        }
    }
}

sealed class CE112EternalWatchStates : StateMachineBuilder
{
    public CE112EternalWatchStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientStoneAncientAeroIIISlow>()
            .ActivateOnEnter<AncientStoneAncientAeroIIIFast>()
            .ActivateOnEnter<AncientHoly>()
            .ActivateOnEnter<RadiantWave>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<HolyBlaze>()
            .ActivateOnEnter<LightSurge>()
            .ActivateOnEnter<WindStoneSurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 46)]
public sealed class CE112EternalWatch(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(870.1f, 180f), 24.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
