namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1ProteanWaveLiquidVisBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProteanWaveLiquidVisBoss, new AOEShapeCone(40, 15.Degrees()));
class P1ProteanWaveLiquidVisHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProteanWaveLiquidVisHelper, new AOEShapeCone(40, 15.Degrees()));

// single protean ("shadow") that fires in the direction the boss is facing
class P1ProteanWaveLiquidInvisFixed(BossModule module) : Components.GenericAOEs(module, (uint)AID.ProteanWaveLiquidInvisBoss)
{
    private readonly Actor? _source = module.Enemies((uint)OID.BossP1).FirstOrDefault();

    private static readonly AOEShapeCone _shape = new(40f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            return new AOEInstance[1] { new(_shape, _source.Position, _source.Rotation) };
        return [];
    }
}

// proteans baited on 4 closest targets
class P1ProteanWaveLiquidInvisBaited(BossModule module) : Components.GenericBaitAway(module, (uint)AID.ProteanWaveLiquidInvisHelper)
{
    private readonly Actor? _source = module.Enemies((uint)OID.BossP1).FirstOrDefault();

    private static readonly AOEShapeCone _shape = new(40f, 15f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
            foreach (var target in Raid.WithoutSlot(false, true, true).SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, target, _shape));
    }
}
