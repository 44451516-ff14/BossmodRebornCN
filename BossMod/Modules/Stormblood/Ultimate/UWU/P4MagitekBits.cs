namespace BossMod.Stormblood.Ultimate.UWU;

sealed class P4MagitekBits(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _bits = module.Enemies((uint)OID.MagitekBit);

    public bool Active => _bits.Any(b => b.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_bits);
    }
}
