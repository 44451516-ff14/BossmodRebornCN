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

abstract class WaterBomb(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
class WaterBomb1(BossModule module) : WaterBomb(module, (uint)AID.WaterBomb1);
class WaterBomb2(BossModule module) : WaterBomb(module, (uint)AID.WaterBomb2);
class WaterBomb3(BossModule module) : WaterBomb(module, (uint)AID.WaterBomb3);

class OdiousCroak(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(14f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OdiousCroak)
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
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Chirp)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(8.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DiscordantHarmony)
            _aoes.Clear();
    }
}

class ToyHammer(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ToyHammer);

class Concussion(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Concussion, "Concussion", "concussed");

class FrogSong(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            if (party[i].FindStatus((uint)SID.Toad) != null)
            {
                hints.Add("Kill the adds to stop the frog song.");
                return;
            }
        }
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
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-365, -250), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(-365, -230), 20, 2.01f),
    new Rectangle(new(-365, -270), 20, 1.75f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PoroggoChoirtoad));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.PoroggoChoirtoad => 1,
                _ => 0
            };
        }
    }
}
