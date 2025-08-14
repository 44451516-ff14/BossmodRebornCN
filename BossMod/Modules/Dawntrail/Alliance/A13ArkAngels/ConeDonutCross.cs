namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class ConcertedDissolution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcertedDissolution, new AOEShapeCone(40f, 20f.Degrees()))
{
    private LightsChain? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        _aoe ??= Module.FindComponent<LightsChain>();
        var check = _aoe!.Casters.Count != 0;

        var aoes = CollectionsMarshal.AsSpan(Casters);
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = check ? color : default;
        }
        return aoes;
    }
}

sealed class LightsChain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightsChain, new AOEShapeDonut(4f, 40f))
{
    private readonly ConcertedDissolution _aoe1 = module.FindComponent<ConcertedDissolution>()!;
    private CrossReaver? _aoe2;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
        {
            return [];
        }
        _aoe2 ??= Module.FindComponent<CrossReaver>();

        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref var aoe0 = ref aoes[0];
        aoe0.Color = _aoe2!.Casters.Count != 0 ? Colors.Danger : default;
        aoe0.Risky = _aoe1.Casters.Count == 0;
        return aoes;
    }
}

sealed class CrossReaver(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossReaverAOE, new AOEShapeCross(50f, 6f))
{
    private LightsChain? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
        {
            return [];
        }
        _aoe ??= Module.FindComponent<LightsChain>();
        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref var aoe0 = ref aoes[0];
        aoe0.Risky = _aoe!.Casters.Count == 0;
        return aoes;
    }
}
