namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class NoxiousNova(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.NoxiousNovaVisual, (uint)AID.NoxiousNova, 0.8d);
sealed class PrimordialChaosRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.PrimordialChaosVisual1, (uint)AID.PrimordialChaos, 1.2d);
sealed class SliceNDice(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SliceNDice, new AOEShapeCone(70f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class NoisomeNuisanceIceboundBuffoonBlazingBelligerent(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NoisomeNuisance,
(uint)AID.IceboundBuffoon, (uint)AID.BlazingBelligerent], 6f);
sealed class VengefulBioIIIBlizzardIIIFireIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VengefulBioIII,
(uint)AID.VengefulBlizzardIII, (uint)AID.VengefulFireIII], new AOEShapeCone(60f, 60f.Degrees()));
sealed class DeltaAttack(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.DeltaAttackFirst, (uint)AID.DeltaAttackRepeat]);
sealed class DeltaAttackRaidwide(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DeltaAttackVisual1, (uint)AID.DeltaAttackFirst, 0.5d);
sealed class Avalaunch(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Avalaunch, (uint)AID.Avalaunch, 8f, 7.8d, 24, 24);
sealed class SelfDestruct(BossModule module) : Components.CastHints(module, [(uint)AID.SelfDestructVisual1, (uint)AID.SelfDestruct2], "Enrage!", true);
sealed class GeothermalRupture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GeothermalRupture, 8f);
sealed class FlameThrower(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.FlameThrowerMarker, (uint)AID.FlameThrower, 5d, 40f, 4f, 24, 24, 1, false);

abstract class Firestrike(BossModule module, uint aidMarker, double delay) : Components.LineStack(module, aidMarker: aidMarker, (uint)AID.Firestrike, delay, 70f, 5, 16, 24);
sealed class Firestrike1(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker1, 5.1d);
sealed class Firestrike2(BossModule module) : Firestrike(module, (uint)AID.FirestrikeMarker2, 6.2d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Triton, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018, NameID = 13737, PlanLevel = 100, SortOrder = 3, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
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
    public Actor? BossDeadStars;
    public Actor? DeathWall;

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
        if (BossDeadStars == null)
        {
            var b = Enemies((uint)OID.DeadStars);
            BossDeadStars = b.Count != 0 ? b[0] : null;
        }
        if (DeathWall == null)
        {
            var b = Enemies((uint)OID.Deathwall);
            DeathWall = b.Count != 0 ? b[0] : null;
        }
    }

    private static readonly ArenaBoundsComplex startingArena = new([new Polygon(ArenaCenter, 39.5f * CosPI.Pi48th, 48)], [new Rectangle(new(-800f, 400f), 7.5f, 1.25f),
    new Rectangle(new(-800f, 320f), 7.5f, 1.25f)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(30f);
    public static readonly ArenaBoundsComplex FistFightArena = new([new DonutV(ArenaCenter.Quantized(), 12f, 30f, 64)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossDeadStars);
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
