namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class LariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect rect1 = new(20f, 30f, 5f, -90f.Degrees());
    private static readonly AOEShapeRect rect2 = new(20f, 30f, 5f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LariatComboVisual1:
                AddAOEs(rect2, rect1);
                break;
            case (uint)AID.LariatComboVisual2:
                AddAOEs(rect2, rect2);
                break;
            case (uint)AID.LariatComboVisual3:
                AddAOEs(rect1, rect2);
                break;
            case (uint)AID.LariatComboVisual4:
                AddAOEs(rect1, rect1);
                break;
        }
        void AddAOEs(AOEShapeRect shape1, AOEShapeRect shape2)
        {
            var rotation = spell.Rotation;
            var position = caster.Position;
            _aoes.Add(new(shape1, position, rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(shape2, position, rotation, Module.CastFinishAt(spell, 4.4f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LariatCombo1 or (uint)AID.LariatCombo2 or (uint)AID.LariatCombo3 or (uint)AID.LariatCombo4)
            _aoes.RemoveAt(0);
    }
}
