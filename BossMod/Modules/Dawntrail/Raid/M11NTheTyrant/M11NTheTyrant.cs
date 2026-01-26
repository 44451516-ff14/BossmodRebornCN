namespace BossMod.Modules.Dawntrail.Raid.M11NTheTyrant;

sealed class CrownOfArcadia(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;


        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Add(new(rect, new WPos(74, 100)));
            _aoes.Add(new(rect, new WPos(126, 100)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}





sealed class Smashdown1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownScytheAOE, new AOEShapeDonut(5, 60));
sealed class Smashdown2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownAxeAOE, 8f);
sealed class Smashdown3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_SmashdownSwordAOE, new AOEShapeCross(40f, 5f));

/**
 * Void Stardust: Marks all players with AoEs that continue hitting them as well as spawning a ground AoE underneath.
 */
sealed class VoidStardust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.VoidStardustSpread, (uint)AID._Spell_VoidStardust, 4f, 4.7f)
{
    public int _numCasts;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_VoidStardust)
            _numCasts = 0;
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_Cometite or AID._Spell_Comet1)
        {
            _numCasts++;
        }
        if (_numCasts >= 16)
        {
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedSpreads;
        }
    }
}
sealed class Cometite(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_Cometite, 4f);

sealed class AssaultEvolved1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedSword, new AOEShapeCross(40, 5f));
sealed class AssaultEvolved2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedScythe, new AOEShapeDonut(5, 60));

sealed class AssaultEvolved3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_AssaultEvolvedAxe, 8f);
sealed class DanceOfDomination(BossModule module) : Components.RaidwideCast(module, (uint)AID._Weaponskill_DanceOfDomination);
sealed class Explosion1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion, new AOEShapeRect(60f, 5f));
sealed class Explosion2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion, new AOEShapeRect(60f, 5f));
sealed class Explosion3(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_Explosion, new AOEShapeRect(60f, 3f));

sealed class RawSteelTankBuster(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.RawSteelSharedTankbuster, (uint)AID._Weaponskill_RawSteel1, 6f);
sealed class RawSteelSpreads(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.RawSteelSpread, (uint)AID._Weaponskill_Impact, 6, 0);
sealed class Charybdistopia(BossModule module) : Components.RaidwideCast(module, (uint)AID._Spell_Charybdistopia);


sealed class Maelstrom(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circ = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;


        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }


    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Add(new(circ, actor.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.Maelstrom)
        {
            _aoes.Clear();
        }
    }
}

//TODO : There is a fast cast of powerful gust that happens when boss teleports back to center that covers all radar and makes ai have a seizure.
// would be great to get rid of that.
class PowerfulGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;


        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Spell_PowerfulGust)
        {
            _aoes.Add(new(cone, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));

        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID._Spell_PowerfulGust)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, (uint)AID._Weaponskill_OneAndOnly);
sealed class CosmicKiss(BossModule module) : Components.CastTowers(module, (uint)AID._Ability_CosmicKiss, 4f);
sealed class MassiveMeteor(BossModule module) : Components.StackWithIcon(module, (uint)IconID.MassiveMeteorStack, (uint)AID._Ability_MassiveMeteor, 6, 0);
sealed class ForegoneFatality(BossModule module) : Components.TankbusterTether(module, (uint)AID._Spell_ForegoneFatality, (uint)TetherID._Gen_TankInterceptTether, 6f);

//TODO: Needs to run for the stone behind a stone. Right now it just goes to nearest stone. Could get hit by the second wave.
sealed class DoubleTyrannhilation(BossModule module)
    : Components.CastLineOfSightAOE(module, (uint)AID._Weaponskill_DoubleTyrannhilation1, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;
        if (count == 0)
            return default;

        var blockers = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (!c.IsDead)
                blockers.Add(c);
        }

        return CollectionsMarshal.AsSpan(blockers);
    }
}

sealed class HiddenTyrannhilation(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID._Weaponskill_HiddenTyrannhilation, 60f, false)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var comets = Module.Enemies((uint)OID.Comet);
        var count = comets.Count;
        if (count == 0)
            return default;

        var blockers = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var c = comets[i];
            if (!c.IsDead)
                blockers.Add(c);
        }

        return CollectionsMarshal.AsSpan(blockers);
    }
}

sealed class Flatliner(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(20, 6, 20);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;


        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_Flatliner)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // We cheat here instead of drawing two new shapes, we make a danger zone between the rectangles.
        if ((AID)spell.Action.ID is AID._Weaponskill_Flatliner)
        {
            Arena.Bounds = new ArenaBoundsRect(26, 20);
        }
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            Arena.Bounds = new ArenaBoundsRect(20, 20);
            _aoes.Clear();
        }
    }
}

sealed class FlatlinerKnockUp(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID._Weaponskill_FlatlinerKnockup, 15, true);

sealed class MajesticMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MajesticMeteor1, 6f);
sealed class MajesticMeteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MajesticMeteorain, new AOEShapeRect(60f, 5f));
sealed class MammothMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Spell_MammothMeteor, new AOEShapeCircle(22));
sealed class FireAndFury1(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_FireAndFuryCone1, new AOEShapeCone(60f, 45.Degrees()));
class FireAndFury2(BossModule module) : Components.SimpleAOEs(module, (uint)AID._Weaponskill_FireAndFuryCone2, new AOEShapeCone(60f, 45.Degrees()));

/**
 * We create the explosion towers with GenericTowers so we can access the constructor fields for ExplosionKnockUp
 */
sealed class ExplosionTower(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosionKnockUp)
        {
            Towers.Add(new(spell.LocXZ, 4f, 1, 8));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ExplosionKnockUp)
        {
            Towers.Clear();
        }
    }
}

sealed class ExplosionKnockUp(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly ExplosionTower _tower = module.FindComponent<ExplosionTower>()!;
    private Knockback[] _kbs = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kbs;

    public override void Update()
    {
        var towers = CollectionsMarshal.AsSpan(_tower.Towers);
        if (towers.Length == 0)
        {
            _kbs = [];
            return;
        }
        _kbs = new Knockback[4];
        for (var i = 0; i < towers.Length; ++i)
        {
            ref var t = ref towers[i];
            _kbs[i] = new(t.Position, 25f, t.Activation, circle);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var towers = CollectionsMarshal.AsSpan(_tower.Towers);
        var len = towers.Length;
        if (len == 0)
        {
            return;
        }
        ref var t0 = ref towers[0];
        if ((t0.Activation - WorldState.CurrentTime).TotalSeconds < 6d)
        {
            for (var i = 0; i < len; ++i)
            {
                ref var t = ref towers[i];
                if (t.IsInside(actor))
                {
                    hints.ActionsToExecute.Push(ActionDefinitions.Armslength, actor, ActionQueue.Priority.High);
                    hints.ActionsToExecute.Push(ActionDefinitions.Surecast, actor, ActionQueue.Priority.High);
                    return;
                }
            }
        }
    }
}


// TODO: doesn't identify which platform will be destroyed in order to jump away.
class ArcadionAvalanche(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40f, 20f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var color = Colors.AOE;


        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 0)
            {
                aoe.Color = color;
                aoe.Risky = true;
            }
        }
        return aoes;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcadionAvalanche)
        {
            _aoes.Add(new(rect, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrownOfArcadia)
        {
            _aoes.Clear();
        }
    }
}

// TODO: the platform that gets smashed should be marked dangerous. Then need to show where the aoe will hit on surviving platform.
sealed class ArcadionAvalancheToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcadionAvalancheToss, new AOEShapeRect(40f, 20f));
sealed class HeartbreakKick(BossModule module) : Components.CastTowers(module, (uint)AID._Weaponskill_HeartbreakKick, 4, 1, 8, damageType: AIHints.PredictedDamageType.Shared)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Towers.Add(new(spell.LocXZ, Radius, MinSoakers, MaxSoakers, activation: Module.CastFinishAt(spell)));
        if ((uint)(AID)spell.Action.ID is (uint)AID._Spell_MajesticMeteor)
        {
            var pos = spell.LocXZ;
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    { }
}

sealed class GreatWallOfFire(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.WallOfFireTankbuster, (uint)AID._Weaponskill_GreatWallOfFire, new AOEShapeRect(60, 3f));

// =========================
// Module
// =========================


[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(M11TheTyrantStates),
    ConfigType = null, // replace null with typeof(TheTyrantConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Boss,
    Contributors = "VeraNala, wen, Topas",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Raid,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1072u,
    NameID = 14305u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class M11NTheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
