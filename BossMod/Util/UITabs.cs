using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

[SkipLocalsInit]
public sealed class UITabs
{
    private readonly List<(string Name, Action Tab)> _tabs = [];
    private string _forceSelect = "";

    public void Add(string name, Action tab)
    {
        if (name.Length == 0)
        {
            throw new ArgumentException($"Tab '{name}' has empty or duplicate name");
        }

        for (var ti = 0; ti < _tabs.Count; ++ti)
        {
            if (_tabs[ti].Name == name)
            {
                throw new ArgumentException($"Tab '{name}' has empty or duplicate name");
            }
        }

        _tabs.Add((name, tab));
    }

    public void Select(string name) => _forceSelect = name;

    public void Draw()
    {
        using var tabs = ImRaii.TabBar("Tabs");
        if (!tabs)
        {
            return;
        }

        var count = _tabs.Count;
        for (var i = 0; i < count; ++i)
        {
            var t = _tabs[i];
            using var tab = ImRaii.TabItem(t.Name, t.Name == _forceSelect ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None);
            if (tab)
            {
                t.Tab();
            }
        }
        _forceSelect = "";
    }
}
