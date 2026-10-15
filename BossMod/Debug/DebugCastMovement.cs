using BossMod.AI;
using BossMod.Autorotation.MiscAI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

// diagnostics for interactions between BMR movement and casts started by us or an external rotation (RSR):
// which movement system is active, what each side believes the cast state is, what we report over ipc and how recent casts ended
// also hosts the fake aoe test that forces cast-interrupting dodges to check that no cast/interrupt loop forms
sealed class DebugCastMovement(WorldState ws, AIHints hints, ActionManagerEx amex, MovementOverride move, RotationSolverRebornModule rsr)
{
    public void Draw()
    {
        DrawState();
        ImGui.Separator();
        DrawFakeAoe();
        ImGui.Separator();
        DrawHistory();
    }

    private void DrawState()
    {
        var aiActive = AIManager.Instance?.Beh != null;
        var autorotMovement = NormalMovement.Instance != null;
        var system = aiActive ? "自动移动（AI）" : autorotMovement ? "自动循环（自动移动模块）" : "无";
        ImGui.TextUnformatted($"移动系统：{system}{(aiActive && autorotMovement ? "（自动移动模块已加载，但 AI 移动启用时不会运行）" : "")}");

        var rsrInstalled = rsr.IsInstalled;
        ImGui.TextUnformatted($"RSR：已安装={rsrInstalled}，自动循环运行中={rsrInstalled && rsr.IsAutorotationActive}");
        ImGui.TextUnformatted($"施法时禁止移动：配置={ActionManagerEx.Config.PreventMovingWhileCasting}，当前生效={amex.PreventMovingWhileCasting}，此刻已拦截输入={move.MovementBlocked}");

        // live client state vs worldstate: the worldstate only shows our own casts once the server has confirmed them
        var castRemaining = amex.CastTimeRemaining;
        ImGui.TextUnformatted($"客户端施法状态：{(castRemaining > 0f ? $"{amex.CastAction}，剩余 {castRemaining:f2} 秒" : "无施法")}，移动可能打断施法={amex.MoveMightInterruptCast}");
        var player = ws.Party.Player();
        var cast = player?.CastInfo;
        ImGui.TextUnformatted($"世界状态施法信息：{(cast != null ? $"{cast.Action}，剩余 {cast.RemainingTime:f2} 秒，效果已生效={cast.EventHappened}" : "无施法")}");

        var forced = hints.ForcedMovement;
        ImGui.TextUnformatted($"提示状态：强制移动={(forced != null ? $"{forced.Value.Length():f2}y" : "无")}，暂缓施法={hints.HoldCasts}，最长施法时间={Seconds(hints.MaxCastTime)}，强制取消机制/其他施法={hints.ForceCancelCastMechanic}/{hints.ForceCancelCastOther}");
        ImGui.TextUnformatted($"IPC Movement.IsMoving（RSR 接收到的状态）：{forced != null || hints.HoldCasts}");

        if (AIManager.Instance is { } ai)
        {
            var ctrl = ai.Controller;
            var dest = ctrl.NaviTargetPos is { } pos && player != null ? $"距离 {(pos - player.Position).Length():f2}y" : "无";
            ImGui.TextUnformatted($"AI 自动移动：目标={dest}，宽限时间={Seconds(ctrl.NaviLeewaySeconds)}，允许移动打断施法={ctrl.AllowInterruptingCastByMovement}，强制取消机制/其他施法={ctrl.ForceCancelCastMechanicAI}/{ctrl.ForceCancelCastOtherAI}");
        }

        ImGui.TextUnformatted($"输入：玩家={Dir(move.UserMove)}，实际={Dir(move.ActualMove)}，目标方向={(move.DesiredDirection is { } d ? Utils.Vec3String(d) : "无")}");
    }

    private void DrawFakeAoe()
    {
        ImGui.TextWrapped("假想 AOE：在玩家脚下反复生成圆形范围，迫使当前移动系统在没有宽限时间的情况下躲避。使用 RSR 施法时，每次范围最多打断一次施法，之后应能使用瞬发技能，不应连续打断。");

        var enabled = FakeAoeTest.Enabled;
        if (ImGui.Checkbox("在玩家脚下启用假想 AOE", ref enabled))
        {
            FakeAoeTest.Enabled = enabled;
            FakeAoeTest.Reset();
        }
        ImGui.DragFloat("半径##fakeaoe", ref FakeAoeTest.Radius, 0.1f, 1f, 15f, "%.1f y");
        ImGui.DragFloat("生效延迟##fakeaoe", ref FakeAoeTest.ActivationDelay, 0.05f, 0f, 10f, "%.2f 秒");
        ImGui.DragFloat("重新生成间隔##fakeaoe", ref FakeAoeTest.Interval, 0.1f, 0.5f, 30f, "%.1f 秒");

        if (FakeAoeTest.Enabled)
        {
            var now = ws.CurrentTime;
            var toActivation = (FakeAoeTest.Activation - now).TotalSeconds;
            ImGui.TextUnformatted($"生成次数：{FakeAoeTest.Drops}，当前位置：{Pos(FakeAoeTest.Center)}，{(toActivation > 0d ? $"{toActivation:f2} 秒后生效" : "已生效")}，下次生成：{(FakeAoeTest.NextDrop - now).TotalSeconds:f2} 秒后");
        }
    }

    private void DrawHistory()
    {
        var tracker = amex.CastTracker;
        var history = tracker.History;
        var count = history.Count;
        int interrupted = 0, streak = 0, longestStreak = 0;
        for (var i = 0; i < count; ++i)
        {
            if (history[i].Outcome == CastTracker.Outcome.Interrupted)
            {
                ++interrupted;
                longestStreak = Math.Max(longestStreak, ++streak);
            }
            else if (history[i].Outcome != CastTracker.Outcome.Casting)
            {
                streak = 0;
            }
        }
        // casts interrupted back to back are the signature of a cast/interrupt loop
        ImGui.TextUnformatted($"近期施法：{count} 次，打断：{interrupted} 次，最长连续打断：{longestStreak} 次{(longestStreak >= 3 ? "（可能陷入循环）" : "")}");
        ImGui.SameLine();
        if (ImGui.Button("清空##casthistory"))
        {
            tracker.Clear();
            return; // count above is stale now; the table is drawn again next frame
        }

        using var table = ImRaii.Table("cast_history", 9, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("技能");
        ImGui.TableSetupColumn("距今");
        ImGui.TableSetupColumn("持续时间");
        ImGui.TableSetupColumn("结果");
        ImGui.TableSetupColumn("剩余时间");
        ImGui.TableSetupColumn("自动移动帧数");
        ImGui.TableSetupColumn("手动移动帧数");
        ImGui.TableSetupColumn("位移");
        ImGui.TableSetupColumn("可能原因");
        ImGui.TableHeadersRow();

        var now = DateTime.Now;
        for (var i = count - 1; i >= 0; --i) // newest first
        {
            var e = history[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Action.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{(now - e.Start).TotalSeconds:f1}s");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{e.Duration:f2}s");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Outcome switch
            {
                CastTracker.Outcome.Casting => "施法中",
                CastTracker.Outcome.Completed => "完成",
                CastTracker.Outcome.Interrupted => "被打断",
                CastTracker.Outcome.NoEffect => "无效果",
                _ => "未知"
            });
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.Outcome == CastTracker.Outcome.Interrupted ? $"{e.RemainingAtEnd:f2}s" : "");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.AutoMoveFrames.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.UserMoveFrames.ToString());
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{e.Drift:f2}y");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(e.LikelyCause);
        }
    }

    private static string Seconds(float s) => s >= 1e6f ? "无限" : $"{s:f2} 秒";
    private static string Dir(WDir d) => d == default ? "无" : $"({d.X:f2}, {d.Z:f2})";
    private static string Pos(WPos p) => $"({p.X:f1}, {p.Z:f1})";
}
