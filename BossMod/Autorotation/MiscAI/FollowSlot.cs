using BossMod.AI;

namespace BossMod.Autorotation.MiscAI;

public sealed class FollowSlot(RotationModuleManager manager, Actor player) : TypedRotationModule<FollowSlot.Strategy>(manager, player)
{
    readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();

    public enum Flag { Disabled, Enabled }

    public struct Strategy
    {
        [Number(DisplayName = "Slot", MinValue = -1, MaxValue = 7)]
        public Track<long> Master;
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Follow party slot", "", "AI", "xan", RotationModuleQuality.Basic, new(~0ul), 1000);

        return def.WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        // fallback for users not using autorot (this module is meant to replace legacy ai)
        if (Hints.GoalZones.Count == 0 && primaryTarget is { IsAlly: false })
        {
            var effectiveRange = Player.Role is Role.Melee or Role.Tank ? 3f : 25f;
            Hints.GoalZones.Add(AIHints.GoalSingleTarget(primaryTarget, effectiveRange));
        }

        var masterSlot = strategy.Master;
        var master = masterSlot > 0 ? World.Party[(int)masterSlot.Value] : null;
        if (master != null)
        {
            if (_aiConfig.FocusTargetMaster)
                Hints.ForcedFocusTarget = master;

            if (Bossmods.ActiveModule == null || _aiConfig.FollowDuringActiveBossModule)
                Hints.GoalZones.Add(AIHints.GoalSingleTarget(master, _aiConfig.MaxDistanceToSlot, 0.2f));
        }
    }
}
