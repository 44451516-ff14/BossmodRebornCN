namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D133LunarBahamut;

public enum OID : uint
{
    Boss = 0x316A, // R8.4
    LunarNail = 0x316B, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 23620, // Boss->player, no cast, single-target

    TwistedScream = 23367, // Boss->self, 3.0s cast, range 40 circle, tiny raidwide, spawns lunar nails
    Upburst = 24667, // LunarNail->self, 3.0s cast, range 2 circle
    BigBurst = 23368, // LunarNail->self, 4.0s cast, range 9 circle
    PerigeanBreath = 23385, // Boss->self, 5.0s cast, range 30 90-degree cone
    AkhMornFirst = 23381, // Boss->players, 5.0s cast, range 4 circle, 4 hits
    AkhMornRest = 23382, // Boss->players, no cast, range 4 circle
    MegaflareVisual = 23372, // Boss->self, 3.0s cast, single-target
    MegaflareSpread = 23373, // Helper->players, 5.0s cast, range 5 circle
    MegaflareAOE = 23374, // Helper->location, 3.0s cast, range 6 circle
    MegaflareDive = 23378, // Boss->self, 4.0s cast, range 41 width 12 rect
    KanRhaiVisual1 = 23375, // Boss->self, 4.0s cast, single-target
    KanRhaiVisual2 = 23377, // Helper->self, no cast, single-target
    KanRhai = 23376, // Helper->self, no cast, range 30 width 6 rect, baitaway cross 15 * 6
    LunarFlareVisual = 23369, // Boss->self, 3.0s cast, single-target
    LunarFlareBig = 23370, // Helper->location, 10.0s cast, range 11 circle
    LunarFlareSmall = 23371, // Helper->location, 10.0s cast, range 6 circle
    Gigaflare = 23383, // Boss->self, 7.0s cast, range 40 circle
    Flatten = 23384 // Boss->player, 5.0s cast, single-target
}

public enum IconID : uint
{
    KanRhai = 260 // player->self
}

class Upburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Upburst, 2f);
class BigBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigBurst, 9f);
class PerigeanBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PerigeanBreath, new AOEShapeCone(30f, 45f.Degrees()));
class MegaflareAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflareAOE, 6f);
class MegaflareSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MegaflareSpread, 5f);
class MegaflareDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegaflareDive, new AOEShapeRect(41f, 6f));
class LunarFlareBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarFlareBig, 11f);
class LunarFlareSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LunarFlareSmall, 6f);
class Gigaflare(BossModule module) : Components.RaidwideCast(module, (uint)AID.Gigaflare);
class Flatten(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Flatten);

class AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4f, default, 4, 4)
{
    private int numCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornFirst)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornFirst or (uint)AID.AkhMornRest)
        {
            if (++numCasts == 4)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }
}

class KanRhaiBait(BossModule module) : Components.GenericBaitAway(module)
{
    public static readonly AOEShapeCross Cross = new(15f, 3f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.KanRhai)
            CurrentBaits.Add(new(actor, actor, Cross, WorldState.FutureTime(5.6d), default));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.KanRhaiVisual2)
            CurrentBaits.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away and move!");
    }
}

class KanRhaiAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.KanRhai)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count != 0 && NumCasts == count * 20)
            {
                _aoes.Clear();
                NumCasts = 0;
            }
        }
        else if (spell.Action.ID == (uint)AID.KanRhaiVisual2)
            _aoes.Add(new(KanRhaiBait.Cross, WPos.ClampToGrid(caster.Position), Angle.AnglesCardinals[1]));
    }
}

class D133LunarBahamutStates : StateMachineBuilder
{
    public D133LunarBahamutStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Upburst>()
            .ActivateOnEnter<BigBurst>()
            .ActivateOnEnter<PerigeanBreath>()
            .ActivateOnEnter<MegaflareAOE>()
            .ActivateOnEnter<MegaflareSpread>()
            .ActivateOnEnter<MegaflareDive>()
            .ActivateOnEnter<LunarFlareBig>()
            .ActivateOnEnter<LunarFlareSmall>()
            .ActivateOnEnter<Gigaflare>()
            .ActivateOnEnter<Flatten>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<KanRhaiBait>()
            .ActivateOnEnter<KanRhaiAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10077)]
public class D133LunarBahamut(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(796.65f, -97.55f), 19.5f * CosPI.Pi40th, 40)], [new Rectangle(new(776f, -97.5f), 1.6f, 20)]);
}
