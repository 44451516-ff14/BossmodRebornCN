namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class GloryNeverlasting(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.GloryNeverlasting));
class ArtOfTheFireblossom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheFireblossom), new AOEShapeCircle(9));
class ArtOfTheWindblossom(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArtOfTheWindblossom), new AOEShapeDonut(5, 60));
class KugeRantsui(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KugeRantsui));
class OkaRanman(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OkaRanman));
class LevinblossomStrike(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LevinblossomStrike), 3);
class DriftingPetals(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.DriftingPetals), 15, ignoreImmunes: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        var component = Module.FindComponent<Mudrain>()?.ActiveAOEs(slot, actor)?.ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            forbidden.Add(ShapeDistance.InvertedCircle(source.Origin, 5));
            if (component != null && component.Count != 0)
                foreach (var c in component)
                    forbidden.Add(ShapeDistance.Cone(source.Origin, 20, Angle.FromDirection(c.Origin - Arena.Center), 20.Degrees()));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Min(f => f(p)), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Witherwind>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) ||
    (Module.FindComponent<Mudrain>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class Mudrain(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.Mudrain), module => module.Enemies(OID.MudVoidzone).Where(z => z.EventState != 7), 0.7f);
class Icebloom(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Icebloom), 6);
class Shadowflight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shadowflight), new AOEShapeRect(10, 3));
class MudPie(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MudPie), new AOEShapeRect(60, 3));
class FireblossomFlare(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FireblossomFlare), 6);
class ArtOfTheFluff1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff1));
class ArtOfTheFluff2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff2));
class TatamiGaeshi(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TatamiGaeshi), new AOEShapeRect(40, 5));
class AccursedSeedling(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.AccursedSeedling));

class RootArrangement(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), ActionID.MakeSpell(AID.RockRootArrangementFirst), ActionID.MakeSpell(AID.RockRootArrangementRest), 4, 1, 4, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(19, 0), Arena.Center + new WDir(-19, 0), 20), Activation);
    }
}

class Witherwind(BossModule module) : Components.PersistentVoidzone(module, 3, m => m.Enemies(OID.AutumnalTempest), 20);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12325, SortOrder = 1)]
public class V021Yozakura(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -700 ? ArenaCenter1 : primary.Position.X > 700 ? ArenaCenter2 : ArenaCenter3, primary.Position.X < -700 ? StartingBounds : primary.Position.X > 700 ? DefaultBounds2 : StartingBounds)
{
    public static readonly WPos ArenaCenter1 = new(-775, 16);
    public static readonly WPos ArenaCenter2 = new(737, 220);
    public static readonly WPos ArenaCenter3 = new(47, 93);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds1 = new(20);
    public static readonly ArenaBoundsSquare DefaultBounds2 = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.LivingGaol));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LivingGaol => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
