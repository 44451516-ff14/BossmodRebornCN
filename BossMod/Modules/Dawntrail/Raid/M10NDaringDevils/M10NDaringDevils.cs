namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// =========================
// Core mechanic components
// =========================

// Shared tankbuster (Red Hot -> players, 5s cast, range 6 circle)
sealed class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.HotImpact, 6f);

// Tankbuster bait (Deep Blue -> player, 5s cast, range 6 circle)
sealed class DeepImpact(BossModule module) : Components.BaitAwayCast(
    module,
    (uint)AID.DeepImpact,
    6f,
    centerAtTarget: true,
    endsOnCastEvent: true,
    tankbuster: true,
    damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class DiversDare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDare);
sealed class DiversDareBlue(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiversDare1);

sealed class DeepVarialCone(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.DeepVarial1,
    new AOEShapeCone(60f, 60f.Degrees()));

sealed class SickestTakeOffLine(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.SickestTakeOff1,
    new AOEShapeRect(50f, 7.5f));

sealed class SickSwellKB(BossModule module) : Components.SimpleKnockbacks(
    module,
    (uint)AID.SickSwell1,   // boss cast is the clean trigger
    distance: 15f,
    kind: Components.SimpleKnockbacks.Kind.DirForward,
    stopAtWall: false); // outside wall is deadly
// Pyrotation stack marker (helper->players, no cast, range 6 circle).
sealed class PyrotationStack(BossModule module) : Components.StackWithIcon(
    module,
    (uint)IconID.FireStack, // 659
    (uint)AID.Pyrotation1,
    activationDelay: 5f,
    radius: 6f,
    minStackSize: 2,
    maxStackSize: 8);

    // Persistent puddles created when Pyrotation resolves; cleared by Divers' Dare.
sealed class PyrotationPuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private static readonly AOEShapeCircle Shape = new(6f);

    private Actor? _stackTarget; // icon-marked player (659)

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_puddles);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FireStack) // 659
            _stackTarget = actor;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Pyrotation1:
            {
                var t = _stackTarget;
                if (t != null)
                    _puddles.Add(new(Shape, t.Position, default, WorldState.CurrentTime, Colors.AOE, true));

                _stackTarget = null; // consume
                break;
            }

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                _stackTarget = null;
                break;
        }
    }
}
sealed class SteamBurst(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.SteamBurst,
    new AOEShapeCircle(9f));
// Xtreme Spectacular “no cast” hits (these appear as event casts).
// Xtreme Spectacular edge - Proximity AOE so set it to 18 to mark the last safe spot in the 40-width rect.
sealed class XtremeSpectacularEdge(BossModule module) : Components.SimpleAOEs(
    module,
    (uint)AID.XtremeSpectacular2,
    new AOEShapeRect(50f, 18f));
sealed class XtremeSpectacularHits(BossModule module) : Components.CastCounterMulti(module, new uint[] { (uint)AID.XtremeSpectacular3, (uint)AID.XtremeSpectacular4 })
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        
        if (NumCasts > 0)
            hints.Add("Raidwide damage - Healer Intensive! Use Cooldowns! (Xtreme Spectacular)");
    }
}

// =========================
// Module
// =========================

[ModuleInfo(
    BossModuleInfo.Maturity.WIP,
    StatesType = typeof(M10NDaringDevilsStates),
    ConfigType = null,
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.RedHot,
    Contributors = "JoeSparkx",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Raid,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1070u,
    NameID = 14370u)]
public sealed class M10NDaringDevils(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public Actor? DeepBlue { get; private set; }

    protected override void UpdateModule()
    {
        // cache secondary boss
        DeepBlue ??= GetActor((uint)OID.DeepBlue);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (DeepBlue != null)
            Arena.Actor(DeepBlue);
        Arena.Actor(PrimaryActor);
    }
}

static class ActorExt
{
    public static bool HasStatus(this Actor a, uint sid) => a.FindStatus(sid) != null;
    public static bool HasIcon(this Actor a, uint iconID) => a.IncomingEffects.Any(e => e.Action.ID == iconID);
}
