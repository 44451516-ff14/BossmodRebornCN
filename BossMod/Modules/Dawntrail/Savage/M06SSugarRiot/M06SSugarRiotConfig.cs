using Dalamud.Bindings.ImGui;
using System.Reflection;

namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

[ConfigDisplay(Order = 0x110, Parent = typeof(DawntrailConfig))]
public sealed class M06SSugarRiotConfig() : ConfigNode
{
    [PropertyDisplay("启用自定义优先级列表", tooltip: "如果禁用，下面的小怪阶段优先级列表将不会被使用。")]
    public bool EnablePriorityList = true;

    [PropertyDisplay("自动循环的小怪阶段优先级（从高到低）")]
    [PropertyStringOrder(["Sugar Riot", "Mu P1", "Yan P1", "Gimme Cat P1", "Mu P2", "Feather Ray NW", "Feather Ray NE", "Yan P3", "Gimme Cat P3", "Jabberwock P3",
    "Feather Ray SW", "Feather Ray SE", "Mu P4", "Gimme Cat P4", "Jabberwock P4", "Yan P4"])]
    public int[] AddsPriorityOrder = [15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0];

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        var fields = GetSerializableFields(GetType());
        var len = fields.Length;
        for (var j = 0; j < len; ++j)
        {
            ref readonly var field = ref fields[j];
            var labelAttr = field.GetCustomAttribute<PropertyDisplayAttribute>();
            var reorderAttr = field.GetCustomAttribute<PropertyStringOrderAttribute>();

            if (labelAttr != null && reorderAttr != null && field.FieldType == typeof(int[]))
            {
                var indices = (int[]?)field.GetValue(this);
                var values = reorderAttr.Values;
                if (indices == null || indices.Length != values.Length)
                    continue;

                ImGui.TextUnformatted(labelAttr.Label + ":");
                var lenI = indices.Length;
                for (var i = 0; i < lenI; ++i)
                {
                    var str = values[indices[i]];
                    ImGui.PushID(i);

                    if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowUp, $"###up{i}") && i > 0)
                    {
                        (indices[i - 1], indices[i]) = (indices[i], indices[i - 1]);
                        Modified.Fire();
                    }

                    ImGui.SameLine();
                    if (UIMisc.IconButton(Dalamud.Interface.FontAwesomeIcon.ArrowDown, $"###down{i}") && i < lenI - 1)
                    {
                        (indices[i + 1], indices[i]) = (indices[i], indices[i + 1]);
                        Modified.Fire();
                    }
                    ImGui.SameLine();
                    ImGui.TextUnformatted(str);
                    ImGui.PopID();
                }
            }
        }
    }
}
