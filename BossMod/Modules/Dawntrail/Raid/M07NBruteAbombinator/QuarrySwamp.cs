namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class QuarrySwamp(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.QuarrySwamp, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.BloomingAbomination));

    public override void Update()
    {
        if (Casters.Count != 0 && BlockerActors().Length != 0)
        {
            Safezones.Clear();
            Refresh();
            AddSafezone(Module.CastFinishAt(Casters[0].CastInfo));
        }
    }
}
