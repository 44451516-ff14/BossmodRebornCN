namespace BossMod.Dawntrail.Savage.M10STheXtremes;

// test bait proximity component
sealed class InsaneAirTest(BossModule module) : Components.GenericBaitProximity(module)
{
    private readonly List<Ability> _abilities = [];
    private int _order = 0;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 0x0E or > 0x16)
            return;

        var ability = GetAbility(index, state);
        if (ability == null)
            return;

        _abilities.Add(ability);
        // only show 1st 2, add others later
        //if (_abilities.Count <= 2)
        if (CurrentBaits.Count < 2)
        {
            CurrentBaits.Add(CreateBait(_abilities[_order++]));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // roughly 8.6 - 9s between casts
        if (spell.Action.ID is (uint)AID.BlastingSnap or (uint)AID.ReEntryBlast or (uint)AID.VerticalBlast or (uint)AID.PlungingSnap or (uint)AID.ReEntryPlunge or (uint)AID.VerticalPlunge)
        {
            var abilities = CollectionsMarshal.AsSpan(_abilities);
            var len = abilities.Length;
            for (var i = 0; i < len; i++)
            {
                ref var ability = ref abilities[i];
                if (caster.Position.AlmostEqual(ability.Position, 1f))
                {
                    RemoveBaitNear(caster.Position, 1f);
                    if (_order < _abilities.Count && CurrentBaits.Count < 2)
                    {
                        CurrentBaits.Add(CreateBait(_abilities[_order++]));
                    }
                    return;
                }
            }
        }
    }

    public Bait CreateBait(Ability ability)
    {
        var mechanic = ability.Mechanic;
        AOEShape shape = mechanic == Mechanic.Tankbuster ? new AOEShapeCircle(6f) : new AOEShapeCone(60f, 22.5f.Degrees());
        var targetNum = mechanic == Mechanic.Spread ? 4 : 1;
        var isStack = mechanic == Mechanic.Stack;
        var stackNum = mechanic == Mechanic.Stack ? 4 : 1;
        var centered = mechanic == Mechanic.Tankbuster;
        Bait bait = new(ability.Position, shape, default, targetNum, true, isStack, stackNum, stackNum, centered, centered);
        return bait;
    }

    public void RemoveBaitNear(WPos position, float eps)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;

        for (var i = 0; i < len; i++)
        {
            if (baits[i].Position.AlmostEqual(position, eps))
            {
                CurrentBaits.RemoveAt(i);
                return;
            }
        }
    }

    public Ability? GetAbility(byte index, uint state)
    {
        // 0x0E -> 0x16 surfboard markers, 0x0E is NW, moves right then down, 13f apart
        // 0E 0F 10
        // 11 12 13
        // 14 15 16
        // 0x00080004 -> marker removed
        // 0x00020001 -> blue down (spread)
        // 0x00200010 -> blue middle (stack)
        // 0x00800040 -> blue up (tb)
        // 0x02000100 -> red down (spread)
        // 0x08000400 -> red middle (stack)
        // 0x20001000 -> red up (tb)

        var num = index - 0x0E;
        var row = num / 3;
        var col = num % 3;
        var NW = new WPos(87f, 87f);
        var position = NW + new WDir(col * 13f, row * 13f);

        var mech = state switch
        {
            0x00020001 => new Ability() { IsBlue = true, Mechanic = Mechanic.Spread, Position = position },
            0x00200010 => new Ability() { IsBlue = true, Mechanic = Mechanic.Stack, Position = position },
            0x00800040 => new Ability() { IsBlue = true, Mechanic = Mechanic.Tankbuster, Position = position },
            0x02000100 => new Ability() { IsBlue = false, Mechanic = Mechanic.Spread, Position = position },
            0x08000400 => new Ability() { IsBlue = false, Mechanic = Mechanic.Stack, Position = position },
            0x20001000 => new Ability() { IsBlue = false, Mechanic = Mechanic.Tankbuster, Position = position },
            //0x00080004 => new Ability() { IsBlue = true, Mechanic = Mechanic.Remove, Position = position },
            _ => null
        };

        return mech;
    }

    public class Ability
    {
        // IsBlue unnecessary? insane air always proximity regardless of color debuff
        public bool IsBlue;
        public Mechanic Mechanic;
        public WPos Position;
    }

    public enum Mechanic
    {
        Spread,
        Stack,
        Tankbuster,
        Remove
    }

    public static WPos GetPosition(byte index, uint state)
    {
        var num = index - 0x0E;
        var row = num / 3;
        var col = num % 3;
        var NW = new WPos(87f, 87f);
        return NW + new WDir(col * 13f, row * 13f);
    }
}
