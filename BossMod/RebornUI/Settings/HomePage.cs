using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;
using System.Diagnostics;
using System.IO;

namespace BossMod.RebornUI;

internal sealed class HomePage(DirectoryInfo? replayDir, Action<string> navigate)
{
    public const string DiscordUrl = "https://discord.gg/p54TZMPnC9";
    public const string GitHubUrl = "https://github.com/FFXIV-CombatReborn/BossmodReborn";
    public const string WikiUrl = "https://github.com/awgil/ffxiv_bossmod/wiki";

    private static readonly Vector4 DiscordColor = Color.FromComponents(88u, 101u, 242u).ToFloat4();

    private readonly record struct Feature(string Title, FontAwesomeIcon Icon, string Page, string Action, string[] Points);

    private readonly Feature[] _features =
    [
        new("战斗雷达", FontAwesomeIcon.Bullseye, nameof(BossModuleConfig), "雷达设置",
        [
            "在屏幕上以小地图显示场地、玩家、首领、即将到来的范围攻击及其他机制。",
            "无需记住每个技能的效果，直接查看即将到来的范围攻击是否会打到你。",
            "适用于“支持的战斗”中列出的所有战斗。",
        ]),
        new("自动循环（不受支持）", FontAwesomeIcon.Bolt, SettingsWindow.PresetsPage, "编辑预设",
        [
            "VBM 的自动循环系统。",
            "Combat Reborn 团队不提供支持；此功能会随 VBM 更新，以兼容其他插件。",
        ]),
        new("冷却规划", FontAwesomeIcon.CalendarAlt, SettingsWindow.FightsPage, "支持的战斗",
        [
            "为支持的首领战创建技能冷却计划。",
            "规划特定技能的使用时机，并特别兼容 Rotation Solver Reborn。",
        ]),
        new("自动移动", FontAwesomeIcon.Robot, nameof(AI.AIConfig), "自动移动设置",
        [
            "根据战斗模块在雷达上显示的安全位置移动角色。",
            "与不熟悉的玩家组队时不建议使用。",
        ]),
        new("战斗回放", FontAwesomeIcon.Film, nameof(ReplayManagementConfig), "回放设置",
        [
            "用于制作战斗模块、分析模块问题以及制定技能冷却计划。",
            "寻求帮助时请附上回放。除非启用匿名化，回放中会包含角色名。",
            "可在回放设置中开启回放管理窗口或自动录制。",
        ]),
    ];

    public void Draw()
    {
        var s = M3.Scheme;
        using (M3Card.Begin("home_intro", "BossMod Reborn", FontAwesomeIcon.None, s.Primary, M3CardStyle.Elevated))
        {
            var logoSize = 64f * M3.Scale;
            ImGui.Dummy(new(logoSize));
            RebornFoldWindow.DrawLogo(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), logoSize);
            ImGui.SameLine(0f, M3.Space3);
            using (ImRaii.Group())
            {
                M3Text.Draw("BossMod Reborn（BMR）提供战斗雷达、自动移动和技能冷却规划。各项功能均可单独启用或关闭。如需帮助，请前往 Combat Reborn Discord。", M3TextStyle.Body, true);
            }
            ImGui.Dummy(new(0f, M3.Space1));
            M3Widgets.Banner("##home_fork", "本项目是原版 BossMod（VBM）的分支。仅在 Combat Reborn Discord 寻求支持。请勿同时加载 VBM 与 BMR；其后果尚未验证，且不受支持。",
                M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle);
        }

        DrawFeatures();
        DrawLinks();
    }

    private void DrawFeatures()
    {
        var columns = ImGui.GetContentRegionAvail().X >= 620f * M3.Scale ? 2 : 1;
        using var table = ImRaii.Table("##home_features", columns, ImGuiTableFlags.SizingStretchSame);
        if (!table)
        {
            return;
        }

        for (var i = 0; i < _features.Length; ++i)
        {
            ImGui.TableNextColumn();
            ref readonly var feature = ref _features[i];
            using var card = M3Card.Begin($"home_feature_{i}", feature.Title, feature.Icon, M3.AccentFor(i));
            foreach (var point in feature.Points)
            {
                M3Text.Bullet(point, M3TextStyle.Muted, dot: M3.AccentFor(i));
            }

            ImGui.Dummy(new(0f, M3.Space1));
            if (M3Widgets.Button($"##home_open_{i}", feature.Action, M3ButtonStyle.Tonal, FontAwesomeIcon.ArrowRight))
            {
                navigate(feature.Page);
            }
        }
    }

    private void DrawLinks()
    {
        using var card = M3Card.Begin("home_links", "相关链接", FontAwesomeIcon.Link, M3.Scheme.Secondary);
        var error = "";
        var flow = new M3Flow();

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Comments, "Combat Reborn Discord"));
        if (M3Widgets.Button("##home_discord", "Combat Reborn Discord", M3ButtonStyle.Filled, FontAwesomeIcon.Comments, accent: DiscordColor))
        {
            error = OpenLink(DiscordUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.CodeBranch, "GitHub"));
        if (M3Widgets.Button("##home_github", "GitHub", M3ButtonStyle.Tonal, FontAwesomeIcon.CodeBranch))
        {
            error = OpenLink(GitHubUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Book, "BossMod Wiki"));
        if (M3Widgets.Button("##home_wiki", "BossMod Wiki", M3ButtonStyle.Tonal, FontAwesomeIcon.Book))
        {
            error = OpenLink(WikiUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.FolderOpen, "打开回放文件夹"));
        if (M3Widgets.Button("##home_replays", "打开回放文件夹", M3ButtonStyle.Outlined, FontAwesomeIcon.FolderOpen, enabled: replayDir != null, tooltip: replayDir?.FullName))
        {
            error = OpenDirectory(replayDir!);
        }

        if (error.Length > 0)
        {
            M3Snackbar.Show(error, severity: M3Severity.Error);
        }
    }

    public static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"无法打开链接“{link}”，请在浏览器中手动打开。";
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
        {
            return $"找不到文件夹“{dir}”。";
        }

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"无法打开文件夹“{dir}”，请手动打开。";
        }
    }
}
