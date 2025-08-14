
namespace BossMod.RealmReborn.Dungeon.D08Qarn.D081Teratotaur;

public enum OID : uint
{
    // Boss
    Boss = 0x477A, // x1

    // Trash
    DungWespe = 0x477B, // spawn during fight

    // EventObj
    Platform1 = 0x1E87E2, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform2 = 0x1E87E3, // x1, EventObj type; eventstate 0 if active, 7 if inactive
    Platform3 = 0x1E87E4 // x1, EventObj type; eventstate 0 if active, 7 if inactive
}

public enum AID : uint
{
    // Boss
    AutoAttackBoss = 870, // Boss->player, no cast
    Triclip = 42231, // Boss->player, 5.0s cast, tankbuster
    Mow = 42232, // Boss->self, 2.5s cast, range 8.25 120-degree cone aoe
    FrightfulRoar = 42233, // Boss->self, 3.0s cast, range 6.0 aoe
    MortalRay = 42229, // Boss->self, 3.0s cast, raidwide doom debuff

    // DungWespe
    AutoAttackWespe = 871, // DungWespe->player, no cast
    FinalSting = 42230 // DungWespe->player, 3.0s cast
}

public enum SID : uint
{
    Doom = 1970 // Boss->player, extra=0x0
}

sealed class Triclip(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Triclip);
sealed class Mow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mow, new AOEShapeCone(8.25f, 60f.Degrees()));
sealed class FrightfulRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrightfulRoar, 6.0f);

sealed class MortalRay(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask _dooms;
    private readonly Actor[] _platforms = [module.Enemies((uint)OID.Platform1)[0], module.Enemies((uint)OID.Platform2)[0], module.Enemies((uint)OID.Platform3)[0]];
    private DateTime activation;
    private static readonly AOEShapeRect square = new(1.75f, 1.75f, 1.75f, InvertForbiddenZone: true);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_dooms[slot])
        {
            for (var i = 0; i < 3; ++i)
            {
                var p = _platforms[i];
                if (_platforms[i].EventState == default)
                {
                    return new AOEInstance[1] { new(square, p.Position, default, activation, Colors.SafeFromAOE) };
                }
            }
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_dooms[slot])
        {
            hints.Add("Go to glowing platform!");
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            _dooms[Raid.FindSlot(actor.InstanceID)] = true;
            activation = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            _dooms[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}

sealed class D081TeratotaurStates : StateMachineBuilder
{
    public D081TeratotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Triclip>()
            .ActivateOnEnter<Mow>()
            .ActivateOnEnter<FrightfulRoar>()
            .ActivateOnEnter<MortalRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1567)]
public sealed class D081Teratotaur(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom([new(-94.9f, -59f), new(-70.2f, -46.1f), new(-55.3f, -46.6f),
    new(-55.7f, -55.6f), new(-51.1f, -60.9f), new(-51.2f, -65), new(-58.1f, -67.7f),
    new(-64.7f, -70.6f), new(-88.4f, -72.2f), new(-89, -66.2f), new(-94.9f, -65.5f)])]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.DungWespe => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DungWespe));
    }
}
