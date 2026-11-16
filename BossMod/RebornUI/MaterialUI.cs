using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Interface;
using Dalamud.Plugin;
using RebornMaterial;
using System.IO;

namespace BossMod.RebornUI;

internal static class MaterialUI
{
    private static IDalamudPluginInterface? _dalamud;
    private static SettingsWindow? _settings;
    private static readonly List<FoldHost> _folds = [];
    private static bool _frameFailed;

    public static void Start(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        try
        {
            StartCore(dalamud, ws, rotationDB, rotation, ai, replayDir);
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Failed to start; using the classic interface instead");
            Stop();
        }
    }

    private static void StartCore(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        M3.ElementBaseline = 0.75f;
        M3.Initialize(dalamud, Service.Config.Get<InterfaceConfig>());
        _dalamud = dalamud;

        dalamud.UiBuilder.Draw += BeginFrame;

        _settings = new(Service.Config, ws, replayDir, rotationDB);

        var rotationHost = FoldHost.Wrap<UIRotationWindow>("自动循环", new(400f, 400f), new() { MinimumSize = new(260f, 140f), MaximumSize = new(5000f, 5000f) },
            () => RotationStatus(rotation), ["编辑自动循环预设"]);
        if (rotationHost != null)
        {
            _folds.Add(rotationHost);
        }

        var aiHost = FoldHost.Wrap<AIManagementWindow>("自动移动", new(560f, 340f), new() { MinimumSize = new(300f, 160f), MaximumSize = new(5000f, 5000f) },
            () => AIStatus(ai), null, new FoldHost.HostAction(FontAwesomeIcon.Cog, "打开自动移动设置", () => OpenSettings(nameof(AIConfig))));
        if (aiHost != null)
        {
            _folds.Add(aiHost);
        }
    }

    public static bool OpenSettings(string page = "")
    {
        if (_settings == null)
        {
            return false;
        }

        _settings.Open(SettingsWindow.PageForClassicTab(page));
        return true;
    }

    public static bool OpenEncounterSettings(BossModuleRegistry.Info info, WorldState ws)
    {
        if (_settings == null)
        {
            return false;
        }

        _ = new EncounterSettingsWindow(info, ws);
        return true;
    }

    public static void Stop()
    {
        if (_dalamud == null)
        {
            return;
        }

        _dalamud.UiBuilder.Draw -= BeginFrame;
        foreach (var fold in _folds)
        {
            Try(fold.Dispose);
        }
        _folds.Clear();
        if (_settings != null)
        {
            Try(_settings.Dispose);
            _settings = null;
        }
        Try(M3.Dispose);
        _dalamud = null;
        _frameFailed = false;
    }

    private static void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Cleanup failed");
        }
    }

    private static void BeginFrame()
    {
        if (_frameFailed)
        {
            return;
        }

        try
        {
            M3.BeginFrame();
        }
        catch (Exception ex)
        {
            _frameFailed = true;
            Service.Logger.Error(ex, "[RebornUI] Per-frame update failed; it's off until the plugin reloads");
        }
    }

    private static (string, Vector4?) RotationStatus(RotationModuleManager rotation)
    {
        var preset = rotation.Preset;
        return preset == RotationModuleManager.ForceDisable ? ("已强制停用", M3.Scheme.Error)
            : preset != null ? ($"运行中：{preset.Name}", M3.Severity(M3Severity.Success))
            : ("当前没有启用预设", null);
    }

    private static (string, Vector4?) AIStatus(AIManager ai)
    {
        if (ai.Beh == null)
        {
            return ("已关闭", null);
        }

        var master = ai.WorldState.Party[ai.MasterSlot];
        return (master != null ? $"已开启，正在跟随 {master.Name}（队伍位置 {ai.MasterSlot + 1}）" : "已开启", M3.Severity(M3Severity.Success));
    }
}
