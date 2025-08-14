namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(30f, 40f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ImitationStarVisual)
            _aoe = new(donut, FTB3MarbleDragon.ArenaCenter, default, Module.CastFinishAt(spell, 2.3f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = FTB3MarbleDragon.DefaultArena;
            Arena.Center = FTB3MarbleDragon.ArenaCenter.Quantized();
            _aoe = null;
        }
    }
}
