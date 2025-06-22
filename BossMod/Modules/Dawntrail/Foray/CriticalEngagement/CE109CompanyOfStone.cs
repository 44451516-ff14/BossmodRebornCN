namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE109CompanyOfStone;

public enum OID : uint
{
    Boss = 0x471E, // R3.0
    OccultKnight1 = 0x4721, // R3.0
    OccultKnight2 = 0x471F, // R3.0
    Megaloknight = 0x4720, // R5.0
    Deathwall = 0x1EBD8C, // R0.5
    DeathwallHelper = 0x4862, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42560, // Boss->player, no cast, single-target
    Deathwall = 41901, // DeathwallHelper->self, no cast, range 20-30 donut

    LineOfFire1 = 41817, // Boss->self, 5.0s cast, range 60 width 8 rect
    LineOfFire2 = 41818, // Boss->self, 5.0s cast, range 60 width 8 rect
    KnuckleCrusher = 41819, // Boss->self, 10.0s cast, range 15 circle
    KnuckleDownVisual = 41820, // Boss->self, 4.0s cast, single-target, raidwides
    KnuckleDown = 41821, // Helper->self, no cast, ???
    SpinningSiegeCCW = 41823, // Boss->self, 8.0s cast, range 60 width 6 cross, 6 hits, 9° step
    SpinningSiegeCW = 41822, // Boss->self, 8.0s cast, range 60 width 6 cross
    SpinningSiegeRest = 41824, // Boss->self, 0.5s cast, range 60 width 6 cross
    BlastKnucklesVisual = 41826, // Boss->self, 5.0s cast, ???
    BlastKnuckles = 41891, // Helper->self, no cast, knockback 15, away from source
    CageOfFire = 41825, // Boss->self, 7.0s cast, range 60 width 8 rect
    Moatmaker = 41827, // Boss->location, 3.0s cast, range 9 circle

    DualfistFlurryFirst = 41828, // Boss->location, 10.0s cast, range 6 circle
    DualfistFlurryRepeat = 43152, // Helper->self, no cast, range 6 circle
    DualfistFlurryAdvance = 41829, // Boss->location, no cast, single-target

    SpiritSling = 41834, // OccultKnight1->self, 3.5s cast, range 60 width 8 rect
    BarefistedDeath = 41830, // Megaloknight->self, 90.0s cast, range 60 circle
    BarefistedDeathRepeat = 41831 // Megaloknight->self, no cast, range 60 circle
}

sealed class LineOfFireSpiritSlingCageOfFire(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.LineOfFire1, (uint)AID.LineOfFire2,
(uint)AID.CageOfFire, (uint)AID.SpiritSling], new AOEShapeRect(60f, 4f));

sealed class KnuckleCrusher : Components.SimpleAOEs
{
    private WPos midpoint;

    public KnuckleCrusher(BossModule module) : base(module, (uint)AID.KnuckleCrusher, 15f, 3)
    {
        MaxDangerColor = 1;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction && Casters.Count == 3)
        {
            // first aoe is always seems to be in center and irrelevant for the dodge spot
            var center = Arena.Center;
            var dir = Casters[1].Origin - center;
            var isCW = dir.OrthoR().Dot(Casters[2].Origin - center) > 0f;
            midpoint = dir.Rotate((isCW ? 1f : -1f) * 55f.Degrees()) + center;
            midpoint += 6f * (midpoint - center).Normalized();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (midpoint != default && NumCasts < 2)
        {
            Arena.AddCircle(midpoint, 2f, Colors.Safe, 2f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count != 0 && NumCasts < 2)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(midpoint, 3f), Casters[^1].Activation); // stay in dodge spot
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (midpoint != default && NumCasts < 2)
        {
            hints.Add("Stay near dodge spot!", !actor.Position.InCircle(midpoint, 3f));
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (midpoint != default && NumCasts < 2)
        {
            movementHints.Add(actor.Position, midpoint, Colors.Safe);
        }
    }
}

sealed class KnuckleDown(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.KnuckleDownVisual, (uint)AID.KnuckleDown, 0.9f, "Raidwide x4");
sealed class Moatmaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Moatmaker, 9f);

sealed class SpinningSiege(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(60f, 3f);
    private static readonly Angle a9 = 9f.Degrees();
    private WPos midpoint;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SpinningSiegeCW:
                AddSequence(-a9);
                break;
            case (uint)AID.SpinningSiegeCCW:
                AddSequence(a9);
                break;
        }
        void AddSequence(Angle increment)
        {
            Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 1.7f, 6));
            if (Sequences.Count == 4)
            {
                var center = Arena.Center;

                // Sort rotations by angle from center
                var centerZ = center.Z;
                var centerX = center.X;
                Sequences.Sort((a, b) => MathF.Atan2(a.Origin.Z - centerZ, a.Origin.X - centerX).CompareTo(MathF.Atan2(b.Origin.Z - centerZ, b.Origin.X - centerX)));

                // Find adjacent pair where left is CCW and right is CW
                for (var i = 0; i < 4; i++)
                {
                    var next = (i + 1) % 4;
                    var a = Sequences[i];
                    var b = Sequences[next];

                    if (a.Increment == -a9 && b.Increment == a9)
                    {
                        var aOrigin = a.Origin;
                        var bOrigin = b.Origin;
                        midpoint = new((aOrigin.X + bOrigin.X) / 2, (aOrigin.Z + bOrigin.Z) / 2);
                        return;
                    }
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpinningSiegeCCW or (uint)AID.SpinningSiegeCW or (uint)AID.SpinningSiegeRest)
        {
            AdvanceSequence(spell.LocXZ, spell.Rotation, WorldState.CurrentTime);
            if (NumCasts % 4 == 0)
            {
                midpoint -= 3f * (midpoint - Arena.Center).Normalized();
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Sequences.Count == 4 && NumCasts < 20)
        {
            Arena.AddCircle(midpoint, 4f, Colors.Safe, 2f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Sequences.Count == 4 && NumCasts < 20)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(midpoint, 5f), Sequences[0].NextActivation); // stay in safe area
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Sequences.Count == 4 && NumCasts < 20)
        {
            hints.Add("Stay in safe area!", !actor.Position.InCircle(midpoint, 5f));
        }
    }
}

sealed class BlastKnuckles(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _kb;
    private readonly LineOfFireSpiritSlingCageOfFire _aoe = module.FindComponent<LineOfFireSpiritSlingCageOfFire>()!;
    private DateTime activation;
    private RelSimplifiedComplexPolygon poly = new();
    private bool polyInit;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlastKnucklesVisual)
        {
            activation = Module.CastFinishAt(spell, 1f);
            _kb = new(spell.LocXZ, 15f, activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlastKnuckles)
        {
            _kb = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback kb)
        {
            if (!IsImmune(slot, activation))
            {
                if (!polyInit)
                {
                    var aoes = _aoe.ActiveAOEs(slot, actor);
                    var len = aoes.Length;
                    if (len < 4)
                        return;
                    var rectangle = new List<RectangleSE>();
                    for (var i = 0; i < len; ++i)
                    {
                        ref readonly var aoe = ref aoes[i];
                        rectangle.Add(new(aoe.Origin, aoe.Origin + 60f * aoe.Rotation.ToDirection(), 4f));
                    }
                    AOEShapeCustom combine = new([.. rectangle]);
                    combine.GetCombinedPolygon(Arena.Center);
                    poly = combine.Polygon!;
                    polyInit = true;
                }
                var center = Arena.Center;
                hints.AddForbiddenZone(p =>
                {
                    var projected = p + 15f * (p - center).Normalized();
                    if (projected.InCircle(center, 20f) && !poly.Contains(projected - center))
                        return 1f;
                    return -1f;
                }, activation);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }
}

sealed class DualfistFlurry(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DualfistFlurryFirst)
        {
            var pos = spell.LocXZ;
            Lines.Add(new() { Next = new(MathF.Round(pos.X * 2f) / 2f, MathF.Round(pos.Z * 2f) / 2f), Advance = 7f * caster.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1f, ExplosionsLeft = 6, MaxShownExplosions = 3 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.DualfistFlurryFirst or (uint)AID.DualfistFlurryRepeat)
        {
            var count = Lines.Count;
            var pos = id == (uint)AID.DualfistFlurryFirst ? spell.TargetXZ : caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class BarefistedDeath(BossModule module) : Components.CastHint(module, (uint)AID.BarefistedDeath, "Enrage!", true);

sealed class CE109CompanyOfStoneStates : StateMachineBuilder
{
    public CE109CompanyOfStoneStates(CE109CompanyOfStone module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LineOfFireSpiritSlingCageOfFire>()
            .ActivateOnEnter<KnuckleCrusher>()
            .ActivateOnEnter<KnuckleDown>()
            .ActivateOnEnter<SpinningSiege>()
            .ActivateOnEnter<Moatmaker>()
            .ActivateOnEnter<BlastKnuckles>()
            .ActivateOnEnter<DualfistFlurry>()
            .ActivateOnEnter<BarefistedDeath>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Boss);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDestroyed)
                        return false;
                }
                return module.BossMegaloknight()?.IsDeadOrDestroyed ?? true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 40)]
public sealed class CE109CompanyOfStone(WorldState ws, Actor primary) : BossModule(ws, primary, WPos.ClampToGrid(new(680f, -280f)), new ArenaBoundsCircle(20f))
{
    private Actor? _bossMegaloknight;
    public Actor? BossMegaloknight() => _bossMegaloknight;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bossMegaloknight == null)
        {
            var b = Enemies((uint)OID.Megaloknight);
            _bossMegaloknight = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.OccultKnight2));
        Arena.Actor(_bossMegaloknight);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
