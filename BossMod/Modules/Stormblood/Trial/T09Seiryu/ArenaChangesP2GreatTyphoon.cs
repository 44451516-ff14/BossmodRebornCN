namespace BossMod.Stormblood.Trial.T09Seiryu;

sealed class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!actor.Position.InCircle(Arena.Center, 20f))
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 19f), DateTime.MaxValue);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StrengthOfSpirit) // in phase 2 the arena no longer got a wall and we need to add back the player hitboxradius
        {
            Arena.Bounds = SeiryuTrial.GetPhase2Arena();
        }
    }
}

sealed class GreatTyphoonDonut(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut1 = new(20f, 28f), donut2 = new(26f, 34f), donut3 = new(32f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.GreatTyphoon1 => donut1,
            (uint)AID.GreatTyphoon2 => donut2,
            (uint)AID.GreatTyphoon3 => donut3,
            _ => null
        };
        if (shape != null)
        {
            var pos = spell.LocXZ;
            _aoe = [new(shape, pos, default, Module.CastFinishAt(spell), actorID: caster.InstanceID, arenaProjectionLayer: 0, shapeDistance: shape.Distance(pos, default), restrictToArenaProjectionLayer: true)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GreatTyphoon1 or (uint)AID.GreatTyphoon2 or (uint)AID.GreatTyphoon3)
        {
            ref var aoe = ref _aoe[0];
            if (caster.InstanceID == aoe.ActorID) // depending on latency a new cast can start in the same frame as previous ended
            {
                _aoe = [];
            }
        }
    }
}
