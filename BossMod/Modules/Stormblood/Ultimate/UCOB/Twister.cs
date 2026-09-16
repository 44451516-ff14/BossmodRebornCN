namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class TwisterBase(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, (uint)AID.Twister, 0.3f, 0.8f); // TODO: verify radius

sealed class Twister(BossModule module) : TwisterBase(module);

sealed class P1Twister(BossModule module) : TwisterBase(module)
{
    public override bool KeepOnPhaseChange => true;
}
