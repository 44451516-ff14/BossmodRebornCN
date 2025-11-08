using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public sealed class ConfigUI : IDisposable
{
    private class UINode(ConfigNode node)
    {
        public ConfigNode Node = node;
        public string Name = "";
        public int Order;
        public UINode? Parent;
        public List<UINode> Children = [];
        public string[] Tags = [];

        public List<string> Path = [];
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    private readonly List<List<string>> _filterNodes = [];

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB) : null;

        _tabs.Add("设置", DrawSettings);
        _tabs.Add("支持的 Bosses", () => _mv.Draw(_tree, _ws));
        _tabs.Add("自动输出预设", () => _presets?.Draw());
        _tabs.Add("Slash commands", DrawAvailableCommands);
        _tabs.Add("说明", _about.Draw);

        Dictionary<Type, UINode> nodes = [];
        var nodes2 = _root.Nodes;
        for (var i = 0; i < nodes2.Count; ++i)
        {
            var n = nodes2[i];
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;
            n.Tags = props?.Tags ?? [];

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
        ResolvePaths(_roots, []);
    }

    private void ResolvePaths(List<UINode> nodes, IEnumerable<string> parent)
    {
        foreach (var n in nodes)
        {
            n.Path = [.. parent, n.Name];
            ResolvePaths(n.Children, n.Path);
        }
    }

    public void Dispose()
    {
        _mv.Dispose();
    }

    public void ShowTab(string name) => _tabs.Select(name);

    public void Draw()
    {
        _tabs.Draw();
    }

    private string _searchText = "";

    private void DrawSettings()
    {
        ImGui.SetNextItemWidth(300);
        if (ImGui.InputTextEx("", "Search for a setting...", ref _searchText))
            FilterNodes();

        ImGui.SameLine();
        using (ImRaii.Disabled(_searchText.Length == 0))
            if (ImGui.Button("Clear"))
            {
                _searchText = "";
                FilterNodes();
            }

        DrawNodes(_roots);
    }

    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", "开启AI" ),
        ( "off", "关闭AI" ),
        ( "toggle", "切换AI开关" ),
        ( "targetmaster", "切换是否聚焦目标领袖" ),
        ( "follow slotX", "跟随指定槽位角色（例：Slot1）" ),
        ( "follow name", "按名称跟随指定队友" ),
        ( "ui", "切换AI菜单显示/隐藏" ),
        ( "forbidactions", "切换技能禁用功能（仅自动循环可用）" ),
        ( "forbidactions on/off", "设置技能禁用为开启/关闭（仅自动循环可用）" ),
        ( "forbidmovement", "切换移动禁用功能" ),
        ( "forbidmovement on/off", "设置移动禁用为开启/关闭" ),
        ( "idlewhilemounted", "切换骑乘状态下是否待机" ),
        ( "idlewhilemounted on/off", "设置骑乘状态下待机为开启/关闭" ),
        ( "followcombat", "切换战斗中是否跟随" ),
        ( "followcombat on/off", "设置战斗中跟随为开启/关闭" ),
        ( "followmodule", "切换首领模块激活时是否跟随" ),
        ( "followmodule on/off", "设置首领模块激活时跟随为开启/关闭" ),
        ( "followoutofcombat", "切换非战斗状态下是否跟随" ),
        ( "followoutofcombat on/off", "设置非战斗状态下跟随目标为开启/关闭" ),
        ( "followtarget", "切换战斗中是否跟随目标" ),
        ( "followtarget on/off", "设置战斗中跟随目标为开启/关闭" ),
        ( "positional X", "跟随目标时切换攻击朝向（可选：任意/背面/侧面/正面）" ),
        ( "maxdistancetarget X", "设置与目标的最大距离（默认=2.6）" ),
        ( "maxdistanceslot X", "设置与槽位角色的最大距离（默认=1）" ),
        ( "mindistance X", "设置与碰撞箱的最小距离（默认=0）" ),
        ( "prefdistance X", "设置与危险区域的偏好距离（默认=0）" ),
        ( "movedelay X", "设置AI移动决策延迟（默认=0）" ),
        ( "obstaclemaps", "切换是否加载障碍物地图" ),
        ( "obstaclemaps on/off", "设置障碍物地图加载为开启/关闭" ),
        ( "setpresetname X", "为AI设置自动循环预设（例：setpresetname vbm default）" )
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", "清除当前预设；除非激活战斗计划，否则自动循环将停止所有操作" ),
        ( "ar disable", "强制禁用自动循环；即使激活战斗计划，也不会自动执行任何技能" ),
        ( "ar set Preset", "开始执行指定预设" ),
        ( "ar toggle", "若未禁用则强制关闭自动循环；若已禁用则清除所有强制覆盖设置" ),
        ( "ar toggle Preset", "若指定预设未激活则开始执行；若已激活则清除该预设（停止执行）" ),
        ( "ar ui", "切换自动循环界面显示/隐藏" ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "restorerotation", "切换技能释放后角色朝向复位设置" ),
        ( "resetcolors", "将所有颜色重置为默认值" ),
        ( "d", "打开调试菜单" ),
        ( "r", "打开回放菜单" ),
        ( "r on/off", "开始/停止录制回放" ),
        ( "gc", "触发垃圾回收" ),
        ( "cfg", "列出所有配置项" )
    ];

    private static void DrawAvailableCommands()
    {
        ImGui.Text("Available Commands:");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        for (var i = 0; i < 28; ++i)
        {
            ref readonly var text = ref _availableAICommands[i];
            ImGui.Text($"/bmrai {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Autorotation commands:");
        ImGui.Separator();
        for (var i = 0; i < 6; ++i)
        {
            ref readonly var text = ref _autorotationCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
        ImGui.Separator();
        ImGui.Text("Other commands:");
        ImGui.Separator();
        for (var i = 0; i < 7; ++i)
        {
            ref readonly var text = ref _availableOtherCommands[i];
            ImGui.Text($"/bmr {text.Item1}: {text.Item2}");
        }
    }

    private void FilterNodes()
    {
        _filterNodes.Clear();

        if (_searchText.Length == 0)
            return;

        foreach (var r in _roots)
            foreach (var path in WalkNodes(r))
                _filterNodes.Add(path);
    }

    private static readonly Dictionary<Type, List<(FieldInfo Field, PropertyDisplayAttribute Attr)>> _fieldCache = [];

    private List<List<string>> WalkNodes(UINode node)
    {
        var results = new List<List<string>>();
        WalkNodesInternal(node, [], results);
        return results;
    }

    private void WalkNodesInternal(UINode node, List<string> path, List<List<string>> results)
    {
        if (Utils.TextMatch(node.Name, _searchText) || TagsMatch(node.Tags))
        {
            var matchPath = new List<string>(path) { node.Name, "*" };
            results.Add(matchPath);
            return;
        }

        foreach (var (_, props) in GetFieldAttributes(node.Node.GetType()))
        {
            if (Utils.TextMatch(props.Label, _searchText) || TagsMatch(props.Tags))
            {
                var matchPath = new List<string>(path) { node.Name, props.Label };
                results.Add(matchPath);
            }
        }

        path.Add(node.Name);
        foreach (var child in node.Children)
        {
            WalkNodesInternal(child, path, results);
        }
        path.RemoveAt(path.Count - 1);
    }

    private static List<(FieldInfo, PropertyDisplayAttribute)> GetFieldAttributes(Type type)
    {
        if (_fieldCache.TryGetValue(type, out var cached))
            return cached;

        var list = new List<(FieldInfo, PropertyDisplayAttribute)>();
        foreach (var field in type.GetFields())
        {
            var attr = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (attr != null)
                list.Add((field, attr));
        }

        _fieldCache[type] = list;
        return list;
    }

    private bool TagsMatch(string[] tags)
    {
        foreach (var tag in tags)
        {
            if (Utils.TextMatch(tag, _searchText))
                return true;
        }
        return false;
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws, Func<PropertyDisplayAttribute, bool>? filter = null)
    {
        // draw standard properties
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
                continue;

            if (filter?.Invoke(props) == false)
                continue;

            var value = field.GetValue(node);
            if (DrawProperty(props.Label, props.Tooltip, node, field, value, root, tree, ws))
            {
                node.Modified.Fire();
            }

            if (props.Separator)
            {
                ImGui.Separator();
            }
        }

        // draw custom stuff
        node.DrawCustom(tree, ws);
    }

    private static string GenerateNodeName(Type t) => t.Name.EndsWith("Config", StringComparison.Ordinal) ? t.Name[..^"Config".Length] : t.Name;

    private static void SortByOrder(List<UINode> nodes)
    {
        nodes.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        foreach (var n in nodes)
            SortByOrder(n.Children);
    }

    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes.Where(n => MatchesFilter(n.Path)), n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws, props => MatchesFilter([.. n.Path, props.Label]));
            DrawNodes(n.Children);
        }
    }

    private bool MatchesFilter(List<string> path)
    {
        if (_filterNodes.Count == 0)
            return true;

        bool matchesOneFilter(List<string> filter)
        {
            var i = 0;
            foreach (var f in filter)
            {
                if (f == "*" || i >= path.Count)
                    return true;

                if (f != path[i])
                    return false;

                i++;
            }

            return true;
        }

        return _filterNodes.Any(matchesOneFilter);
    }

    private static void DrawHelp(string tooltip)
    {
        // draw tooltip marker with proper alignment
        ImGui.AlignTextToFramePadding();
        if (tooltip.Length > 0)
        {
            UIMisc.HelpMarker(tooltip);
        }
        else
        {
            using var invisible = ImRaii.PushColor(ImGuiCol.Text, 0x00000000);
            UIMisc.IconText(Dalamud.Interface.FontAwesomeIcon.InfoCircle, "(?)");
        }
        ImGui.SameLine();
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, object? value, ConfigRoot root, UITree tree, WorldState ws) => value switch
    {
        bool v => DrawProperty(label, tooltip, node, member, v),
        Enum v => DrawProperty(label, tooltip, node, member, v),
        float v => DrawProperty(label, tooltip, node, member, v),
        int v => DrawProperty(label, tooltip, node, member, v),
        string v => DrawProperty(label, tooltip, node, member, v),
        Color v => DrawProperty(label, tooltip, node, member, v),
        Color[] v => DrawProperty(label, tooltip, node, member, v),
        GroupAssignment v => DrawProperty(label, tooltip, node, member, v, root, tree, ws),
        _ => false
    };

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, bool v)
    {
        DrawHelp(tooltip);
        var combo = member.GetCustomAttribute<PropertyComboAttribute>();
        if (combo != null)
        {
            if (UICombo.Bool(label, combo.Values, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.Checkbox(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Enum v)
    {
        DrawHelp(tooltip);
        if (UICombo.Enum(label, ref v))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, float v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragFloat(label, ref v, slider.Speed, slider.Min, slider.Max, "%.3f", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputFloat(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, int v)
    {
        DrawHelp(tooltip);
        var slider = member.GetCustomAttribute<PropertySliderAttribute>();
        if (slider != null)
        {
            var flags = ImGuiSliderFlags.None;
            if (slider.Logarithmic)
                flags |= ImGuiSliderFlags.Logarithmic;
            ImGui.SetNextItemWidth(Math.Min(ImGui.GetWindowWidth() * 0.30f, 175));
            if (ImGui.DragInt(label, ref v, slider.Speed, (int)slider.Min, (int)slider.Max, "%d", flags))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        else
        {
            if (ImGui.InputInt(label, ref v))
            {
                member.SetValue(node, v);
                return true;
            }
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, string v)
    {
        DrawHelp(tooltip);
        if (ImGui.InputText(label, ref v, 256))
        {
            member.SetValue(node, v);
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color v)
    {
        DrawHelp(tooltip);
        var col = v.ToFloat4();
        if (ImGui.ColorEdit4(label, ref col, ImGuiColorEditFlags.PickerHueWheel))
        {
            member.SetValue(node, Color.FromFloat4(col));
            return true;
        }
        return false;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, Color[] v)
    {
        var modified = false;
        for (var i = 0; i < v.Length; ++i)
        {
            DrawHelp(tooltip);
            var col = v[i].ToFloat4();
            if (ImGui.ColorEdit4($"{label} {i}", ref col, ImGuiColorEditFlags.PickerHueWheel))
            {
                v[i] = Color.FromFloat4(col);
                member.SetValue(node, v);
                modified = true;
            }
        }
        return modified;
    }

    private static bool DrawProperty(string label, string tooltip, ConfigNode node, FieldInfo member, GroupAssignment v, ConfigRoot root, UITree tree, WorldState ws)
    {
        var group = member.GetCustomAttribute<GroupDetailsAttribute>();
        if (group == null)
            return false;

        DrawHelp(tooltip);
        var modified = false;
        foreach (var tn in tree.Node(label, false, v.Validate() ? Colors.TextColor1 : Colors.TextColor2, () => DrawPropertyContextMenu(node, member, v)))
        {
            using var indent = ImRaii.PushIndent();
            using var table = ImRaii.Table("table", group.Names.Length + 2, ImGuiTableFlags.SizingFixedFit);
            if (!table)
                continue;

            foreach (var n in group.Names)
                ImGui.TableSetupColumn(n);
            ImGui.TableSetupColumn("----");
            ImGui.TableSetupColumn("Name");
            ImGui.TableHeadersRow();

            var assignments = root.Get<PartyRolesConfig>().SlotsPerAssignment(ws.Party);
            for (var i = 0; i < (int)PartyRolesConfig.Assignment.Unassigned; ++i)
            {
                var r = (PartyRolesConfig.Assignment)i;
                ImGui.TableNextRow();
                for (var c = 0; c < group.Names.Length; ++c)
                {
                    ImGui.TableNextColumn();
                    if (ImGui.RadioButton($"###{r}:{c}", v[r] == c))
                    {
                        v[r] = c;
                        modified = true;
                    }
                }
                ImGui.TableNextColumn();
                if (ImGui.RadioButton($"###{r}:---", v[r] < 0 || v[r] >= group.Names.Length))
                {
                    v[r] = -1;
                    modified = true;
                }

                var name = r.ToString();
                if (assignments.Length > 0)
                    name += $" ({ws.Party[assignments[i]]?.Name})";
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(name);
            }
        }
        return modified;
    }

    private static void DrawPropertyContextMenu(ConfigNode node, FieldInfo member, GroupAssignment v)
    {
        foreach (var preset in member.GetCustomAttributes<GroupPresetAttribute>())
        {
            if (ImGui.MenuItem(preset.Name))
            {
                for (var i = 0; i < preset.Preset.Length; ++i)
                    v.Assignments[i] = preset.Preset[i];
                node.Modified.Fire();
            }
        }
    }
}
