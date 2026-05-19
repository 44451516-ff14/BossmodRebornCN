using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

[ConfigDisplay(Name = "队伍位置分配", Order = 2)]
public class PartyRolesConfig : ConfigNode
{
    public enum Assignment { MT, OT, H1, H2, M1, M2, R1, R2, Unassigned }

    [PropertyDisplay("Automatically assign roles on zone change")]
    public bool AutoAssignOnDutyEnter = false;

    public Dictionary<ulong, Assignment> Assignments = [];

    public Assignment this[ulong contentID] => Assignments.GetValueOrDefault(contentID, Assignment.Unassigned);

    // return either array of assigned roles per party slot (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public Assignment[] AssignmentsPerSlot(PartyState party)
    {
        var counts = new int[(int)Assignment.Unassigned];
        var res = Utils.MakeArray(PartyState.MaxPartySize, Assignment.Unassigned);
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
            {
                return [];
            }

            if (counts[(int)r]++ > 0)
            {
                return [];
            }

            res[i] = r;
        }
        return res;
    }

    // return either array of party slots per assigned role (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public int[] SlotsPerAssignment(PartyState party)
    {
        var res = Utils.MakeArray((int)Assignment.Unassigned, PartyState.MaxPartySize);
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
            {
                return [];
            }

            if (res[(int)r] != PartyState.MaxPartySize)
            {
                return [];
            }

            res[(int)r] = i;
        }
        return res;
    }

    // return array of effective roles per party slot
    public Role[] EffectiveRolePerSlot(PartyState party)
    {
        var res = new Role[PartyState.MaxPartySize];
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            res[i] = this[party.Members[i].ContentId] switch
            {
                Assignment.MT or Assignment.OT => Role.Tank,
                Assignment.H1 or Assignment.H2 => Role.Healer,
                Assignment.M1 or Assignment.M2 => Role.Melee,
                Assignment.R1 or Assignment.R2 => Role.Ranged,
                _ => party[i]?.Role ?? Role.None
            };
        }
        return res;
    }

    // automatically assign party roles based on job priority and party composition
    public void AutoAssignRoles(PartyState party)
    {
        // collect all valid party members with their jobs
        List<(ulong contentId, Class job, Role role, ClassCategory category)> members = [];
        for (var i = 0; i < PartyState.MaxPartySize; ++i)
        {
            ref var m = ref party.Members[i];
            if (m.IsValid() && m.ContentId != 0)
            {
                var actor = party[i];
                if (actor != null)
                {
                    members.Add((m.ContentId, actor.Class, actor.Role, actor.Class.GetClassCategory()));
                }
            }
        }

        // separate by role
        List<(ulong contentId, Class job, Role role, ClassCategory category)> tanks = [];
        List<(ulong contentId, Class job, Role role, ClassCategory category)> healers = [];
        List<(ulong contentId, Class job, Role role, ClassCategory category)> melee = [];
        List<(ulong contentId, Class job, Role role, ClassCategory category)> ranged = [];

        foreach (var member in members)
        {
            if (member.role == Role.Tank)
                tanks.Add(member);
            else if (member.role == Role.Healer)
                healers.Add(member);
            else if (member.role == Role.Melee)
                melee.Add(member);
            else if (member.role == Role.Ranged)
                ranged.Add(member);
        }

        // tank priority: PLD > WAR > DRK > GNB for MT, reverse for OT
        tanks.Sort((a, b) => GetTankPriority(a.job).CompareTo(GetTankPriority(b.job)));
        if (tanks.Count > 0)
            Assignments[tanks[0].contentId] = Assignment.MT;
        if (tanks.Count > 1)
            Assignments[tanks[1].contentId] = Assignment.OT;

        // healer priority: WHM > AST > SCH > SGE for H1, reverse for H2
        healers.Sort((a, b) => GetHealerPriority(a.job).CompareTo(GetHealerPriority(b.job)));
        if (healers.Count > 0)
            Assignments[healers[0].contentId] = Assignment.H1;
        if (healers.Count > 1)
            Assignments[healers[1].contentId] = Assignment.H2;

        // sort ranged by priority first
        ranged.Sort((a, b) => GetRangedPriority(a.job).CompareTo(GetRangedPriority(b.job)));

        // melee DPS - if 3+ ranged, fill melee slots from the lowest priority ranged
        if (ranged.Count >= 3)
        {
            // first try to move RDM to melee (if it exists and is low priority)
            var rdmIndex = -1;
            for (var i = 0; i < ranged.Count; ++i)
            {
                if (ranged[i].job == Class.RDM)
                {
                    rdmIndex = i;
                    break;
                }
            }

            if (rdmIndex >= 0)
            {
                melee.Add(ranged[rdmIndex]);
                ranged.RemoveAt(rdmIndex);
            }
        }

        // if we still need more melee slots and have ranged to spare, move lowest priority ranged
        while (melee.Count < 2 && ranged.Count > 2)
        {
            // take from the end (lowest priority)
            var lastIndex = ranged.Count - 1;
            melee.Add(ranged[lastIndex]);
            ranged.RemoveAt(lastIndex);
        }

        // assign melee by party order
        for (var i = 0; i < melee.Count && i < 2; ++i)
        {
            Assignments[melee[i].contentId] = i == 0 ? Assignment.M1 : Assignment.M2;
        }

        // assign ranged (already sorted by priority)
        if (ranged.Count > 0)
            Assignments[ranged[0].contentId] = Assignment.R1;
        if (ranged.Count > 1)
            Assignments[ranged[1].contentId] = Assignment.R2;

        Modified.Fire();
    }

    private static int GetTankPriority(Class job) => job switch
    {
        Class.PLD or Class.GLA => 1,
        Class.WAR or Class.MRD => 2,
        Class.DRK => 3,
        Class.GNB => 4,
        _ => 99
    };

    private static int GetHealerPriority(Class job) => job switch
    {
        Class.WHM or Class.CNJ => 1,
        Class.AST => 2,
        Class.SCH => 3,
        Class.SGE => 4,
        _ => 99
    };

    private static int GetRangedPriority(Class job) => job switch
    {
        // R1 priority: MCH > BRD > DNC > casters
        Class.MCH => 1,
        Class.BRD or Class.ARC => 2,
        Class.DNC => 3,
        // casters
        Class.BLM or Class.THM => 4,
        Class.SMN or Class.ACN => 5,
        Class.RDM => 6,
        Class.PCT => 7,
        Class.BLU => 8,
        _ => 99
    };

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        if (ImGui.Button("Auto-Assign Roles"))
        {
            AutoAssignRoles(ws.Party);
        }
        ImGui.SameLine();
        ImGui.TextUnformatted("Click to automatically assign party roles based on job and party order");

        using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
        {
            if (table)
            {
                foreach (var r in typeof(Assignment).GetEnumValues())
                {
                    ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                }

                ImGui.TableSetupColumn("名称");
                ImGui.TableHeadersRow();

                List<(ulong cid, string name, char role, Assignment assignment)> party = [];
                for (var i = 0; i < PartyState.MaxPartySize; ++i)
                {
                    ref var m = ref ws.Party.Members[i];
                    if (m.IsValid())
                    {
                        party.Add((m.ContentId, m.Name, ws.Party[i]?.Role.ToString()[0] ?? '?', this[m.ContentId]));
                    }
                }

                party.Sort(static (a, b) => a.role.CompareTo(b.role));
                foreach (var (contentID, name, classRole, assignment) in party)
                {
                    ImGui.TableNextRow();
                    foreach (var r in (Assignment[])typeof(Assignment).GetEnumValues())
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.RadioButton($"###{contentID:X}:{r}", assignment == r))
                        {
                            if (r != Assignment.Unassigned)
                            {
                                Assignments[contentID] = r;
                            }
                            else
                            {
                                Assignments.Remove(contentID);
                            }

                            Modified.Fire();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"({classRole}) {name}");
                }
            }
        }

        if (AssignmentsPerSlot(ws.Party).Length == 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor2);
            ImGui.TextUnformatted("分配无效：每个位置应且只能有一名队员。");
        }
        else
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor4);
            ImGui.TextUnformatted("分配有效！");
        }
    }
}
