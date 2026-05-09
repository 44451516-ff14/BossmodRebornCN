#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Dawntrail.Alliance.A35Promathia;

public enum OID : uint
{
    Boss = 0x4DEE, // R8.000, x1
    Helper = 0x233C, // R0.500, x30 (spawn during fight), Helper type
    _Gen_LinkOfPromathia = 0x4DEF, // R2.240, x10
    _Gen_EmptyThinker = 0x4DF3, // R2.300, x0 (spawn during fight)
    _Gen_EmptyWeeper = 0x4DF2, // R2.000, x0 (spawn during fight)
    _Gen_EmptyWanderer = 0x4DF1, // R1.200, x0 (spawn during fight)
    _Gen_MemoryReceptacle = 0x4DF0, // R2.040, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 45308, // Boss->player, no cast, single-target
    _Ability_ = 50342, // Boss->location, no cast, single-target
    _Ability_EmptySalvation = 50317, // Boss->location, 5.0s cast, range 25 circle
    _Ability_FleetingEternity = 50318, // Boss->self, 3.5+1.0s cast, single-target
    _Ability_Explosion = 50320, // Helper->self, 5.0s cast, range 16 circle
    _Ability_WheelOfImpregnability = 50321, // Boss->self, 2.0+1.0s cast, single-target
    _Ability_WheelOfImpregnability1 = 50323, // Helper->self, no cast, range 13 circle
    _Ability_BastionOfTwilight = 50322, // Boss->self, 2.0+1.0s cast, single-target
    _Ability_PestilentPenance = 50330, // Boss->self, 6.4+0.6s cast, single-target
    _Ability_BastionOfTwilight1 = 50324, // Helper->self, no cast, range 13-50 donut
    _Ability_PestilentPenance1 = 50331, // Helper->self, 7.0s cast, range 50 width 50 rect
    _Ability_FleetingEternity1 = 50319, // Boss->self, 3.5+1.0s cast, single-target
    _Spell_Comet = 50337, // Boss->self, 4.5+0.5s cast, single-target
    _Spell_Comet1 = 50338, // Helper->players, 0.5s cast, range 6 circle
    _Ability_1 = 50345, // Helper->4D66, 1.0s cast, single-target
    _Ability_FalseGenesis = 50343, // Boss->self, 9.5+0.5s cast, single-target
    _Ability_FalseGenesis1 = 50344, // Helper->self, 0.5s cast, range 25 circle
    _Ability_WindsOfPromyvion = 50352, // 4DF3->self, 3.9+0.6s cast, single-target
    _Ability_WindsOfPromyvion1 = 50353, // Helper->self, 4.5s cast, range 16 width 6 rect
    _Ability_WindsOfPromyvion2 = 50460, // 4DF3->self, no cast, single-target
    _Ability_WindsOfPromyvion3 = 50354, // Helper->self, 0.6s cast, range 16 width 6 rect
    _Ability_EmptyBeleaguer = 50351, // 4DF1->self, 6.0s cast, range 6 circle
    _Ability_AuroralDrape = 50355, // 4DF2->self, 7.0s cast, range 7 width 7 rect
    _Ability_EmptySeed = 50349, // 4DF0->self, 5.0s cast, range 10 circle
    _Ability_DeadlyRebirth = 50346, // Boss->self, 5.7+1.3s cast, single-target
    _Ability_DeadlyRebirth1 = 50348, // Helper->self, 1.3s cast, range 50 circle, adds phase enrage
    _Ability_DeadlyRebirth2 = 50694, // Boss->self, 8.0+2.0s cast, single-target
    _Ability_DeadlyRebirth3 = 50347, // Helper->self, 2.0s cast, range 50 circle, regular raidwide
    _Ability_EarthboundHeaven = 50333, // Boss->self, 2.0+1.0s cast, single-target
    _Ability_MalevolentBlessing = 50327, // Boss->self, 5.7+0.8s cast, single-target
    _Ability_MalevolentBlessing1 = 50328, // Helper->self, 6.5s cast, range 40 23-degree cone
    _Ability_MalevolentBlessing2 = 50329, // Helper->self, 6.5s cast, range 50 width 50 rect
    _Ability_PestilentPenance2 = 50332, // 4DEF->self, 7.5s cast, range 50 width 5 rect
    _Ability_InfernalDeliverance = 50334, // Boss->self, 5.5+1.5s cast, single-target
    _Ability_InfernalDeliverance1 = 50335, // Helper->self, 7.0s cast, range 4 circle
    _Ability_InfernalDeliverance2 = 50565, // Helper->self, 5.0s cast, range 8 circle
    _Spell_Meteor = 50339, // Boss->self, 4.5+0.5s cast, single-target
    _Spell_Meteor1 = 50340, // Helper->players, 0.5s cast, range 6 circle
    _Spell_Meteor2 = 50341, // Helper->player, 0.5s cast, range 6 circle
    _Ability_MalevolentBlessing3 = 50326, // Boss->self, 5.7+0.8s cast, single-target
}

public enum SID : uint
{
    _Gen_ = 2552, // none->Boss, extra=0x48D/0x458/0x457
    _Gen_Heavy = 1796, // none->player, extra=0x32
    _Gen_VulnerabilityUp = 1789, // Helper/4DF2/4DF1/4DEF->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_1 = 2056, // none->4DF2, extra=0x498
    _Gen_SystemLock = 2578, // none->player, extra=0x0
    _Gen_Invincibility = 1570, // none->player, extra=0x0
    _Gen_2 = 2160, // none->4D66, extra=0x3931
    _Gen_3 = 2273, // Boss->Boss, extra=0x226
    _Gen_DownForTheCount = 3908, // Helper->player/4D66, extra=0xEC7
}

public enum IconID : uint
{
    _Gen_Icon_m1001_lockon_c0w = 687, // Boss->self
    _Gen_Icon_m1001_lockon_c1w = 688, // Boss->self
    _Gen_Icon_tank_lockonae_6m_5s_01t = 344, // player->self
    _Gen_Icon_m1001_turning_right01w = 689, // 4DF3->self
    _Gen_Icon_m1001_turning_left01w = 690, // 4DF3->self
    _Gen_Icon_loc06sp_05ak1 = 466, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chm_m1001_01w = 427, // Boss->4D66
    _Gen_Tether_chn_nomal01f = 12, // 4DF0->4D66
}

class EmptySalvation(BossModule module) : Components.RaidwideCast(module, AID._Ability_EmptySalvation);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID._Ability_Explosion, 16, maxCasts: 4, highlightImminent: true);
// 58.45 -> 48.59
class WheelOfImpregnability(BossModule module) : Components.GenericAOEs(module, AID._Ability_WheelOfImpregnability1)
{
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(13), Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_m1001_lockon_c0w)
            _activation = WorldState.FutureTime(9.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}
class BastionOfTwilight(BossModule module) : Components.GenericAOEs(module, AID._Ability_BastionOfTwilight1)
{
    DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeDonut(13, 50), Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_m1001_lockon_c1w)
            _activation = WorldState.FutureTime(9.9f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}
class PestilentPenance(BossModule module) : Components.StandardAOEs(module, AID._Ability_PestilentPenance, new AOEShapeRect(50, 25));
class Comet(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID._Gen_Icon_tank_lockonae_6m_5s_01t, AID._Spell_Comet1, centerAtTarget: true)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Spell_Comet1 or AID._Spell_Meteor1)
            CurrentBaits.Clear();
    }
}

class FalseGenesis(BossModule module) : BossComponent(module)
{
    DateTime _activation;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation == default)
            return;

        Arena.ZoneRect(Arena.Center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
        Arena.ZoneRect(Arena.Center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
        Arena.ZoneRect(Arena.Center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        var pos = actor.Position;
        if (!(pos.InRect(Arena.Center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f) || pos.InRect(Arena.Center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f) || pos.InRect(Arena.Center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f)))
            hints.Add("Go to platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;

        var center = Arena.Center;
        hints.AddForbiddenZone(p => !(p.InRect(center + new WDir(0, 13), default(Angle), 6.5f, 6.5f, 6.5f) || p.InRect(center + new WDir(0, 13).Rotate(120.Degrees()), 120.Degrees(), 6.5f, 6.5f, 6.5f) || p.InRect(center + new WDir(0, 13).Rotate(-120.Degrees()), -120.Degrees(), 6.5f, 6.5f, 6.5f)), _activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00200010)
            _activation = WorldState.FutureTime(10.7f);

        if (index == 0x0C && state == 0x00020001)
        {
            _activation = default;
            WDir[] corner = [
                new(3.5f, -6.5f),
                new(3.5f, -6),
                new(6, -6),
                new(6, -3.5f),
                new(6.5f, -3.5f),
            ];
            List<WDir> plat = [.. corner, .. corner.Select(c => c.OrthoR()), .. corner.Select(c => -c), .. corner.Select(c => c.OrthoL())];
            var platform = plat.Select(p => p + new WDir(0, 13));
            var combined = Arena.Bounds.Clipper.UnionAll(new(platform), [new(platform.Select(p => p.Rotate(120.Degrees()))), new(platform.Select(p => p.Rotate(-120.Degrees())))]);
            Arena.Bounds = new ArenaBoundsCustom(20, combined);
        }

        if (index == 0x0C && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(25);
    }
}

class MemoryReceptacle(BossModule module) : Components.Adds(module, (uint)OID._Gen_MemoryReceptacle);
class AuroralDrape(BossModule module) : Components.StandardAOEs(module, AID._Ability_AuroralDrape, new AOEShapeRect(7, 3.5f));
class EmptyBeleaguer(BossModule module) : Components.StandardAOEs(module, AID._Ability_EmptyBeleaguer, 6, 2);
// winds: 4 casts, rotates 30 degrees between each
class WindsOfPromyvion(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_m1001_turning_right01w:
                Sequences.Add(new(new AOEShapeRect(16, 3), actor.Position, actor.Rotation, -30.Degrees(), WorldState.FutureTime(4.5f), 1.4f, 4));
                break;
            case IconID._Gen_Icon_m1001_turning_left01w:
                Sequences.Add(new(new AOEShapeRect(16, 3), actor.Position, actor.Rotation, 30.Degrees(), WorldState.FutureTime(4.5f), 1.4f, 4));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_WindsOfPromyvion1 or AID._Ability_WindsOfPromyvion3)
        {
            NumCasts++;
            if (Sequences.Count > 0)
                AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}

class EmptySeed(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Ability_EmptySeed, 10, shape: new AOEShapeCircle(10))
{
    // 29-ish degrees
    static readonly float SafeConeWidth = MathF.Atan2(6, 3.5f) - FortyFiveRad;
    const float FortyFiveRad = MathF.PI / 4f;

    Angle PlatformOrientation(WPos p)
    {
        if (p.Z > Arena.Center.Z)
            return default;
        if (p.X < Arena.Center.X)
            return -120.Degrees();
        return 120.Degrees();
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in base.Sources(slot, actor))
        {
            if (IsImmune(slot, src.Activation))
                continue;

            if (!actor.Position.InCircle(src.Origin, 10))
                continue;

            var orient = PlatformOrientation(src.Origin);
            var dir = ((actor.Position - src.Origin).ToAngle() - orient).Normalized().Rad;
            if (dir < 0)
                dir += MathF.PI * 2;

            var qu = dir % (MathF.PI / 2f);

            if (MathF.Abs(qu - FortyFiveRad) < SafeConeWidth)
            {
                var intersect = Arena.Bounds.IntersectRay(actor.Position - Arena.Center, (actor.Position - src.Origin).Normalized());
                yield return src with { Distance = MathF.Min(intersect - 0.1f, 10) };
            }
            else
                yield return src;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // already ensured that the only sources returned are the ones that will actually hit us
        foreach (var src in Sources(slot, actor))
        {
            var center = src.Origin;
            var orient = PlatformOrientation(center);
            hints.AddForbiddenZone(p =>
            {
                if (!p.InCircle(center, 10))
                    return false;
                var dir = ((p - center).ToAngle() - orient).Normalized().Rad;
                if (dir < 0)
                    dir += MathF.PI * 2;
                var qu = dir % (MathF.PI / 2f);
                return MathF.Abs(qu - FortyFiveRad) >= SafeConeWidth;
            }, src.Activation);
        }
    }
}

class MalevolentBlessingCone(BossModule module) : Components.StandardAOEs(module, AID._Ability_MalevolentBlessing1, new AOEShapeCone(40, 11.5f.Degrees()));
class MalevolentBlessingSide(BossModule module) : Components.StandardAOEs(module, AID._Ability_MalevolentBlessing2, new AOEShapeRect(50, 25));
class PestilentPenanceSkinny(BossModule module) : Components.StandardAOEs(module, AID._Ability_PestilentPenance2, new AOEShapeRect(50, 2.5f));
class InfernalDeliveranceTower(BossModule module) : Components.CastTowers(module, AID._Ability_InfernalDeliverance1, 4, 6, 8);
class InfernalDeliverancePuddle(BossModule module) : Components.StandardAOEs(module, AID._Ability_InfernalDeliverance2, 8);
class MeteorSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID._Gen_Icon_loc06sp_05ak1, AID._Spell_Meteor2, 6, 5.1f);

class A35PromathiaStates : StateMachineBuilder
{
    public A35PromathiaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<EmptySalvation>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<WheelOfImpregnability>()
            .ActivateOnEnter<BastionOfTwilight>()
            .ActivateOnEnter<PestilentPenance>()
            .ActivateOnEnter<PestilentPenanceSkinny>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<FalseGenesis>()
            .ActivateOnEnter<MemoryReceptacle>()
            .ActivateOnEnter<AuroralDrape>()
            .ActivateOnEnter<EmptyBeleaguer>()
            .ActivateOnEnter<WindsOfPromyvion>()
            .ActivateOnEnter<EmptySeed>()
            .ActivateOnEnter<MalevolentBlessingCone>()
            .ActivateOnEnter<MalevolentBlessingSide>()
            .ActivateOnEnter<InfernalDeliveranceTower>()
            .ActivateOnEnter<InfernalDeliverancePuddle>()
            .ActivateOnEnter<MeteorSpread>();
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    //private void XXX(uint id, float delay)
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14779, DevOnly = true)]
public class A35Promathia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-820, -820), new ArenaBoundsCircle(25));

