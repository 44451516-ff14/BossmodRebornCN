namespace BossMod.Dawntrail.Alliance.A12Fafnir;

sealed class SpikeFlail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpikeFlail, new AOEShapeCone(80f, 135f.Degrees()))
{
    public override bool KeepOnPhaseChange => true;
}

sealed class Touchdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Touchdown, 24f)
{
    public override bool KeepOnPhaseChange => true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Count != 0 && (Module.FindComponent<DragonBreath>()?.AOE == null || Arena.Bounds != A12Fafnir.FireArena) ? new AOEInstance[1] { Casters[0] } : [];
    }
}

sealed class DragonBreath(BossModule module) : Components.GenericAOEs(module, (uint)AID.DragonBreath)
{
    public override bool KeepOnPhaseChange => true;
    public AOEInstance? AOE;

    private static readonly AOEShapeDonut donut = new(16f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OffensivePostureDragonBreath)
        {
            NumCasts = 0;
            AOE = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.2d));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && actor.OID == (uint)OID.FireVoidzone)
            AOE = null;
    }
}
