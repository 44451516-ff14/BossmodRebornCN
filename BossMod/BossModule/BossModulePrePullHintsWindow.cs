using Dalamud.Bindings.ImGui;

namespace BossMod;

public sealed class BossModulePrePullHintsWindow : UIWindow
{
    private const string WindowID = "###Boss module pre-pull hints";

    private readonly BossModuleManager _mgr;
    private BossModule? _displayedModule;
    private BossModule? _dismissedModule;
    private bool _permanentlySuppressing;

    public BossModulePrePullHintsWindow(BossModuleManager mgr) : base(WindowID, false, new(450f, 180f))
    {
        _mgr = mgr;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        var module = _mgr.ActiveModule;
        _displayedModule = module;

        var config = BossModuleManager.Config;
        var show = config.ShowPrePullHints && module != null && module.StateMachine.ActiveState == null && module.Info is { HasPrePullHints: true } info
            && module.PrePullHints.Length > 0 && config.ShowPrePullHintsFor(info.PrimaryActorOID) && !ReferenceEquals(module, _dismissedModule);

        IsOpen = show;
        if (show)
        {
            var encounterName = string.IsNullOrEmpty(module!.PrimaryActor.Name) ? module.GetType().Name : module.PrimaryActor.Name;
            WindowName = $"Pre-fight hints: {encounterName}{WindowID}";
        }
    }

    public override void Draw()
    {
        var module = _displayedModule;
        if (module == null)
        {
            return;
        }

        ImGui.TextWrapped("Before the pull:");
        ImGui.Separator();
        ImGui.PushTextWrapPos();
        var hints = module.PrePullHints;
        var len = hints.Length;
        for (var i = 0; i < len; ++i)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(hints[i]);
        }
        ImGui.PopTextWrapPos();

        ImGui.Spacing();
        ImGui.Separator();

        if (ImGui.Button("Never show again"))
        {
            _permanentlySuppressing = true;
            if (module.Info != null)
            {
                BossModuleManager.Config.SetShowPrePullHintsFor(module.Info.PrimaryActorOID, false);
            }
            IsOpen = false;
        }
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("You can re-enable this popup from this encounter's config window.");
        }

        ImGui.SameLine();
        if (ImGui.Button("Dismiss for now"))
        {
            _dismissedModule = module;
            IsOpen = false;
        }
    }

    public override void PostDraw()
    {
        // Closing with the title-bar X should behave like "Dismiss for now" rather than reopening next frame.
        if (!IsOpen && _displayedModule != null && !_permanentlySuppressing)
        {
            _dismissedModule = _displayedModule;
        }
        _permanentlySuppressing = false;
    }
}
