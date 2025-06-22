namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ArchInferno(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.ArchInferno, GetVoidzone, 0f)
{
    private static Actor[] GetVoidzone(BossModule module)
    {
        var primary = module.PrimaryActor;
        if (module.PrimaryActor.CastInfo?.IsSpell(AID.ArchInferno) ?? false)
            return [primary];
        return [];
    }
}
class InfernoDevilFirst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernoDevilFirst, 10f);
class InfernoDevilRest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernoDevilRest, 10f);
class Conflagration(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Conflagration, new AOEShapeRect(20f, 5f));
class RadialFlagration(BossModule module) : Components.SimpleProtean(module, (uint)AID.RadialFlagrationAOE, new AOEShapeCone(21f, 15f.Degrees())); // TODO: verify angle
class SpikeOfFlame(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SpikeOfFlame, 5f);
class FourfoldFlame(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FourfoldFlame, 6f, 4, 4);
class TwinfoldFlame(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TwinfoldFlame, 4f, 2, 2);
