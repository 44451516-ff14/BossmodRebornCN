namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P1ProteanWaveLiquid(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ProteanWaveLiquidVisBoss, (uint)AID.ProteanWaveLiquidVisHelper], Cone)
{
    public static readonly AOEShapeCone Cone = new(40f, 15f.Degrees());
}

// single protean ("shadow") that fires in the direction the boss is facing
[SkipLocalsInit]
sealed class P1ProteanWaveLiquidInvisFixed(BossModule module) : Components.GenericAOEs(module, (uint)AID.ProteanWaveLiquidInvisBoss)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return new AOEInstance[1] { new(P1ProteanWaveLiquid.Cone, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation) };
    }
}

// proteans baited on 4 closest targets
[SkipLocalsInit]
sealed class P1ProteanWaveLiquidInvisBaited(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ProteanWaveLiquidInvisHelper)
{
    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var target in Raid.WithoutSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position).Take(4))
            CurrentBaits.Add(new(Module.PrimaryActor, target, P1ProteanWaveLiquid.Cone));
    }
}
