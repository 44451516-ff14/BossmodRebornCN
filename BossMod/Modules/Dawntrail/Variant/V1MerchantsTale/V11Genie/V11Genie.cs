namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V11Genie;

sealed class FabulousFirecrackersBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FabulousFirecrackersBig, new AOEShapeCone(30f, 90f.Degrees()));
sealed class FabulousFirecrackersSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FabulousFirecrackersSmall, new AOEShapeCone(30f, 45f.Degrees()));
sealed class ParadeOfWonders(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParadeOfWonders);
sealed class SpectacularSparks(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpectacularSparks, new AOEShapeRect(36f, 3f), 3);
sealed class ExplosiveEnding(BossModule module) : Components.RaidwideCast(module, (uint)AID.ExplosiveEnding);
sealed class FanningFlame(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FanningFlame1, (uint)AID.FanningFlame2], new AOEShapeCone(30f, 22.5f.Degrees()), 4, 8);
sealed class SupernaturalSurprise(BossModule module) : Components.RaidwideCast(module, (uint)AID.SupernaturalSurprise);
sealed class LampLighting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LampLighting, new AOEShapeRect(60f, 4f));
sealed class RubBurn(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RubBurn);
sealed class Pyromagicks(BossModule module) : Components.SimpleExaflare(module, 6f, (uint)AID.PyromagicksFirst, (uint)AID.PyromagicksRest, 8f, 2.3d, 5, 2, true);
sealed class RainbowRoad(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RainbowRoad1, (uint)AID.RainbowRoad2], 15f, 2, 4);
sealed class AetherialBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialBlizzard, new AOEShapeRect(36f, 2f));
sealed class LampOil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LampOil, 8f);
sealed class Voyage(BossModule module) : Components.GenericAOEs(module)
{
    // tracks spawn when boss starts casting A Sailor's Tale; initial track spawned not always the same?
    // left path
    // middle will always be full length; north/south can be short/medium
    // north/south decided by tether to lever during Chart Course
    // ships start slightly different times, based on status gain
    // EObjAnim _Gen_Actor1ebf6c_Tracks -> 00010002 = north short, south middle
    // EObjAnim _Gen_Actor1ebf6c_Tracks -> 00400020 = north middle, south short
    private readonly List<AOEInstance> _aoes = [];
    private bool _isNorthShort = false;
    private bool _isNorth = false;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_)
        {
            _aoes.Clear();
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID._Gen_Actor1ebf6c_Tracks)
        {
            if (state == 0x00010002)
            {
                _isNorthShort = true;
            }
            else if (state == 0x00400020)
            {
                _isNorthShort = false;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChartCourse)
        {
            _isNorth = source.Position.Z < Arena.Center.Z;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Voyage)
        {
            if (caster.Position.Z == Arena.Center.Z)
            {
                _aoes.Add(new(new AOEShapeRect(40f, 6f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
            }
            else
            {
                var crossOffsetShort = 3.78f;
                var crossOffsetMiddle = 17.743f;
                // north airship
                if (caster.Position.Z < Arena.Center.Z)
                {
                    if (_isNorth)
                    {
                        _aoes.Add(new(new AOEShapeRect(40f, 6f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
                    }
                    else
                    {
                        _aoes.Add(new(new AOEShapeRect(_isNorthShort ? 8f : 20f, 6f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
                        // unsure why Y offset so small for crossover tracks
                        _aoes.Add(new(new AOEShapeRect(22f, 6f), caster.Position + new WDir(_isNorthShort ? crossOffsetShort : crossOffsetMiddle, -1.757f), caster.Rotation - 45f.Degrees(), actorID: caster.InstanceID));
                    }
                }
                else
                {
                    if (_isNorth)
                    {
                        _aoes.Add(new(new AOEShapeRect(_isNorthShort ? 20f : 8f, 6f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
                        _aoes.Add(new(new AOEShapeRect(22f, 6f), caster.Position + new WDir(_isNorthShort ? crossOffsetMiddle : crossOffsetShort, 1.757f), caster.Rotation + 45f.Degrees(), actorID: caster.InstanceID));
                    }
                    else
                    {
                        _aoes.Add(new(new AOEShapeRect(40f, 6f), caster.Position, caster.Rotation, actorID: caster.InstanceID));
                    }
                }
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(V11GenieStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.GenieOfTheLamp,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14396u,
SortOrder = 2,
PlanLevel = 0)]
public class V11Genie(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.X < -700f ? ArenaCenter1 : ArenaCenter2, new ArenaBoundsSquare(17.5f))
{
    public static readonly WPos ArenaCenter1 = new(-750, -415);
    public static readonly WPos ArenaCenter2 = new(160f, 787f);
}
