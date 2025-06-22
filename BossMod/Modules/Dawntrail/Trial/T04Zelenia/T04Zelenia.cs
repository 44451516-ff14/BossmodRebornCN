namespace BossMod.Dawntrail.Trial.T04Zelenia;

class PowerBreak(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PowerBreak1, (uint)AID.PowerBreak2], new AOEShapeRect(24f, 32f));

class HolyHazard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyHazard, new AOEShapeCone(24f, 60f.Degrees()), 2);

class RosebloodBloom(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RosebloodBloom, 10f, true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var source = Casters[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Position, 6f), Module.CastFinishAt(source.CastInfo));
        }
    }
}

class ThunderSlash : Components.SimpleAOEs
{
    public ThunderSlash(BossModule module) : base(module, (uint)AID.ThunderSlash, new AOEShapeCone(24f, 30f.Degrees()), 4)
    {
        MaxDangerColor = 2;
    }
}

class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, (uint)AID.RosebloodBloom); // using the knockback here, since after knockback player is stunned for a cutscene and can't heal up
class ThornedCatharsis(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThornedCatharsis);
class SpecterOfTheLost(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SpecterOfTheLost, new AOEShapeCone(50f, 22.5f.Degrees()), tankbuster: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1030, NameID = 13861)]
public class T04Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultArena)
{
    private static readonly WPos arenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsComplex DefaultArena = new([new Polygon(arenaCenter, 16f, 64)]);
    public static readonly ArenaBoundsComplex DonutArena = new([new DonutV(arenaCenter, 4f, 16f, 64)]);
}
