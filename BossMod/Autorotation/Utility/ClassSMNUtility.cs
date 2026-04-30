namespace BossMod.Autorotation;

public sealed class ClassSMNUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { RadiantAegis = SharedTrack.Count }
    public enum AegisStrategy { None, Use }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SMN.AID.Teraflare);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SMN", "为工具技能提供冷却规划支持。\n注意：这不是循环预设！所有工具模块仅用于冷却规划。", "规划器工具", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.SMN), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.RadiantAegis).As<AegisStrategy>("Radiant Aegis", "Aegis", 20)
            .AddOption(AegisStrategy.None, "不使用")
            .AddOption(AegisStrategy.Use, "Use Radiant Aegis", 60, 30, ActionTargets.Self, 2);

        //TODO: Rekindle here or inside own module?

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var radi = strategy.Option(Track.RadiantAegis);
        var hasAegis = StatusDetails(Player, SMN.SID.RadiantAegis, Player.InstanceID, 30).Left > 0.1f;
        if (radi.As<AegisStrategy>() != AegisStrategy.None && !hasAegis)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SMN.AID.RadiantAegis), Player, radi.Priority(), radi.Value.ExpireIn);
    }
}
