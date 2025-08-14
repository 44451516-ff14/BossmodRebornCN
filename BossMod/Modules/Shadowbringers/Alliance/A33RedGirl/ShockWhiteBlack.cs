namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class ShockWhiteBlack(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ShockWhiteAOE, (uint)AID.ShockBlackAOE], 5f);
abstract class ShockWhiteBait(BossModule module, uint iconID, double delay) : Components.BaitAwayIcon(module, 5f, iconID, (uint)AID.ShockWhiteBait, delay)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_arena.NumWalls != 0 && ActiveBaitsOn(actor).Count != 0)
        {
            hints.Add("Intersect black walls!");
        }
    }
}
sealed class ShockWhiteBaitSlow(BossModule module) : ShockWhiteBait(module, (uint)IconID.ShockWhiteSlow, 10.1d);
sealed class ShockWhiteBaitFast(BossModule module) : ShockWhiteBait(module, (uint)IconID.ShockWhiteFast, 5.1d);

sealed class ShockBlackBait(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.ShockBlack, (uint)AID.ShockBlackBait, 10.1d)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_arena.NumWalls != 0 && ActiveBaitsOn(actor).Count != 0)
        {
            hints.Add("Intersect white walls!");
        }
    }
}
