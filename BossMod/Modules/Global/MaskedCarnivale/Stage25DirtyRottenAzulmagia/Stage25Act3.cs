namespace BossMod.Global.MaskedCarnivale.Stage25.Act3;

public enum OID : uint
{
    Boss = 0x2680, //R=1.2
    Maelstrom = 0x2681, //R=0.6
    LavaVoidzone = 0x1EA8BB,
    Helper = 0x233C
}

public enum AID : uint
{
    RepellingSpray = 14768, // Boss->self, 2.0s cast, single-target, boss reflect magic attacks
    IceSpikes = 14762, // Boss->self, 2.0s cast, single-target, boss reflects physical attacks
    ApocalypticBolt = 14766, // Boss->self, 3.0s cast, range 50+R width 8 rect
    TheRamsVoice = 14763, // Boss->self, 3.5s cast, range 8 circle
    TheDragonsVoice = 14764, // Boss->self, 3.5s cast, range 6-30 donut
    ApocalypticRoar = 14767, // Boss->self, 5.0s cast, range 35+R 120-degree cone
    Charybdis = 14772, // Boss->self, 3.0s cast, single-target
    Charybdis2 = 14773, // Helper->self, 4.0s cast, range 8 circle
    Maelstrom = 14780, // Maelstrom->self, 1.0s cast, range 8 circle
    Web = 14770, // Boss->player, 3.0s cast, single-target
    Meteor = 14771, // Boss->location, 7.0s cast, range 15 circle
    Plaincracker = 14765, // Boss->self, 3.5s cast, range 6+R circle
    TremblingEarth = 14774, // Helper->self, 3.5s cast, range 10-20 donut
    TremblingEarth2 = 14775 // Helper->self, 3.5s cast, range 20-30 donut
}

public enum SID : uint
{
    RepellingSpray = 556, // Boss->Boss, extra=0x64
    IceSpikes = 1307, // Boss->Boss, extra=0x64
    Doom = 910 // Boss->player, extra=0x0
}

class Charybdis(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Charybdis2, 8f);

class Web(BossModule module) : BossComponent(module)
{
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Web)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Web)
            casting = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (casting)
            hints.Add("Bait the Meteor to the edge of the arena!\nUse Loom to escape or Diamondback to survive.");
    }
}

class Plaincracker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plaincracker, 7.2f);
class TremblingEarth(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TremblingEarth, new AOEShapeDonut(10f, 20f));
class TremblingEarth2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TremblingEarth2, new AOEShapeDonut(20f, 30f));
class ApocalypticBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ApocalypticBolt, new AOEShapeRect(51.2f, 4f));
class ApocalypticRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ApocalypticRoar, new AOEShapeCone(36.2f, 60f.Degrees()));
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 8f);
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(6f, 30f));
class Maelstrom(BossModule module) : Components.Voidzone(module, 8f, GetMaelstrom)
{
    private static List<Actor> GetMaelstrom(BossModule module) => module.Enemies((uint)OID.Maelstrom);
}
class Meteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Meteor, 15f);
class MeteorVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 10f, (uint)AID.Meteor, GetVoidzones, 1.2f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.LavaVoidzone);
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
        hints.Add($"In this act {Module.PrimaryActor.Name} will switch between magic and physical reflects.\nSpend attention to that so you don't accidently kill yourself.\nAs soon as he starts casting Web go to the edge to bait Meteor, then use Loom\nto escape. You can start the Final Sting combination at about 50% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.RepellingSpray) != null)
            hints.Add($"{Module.PrimaryActor.Name} will reflect all magic damage!");
        if (Module.PrimaryActor.FindStatus((uint)SID.IceSpikes) != null)
            hints.Add($"{Module.PrimaryActor.Name} will reflect all physical damage!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Doom) != null)
            hints.Add("You were doomed! Cleanse it with Exuviation or finish the act fast.");
    }
}

class Stage25Act3States : StateMachineBuilder
{
    public Stage25Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ApocalypticBolt>()
            .ActivateOnEnter<ApocalypticRoar>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Plaincracker>()
            .ActivateOnEnter<TremblingEarth>()
            .ActivateOnEnter<TremblingEarth2>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<MeteorVoidzone>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Web>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 635, NameID = 8129, SortOrder = 3)]
public class Stage25Act3 : BossModule
{
    public Stage25Act3(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }
}
