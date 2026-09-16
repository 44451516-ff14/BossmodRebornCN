using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public sealed class UITabs(string id)
{
    private readonly List<(string Name, Action Tab)> _tabs = [];
    private readonly string _id = id;
    private string _forceSelect = "";

    public void Add(string name, Action tab)
    {
        if (name.Length == 0)
        {
            throw new ArgumentException($"Tab '{name}' has empty or duplicate name");
        }

        var count = _tabs.Count;
        for (var ti = 0; ti < count; ++ti)
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
        using var id = ImRaii.PushId(_id);

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
