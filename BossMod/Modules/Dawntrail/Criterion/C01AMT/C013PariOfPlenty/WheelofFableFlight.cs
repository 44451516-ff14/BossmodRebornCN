namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO figure out who gets the stack - its always support / DPS - but can we tell who?
class WheelOfFableFlight(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private Angle offset;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.WheelOfFableflightRight) {
            offset = -90.Degrees();
            return;
        }
        
        if (spell.Action.ID == (uint)AID.WheelOfFableflightLeft) {
            offset = 90.Degrees();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID is (uint)IconID.FalseFlameRight) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.Danger));
        }
        
        if (iconID is (uint)IconID.FalseFlameLeft) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, -offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.Danger));
        }
        
        if (iconID is (uint)IconID.FalseFlameRRight) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.Danger));
        }
        
        if (iconID is (uint)IconID.FalseFlameRLeft) {
            aoes.Add(new AOEInstance(new AOEShapeRect(40, 40, 0, -offset), actor.Position, actor.Rotation - 90.Degrees(), WorldState.CurrentTime, Colors.Danger));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID == AID.WheelOfFireflight || 
            (AID)spell.Action.ID == AID.WheelOfFireflight1 || 
            (AID)spell.Action.ID == AID.WheelOfFireflight2 || 
            (AID)spell.Action.ID == AID.WheelOfFireflight3) {
            aoes.Clear();
            NumCasts++;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        return CollectionsMarshal.AsSpan(aoes);
    }
}

class WheelofFableFlightStackSpread(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 2) {
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.KindledFlameStack) {
            AddStacks(Raid.WithoutSlot().Where(p => p.Class.IsSupport()));
        }

        if ((AID)spell.Action.ID == AID.ScatteredKindlingSpread) {
            foreach (var (i, player) in Raid.WithSlot()) {
                AddSpread(player);
            }
        }
    }
    
    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID == AID.KindledFlame1) {
            Stacks.Clear();
            return;
        }
        
        if ((AID)spell.Action.ID == AID.ScatteredKindling1) {
            Spreads.Clear();
        }
    }
}