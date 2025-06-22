namespace BossMod.Stormblood.Trial.T02Lakshmi;

class DivineDenial(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineDenial);
class Stotram1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Stotram1);
class Stotram2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Stotram2);
class ThePallOfLightStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThePallOfLightStack, 7, 8, 8);
class ThePullOfLightTB1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThePullOfLightTB1);
class ThePullOfLightTB2(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThePullOfLightTB2);
class BlissfulArrow1(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulArrow1);
class BlissfulArrow2(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulArrow2);
class BlissfulSpear1(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulSpear1);
class BlissfulSpear2(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulSpear2);
class BlissfulSpear3(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulSpear3);
class BlissfulSpear4(BossModule module) : Components.CastCounter(module, (uint)AID.BlissfulSpear4);

class BlissfulSpear(BossModule module) : Components.BaitAwayIcon(module, 7f, (uint)IconID.Spread1, (uint)AID.BlissfulSpear1, 3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            CurrentBaits.Clear();
    }
}

class BlissfulArrow(BossModule module) : Components.BaitAwayIcon(module, 7f, (uint)IconID.Spread2, (uint)AID.BlissfulArrow2, 3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
            CurrentBaits.Clear();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 263, NameID = 6385)]
public class T02Lakshmi(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f));
