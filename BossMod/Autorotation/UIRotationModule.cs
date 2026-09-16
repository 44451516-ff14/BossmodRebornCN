using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BossMod.Autorotation;

public sealed class UIRotationModule
{
    public static void DescribeModule(Type type, RotationModuleDefinition definition)
    {
        ImGui.TextUnformatted(definition.DisplayName);
        ImGui.TextUnformatted(definition.Description);
        ImGui.TextUnformatted($"L{definition.MinLevel}-{definition.MaxLevel} {string.Join(" ", GetClasses(definition))}");
        ImGui.TextUnformatted($"Author/contributors: {definition.Author}");
        ImGui.TextUnformatted($"Quality: {(int)definition.Quality}/{(int)RotationModuleQuality.Count - 1} {GeneratedEnumMetadata.For(definition.Quality).Attribute<PropertyDisplayAttribute>()?.Label ?? ""}");
        using (ImRaii.Disabled())
        {
            ImGui.TextUnformatted($"Class: {type.FullName}");
            ImGui.TextUnformatted($"Order group: {definition.Order}");
        }
    }

    private static Class[] GetClasses(RotationModuleDefinition definition)
    {
        var classes = definition.Classes.SetBits();
        var len = classes.Length;
        var strings = new Class[len];
        for (var i = 0; i < len; ++i)
        {
            strings[i] = (Class)classes[i];
        }
        return strings;
    }
}
