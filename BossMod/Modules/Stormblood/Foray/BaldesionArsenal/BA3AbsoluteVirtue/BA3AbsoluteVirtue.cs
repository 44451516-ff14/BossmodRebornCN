namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA3AbsoluteVirtue;

sealed class Meteor(BossModule module) : Components.RaidwideCast(module, (uint)AID.Meteor);
sealed class MedusaJavelin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MedusaJavelin, new AOEShapeCone(65.4f, 45f.Degrees()));
sealed class AuroralWind(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AuroralWind, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class ExplosiveImpulse(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ExplosiveImpulse1, (uint)AID.ExplosiveImpulse2], 18f);
sealed class AernsWynavExplosion(BossModule module) : Components.CastHint(module, (uint)AID.ExplosionWyvern, "Aerns Wyvnav is enraging!", true);
sealed class MeteorEnrageCounter(BossModule module) : Components.CastCounter(module, (uint)AID.MeteorEnrageRepeat);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.BaldesionArsenal, GroupID = 639, NameID = 7976, PlanLevel = 70, SortOrder = 4)]
public sealed class BA3AbsoluteVirtue(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-175, 314), 29.95f, 96), new Rectangle(new(-146f, 314f), 0.8f, 5.8f), new Rectangle(new(-175f, 285f), 6f, 1.05f)],
    [new Rectangle(new(-144.4f, 314f), 0.8f, 5.8f), new Polygon(new(-144.85f, 306.75f), 1.5f, 8, 22.5f.Degrees()), new Polygon(new(-144.85f, 321.25f), 1.5f, 8, 22.5f.Degrees()),
    new Rectangle(new(-206, 314), 1.525f, 20f)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AernsWynav));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AernsWynav => 1,
                _ => 0
            };
        }
    }
}
