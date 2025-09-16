namespace BossMod.Stormblood.Dungeon.D13TheBurn.D133MistDragon;

public enum OID : uint
{
    Boss = 0x2431, // R3.0-7.0
    IceVoidzone = 0x1E8713,
    DraconicRegard = 0x2432, // R3.0
    Mist = 0x2433, // R2.16
    Helper2 = 0x2434, // R7.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    RimeWreath = 12619, // Boss->self, 5.0s cast, range 40 circle
    FrostBreath = 12620, // Boss->self, no cast, range 20 90-degree cone
    FogPlumeVisual = 12612, // Boss->self, 3.0s cast, single-target
    FogPlumeCircle = 12613, // Helper->self, 3.0s cast, range 6 circle
    FogPlumeCross = 12614, // Helper->self, 1.5s cast, range 40 width 5 cross
    Vaporize = 12608, // Boss->self, 4.0s cast, single-target
    ColdFogVisual = 12609, // Boss->self, 24.0s cast, range 4 circle
    ColdFog = 12610, // Helper->self, no cast, range 4 circle
    WhiteDeath = 12611, // Helper->player, no cast, single-target
    ChillingAspiration = 12621, // Boss->self, no cast, range 40 width 6 rect
    DeepFog = 12615, // Boss->self, 4.0s cast, range 40 circle
    CauterizeVisual = 12616, // Helper2->self, no cast, range 40 width 16 rect
    Cauterize = 12617, // Helper->self, 0.7s cast, range 40 width 16 rect
    Touchdown = 12618 // Boss->self, 6.0s cast, range 40 circle
}

public enum SID : uint
{
    AreaOfInfluenceUp = 618 // Boss->Boss, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xC/0xD/0xE/0xF/0x10
}

public enum IconID : uint
{
    BaitawayCone = 26, // player
    BaitawayRect = 14 // player
}

sealed class FogPlumeCross(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCross cross = new(40f, 2.5f);
    private static readonly Angle[] angles = [Angle.AnglesCardinals[1], Angle.AnglesIntercardinals[1]];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FogPlumeCircle)
        {
            for (var i = 0; i < 2; ++i)
            {
                _aoes.Add(new(cross, spell.LocXZ, angles[i], Module.CastFinishAt(spell, 3.6d)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FogPlumeCross)
        {
            _aoes.Clear();
        }
    }
}

sealed class FogPlumeCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FogPlumeCircle, 6f);

sealed class ColdFog(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private DateTime activation;
    private bool reset;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AreaOfInfluenceUp)
        {
            _aoe = [new(new AOEShapeCircle(4f + status.Extra), Arena.Center.Quantized(), default, activation)];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ColdFogVisual)
        {
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ColdFog)
        {
            _aoe = [];
            reset = false;
        }
    }

    public override void Update()
    {
        if (_aoe.Length != 0 && !reset)
        {
            var enemies = Module.Enemies((uint)OID.DraconicRegard);
            var count = enemies.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!enemies[i].IsDead)
                {
                    return;
                }
            }
            ref var aoe = ref _aoe[0];
            aoe.Activation = WorldState.FutureTime(5.6d);
            reset = true;
        }
    }
}

abstract class BaitAway(BossModule module) : Components.GenericBaitAway(module)
{
    protected static readonly AOEShapeCircle circle = new(6f);
    protected static readonly AOEShapeCone cone = new(20f, 45f.Degrees());
    protected static readonly AOEShapeRect rect = new(40f, 3f);

    protected void DrawPositionsInBounds(WPos[] positions)
    {
        var len = positions.Length;
        var center = Arena.Center;
        for (var i = 0; i < len; ++i)
        {
            var position = positions[i];
            if (position.InCircle(center, 20f))
            {
                circle.Outline(Arena, position);
            }
        }
    }

    protected static WPos[] CalculatePositions(Actor boss, Actor target, int count = 5)
    {
        var positions = new WPos[count];
        for (var i = 0; i < count; ++i)
        {
            positions[i] = CalculatePosition(boss, target, boss.HitboxRadius + i * 9f);
        }
        return positions;
    }

    protected static WPos CalculatePosition(Actor boss, Actor target, float distance) => boss.Position + distance * boss.DirectionTo(target);

    protected static WPos[] CalculateRotatedPositions(Actor boss, Actor target)
    {
        return
        [
            CalculatePosition(boss, target, 7f),
            CalculatePosition(boss, target, 15f),
            CalculateRotatedPosition(boss, target, 15f, 30f),
            CalculateRotatedPosition(boss, target, 15f, -30f)
        ];
    }

    private static WPos CalculateRotatedPosition(Actor boss, Actor target, float distance, float angle) => boss.Position + (distance * boss.DirectionTo(target)).Rotate(angle.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.Add("Bait away!");
        }
    }

    protected void DrawBaitsOnActor(int pcSlot, Actor pc, Action<Actor> drawAction)
    {
        if (ActiveBaitsOn(pc).Count == 0)
        {
            return;
        }

        base.DrawArenaForeground(pcSlot, pc);
        drawAction(pc);
    }

    protected void DrawBaitsOnOther(int pcSlot, Actor pc, Action<Actor> drawAction)
    {
        if (ActiveBaitsOn(pc).Count == 0)
        {
            return;
        }

        base.DrawArenaBackground(pcSlot, pc);
        var baits = ActiveBaitsNotOn(pc);
        var count = baits.Count;
        for (var i = 0; i < count; ++i)
        {
            drawAction(baits[i].Target);
        }
    }
}

sealed class ChillingAspiration(BossModule module) : BaitAway(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BaitawayRect)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect, Module.WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChillingAspiration)
        {
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawBaitsOnActor(pcSlot, pc, target =>
        {
            var boss = Module.PrimaryActor;
            DrawPositionsInBounds(CalculatePositions(boss, target));
        });
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        DrawBaitsOnOther(pcSlot, pc, target =>
        {
            var boss = Module.PrimaryActor;
            DrawPositionsInBounds(CalculatePositions(boss, target));
        });
    }
}

sealed class FrostBreath(BossModule module) : BaitAway(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BaitawayCone)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, cone, Module.WorldState.FutureTime(4.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FrostBreath)
            CurrentBaits.Clear();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawBaitsOnActor(pcSlot, pc, target =>
        {
            var positions = CalculateRotatedPositions(Module.PrimaryActor, target);
            DrawPositionsInBounds(positions);
        });
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        DrawBaitsOnOther(pcSlot, pc, target =>
        {
            var positions = CalculateRotatedPositions(Module.PrimaryActor, target);
            DrawPositionsInBounds(positions);
        });
    }
}

sealed class RimeWreath(BossModule module) : Components.RaidwideCast(module, (uint)AID.RimeWreath);
sealed class TouchDown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Touchdown, 15f);
sealed class IceVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.IceVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class Cauterize(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(40f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Helper2)
        {
            _aoe = [new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(7.3d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Cauterize)
        {
            _aoe = [];
        }
    }
}

sealed class D133MistDragonStates : StateMachineBuilder
{
    public D133MistDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FogPlumeCircle>()
            .ActivateOnEnter<FogPlumeCross>()
            .ActivateOnEnter<ColdFog>()
            .ActivateOnEnter<ChillingAspiration>()
            .ActivateOnEnter<RimeWreath>()
            .ActivateOnEnter<TouchDown>()
            .ActivateOnEnter<IceVoidzone>()
            .ActivateOnEnter<FrostBreath>()
            .ActivateOnEnter<Cauterize>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 585, NameID = 7672)]
public sealed class D133MistDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300f, -400f), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DraconicRegard));
        Arena.Actors(Enemies((uint)OID.Mist));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Mist => 2,
                (uint)OID.DraconicRegard => 1,
                _ => 0
            };
        }
    }
}
