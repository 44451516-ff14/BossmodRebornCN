namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class FlarePlay(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlarePlay);

sealed class Vidohunir(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.Vidohunir1, 5f);

sealed class EmpiricalResearch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmpiricalResearch1, new AOEShapeRect(80f, 6f));

sealed class SuperiorStoneIITelegraph(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SuperiorStoneII1, new AOEShapeRect(21f, 6.5f));

sealed class SuperiorStoneIIArena(BossModule module) : BossComponent(module)
{
    public sealed class Cutoff(WPos wpos, Angle angle, Vector4 posrot)
    {
        public WPos Position = wpos;
        public Angle Rotation = angle;
        public Vector4 PosRot = posrot;
    }

    private readonly List<Cutoff> cutoffs = [];

    private readonly Rectangle _basearena = new(new(0f, -720f), 24f, 30f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuperiorStoneII1)
        {
            cutoffs.Add(new(caster.Position, caster.Rotation, caster.PosRot));
            //Service.Log($"Adding cutoff {caster.Position}, {caster.Rotation}");
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuperiorStoneII)
        {
            var _rects = new Rectangle[cutoffs.Count];
            for (var i = 0; i < cutoffs.Count; i++)
            {
                var pos = cutoffs[i].Position + cutoffs[i].Rotation.ToDirection() * 10.5f;
                _rects[i] = new Rectangle(pos, 6.1f, 10.5f, cutoffs[i].Rotation); //This functions but could probably be more accurate.
                //Service.Log($"Adding Rectangle {_rects[i]}");
            }

            var newbounds = new ArenaBoundsCustom([_basearena], _rects);
            Arena.Bounds = newbounds;
        }
        if (spell.Action.ID == (uint)AID.GroundbreakingQuake1)
        {
            Arena.Bounds = new ArenaBoundsSquare(24f);
            cutoffs.Clear();
        }
    }
}

sealed class GroundBreakingQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundbreakingQuake1, new AOEShapeRect(30f, 6f));

sealed class CircumscribedFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CircumscribedFire, (uint)AID.CircumscribedFire1], new AOEShapeDonut(6f, 70f));

sealed class LocalizedBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LocalizedBlizzard, new AOEShapeCircle(10f));

sealed class ThunderAndError(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ThunderAndError, 5f);

sealed class SmallSpecimen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmallSpecimen, new AOEShapeCircle(6f));

sealed class LargeSpecimen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LargeSpecimen, new AOEShapeCircle(13f));

sealed class StardustSpecimen(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.StardustSpecimen, 6f);

sealed class Shockwave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Shockwave);

sealed class FallingRubble(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble, new AOEShapeRect(35f, 5f));

sealed class FallingRubble1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble1, new AOEShapeCircle(12f));

sealed class FallingRubble2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble2, new AOEShapeCircle(8f));

sealed class FallingRubble3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRubble3, new AOEShapeRect(25f, 3f));

sealed class FinalExam(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FinalExam1, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, HerStolenLight", PrimaryActorOID = (uint)OID.ShantottoTheDemon, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14778u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class A30Shantoto(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(24f, 30f))
{
    public static readonly WPos ArenaCenter = new(0f, -720f);
}
