namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

public class Doubling(BossModule module) : Components.CastTowers(module, (uint)AID.TowerExplosion, 4) {
    private WPos southPos = new(-759.988f, -785.093f);
    private WPos westPos  = new(-779.885f, -804.986f);
    private WPos eastPos  = new(-740.095f, -805.000f);
    private WPos northPos = new(-760.000f, -824.900f);
    
    public int side; // 1 -> NE & SW, 2 -> NW & SE
    private enum roleDirection { North, East, South, West }
    public enum towerSide { East, West }
    
    private Dictionary<ulong, roleDirection> tetheredRoles = new();
    private List<ulong> eastPlayers = new();
    private List<ulong> westPlayers = new();
    private List<ulong> priorityPlayers = new ();
    
    private List<Tower> eastTowers = new();
    private List<Tower> westTowers = new();
    private bool towerWave1 = false;
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        base.OnCastStarted(caster, spell);

        // Only check the north actor to see which side is safe since
        if ((AID)spell.Action.ID == AID.FableflightLeft || (AID)spell.Action.ID == AID.FableflightLeft1) {
            if (caster.Position.AlmostEqual(northPos, 1.0f)) {
                side = 1;
            }
        }
        
        if ((AID)spell.Action.ID == AID.FableflightRight || (AID)spell.Action.ID == AID.FableflightRight1) {
            if (caster.Position.AlmostEqual(northPos, 1.0f)) {
                side = 2;
            }
        }
    }
    
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        base.OnCastFinished(caster, spell);
        
        if (spell.Action.ID == (uint)AID.TowerExplosion) {
            if (towerWave1 == false) {
                towerWave1 = true;
                priorityPlayers.RemoveAt(0);
                priorityPlayers.RemoveAt(0);
                eastTowers.Clear();
                westTowers.Clear();
            }
        }
    }
    
    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.TowerTether) {
            WPos position = source.Position;

            if (position.AlmostEqual(northPos, 1.0f)) {
                tetheredRoles.TryAdd(tether.Target, roleDirection.North);
            }

            if (position.AlmostEqual(eastPos, 1.0f)) {
                tetheredRoles.TryAdd(tether.Target, roleDirection.East);
            }

            if (position.AlmostEqual(westPos, 1.0f)) {
                tetheredRoles.TryAdd(tether.Target, roleDirection.West);
            }

            if (position.AlmostEqual(southPos, 1.0f)) {
                tetheredRoles.TryAdd(tether.Target, roleDirection.South);
            }
            
            if (tetheredRoles.Count == 4) {
                foreach (var player in tetheredRoles.Keys) {
                    var role = tetheredRoles[player];
                    
                    var westSide = side == 1 ? 
                        (role == roleDirection.West || role == roleDirection.South) : 
                        (role == roleDirection.North || role == roleDirection.West);
                    
                    if (westSide) {
                        westPlayers.Add(player);
                    } else {
                        eastPlayers.Add(player);
                    }
                }
            }
        }
    }
    
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if ((IconID)iconID == IconID.BlueRug) {
            priorityPlayers.Add(targetID);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (eastTowers.Count != 2 || westTowers.Count != 2) {
            sortTowerOrder();
        }
        
        bool priority = priorityPlayers.Take(2).Contains(pc.InstanceID);
        bool eastPlayer = eastPlayers.Contains(pc.InstanceID);
        var list = eastPlayer ? eastTowers : westTowers;

        ulong targetId = 0;
        if (list.Count >= 2) {
            targetId = list[priority ? 0 : 1].ActorID;
        }

        foreach (Tower tower in Towers.Take(4)) {
            uint colour = tower.ActorID == targetId ? Colors.Safe : Colors.Danger;
            tower.Shape.Outline(Arena, tower.Position, tower.Rotation, colour, 2f);
        }
    }

    // Orders the tower on that specific side, with the 1st slot being the closer tower & the 2nd slot being the further tower
    private void sortTowerOrder() {
        eastTowers.Clear();
        westTowers.Clear();
        
        foreach (var tower in Towers) {
            if (tower.Position.X > Module.Center.X) {
                eastTowers.Add(tower);
            } else {
                westTowers.Add(tower);
            }
        }
        
        eastTowers.Sort(((a, b) => {
            var compare = sortTowerAngle(a, towerSide.East).Deg.CompareTo(sortTowerAngle(b, towerSide.East).Deg);
            return compare;
        }));

        westTowers.Sort(((a, b) => {
            var compare = sortTowerAngle(a, towerSide.West).Deg.CompareTo(sortTowerAngle(b, towerSide.West).Deg);
            return compare;
        }));
    }

    // Helper function for sorting the towers base on their angle depending on the safe side
    private Angle sortTowerAngle(Tower tower, towerSide towerSide) {
        var position = tower.Position;
        var angle = Angle.FromDirection(position - Module.Center);

        Angle startAngle;
        bool clockwise;

        if (towerSide == towerSide.East) {
            startAngle = side == 1 ? 180f.Degrees() : 0f.Degrees();
            clockwise = side == 1;
        } else {
            startAngle = side == 1 ? 0f.Degrees() : 180f.Degrees();
            clockwise = side == 1; 
        }
        
        Angle angleDifference = startAngle.DistanceToAngle(angle);
        if (clockwise) {
            angleDifference = -angleDifference;
        }

        if (angleDifference < 0.Degrees()) {
            angleDifference += 360f.Degrees();
        }

        return angleDifference;
    }
}