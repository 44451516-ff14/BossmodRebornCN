namespace BossMod.Autorotation;

public sealed class ClassNINUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ShadeShift = SharedTrack.Count, Shukuchi }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(NIN.AID.Chimatsuri);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: NIN", "为工具技能提供冷却规划支持。\n注意：这不是循环预设！所有工具模块仅用于冷却规划。", "规划器工具", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.NIN), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ShadeShift, "Shade", "", 400, NIN.AID.ShadeShift, 20);

        res.Define(Track.Shukuchi).As<DashStrategy>("Shukuchi", "冲刺", 20)
            .AddOption(DashStrategy.None, "No use.")
            .AddOption(DashStrategy.GapClose, "超出近战距离时作为突进使用", 60, 0, ActionTargets.Area, 45)
            .AddOption(DashStrategy.GapCloseHold1, "超出近战距离时作为突进使用；保留 1 层供手动使用", 60, 0, ActionTargets.Area, 74)
            .AddAssociatedActions(NIN.AID.Shukuchi);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.ShadeShift), NIN.AID.ShadeShift, Player);

        // TODO: revise, this doesn't look correct (shukuchi is area targeted, so it should use that; probably should expose options to use regardless of melee distance...)
        var dash = strategy.Option(Track.Shukuchi);
        var dashStrategy = strategy.Option(Track.Shukuchi).As<DashStrategy>();
        var distance = Player.DistanceToPoint(ResolveTargetLocation(dash.Value));
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(NIN.AID.Shukuchi)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && cd < 0.6f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(NIN.AID.Shukuchi), null, dash.Priority(), dash.Value.ExpireIn, targetPos: ResolveTargetLocation(dash.Value).ToVec3(Player.PosRot.Y));
    }
}
