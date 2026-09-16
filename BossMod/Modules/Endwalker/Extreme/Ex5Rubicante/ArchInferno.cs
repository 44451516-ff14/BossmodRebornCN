namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

sealed class ArchInferno(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.ArchInferno, GetVoidzone, 0f)
{
    private static Actor[] GetVoidzone(BossModule module)
    {
        var primary = module.PrimaryActor;
        if ((module.PrimaryActor.CastInfo?.Action.ID ?? 0u) == (uint)AID.ArchInferno)
        {
            return [primary];
        }
        return [];
    }
}
sealed class InfernoDevilFirst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernoDevilFirst, 10f);
sealed class InfernoDevilRest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernoDevilRest, 10f);
sealed class Conflagration(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Conflagration, new AOEShapeRect(40f, 5f));
sealed class RadialFlagration(BossModule module) : Components.SimpleProtean(module, (uint)AID.RadialFlagrationAOE, new AOEShapeCone(21f, 15f.Degrees())); // TODO: verify angle
sealed class SpikeOfFlame(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SpikeOfFlame, 5f);
sealed class FourfoldFlame(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FourfoldFlame, 6f, 4, 4);
sealed class TwinfoldFlame(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TwinfoldFlame, 4f, 2, 2);
