namespace BossMod.Shadowbringers.Dungeon.D01Holmintser.D013Philia;

public enum OID : uint
{
    Boss = 0x278C, // R9.8
    IronChain = 0x2895, // R1.0
    SludgeVoidzone = 0x1EABFA,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ScavengersDaughter = 15832, // Boss->self, 4.0s cast, range 40 circle
    HeadCrusher = 15831, // Boss->player, 4.0s cast, single-target
    Pendulum = 16777, // Boss->self, 5.0s cast, single-target, cast to jump
    PendulumAOE1 = 16790, // Boss->location, no cast, range 40 circle, jump to target
    PendulumAOE2 = 15833, // Boss->location, no cast, range 40 circle, jump back to center
    PendulumAOE3 = 16778, // Helper->location, 4.5s cast, range 40 circle, damage fall off AOE visual
    ChainDown = 17052, // Boss->self, 5.0s cast, single-target 
    AethersupFirst = 15848, // Boss->self, 15.0s cast, range 21 120-degree cone
    AethersupRest = 15849, // Helper->self, no cast, range 24+R 120-degree cone
    RightKnout = 15846, // Boss->self, 5.0s cast, range 24 210-degree cone
    LeftKnout = 15847, // Boss->self, 5.0s cast, range 24 210-degree cone
    Taphephobia = 15842, // Boss->self, 4.5s cast, single-target
    Taphephobia2 = 16769, // Helper->player, 5.0s cast, range 6 circle
    IntoTheLightMarker = 15844, // Helper->player, no cast, single-target, line stack
    IntoTheLightVisual = 17232, // Boss->self, 5.0s cast, single-target
    IntoTheLight = 15845, // Boss->self, no cast, range 50 width 8 rect
    FierceBeating1 = 15834, // Boss->self, 5.0s cast, single-target
    FierceBeating2 = 15836, // Boss->self, no cast, single-target
    FierceBeating3 = 15835, // Boss->self, no cast, single-target
    FierceBeating4 = 15837, // Helper->self, 5.0s cast, range 4 circle
    FierceBeating5 = 15839, // Helper->location, no cast, range 4 circle
    FierceBeating6 = 15838, // Helper->self, no cast, range 4 circle
    CatONineTailsVisual = 15840, // Boss->self, no cast, single-target
    CatONineTails = 15841 // Helper->self, 2.0s cast, range 25 120-degree cone
}

public enum IconID : uint
{
    Tankbuster = 198, // player 
    SpreadFlare = 87, // player
    ChainTarget = 92, // player
    Spread = 139, // player
    RotateCW = 167 // Boss
}

public enum SID : uint
{
    Fetters = 1849, // none->player, extra=0xEC4
    DownForTheCount = 783, // none->player, extra=0xEC7
    Sludge = 287 // none->player, extra=0x0
}

class SludgeVoidzone(BossModule module) : Components.PersistentVoidzone(module, 9.8f, m => m.Enemies(OID.SludgeVoidzone).Where(z => z.EventState != 7));
class ScavengersDaughter(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ScavengersDaughter));
class HeadCrusher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.HeadCrusher));

class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus(SID.Fetters) != null;
        if (fetters)
            chainsactive = true;
        if (fetters && !chained)
            chained = true;
        if (chaintarget != null && !fetters && !casting)
        {
            chained = false;
            chaintarget = null;
            chainsactive = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (chaintarget != null && !chainsactive)
            hints.Add($"{chaintarget.Name} is about to be fettered!");
        else if (chaintarget != null && chainsactive)
            hints.Add($"Destroy fetters on {chaintarget.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (chained && actor != chaintarget)
            foreach (var e in hints.PotentialTargets)
                e.Priority = (OID)e.Actor.OID switch
                {
                    OID.IronChain => 1,
                    OID.Boss => -1,
                    _ => 0
                };
        var ironchain = Module.Enemies(OID.IronChain).FirstOrDefault();
        if (ironchain != null && !ironchain.IsDead)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(ironchain.Position, ironchain.HitboxRadius + 3));
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.ChainTarget)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChainDown)
            casting = false;
    }
}

class Aethersup(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(24, 60.Degrees());
    private AOEInstance _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != default)
            yield return _aoe with { Risky = Module.Enemies(OID.IronChain).All(x => x.IsDead) };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AethersupFirst)
            _aoe = new(cone, Module.PrimaryActor.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AethersupFirst:
            case AID.AethersupRest:
                if (++NumCasts == 4)
                {
                    _aoe = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class PendulumFlare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(20), (uint)IconID.SpreadFlare, ActionID.MakeSpell(AID.PendulumAOE1), 5.1f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(D013Philia.ArenaCenter, 18.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.Add("Bait away!");
    }
}

class PendulumAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PendulumAOE3), 15);

class Knout(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(24, 105.Degrees()));
class LeftKnout(BossModule module) : Knout(module, AID.LeftKnout);
class RightKnout(BossModule module) : Knout(module, AID.RightKnout);

class Taphephobia(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Taphephobia2), 6);

class IntoTheLight(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.IntoTheLightMarker), ActionID.MakeSpell(AID.IntoTheLight), 5.3f);

class CatONineTails(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(25, 60.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating1)
            Sequences.Add(new(_shape, D013Philia.ArenaCenter, spell.Rotation + 180.Degrees(), -45.Degrees(), Module.CastFinishAt(spell), 2, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CatONineTails && Sequences.Count > 0)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class FierceBeating(BossModule module) : Components.Exaflare(module, 4)
{
    private readonly List<WPos> _casters = [];
    private int linesstartedcounttotal;
    private int linesstartedcount1;
    private int linesstartedcount2;
    private static readonly AOEShapeCircle circle = new(4);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor);
        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
        if (Lines.Count > 0 && linesstartedcount1 < 8)
            yield return new(circle, Helpers.RotateAroundOrigin(linesstartedcount1 * 45, D013Philia.ArenaCenter, _casters[0]), default, _activation.AddSeconds(linesstartedcount1 * 3.7f));
        if (Lines.Count > 1 && linesstartedcount2 < 8)
            yield return new(circle, Helpers.RotateAroundOrigin(linesstartedcount2 * 45, D013Philia.ArenaCenter, _casters[1]), default, _activation.AddSeconds(linesstartedcount2 * 3.7f));
    }

    public override void Update()
    {
        if (linesstartedcount1 != 0 && Lines.Count == 0)
        {
            linesstartedcounttotal = 0;
            linesstartedcount1 = 0;
            linesstartedcount2 = 0;
            _casters.Clear();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating4)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 2.5f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 7, MaxShownExplosions = 3 });
            _activation = Module.CastFinishAt(spell);
            ++linesstartedcounttotal;
            ++NumCasts;
            _casters.Add(caster.Position);
            if (linesstartedcounttotal % 2 != 0)
                ++linesstartedcount1;
            else
                ++linesstartedcount2;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FierceBeating6)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 2.5f * caster.Rotation.ToDirection(), NextExplosion = WorldState.FutureTime(1), TimeToMove = 1, ExplosionsLeft = 7, MaxShownExplosions = 3 });
            ++linesstartedcounttotal;
            if (linesstartedcounttotal % 2 != 0)
                ++linesstartedcount1;
            else
                ++linesstartedcount2;
        }
        if (Lines.Count > 0)
        {
            if ((AID)spell.Action.ID is AID.FierceBeating4 or AID.FierceBeating6)
            {
                var index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
                AdvanceLine(Lines[index], caster.Position);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
            else if ((AID)spell.Action.ID == AID.FierceBeating5)
            {
                var index = Lines.FindIndex(item => item.Next.AlmostEqual(spell.TargetXZ, 1));
                AdvanceLine(Lines[index], spell.TargetXZ);
                if (Lines[index].ExplosionsLeft == 0)
                    Lines.RemoveAt(index);
            }
        }
    }
}

class D013PhiliaStates : StateMachineBuilder
{
    public D013PhiliaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScavengersDaughter>()
            .ActivateOnEnter<HeadCrusher>()
            .ActivateOnEnter<PendulumFlare>()
            .ActivateOnEnter<PendulumAOE>()
            .ActivateOnEnter<Aethersup>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<SludgeVoidzone>()
            .ActivateOnEnter<LeftKnout>()
            .ActivateOnEnter<RightKnout>()
            .ActivateOnEnter<Taphephobia>()
            .ActivateOnEnter<IntoTheLight>()
            .ActivateOnEnter<CatONineTails>()
            .ActivateOnEnter<FierceBeating>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8301)]
public class D013Philia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(134, -465); // slightly different from calculated center due to difference operation
    private static readonly ArenaBoundsComplex arena = new([new Circle(ArenaCenter, 19.5f)], [new Rectangle(new(134, -445), 20, 1.5f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.IronChain), Colors.Vulnerable);
    }
}