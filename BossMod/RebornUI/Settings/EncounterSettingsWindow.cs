using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed class EncounterSettingsWindow(BossModuleRegistry.Info info, WorldState ws) : RebornWindow($"{info.ModuleType.Name} 配置", true, new(900, 700))
{
    private static readonly M3Tab[] _tabs = [new("战斗设置", FontAwesomeIcon.SlidersH), new("队伍职责", FontAwesomeIcon.Users)];

    private readonly BossModuleRegistry.Info _info = info;
    private readonly ConfigNode? _node = info.ConfigType != null ? Service.Config.Get<ConfigNode>(info.ConfigType) : null;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly WorldState _ws = ws;
    private readonly UITree _tree = new();
    private int _tab;

    protected override M3Density Density => SettingsDensity;

    public override void Draw()
    {
        var clicked = M3Navigation.Tabs("##tabs", _tabs, _tab);
        if (clicked >= 0)
        {
            _tab = clicked;
        }
        ImGui.Dummy(new(0f, M3.Space2));

        using var child = ImRaii.Child("##content", -Vector2.One);
        if (!child)
        {
            return;
        }

        if (_tab == 0)
        {
            DrawEncounterTab();
        }
        else
        {
            DrawPartyRolesTab();
        }
    }

    private void DrawEncounterTab()
    {
        if (_info.HasPrePullHints)
        {
            using var card = M3Card.Begin("hints", "开战前提示", FontAwesomeIcon.CommentDots, M3.Scheme.Secondary);
            SettingsRenderer.DrawPrePullHintSetting(_info);
        }

        if (_node != null)
        {
            SettingsRenderer.DrawNodeCards(_node, Service.Config, _tree, _ws, null, accentOffset: _info.HasPrePullHints ? 1 : 0);
        }
        else if (!_info.HasPrePullHints)
        {
            M3Widgets.EmptyState("##nosettings", "没有可用设置", "此模块没有提供其他配置项。", FontAwesomeIcon.SlidersH);
        }
    }

    private void DrawPartyRolesTab()
    {
        if (_ws.Party.Player() != null)
        {
            SettingsRenderer.DrawNodeCards(_prc, Service.Config, _tree, _ws, null, FontAwesomeIcon.Users);
        }
        else
        {
            M3Widgets.EmptyState("##noplayer", "尚未登录", "登录后即可分配队伍职责。", FontAwesomeIcon.Users);
        }
    }
}
