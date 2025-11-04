namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

sealed class Quantumlevel(BossModule module) : BossComponent(module)
{
    public uint QuantumLevel;
    private readonly Q1FinalVerse bossmod = (Q1FinalVerse)module;

    public override void Update()
    {
        if (bossmod.BossEater == null || QuantumLevel == 40u)
        {
            return;
        }
        QuantumLevel = default;
        if (bossmod.BossEater.FindStatus((uint)SID.LightDamageUp) is ActorStatus light)
        {
            QuantumLevel += light.Extra;
        }
        if (Module.PrimaryActor.FindStatus((uint)SID.DarkDamageUp) is ActorStatus dark)
        {
            QuantumLevel += dark.Extra;
        }
        if (Module.PrimaryActor.FindStatus((uint)SID.PhysicalDamageUp) is ActorStatus dmg)
        {
            QuantumLevel += dmg.Extra;
        }
        if (Module.PrimaryActor.FindStatus((uint)SID.FireDamageUp) is ActorStatus fire)
        {
            QuantumLevel += fire.Extra;
        }
        if (Module.PrimaryActor.FindStatus((uint)SID.HPBoost) is ActorStatus hp)
        {
            QuantumLevel += hp.Extra;
        }
        Service.Log($"{QuantumLevel}");
    }
}
