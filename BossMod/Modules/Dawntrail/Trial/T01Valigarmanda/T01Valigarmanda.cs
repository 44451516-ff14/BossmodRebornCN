namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class SlitheringStrike(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitheringStrike), new AOEShapeCone(24, 90.Degrees()));
class Skyruin1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Skyruin1));
class Skyruin2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Skyruin2));
class HailOfFeathers(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HailOfFeathers));
class DisasterZone1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone1));
class DisasterZone2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone2));

abstract class CalamitousCry(BossModule module, AID aid) : Components.LineStack(module, ActionID.MakeSpell(aid), ActionID.MakeSpell(AID.CalamitousCry), 5, 60, 3)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.LimitBreakVisual4) // not sure if line stack gets cancelled when limit break phase ends, just a safety feature
            CurrentBaits.Clear();
    }
}
class CalamitousCry1(BossModule module) : CalamitousCry(module, AID.CalamitousCryMarker1);
class CalamitousCry2(BossModule module) : CalamitousCry(module, AID.CalamitousCryMarker2);

class FreezingDust(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FreezingDust)
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 1)));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status) // it sometimes seems to skip the freezing up debuff?
    {
        if ((SID)status.ID == SID.DeepFreeze && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class RuinForetold(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.RuinForetold));
class CalamitousEcho(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CalamitousEcho), new AOEShapeCone(40, 10.Degrees()));

abstract class Tulidisaster(BossModule module, AID aid, float delay) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.TulidisasterVisual), ActionID.MakeSpell(aid), delay);
class Tulidisaster1(BossModule module) : Tulidisaster(module, AID.Tulidisaster1, 3.1f);
class Tulidisaster2(BossModule module) : Tulidisaster(module, AID.Tulidisaster2, 11.6f);
class Tulidisaster3(BossModule module) : Tulidisaster(module, AID.Tulidisaster3, 19.6f);

class Eruption(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Eruption), 6);
class IceTalon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, ActionID.MakeSpell(AID.IceTalon), 5, true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class T01ValigarmandaStates : StateMachineBuilder
{
    public T01ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SlitheringStrike>()
            .ActivateOnEnter<ArcaneLightning>()
            .ActivateOnEnter<StranglingCoilSusurrantBreath>()
            .ActivateOnEnter<Skyruin1>()
            .ActivateOnEnter<Skyruin2>()
            .ActivateOnEnter<HailOfFeathers>()
            .ActivateOnEnter<DisasterZone1>()
            .ActivateOnEnter<DisasterZone2>()
            .ActivateOnEnter<RuinfallTower>()
            .ActivateOnEnter<RuinfallKB>()
            .ActivateOnEnter<RuinfallAOE>()
            .ActivateOnEnter<ChillingCataclysm>()
            .ActivateOnEnter<NorthernCross>()
            .ActivateOnEnter<FreezingDust>()
            .ActivateOnEnter<CalamitousEcho>()
            .ActivateOnEnter<CalamitousCry1>()
            .ActivateOnEnter<CalamitousCry2>()
            .ActivateOnEnter<Tulidisaster1>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<IceTalon>()
            .ActivateOnEnter<Tulidisaster1>()
            .ActivateOnEnter<Tulidisaster2>()
            .ActivateOnEnter<Tulidisaster3>()
            .ActivateOnEnter<ThunderPlatform>()
            .ActivateOnEnter<BlightedBolt1>()
            .ActivateOnEnter<BlightedBolt2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 832, NameID = 12854)]
public class T01Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.IceBoulder).Concat(Enemies(OID.FlameKissedBeacon)).Concat(Enemies(OID.GlacialBeacon)).Concat(Enemies(OID.ThunderousBeacon)).Concat([PrimaryActor]));
    }
}
