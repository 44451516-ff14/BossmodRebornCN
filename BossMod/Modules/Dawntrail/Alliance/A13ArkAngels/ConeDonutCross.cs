namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class ConcertedDissolution(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcertedDissolution, new AOEShapeCone(40f, 20f.Degrees()))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var chain = Module.FindComponent<LightsChain>();
        var check = chain != null && chain.Casters.Count != 0;

        var aoes = new AOEInstance[count];
        var color = Colors.Danger;
        for (var i = 0; i < count; ++i)
        {
            var aoe = Casters[i];
            aoes[i] = check ? aoe with { Color = color } : aoe;
        }
        return aoes;
    }
}

class LightsChain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightsChain, new AOEShapeDonut(4f, 40f))
{
    private readonly ConcertedDissolution? _aoe = module.FindComponent<ConcertedDissolution>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
            return [];

        var reaver = Module.FindComponent<CrossReaver>();
        var check = _aoe != null && _aoe.Casters.Count != 0;
        var check2 = reaver != null && reaver.Casters.Count != 0;

        return new AOEInstance[1] { Casters[0] with { Color = check2 ? Colors.Danger : 0, Risky = !check } };
    }
}

class CrossReaver(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossReaverAOE, new AOEShapeCross(50f, 6f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
            return [];
        var chain = Module.FindComponent<LightsChain>();
        var check = chain != null && chain.Casters.Count != 0;
        return new AOEInstance[1] { Casters[0] with { Risky = !check } };
    }
}
