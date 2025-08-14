namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(30f, 40f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DecisiveBattleTriton)
        {
            _aoe = new(donut, FTB2DeadStars.ArenaCenter, default, Module.CastFinishAt(spell, 1.1d));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = FTB2DeadStars.DefaultArena;
            Arena.Center = FTB2DeadStars.ArenaCenter.Quantized();
            _aoe = null;
        }
    }
}
