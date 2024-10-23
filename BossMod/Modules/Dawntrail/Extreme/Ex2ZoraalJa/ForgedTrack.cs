namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// there are only 4 possible patterns for this mechanic, here are the findings:
// - wide/knockback are always NE/NW platforms, narrow are always SE/SW platforms
// - NE/NW are always mirrored (looking from platform to the main one, wide is on the left side on one of them and on the right side on the other); which one is which is random
// - SE/SW have two patterns (either inner or outer lanes change left/right side); which one is which is random
class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    public enum Pattern { Unknown, A, B } // B is always inverted

    public readonly List<AOEInstance> NarrowAOEs = [];
    public readonly List<AOEInstance> WideAOEs = [];
    public readonly List<AOEInstance> KnockbackAOEs = [];
    private Pattern _patternN;
    private Pattern _patternS;

    private static readonly AOEShapeRect _shape = new(10, 2.5f, 10);
    private static readonly AOEShapeRect _shapeWide = new(10, 7.5f, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => NarrowAOEs.Concat(WideAOEs).Concat(KnockbackAOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.ForgedTrackPreview)
            return;

        var casterOffset = caster.Position - Arena.Center;
        var rightDir = spell.Rotation.ToDirection().OrthoR();
        var laneOffset = casterOffset.Dot(rightDir);
        var laneRight = laneOffset > 0;
        var laneInner = laneOffset is > -5 and < 5;
        var west = casterOffset.X > 0;
        if (casterOffset.Z < 0)
        {
            // N => wide/knockback
            if (_patternN == Pattern.Unknown)
                return;
            var rightIsWide = west == (_patternN == Pattern.A);
            var adjustedLaneOffset = laneOffset + (laneRight ? -5 : 5);
            if (rightIsWide == laneRight)
            {
                // wide
                WideAOEs.Add(new(_shapeWide, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.4f)));
            }
            else
            {
                // knockback
                KnockbackAOEs.Add(new(_shape, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.9f)));
            }
        }
        else
        {
            // S => narrow
            if (_patternS == Pattern.Unknown)
                return;
            var crossInner = west == (_patternS == Pattern.A);
            var cross = crossInner == laneInner;
            var adjustedRight = cross ^ laneRight;
            var adjustedLaneOffset = (laneInner ? 7.5f : 2.5f) * (adjustedRight ? 1 : -1);
            NarrowAOEs.Add(new(_shape, Arena.Center + rightDir * adjustedLaneOffset, spell.Rotation, Module.CastFinishAt(spell, 1.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ForgedTrackAOE:
                ++NumCasts;
                NarrowAOEs.Clear();
                break;
            case AID.FieryEdgeAOECenter:
                if (WideAOEs.Count == 0)
                    Module.ReportError(this, "Unexpected wide aoe");
                WideAOEs.Clear();
                break;
            case AID.StormyEdgeAOE:
                if (KnockbackAOEs.Count == 0)
                    Module.ReportError(this, "Unexpected knockback");
                KnockbackAOEs.Clear();
                break;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        switch (index)
        {
            case 2:
                AssignPattern(ref _patternS, 0x00800040, 0x02000100, state);
                break;
            case 3:
                AssignPattern(ref _patternS, 0x02000100, 0x00800040, state);
                break;
            case 5:
                AssignPattern(ref _patternN, 0x00020001, 0x00200010, state);
                break;
            case 8:
                AssignPattern(ref _patternN, 0x00200010, 0x00020001, state);
                break;
        }
    }

    private void AssignPattern(ref Pattern field, uint stateA, uint stateB, uint state)
    {
        // 0x00020001 XX, 0x00200010 out and inner crossed, 0x02000100 inner crossed, 0x00800040 outer crossed, 0x00080004 disappear
        if (state == 0x00080004)
            return; // end
        if (state != stateA && state != stateB)
        {
            Module.ReportError(this, $"Unknown pattern: {state:X8}, expected {stateA:X8} or {stateB:X8}");
            return;
        }
        var value = state == stateA ? Pattern.A : Pattern.B;
        if (field != Pattern.Unknown && field != value)
            Module.ReportError(this, $"Inconsistent pattern assignments: {field} vs {value}");
        field = value;
    }
}

class ForgedTrackKnockback(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.StormyEdgeKnockback))
{
    private readonly ForgedTrack? _main = module.FindComponent<ForgedTrack>();

    private static readonly AOEShapeRect _shape = new(20, 10);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_main == null)
            yield break;
        foreach (var aoe in _main.KnockbackAOEs)
        {
            yield return new(aoe.Origin, 7, aoe.Activation, _shape, aoe.Rotation + 90.Degrees(), Kind.DirForward);
            yield return new(aoe.Origin, 7, aoe.Activation, _shape, aoe.Rotation - 90.Degrees(), Kind.DirForward);
        }
    }
}
