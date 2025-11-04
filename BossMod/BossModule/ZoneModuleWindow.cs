using Dalamud.Utility;
using Dalamud.Bindings.ImGui;

namespace BossMod;

[SkipLocalsInit]
public sealed class ZoneModuleWindow : UIWindow
{
    private static readonly ZoneModuleConfig config = Service.Config.Get<ZoneModuleConfig>();
    private readonly ZoneModuleManager _zmm;
    private bool _wasOpen;

    public ZoneModuleWindow(ZoneModuleManager zmm) : base("Zone module###Zone module", false, new(400f, 400f))
    {
        _zmm = zmm;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = _zmm.ActiveModule?.WantDrawExtra() ?? false;
        _wasOpen = IsOpen;
        if (IsOpen)
        {
            Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            if (config.TransparentMode)
            {
                Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
            }
            if (config.Lock)
            {
                Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
            }
            ForceMainWindow = config.TransparentMode; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason
            var title = _zmm.ActiveModule!.WindowName();
            if (title.IsNullOrEmpty())
            {
                title = "Zone module###Zone module";
            }
            WindowName = title;
        }
    }

    public override void PostDraw()
    {
        // user closed window
        if (_wasOpen && !IsOpen)
            _zmm.ActiveModule?.OnWindowClose();
    }

    public override void Draw()
    {
        _zmm.ActiveModule?.DrawExtra();
    }
}
