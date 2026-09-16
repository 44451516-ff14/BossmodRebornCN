namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class WindsPeak(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindsPeak, 5f, arenaProjectionLayer: 1);

sealed class WindsPeakKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WindsPeak, 10f, arenaProjectionLayer: 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && Module.ActorMatchesArenaProjectionLayer(actor, 1, true))
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 10f), c.Activation);
        }
    }
}

sealed class HeartOfNature(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeartOfNature, arenaProjectionLayer: 1);

sealed class TheKingsNotice(BossModule module) : Components.CastGaze(module, (uint)AID.TheKingsNotice, arenaProjectionLayer: 1);

sealed class TasteOfBlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TasteOfBlood, new AOEShapeCone(40f, 90f.Degrees()), arenaProjectionLayer: 1);

sealed class Pentagust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pentagust, new AOEShapeCone(50f, 10f.Degrees()), arenaProjectionLayer: 0);

sealed class FervidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FervidPulse, new AOEShapeCross(50f, 7f), arenaProjectionLayer: 0);

sealed class FrigidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidPulse, new AOEShapeDonut(12f, 60f), arenaProjectionLayer: 0);

sealed class SwoopingFrenzy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzy, 12f, arenaProjectionLayer: 0);

sealed class MoltingPlumage(BossModule module) : Components.RaidwideCast(module, (uint)AID.MoltingPlumage, arenaProjectionLayer: 0);

sealed class Scratch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Scratch, arenaProjectionLayer: 0);

sealed class TwinAgonies(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TwinAgonies, arenaProjectionLayer: 1);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735u, NameID = 9452u)]
public sealed class CLL4Dawon : BossModule
{
    public CLL4Dawon(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private CLL4Dawon(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(80f, -813f), 34.5f, 96)], [new Rectangle(new(80f, -778f), 20f, 1.25f)]) { Y = 254.5f, BorderY = 254.5f };
        return (arena.Center, arena);
    }

    private readonly uint[] adds = [(uint)OID.TamedBeetle, (uint)OID.TamedCoeurl, (uint)OID.TamedManticore];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (ActorMatchesArenaProjectionLayer(pc, 1, true))
        {
            Arena.Actors(Enemies((uint)OID.LyonTheBeastKing)); // there are two of them, but only one is visible/targetable
        }
        else
        {
            Arena.Actors(this, adds);
            Arena.Actor(PrimaryActor);
        }
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        var potHints = CollectionsMarshal.AsSpan(hints.PotentialTargets);
        var isTop = ActorMatchesArenaProjectionLayer(actor, 1, true);
        for (var i = 0; i < count; ++i)
        {
            var h = potHints[i];
            var e = h.Actor;
            var enemyPrio = h.Priority;
            var oid = e.OID;
            if (isTop)
            {
                if (oid != (uint)OID.LyonTheBeastKing)
                {
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
                }
                else
                {
                    enemyPrio = 0;
                }
            }
            else
            {
                if (oid is not (uint)OID.Boss and not (uint)OID.LyonTheBeastKing)
                {
                    enemyPrio = 1;
                }
                else if (oid == (uint)OID.Boss)
                {
                    enemyPrio = 0;
                }
                else if (oid == (uint)OID.LyonTheBeastKing)
                {
                    enemyPrio = AIHints.Enemy.PriorityInvincible;
                }
            }
            h.Priority = enemyPrio;
        }
    }
}
