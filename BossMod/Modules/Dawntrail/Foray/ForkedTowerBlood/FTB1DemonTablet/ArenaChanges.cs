namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00u)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = FTB1DemonTablet.DefaultArena;
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = FTB1DemonTablet.CompleteArena;
            }
        }
        else if (index == 0x01u && state == 0x00020001u)
        {
            Arena.Bounds = FTB1DemonTablet.RotationArena;
        }
    }
}
