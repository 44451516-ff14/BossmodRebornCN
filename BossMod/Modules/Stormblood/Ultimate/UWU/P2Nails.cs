namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: kill priorities
sealed class P2Nails(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _nails = module.Enemies((uint)OID.InfernalNail);

    public bool Active => _nails.Any(a => a.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_nails);
    }
}
