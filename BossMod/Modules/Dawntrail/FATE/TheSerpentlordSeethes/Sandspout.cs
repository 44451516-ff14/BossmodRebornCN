namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class Sandspout(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());
    private static readonly AOEShapeCircle circle = new(13f);
    private static readonly Angle a180 = 180f.Degrees();
    private static readonly Angle a90 = 90f.Degrees();

    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TailwardSandspoutVisual:
                AddAOEs(a180);
                break;
            case (uint)AID.LeftwardSandspoutVisual:
                AddAOEs(a90);
                break;
            case (uint)AID.RightwardSandspoutVisual:
                AddAOEs(-a90);
                break;
            case (uint)AID.FangwardSandspoutVisual:
                AddAOEs(default);
                break;
        }
        void AddAOEs(Angle offset)
        {
            var position = spell.LocXZ;
            var act = Module.CastFinishAt(spell, 6.1f);
            _aoes.Add(new(cone, position, spell.Rotation + offset, act));
            _aoes.Add(new(circle, position, default, act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TailwardSandspout:
            case (uint)AID.RightwardSandspout:
            case (uint)AID.LeftwardSandspout:
            case (uint)AID.FangwardSandspout:
                _aoes.Clear();
                break;
        }
    }
}
