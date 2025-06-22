namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class SlitheringStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SlitheringStrike, new AOEShapeCone(24f, 90f.Degrees()));
class Skyruin1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Skyruin1);
class Skyruin2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Skyruin2);
class HailOfFeathers(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfFeathers);
class DisasterZone1(BossModule module) : Components.RaidwideCast(module, (uint)AID.DisasterZone1);
class DisasterZone2(BossModule module) : Components.RaidwideCast(module, (uint)AID.DisasterZone2);

abstract class CalamitousCry(BossModule module, uint aid) : Components.LineStack(module, aidMarker: aid, (uint)AID.CalamitousCry, 5f, 60f, 3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.LimitBreakVisual4) // not sure if line stack gets cancelled when limit break phase ends, just a safety feature
            CurrentBaits.Clear();
    }
}
class CalamitousCry1(BossModule module) : CalamitousCry(module, (uint)AID.CalamitousCryMarker1);
class CalamitousCry2(BossModule module) : CalamitousCry(module, (uint)AID.CalamitousCryMarker2);

class FreezingDust(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FreezingDust)
            Array.Fill(PlayerStates, new(Requirement.Move, Module.CastFinishAt(spell, 1)));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.FreezingUp && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status) // it sometimes seems to skip the freezing up debuff?
    {
        if (status.ID == (uint)SID.DeepFreeze && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class RuinForetold(BossModule module) : Components.RaidwideCast(module, (uint)AID.RuinForetold);
class CalamitousEcho(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CalamitousEcho, new AOEShapeCone(40f, 10f.Degrees()));

abstract class Tulidisaster(BossModule module, uint aid, float delay) : Components.RaidwideCastDelay(module, (uint)AID.TulidisasterVisual, aid, delay);
class Tulidisaster1(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster1, 3.1f);
class Tulidisaster2(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster2, 11.6f);
class Tulidisaster3(BossModule module) : Tulidisaster(module, (uint)AID.Tulidisaster3, 19.6f);

class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 6f);
class IceTalon(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Tankbuster, (uint)AID.IceTalon, 5f, tankbuster: true);

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
    private static readonly uint[] objects = [(uint)OID.IceBoulder, (uint)OID.FlameKissedBeacon, (uint)OID.GlacialBeacon, (uint)OID.ThunderousBeacon];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(objects), Colors.Object);
    }
}
