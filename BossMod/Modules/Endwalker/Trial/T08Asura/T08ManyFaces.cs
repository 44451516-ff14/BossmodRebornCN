namespace BossMod.Endwalker.Trial.T08Asura;

sealed class ManyFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(20f, 90f.Degrees());
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? rotation = spell.Action.ID switch
        {
            (uint)AID.TheFaceOfDelightA or (uint)AID.TheFaceOfWrathB => Angle.AnglesCardinals[2],
            (uint)AID.TheFaceOfDelightB or (uint)AID.TheFaceOfWrathC => Angle.AnglesCardinals[1],
            (uint)AID.TheFaceOfDelightC or (uint)AID.TheFaceOfWrathA => Angle.AnglesCardinals[3],
            _ => null
        };

        if (rotation != null)
        {
            _aoe = [new(cone, spell.LocXZ, rotation.Value, Module.CastFinishAt(spell, 0.1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TheFaceOfDelightAOE or (uint)AID.TheFaceOfWrathAOE)
        {
            _aoe = [];
        }
    }
}
