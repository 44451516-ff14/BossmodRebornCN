using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed class FactoryGenerator : IIncrementalGenerator
{
    private const string Category = "BossMod.SourceGen";
    private const int ChunkSize = 128;
    private const string RendererMetadataName = "BossMod.Autorotation.IStrategyRenderer";
    private const string PropertyRendererMetadataName = "BossMod.PropertyRenderer";
    private const string UnmanagedRotationMetadataName = "BossMod.QuestBattle.UnmanagedRotation";
    private const string RotationWrapperMetadataName = "BossMod.QuestBattle.RotationModule`1";
    private const string BossModuleMetadataName = "BossMod.BossModule";
    private const string WorldStateMetadataName = "BossMod.WorldState";

    private static readonly DiagnosticDescriptor MissingSymbol = new(
        "BMSG400", "Factory source generation could not start", "Required symbol '{0}' was not found",
        Category, DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor InvalidFactory = new(
        "BMSG401", "Factory cannot be source generated", "Type '{0}' cannot be source generated for {1}: {2}",
        Category, DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context) => Register(context, SourceGenUtilities.DeclaredTypes(context).Collect());

    internal static void Register(IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<System.Collections.Immutable.ImmutableArray<INamedTypeSymbol>> declaredTypes)
    {
        // Factory generation needs the complete declared-type set for the full-name registry, but only a tiny required-symbol state from Compilation itself.
        var missingRequiredSymbols = context.CompilationProvider
            .Select(static (compilation, _) => GetMissingRequiredSymbols(compilation));

        context.RegisterImplementationSourceOutput(declaredTypes.Combine(missingRequiredSymbols), static (productionContext, value) =>
        {
            if (value.Right != 0)
            {
                ReportMissingSymbols(productionContext, value.Right);
                return;
            }
            Generate(productionContext, value.Left);
        });
    }

    private static int GetMissingRequiredSymbols(Compilation compilation)
    {
        var result = 0;
        if (compilation.GetTypeByMetadataName(RendererMetadataName) == null)
        {
            result |= 1 << 0;
        }
        if (compilation.GetTypeByMetadataName(PropertyRendererMetadataName) == null)
        {
            result |= 1 << 1;
        }
        if (compilation.GetTypeByMetadataName(UnmanagedRotationMetadataName) == null)
        {
            result |= 1 << 2;
        }
        if (compilation.GetTypeByMetadataName(RotationWrapperMetadataName) == null)
        {
            result |= 1 << 3;
        }
        if (compilation.GetTypeByMetadataName(BossModuleMetadataName) == null)
        {
            result |= 1 << 4;
        }
        if (compilation.GetTypeByMetadataName(WorldStateMetadataName) == null)
        {
            result |= 1 << 5;
        }
        return result;
    }

    private static void ReportMissingSymbols(SourceProductionContext context, int missing)
    {
        if ((missing & (1 << 0)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, RendererMetadataName));
        }
        if ((missing & (1 << 1)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertyRendererMetadataName));
        }
        if ((missing & (1 << 2)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, UnmanagedRotationMetadataName));
        }
        if ((missing & (1 << 3)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, RotationWrapperMetadataName));
        }
        if ((missing & (1 << 4)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, BossModuleMetadataName));
        }
        if ((missing & (1 << 5)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, WorldStateMetadataName));
        }
    }

    private static void Generate(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> declaredTypes)
    {
        var allTypes = SourceGenUtilities.DistinctTypes(declaredTypes);
        var count = allTypes.Count;
        var knownTypes = new List<INamedTypeSymbol>(count);
        for (var i = 0; i < count; ++i)
        {
            var t = allTypes[i];
            if (CanEmitTypeOf(t))
            {
                knownTypes.Add(t);
            }
        }
        SourceGenUtilities.SortTypesByName(knownTypes);

        var renderers = new List<INamedTypeSymbol>(Math.Min(count, 64));
        for (var i = 0; i < count; ++i)
        {
            var type = allTypes[i];
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || !SourceGenUtilities.Implements(type, RendererMetadataName))
            {
                continue;
            }
            if (!SourceGenUtilities.CanEmitClosedType(type) || !SourceGenUtilities.HasConstructor(type))
            {
                Report(context, type, "strategy renderer", "the type or its parameterless constructor is not accessible");
                continue;
            }
            renderers.Add(type);
        }
        SourceGenUtilities.SortTypesByName(renderers);

        var propertyRenderers = new List<INamedTypeSymbol>(Math.Min(count, 16));
        for (var i = 0; i < count; ++i)
        {
            var type = allTypes[i];
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || !SourceGenUtilities.InheritsFrom(type, PropertyRendererMetadataName))
            {
                continue;
            }
            if (!SourceGenUtilities.CanEmitClosedType(type) || !SourceGenUtilities.HasConstructor(type))
            {
                Report(context, type, "property renderer", "the type or its parameterless constructor is not accessible");
                continue;
            }
            propertyRenderers.Add(type);
        }
        SourceGenUtilities.SortTypesByName(propertyRenderers);

        var rotations = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        for (var i = 0; i < count; ++i)
        {
            var type = allTypes[i];
            if (type.TypeKind != TypeKind.Class || type.IsAbstract)
            {
                continue;
            }
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (!SourceGenUtilities.HasMetadataName(current, RotationWrapperMetadataName) || current.TypeArguments[0] is not INamedTypeSymbol rotation)
                {
                    continue;
                }
                rotations.Add(rotation);
                break;
            }
        }

        var rotationModels = new List<RotationModel>(rotations.Count);
        foreach (var rotation in rotations)
        {
            var ctorKind = SourceGenUtilities.HasConstructor(rotation, WorldStateMetadataName) ? RotationConstructor.WorldState
                : SourceGenUtilities.HasConstructor(rotation, BossModuleMetadataName, WorldStateMetadataName) ? RotationConstructor.ModuleAndWorldState
                : RotationConstructor.None;
            if (!SourceGenUtilities.CanEmitClosedType(rotation) || ctorKind == RotationConstructor.None)
            {
                Report(context, rotation, "unmanaged rotation", "expected an accessible (WorldState) or (BossModule, WorldState) constructor");
                continue;
            }
            rotationModels.Add(new RotationModel(rotation, ctorKind));
        }
        rotationModels.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.TypeName, right.TypeName));

        context.AddSource("GeneratedFactories.g.cs", SourceText.From(Render(knownTypes, renderers, propertyRenderers, rotationModels), Encoding.UTF8));
    }

    // Extension declarations are named-type symbols, but cannot legally appear in a typeof expression (for example extension(Clockspot)).
    private static bool CanEmitTypeOf(INamedTypeSymbol type) => !type.IsImplicitlyDeclared && !type.IsExtension && SourceGenUtilities.CanEmitClosedType(type);

    private static string Render(IReadOnlyList<INamedTypeSymbol> knownTypes, IReadOnlyList<INamedTypeSymbol> renderers, IReadOnlyList<INamedTypeSymbol> propertyRenderers, IReadOnlyList<RotationModel> rotations)
    {
        var countK = knownTypes.Count;
        var sb = new StringBuilder(Math.Max(1024, countK * 112));
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace BossMod;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class GeneratedFactories");
        sb.AppendLine("{");
        sb.AppendLine("    internal static partial bool TryCreateStrategyRenderer(global::System.Type type, out global::BossMod.Autorotation.IStrategyRenderer renderer)");
        sb.AppendLine("    {");
        var countR = renderers.Count;
        for (var i = 0; i < countR; ++i)
        {
            var typeName = SourceGenUtilities.TypeName(renderers[i]);
            sb.Append("        if (type == typeof(").Append(typeName).AppendLine("))");
            sb.AppendLine("        {");
            sb.Append("            renderer = new ").Append(typeName).AppendLine("();");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }
        sb.AppendLine("        renderer = null!;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    internal static partial bool TryCreatePropertyRenderer(global::System.Type type, out global::BossMod.PropertyRenderer renderer)");
        sb.AppendLine("    {");
        var countPR = propertyRenderers.Count;
        for (var i = 0; i < countPR; ++i)
        {
            var typeName = SourceGenUtilities.TypeName(propertyRenderers[i]);
            sb.Append("        if (type == typeof(").Append(typeName).AppendLine("))");
            sb.AppendLine("        {");
            sb.Append("            renderer = new ").Append(typeName).AppendLine("();");
            sb.AppendLine("            return true;");
            sb.AppendLine("        }");
        }
        sb.AppendLine("        renderer = null!;");
        sb.AppendLine("        return false;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    internal static partial R CreateUnmanagedRotation<R>(BossModule module)");
        sb.AppendLine("        where R : global::BossMod.QuestBattle.UnmanagedRotation");
        sb.AppendLine("    {");
        var countRot = rotations.Count;
        for (var i = 0; i < countRot; ++i)
        {
            var rotation = rotations[i];
            var typeName = rotation.TypeName;
            sb.Append("        if (typeof(R) == typeof(").Append(typeName).AppendLine("))");
            sb.Append("            return global::System.Runtime.CompilerServices.Unsafe.As<R>(new ").Append(typeName).Append(rotation.Constructor == RotationConstructor.WorldState ? "(module.WorldState));" : "(module, module.WorldState));").AppendLine();
        }
        sb.AppendLine("        throw new global::System.ArgumentException($\"No generated unmanaged rotation factory for {typeof(R).FullName}\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    private static partial void RegisterTypes(global::System.Collections.Generic.Dictionary<string, global::System.Type> types)");
        sb.AppendLine("    {");
        var chunks = (countK + ChunkSize - 1) / ChunkSize;
        for (var chunk = 0; chunk < chunks; ++chunk)
        {
            sb.Append("        RegisterTypesChunk").Append(chunk).AppendLine("(types);");
        }
        sb.AppendLine("    }");
        for (var chunk = 0; chunk < chunks; ++chunk)
        {
            sb.AppendLine();
            sb.Append("    private static void RegisterTypesChunk").Append(chunk).AppendLine("(global::System.Collections.Generic.Dictionary<string, global::System.Type> types)");
            sb.AppendLine("    {");
            var end = Math.Min(countK, (chunk + 1) * ChunkSize);
            for (var i = chunk * ChunkSize; i < end; ++i)
            {
                var t = knownTypes[i];
                var typeName = SourceGenUtilities.TypeName(t);
                sb.Append("        types.Add(").Append(SourceGenUtilities.PrimitiveLiteral(SourceGenUtilities.RuntimeFullName(t)))
                    .Append(", typeof(").Append(typeName).AppendLine("));");
            }
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void Report(SourceProductionContext context, ISymbol symbol, string registry, string reason)
        => context.ReportDiagnostic(Diagnostic.Create(InvalidFactory, SourceGenUtilities.FirstSourceLocation(symbol), SourceGenUtilities.TypeName((ITypeSymbol)symbol), registry, reason));

    private sealed class RotationModel(INamedTypeSymbol type, RotationConstructor constructor)
    {
        public readonly INamedTypeSymbol Type = type;
        public readonly string TypeName = SourceGenUtilities.TypeName(type);
        public readonly RotationConstructor Constructor = constructor;
    }

    private enum RotationConstructor { None, WorldState, ModuleAndWorldState }
}
