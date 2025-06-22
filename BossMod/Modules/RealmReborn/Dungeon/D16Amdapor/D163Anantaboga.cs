namespace BossMod.RealmReborn.Dungeon.D16Amdapor.D163Anantaboga;

public enum OID : uint
{
    Boss = 0x5CC, // x1
    DarkHelot = 0x5CD, // spawn during fight
    DarkNova = 0x5CE, // spawn during fight
    Pillar1 = 0x1E86A7, // x1, EventObj type
    Pillar2 = 0x1E86A8, // x1, EventObj type
    Pillar3 = 0x1E86A9, // x1, EventObj type
    Pillar4 = 0x1E86AA // x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/DarkHelot->player, no cast, single-target
    TheLook = 1072, // Boss->self, no cast, range 6+R ?-degree cone cleave
    RottenBreath = 584, // Boss->self, 1.0s cast, range 6+R ?-degree cone tankbuster
    TailDrive = 1073, // Boss->self, 2.5s cast, range 30+R 90-degree cone backward cleave (baited when anyone is behind)
    ImminentCatastrophe = 1074, // Boss->location, 7.0s cast, raidwide that can be avoided by LoS
    TerrorEye = 1077, // DarkHelot->location, 3.5s cast, range 6 circle puddle
    TriumphantRoar = 741, // DarkHelot->self, no cast, single-target damage up (enrage if not killed fast enough)
    PlagueDance = 1075, // Boss->self, no cast, single-target, visual (spawns dark nova)
    BubonicCloud = 1076 // DarkNova->self, no cast, range 10+R circle, voidzone
}

public enum TetherID : uint
{
    PlagueDance = 1 // Boss->player
}

class TheLook(BossModule module) : Components.Cleave(module, (uint)AID.TheLook, new AOEShapeCone(11.5f, 45f.Degrees())); // TODO: verify angle
class RottenBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenBreath, new AOEShapeCone(11.5f, 45f.Degrees())); // TODO: verify angle
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(35.5f, 45f.Degrees()));

class ImminentCatastrophe(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.ImminentCatastrophe, 100f, true)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(((D163Anantaboga)Module).ActivePillars());
}

class TerrorEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrorEye, 6f);

class PlagueDance(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(11.5f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == _target)
        {
            foreach (var p in ((D163Anantaboga)Module).ActivePillars())
                hints.AddForbiddenZone(_shape, p.Position, default, _activation);
        }
        else if (_target != null)
        {
            hints.AddForbiddenZone(_shape, _target.Position, default, _activation);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        _shape.Outline(Arena, _target);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PlagueDance)
        {
            _target = WorldState.Actors.Find(tether.Target);
            _activation = WorldState.FutureTime(6.1d);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PlagueDance)
            _target = null;
    }
}

class BubonicCloud(BossModule module) : Components.Voidzone(module, 11.5f, GetDarkNova)
{
    private static List<Actor> GetDarkNova(BossModule module) => module.Enemies((uint)OID.DarkNova);
}

class D163AnantabogaStates : StateMachineBuilder
{
    public D163AnantabogaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheLook>()
            .ActivateOnEnter<RottenBreath>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<ImminentCatastrophe>()
            .ActivateOnEnter<TerrorEye>()
            .ActivateOnEnter<PlagueDance>()
            .ActivateOnEnter<BubonicCloud>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 14, NameID = 1696)]
public class D163Anantaboga(WorldState ws, Actor primary) : BossModule(ws, primary, new(10f, default), new ArenaBoundsSquare(25))
{
    private static readonly uint[] _pillars = [(uint)OID.Pillar1, (uint)OID.Pillar2, (uint)OID.Pillar3, (uint)OID.Pillar4];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.DarkHelot => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DarkHelot));
        Arena.Actors(ActivePillars(), Colors.Object, true);
    }

    // TODO: blocker coordinates are slightly different, find out correct coords...

    public List<Actor> ActivePillars()
    {
        var pillars = Enemies(_pillars);
        var count = pillars.Count;
        var activepillars = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var p = pillars[i];
            if (p.EventState == 0)
                activepillars.Add(p);
        }
        return activepillars;
    }
}
