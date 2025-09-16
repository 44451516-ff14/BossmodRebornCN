namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.LiquidHell, m => m.Enemies((uint)OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f)
{
    public void Reset() => NumCasts = 0;
}

class P1LiquidHell(BossModule module) : LiquidHell(module)
{
    public override bool KeepOnPhaseChange => true;
}
