using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal static class PartyRolesPanel
{
    private static readonly List<Class> _priorityScratch = [];

    public static void Draw(PartyRolesConfig config, WorldState ws)
    {
        if (M3Widgets.RowButton("自动分配职责###autoassign", "自动分配", "根据队员职业和队伍顺序自动分配职责。", M3ButtonStyle.Filled, FontAwesomeIcon.Magic))
        {
            config.AutoAssignRoles(ws.Party);
        }

        M3Widgets.SectionLabel("当前队伍");
        DrawPartyAssignments(config, ws);

        M3Widgets.SectionLabel("坦克自动分配优先级");
        M3Text.Draw("拖动以调整顺序，越靠前优先级越高。", M3TextStyle.Muted, true);
        using (var table = ImRaii.Table("tankpriority", 2, ImGuiTableFlags.SizingStretchSame))
        {
            if (table)
            {
                ImGui.TableNextColumn();
                M3Text.Draw("主坦克", M3TextStyle.Label);
                DrawTankPriority(config, "mt", config.MainTankPriority);
                ImGui.TableNextColumn();
                M3Text.Draw("副坦克", M3TextStyle.Label);
                DrawTankPriority(config, "ot", config.OffTankPriority);
            }
        }

        ImGui.Dummy(new(0f, M3.Space1));
        if (M3Widgets.RowHoldButton("清除所有分配###clearassignments", "按住以清除", "删除所有已保存的分配，例如可用于缩小配置文件。",
            1f, M3ButtonStyle.Danger, FontAwesomeIcon.TrashAlt, enabled: config.Assignments.Count > 0))
        {
            config.Assignments.Clear();
            config.Modified.Fire();
        }
    }

    private static void DrawTankPriority(PartyRolesConfig config, string id, Class[] order)
    {
        _priorityScratch.Clear();
        _priorityScratch.AddRange(order);
        var selected = -1;
        if (M3Widgets.ReorderList(id, _priorityScratch, static c => c.ToString(), ref selected))
        {
            _priorityScratch.CopyTo(order);
            config.Modified.Fire();
        }
    }

    private static void DrawPartyAssignments(PartyRolesConfig config, WorldState ws)
    {
        List<(ulong cid, string name, char role, PartyRolesConfig.Assignment assignment)> party = [with(PartyState.MaxPartySize)];
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            ref var m = ref ws.Party.Members[i];
            if (m.IsValid())
            {
                party.Add(new(m.ContentId, ws.Party[i]?.Name ?? "", ws.Party[i]?.Role.ToString()[0] ?? '?', config[m.ContentId]));
            }
        }

        if (party.Count == 0)
        {
            M3Text.Draw("登录后即可分配队伍职责。", M3TextStyle.Muted, true);
            return;
        }

        using (var table = ImRaii.Table("assignments", 10, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            if (table)
            {
                var type = typeof(PartyRolesConfig.Assignment);
                var assignments = (PartyRolesConfig.Assignment[])type.GeneratedEnumValues();
                var len = assignments.Length;
                var names = type.GeneratedEnumNames();
                var radio = M3Widgets.RadioSize().X;
                for (var i = 0; i < len; ++i)
                {
                    var name = i == (int)PartyRolesConfig.Assignment.Unassigned ? "无" : names[i];
                    ImGui.TableSetupColumn(name, ImGuiTableColumnFlags.WidthFixed, MathF.Max(radio, ImGui.CalcTextSize(name).X));
                }

                ImGui.TableSetupColumn("队员");
                ImGui.TableHeadersRow();

                party.Sort(static (a, b) => a.role.CompareTo(b.role));
                var partySpan = CollectionsMarshal.AsSpan(party);
                var count = party.Count;
                for (var j = 0; j < count; ++j)
                {
                    ref var p = ref partySpan[j];
                    ImGui.TableNextRow();
                    for (var i = 0; i < len; ++i)
                    {
                        var r = assignments[i];
                        ImGui.TableNextColumn();
                        if (M3Widgets.RadioButton($"###{p.cid:X}:{r}", p.assignment == r))
                        {
                            if (r != PartyRolesConfig.Assignment.Unassigned)
                            {
                                config.Assignments[p.cid] = r;
                            }
                            else
                            {
                                config.Assignments.Remove(p.cid);
                            }

                            config.Modified.Fire();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    M3Text.Draw($"({p.role}) {p.name}");
                }
            }
        }

        ImGui.Dummy(new(0f, M3.Space1));
        if (config.AssignmentsPerSlot(ws.Party).Length == 0)
        {
            M3Widgets.Banner("##assignmentstatus", "分配无效：每项职责必须且只能分配给一名队员。", M3Severity.Warning, FontAwesomeIcon.ExclamationTriangle);
        }
        else
        {
            M3Widgets.Banner("##assignmentstatus", "分配有效：每项职责均已分配给一名队员。", M3Severity.Success, FontAwesomeIcon.CheckCircle);
        }
    }
}
