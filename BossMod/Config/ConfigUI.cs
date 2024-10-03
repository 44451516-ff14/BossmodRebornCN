using BossMod.Autorotation;
using ImGuiNET;
using System.Diagnostics;
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
    }

    private readonly List<UINode> _roots = [];
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly AboutTab _about;
    private readonly ModuleViewer _mv;
    private readonly ConfigRoot _root;
    private readonly WorldState _ws;
    private readonly UIPresetDatabaseEditor? _presets;

    public ConfigUI(ConfigRoot config, WorldState ws, DirectoryInfo? replayDir, RotationDatabase? rotationDB)
    {
        _root = config;
        _ws = ws;
        _about = new(replayDir);
        _mv = new(rotationDB?.Plans, ws);
        _presets = rotationDB != null ? new(rotationDB.Presets) : null;

        
        _tabs.Add("设置", DrawSettings);
        _tabs.Add("支持的 Bosses", () => _mv.Draw(_tree, _ws));
        _tabs.Add("自动输出预设", () => _presets?.Draw());
        _tabs.Add("Slash commands", DrawAvailableCommands);
        _tabs.Add("说明", _about.Draw);
        

        Dictionary<Type, UINode> nodes = [];
        foreach (var n in config.Nodes)
        {
            nodes[n.GetType()] = new(n);
        }

        foreach (var (t, n) in nodes)
        {
            var props = t.GetCustomAttribute<ConfigDisplayAttribute>();
            n.Name = props?.Name ?? GenerateNodeName(t);
            n.Order = props?.Order ?? 0;
            n.Parent = props?.Parent != null ? nodes.GetValueOrDefault(props.Parent) : null;

            var parentNodes = n.Parent?.Children ?? _roots;
            parentNodes.Add(n);
        }

        SortByOrder(_roots);
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

    private static readonly Dictionary<string, string> _availableAICommands = new()
    {
        { "on", "开启AI" },
        { "off", "关闭AI " },
        { "toggle", "切换 the AI on/off." },
        { "targetmaster", "Toggles the focus on target leader." },
        { "follow slotX", "Follows the specified slot, eg. Slot1." },
        { "follow name", "Follows the specified party member by name." },
        { "ui", "Toggles the AI menu." },
        { "forbidactions", "Toggles the forbidding of actions. (only for autorotation)" },
        { "forbidactions on/off", "Sets forbid actions to on or off. (only for autorotation)" },
        { "forbidmovement", "Toggles the forbidding of movement." },
        { "forbidmovement on/off", "Sets forbid movement to on or off." },
        { "followcombat", "Toggles following during combat." },
        { "followcombat on/off", "Sets following following during combat to on or off." },
        { "followmodule", "Toggles following during active boss module." },
        { "followmodule on/off", "Sets following following during active boss module to on or off." },
        { "followoutofcombat", "Toggles following during out of combat." },
        { "followoutofcombat on/off", "Sets following target out of combat to on or off." },
        { "followtarget", "Toggles following targets during combat." },
        { "followtarget on/off", "Sets following target during combat to on or off." },
        { "positional X", "Switch to positional when following targets. (any, rear, flank, front)" },
        { "maxdistancetarget X", "Set max distance to target. (default = 2.6)" },
        { "maxdistanceslot X", "Set max distance to slot. (default = 1)" }
    };

    private static readonly Dictionary<string, string> _autorotationCommands = new()
    {
        { "ar", "Lists all autorotation commands." },
        { "ar clear", "Clear current preset; autorotation will do nothing unless plan is active" },
        { "ar set Preset", "Force disable autorotation; no actions will be executed automatically even if plan is active." },
        { "ar toggle", "Force disable autorotation if not already; otherwise clear overrides." },
        { "ar toggle Preset", "Start executing specified preset unless it's already active; clear otherwise" },
    };

    private static readonly Dictionary<string, string> _availableOtherCommands = new()
    {
        { "restorerotation", "Toggle restore character orientation after action use setting." },
        { "resetcolors", "Resets all colors to their default values." },
        { "d", "Opens the debug menu." },
        { "r", "Opens the replay menu." },
        { "r on/off", "Starts/stops recording a replay." },
        { "gc", "Triggers the garbage collection." },
        { "cfg", "Lists all configs." }
    };

    private static void DrawAvailableCommands()
    {
        ImGui.Text("Available Commands:");
        ImGui.Separator();
        ImGui.Text("AI:");
        ImGui.Separator();
        foreach (var command in _availableAICommands)
        {
            ImGui.Text($"/bmrai {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("Autorotation commands:");
        ImGui.Separator();
        foreach (var command in _autorotationCommands)
        {
            ImGui.Text($"/bmr {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("Other commands:");
        ImGui.Separator();
        foreach (var command in _availableOtherCommands)
        {
            ImGui.Text($"/bmr {command.Key}: {command.Value}");
        }
        ImGui.Separator();
        ImGui.Text("/vbm can be used instead of /bmr and /vbmai can be used instead of /bmrai");
    }

    private void DrawSettings()
    {
        using var child = ImRaii.Child("SettingsWindow", new Vector2(0, 0), true);
        if (child)
            DrawNodes(_roots);
    }

    public static void DrawNode(ConfigNode node, ConfigRoot root, UITree tree, WorldState ws)
    {
        // draw standard properties
        foreach (var field in node.GetType().GetFields())
        {
            var props = field.GetCustomAttribute<PropertyDisplayAttribute>();
            if (props == null)
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
        nodes.SortBy(e => e.Order);
        foreach (var n in nodes)
            SortByOrder(n.Children);
    }

    private void DrawNodes(List<UINode> nodes)
    {
        foreach (var n in _tree.Nodes(nodes, n => new(n.Name)))
        {
            DrawNode(n.Node, _root, _tree, _ws);
            DrawNodes(n.Children);
        }
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
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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
            ImGui.SetNextItemWidth(MathF.Min(ImGui.GetWindowWidth() * 0.30f, 175));
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
