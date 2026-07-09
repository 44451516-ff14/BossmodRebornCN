namespace BossMod.Dawntrail.Ultimate.DMU;

// Used for displaying the casts after they have locked in
class AllThingsEndingCasts(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AllThingsEnding, (uint)AID.AllThingsEnding1], new AOEShapeCone(100, 90.Degrees())) {

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.AllThingsEnding || spell.Action.ID == (uint)AID.AllThingsEnding1) {
            NumCasts++;
        }
    }
}

// Used for displaying where the baits for the casts currently are before they lock in
// Baits are guessed by closest player to the clone
class AllThingsEnding(BossModule module) : Components.GenericBaitAway(module, onlyShowOutlines: true) {
    private enum baitType { None, Far, Close }
    private baitType currentBait = baitType.None;
    private List<Actor> clones = []; // Also includes the boss since he will cast the same spell
    private List<Actor> baiters = []; // players currently baiting
    private WPos? lastKnownTowerMidPoint = null;
    public bool aoesLocked = true; // Used to prevent the aoes baits constantly showing

    private PathOfLight? towers = module.FindComponent<PathOfLight>();
    private ForsakenShapes? shapes = module.FindComponent<ForsakenShapes>();
    private readonly DMUConfig dmuConfig = Service.Config.Get<DMUConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FuturesEndCast) {
            currentBait = baitType.Far;
        }

        if (spell.Action.ID == (uint)AID.PastsEndCast) {
            currentBait = baitType.Close;
        }

        if (spell.Action.ID == (uint)AID.AllThingsEnding || spell.Action.ID == (uint)AID.AllThingsEnding1) {
            aoesLocked = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.PastsEndSpread || spell.Action.ID == (uint)AID.PastsEndSpread1 ||
            spell.Action.ID == (uint)AID.FuturesEndSpread || spell.Action.ID == (uint)AID.FuturesEndSpread1) {
            clones.Add(caster);
        }
    }

    public override void Update() {
        CurrentBaits.Clear();
        baiters.Clear();

        if (aoesLocked == true) {
            return;
        }

        foreach (var clone in clones) {
            var baiter = Raid.WithoutSlot().Where(p => !baiters.Contains(p)).SortedByRange(clone.Position).Take(1).FirstOrDefault();
            if (baiter == null) {
                continue;
            }

            baiters.Add(baiter);
            var direction = clone.AngleTo(baiter);
            if (currentBait == baitType.Close) {
                direction = direction + 180.Degrees();
            }
            CurrentBaits.Add(new(clone.Position, baiter, new AOEShapeCone(25, 90.Degrees()), customRotation: direction));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (aoesLocked == true) {
            return;
        }

        if (towers == null || shapes == null) {
            return;
        }

        if (shapes.currentTowerSet % 2 == 0 && shapes.currentTowerSet != 8) {
            return;
        }

        // Final set displays safe spots differently compared to every other tower set
        if (shapes.currentTowerSet == 9) {
            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_Markerless ||
                dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Meow_DN_ZENITH_Markers) {
                var waymark = WorldState.Waymarks.GetFieldMark((int)Waymark.A);
                if (waymark == null) {
                    return;
                }

                Arena.AddCircle(waymark.Value.ToWPos(), 1.0f, Colors.Safe, 2.0f);
                return;
            }

            if (dmuConfig.P2Forsaken == DMUConfig.P2ForsakenStrategy.Kroxy_Rinon_Melee_Flex) {
                if (lastKnownTowerMidPoint != null) {
                    baitSafeSpot(lastKnownTowerMidPoint.Value);
                }
            }

            return;
        }

        if (towers.Towers.Count != 2 || towers.CurrentSE == null || towers.CurrentSW == null) {
            return;
        }

        var midpoint = new WPos((towers.CurrentSW.Value.Position.X + towers.CurrentSE.Value.Position.X) * 0.5f,
                                (towers.CurrentSW.Value.Position.Z + towers.CurrentSE.Value.Position.Z) * 0.5f);
        lastKnownTowerMidPoint = midpoint;

        if (shapes.currentTowerSet == 8) { // Just to collect the lastKnownTowerMidPoint otherwise it will be null
            return;
        }

        baitSafeSpot(midpoint);
    }

    private void baitSafeSpot(WPos midPoint) {
        var newSouth = (midPoint - Arena.Center).Normalized();
        if (currentBait == baitType.Close) {
            Arena.AddCircle(midPoint + (newSouth * 2.0f), 1.0f, Colors.Safe, 2.0f);
        }

        if (currentBait == baitType.Far) {
            Arena.AddCircle(midPoint - (newSouth * 13.0f), 1.0f, Colors.Safe, 2.0f);
        }
    }
}
