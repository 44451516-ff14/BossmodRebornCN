using BossMod.RebornUI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using RebornMaterial;

namespace BossMod;

public sealed partial class ModuleViewer
{
    private enum MaterialFilterAction { None, Toggle, Solo, Enable, Disable }

    internal void DrawMaterial(WorldState ws)
    {
        EnsureInitialized();

        var avail = ImGui.GetContentRegionAvail();
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var filterWidth = MathF.Min(280f * M3.Scale, avail.X * 0.4f);

        using (var child = BeginMaterialPanel("FiltersPanel", new(filterWidth, avail.Y)))
        {
            if (child)
            {
                DrawMaterialFilters();
            }
        }

        ImGui.SameLine(0f, spacing);
        using (var child = BeginMaterialPanel("ModulesPanel", new(MathF.Max(1f, avail.X - filterWidth - spacing), avail.Y)))
        {
            if (child)
            {
                DrawMaterialModules(ws);
            }
        }
    }

    private static ImRaii.ChildDisposable BeginMaterialPanel(string id, Vector2 size)
    {
        var s = M3.Scheme;
        var min = ImGui.GetCursorScreenPos();
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(min, min + size, M3.U32(s.SurfaceContainerLow, 0.85f), M3.ShapeMedium);
        drawList.AddRect(min, min + size, M3.U32(s.OutlineVariant, 0.55f), M3.ShapeMedium, ImDrawFlags.None, 1f * M3.Scale);

        using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(M3.Space2, M3.Space2));
        return ImRaii.Child(id, size, false, ImGuiWindowFlags.AlwaysUseWindowPadding);
    }

    private void DrawMaterialFilters()
    {
        M3Widgets.SearchField("##search", "搜索战斗，例如“绝境战”", ref _searchText, ImGui.GetContentRegionAvail().X, 100);

        M3Widgets.SectionLabel("资料片");
        for (var e = BossModuleInfo.Expansion.RealmReborn; e < BossModuleInfo.Expansion.Count; ++e)
        {
            ref var expansion = ref Expansions[(int)e];
            var (anyEnabled, allEnabled) = _moduleConfig.ExpansionEnabledState(e);
            var action = DrawMaterialFilterRow($"expansion-{e}", expansion.name, expansion.icon, !_filterExpansions[(int)e], anyEnabled, allEnabled);
            switch (action)
            {
                case MaterialFilterAction.Toggle:
                    _filterExpansions.Toggle((int)e);
                    break;
                case MaterialFilterAction.Solo:
                    _filterExpansions = ~_filterExpansions;
                    _filterExpansions.Toggle((int)e);
                    break;
                case MaterialFilterAction.Enable or MaterialFilterAction.Disable:
                    _moduleConfig.SetExpansionEnabled(e, action == MaterialFilterAction.Enable);
                    break;
            }
        }

        M3Widgets.SectionLabel("内容类型");
        for (var c = BossModuleInfo.Category.Uncategorized; c < BossModuleInfo.Category.Count; ++c)
        {
            ref var category = ref Categories[(int)c];
            var (anyEnabled, allEnabled) = _moduleConfig.CategoryEnabledState(c);
            var action = DrawMaterialFilterRow($"category-{c}", category.name, category.icon, !_filterCategories[(int)c], anyEnabled, allEnabled);
            switch (action)
            {
                case MaterialFilterAction.Toggle:
                    _filterCategories.Toggle((int)c);
                    break;
                case MaterialFilterAction.Solo:
                    _filterCategories = ~_filterCategories;
                    _filterCategories.Toggle((int)c);
                    break;
                case MaterialFilterAction.Enable or MaterialFilterAction.Disable:
                    _moduleConfig.SetCategoryEnabled(c, action == MaterialFilterAction.Enable);
                    break;
            }
        }
    }

    private static MaterialFilterAction DrawMaterialFilterRow(string id, string name, uint icon, bool visible, bool anyEnabled, bool allEnabled)
    {
        using var scope = ImRaii.PushId(id);
        var action = MaterialFilterAction.None;
        var start = ImGui.GetCursorScreenPos();
        var height = M3Widgets.ChipHeight;
        var hit = M3Widgets.CheckboxHitSize().Y;

        ImGui.SetCursorScreenPos(start + new Vector2(0f, (height - hit) * 0.5f));
        var enabled = allEnabled;
        if (M3Widgets.Checkbox("##enable", ref enabled, mixed: anyEnabled && !allEnabled))
        {
            action = enabled ? MaterialFilterAction.Enable : MaterialFilterAction.Disable;
        }
        M3Tooltip.Hovered(anyEnabled != allEnabled ? "此分类中有部分模块已禁用。点击以全部启用。" : allEnabled ? "禁用此分类中的所有模块。" : "启用此分类中的所有模块。");

        ImGui.SetCursorScreenPos(new(start.X + hit + M3.Space1, start.Y));
        var texture = Service.Texture?.GetFromGameIcon(icon).GetWrapOrDefault();
        var room = ImGui.GetContentRegionAvail().X - (M3Widgets.ChipWidth("", texture: texture) + 1f);
        var label = M3Navigation.Truncate(name, MathF.Max(16f * M3.Scale, room));
        var tooltip = label != name ? $"{name}\n点击以显示或隐藏。右键点击以仅显示此项。" : "点击以显示或隐藏。右键点击以仅显示此项。";
        if (M3Widgets.Chip("##filter", label, visible, tooltip: tooltip, texture: texture))
        {
            action = MaterialFilterAction.Toggle;
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            action = MaterialFilterAction.Solo;
        }
        return action;
    }

    private void DrawMaterialModules(WorldState ws)
    {
        var rowHeight = M3Tree.RowHeight;
        var anyShown = false;
        for (var i = 0; i < (int)BossModuleInfo.Expansion.Count; ++i)
        {
            if (_filterExpansions[i])
            {
                continue;
            }

            for (var j = 0; j < (int)BossModuleInfo.Category.Count; ++j)
            {
                if (_filterCategories[j])
                {
                    continue;
                }

                var groupList = Groups[i, j];
                if (groupList == null)
                {
                    continue;
                }

                var groups = CollectionsMarshal.AsSpan(groupList);
                var countG = groups.Length;
                for (var k = 0; k < countG; ++k)
                {
                    var group = groups[k];
                    var groupModuleOIDs = _moduleConfig.MinMaturity == BossModuleInfo.Maturity.Dummy ? group.ModuleOIDs : group.NonDummyModuleOIDs;
                    if (groupModuleOIDs.Count == 0)
                    {
                        continue;
                    }

                    if (!_searchText.IsNullOrEmpty() && !group.Info.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    anyShown = true;
                    DrawMaterialGroup(i, j, group, groupModuleOIDs, rowHeight, ws);
                }
            }
        }

        if (!anyShown)
        {
            M3Widgets.EmptyState("##nofights", "没有可显示的战斗",
                _searchText.Length > 0 ? $"当前筛选条件下没有匹配“{_searchText}”的战斗。" : "所有资料片或内容类型均已被筛选隐藏。", FontAwesomeIcon.Filter);
        }
    }

    private void DrawMaterialGroup(int expansion, int category, ModuleGroup group, List<uint> moduleOIDs, float rowHeight, WorldState ws)
    {
        var start = ImGui.GetCursorScreenPos();
        var width = MathF.Max(1f, ImGui.GetContentRegionAvail().X);

        if (!ImGui.IsRectVisible(start, start + new Vector2(width, rowHeight)) && !M3Tree.IsOpen(group.NodeLabel))
        {
            ImGui.Dummy(new(width, rowHeight));
            return;
        }

        var drawList = ImGui.GetWindowDrawList();
        var x = start.X;
        DrawMaterialIcon(drawList, Expansions[expansion].icon, new(x, start.Y), rowHeight);
        x += rowHeight + M3.Space1;
        DrawMaterialIcon(drawList, group.Info.Icon != 0u ? group.Info.Icon : Categories[category].icon, new(x, start.Y), rowHeight);
        x += rowHeight + M3.Space2;

        var hit = M3Widgets.CheckboxHitSize().Y;
        ImGui.SetCursorScreenPos(new(x, start.Y + (rowHeight - hit) * 0.5f));
        var (anyEnabled, allEnabled) = _moduleConfig.ModulesEnabledState(moduleOIDs);
        var enabled = allEnabled;
        if (M3Widgets.Checkbox(group.EnableID, ref enabled, mixed: anyEnabled && !allEnabled))
        {
            _moduleConfig.SetModulesEnabled(moduleOIDs, enabled);
        }
        M3Tooltip.Hovered(anyEnabled != allEnabled ? "此分组中有部分模块已禁用。点击以全部启用。" : enabled ? "禁用此分组中的所有模块。" : "启用此分组中的所有模块。");
        x += hit + M3.Space1;

        ImGui.SetCursorScreenPos(new(x, start.Y));
        using var node = M3Tree.Node(group.NodeLabel, group.Info.Name, trailing: moduleOIDs.Count > 1 ? moduleOIDs.Count.ToString() : null);
        if (!node.Open)
        {
            return;
        }

        var indent = x - start.X;
        ImGui.Indent(indent);
        var modules = CollectionsMarshal.AsSpan(EnsureModules(group));
        var len = modules.Length;
        for (var l = 0; l < len; ++l)
        {
            ref readonly var mod = ref modules[l];
            if (_moduleConfig.IncludeInSupportedFightControls(mod.Info))
            {
                DrawMaterialModuleRow(mod, rowHeight, ws);
            }
        }
        ImGui.Unindent(indent);
    }

    private void DrawMaterialModuleRow(in ModuleInfo mod, float height, WorldState ws)
    {
        var start = ImGui.GetCursorScreenPos();
        var hit = M3Widgets.CheckboxHitSize().Y;

        ImGui.SetCursorScreenPos(start + new Vector2(0f, (height - hit) * 0.5f));
        var moduleEnabled = _moduleConfig.IsModuleEnabled(mod.Info.PrimaryActorOID);
        if (M3Widgets.Checkbox(mod.EnableID, ref moduleEnabled))
        {
            _moduleConfig.SetModuleEnabled(mod.Info.PrimaryActorOID, moduleEnabled);
        }
        M3Tooltip.Hovered(moduleEnabled ? "禁用此模块。" : "启用此模块。");

        var x = start.X + hit + M3.Space1;
        ImGui.SetCursorScreenPos(new(x, start.Y));
        var hasConfig = mod.Info.ConfigType != null || mod.Info.HasPrePullHints;
        if (M3Widgets.IconButton(mod.ConfigID, FontAwesomeIcon.Cog, hasConfig ? "战斗设置" : "此模块没有可用设置", diameter: height, enabled: hasConfig))
        {
            _ = new EncounterSettingsWindow(mod.Info, ws);
        }

        x += height;
        ImGui.SetCursorScreenPos(new(x, start.Y));
        var hasPlans = mod.Info.PlanLevel > 0 && _planDB != null;
        if (M3Widgets.IconButton(mod.PlansID, FontAwesomeIcon.ClipboardList, hasPlans ? "技能冷却计划" : "此模块不支持技能冷却规划", diameter: height, enabled: hasPlans))
        {
            ImGui.OpenPopup(mod.PopupID);
        }

        using (var popup = ImRaii.Popup(mod.PopupID))
        {
            if (popup)
            {
                ModulePlansPopup(mod.Info);
            }
        }

        x += height + M3.Space1;
        var drawList = ImGui.GetWindowDrawList();
        var maturity = mod.Info.Maturity;
        var accent = MaterialMaturityColor(maturity);
        var textSize = ImGui.CalcTextSize(mod.DisplayName);
        var textPos = new Vector2(x, start.Y + (height - textSize.Y) * 0.5f);
        drawList.AddText(textPos, M3.U32(accent), mod.DisplayName);
        var hoverMax = textPos + textSize;

        if (MaterialMaturityBadge(maturity) is { } badge)
        {
            var badgeMin = new Vector2(hoverMax.X + M3.Space2, start.Y + (height - M3Badge.Size(badge).Y) * 0.5f);
            var badgeMax = M3Badge.Paint(drawList, badgeMin, badge, accent);
            hoverMax = new(badgeMax.X, MathF.Max(hoverMax.Y, badgeMax.Y));
        }

        if (ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(textPos, hoverMax))
        {
            var description = GeneratedEnumMetadata.For(maturity).Attribute<PropertyDisplayAttribute>()?.Label ?? maturity.ToString();
            M3Tooltip.Show($"成熟度：{description}\n{mod.HelpText.TrimEnd()}");
        }

        ImGui.SetCursorScreenPos(new(start.X, start.Y + height));
        ImGui.Dummy(new(1f, 0f));
    }

    private static void DrawMaterialIcon(ImDrawListPtr drawList, uint icon, Vector2 min, float size)
    {
        if (Service.Texture?.GetFromGameIcon(icon).GetWrapOrDefault() is { } wrap)
        {
            drawList.AddImage(wrap.Handle, min, min + new Vector2(size));
        }
    }

    private static Vector4 MaterialMaturityColor(BossModuleInfo.Maturity maturity) => maturity switch
    {
        BossModuleInfo.Maturity.WIP => M3.Severity(M3Severity.Error),
        BossModuleInfo.Maturity.Verified => M3.Severity(M3Severity.Success),
        BossModuleInfo.Maturity.AISupport => M3.Severity(M3Severity.Warning),
        BossModuleInfo.Maturity.Dummy => M3.Alpha(M3.Scheme.OnSurfaceVariant, 0.6f),
        _ => M3.Scheme.OnSurface
    };

    private static string? MaterialMaturityBadge(BossModuleInfo.Maturity maturity) => maturity switch
    {
        BossModuleInfo.Maturity.WIP => "开发中",
        BossModuleInfo.Maturity.Verified => "已验证",
        BossModuleInfo.Maturity.AISupport => "AI 支持",
        BossModuleInfo.Maturity.Dummy => "占位模块",
        _ => null
    };
}
