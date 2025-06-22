namespace BossMod.Endwalker.Quest.Job.Sage.LifeEphemeralPathEternal.Guildivain;

public enum OID : uint
{
    Boss = 0x35C6, // R4.998, x1
    StrengthenedNoulith = 0x35C8, // R1.0
    EnhancedNoulith = 0x3859, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    Nouliths = 26851, // BossP2->self, 5.0s cast, single-target
    AetherstreamTank = 26852, // StrengthenedNoulith->Lalah, no cast, range 50 width 4 rect
    AetherstreamPlayer = 26853, // StrengthenedNoulith->players/Loifa, no cast, range 50 width 4 rect
    Tracheostomy = 26854, // BossP2->self, 5.0s cast, range 10-20 donut

    RightScalpel = 26855, // BossP2->self, 5.0s cast, range 15 210-degree cone
    LeftScalpel = 26856, // BossP2->self, 5.0s cast, range 15 210-degree cone
    LeftRightScalpel1 = 26864, // BossP2->self, 7.0s cast, range 15 210-degree cone
    LeftRightScalpel2 = 26865, // BossP2->self, 3.0s cast, range 15 210-degree cone
    RightLeftScalpel1 = 26862, // BossP2->self, 7.0s cast, range 15 210-degree cone
    RightLeftScalpel2 = 26863, // BossP2->self, 3.0s cast, range 15 210-degree cone

    Laparotomy = 26857, // BossP2->self, 5.0s cast, range 15 120-degree cone
    Amputation = 26858, // BossP2->self, 7.0s cast, range 20 120-degree cone
    Hypothermia = 26861, // BossP2->self, 5.0s cast, range 50 circle
    CryonicsVisual = 26859, // BossP2->self, 8.0s cast, single-target
    Cryonics = 26860, // Helper->players, 8.0s cast, range 6 circle

    Craniotomy = 28386, // BossP2->self, 8.0s cast, range 40 circle

    FrigotherapyVisual = 26866, // BossP2->self, 5.0s cast, single-target
    Frigotherapy = 26867 // Helper->players/Mahaud/Loifa, 7.0s cast, range 5 circle
}

public enum TetherID : uint
{
    Noulith = 17, // StrengthenedNoulith->Lalah/player/Loifa
    Craniotomy = 174 // EnhancedNoulith->Lalah/Loifa/player/Mahaud/Ancel
}

public enum SID : uint
{
    Craniotomy = 2968 // none->player/Lalah/Mahaud/Ancel/Loifa, extra=0x0
}

class AetherstreamTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50f, 2f), (uint)TetherID.Noulith)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AetherstreamPlayer or (uint)AID.AetherstreamTank)
            CurrentBaits.RemoveAll(x => x.Target.InstanceID == spell.MainTargetID);
    }
}

class Tracheostomy : Components.SimpleAOEs
{
    public Tracheostomy(BossModule module) : base(module, (uint)AID.Tracheostomy, new AOEShapeDonut(10f, 20f))
    {
        WorldState.Actors.EventStateChanged.Subscribe((act) =>
        {
            if (act.OID == 0x1EA1A1 && act.EventState == 7)
                Arena.Bounds = AncelAndMahaud.AncelAndMahaud.ArenaBounds;
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            Arena.Bounds = Guildivain.SmallBounds;
    }
}

class Scalpel(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightScalpel, (uint)AID.LeftScalpel,
(uint)AID.RightLeftScalpel1, (uint)AID.RightLeftScalpel2, (uint)AID.LeftRightScalpel1, (uint)AID.LeftRightScalpel2], new AOEShapeCone(15f, 105f.Degrees()));

class Laparotomy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Laparotomy, new AOEShapeCone(15f, 60f.Degrees()));
class Amputation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Amputation, new AOEShapeCone(20f, 60f.Degrees()));

class Hypothermia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hypothermia);
class Cryonics(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Cryonics, 6f);
class Craniotomy(BossModule module) : Components.RaidwideCast(module, (uint)AID.Craniotomy);

class Frigotherapy(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Frigotherapy, 5f);

class GuildivainStates : StateMachineBuilder
{
    public GuildivainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherstreamTether>()
            .ActivateOnEnter<Tracheostomy>()
            .ActivateOnEnter<Scalpel>()
            .ActivateOnEnter<Laparotomy>()
            .ActivateOnEnter<Amputation>()
            .ActivateOnEnter<Hypothermia>()
            .ActivateOnEnter<Cryonics>()
            .ActivateOnEnter<Craniotomy>()
            .ActivateOnEnter<Frigotherapy>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69608, NameID = 10733)]
public class Guildivain(WorldState ws, Actor primary) : BossModule(ws, primary, SmallBounds.Center, AncelAndMahaud.AncelAndMahaud.ArenaBounds)
{
    public static readonly ArenaBoundsComplex SmallBounds = new([new Polygon(new(224.8f, -855.8f), 10, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.EnhancedNoulith));
        Arena.Actor(PrimaryActor);
    }
}
