using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed partial class RegistryGenerator : IIncrementalGenerator
{
    private const string Category = "BossMod.SourceGen";
    private const int BossChunkSize = 32;
    private const int DefaultChunkSize = 96;
    private const string BossModuleMetadataName = "BossMod.BossModule";
    private const string StateMachineBuilderMetadataName = "BossMod.StateMachineBuilder";
    private const string WorldStateMetadataName = "BossMod.WorldState";
    private const string ActorMetadataName = "BossMod.Actor";
    private const string ModuleInfoAttributeMetadataName = "BossMod.ModuleInfoAttribute";
    private const string MaturityMetadataName = "BossMod.BossModuleInfo+Maturity";
    private const string ExpansionMetadataName = "BossMod.BossModuleInfo+Expansion";
    private const string CategoryMetadataName = "BossMod.BossModuleInfo+Category";
    private const string GroupTypeMetadataName = "BossMod.BossModuleInfo+GroupType";
    private const string ZoneModuleMetadataName = "BossMod.ZoneModule";
    private const string ZoneModuleInfoAttributeMetadataName = "BossMod.ZoneModuleInfoAttribute";
    private const string RotationModuleMetadataName = "BossMod.Autorotation.RotationModule";
    private const string RotationModuleManagerMetadataName = "BossMod.Autorotation.RotationModuleManager";
    private const string RotationModuleDefinitionMetadataName = "BossMod.Autorotation.RotationModuleDefinition";
    private const string ActionDefinitionsMetadataName = "BossMod.Defs";
    private const string ConfigNodeMetadataName = "BossMod.ConfigNode";
    private const string BossComponentMetadataName = "BossMod.BossComponent";
    private const string DemoModuleMetadataName = "BossMod.DemoModule";

    private static readonly DiagnosticDescriptor MissingFrameworkSymbol = new(
        "BMSG000",
        "BossMod source generation could not start",
        "Required symbol '{0}' was not found",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidBossModule = new(
        "BMSG001",
        "Boss module cannot be source generated",
        "Boss module '{0}' was skipped: {1}",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidZoneModule = new(
        "BMSG002",
        "Zone module cannot be source generated",
        "Zone module '{0}' was skipped: {1}",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidRotationModule = new(
        "BMSG003",
        "Rotation module cannot be source generated",
        "Rotation module '{0}' was skipped: {1}",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidRegistration = new(
        "BMSG004",
        "Type cannot be source generated",
        "Type '{0}' was not added to the generated {1}: {2}",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor DuplicateRegistration = new(
        "BMSG005",
        "Duplicate generated registry key",
        "{0} '{1}' and '{2}' use the same key {3}; runtime registration keeps the first one",
        Category,
        DiagnosticSeverity.Warning,
        true);

    private static readonly DiagnosticDescriptor InferredMetadataFallback = new(
        "BMSG006",
        "Boss module metadata could not be inferred",
        "Boss module '{0}' has no valid inferred {1}; generated metadata uses {2}",
        Category,
        DiagnosticSeverity.Info,
        true);

    private static readonly DiagnosticDescriptor InvalidBossComponent = new(
        "BMSG007",
        "Boss component cannot be source generated",
        "Boss component '{0}' cannot be activated without reflection: {1}",
        Category,
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => Register(context, SourceGenUtilities.DeclaredTypes(context));

    internal static void Register(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<INamedTypeSymbol> declaredTypes)
    {
        // Registries only consume classes. Project Compilation to a small, value-equatable framework state before combining it with source symbols
        var classTypes = declaredTypes
            .Where(static type => type.TypeKind == TypeKind.Class)
            .Select(static (type, _) => RegistryTypeSnapshot.Create(type))
            .Collect();
        var frameworkState = context.CompilationProvider
            .Select(static (compilation, _) => RegistryFrameworkState.Create(compilation));

        context.RegisterImplementationSourceOutput(classTypes.Combine(frameworkState), static (productionContext, value) =>
        {
            if (value.Right.MissingSymbols != 0)
            {
                ReportMissingSymbols(productionContext, value.Right.MissingSymbols);
                return;
            }
            GenerateSnapshots(productionContext, value.Left, value.Right);
        });
    }

    private static int GetMissingRequiredSymbols(Compilation compilation)
    {
        var result = 0;
        if (compilation.GetTypeByMetadataName(BossModuleMetadataName) == null)
        {
            result |= 1 << 0;
        }
        if (compilation.GetTypeByMetadataName(StateMachineBuilderMetadataName) == null)
        {
            result |= 1 << 1;
        }
        if (compilation.GetTypeByMetadataName(WorldStateMetadataName) == null)
        {
            result |= 1 << 2;
        }
        if (compilation.GetTypeByMetadataName(ActorMetadataName) == null)
        {
            result |= 1 << 3;
        }
        if (compilation.GetTypeByMetadataName(ModuleInfoAttributeMetadataName) == null)
        {
            result |= 1 << 4;
        }
        if (compilation.GetTypeByMetadataName(MaturityMetadataName) == null)
        {
            result |= 1 << 5;
        }
        if (compilation.GetTypeByMetadataName(ExpansionMetadataName) == null)
        {
            result |= 1 << 6;
        }
        if (compilation.GetTypeByMetadataName(CategoryMetadataName) == null)
        {
            result |= 1 << 7;
        }
        if (compilation.GetTypeByMetadataName(GroupTypeMetadataName) == null)
        {
            result |= 1 << 8;
        }
        if (compilation.GetTypeByMetadataName(ZoneModuleMetadataName) == null)
        {
            result |= 1 << 9;
        }
        if (compilation.GetTypeByMetadataName(ZoneModuleInfoAttributeMetadataName) == null)
        {
            result |= 1 << 10;
        }
        if (compilation.GetTypeByMetadataName(RotationModuleMetadataName) == null)
        {
            result |= 1 << 11;
        }
        if (compilation.GetTypeByMetadataName(RotationModuleManagerMetadataName) == null)
        {
            result |= 1 << 12;
        }
        if (compilation.GetTypeByMetadataName(RotationModuleDefinitionMetadataName) == null)
        {
            result |= 1 << 13;
        }
        if (compilation.GetTypeByMetadataName(ActionDefinitionsMetadataName) == null)
        {
            result |= 1 << 14;
        }
        if (compilation.GetTypeByMetadataName(ConfigNodeMetadataName) == null)
        {
            result |= 1 << 15;
        }
        if (compilation.GetTypeByMetadataName(BossComponentMetadataName) == null)
        {
            result |= 1 << 16;
        }
        return result;
    }

    private static void ReportMissingSymbols(SourceProductionContext context, int missing)
    {
        if ((missing & (1 << 0)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, BossModuleMetadataName));
        }
        if ((missing & (1 << 1)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, StateMachineBuilderMetadataName));
        }
        if ((missing & (1 << 2)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, WorldStateMetadataName));
        }
        if ((missing & (1 << 3)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ActorMetadataName));
        }
        if ((missing & (1 << 4)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ModuleInfoAttributeMetadataName));
        }
        if ((missing & (1 << 5)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, MaturityMetadataName));
        }
        if ((missing & (1 << 6)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ExpansionMetadataName));
        }
        if ((missing & (1 << 7)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, CategoryMetadataName));
        }
        if ((missing & (1 << 8)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, GroupTypeMetadataName));
        }
        if ((missing & (1 << 9)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ZoneModuleMetadataName));
        }
        if ((missing & (1 << 10)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ZoneModuleInfoAttributeMetadataName));
        }
        if ((missing & (1 << 11)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, RotationModuleMetadataName));
        }
        if ((missing & (1 << 12)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, RotationModuleManagerMetadataName));
        }
        if ((missing & (1 << 13)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, RotationModuleDefinitionMetadataName));
        }
        if ((missing & (1 << 14)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ActionDefinitionsMetadataName));
        }
        if ((missing & (1 << 15)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, ConfigNodeMetadataName));
        }
        if ((missing & (1 << 16)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingFrameworkSymbol, Location.None, BossComponentMetadataName));
        }
    }

    private static void AddSource<T>(SourceProductionContext context, string hintName, int initialCapacity, IReadOnlyList<T> items, Action<StringBuilder, IReadOnlyList<T>> renderBody)
    {
        var source = new StringBuilder(initialCapacity);
        source.AppendLine("// <auto-generated />");
        source.AppendLine("#nullable enable");
        source.AppendLine();
        source.AppendLine("namespace BossMod;");
        source.AppendLine();
        source.AppendLine("internal static partial class GeneratedRegistries");
        source.AppendLine("{");
        renderBody(source, items);
        source.AppendLine("}");
        context.AddSource(hintName, SourceText.From(source.ToString(), Encoding.UTF8));
    }

    private static int InitialCapacity(int itemCount, int estimatedCharactersPerItem)
    {
        var estimate = (long)itemCount * estimatedCharactersPerItem;
        const long capacityLong = 4L * 1024L * 1024L;
        const int capacityInt = 4 * 1024 * 1024;
        if (estimate < 1024L)
        {
            return 1024;
        }
        return estimate > capacityLong ? capacityInt : (int)estimate;
    }

    private static void RenderChunkDispatcher(StringBuilder source, string methodName, string parameter, string argumentName, int itemCount, int chunkSize)
    {
        source.Append("    internal static partial void ").Append(methodName).Append('(').Append(parameter).AppendLine(")");
        source.AppendLine("    {");
        for (var chunk = 0; chunk * chunkSize < itemCount; ++chunk)
        {
            source.Append("        ").Append(methodName).Append(chunk).Append('(').Append(argumentName).AppendLine(");");
        }
        source.AppendLine("    }");
        source.AppendLine();
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string expectedBaseMetadataName)
        => SourceGenUtilities.InheritsFrom(type, expectedBaseMetadataName);

    private static bool HasPrePullHints(INamedTypeSymbol type)
    {
        for (var current = type; current is not null && !SourceGenUtilities.HasMetadataName(current, BossModuleMetadataName); current = current.BaseType)
        {
            var members = current.GetMembers("PrePullHints");
            var len = members.Length;
            for (var i = 0; i < len; ++i)
            {
                if (members[i] is IPropertySymbol { IsOverride: true })
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsAssignableTo(INamedTypeSymbol source, string destinationMetadataName)
    {
        if (SourceGenUtilities.HasMetadataName(source, destinationMetadataName))
        {
            return true;
        }

        for (var current = source.BaseType; current is not null; current = current.BaseType)
        {
            if (SourceGenUtilities.HasMetadataName(current, destinationMetadataName))
            {
                return true;
            }
        }

        var interfaces = source.AllInterfaces;
        var len = interfaces.Length;
        for (var i = 0; i < len; ++i)
        {
            if (SourceGenUtilities.HasMetadataName(interfaces[i], destinationMetadataName))
            {
                return true;
            }
        }
        return false;
    }

    private static bool IsAssignableTo(INamedTypeSymbol source, ITypeSymbol destination)
    {
        if (SymbolEqualityComparer.Default.Equals(source, destination))
        {
            return true;
        }
        for (var current = source.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, destination))
            {
                return true;
            }
        }
        var interfaces = source.AllInterfaces;
        var len = interfaces.Length;
        for (var i = 0; i < len; ++i)
        {
            if (SymbolEqualityComparer.Default.Equals(interfaces[i], destination))
            {
                return true;
            }
        }
        return false;
    }

    private static int InheritanceDepth(INamedTypeSymbol type, string baseTypeMetadataName)
    {
        var depth = 0;
        for (var current = type; current is not null; current = current.BaseType, ++depth)
        {
            if (SourceGenUtilities.HasMetadataName(current, baseTypeMetadataName))
            {
                return depth;
            }
        }
        return -1;
    }

    private static void StableSortModuleTypes(List<INamedTypeSymbol> moduleTypes, string bossModuleMetadataName)
    {
        var count = moduleTypes.Count;
        for (var i = 1; i < count; ++i)
        {
            var moduleType = moduleTypes[i];
            var j = i - 1;
            while (j >= 0 && CompareModuleTypes(moduleType, moduleTypes[j], bossModuleMetadataName) < 0)
            {
                moduleTypes[j + 1] = moduleTypes[j];
                --j;
            }
            moduleTypes[j + 1] = moduleType;
        }
    }

    private static int CompareModuleTypes(INamedTypeSymbol left, INamedTypeSymbol right, string bossModuleMetadataName)
    {
        var byDepth = InheritanceDepth(right, bossModuleMetadataName).CompareTo(InheritanceDepth(left, bossModuleMetadataName));
        return byDepth != 0 ? byDepth : string.Compare(TypeName(left), TypeName(right), StringComparison.Ordinal);
    }

    private static bool HasApplicableSingleArgumentConstructor(INamedTypeSymbol type, INamedTypeSymbol argumentType)
    {
        var constructors = type.InstanceConstructors;
        var len = constructors.Length;
        for (var i = 0; i < len; ++i)
        {
            var constructor = constructors[i];
            if (IsAccessibleMember(constructor.DeclaredAccessibility) && constructor.Parameters.Length == 1 && constructor.Parameters[0].RefKind == RefKind.None
                && IsAssignableTo(argumentType, constructor.Parameters[0].Type))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasDefinitionMethod(INamedTypeSymbol type, string expectedReturnTypeMetadataName)
    {
        var members = type.GetMembers("Definition");
        var len = members.Length;
        for (var i = 0; i < len; ++i)
        {
            if (members[i] is IMethodSymbol method && method.IsStatic && method.DeclaredAccessibility == Accessibility.Public
                && method.Arity == 0 && method.Parameters.Length == 0 && method.ReturnType is INamedTypeSymbol returnType
                && SourceGenUtilities.HasMetadataName(returnType, expectedReturnTypeMetadataName))
            {
                return true;
            }
        }
        return false;
    }

    private static bool CanReference(INamedTypeSymbol type)
        => SourceGenUtilities.CanEmitClosedType(type);

    private static bool IsAccessibleMember(Accessibility accessibility)
        => accessibility is Accessibility.Public or Accessibility.Internal or Accessibility.ProtectedOrInternal;

    private static bool IsOpenGeneric(INamedTypeSymbol type)
    {
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.Arity != 0)
            {
                return true;
            }
        }
        return false;
    }

    private static INamedTypeSymbol? NamedTypeArgument(AttributeData? attribute, string name)
    {
        if (attribute is null)
        {
            return null;
        }

        var args = attribute.NamedArguments;
        var len = args.Length;
        for (var i = 0; i < len; ++i)
        {
            var argument = args[i];
            if (argument.Key == name)
            {
                return argument.Value.Value as INamedTypeSymbol;
            }
        }
        return null;
    }

    private static INamedTypeSymbol? ConventionSibling(INamedTypeSymbol type, string siblingName)
    {
        if (type.ContainingType is { } containingType)
        {
            var nestedTypes = containingType.GetTypeMembers(siblingName);
            if (nestedTypes.Length != 0)
            {
                return nestedTypes[0];
            }
        }
        return FirstType(type.ContainingNamespace.GetTypeMembers(siblingName));
    }

    private static INamedTypeSymbol? NamespaceType(INamedTypeSymbol type, string name)
        => FirstType(type.ContainingNamespace.GetTypeMembers(name));

    private static INamedTypeSymbol? FirstType(ImmutableArray<INamedTypeSymbol> types)
        => types.Length == 0 ? null : types[0];

    private static int NamedInt32(AttributeData? attribute, string name, int fallback)
    {
        if (!TryNamedArgument(attribute, name, out var value) || value.Value is null)
        {
            return fallback;
        }
        try
        {
            return Convert.ToInt32(value.Value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static uint NamedUInt32(AttributeData? attribute, string name, uint fallback)
        => TryNamedArgument(attribute, name, out var value) ? UInt32Value(value, fallback) : fallback;

    private static string NamedString(AttributeData? attribute, string name, string fallback)
    {
        if (!TryNamedArgument(attribute, name, out var value))
        {
            return fallback;
        }
        return value.Value as string ?? fallback;
    }

    private static bool TryNamedArgument(AttributeData? attribute, string name, out TypedConstant value)
    {
        if (attribute is not null)
        {
            var args = attribute.NamedArguments;
            var len = args.Length;
            for (var i = 0; i < len; ++i)
            {
                var argument = args[i];
                if (argument.Key == name)
                {
                    value = argument.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static long Int64Value(TypedConstant value, long fallback)
    {
        if (value.Value is null)
        {
            return fallback;
        }
        try
        {
            return Convert.ToInt64(value.Value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static uint UInt32Value(TypedConstant value, uint fallback)
    {
        if (value.Value is null)
        {
            return fallback;
        }
        try
        {
            return Convert.ToUInt32(value.Value, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static long EnumValue(INamedTypeSymbol enumType, string memberName) => TryEnumValue(enumType, memberName) ?? 0L;

    private static long? TryEnumValue(INamedTypeSymbol enumType, string memberName)
    {
        var field = FindConstantField(enumType, memberName);
        if (field?.ConstantValue is null)
        {
            return null;
        }
        try
        {
            return Convert.ToInt64(field.ConstantValue, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static uint? EnumUInt32Value(INamedTypeSymbol enumType, string memberName)
    {
        var field = FindConstantField(enumType, memberName);
        if (field?.ConstantValue is null)
        {
            return null;
        }
        try
        {
            return Convert.ToUInt32(field.ConstantValue, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static IFieldSymbol? FindConstantField(INamedTypeSymbol enumType, string memberName)
    {
        var members = enumType.GetMembers(memberName);
        var len = members.Length;
        for (var i = 0; i < len; ++i)
        {
            if (members[i] is IFieldSymbol { HasConstantValue: true } field)
            {
                return field;
            }
        }
        return null;
    }

    private static string? DottedPart(string value, int requestedPart)
    {
        var part = 0;
        var start = 0;
        var len = value.Length;
        for (var i = 0; i <= len; ++i)
        {
            if (i != len && value[i] != '.')
            {
                continue;
            }
            if (part == requestedPart)
            {
                return value.Substring(start, i - start);
            }
            ++part;
            start = i + 1;
        }
        return null;
    }

    private static int FirstNumber(string name)
    {
        var start = 0;
        var len = name.Length;
        while (start < len && (name[start] < '0' || name[start] > '9'))
        {
            ++start;
        }

        if (start == len)
        {
            return 0;
        }

        var value = 0;
        for (var i = start; i < len && name[i] >= '0' && name[i] <= '9'; ++i)
        {
            var digit = name[i] - '0';
            if (value > (int.MaxValue - digit) / 10)
            {
                return 0;
            }
            value = value * 10 + digit;
        }
        return value;
    }

    private static string TypeName(INamedTypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static string UIntLiteral(uint value) => value.ToString(CultureInfo.InvariantCulture) + "u";

    // Keep the generated second-level hash switches small without creating an excessive number of resolver methods. Bucket counts are powers of two so
    // the generated runtime dispatch can use a cheap bit mask instead of modulo. With the target load of 16, ~10,000 components select 1,024 buckets.
    private const int ComponentFactoryTargetBucketLoad = 16;
    private const int ComponentFactoryMinBucketCount = 32;
    private const int ComponentFactoryMaxBucketCount = 2048;

    // Module-local component routing is worthwhile only for the small encounter namespaces that are common BossMod. Larger/ambiguous namespaces use the
    // global adaptive table instead.
    private const int ComponentFactoryMaxModuleLocalComponents = 16;
    private const int ComponentFactoryTargetModuleNamespaceBucketLoad = 4;
    private const int ComponentFactoryMinModuleNamespaceBucketCount = 16;
    private const int ComponentFactoryMaxModuleNamespaceBucketCount = 512;

    private static int ComponentFactoryBucketCount(int componentCount)
    {
        var requiredBuckets = (componentCount + ComponentFactoryTargetBucketLoad - 1) / ComponentFactoryTargetBucketLoad;
        var bucketCount = ComponentFactoryMinBucketCount;
        while (bucketCount < requiredBuckets && bucketCount < ComponentFactoryMaxBucketCount)
        {
            bucketCount <<= 1;
        }
        return bucketCount;
    }

    private static int ComponentFactoryModuleNamespaceBucketCount(int namespaceCount)
    {
        var requiredBuckets = (namespaceCount + ComponentFactoryTargetModuleNamespaceBucketLoad - 1) / ComponentFactoryTargetModuleNamespaceBucketLoad;
        var bucketCount = ComponentFactoryMinModuleNamespaceBucketCount;
        while (bucketCount < requiredBuckets && bucketCount < ComponentFactoryMaxModuleNamespaceBucketCount)
        {
            bucketCount <<= 1;
        }
        return bucketCount;
    }

    private static uint ComponentNameHash(string value)
    {
        var hash = 2166136261u;
        var len = value.Length;
        for (var i = 0; i < len; ++i)
        {
            var character = value[i];
            hash = (hash ^ character) * 16777619u;
        }
        return hash;
    }

    private static string StringLiteral(string value)
    {
        var result = new StringBuilder(value.Length + 2);
        result.Append('"');
        var len = value.Length;
        for (var i = 0; i < len; ++i)
        {
            var character = value[i];
            switch (character)
            {
                case '\\':
                    result.Append("\\\\");
                    break;
                case '"':
                    result.Append("\\\"");
                    break;
                case '\0':
                    result.Append("\\0");
                    break;
                case '\a':
                    result.Append("\\a");
                    break;
                case '\b':
                    result.Append("\\b");
                    break;
                case '\f':
                    result.Append("\\f");
                    break;
                case '\n':
                    result.Append("\\n");
                    break;
                case '\r':
                    result.Append("\\r");
                    break;
                case '\t':
                    result.Append("\\t");
                    break;
                case '\v':
                    result.Append("\\v");
                    break;
                default:
                    if (character is < ' ' or '\u2028' or '\u2029')
                    {
                        result.Append("\\u").Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        result.Append(character);
                    }
                    break;
            }
        }
        return result.Append('"').ToString();
    }

    private sealed class RegistryFrameworkState : IEquatable<RegistryFrameworkState>
    {
        private RegistryFrameworkState(int missingSymbols, string[] sourcePaths, long maturityWip, long expansionCount, long expansionGlobal,
            long categoryCount, long categoryUncategorized, long groupNone, EnumValueMap expansionValues, EnumValueMap categoryValues)
        {
            MissingSymbols = missingSymbols;
            SourcePaths = sourcePaths;
            MaturityWip = maturityWip;
            ExpansionCount = expansionCount;
            ExpansionGlobal = expansionGlobal;
            CategoryCount = categoryCount;
            CategoryUncategorized = categoryUncategorized;
            GroupNone = groupNone;
            ExpansionValues = expansionValues;
            CategoryValues = categoryValues;
        }

        public readonly int MissingSymbols;
        public readonly long MaturityWip;
        public readonly long ExpansionCount;
        public readonly long ExpansionGlobal;
        public readonly long CategoryCount;
        public readonly long CategoryUncategorized;
        public readonly long GroupNone;
        public readonly EnumValueMap ExpansionValues;
        public readonly EnumValueMap CategoryValues;
        private readonly string[] SourcePaths;

        public static RegistryFrameworkState Create(Compilation compilation)
        {
            var sourcePaths = new List<string>();
            foreach (var tree in compilation.SyntaxTrees)
            {
                sourcePaths.Add(SourceGenUtilities.SourceTreeKey(tree));
            }

            var maturity = compilation.GetTypeByMetadataName(MaturityMetadataName);
            var expansion = compilation.GetTypeByMetadataName(ExpansionMetadataName);
            var category = compilation.GetTypeByMetadataName(CategoryMetadataName);
            var groupType = compilation.GetTypeByMetadataName(GroupTypeMetadataName);
            return new RegistryFrameworkState(GetMissingRequiredSymbols(compilation), [.. sourcePaths], EnumValueOrDefault(maturity, "WIP"),
                EnumValueOrDefault(expansion, "Count"), EnumValueOrDefault(expansion, "Global"), EnumValueOrDefault(category, "Count"),
                EnumValueOrDefault(category, "Uncategorized"), EnumValueOrDefault(groupType, "None"), EnumValueMap.Create(expansion), EnumValueMap.Create(category));
        }

        public Dictionary<string, int> CreateSourcePathOrder()
        {
            var len = SourcePaths.Length;
            var result = new Dictionary<string, int>(len, StringComparer.Ordinal);

            for (var i = 0; i < len; ++i)
            {
                var s = SourcePaths[i];
                if (!result.ContainsKey(s))
                {
                    result.Add(s, i);
                }
            }
            return result;
        }

        private static long EnumValueOrDefault(INamedTypeSymbol? enumType, string memberName) => enumType is null ? 0 : EnumValue(enumType, memberName);

        public bool Equals(RegistryFrameworkState? other)
        {
            if (other is null || MissingSymbols != other.MissingSymbols || MaturityWip != other.MaturityWip || ExpansionCount != other.ExpansionCount
                || ExpansionGlobal != other.ExpansionGlobal || CategoryCount != other.CategoryCount || CategoryUncategorized != other.CategoryUncategorized
                || GroupNone != other.GroupNone || !ExpansionValues.Equals(other.ExpansionValues) || !CategoryValues.Equals(other.CategoryValues)
                || SourcePaths.Length != other.SourcePaths.Length)
            {
                return false;
            }
            var len = SourcePaths.Length;
            for (var i = 0; i < len; ++i)
            {
                if (!StringComparer.Ordinal.Equals(SourcePaths[i], other.SourcePaths[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is RegistryFrameworkState other && Equals(other);

        public override int GetHashCode()
        {
            var hash = MissingSymbols;
            hash = hash * 31 + MaturityWip.GetHashCode();
            hash = hash * 31 + ExpansionCount.GetHashCode();
            hash = hash * 31 + ExpansionGlobal.GetHashCode();
            hash = hash * 31 + CategoryCount.GetHashCode();
            hash = hash * 31 + CategoryUncategorized.GetHashCode();
            hash = hash * 31 + GroupNone.GetHashCode();
            hash = hash * 31 + ExpansionValues.GetHashCode();
            hash = hash * 31 + CategoryValues.GetHashCode();
            var len = SourcePaths.Length;
            for (var i = 0; i < len; ++i)
            {
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(SourcePaths[i]);
            }
            return hash;
        }
    }

    private sealed class EnumValueMap : IEquatable<EnumValueMap>
    {
        private EnumValueMap(string[] names, long[] values)
        {
            Names = names;
            Values = values;
        }

        private readonly string[] Names;
        private readonly long[] Values;

        public static EnumValueMap Create(INamedTypeSymbol? enumType)
        {
            if (enumType is null)
            {
                return new EnumValueMap([], []);
            }
            var names = new List<string>();
            var values = new List<long>();
            var members = enumType.GetMembers();
            var len = members.Length;
            for (var i = 0; i < len; ++i)
            {
                if (members[i] is not IFieldSymbol { HasConstantValue: true, ConstantValue: { } constant } field)
                {
                    continue;
                }
                try
                {
                    names.Add(field.Name);
                    values.Add(Convert.ToInt64(constant, CultureInfo.InvariantCulture));
                }
                catch (Exception)
                {
                }
            }
            return new EnumValueMap([.. names], [.. values]);
        }

        public long? TryGetValue(string name)
        {
            var len = Names.Length;
            for (var i = 0; i < len; ++i)
            {
                if (StringComparer.Ordinal.Equals(Names[i], name))
                {
                    return Values[i];
                }
            }
            return null;
        }

        public bool Equals(EnumValueMap? other)
        {
            var len = Names.Length;
            if (other is null || len != other.Names.Length)
            {
                return false;
            }
            for (var i = 0; i < len; ++i)
            {
                if (!StringComparer.Ordinal.Equals(Names[i], other.Names[i]) || Values[i] != other.Values[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object? obj) => obj is EnumValueMap other && Equals(other);

        public override int GetHashCode()
        {
            var len = Names.Length;
            var hash = len;
            for (var i = 0; i < len; ++i)
            {
                hash = hash * 31 + StringComparer.Ordinal.GetHashCode(Names[i]);
                hash = hash * 31 + Values[i].GetHashCode();
            }
            return hash;
        }
    }
}
