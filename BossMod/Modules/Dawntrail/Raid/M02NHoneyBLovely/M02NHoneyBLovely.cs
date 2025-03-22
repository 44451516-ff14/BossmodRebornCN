namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

class CallMeHoney(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CallMeHoney));

abstract class TemptingTwist(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(7, 30));
class TemptingTwist1(BossModule module) : TemptingTwist(module, AID.TemptingTwist1);
class TemptingTwist2(BossModule module) : TemptingTwist(module, AID.TemptingTwist2);

abstract class HoneyBeeline(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(60, 7));
class HoneyBeeline1(BossModule module) : HoneyBeeline(module, AID.HoneyBeeline1);
class HoneyBeeline2(BossModule module) : HoneyBeeline(module, AID.HoneyBeeline2);

class HoneyedBreeze(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 15.Degrees()), (uint)IconID.HoneyedBreezeTB, ActionID.MakeSpell(AID.HoneyedBreeze), 5f, tankbuster: true);

class HoneyBLive(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.HoneyBLiveVisual), ActionID.MakeSpell(AID.HoneyBLive), 8.3f);
class Heartsore(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Heartsore), 6);
class Heartsick(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Heartsick), 6, 4, 4);
class Loveseeker(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Loveseeker), 10);
class BlowKiss(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlowKiss), new AOEShapeCone(40, 60.Degrees()));
class HoneyBFinale(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HoneyBFinale));
class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DropOfVenom), 6, 8, 8);
class SplashOfVenom(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SplashOfVenom), 6);

class BlindingLove1 : Components.SimpleAOEs
{
    public BlindingLove1(BossModule module) : base(module, ActionID.MakeSpell(AID.BlindingLove1), new AOEShapeRect(50, 4)) { MaxDangerColor = 2; }
}
class BlindingLove2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BlindingLove2), new AOEShapeRect(50, 4));
class HeartStruck1(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HeartStruck1), 4);
class HeartStruck2(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HeartStruck2), 6);
class HeartStruck3(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.HeartStruck3), 10, maxCasts: 8);

class Fracture(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.Fracture), 4)
{
    public override void Update()
    {
        var count = Towers.Count;
        if (count == 0)
            return;
        var party = Raid.WithoutSlot(false, true, true);
        var len = party.Length;
        BitMask forbidden = new();
        for (var i = 0; i < len; ++i)
        {
            ref readonly var statuses = ref party[i].Statuses;
            var lenStatuses = statuses.Length;
            for (var j = 0; j < lenStatuses; ++j)
            {
                if (statuses[j].ID is ((uint)SID.HeadOverHeels) or ((uint)SID.HopelessDevotion))
                {
                    forbidden[i] = true;
                }
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            ref var t = ref towers[i];
            t.ForbiddenSoakers = forbidden;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 987, NameID = 12685)]
public class M02NHoneyBLovely(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
