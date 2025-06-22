namespace BossMod.Global.MaskedCarnivale.Stage19.Act2;

public enum OID : uint
{
    Boss = 0x2728, //R=5.775
    HotHip = 0x2779, //R=1.50
    Voidzone = 0x1EA9F9, //R=0.5
}

public enum AID : uint
{
    Reflect = 15073, // Boss->self, 3.0s cast, single-target, boss starts reflecting all melee attacks
    AutoAttack = 6499, // Boss->player, no cast, single-target
    VineProbe = 15075, // Boss->self, 2.5s cast, range 6+R width 8 rect
    OffalBreath = 15076, // Boss->location, 3.5s cast, range 6 circle
    Schizocarps = 15077, // Boss->self, 5.0s cast, single-target
    ExplosiveDehiscence = 15078, // HotHip->self, 6.0s cast, range 50 circle, gaze
    BadBreath = 15074, // Boss->self, 3.5s cast, range 12+R 120-degree cone, interruptible, voidzone
}

public enum SID : uint
{
    Blind = 15 // Boss->player, extra=0x0
}

class ExplosiveDehiscence(BossModule module) : Components.CastGaze(module, (uint)AID.ExplosiveDehiscence)
{
    public bool casting;
    public BitMask _blinded;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_blinded[slot] && casting)
            hints.Add("Cast Ink Jet on boss to get blinded!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Schizocarps)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosiveDehiscence)
            casting = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
            _blinded[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Blind)
            _blinded[Raid.FindSlot(actor.InstanceID)] = false;
    }

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        return _blinded[slot] ? [] : base.ActiveEyes(slot, actor);
    }
}

class Reflect(BossModule module) : BossComponent(module)
{
    private bool reflect;
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Reflect)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Reflect)
        {
            reflect = true;
            casting = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Boss will reflect all magic damage!");
        else if (reflect)
            hints.Add("Boss reflects all magic damage!"); // TODO: could use an AI hint to never use magic abilities after this is casted
    }
}

class BadBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BadBreath, new AOEShapeCone(17.775f, 60f.Degrees()));
class VineProbe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VineProbe, new AOEShapeRect(11.775f, 4f));
class OffalBreath(BossModule module) : Components.CastInterruptHint(module, (uint)AID.OffalBreath);
class OffalBreathVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.OffalBreath, GetVoidzones, 1.6f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Voidzone);
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

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as first act, but this time the boss will cast a gaze from all directions.\nThe easiest counter for this is to blind yourself by casting Ink Jet on the\nboss after it casted Schizocarps.\nThe Final Sting combo window opens at around 75% health.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage19Act2States : StateMachineBuilder
{
    public Stage19Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Reflect>()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<ExplosiveDehiscence>()
            .ActivateOnEnter<OffalBreath>()
            .ActivateOnEnter<OffalBreathVoidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 629, NameID = 8117, SortOrder = 2)]
public class Stage19Act2 : BossModule
{
    public Stage19Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
