namespace BossMod.Stormblood.Ultimate.UCOB;

class Twister(BossModule module) : Components.CastTwister(module, 2f, (uint)OID.VoidzoneTwister, (uint)AID.Twister, 0.3f, 0.5f); // TODO: verify radius

class P1Twister(BossModule module) : Twister(module)
{
    public override bool KeepOnPhaseChange => true;
}
