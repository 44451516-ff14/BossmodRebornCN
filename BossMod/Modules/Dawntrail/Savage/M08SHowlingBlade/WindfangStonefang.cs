namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class WindfangStonefang(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(15f, 3f);
    private static readonly AOEShapeCircle circle = new(9f);
    private static readonly AOEShapeDonut donut = new(8f, 20f);
    public bool Draw;
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.StonefangCircle => circle,
            (uint)AID.StonefangCross1 or (uint)AID.StonefangCross2 or (uint)AID.WindfangCross1 or (uint)AID.WindfangCross2 => cross,
            (uint)AID.WindfangDonut => donut,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.StonefangCircle:
            case (uint)AID.WindfangDonut:
                ++NumCasts;
                break;
        }
    }
}

sealed class StonefangBait(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StonefangCircle)
        {
            var act = Module.CastFinishAt(spell);
            var party = Raid.WithoutSlot(false, true, true);
            var source = Module.PrimaryActor;
            var len = party.Length;

            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                CurrentBaits.Add(new(source, p, cone, act));
            }
        }
    }
}

sealed class WindfangBait(BossModule module) : Components.GenericBaitStack(module)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindfangDonut)
        {
            var act = Module.CastFinishAt(spell);
            var party = Raid.WithoutSlot(true, true, true);
            var source = Module.PrimaryActor;
            var len = party.Length;

            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Class.IsSupport())
                    CurrentBaits.Add(new(source, p, cone, act));
            }
        }
    }
}
