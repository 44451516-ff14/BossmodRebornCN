namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class Border(BossModule module) : BossComponent(module)
{
    public bool LBridgeActive;
    public bool RBridgeActive;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state is 0x00020001u or 0x00080004u)
        {
            switch (index)
            {
                case 0x02:
                    RBridgeActive = state == 0x00020001u;
                    break;
                case 0x03:
                    LBridgeActive = state == 0x00020001u;
                    break;
            }
        }
        if (!LBridgeActive && !RBridgeActive)
        {
            Arena.Bounds = P10SPandaemonium.GetDefaultArena();
        }
        else if (!LBridgeActive && RBridgeActive)
        {
            Arena.Bounds = P10SPandaemonium.GetArenaR();
        }
        else if (LBridgeActive && !RBridgeActive)
        {
            Arena.Bounds = P10SPandaemonium.GetArenaL();
        }
        else if (LBridgeActive && RBridgeActive)
        {
            Arena.Bounds = P10SPandaemonium.GetArenaLR();
        }
    }
}
