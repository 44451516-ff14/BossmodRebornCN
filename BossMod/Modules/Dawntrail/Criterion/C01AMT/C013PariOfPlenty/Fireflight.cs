namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class Fireflight(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    public List<PathAOE> pathAOEs = new();
    public int side = 0;

    public class PathAOE() {
        public Actor? actor;
        public WPos startPosition;
        public WPos endPosition;
        public int aoePosition;
    }
    
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.FireflightByPyrelightLeft || (AID)spell.Action.ID == AID.FireflightByEmberlightLeft) {
            side = -1;
            return;
        }

        if ((AID)spell.Action.ID == AID.FireflightByPyrelightRight || (AID)spell.Action.ID == AID.FireflightByEmberlightRight) {
            side = 1;
        }
    }
    
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.CarpetRide1 || (AID)spell.Action.ID == AID.CarpetRide2) {
            if (pathAOEs.Count > 0) {
                pathAOEs.RemoveAt(0);
                NumCasts++;
            }
        }
    }
    
    public override void OnUntethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.CarpetRideTether) {
            Service.Log($"tether {source.Position}");
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
    
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();
        
        Angle dirOffset = side == 1 ? 90.Degrees() : (-90).Degrees();

        int shown = 0;
        foreach (var pathAOE in pathAOEs) {
            if (shown >= 2) {
                break;
            }
            
            var nextDir = (pathAOE.endPosition - pathAOE.startPosition);
            Angle nextAng = nextDir.ToAngle();
            Angle baseAng = 0.Degrees();
            Angle delta = baseAng.DistanceToAngle(nextAng);
            Angle final = (baseAng + delta).Normalized();

            var halfWidth = pathAOE.aoePosition == 2 ? 27 : 60;
            uint colour = (shown < 1) ? Colors.Danger : Colors.AOE;
            
            var shape = new AOEShapeRect(40, halfWidth, 0f, dirOffset);
            aoes.Add(new AOEInstance(shape, pathAOE.startPosition, final, default, colour, (shown < 1)));
            shown++;
        }
        
        return CollectionsMarshal.AsSpan(aoes);
    }
}

// TODO find out who actually gets the stack - is it random?
class FireflightStackSpread(BossModule module) : Components.UniformStackSpread(module, 3, 3) {
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.FireflightByPyrelightLeft || (AID)spell.Action.ID == AID.FireflightByPyrelightRight) {
            Actor? stackPlayer = Raid.WithSlot().FirstOrDefault().Item2;
            AddStack(stackPlayer);
            return;
        }

        if ((AID)spell.Action.ID == AID.FireflightByEmberlightLeft || (AID)spell.Action.ID == AID.FireflightByEmberlightRight) {
            foreach (var (i, player) in Raid.WithSlot()) {
                AddSpread(player);
            }
        }
    }
    
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID == AID.FireflightByPyrelightStack) {
            Stacks.Clear();
            return;
        }
        
        if ((AID)spell.Action.ID == AID.FireflightByEmberlightSpread) {
            Spreads.Clear();
        }
    }
}

class SunCirclet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunCirclet, new AOEShapeDonut(8, 60));