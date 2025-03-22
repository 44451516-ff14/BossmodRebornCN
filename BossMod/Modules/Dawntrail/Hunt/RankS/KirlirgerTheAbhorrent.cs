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

class Flourish(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40f, 135f.Degrees()));
class FightersFlourish1(BossModule module) : Flourish(module, AID.FightersFlourish1);
class FightersFlourish2(BossModule module) : Flourish(module, AID.FightersFlourish2);
class FightersFlourish3(BossModule module) : Flourish(module, AID.FightersFlourish3);
class DiscordantFlourish1(BossModule module) : Flourish(module, AID.DiscordantFlourish1);
class DiscordantFlourish2(BossModule module) : Flourish(module, AID.DiscordantFlourish2);
class DiscordantFlourish3(BossModule module) : Flourish(module, AID.DiscordantFlourish3);

class Fullmoon(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 20f);
class FullmoonFuryCircle1(BossModule module) : Fullmoon(module, AID.FullmoonFuryCircle1);
class FullmoonFuryCircle2(BossModule module) : Fullmoon(module, AID.FullmoonFuryCircle2);
class DiscordantMoonCircle(BossModule module) : Fullmoon(module, AID.DiscordantMoonCircle);

class DiscordantMoon(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(10f, 40f));
class FullmoonFuryDonut(BossModule module) : DiscordantMoon(module, AID.FullmoonFuryDonut);
class DiscordantMoonDonut1(BossModule module) : DiscordantMoon(module, AID.DiscordantMoonDonut1);
class DiscordantMoonDonut2(BossModule module) : DiscordantMoon(module, AID.DiscordantMoonDonut2);

class FlyingFist(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FlyingFist), new AOEShapeRect(40f, 4f));
class OdiousUproar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OdiousUproar));
class EnervatingGloom(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.EnervatingGloom), 6f, 8);

class KirlirgerTheAbhorrentStates : StateMachineBuilder
{
    public KirlirgerTheAbhorrentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FightersFlourish1>()
            .ActivateOnEnter<FightersFlourish2>()
            .ActivateOnEnter<FightersFlourish3>()
            .ActivateOnEnter<DiscordantFlourish1>()
            .ActivateOnEnter<DiscordantFlourish2>()
            .ActivateOnEnter<DiscordantFlourish3>()
            .ActivateOnEnter<FullmoonFuryCircle1>()
            .ActivateOnEnter<FullmoonFuryCircle2>()
            .ActivateOnEnter<DiscordantMoonCircle>()
            .ActivateOnEnter<FullmoonFuryDonut>()
            .ActivateOnEnter<DiscordantMoonDonut1>()
            .ActivateOnEnter<DiscordantMoonDonut2>()
            .ActivateOnEnter<FlyingFist>()
            .ActivateOnEnter<OdiousUproar>()
            .ActivateOnEnter<EnervatingGloom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13360)]
public class KirlirgerTheAbhorrent(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
