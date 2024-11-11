namespace BossMod.Heavensward.Dungeon.D11Antitower.D111ZuroRoggo;

public enum OID : uint
{
    Boss = 0x14FC, // R3.0
    Chirp = 0x14FE, // R2.0
    PoroggoChoirtoad = 0x14FD, // R2.1
    FrogSong = 0x1E9F4A, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/PoroggoChoirtoad->player, no cast, single-target

    WaterBombVisual = 5537, // Boss->self, 3.0s cast, single-target
    WaterBomb1 = 5538, // Helper->location, 3.0s cast, range 6 circle
    WaterBomb2 = 5979, // Helper->location, 3.0s cast, range 6 circle
    WaterBomb3 = 5977, // Helper->location, 3.0s cast, range 6 circle

    OdiousCroakVisual = 32370, // Helper->self, 4.0s cast, range 40+R 120-degree cone
    OdiousCroak = 5540, // Helper->self, no cast, range 11+R 120-degree cone, 12 casts

    SpawnChirps = 5542, // Boss->self, no cast, single-target
    DiscordantHarmony = 5543, // Chirp->self, no cast, range 6 circle
    FrogSong = 5541, // Helper->self, no cast, range 40 circle

    ToyHammer = 5539 // Boss->player, 3.0s cast, single-target, tankbuster + concussion
}

public enum SID : uint
{
    Toad = 439, // none->player, extra=0x1
    Concussion = 3513 // Boss->player, extra=0xF43
}

abstract class WaterBomb(BossModule module, AID aid) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(aid), 6);
class WaterBomb1(BossModule module) : WaterBomb(module, AID.WaterBomb1);
class WaterBomb2(BossModule module) : WaterBomb(module, AID.WaterBomb2);
class WaterBomb3(BossModule module) : WaterBomb(module, AID.WaterBomb3);

class OdiousCroak(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(14, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.OdiousCroak)
        {
            if (_aoe == null)
                _aoe = new(cone, caster.Position, caster.Rotation);
            if (++NumCasts == 12)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}

class DiscordantHarmony(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Chirp)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(8.7f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DiscordantHarmony)
            _aoes.Clear();
    }
}

class ToyHammer(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ToyHammer));

class Concussion(BossModule module) : BossComponent(module)
{
    private Actor? _concussion;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Concussion)
            _concussion = actor;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Concussion)
            _concussion = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_concussion == null || !(actor.Role == Role.Healer || actor.Class == Class.BRD))
            return;
        if (_concussion == actor)
            hints.Add("Cleanse your concussion.");
        else
            hints.Add($"Cleanse {_concussion.Name}! (Concussion))");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_concussion != null)
        {
            if (actor.Role == Role.Healer)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Esuna), _concussion, ActionQueue.Priority.High);
            else if (actor.Class == Class.BRD)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(BRD.AID.WardensPaean), _concussion, ActionQueue.Priority.High);
        }
    }
}

class FrogSong(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus(SID.Toad) != null)
            return;
        if (Raid.WithoutSlot().Any(x => x.FindStatus(SID.Toad) != null))
            hints.Add("Kill the adds to stop the frog song.");
    }
}

class D111ZuroRoggoStates : StateMachineBuilder
{
    public D111ZuroRoggoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterBomb1>()
            .ActivateOnEnter<WaterBomb2>()
            .ActivateOnEnter<WaterBomb3>()
            .ActivateOnEnter<OdiousCroak>()
            .ActivateOnEnter<DiscordantHarmony>()
            .ActivateOnEnter<ToyHammer>()
            .ActivateOnEnter<Concussion>()
            .ActivateOnEnter<FrogSong>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 141, NameID = 4805)]
public class D111ZuroRoggo(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-365, -250), 19.5f / MathF.Cos(MathF.PI / 32), 32)], [new Rectangle(new(-365, -230), 20, 2.01f),
    new Rectangle(new(-365, -270), 20, 1.75f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.PoroggoChoirtoad).Concat([PrimaryActor]));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.PoroggoChoirtoad => 1,
                _ => 0
            };
        }
    }
}
