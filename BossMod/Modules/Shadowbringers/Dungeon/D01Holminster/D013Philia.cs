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
    TaphephobiaVisual = 15842, // Boss->self, 4.5s cast, single-target
    Taphephobia = 16769, // Helper->player, 5.0s cast, range 6 circle
    IntoTheLightMarker = 15844, // Helper->player, no cast, single-target, line stack
    IntoTheLightVisual = 17232, // Boss->self, 5.0s cast, single-target
    IntoTheLight = 15845, // Boss->self, no cast, range 50 width 8 rect
    FierceBeatingRotationVisual = 15834, // Boss->self, 5.0s cast, single-target
    FierceBeatingVisual1 = 15836, // Boss->self, no cast, single-target
    FierceBeatingVisual2 = 15835, // Boss->self, no cast, single-target
    FierceBeatingExaFirst = 15837, // Helper->self, 5.0s cast, range 4 circle
    FierceBeatingExaRestFirst = 15838, // Helper->self, no cast, range 4 circle
    FierceBeatingExaRestRest = 15839, // Helper->location, no cast, range 4 circle
    CatONineTailsVisual = 15840, // Boss->self, no cast, single-target
    CatONineTails = 15841 // Helper->self, 2.0s cast, range 25 120-degree cone
}

public enum IconID : uint
{
    SpreadFlare = 87, // player
    ChainTarget = 92 // player
}

public enum SID : uint
{
    Fetters = 1849 // none->player, extra=0xEC4
}

sealed class SludgeVoidzone(BossModule module) : Components.Voidzone(module, 9f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SludgeVoidzone);
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

sealed class ScavengersDaughter(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScavengersDaughter);
sealed class HeadCrusher(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HeadCrusher);

sealed class Fetters(BossModule module) : BossComponent(module)
{
    private bool chained;
    private bool chainsactive;
    private Actor? chaintarget;
    private bool casting;

    public override void Update()
    {
        var fetters = chaintarget?.FindStatus((uint)SID.Fetters) != null;
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
            for (var i = 0; i < hints.PotentialTargets.Count; ++i)
            {
                var e = hints.PotentialTargets[i];
                e.Priority = e.Actor.OID switch
                {
                    (uint)OID.IronChain => 1,
                    (uint)OID.Boss => AIHints.Enemy.PriorityInvincible,
                    _ => 0
                };
            }
        var chain = Module.Enemies((uint)OID.IronChain);
        var ironchain = chain.Count != 0 ? chain[0] : null;
        if (ironchain != null && !ironchain.IsDead)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(ironchain.Position, 3.6f));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChainTarget)
        {
            chaintarget = actor;
            casting = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChainDown)
            casting = false;
    }
}

sealed class Aethersup(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(24f, 60f.Degrees());
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is AOEInstance aoe)
        {
            var chain = Module.Enemies((uint)OID.IronChain);
            var count = chain.Count;
            aoe.Risky = count == 0 || count != 0 && chain[0].IsDead;
            return new AOEInstance[1] { aoe };
        }
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AethersupFirst)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AethersupFirst:
            case (uint)AID.AethersupRest:
                if (++NumCasts == 4)
                {
                    _aoe = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class PendulumFlare(BossModule module) : Components.BaitAwayIcon(module, 20f, (uint)IconID.SpreadFlare, (uint)AID.PendulumAOE1, 5.1f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(D013Philia.ArenaCenter, 18.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }
}

sealed class PendulumAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PendulumAOE3, 15f);

sealed class Knout(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftKnout, (uint)AID.RightKnout], new AOEShapeCone(24f, 105f.Degrees()));

sealed class Taphephobia(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Taphephobia, 6f);

sealed class IntoTheLight(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.IntoTheLightMarker, (uint)AID.IntoTheLight, 5.3f);

sealed class CatONineTails(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(25f, 60f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingRotationVisual)
        {
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation + 180f.Degrees(), -45f.Degrees(), Module.CastFinishAt(spell), 2f, 8));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CatONineTails)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

sealed class FierceBeating(BossModule module) : Components.Exaflare(module, 4f)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
            return [];
        var futureAOEs = FutureAOEs(linesCount);
        var imminentAOEs = ImminentAOEs(linesCount);
        var futureCount = futureAOEs.Count;
        var imminentCount = imminentAOEs.Length;
        var aoesCount = _aoes.Count;
        var total = futureCount + imminentCount + aoesCount;
        var index = 0;
        var aoes = new AOEInstance[total];
        for (var i = 0; i < futureCount; ++i)
        {
            var aoe = futureAOEs[i];
            aoes[index++] = new(Shape, aoe.Item1.Quantized(), aoe.Item3, aoe.Item2, FutureColor);
        }
        for (var i = 0; i < imminentCount; ++i)
        {
            var aoe = imminentAOEs[i];
            aoes[index++] = new(Shape, aoe.Item1.Quantized(), aoe.Item3, aoe.Item2, ImminentColor);
        }
        for (var i = 0; i < aoesCount; ++i)
        {
            var aoe = _aoes[i];
            aoes[index++] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingExaFirst)
        {
            AddLine(ref caster, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FierceBeatingExaRestFirst)
        {
            AddLine(ref caster, WorldState.FutureTime(1d));
        }
        if (Lines.Count != 0)
        {
            if (spell.Action.ID is (uint)AID.FierceBeatingExaFirst or (uint)AID.FierceBeatingExaRestFirst)
                Advance(caster.Position);
            else if (spell.Action.ID == (uint)AID.FierceBeatingExaRestRest)
                Advance(spell.TargetXZ);
        }

        void Advance(WPos pos)
        {
            var count = Lines.Count;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public void AddLine(ref Actor caster, DateTime activation)
    {
        var adv = 2.5f * caster.Rotation.ToDirection();
        Lines.Add(new(caster.Position, adv, activation, 1d, 7, 3));
        ++NumCasts;
        if (_aoes.Count != 0 && NumCasts > 2)
            _aoes.RemoveAt(0);
        if (NumCasts <= 14)
        {
            _aoes.Add(new(circle, WPos.RotateAroundOrigin(45, D013Philia.ArenaCenter, caster.Position.Quantized()), default, WorldState.FutureTime(3.7d)));
        }
        if (NumCasts == 16)
            NumCasts = 0;
    }
}

sealed class D013PhiliaStates : StateMachineBuilder
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
            .ActivateOnEnter<Knout>()
            .ActivateOnEnter<Taphephobia>()
            .ActivateOnEnter<IntoTheLight>()
            .ActivateOnEnter<CatONineTails>()
            .ActivateOnEnter<FierceBeating>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8301)]
public sealed class D013Philia(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(134f, -465f); // slightly different from calculated center due to difference operation
    private static readonly ArenaBoundsComplex arena = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi64th, 64)], [new Rectangle(new(134f, -445.277f), 20f, 1.25f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IronChain), Colors.Vulnerable);
    }
}