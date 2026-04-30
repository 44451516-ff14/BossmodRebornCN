namespace BossMod.Autorotation;

public sealed class ClassRDMUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { MagickBarrier = SharedTrack.Count }
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RDM.AID.VermilionScourge);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RDM", "为工具技能提供冷却规划支持。\n注意：这不是循环预设！所有工具模块仅用于冷却规划。", "规划器工具", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.RDM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.MagickBarrier, "Magick Barrier", "Barrier", 600, RDM.AID.MagickBarrier, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.MagickBarrier), RDM.AID.MagickBarrier, Player);
    }
}
