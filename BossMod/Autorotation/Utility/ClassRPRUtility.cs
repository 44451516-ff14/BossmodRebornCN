namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", "为工具技能提供冷却规划支持。\n注意：这不是循环预设！所有工具模块仅用于冷却规划。", "规划器工具", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.RPR), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ArcaneCrest, "Crest", "", 600, RPR.AID.ArcaneCrest, 5);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);
    }
}
