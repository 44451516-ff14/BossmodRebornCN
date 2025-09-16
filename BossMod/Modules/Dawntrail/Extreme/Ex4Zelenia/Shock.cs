namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class Towers1(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion1)
        {
            Towers.Add(new(spell.LocXZ, 3f, forbiddenSoakers: forbidden, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ShockCircle)
        {
            forbidden.Set(Raid.FindSlot(targetID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion1)
        {
            ++NumCasts;
        }
    }
}

sealed class ShockSpread(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public static readonly AOEShapeCircle Circle = new(4f);
    public static readonly AOEShapeDonut Donut = new(1f, 6f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        AOEShape? shape = iconID switch
        {
            (uint)IconID.ShockCircle => Circle,
            (uint)IconID.ShockDonut => Donut,
            _ => null
        };
        if (shape != null)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, shape, WorldState.FutureTime(8d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShockDonutLock)
        {
            ++NumCasts;
        }
    }
}

sealed class ShockAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private int donuts;
    private int circles;
    public bool Done;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.ShockCircleLock => ShockSpread.Circle,
            (uint)AID.ShockDonutLock => ShockSpread.Donut,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, caster.Position.Quantized()));
            if (shape == ShockSpread.Donut)
                ++donuts;
            else
                ++circles;
        }
        else if (spell.Action.ID is (uint)AID.ShockDonut2 or (uint)AID.Shock6)
        {
            if (++NumCasts == 2 * circles + 11 * donuts)
            {
                Done = true;
            }
        }
    }
}
