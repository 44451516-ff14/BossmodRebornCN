namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE208FamiliarTactics;

public enum OID : uint
{
    ElmGigas = 0x4BD9,
    Helper = 0x233C,
    ElmGigasPuddle = 0x4BDA, // R4.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50851, // ElmGigas->player, no cast, single-target
    AncientAeroIII = 47544, // ElmGigas->self, 3.5+1.5s cast, single-target
    AncientAeroIIIVisual = 48041, // Helper->self, 5.0s cast, ???
    SpinningSweep = 47541, // ElmGigas->self, 6.0s cast, range 40 120.000-degree cone
    InspiritedCrosswindsCast = 47533, // ElmGigas->self, 6.0+0.8s cast, single-target
    InspiritedCrosswinds = 47535, // 4BDA->self, 6.0s cast, range 60 width 8 cross
    InspiritedImpactCast = 47542, // ElmGigas->self, 3.0s cast, single-target
    InspiritedImpact = 47543, // Helper->self, 9.6s cast, range 25 circle
    InspiritedHurricaneCast = 47536, // ElmGigas->self, 4.3+0.7s cast, single-target
    InspiritedHurricaneCross = 47538, // Helper->self, 5.0s cast, range 60 width 10 cross
    InspiritedHurricaneCircle = 47537, // Helper->self, 5.0s cast, range 12 circle
    AncientAero = 47540, // Helper->self, 3.0s cast, range 70 width 6 rect
    InspiritedCycloneCast = 47532, // ElmGigas->self, 5.0+1.0s cast, single-target
    InspiritedCyclone = 47534, // 4BDA->self, 6.0s cast, range 12 circle
    UnbowedSpiritCast = 47530, // ElmGigas->self, 3.0+1.0s cast, single-target
    UnbowedSpirit = 47531, // Helper->self, no cast, range 4 circle
}

[SkipLocalsInit]
sealed class AncientAeroIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.AncientAeroIII);
[SkipLocalsInit]
sealed class SpinningSweep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningSweep, new AOEShapeCone(40.0f, 60.0f.Degrees()));
[SkipLocalsInit]
sealed class InspiritedCrosswinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedCrosswinds, new AOEShapeCross(60.0f, 4.0f));
[SkipLocalsInit]
sealed class InspiritedHurricaneCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedHurricaneCross, new AOEShapeCross(60.0f, 5.0f));
[SkipLocalsInit]
sealed class InspiritedHurricaneCircleCyclone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.InspiritedHurricaneCircle, (uint)AID.InspiritedCyclone], 12f);
[SkipLocalsInit]
sealed class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(70.0f, 3.0f));

[SkipLocalsInit]
sealed class UnbowedSpirit(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoes = [];
    private readonly List<Actor> puddles = [];
    private bool circular;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ElmGigasPuddle)
        {
            if (puddles.Count == 0)
            {
                var offset = actor.Position - Arena.Center;
                circular = MathF.Abs(offset.X % 10f) > 1f || MathF.Abs(offset.Z % 10f) > 1f;
            }

            puddles.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.InspiritedCrosswinds or (uint)AID.InspiritedCyclone)
        {
            if (puddles.Count > 0)
            {
                puddles.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoes;
    }

    public override void Update()
    {
        var countP = puddles.Count;
        if (countP == 0 && _aoes.Length != 0)
        {
            _aoes = [];
        }
        var center = Arena.Center;
        _aoes = new AOEInstance[countP];
        for (var i = 0; i < countP; ++i)
        {
            var puddle = puddles[i];
            var pos = puddle.Position;
            var rot = puddle.Rotation;
            if (circular)
            {
                var offset = pos - center;
                var angleDirection = offset.Cross(rot.ToDirection()) > 0f;
                var length = 4f / offset.Length();
                var lengthDirection = (angleDirection ? -length : length).Radians();
                _aoes[i] = new(new AOEShapeArcCapsule(4.2f, lengthDirection, center), pos, rot, color: Colors.Danger);
            }
            else
            {
                _aoes[i] = new(new AOEShapeCapsule(4.2f, 4f), pos, rot, color: Colors.Danger);
            }
        }
    }
}

[SkipLocalsInit]
sealed class InspiritedImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InspiritedImpact, 25f, 3);

[SkipLocalsInit]
sealed class CE208FamiliarTacticsStates : StateMachineBuilder
{
    public CE208FamiliarTacticsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<SpinningSweep>()
            .ActivateOnEnter<InspiritedCrosswinds>()
            .ActivateOnEnter<InspiritedImpact>()
            .ActivateOnEnter<InspiritedHurricaneCross>()
            .ActivateOnEnter<InspiritedHurricaneCircleCyclone>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<UnbowedSpirit>();
    }
}

//TODO: Needs extended moving AOE support- once implemented can be moved to Verified after testing
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE208FamiliarTacticsStates),
    ConfigType = null, // replace null with typeof(ElmGigasConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ElmGigas,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 58u,
    SortOrder = 10,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE208FamiliarTactics : BossModule
{
    public CE208FamiliarTactics(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private CE208FamiliarTactics(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-390f, 700f), 29.5f, 32)]);
        return (arena.Center, arena);
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
