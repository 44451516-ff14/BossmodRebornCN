namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class Wringer(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle circle = new(14f);
    private static readonly AOEShapeDonut donut = new(14f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        AOEShape? shape = id switch
        {
            (uint)AID.Wringer1 or (uint)AID.WringerTelegraph or (uint)AID.DoubleWringer => circle,
            (uint)AID.DeadWringer1 => donut,
            _ => null
        };
        if (shape != null)
        {
            if (_aoes.Count != 0 && _aoes.Ref(0).Shape == shape)
            {
                return;
            }
            var isTelegraph = id == (uint)AID.WringerTelegraph;
            var pos = spell.LocXZ;
            AddAOE(shape, isTelegraph ? 6.5d : default);
            if (isTelegraph)
            {
                AddAOE(donut, 11.5d);
            }
            void AddAOE(AOEShape shape, double delay) => _aoes.Add(new(shape, pos, default, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.DeadWringer1 or (uint)AID.DeadWringer2 or (uint)AID.Wringer1 or (uint)AID.Wringer2 or (uint)AID.DoubleWringer)
        {
            _aoes.RemoveAt(0);
        }
    }
}
