namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

sealed class Megaflare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Megaflare, 6f);
sealed class Puddles(BossModule module) : Components.PersistentInvertibleVoidzoneByCast(module, 5f, GetVoidzones, (uint)AID.Hellfire)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Puddles);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class JudgementBolt(BossModule module) : Components.RaidwideCast(module, (uint)AID.JudgementBoltVisual);
sealed class Hellfire(BossModule module) : Components.RaidwideCast(module, (uint)AID.HellfireVisual);
sealed class StarBeyondStars(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StarBeyondStarsHelper, new AOEShapeCone(50f, 15f.Degrees()), 6);
sealed class TheEdgeUnbound(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheEdgeUnbound, 10f);
sealed class WyrmsTongue(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WyrmsTongueHelper, new AOEShapeCone(40f, 30f.Degrees()));

sealed class NineNightsAvatar : Components.SimpleAOEs
{
    public NineNightsAvatar(BossModule module) : base(module, (uint)AID.NineNightsAvatar, 10f) { Color = Colors.Danger; }
}

sealed class NineNightsHelpers : Components.SimpleAOEs
{
    public NineNightsHelpers(BossModule module) : base(module, (uint)AID.NineNightsHelpers, 10f, 6) { MaxDangerColor = 2; }
}
sealed class VeilAsunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VeilAsunderHelper, 6f);
sealed class MortalCoil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortalCoilVisual, new AOEShapeDonut(8f, 20f));
sealed class DiamondDust(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondDustVisual, "Raidwide. Turns floor to ice.");
sealed class DeadGaze(BossModule module) : Components.CastGaze(module, (uint)AID.DeadGazeVisual);
sealed class TidalWave2(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWaveVisual2, 25f, kind: Kind.DirForward, stopAtWall: true);
sealed class SwiftAsShadow(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SwiftAsShadow, 1f);
sealed class Extinguishment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExtinguishmentVisual, new AOEShapeDonut(10f, 30f));
sealed class TheEdgeUnbound2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheEdgeUnbound2, 10f);

sealed class UnmovingDvenadkatik : Components.SimpleAOEs
{
    public UnmovingDvenadkatik(BossModule module) : base(module, (uint)AID.UnmovingDvenadkatikVisual, new AOEShapeCone(50f, 15f.Degrees()), 6) { MaxDangerColor = 2; }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat, Malediktus", PrimaryActorOID = (uint)OID.ZenosP1, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70000, NameID = 10393)]
public sealed class Endwalker(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(19.5f))
{
    private Actor? _zenosP2;
    public Actor? ZenosP2() => _zenosP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_zenosP2 == null)
        {
            var b = Enemies((uint)OID.ZenosP2);
            _zenosP2 = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_zenosP2);
    }
}
