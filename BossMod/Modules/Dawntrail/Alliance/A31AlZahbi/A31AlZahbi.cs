namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Medusa, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14839u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 2)]
public sealed class A31AlZahbi(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(29f, 25f))
{
    public static readonly WPos ArenaCenter = new(720f, 720f);

    public static readonly uint[] AlZahbiMobs = [(uint)OID.Acrolith, (uint)OID.LamiaRover1, (uint)OID.LamiaRover2, (uint)OID.LamiaJaeger, (uint)OID.QutrubForayer, (uint)OID.PiningAbazohn, (uint)OID.NemeanLion, (uint)OID.LamiaNo2, (uint)OID.AssaultBhoot, (uint)OID.Medusa];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, AlZahbiMobs);
    }
}

//Wave 1
// Acrolith = 14839u
// Lamia Rover = 14832u

//Wave 2
// Lamia Jaeger = 14830u
// Qutrub Forayer = 14835u
// Pining Abazohn = 14834u

//Wave 3
// Nemean Lion = 14829u
// Lamia No.2 = 14831u
// Assault Bhoot = 14836u

//Wave 4
// Medusa Swarmsinger = 14828u
