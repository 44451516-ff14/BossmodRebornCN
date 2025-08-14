namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA1Art;

sealed class LegendMythSpinnerCarver(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(15f);
    private static readonly AOEShapeDonut donut = new(7f, 22f);
    public readonly List<AOEInstance> AOEs = new(5);
    private bool mythcall;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        if (count == 5)
        {
            aoes[0].Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        void AddAOEs(AOEShape shape)
        {
            var orlasrach = Module.Enemies((uint)OID.Orlasrach);
            for (var i = 0; i < orlasrach.Count; ++i)
                AOEs.Add(new(shape, orlasrach[i].Position.Quantized(), default, Module.CastFinishAt(spell, 2.6f)));
            mythcall = false;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.Legendcarver:
                AddAOE(circle);
                if (mythcall)
                    AddAOEs(circle);
                break;
            case (uint)AID.Legendspinner:
                AddAOE(donut);
                if (mythcall)
                    AddAOEs(donut);
                break;
            case (uint)AID.Mythcall:
                mythcall = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.Legendcarver or (uint)AID.Legendspinner or (uint)AID.Mythcarver or (uint)AID.Mythspinner)
            AOEs.RemoveAt(0);
    }
}
