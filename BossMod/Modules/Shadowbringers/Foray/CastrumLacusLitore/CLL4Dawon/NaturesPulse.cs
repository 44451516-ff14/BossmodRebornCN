namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class NaturesPulse(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];
    private readonly ArenaChange _arena = module.FindComponent<ArenaChange>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_arena.IsDawonArena)
            return base.ActiveAOEs(slot, actor);
        else
            return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NaturesPulse1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.NaturesPulse1 => 0,
                (uint)AID.NaturesPulse2 => 1,
                (uint)AID.NaturesPulse3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5d));
        }
    }
}
