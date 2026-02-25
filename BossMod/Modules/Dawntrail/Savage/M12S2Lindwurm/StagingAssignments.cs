#pragma warning disable IDE0028
namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

public enum CloneShape
{
    Boss,
    Cone,
    Spread,
    Stack
}
abstract class StagingAssignment<TRole>(
    BossModule module,
    int playerGroupSize,
    int cloneGroupSize,
    bool hasBossTether)
    : BossComponent(module)
    where TRole : struct, Enum
{
    public readonly List<PlayerClone> PlayerClones = new();
    public readonly List<WurmClone> WurmClones = new();

    public readonly PlayerClone?[] PlayersBySlot = new PlayerClone?[8];
    public readonly WurmClone?[] WurmsBySlot = new WurmClone?[8];

    public bool PlayersAssigned { get; private set; }
    public bool WurmsAssigned { get; private set; }

    private bool _wurmsOrganized;
    public bool WatchSpawns = true;

    public class PlayerClone(Actor actor, Angle position, int spawnOrder)
    {
        public Actor Actor = actor;
        public Angle Position = position;
        public int SpawnOrder = spawnOrder;
        public Clockspot Clock = Clockspot.GetClosest(position);
        public TRole? WantedRole;
    }

    public class WurmClone(Actor actor, Angle position, int spawnOrder)
    {
        public Actor Actor = actor;
        public Angle Position = position;
        public int SpawnOrder = spawnOrder;
        public TRole AssignedRole;

        public bool Locked;
        public Actor? Target;
        public CloneShape? Shape;
    }

    private int _totalPlayerSpawns;
    private int _totalCloneSpawns;

    protected abstract TRole? DeterminePlayerRole(PlayerClone c);
    protected abstract TRole DetermineCloneRole(WurmClone w);

    // ============================================================
    // Spawn Tracking
    // ============================================================

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (WurmsAssigned)
            return;

        if ((OID)actor.OID == OID.Understudy && id == 0x11D2)
        {
            PlayerClones.Add(new(
                actor,
                (actor.Position - Arena.Center).ToAngle(),
                _totalPlayerSpawns++ / playerGroupSize));
        }

        if ((OID)actor.OID == OID.Lindschrat && id == 0x1E46 && WatchSpawns)
        {
            WurmClones.Add(new(
                actor,
                (actor.Position - Arena.Center).ToAngle(),
                _totalCloneSpawns++ / cloneGroupSize));
        }
    }

    // ============================================================
    // Tethers
    // ============================================================

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var tid = (TetherID)tether.ID;

        if (tid == TetherID.Fixed)
        {
            HandleFixedTether(source, tether.Target);
            return;
        }

        HandleShapeTether(source, tid, tether.Target);
    }

    private void HandleFixedTether(Actor source, ulong targetID)
    {
        if ((OID)source.OID == OID.Understudy)
        {
            var slot = Raid.FindSlot(targetID);
            if (slot >= 0 && !PlayersAssigned)
            {
                var clone = FindPlayerClone(source);
                if (clone != null)
                {
                    PlayersBySlot[slot] = clone;
                    AssignPlayers();
                }
            }
        }

        if ((OID)source.OID is OID.Lindschrat or OID.Boss)
        {
            var slot = Raid.FindSlot(targetID);
            if (slot >= 0)
            {
                var wurm = FindWurmClone(source);
                if (wurm != null)
                {
                    wurm.Locked = true;
                    WurmsBySlot[slot] = wurm;
                    AssignWurms();
                }
            }
        }
    }

    private void HandleShapeTether(Actor source, TetherID tid, ulong targetID)
    {
        CloneShape? shape = tid switch
        {
            TetherID.RepBoss => CloneShape.Boss,
            TetherID.RepCone => CloneShape.Cone,
            TetherID.RepStack => CloneShape.Stack,
            TetherID.RepSpread => CloneShape.Spread,
            _ => null
        };

        if (shape == null)
            return;

        var wurm = FindWurmClone(source);

        if (wurm != null)
        {
            wurm.Shape = shape.Value;
            wurm.Target = WorldState.Actors.Find(targetID);
            OrganizeWurms();
            return;
        }

        if (shape == CloneShape.Boss && source == Module.PrimaryActor)
        {
            WurmClones.Add(new(source, default, -1)
            {
                Shape = CloneShape.Boss,
                Target = WorldState.Actors.Find(targetID)
            });

            OrganizeWurms();
            return;
        }

        ReportError($"Tether {shape} from untracked actor {source}");
    }

    // ============================================================
    // Lookup Helpers (No LINQ)
    // ============================================================

    private PlayerClone? FindPlayerClone(Actor actor)
    {
        var list = PlayerClones;
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i].Actor == actor)
                return list[i];
        }
        return null;
    }

    private WurmClone? FindWurmClone(Actor actor)
    {
        var list = WurmClones;
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i].Actor == actor)
                return list[i];
        }
        return null;
    }

    // ============================================================
    // Assignment Logic
    // ============================================================

    private void AssignPlayers()
    {
        if (PlayersAssigned)
            return;

        for (var i = 0; i < PlayersBySlot.Length; ++i)
        {
            if (PlayersBySlot[i] == null)
                return;
        }

        for (var i = 0; i < PlayerClones.Count; ++i)
        {
            var clone = PlayerClones[i];
            clone.WantedRole = DeterminePlayerRole(clone);
        }

        PlayersAssigned = true;
    }

    private void OrganizeWurms()
    {
        if (_wurmsOrganized)
            return;

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            if (WurmClones[i].Shape == null)
                return;
        }

        if (hasBossTether)
        {
            var foundBoss = false;
            for (var i = 0; i < WurmClones.Count; ++i)
            {
                if (WurmClones[i].Shape == CloneShape.Boss)
                {
                    foundBoss = true;
                    break;
                }
            }

            if (!foundBoss)
                return;
        }

        for (var i = 0; i < WurmClones.Count; ++i)
            WurmClones[i].AssignedRole = DetermineCloneRole(WurmClones[i]);

        _wurmsOrganized = true;
    }

    private void AssignWurms()
    {
        if (WurmsAssigned)
            return;

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            if (!WurmClones[i].Locked)
                return;
        }

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            var w = WurmClones[i];
            if (w.Target == null)
            {
                ReportError($"Clone {w.Actor} has no target");
                continue;
            }

            var slot = Raid.FindSlot(w.Target.InstanceID);
            if (slot >= 0)
                WurmsBySlot[slot] = w;
        }

        WurmsAssigned = true;
    }

    // ============================================================
    // Hints & Drawing
    // ============================================================

    protected bool RoleEq(TRole? a, TRole b)
        => EqualityComparer<TRole?>.Default.Equals(a, b);

    protected string RoleReadable(TRole? a)
        => a == null ? "???" : UICombo.EnumString(a.Value);

    protected virtual IEnumerable<string> GetHelpHints(int slot, Actor actor)
    {
        var w = WurmsBySlot[slot];
        if (w != null)
        {
            yield return $"Clone: {RoleReadable(w.AssignedRole)}";
            yield break;
        }

        var p = PlayersBySlot[slot];
        if (p != null)
            yield return $"Position: {p.Clock}, role: {RoleReadable(p.WantedRole)}";
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var p = PlayersBySlot[slot];
        if (p == null)
            return;

        foreach (var hint in GetHelpHints(slot, actor))
            hints.Add(hint, false);

        if (WurmsAssigned)
            return;

        var correctExists = false;
        var wrongGrab = false;

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            var w = WurmClones[i];

            if (w.Target != null &&
                RoleEq(p.WantedRole, w.AssignedRole) &&
                w.Target != actor)
                correctExists = true;

            if (w.Target == actor &&
                !RoleEq(p.WantedRole, w.AssignedRole))
                wrongGrab = true;
        }

        if (correctExists)
            hints.Add("Grab correct tether!");

        if (wrongGrab)
            hints.Add("Pass tether!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var p = PlayersBySlot[pcSlot];
        if (p == null || WurmsAssigned)
            return;

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            var w = WurmClones[i];
            if (w.Locked || w.Target == null)
                continue;

            var correct = RoleEq(p.WantedRole, w.AssignedRole);

            var color = correct ? Colors.Safe : Colors.Danger;
            var thickness = w.Target == pc ? (correct ? 1 : 2) : (correct ? 2 : 1);

            DrawTether(w.Actor, w.Target, color, thickness);
            Arena.ActorInsideBounds(w.Actor.Position, w.Actor.Rotation, Colors.Object);
        }
    }

    private void DrawTether(Actor a, Actor b, uint color, float thickness)
    {
        if (BossModule.WindowConfig.ShowOutlinesAndShadows)
            Arena.AddLine(a.Position, b.Position, 0xFF000000, thickness + 1);

        Arena.AddLine(a.Position, b.Position, color, thickness);
    }
}