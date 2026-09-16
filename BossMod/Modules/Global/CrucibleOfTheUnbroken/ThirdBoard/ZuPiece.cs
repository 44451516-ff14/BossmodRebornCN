namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece;

public enum OID : uint
{
    ZuPiece = 0x4C96,

    PulletPieceEgg = 0x4C9A, // R0.500, x8
    CockerelPieceEgg = 0x4C99, // R0.500, x8
    PulletPiece = 0x4C98, // R0.400, x0 (spawn during fight)
    CockerelPiece = 0x4C97, // R0.400, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49680, // ZuPiece->player, no cast, single-target
    AutoAttackBreakbeak = 48488, // 4C97->player, no cast, single-target
    CausticVomit = 48489, // 4C98->player, 13.0s cast, single-target
    Hatch = 48487, // 4C9A/4C99->self, 10.0s cast, single-target
    CrossbreezeIcon = 48491, // ZuPiece->self, 10.0s cast, single-target
    Crossbreeze = 48492, // Helper->self, 3.0s cast, range 50 width 8 cross
    FlyingFrenzy = 50465, // ZuPiece->self, 8.0s cast, single-target
    FlyingFrenzyAOE = 48490, // ZuPiece->players, no cast, range 6 circle

    AiryPursuit = 48500, // ZuPiece->self, 8.0s cast, single-target
    AiryPursuitAOE = 48501, // Helper->location, 3.0s cast, range 6 circle
    AiryPursuitTeleport = 48502, // Helper->location, no cast, range 6 circle

    Featherglide = 48495, // ZuPiece->player, no cast, width 3 rect charge
    FerociousForeCarve = 48493, // ZuPiece->self, 6.0s cast, single-target
    ForeCarveVisual = 48496, // ZuPiece->self, 0.5+0.7s cast, single-target
    ForeCarve = 48497, // Helper->self, 1.0s cast, range 15 180.000-degree cone
    RampagingRearCarve = 48494, // ZuPiece->self, 6.0s cast, single-target
    RearCarveVisual = 48498, // ZuPiece->self, 0.5+0.7s cast, single-target
    RearCarve = 48499, // Helper->self, 1.0s cast, range 15 180.000-degree cone
}

public enum SID : uint
{
    BroodRage = 5433, // none->ZuPiece, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xD/0xE/0xF/0x10
}

public enum IconID : uint
{
    EggExclamation = 569, // 4C9A/4C99->self
    Crossbreeze = 686, // player->self
    TankBuster = 465, // player->self
    AiryPursuit = 197, // player->self
}

public enum TetherID : uint
{
    Carve = 57, // ZuPiece->player
    CarveStretched = 1, // ZuPiece->player
}

sealed class Crossbreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crossbreeze, new AOEShapeCross(50.0f, 4.0f));
sealed class AiryPursuit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AiryPursuitAOE, 6.0f);
sealed class CarveTether(BossModule module) : Components.StretchTetherDuo(module, 16.0f, 5.0f);

sealed class CrossbreezeBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCross(50.0f, 4.0f), (uint)IconID.Crossbreeze,
    (uint)AID.CrossbreezeIcon, 8.1f, centerAtTarget: true)
{

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
        {
            CurrentBaits.Add(new(source, WorldState.Actors.Find(targetID) ?? actor, new AOEShapeCross(50.0f, 4.0f), WorldState.FutureTime(ActivationDelay), customRotation: 0.0f.Degrees(), arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Crossbreeze)
        {
            if (CurrentBaits.Count > 0)
            {
                CurrentBaits.Clear();
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        bait.Shape = new AOEShapeCross(50.0f, 4.0f + 0.6f);

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var onHitbox = bait.Shape.Check(egg.Position, bait.Target.Position, bait.Rotation);
            Arena.ZoneCircleOutline(egg.Position, egg.HitboxRadius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];

        foreach (var egg in Module.Enemies((uint)OID.PulletPieceEgg).Concat(Module.Enemies((uint)OID.CockerelPieceEgg)))
        {
            hints.AddForbiddenZone(new SDCross(egg.Position, default, 50f, 4f + 0.6f), bait.Activation);
        }
    }
}

sealed class FlyingFrenzy(BossModule module) : Components.BaitAwayIcon(module, 6.0f, (uint)IconID.TankBuster, (uint)AID.FlyingFrenzyAOE, activationDelay: 8.1f,
    tankbuster: true)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            var onHitbox = (egg.Position - bait.Target.Position).LengthSq() <= reach * reach;
            Arena.ZoneCircleOutline(egg.Position, egg.HitboxRadius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            hints.AddForbiddenZone(new SDCircle(egg.Position, reach), bait.Activation);
        }
    }
}

sealed class AiryPursuitPuddles(BossModule module) : Components.StandardChasingAOEs(module, 6.0f, (uint)AID.AiryPursuitAOE, (uint)AID.AiryPursuitTeleport, 3.5f,
    1.0d, 6, icon: (uint)IconID.AiryPursuit)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsChaserTarget(pc) || Chasers.Count == 0)
        {
            return;
        }

        var bait = Chasers[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            var onHitbox = (egg.Position - bait.PredictedPosition()).LengthSq() <= reach * reach;
            Arena.ZoneCircleOutline(egg.Position, egg.HitboxRadius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsChaserTarget(actor) || Chasers.Count == 0)
        {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsChaserTarget(actor) || Chasers.Count == 0)
        {
            return;
        }

        var bait = Chasers[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            hints.AddForbiddenZone(new SDCircle(egg.Position, reach), bait.NextActivation);
        }
    }
}

sealed class AiryPursuitBait(BossModule module) : Components.BaitAwayIcon(module, 6.0f, (uint)IconID.AiryPursuit)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AiryPursuitAOE or (uint)AID.AiryPursuitTeleport)
        {
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            var onHitbox = (egg.Position - bait.Target.Position).LengthSq() <= reach * reach;
            Arena.ZoneCircleOutline(egg.Position, egg.HitboxRadius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0)
        {
            return;
        }

        var bait = CurrentBaits[0];
        var baitRadius = ((AOEShapeCircle)bait.Shape).Radius;

        foreach (var egg in Module.Enemies([(uint)OID.PulletPieceEgg, (uint)OID.CockerelPieceEgg]))
        {
            var reach = baitRadius + egg.HitboxRadius;
            hints.AddForbiddenZone(new SDCircle(egg.Position, reach), bait.Activation);
        }
    }
}

sealed class Carve(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCone shape = new(15.0f, 90.0f.Degrees());
    private Actor? tetherTarget;
    private Actor? tetherSource;
    private DateTime activation;
    private Angle? rotation = default;
    private bool baitLocked = false;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is not (uint)TetherID.Carve and not (uint)TetherID.CarveStretched)
        {
            return;
        }

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
        {
            return;
        }

        tetherSource = source;
        tetherTarget = target;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FerociousForeCarve or (uint)AID.RampagingRearCarve)
        {
            activation = Module.CastFinishAt(spell).AddSeconds(1d);
            rotation = spell.Action.ID switch
            {
                (uint)AID.FerociousForeCarve => default,
                (uint)AID.RampagingRearCarve => 180f.Degrees(),
                _ => default
            };
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ForeCarve or (uint)AID.RearCarve)
        {
            tetherTarget = null;
            tetherSource = null;
            activation = default;
            rotation = default;
            baitLocked = false;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Featherglide)
        {
            baitLocked = true;
            tetherTarget = caster;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (tetherTarget == null || tetherSource == null || activation == default || rotation == null)
        {
            return [];
        }

        WPos origin;
        Angle offset;

        if (baitLocked)
        {
            origin = tetherSource.Position;
            offset = Module.PrimaryActor.Rotation + rotation.Value;
        }
        else
        {
            origin = tetherTarget.Position;
            offset = Angle.FromDirection(tetherTarget.Position - tetherSource.Position) + rotation.Value;
        }

        return new AOEInstance[] { new(shape, origin, offset, risky: actor != tetherTarget) };
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) { }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (tetherTarget == null || tetherSource == null || activation == default || rotation == null)
        {
            return;
        }

        WPos origin;
        Angle offset;

        if (baitLocked)
        {
            origin = tetherSource.Position;
            offset = Module.PrimaryActor.Rotation + rotation.Value;
            shape.Draw(Arena, origin, offset);
        }
        else
        {
            origin = tetherTarget.Position;
            offset = Angle.FromDirection(tetherTarget.Position - tetherSource.Position) + rotation.Value;
            shape.Outline(Arena, origin, offset);
        }
    }
}

sealed class ZuPieceStates : StateMachineBuilder
{
    public ZuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrossbreezeBait>()
            .ActivateOnEnter<Crossbreeze>()
            .ActivateOnEnter<FlyingFrenzy>()
            .ActivateOnEnter<AiryPursuitPuddles>()
            .ActivateOnEnter<AiryPursuit>()
            .ActivateOnEnter<AiryPursuitBait>()
            .ActivateOnEnter<Carve>()
            .ActivateOnEnter<CarveTether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ZuPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14572u, SortOrder = 4)]
public sealed class ZuPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.PulletPiece), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.CockerelPiece));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.PulletPiece => 3,
                (uint)OID.CockerelPiece => 2,
                (uint)OID.ZuPiece => 1,
                (uint)OID.PulletPieceEgg => AIHints.Enemy.PriorityForbidden,
                (uint)OID.CockerelPieceEgg => AIHints.Enemy.PriorityForbidden,
                _ => 0
            };
        }
    }

    private readonly string[] _prePullHints = [
        "Avoid breaking the eggs!",
        "When adds spawn kill order is the following: PulletPiece (purple) -> CockerelPiece -> Boss"
    ];

    public override string[] PrePullHints => _prePullHints;
}
