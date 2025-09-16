namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class CthonicFury(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public bool Active => _aoe.Length != 0 || Arena.Bounds != A14ShadowLord.DefaultBounds;
    private static readonly Square[] def = [new Square(A14ShadowLord.ArenaCenter, 30f)]; // using a square for the difference instead of a circle since less vertices will result in slightly better performance
    public static readonly AOEShapeCustom AOEBurningBattlements = new(def, [new Square(A14ShadowLord.ArenaCenter, 11.5f, 45f.Degrees())]);
    private static readonly AOEShapeCustom aoeCthonicFury = new(def, A14ShadowLord.Combined);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CthonicFuryStart)
        {
            _aoe = [new(aoeCthonicFury, Arena.Center, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CthonicFuryStart:
                _aoe = [];
                SetArena(A14ShadowLord.ComplexBounds, A14ShadowLord.ComplexBounds.Center);
                break;
            case (uint)AID.CthonicFuryEnd:
                SetArena(A14ShadowLord.DefaultBounds, A14ShadowLord.ArenaCenter);
                break;
        }

        void SetArena(ArenaBounds bounds, WPos center)
        {
            Arena.Bounds = bounds;
            Arena.Center = center;
        }
    }
}

sealed class BurningCourtMoatKeepBattlements(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private static readonly AOEShape _shapeC = new AOEShapeCircle(8f);
    private static readonly AOEShape _shapeM = new AOEShapeDonut(5f, 15f);
    private static readonly AOEShape _shapeK = new AOEShapeRect(23f, 11.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape != null)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape != null)
        {
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var len = aoes.Length;
            var id = caster.InstanceID;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    private static AOEShape? ShapeForAction(uint aid) => aid switch
    {
        (uint)AID.BurningCourt => _shapeC,
        (uint)AID.BurningMoat => _shapeM,
        (uint)AID.BurningKeep => _shapeK,
        (uint)AID.BurningBattlements => CthonicFury.AOEBurningBattlements,
        _ => null
    };
}

sealed class EchoesOfAgony(BossModule module) : Components.StackWithIcon(module, (uint)IconID.EchoesOfAgony, (uint)AID.EchoesOfAgonyAOE, 5f, 9.2d, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EchoesOfAgony)
        {
            NumFinishedStacks = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
