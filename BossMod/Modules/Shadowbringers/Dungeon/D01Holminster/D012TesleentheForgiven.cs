namespace BossMod.Shadowbringers.Dungeon.D01Holminster.D012TesleentheForgiven;

public enum OID : uint
{
    Boss = 0x278B, // R1.8
    HolyWaterVoidzone = 0x1EABF9, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    TheTickler = 15823, // Boss->player, 4.0s cast, single-target, tankbuster
    ScoldsBridle = 15824, // Boss->self, 4.0s cast, range 40 circle, raidwide
    FeveredFlagellation1 = 15829, // Boss->self, 8.0s cast, single-target
    FeveredFlagellation2 = 15830, // Boss->players, no cast, width 4 rect charge, limit cut mechanic
    ExorciseA = 15826, // Boss->none, 5.0s cast, single-target
    ExorciseB = 15827, // Boss->location, no cast, range 6 circle
    HolyWaterVoidzones = 15825, // Boss->self, no cast, single-target
    HolyWater = 15828 // Helper->location, 7.0s cast, range 6 circle
}

public enum IconID : uint
{
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82 // player
}

class TheTickler(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TheTickler);
class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScoldsBridle);

class FeveredFlagellation(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(default, 2);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && spell.Action.ID == (uint)AID.FeveredFlagellation2)
            CurrentBaits.RemoveAt(0);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Icon1 and <= (uint)IconID.Icon4)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, rect));
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            CurrentBaits[i] = b with { Shape = rect with { LengthFront = (b.Target.Position - b.Source.Position).Length() } };
        }
    }
}

class Exorcise(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ExorciseA, 6f, 4, 4);
class HolyWater(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.HolyWater, GetVoidzones, 0.8f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HolyWaterVoidzone);
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

class D012TesleentheForgivenStates : StateMachineBuilder
{
    public D012TesleentheForgivenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheTickler>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<Exorcise>()
            .ActivateOnEnter<HolyWater>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8300)]
public class D012TesleentheForgiven(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(78f, -82f), 19.5f)], [new Rectangle(new(78f, -62f), 20f, 1f), new Rectangle(new(78f, -102f), 20f, 1f)]);
}
