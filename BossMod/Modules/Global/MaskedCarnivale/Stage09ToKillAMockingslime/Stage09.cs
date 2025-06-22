namespace BossMod.Global.MaskedCarnivale.Stage09;

public enum OID : uint
{
    Boss = 0x242D, //R=2.4
    Pudding = 0x2711, //R=1.8
    Gelato = 0x2712, //R=1.8
    Marshmallow = 0x2713, //R=1.8
    Licorice = 0x2714, //R=1.8
    Bavarois = 0x2715, //R=1.8
    Flan = 0x2716, //R=1.8
    DarkVoidzone = 0x1E9C9D //R=0.5
}

public enum AID : uint
{
    DeathRay = 15056, // Boss->player, 1.0s cast, single-target
    Dark = 15057, // Boss->location, 3.0s cast, range 5 circle, creates a voidzone with radius 4
    GoldenTongue = 14265, // Boss/Marshmallow/Licorice/Bavarois->self, 5.0s cast, single-target
    Fire = 14266, // Pudding->player, 1.0s cast, single-target
    Blizzard = 14267, // Gelato->player, 1.0s cast, single-target
    Aero = 14269, // Marshmallow->player, 1.0s cast, single-target
    Stone = 14270, // Licorice->player, 1.0s cast, single-target
    Thunder = 14268, // Bavarois->player, 1.0s cast, single-target
    Water = 14271 // Flan->player, 1.0s cast, single-target
}

class GoldenTongue(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldenTongue);
class DarkVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 4f, (uint)AID.Dark, GetVoidzones, 1f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.DarkVoidzone);
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

class Dark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dark, 5f);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} summons a total of 6 adds during the fight, one of each element.\nHealer mimikry can be helpful if you have trouble surviving.");
    }
}

class Stage09States : StateMachineBuilder
{
    public Stage09States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<DarkVoidzone>()
            .ActivateOnEnter<GoldenTongue>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 619, NameID = 8099)]
public class Stage09 : BossModule
{
    public Stage09(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
    private static readonly uint[] adds = [(uint)OID.Licorice, (uint)OID.Flan, (uint)OID.Pudding, (uint)OID.Marshmallow, (uint)OID.Bavarois, (uint)OID.Gelato];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds), Colors.Object);
    }
}
