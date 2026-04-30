namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "为工具技能提供冷却规划支持。\n注意：这不是循环预设！所有工具模块仅用于冷却规划。", "规划器工具", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "W.Glide", 20)
            .AddOption(DashStrategy.None, "No use.")
            .AddOption(DashStrategy.GapClose, "超出近战距离时作为突进使用", 60, 0, ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapCloseHold1, "超出近战距离时作为突进使用；保留 1 层供手动使用", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var dash = strategy.Option(Track.WingedGlide);
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRG.AID.WingedGlide)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3 and <= 20 && cd <= 60.5f,
            DashStrategy.GapCloseHold1 => distance is > 3 and <= 20 && cd < 0.6f,
            _ => true,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
