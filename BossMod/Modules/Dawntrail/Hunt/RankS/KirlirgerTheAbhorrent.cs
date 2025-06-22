namespace BossMod.Dawntrail.Hunt.RankS.KirlirgerTheAbhorrent;

public enum OID : uint
{
    Boss = 0x452A // R6.25
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    FightersFlourish1 = 39466, // Boss->self, 5.0s cast, range 40 270-degree cone
    FightersFlourish2 = 39477, // Boss->self, 5.0s cast, range 40 270-degree cone
    FightersFlourish3 = 39473, // Boss->self, 5.0s cast, range 40 270-degree cone
    DiscordantFlourish1 = 39467, // Boss->self, 5.0s cast, range 40 270-degree cone
    DiscordantFlourish2 = 39538, // Boss->self, 5.0s cast, range 40 270-degree cone
    DiscordantFlourish3 = 39877, // Boss->self, 5.0s cast, range 40 270-degree cone

    FullmoonFuryCircle1 = 39464, // Boss->self, 6.0s cast, range 20 circle
    FullmoonFuryCircle2 = 39459, // Boss->self, 6.0s cast, range 20 circle
    FullmoonFuryDonut = 39463, // Boss->self, 6.0s cast, range 10-40 donut
    DiscordantMoonCircle = 39484, // Boss->self, 6.0s cast, range 20 circle
    DiscordantMoonDonut1 = 39465, // Boss->self, 6.0s cast, range 10-40 donut
    DiscordantMoonDonut2 = 39875, // Boss->self, 6.0s cast, range 10-40 donut

    DishonorsDiscord1 = 39456, // Boss->self, 3.0s cast, single-target
    DishonorsDiscord2 = 39457, // Boss->self, 3.0s cast, single-target
    DishonorsDiscord3 = 39458, // Boss->self, 3.0s cast, single-target
    HonorsAccord1 = 39455, // Boss->self, 3.0s cast, single-target
    HonorsAccord2 = 39454, // Boss->self, 3.0s cast, single-target
    HonorsAccord3 = 39453, // Boss->self, 3.0s cast, single-target

    EnervatingGloom = 39480, // Boss->players, 5.0s cast, range 6 circle, stack
    FlyingFist = 39524, // Boss->self, 2.5s cast, range 40 width 8 rect
    OdiousUproar = 39481 // Boss->self, 5.0s cast, range 40 circle
}

sealed class Flourish(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FightersFlourish1, (uint)AID.FightersFlourish2, (uint)AID.FightersFlourish3,
(uint)AID.DiscordantFlourish1, (uint)AID.DiscordantFlourish2, (uint)AID.DiscordantFlourish3], new AOEShapeCone(40f, 135f.Degrees()));
sealed class Fullmoon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FullmoonFuryCircle1, (uint)AID.FullmoonFuryCircle2, (uint)AID.DiscordantMoonCircle], 20f);
sealed class DiscordantMoon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FullmoonFuryDonut, (uint)AID.DiscordantMoonDonut1,
(uint)AID.DiscordantMoonDonut2], new AOEShapeDonut(10f, 40f));

sealed class FlyingFist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlyingFist, new AOEShapeRect(40f, 4f));
sealed class OdiousUproar(BossModule module) : Components.RaidwideCast(module, (uint)AID.OdiousUproar);
sealed class EnervatingGloom(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.EnervatingGloom, 6f, 8);

sealed class KirlirgerTheAbhorrentStates : StateMachineBuilder
{
    public KirlirgerTheAbhorrentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flourish>()
            .ActivateOnEnter<Fullmoon>()
            .ActivateOnEnter<DiscordantMoon>()
            .ActivateOnEnter<FlyingFist>()
            .ActivateOnEnter<OdiousUproar>()
            .ActivateOnEnter<EnervatingGloom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13360)]
public sealed class KirlirgerTheAbhorrent(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
