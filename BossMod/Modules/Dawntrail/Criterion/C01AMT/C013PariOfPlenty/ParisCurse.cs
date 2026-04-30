namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class GridMap {
    public WPos Center { get; set; }
    public int gridDimension { get; set; }
    public float cellSize { get; set; }
    private float totalSize { get; set; }
    public float halfSize { get; set; }
    
    public enum tileType { E = 0, B = 1, R = 2 }
    public tileType[] tiles;
    
    public GridMap(int gridDimension, float cellSize, WPos center) {
        this.Center = center;
        this.gridDimension = gridDimension;
        this.cellSize = cellSize;
        this.totalSize = gridDimension * cellSize;
        this.halfSize = totalSize / 2f;
        tiles = Enumerable.Repeat(tileType.E, gridDimension * gridDimension).ToArray();
    }
    
    public WPos gridToWorld(int gridCell) {
        float startX = Center.X - halfSize + cellSize / 2f;
        float startZ = Center.Z - halfSize + cellSize / 2f;
        
        float x = startX + (gridCell % gridDimension) * cellSize;
        float z = startZ + (gridCell / gridDimension) * cellSize;
        
        return new WPos(x, z);
    }
    
    public int worldToGrid(WPos worldPos) {
        float startX = Center.X - halfSize + (cellSize / 2f);
        float startZ = Center.Z - halfSize + (cellSize / 2f);
        
        float relX = worldPos.X - startX;
        float relZ = worldPos.Z - startZ;
        
        int x = (int)MathF.Floor(relX / cellSize);
        int z = (int)MathF.Floor(relZ / cellSize);
        
        x = Math.Clamp(x, 0, gridDimension - 1);
        z = Math.Clamp(z, 0, gridDimension - 1);
        return z * gridDimension + x;
    }
    
    public bool gridCellIntersectsShape(AOEShape shape, WPos pos, Angle angle, int gridCell) {
        WPos center = gridToWorld(gridCell);

        if (shape.Check(pos, center, angle)) {
            return true;
        }
        
        var corners = new[] {
            new WPos(center.X - cellSize / 2f, center.Z - cellSize / 2f),
            new WPos(center.X - cellSize / 2f, center.Z + cellSize / 2f),
            new WPos(center.X + cellSize / 2f, center.Z - cellSize / 2f),
            new WPos(center.X + cellSize / 2f, center.Z + cellSize / 2f)
        };

        foreach (WPos corner in corners) {
            if (shape.Check(corner, pos, angle)) {
                return true;
            }
        }
        
        return false;
    }
    
    public void markTiles(AOEShape shape, WPos origin, Angle angle, tileType t) {
        for (int i = 0; i < gridDimension * gridDimension; ++i) {
            if (gridCellIntersectsShape(shape, origin, angle, i)) {
                if (t == tileType.R) {
                    tiles[i] = tileType.R; // Red overrides everything for easier detecting of L shape
                } else if (t == tileType.B && tiles[i] != tileType.R) {
                    tiles[i] = tileType.B;
                }
            }
        }
    }
    
    public List<int[]> FindSafeSpotsFromRedIndices() {
        var results = new List<int[]>();
        var seen = new HashSet<(int, int, int)>();

        for (int y = 0; y < gridDimension; y++) {
            for (int x = 0; x < gridDimension; x++) {
                int[][] orientations = {
                    new[] { y * gridDimension + x, y * gridDimension + x + 1, y * gridDimension + x + 2, (y + 1) * gridDimension + x, (y + 2) * gridDimension + x }, // right & down
                    new[] { y * gridDimension + x, y * gridDimension + x + 1, y * gridDimension + x + 2, (y - 1) * gridDimension + x, (y - 2) * gridDimension + x }, // right & up
                    new[] { y * gridDimension + x, y * gridDimension + x - 1, y * gridDimension + x - 2, (y + 1) * gridDimension + x, (y + 2) * gridDimension + x }, // left & down
                    new[] { y * gridDimension + x, y * gridDimension + x - 1, y * gridDimension + x - 2, (y - 1) * gridDimension + x, (y - 2) * gridDimension + x }  // left & up
                };

                foreach (var shapeL in orientations) {
                    if (shapeL.Any(id => id < 0 || id >= gridDimension * gridDimension)) {
                        continue;
                    }
                    
                    int p0 = shapeL[0], p1 = shapeL[1], p2 = shapeL[2], p3 = shapeL[3], p4 = shapeL[4];
                    
                    // Looking for L shape where there is a red tile between each other tile
                    if (tiles[p1] != tileType.R || tiles[p3] != tileType.R) {
                        continue;
                    }
                    
                    int blueTiles = 0, emptyTiles = 0;
                    
                    // Other points must be either empty or blue resulting in 2 blue & 1 empty
                    if (tiles[p0] == tileType.B) { blueTiles++; }
                    if (tiles[p2] == tileType.B) { blueTiles++; }
                    if (tiles[p4] == tileType.B) { blueTiles++; }
                    
                    if (tiles[p0] == tileType.E) { emptyTiles++; }
                    if (tiles[p2] == tileType.E) { emptyTiles++; }
                    if (tiles[p4] == tileType.E) { emptyTiles++; }
                    

                    if (blueTiles != 2 || emptyTiles != 1) {
                        continue;
                    }

                    int empty = -1, blue1 = -1, blue2 = -1;
                    foreach (var i in new[] { p0, p2, p4 }) {
                        if (tiles[i] == tileType.E) {
                            empty = i;
                        }
                        
                        if (tiles[i] == tileType.B) {
                            (blue1 == -1 ? ref blue1 : ref blue2) = i;
                        }
                    }
                    
                    // Sort the order to avoid dupes
                    int minB = Math.Min(blue1, blue2);
                    int maxB = Math.Max(blue1, blue2);

                    if (seen.Add((minB, maxB, empty))) {
                        results.Add(new[] { blue1, blue2, empty });
                    }
                }
            }
        }
        
        return results;
    }        
}

class ParisCurse(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private Actor? blueCrystal;
    private Actor? blueCrystalTracker; // Carpet that covers the blueCrystal
    private float blueCrystalTrackerDistance = float.MaxValue;
    private List<Actor> redCrystals = new();
    
    GridMap? gridMap = null;
    private enum SafeHalf { Unknown, North, South }
    private SafeHalf safeHalf = SafeHalf.Unknown;
    private List<int[]> safeTiles = new();
    
    private int? assignment;
    
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.ChillingGleam) {
            NumCasts++;
        }
        
        if ((AID)spell.Action.ID == AID.CharmingBaubles) {
            Actor? actor = Module.WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.IcyBauble);
            if (actor != null) {
                blueCrystal = actor;
            }
        }
        
        if ((AID)spell.Action.ID == AID.CarpetTeleport) {
            if (blueCrystal == null) {
                return;
            }
            
            WDir difference = caster.Position - blueCrystal.Position;
            float distance = difference.Length();
            
            if (distance < blueCrystalTrackerDistance) {
                blueCrystalTrackerDistance = distance;
                blueCrystalTracker = caster;
            }
        }
    }
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.ParisCurse) {
            gridMap = new GridMap(4, 10, Module.Center);
        }
        
        if ((AID)spell.Action.ID == AID.Unravel) {
            safeTiles = generateSafeSpots();
            assignment = ResolvePlayerAssignment(safeTiles);
        }
        
        if ((AID)spell.Action.ID == AID.BurningGleam) {
            redCrystals.Add(caster);
        }
        
        if ((AID)spell.Action.ID == AID.CurseFableflightRight) {
            safeHalf = SafeHalf.North;
        }
        
        if ((AID)spell.Action.ID == AID.CurseFableflightLeft) {
            safeHalf = SafeHalf.South;
        }
    }
    
    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaBackground(pcSlot, pc); // TODO is this needed anymore?

        if (gridMap == null) {
            return;
        }
        
        float startX = gridMap.Center.X - gridMap.halfSize + gridMap.cellSize / 2f;
        float startZ = gridMap.Center.Z - gridMap.halfSize + gridMap.cellSize / 2f;

        for (int z = 0; z < gridMap.gridDimension; ++z) {
            for (int x = 0; x < gridMap.gridDimension; ++x) {
                WPos pos = new WPos(startX + x * gridMap.cellSize, startZ + z * gridMap.cellSize);
                Arena.AddRect(pos, new WDir(1,0), gridMap.cellSize / 2f, gridMap.cellSize / 2f, gridMap.cellSize / 2f, 0xffffffff, 2);
            }
        }
    }
    
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        if (gridMap == null) {
            return CollectionsMarshal.AsSpan(aoes);
        }
        
        AOEShapeRect cellShape = new(gridMap.cellSize / 2f, gridMap.cellSize / 2f, gridMap.cellSize / 2f);

        if (assignment.HasValue) {
            aoes.Add(new AOEInstance(cellShape, gridMap.gridToWorld(assignment.Value), default, WorldState.CurrentTime, Colors.SafeFromAOE));
            return CollectionsMarshal.AsSpan(aoes);
        }

        if (safeTiles.Count > 0) {
            foreach (var tile in safeTiles[0]) {
                uint colour = gridMap.tiles[tile] == GridMap.tileType.B ? Color.FromRGBA(0x268BD280).ABGR : Colors.SafeFromAOE;
                aoes.Add(new AOEInstance(cellShape, gridMap.gridToWorld(tile), default, WorldState.CurrentTime, colour));
            }
        }
        
        return CollectionsMarshal.AsSpan(aoes);
    }
    
    private List<int[]> generateSafeSpots() {
        if (gridMap == null || blueCrystalTracker == null) {
            return [];
        }
        
        gridMap.markTiles(new AOEShapeCross(40, 4), blueCrystalTracker.Position, 0.Degrees(), GridMap.tileType.B);
        foreach (Actor actor in redCrystals) {
            gridMap.markTiles(new AOEShapeCross(40, 4), actor.Position, 0.Degrees(), GridMap.tileType.R);
        }
        
        var safeSpots = gridMap.FindSafeSpotsFromRedIndices();
        var candidates = safeSpots;
        
        if (candidates.Count > 0) {
            var allowed = safeHalf == SafeHalf.North ? new[] { 0,1,2,3,4,5,6,8,9,12 } : new[] { 3,6,7,9,10,11,12,13,14,15 };
            candidates = safeSpots.Where(tile => allowed.Contains(tile[0]) && allowed.Contains(tile[1]) && allowed.Contains(tile[2])).ToList();
        }
        
        if (safeSpots.Count == 0) {
            return [];
        }
        
        return candidates;
    }

    // To check if each player in the group has a unique role
    private bool checkAssignmentRoles(PartyState partyState) {
        var roles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(partyState);
        var tankRole = false;
        var healerRole = false;
        var meleeRole = false;
        var rangedRole = false;
        
        for (var i = 0; i < PartyState.MaxPartySize; i++) {
            switch (roles[i]) {
                case Role.Tank:
                    tankRole = true;
                    break;
                case Role.Healer:
                    healerRole = true;
                    break;
                case Role.Melee:
                    meleeRole = true;
                    break;
                case Role.Ranged:
                    rangedRole = true;
                    break;
            }
        }

        if (!tankRole || !healerRole || !meleeRole || !rangedRole) {
            Service.Log("Missing unique roles - each player must have a unique role!");
            return false;
        }

        return true;
    }
    
    // Group assignment
    static int RolePriority(Role r) => r switch {
        Role.Tank => 0,
        Role.Melee => 1,
        Role.Ranged => 2,
        Role.Healer => 3,
        _ => -1
    };
    
    // Sorts the safe spots into north relative order
    private int[] sortSafeSpotsOrder(List<int[]> safeSpots) {
        foreach (var s in safeSpots) {
            var tiles = new[] { s[0], s[1], s[2] };
            var northNumber = (safeHalf == SafeHalf.South) ? tiles.Min() : tiles.Max(); // Depending on the safe side where which tile we should treat as new north
            var otherTiles = tiles.Where(t => t != northNumber).ToArray();
            
            // Depending on the safe side depends on the second tile -> min is north safe, max is south safe
            int secondTile = (safeHalf == SafeHalf.North) ? Math.Min(otherTiles[0], otherTiles[1]) : Math.Max(otherTiles[0], otherTiles[1]);
            int thirdTile = (otherTiles[0] == secondTile) ? otherTiles[1] : otherTiles[0]; // Remaining tile
            var ordered = new[] {northNumber, secondTile, thirdTile};
            return ordered;
        }

        return [];
    }

    bool spreadPlayer(Actor player) => player.FindStatus(SID.CurseOfSolitude) != null;
    bool stackPlayer(Actor player) => player.FindStatus(SID.CurseOfCompanionship) != null;
    bool firePlayer(Actor player) => player.FindStatus(SID.CurseOfImmolation) != null;
    
    private int? ResolvePlayerAssignment(List<int[]> safeSpots) {
        if (gridMap == null) {
            return null;
        }
        
        if (checkAssignmentRoles(Raid) == false) {
            return null;
        }
        
        // Get safe spots
        var emptyTile = 0;
        var blueTiles = new List<int>();
        
        var safeSpotsOrder = sortSafeSpotsOrder(safeSpots);
        foreach (var safeSpot in safeSpotsOrder) {
            if (gridMap.tiles[safeSpot] == GridMap.tileType.E) {
                emptyTile = safeSpot;
            }

            if (gridMap.tiles[safeSpot] == GridMap.tileType.B) {
                blueTiles.Add(safeSpot);
            }
        }
        
        // Get party and their debuffs
        List<Actor> party = WorldState.Party.WithoutSlot().ToList();
        var roles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(Raid);

        var players = party.Select(p => new {
            Actor = p,
            Fire = firePlayer(p),
            Stack = stackPlayer(p),
            Spread = spreadPlayer(p),
            Priority = RolePriority(roles[Raid.FindSlot(p.InstanceID)])
        }).ToList();
        
        Actor? actor = WorldState.Party.Player();
        if (actor == null) {
            return null;
        }
        var player = players.First(p => p.Actor.InstanceID == actor.InstanceID);
        
        // No fire debuff -> assign to empty tile
        if (player.Fire == false) {
            return emptyTile;
        }

        // Fire debuff -> assign to one of the blue tiles
        if (player.Fire == true) {
            var firePlayers = players.Where(p => p.Fire == true).OrderBy(p => p.Priority).ToList();
            
            // Case 1: There are only 2 fire debuff players -> means both of them are spreads as well -> normal TMRH priority
            if (firePlayers.Count == 2) {
                int tileNumber = firePlayers.FindIndex(p => p.Actor.InstanceID == player.Actor.InstanceID);
                return blueTiles[tileNumber];
            }

            // Case 2: There are 3 fire debuff players -> means on the pair is stack/nothing and the other is a spread
            // Stack + nothing takes the first blue tile & spread takes the second blue tile
            if (firePlayers.Count == 3) {
                var spread = firePlayers.First(p => p.Spread);
                return player.Actor.InstanceID == spread.Actor.InstanceID ? blueTiles[1] : blueTiles[0];
            }
        }
        
        return null;
    }
}

class Fableflight : Fireflight {
    public Fableflight(BossModule module) : base(module) { }
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.CurseFableflightLeft) {
            side = -1;
            return;
        }
        
        if ((AID)spell.Action.ID == AID.CurseFableflightRight) {
            side = 1;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) { }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.CarpetRideTether) {
            PathAOE pathAoe = new PathAOE();
            Actor? actor = WorldState.Actors.Find(tether.Target);

            if (actor == null) {
                return;
            }
            
            pathAoe.actor = actor;
            pathAoe.startPosition = source.Position;
            pathAoe.endPosition = actor.Position;
            pathAoe.aoePosition = pathAOEs.Count + 1;
            pathAOEs.Add(pathAoe);
        }
    }

    // This function is needed for this version as "OnUntethered" doesn't happen until the AOE resolves, which is too late
    // and OnTethered is normally too early before the actor moves giving the wrong position
    public override void Update() {
        foreach (var pathAOE in pathAOEs) {
            var target = pathAOE.actor;
            if (target == null) {
                continue;
            }
            
            var pos = target.Position;
            if (!pathAOE.endPosition.Equals(pos)) {
                pathAOE.endPosition = pos;
            }
        }
    }
}