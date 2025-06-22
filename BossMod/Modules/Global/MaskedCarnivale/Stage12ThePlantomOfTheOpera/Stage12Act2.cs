namespace BossMod.Global.MaskedCarnivale.Stage12.Act2;

public enum OID : uint
{
    Boss = 0x271B, //R=6.96
    Roselet = 0x271C //R=0.8
}

public enum AID : uint
{
    WildHorn = 14751, // Boss->self, 3.5s cast, range 10+R 120-degree cone
    SporeSac = 14752, // Boss->self, 3.0s cast, range 50 circle
    Seedvolley = 14750, // Roselet->player, no cast, single-target
    Trounce = 14754, // Boss->self, 4.5s cast, range 40+R 60-degree cone
    InflammableFumes = 14753 // Boss->self, 15.0s cast, range 50 circle
}

class WildHorn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WildHorn, new AOEShapeCone(16.96f, 60f.Degrees()));
class Trounce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Trounce, new AOEShapeCone(46.96f, 30f.Degrees()));
class SporeSac(BossModule module) : Components.CastHint(module, (uint)AID.SporeSac, "Calls Roselets. Prepare Ice Spikes if available.");
class InflammableFumes(BossModule module) : Components.CastInterruptHint(module, (uint)AID.InflammableFumes, false, true);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Use Bomb Toss to stun {Module.PrimaryActor.Name} when he casts Inflammable Fumes.\nUse Ice Spikes to instantly kill roselets once they become aggressive.\n{Module.PrimaryActor.Name} is weak against water and strong against earth spells.");
    }
}

class Stage12Act2States : StateMachineBuilder
{
    public Stage12Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildHorn>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<InflammableFumes>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 622, NameID = 8102, SortOrder = 2)]
public class Stage12Act2 : BossModule
{
    public Stage12Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Roselet), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Roselet => 1, // TODO: ideally AI would use Ice Spikes when these spawn instead of attacking them directly
                _ => 0
            };
        }
    }
}
