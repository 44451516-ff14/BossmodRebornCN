namespace BossMod.Endwalker.VariantCriterion.V01SS.V012Silkie;

class CarpetBeater(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CarpetBeater);
class TotalWash(BossModule module) : Components.RaidwideCast(module, (uint)AID.TotalWash);
class DustBlusterKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DustBlusterKnockback, 16, shape: new AOEShapeCircle(60));
class WashOutKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WashOutKnockback, 35, shape: new AOEShapeRect(60, 60), kind: Kind.DirForward);

class BracingDuster1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BracingDuster1, new AOEShapeDonut(5, 60));
class BracingDuster2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BracingDuster2, new AOEShapeDonut(5, 60));

class ChillingDuster1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChillingDuster1, new AOEShapeCross(60, 5));
class ChillingDuster2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChillingDuster2, new AOEShapeCross(60, 5));
class ChillingDuster3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChillingDuster3, new AOEShapeCross(60, 5));

class SlipperySoap(BossModule module) : Components.ChargeAOEs(module, (uint)AID.SlipperySoap, 5);
class SpotRemover2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpotRemover2, 5);

class PuffAndTumble1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PuffAndTumble1, 4);
class PuffAndTumble2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PuffAndTumble2, 4);

class SqueakyCleanAOE1E(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE1E, new AOEShapeCone(60, 45.Degrees()));
class SqueakyCleanAOE2E(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE2E, new AOEShapeCone(60, 45.Degrees()));
class SqueakyCleanAOE3E(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE3E, new AOEShapeCone(60, 112.5f.Degrees()));

class SqueakyCleanAOE1W(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE1W, new AOEShapeCone(60, 45.Degrees()));
class SqueakyCleanAOE2W(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE2W, new AOEShapeCone(60, 45.Degrees()));
class SqueakyCleanAOE3W(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SqueakyCleanAOE3W, new AOEShapeCone(60, 112.5f.Degrees()));

class EasternEwers(BossModule module) : Components.Exaflare(module, 4f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BrimOver)
        {
            Lines.Add(new() { Next = caster.Position, Advance = new(default, 5.1f), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 11, MaxShownExplosions = int.MaxValue });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BrimOver or (uint)AID.Rinse)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11369)]
public class V012Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-335, -155), new ArenaBoundsSquare(20));
