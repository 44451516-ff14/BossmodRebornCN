namespace BossMod.Dawntrail.FATE.Ttokrrone;

sealed class DesertTempest(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(19f, 90f.Degrees());
    private static readonly AOEShapeCircle circle = new(19f);
    private static readonly AOEShapeDonut donut = new(14f, 60f);
    private static readonly AOEShapeDonutSector donutSector = new(14f, 60f, 90f.Degrees());

    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DesertTempestVisualDonut:
                AddAOEs(donut);
                break;
            case (uint)AID.DesertTempestVisualCircle:
                AddAOEs(circle);
                break;
            case (uint)AID.DesertTempestVisualConeDonutSegment:
                AddAOEs(cone, donutSector);
                break;
            case (uint)AID.DesertTempestVisualDonutSegmentCone:
                AddAOEs(donutSector, cone);
                break;
        }
        void AddAOEs(AOEShape first, AOEShape? second = null)
        {
            var act = Module.CastFinishAt(spell, 1d);
            var a90 = 90f.Degrees();
            AddAOE(first, a90);
            if (second != null)
            {
                AddAOE(second, -a90);
            }
            void AddAOE(AOEShape shape, Angle offset) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation + offset, act));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.DesertTempestCircle:
                case (uint)AID.DesertTempestDonut:
                case (uint)AID.DesertTempestDonutSegment1:
                case (uint)AID.DesertTempestDonutSegment2:
                case (uint)AID.DesertTempestCone1:
                case (uint)AID.DesertTempestCone2:
                    _aoes.Clear();
                    break;
            }
        }
    }
}
