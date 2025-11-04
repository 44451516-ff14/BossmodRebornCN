namespace BossMod.Stormblood.Ultimate.UWU;

class P2InfernalFetters(BossModule module) : BossComponent(module)
{
    public BitMask Fetters;
    private int _fettersStrength;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Fetters[playerSlot] ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Fetters.NumSetBits() > 1)
        {
            var from = Raid[Fetters.LowestSetBit()];
            var to = Raid[Fetters.HighestSetBit()];
            if (from != null && to != null)
                Arena.AddLine(from.Position, to.Position, _fettersStrength > 1 ? 0 : Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.InfernalFetters)
        {
            Fetters[Raid.FindSlot(actor.InstanceID)] = true;
            _fettersStrength = status.Extra;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.InfernalFetters)
        {
            Fetters[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}
