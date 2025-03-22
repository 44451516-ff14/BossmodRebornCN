namespace BossMod.Endwalker.VariantCriterion.V02MR.V021Yozakura;

class GloryNeverlasting(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.GloryNeverlasting));
class ArtOfTheFireblossom(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ArtOfTheFireblossom), 9f);
class ArtOfTheWindblossom(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.ArtOfTheWindblossom), new AOEShapeDonut(5f, 60f));
class KugeRantsui(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KugeRantsui));
class OkaRanman(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OkaRanman));
class LevinblossomStrike(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LevinblossomStrike), 3f);
class DriftingPetals(BossModule module) : Components.SimpleKnockbacks(module, ActionID.MakeSpell(AID.DriftingPetals), 15, ignoreImmunes: true)
{
    private readonly Mudrain _aoe1 = module.FindComponent<Mudrain>()!;
    private readonly Witherwind _aoe2 = module.FindComponent<Witherwind>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            var component = _aoe1.Sources(Module).ToList();
            var forbidden = new List<Func<WPos, float>>
            {
                ShapeDistance.InvertedCircle(source.Position, 5f)
            };
            if (component.Count != 0)
                for (var i = 0; i < component.Count; ++i)
                    forbidden.Add(ShapeDistance.Cone(source.Position, 20f, Angle.FromDirection(component[i].Position - Arena.Center), 20f.Degrees()));
            hints.AddForbiddenZone(ShapeDistance.Union(forbidden), Module.CastFinishAt(source.CastInfo));
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        foreach (var aoe in _aoe1.ActiveAOEs(slot, actor))
        {
            if (aoe.Shape.Check(pos, aoe.Origin, aoe.Rotation))
                return true;
        }
        foreach (var aoe in _aoe2.ActiveAOEs(slot, actor))
        {
            if (aoe.Shape.Check(pos, aoe.Origin, aoe.Rotation))
                return true;
        }
        return !Arena.InBounds(pos);
    }
}

class Mudrain(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, ActionID.MakeSpell(AID.Mudrain), module => module.Enemies((uint)OID.MudVoidzone).Where(z => z.EventState != 7), 0.7f);
class Icebloom(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Icebloom), 6);
class Shadowflight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Shadowflight), new AOEShapeRect(10f, 3f));
class MudPie(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MudPie), new AOEShapeRect(60f, 3f));
class FireblossomFlare(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FireblossomFlare), 6f);
class ArtOfTheFluff1(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff1));
class ArtOfTheFluff2(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ArtOfTheFluff2));
class TatamiGaeshi(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.TatamiGaeshi), new AOEShapeRect(40f, 5f));
class AccursedSeedling(BossModule module) : Components.Voidzone(module, 4f, m => m.Enemies((uint)OID.AccursedSeedling));

class RootArrangement(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4f), ActionID.MakeSpell(AID.RockRootArrangementFirst), ActionID.MakeSpell(AID.RockRootArrangementRest), 4, 1, 4, true, (uint)IconID.ChasingAOE)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(19f, default), Arena.Center + new WDir(-19f, default), 20f), Activation);
    }
}

class Witherwind(BossModule module) : Components.Voidzone(module, 3f, m => m.Enemies((uint)OID.AutumnalTempest), 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12325, SortOrder = 1)]
public class V021Yozakura(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -700 ? ArenaCenter1 : primary.Position.X > 700 ? ArenaCenter2 : ArenaCenter3, primary.Position.X < -700 ? StartingBounds : primary.Position.X > 700 ? DefaultBounds2 : StartingBounds)
{
    public static readonly WPos ArenaCenter1 = new(-775f, 16f);
    public static readonly WPos ArenaCenter2 = new(737f, 220f);
    public static readonly WPos ArenaCenter3 = new(47f, 93f);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds1 = new(20f);
    public static readonly ArenaBoundsSquare DefaultBounds2 = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LivingGaol));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.LivingGaol => 1,
                _ => 0
            };
        }
    }
}
