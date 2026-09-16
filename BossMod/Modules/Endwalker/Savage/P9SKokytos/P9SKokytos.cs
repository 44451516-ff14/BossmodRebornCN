namespace BossMod.Endwalker.Savage.P9SKokytos;

sealed class GluttonysAugur(BossModule module) : Components.CastCounter(module, (uint)AID.GluttonysAugurAOE);
sealed class SoulSurge(BossModule module) : Components.CastCounter(module, (uint)AID.SoulSurge);
sealed class BeastlyFury(BossModule module) : Components.CastCounter(module, (uint)AID.BeastlyFuryAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 937u, NameID = 12369u, PlanLevel = 90)]
public sealed class P9SKokytos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    public static Circle[] GetDefaultCircle() => [new(new(100f, 100f), 20f)];
}
