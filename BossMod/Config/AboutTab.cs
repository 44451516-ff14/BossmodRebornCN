using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private static readonly Color TitleColor = Color.FromComponents(255u, 165u, default);
    private static readonly Color SectionBgColor = Color.FromComponents(38u, 38u, 38u);
    private static readonly Color BorderColor = Color.FromComponents(178u, 178u, 178u, 204u);
    private static readonly Color DiscordColor = Color.FromComponents(88u, 101u, 242u);

    private string _lastErrorMessage = "";

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);

        ImGui.TextUnformatted("BossModReborn（BMR）提供首领战雷达、自动循环、冷却规划及AI功能。所有模块均可单独开关。技术支持请通过本页底部Discord链接获取。");
        ImGui.TextUnformatted("此为原版BossMod（VBM）的分支版本，技术支持仅在Combat Reborn Discord提供。");
        ImGui.TextUnformatted("请勿同时加载VBM与本分支版本，可能导致的后果未经测试且不受支持。");
        ImGui.Spacing();
        DrawSection("雷达功能",
        [
            "提供包含区域小地图的屏幕窗口，实时显示玩家位置、首领位置、即将触发的范围攻击及其他机制。",
            "无需记忆技能名称即可直观理解机制。",
            "清晰判断自身是否处于范围攻击覆盖区域。",
            "可在\"支持的首领\"标签页查看已启用的首领模块。",
        ]);
        ImGui.Spacing();
        DrawSection("自动循环",
        [
            "尽最大可能执行最优技能循环。",
            "前往\"自动循环配置\"标签页创建预设方案。",
            "各职业模块的完成度可通过工具提示查看。",
            "使用指南详见项目Wiki。",
        ]);
        ImGui.Spacing();
        DrawSection("冷却规划",
        [
            "为特定首领战创建冷却技能使用计划。",
            "在特定战斗中替代自动循环功能。",
            "允许精确安排特定技能的使用时机。",
            "使用指南详见项目Wiki。",
        ]);
        ImGui.Spacing();
        DrawSection("AI系统",
        [
            "自动化首领战中的移动操作。",
            "根据雷达显示的安全区域自动调整角色位置。",
            "不建议在野队环境中使用。",
            "可被其他插件调用以实现全副本自动化。",
        ]);
        ImGui.Spacing();
        DrawSection("战斗回放",
        [
            "用于模块开发、问题分析及冷却规划。",
            "提交问题报告时请务必提供回放文件（注意包含玩家ID信息）。",
            "启用路径：设置 > 显示回放管理界面（或启用自动录制）。",
            $"文件存储路径：'{replayDir}'。",
        ]);
        ImGui.Spacing();
        ImGui.Spacing();

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            if (ImGui.Button("加入Combat Reborn Discord", new(220, 0)))
                _lastErrorMessage = OpenLink("https://discord.gg/p54TZMPnC9");
        ImGui.SameLine();
        if (ImGui.Button("访问BossModReborn GitHub", new(220, 0)))
            _lastErrorMessage = OpenLink("https://github.com/FFXIV-CombatReborn/BossmodReborn");
        ImGui.SameLine();
        if (ImGui.Button("查看BossMod Wiki", new(130, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        ImGui.SameLine();
        if (ImGui.Button("打开回放文件夹", new(180, 0)) && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);
        ImGui.SameLine();
        if (ImGui.Button("爱发电", new(180, 0)))
        {
            _lastErrorMessage = OpenLink("https://afdian.com/a/a_44451516");
        }
        
        if (_lastErrorMessage.Length > 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor3);
            ImGui.TextUnformatted(_lastErrorMessage);
        }
    }

    private static void DrawSection(string title, string[] bulletPoints)
    {
        using var colorBackground = ImRaii.PushColor(ImGuiCol.ChildBg, SectionBgColor.ABGR);
        using var colorBorder = ImRaii.PushColor(ImGuiCol.Border, BorderColor.ABGR);
        var height = ImGui.GetTextLineHeightWithSpacing() * (bulletPoints.Length + 2);
        using var section = ImRaii.Child(title, new(0, height), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

        if (!section)
            return;

        using (ImRaii.PushColor(ImGuiCol.Text, TitleColor.ABGR))
            ImGui.TextUnformatted(title);

        ImGui.Separator();
        ImGui.PushTextWrapPos();
        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(point);
        }
        ImGui.PopTextWrapPos();
    }

    private static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"链接'{link}'打开失败，请手动在浏览器中访问。";
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"目录'{dir}'不存在";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"文件夹'{dir}'打开失败，请手动访问。";
        }
    }
}