namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class Sleepga(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(70f, 35f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WarpVisual1 && Module.PrimaryActor is var primary && !primary.Position.AlmostEqual(caster.Position, 0.1f) && spell.Rotation != primary.Rotation)
        {
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 6.5d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Sleepga)
        {
            _aoe = [];
        }
    }
}

sealed class Sleep(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Sleep, "Sleep", "put to sleep");
