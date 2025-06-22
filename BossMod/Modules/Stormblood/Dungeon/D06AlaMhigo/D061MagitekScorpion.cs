namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D061MagitekScorpion;

public enum OID : uint
{
    Boss = 0x1BA4, // R6.0
    Target = 0x1BA5,
    FireVoidzone = 0x1EA66D,
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 9303, // Boss->player, no cast, single-target

    ElectromagneticField = 8269, // Boss->self, 3.0s cast, range 40 circle
    TargetSearch = 8262, // Boss->self, 3.0s cast, single-target
    LockOn = 8263, // Helper->self, no cast, range 5 circle
    TailLaserVisual = 8264, // Boss->self, 2.0s cast, single-target
    TailLaserFrontFirst = 8265, // Helper->self, 3.0s cast, range 20+R width 10 rect
    TailLaserBackFirst = 8266, // Helper->self, 3.0s cast, range 20+R width 10 rect
    TailLaserFrontRest = 8267, // Helper->self, no cast, range 20+R width 10 rect
    TailLaserBackRest = 8268, // Helper->self, no cast, range 20+R width 10 rect
}

class TailLaser(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(20.5f, 5f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TailLaserBackFirst or (uint)AID.TailLaserFrontFirst)
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 1)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TailLaserFrontRest or (uint)AID.TailLaserBackRest)
        {
            if (++NumCasts == 12)
            {
                NumCasts = 0;
                _aoes.Clear();
            }
        }
    }
}

class TargetSearch(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var enemies = Module.Enemies((uint)OID.Target);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var aoes = new List<AOEInstance>(count);

        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState == 0)
                aoes.Add(new(circle, z.Position));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

class LockOn(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.LockOn, GetVoidzones, 0.7f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
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

class ElectromagneticField(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElectromagneticField);

class D061MagitekScorpionStates : StateMachineBuilder
{
    public D061MagitekScorpionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailLaser>()
            .ActivateOnEnter<ElectromagneticField>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<TargetSearch>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6037)]
public class D061MagitekScorpion(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-191, 72), 19.75f)], [new Rectangle(new(-210, 72), 1f, 20f), new Rectangle(new(-172.2f, 72), 1f, 20f)]);
}
