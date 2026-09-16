namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

sealed class SavageBarbery(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public int NumActiveCasts => _aoes.Count;
    private readonly AOEShapeDonut donut = new(6f, 20f);
    private readonly AOEShapeRect rect = new(40f, 6f);
    private readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.SavageBarberyDonutAOE => donut,
            (uint)AID.SavageBarberyRectAOE => rect,
            (uint)AID.SavageBarberyDonutSword or (uint)AID.SavageBarberyRectSword => circle,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SavageBarberyDonutAOE or (uint)AID.SavageBarberyRectAOE or (uint)AID.SavageBarberyDonutSword or (uint)AID.SavageBarberyRectSword)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class HairRaid(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public int NumActiveCasts => _aoes.Count;
    private readonly AOEShapeCone cone = new(40f, 60f.Degrees()); // TODO: verify angle
    private readonly AOEShapeDonut donut = new(6f, 20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.HairRaidConeAOE => cone,
            (uint)AID.HairRaidDonutAOE => donut,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.HairRaidConeAOE or (uint)AID.HairRaidDonutAOE)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class HairSprayDeadlyTwist(BossModule module) : Components.CastStackSpread(module, (uint)AID.DeadlyTwist, (uint)AID.HairSpray, 6f, 5f, 4);
