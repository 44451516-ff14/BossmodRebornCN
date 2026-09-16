namespace BossMod.Stormblood.Trial.T09Seiryu;

sealed class HundredTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HundredTonzeSwing, 16f, restrictToArenaProjectionLayer: null);
sealed class CoursingRiver(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.CoursingRiverAOE, 25f, true, kind: Kind.DirForward, restrictToArenaProjectionLayer: null)
{
    private readonly Handprint _aoe = module.FindComponent<Handprint>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Casters.Count == 0 && Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDKnockbackInCircleFixedDirection(Arena.Center, 25f * c.Direction.ToDirection(), 19f), c.Activation);
        }
    }
}

sealed class DragonsWake(BossModule module) : Components.RaidwideCast(module, (uint)AID.DragonsWake2);
sealed class FifthElement(BossModule module) : Components.RaidwideCast(module, (uint)AID.FifthElement, restrictToArenaProjectionLayer: null);
sealed class FortuneBladeSigil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FortuneBladeSigil, new AOEShapeRect(50.5f, 2f), restrictToArenaProjectionLayer: null);

sealed class InfirmSoul(BossModule module) : Components.BaitAwayCast(module, (uint)AID.InfirmSoul, 4f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class SerpentDescending(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.SerpentDescending, 5f, 6d, restrictToArenaProjectionLayer: null);
sealed class YamaKagura(BossModule module) : Components.SimpleAOEs(module, (uint)AID.YamaKagura, new AOEShapeRect(62.7f, 3f), restrictToArenaProjectionLayer: null);
sealed class Handprint(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Handprint1, new AOEShapeCone(40f, 90f.Degrees()), restrictToArenaProjectionLayer: null);

sealed class ForceOfNature1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ForceOfNature1, 10f, restrictToArenaProjectionLayer: null)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 10f), Casters.Ref(0).Activation);
        }
    }
}
sealed class ForceOfNature2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForceOfNature2, 5f, arenaProjectionLayer: 1);
sealed class KanaboBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(44f, 30f.Degrees()), (uint)TetherID.BaitAway, (uint)AID.KanaboVisual2, (uint)OID.IwaNoShiki, 5.9d, restrictToArenaProjectionLayer: null)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsBaitTarget(actor))
        {
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 19f), WorldState.FutureTime(ActivationDelay));
        }
    }
}

sealed class KanaboAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Kanabo, new AOEShapeCone(44f, 30f.Degrees()), restrictToArenaProjectionLayer: null);
sealed class BlueBolt(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.BlueBoltMarker, (uint)AID.BlueBolt, 5.9d, 83f, 2.5f, restrictToArenaProjectionLayer: null)
{
    public override void Update()
    {
        if (CurrentBaits.Count != 0)
        {
            CurrentBaits.Ref(0).Forbidden = ForbiddenPlayers;
        }
    }
}

sealed class ForbiddenArts(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.ForbiddenArtsMarker, (uint)AID.ForbiddenArtsSecond, 5.2f, 84.4f, 4, restrictToArenaProjectionLayer: null); // this hits twice
sealed class RedRush(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(82.6f, 2.5f), (uint)TetherID.BaitAway, (uint)AID.RedRush, (uint)OID.AkaNoShiki, 6d, restrictToArenaProjectionLayer: null)
{
    private readonly BlueBolt _stack = module.FindComponent<BlueBolt>()!;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.AkaNoShiki)
        {
            return;
        }
        base.OnTethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            _stack.ForbiddenPlayers.Set(Raid.FindSlot(player.InstanceID));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.AkaNoShiki)
        {
            return;
        }
        base.OnUntethered(source, tether);
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            _stack.ForbiddenPlayers.Clear(Raid.FindSlot(player.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsBaitTarget(actor))
        {
            hints.AddForbiddenZone(Arena.Bounds.Radius >= 20f ? new SDInvertedCircle(Arena.Center, 5f) : new SDCircle(Arena.Center, 18.5f), WorldState.FutureTime(ActivationDelay));
        }
    }
}

public abstract class SeiryuTrial : BossModule
{
    public SeiryuTrial(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private SeiryuTrial(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 19.5f, 48)]);
        return (arena.Center, arena);
    }

    public static ArenaBoundsCustom GetPhase2Arena()
    {
        var center = new WPos(100f, 100f);
        Polygon phase2withwater = new Polygon(center, 44.5f, 48);
        var phase2nowater = new Polygon(center, 20f, 48);
        var polywithwater = new RelSimplifiedComplexPolygon(phase2withwater.Contour(center));
        var polynowater = new RelSimplifiedComplexPolygon(phase2nowater.Contour(center));
        var arena = new ArenaBoundsCustom([phase2withwater], WorldProjectionLayers: [new(polywithwater, -0.8f, borderY: -0.8f), new(polynowater, 0f, borderY: 0f)]);
        return arena;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Seiryu, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637u, NameID = 7922u)]
public sealed class T09Seiryu(WorldState ws, Actor primary) : SeiryuTrial(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DoroNoShiki));
        Arena.Actors(Enemies((uint)OID.NumaNoShiki));
    }
}
