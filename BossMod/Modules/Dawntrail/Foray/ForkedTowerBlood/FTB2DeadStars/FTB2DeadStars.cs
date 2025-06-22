namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class NoxiousNova(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.NoxiousNovaVisual, (uint)AID.NoxiousNova, 0.8f);
sealed class PrimordialChaosRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.PrimordialChaosVisual1, (uint)AID.PrimordialChaos, 1.2f);
sealed class SliceNDice(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SliceNDice, new AOEShapeCone(70f, 45f.Degrees()), tankbuster: true);
sealed class NoisomeNuisanceIceboundBuffoonBlazingBelligerent(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NoisomeNuisance,
(uint)AID.IceboundBuffoon, (uint)AID.BlazingBelligerent], 6f);
sealed class VengefulBioIIIBlizzardIIIFireIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VengefulBioIII,
(uint)AID.VengefulBlizzardIII, (uint)AID.VengefulFireIII], new AOEShapeCone(60f, 60f.Degrees()));
sealed class DeltaAttack(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.DeltaAttackFirst, (uint)AID.DeltaAttackRepeat]);
sealed class DeltaAttackRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DeltaAttackVisual1, (uint)AID.DeltaAttackFirst, 0.5f);
sealed class Avalaunch(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Avalaunch, (uint)AID.Avalaunch, 8f, 7.8f, 24, 24);
sealed class SelfDestruct(BossModule module) : Components.CastHints(module, [(uint)AID.SelfDestructVisual1, (uint)AID.SelfDestruct2], "Enrage!", true);
sealed class GeothermalRupture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GeothermalRupture, 8f);
sealed class FlameThrower(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.FlameThrowerVisual, (uint)AID.FlameThrower, 0.1f);

abstract class Firestrike(BossModule module, uint aidMarker, float delay) : Components.LineStack(module, aidMarker: aidMarker, (uint)AID.Firestrike, delay, 70f, 5f, 8, 24);
sealed class Firestrike1(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker1, 5.1f);
sealed class Firestrike2(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker2, 6.2f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018, NameID = 13737, PlanLevel = 100, SortOrder = 3)]
public sealed class FTB2DeadStars(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    public static readonly WPos ArenaCenter = new(-800f, 360f);
    private Actor? _bossPhobos;
    private Actor? _bossNereid;
    public Actor? BossNereid() => _bossNereid;
    private Actor? _bossLiquifiedTriton;
    private Actor? _bossLiquifiedNereid;
    private Actor? _bossFrozenTriton;
    private Actor? _bossFrozenPhobos;
    private Actor? _bossGaseousNereid;
    private Actor? _bossGaseousPhobos;
    private Actor? _bossDeadStars;
    public Actor? BossDeadStars() => _bossDeadStars;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bossPhobos == null)
        {
            var b = Enemies((uint)OID.Phobos);
            _bossPhobos = b.Count != 0 ? b[0] : null;
        }
        if (_bossNereid == null)
        {
            var b = Enemies((uint)OID.Nereid);
            _bossNereid = b.Count != 0 ? b[0] : null;
        }
        if (_bossLiquifiedTriton == null)
        {
            var b = Enemies((uint)OID.LiquifiedTriton);
            _bossLiquifiedTriton = b.Count != 0 ? b[0] : null;
        }
        if (_bossLiquifiedNereid == null)
        {
            var b = Enemies((uint)OID.LiquifiedNereid);
            _bossLiquifiedNereid = b.Count != 0 ? b[0] : null;
        }
        if (_bossFrozenTriton == null)
        {
            var b = Enemies((uint)OID.FrozenTriton);
            _bossFrozenTriton = b.Count != 0 ? b[0] : null;
        }
        if (_bossFrozenPhobos == null)
        {
            var b = Enemies((uint)OID.FrozenPhobos);
            _bossFrozenPhobos = b.Count != 0 ? b[0] : null;
        }
        if (_bossGaseousNereid == null)
        {
            var b = Enemies((uint)OID.GaseousNereid);
            _bossGaseousNereid = b.Count != 0 ? b[0] : null;
        }
        if (_bossGaseousPhobos == null)
        {
            var b = Enemies((uint)OID.GaseousPhobos);
            _bossGaseousPhobos = b.Count != 0 ? b[0] : null;
        }
        if (_bossDeadStars == null)
        {
            var b = Enemies((uint)OID.DeadStars);
            _bossDeadStars = b.Count != 0 ? b[0] : null;
        }
    }

    private static readonly ArenaBoundsComplex startingArena = new([new Polygon(ArenaCenter, 39.5f * CosPI.Pi48th, 48)], [new Rectangle(new(-800f, 400f), 7.5f, 1.25f),
    new Rectangle(new(-800f, 320f), 7.5f, 1.25f)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(30f);
    public static readonly ArenaBoundsComplex FistFightArena = new([new DonutV(WPos.ClampToGrid(ArenaCenter), 12f, 30f, 64)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_bossDeadStars);
        Arena.Actor(_bossNereid);
        Arena.Actor(_bossPhobos);
        Arena.Actor(_bossGaseousPhobos);
        Arena.Actor(_bossFrozenPhobos);
        Arena.Actor(_bossFrozenTriton);
        Arena.Actor(_bossGaseousNereid);
        Arena.Actor(_bossLiquifiedNereid);
        Arena.Actor(_bossLiquifiedTriton);
        Arena.Actor(PrimaryActor);
    }
}
