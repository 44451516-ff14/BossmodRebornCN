namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class SkyruinFire(BossModule module) : Components.CastCounter(module, (uint)AID.SkyruinFireAOE);
sealed class SkyruinIce(BossModule module) : Components.CastCounter(module, (uint)AID.SkyruinIceAOE);
sealed class SkyruinThunder(BossModule module) : Components.CastCounter(module, (uint)AID.SkyruinThunderAOE);
sealed class DisasterZoneFire(BossModule module) : Components.CastCounter(module, (uint)AID.DisasterZoneFireAOE);
sealed class DisasterZoneIce(BossModule module) : Components.CastCounter(module, (uint)AID.DisasterZoneIceAOE);
sealed class DisasterZoneThunder(BossModule module) : Components.CastCounter(module, (uint)AID.DisasterZoneThunderAOE);
sealed class Tulidisaster1(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterAOE1);
sealed class Tulidisaster2(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterAOE2);
sealed class Tulidisaster3(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterAOE3);
sealed class IceTalon(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.IceTalon, (uint)AID.IceTalonAOE, 5.1f, tankbuster: true);
sealed class WrathUnfurled(BossModule module) : Components.CastCounter(module, (uint)AID.WrathUnfurledAOE);
sealed class TulidisasterEnrage1(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterEnrageAOE1);
sealed class TulidisasterEnrage2(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterEnrageAOE2);
sealed class TulidisasterEnrage3(BossModule module) : Components.CastCounter(module, (uint)AID.TulidisasterEnrageAOE3);

// TODO: investigate how exactly are omens drawn for northern cross & susurrant breath
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 833, NameID = 12854, PlanLevel = 100)]
public sealed class Ex1Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IceBoulderJail));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.IceBoulderJail => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}

