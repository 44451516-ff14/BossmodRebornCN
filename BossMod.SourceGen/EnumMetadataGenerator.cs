using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed class EnumMetadataGenerator : IIncrementalGenerator
{
    private const string Category = "BossMod.SourceGen";
    private const int ChunkSize = 96;
    private const string PropertyDisplayMetadataName = "BossMod.PropertyDisplayAttribute";
    private const string TrackMetadataName = "BossMod.Autorotation.Track`1";
    private const string ConfigNodeMetadataName = "BossMod.ConfigNode";
    private const string NMAttributeMetadataName = "BossMod.Stormblood.Foray.NMAttribute";
    private const string DuelAttributeMetadataName = "BossMod.Shadowbringers.Foray.DuelAttribute";
    private const char UsageSeparator = '\u001F';

    private static readonly DiagnosticDescriptor MissingSymbol = new(
        "BMSG300", "Enum source generation could not start", "Required symbol '{0}' was not found",
        Category, DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
        => Register(context, SourceGenUtilities.DeclaredTypes(context));

    internal static void Register(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<INamedTypeSymbol> declaredTypes)
    {
        // Enum declarations are the only symbols needed to build metadata. Uses of those enums are projected to strings so unrelated edits to containing
        // classes do not invalidate enum emission
        var enums = declaredTypes
            .Where(static type => type.TypeKind == TypeKind.Enum && !type.IsImplicitlyDeclared)
            .Collect();
        var fieldEnumUsages = declaredTypes
            .Where(static type => type.TypeKind is TypeKind.Class or TypeKind.Struct)
            .Select(static (type, _) => EnumFieldUsageKey(type))
            .Where(static key => key.Length != 0)
            .Collect();
        var invokedEnumUsages = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => IsGenericInvocation(node),
                static (syntaxContext, cancellationToken) => InvocationEnumUsageKey(syntaxContext, cancellationToken))
            .Where(static key => key.Length != 0)
            .Collect();
        var missingPropertyDisplay = context.CompilationProvider
            .Select(static (compilation, _) => compilation.GetTypeByMetadataName(PropertyDisplayMetadataName) is null);

        var input = enums.Combine(fieldEnumUsages).Combine(invokedEnumUsages).Combine(missingPropertyDisplay);
        context.RegisterImplementationSourceOutput(input, static (productionContext, value) =>
        {
            if (value.Right)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, PropertyDisplayMetadataName));
                return;
            }
            var left = value.Left;
            Generate(productionContext, left.Left.Left, left.Left.Right, left.Right);
        });
    }

    private static string EnumFieldUsageKey(INamedTypeSymbol type)
    {
        var isConfigNode = SourceGenUtilities.InheritsFrom(type, ConfigNodeMetadataName);
        StringBuilder? result = null;
        var members = type.GetMembers();
        var len = members.Length;
        for (var i = 0; i < len; ++i)
        {
            if (members[i] is not IFieldSymbol { IsStatic: false } field)
            {
                continue;
            }

            INamedTypeSymbol? enumType = null;
            if (isConfigNode && field.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } fieldEnum)
            {
                enumType = fieldEnum;
            }
            else if (field.Type is INamedTypeSymbol constructedTrack
                && SourceGenUtilities.HasMetadataName(constructedTrack.OriginalDefinition, TrackMetadataName)
                && constructedTrack.TypeArguments.Length != 0
                && constructedTrack.TypeArguments[0] is INamedTypeSymbol { TypeKind: TypeKind.Enum } optionEnum)
                enumType = optionEnum;

            if (enumType == null)
            {
                continue;
            }
            result ??= new StringBuilder();
            if (result.Length != 0)
            {
                result.Append(UsageSeparator);
            }
            result.Append(SourceGenUtilities.RuntimeFullName(enumType));
        }
        return result?.ToString() ?? string.Empty;
    }

    private static bool IsGenericInvocation(SyntaxNode node)
        => node is InvocationExpressionSyntax
        {
            Expression: GenericNameSyntax or MemberAccessExpressionSyntax { Name: GenericNameSyntax } or MemberBindingExpressionSyntax { Name: GenericNameSyntax }
        };

    private static string InvocationEnumUsageKey(GeneratorSyntaxContext syntaxContext, System.Threading.CancellationToken cancellationToken)
    {
        if (syntaxContext.SemanticModel.GetSymbolInfo(syntaxContext.Node, cancellationToken).Symbol is not IMethodSymbol method)
        {
            return string.Empty;
        }
        StringBuilder? result = null;
        var typeArguments = method.TypeArguments;
        var len = typeArguments.Length;
        for (var i = 0; i < len; ++i)
        {
            if (typeArguments[i] is not INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType)
            {
                continue;
            }
            result ??= new StringBuilder();
            if (result.Length != 0)
            {
                result.Append(UsageSeparator);
            }
            result.Append(SourceGenUtilities.RuntimeFullName(enumType));
        }
        return result?.ToString() ?? string.Empty;
    }

    private static void Generate(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> declaredEnums, ImmutableArray<string> fieldEnumUsages, ImmutableArray<string> invokedEnumUsages)
    {
        var enums = SourceGenUtilities.DistinctTypes(declaredEnums);

        var countEAdj = enums.Count - 1;
        for (var i = countEAdj; i >= 0; --i)
        {
            var e = enums[i];
            if (e.TypeKind != TypeKind.Enum || e.IsImplicitlyDeclared || !SourceGenUtilities.CanEmitClosedType(e))
            {
                enums.RemoveAt(i);
            }
        }
        SourceGenUtilities.SortTypesByName(enums);
        var countE = enums.Count;

        var enumByRuntimeName = new Dictionary<string, INamedTypeSymbol>(countE, StringComparer.Ordinal);
        for (var i = 0; i < countE; ++i)
        {
            var e = enums[i];
            enumByRuntimeName[SourceGenUtilities.RuntimeFullName(e)] = e;
        }
        var full = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        for (var i = 0; i < countE; ++i)
        {
            var enumType = enums[i];
            if (!IsConventionID(enumType.Name) || enumType.Name == "OID")
            {
                full.Add(enumType);
            }
            if (NeedsFullMetadata(enumType))
            {
                full.Add(enumType);
            }
        }

        AddUsageKeys(full, enumByRuntimeName, fieldEnumUsages);
        AddUsageKeys(full, enumByRuntimeName, invokedEnumUsages);

        var models = new List<EnumModel>(countE);
        for (var i = 0; i < countE; ++i)
        {
            var e = enums[i];
            models.Add(BuildModel(e, full.Contains(e)));
        }
        context.AddSource("GeneratedEnumMetadata.g.cs", SourceText.From(Render(models), Encoding.UTF8));
    }

    private static void AddUsageKeys(HashSet<INamedTypeSymbol> full, IReadOnlyDictionary<string, INamedTypeSymbol> enumByRuntimeName, ImmutableArray<string> usageKeys)
    {
        var lenUK = usageKeys.Length;
        for (var keyIndex = 0; keyIndex < lenUK; ++keyIndex)
        {
            var key = usageKeys[keyIndex];
            var start = 0;
            var lenK = key.Length;
            for (var i = 0; i <= lenK; ++i)
            {
                if (i != lenK && key[i] != UsageSeparator)
                {
                    continue;
                }
                var name = key.Substring(start, i - start);
                if (enumByRuntimeName.TryGetValue(name, out var enumType))
                {
                    full.Add(enumType);
                }
                start = i + 1;
            }
        }
    }

    private static bool IsConventionID(string name) => name is "AID" or "OID" or "SID" or "IconID" or "TetherID" or "TraitID";

    private static bool NeedsFullMetadata(INamedTypeSymbol enumType)
    {
        var members = enumType.GetMembers();
        var lenM = members.Length;
        for (var i = 0; i < lenM; ++i)
        {
            if (members[i] is not IFieldSymbol field)
            {
                continue;
            }
            var attributes = field.GetAttributes();
            var lenA = attributes.Length;
            for (var j = 0; j < lenA; ++j)
            {
                if (IsMetadataAttribute(attributes[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static EnumModel BuildModel(INamedTypeSymbol type, bool full)
    {
        var symbols = type.GetMembers();
        var len = symbols.Length;
        var fields = new List<IFieldSymbol>(len);
        for (var i = 0; i < len; ++i)
        {
            if (symbols[i] is IFieldSymbol { HasConstantValue: true } field)
            {
                fields.Add(field);
            }
        }
        fields.Sort(static (left, right) =>
        {
            var byValue = SourceGenUtilities.EnumRawValue(left).CompareTo(SourceGenUtilities.EnumRawValue(right));
            return byValue != 0 ? byValue : StringComparer.Ordinal.Compare(left.Name, right.Name);
        });

        var countF = fields.Count;
        var members = new List<EnumMemberModel>(countF);
        for (var fieldIndex = 0; fieldIndex < countF; ++fieldIndex)
        {
            var field = fields[fieldIndex];
            var displayName = field.Name;
            AttributeData[] attributes = [];
            if (full)
            {
                var fieldAttributes = field.GetAttributes();
                var property = SourceGenUtilities.Attribute(fieldAttributes, PropertyDisplayMetadataName);
                var matchingAttributeCount = 0;
                var lenFA = fieldAttributes.Length;
                for (var i = 0; i < lenFA; ++i)
                {
                    if (IsMetadataAttribute(fieldAttributes[i]))
                    {
                        ++matchingAttributeCount;
                    }
                }
                if (matchingAttributeCount != 0)
                {
                    attributes = new AttributeData[matchingAttributeCount];
                    var attributeIndex = 0;
                    for (var i = 0; i < lenFA; ++i)
                    {
                        var f = fieldAttributes[i];
                        if (IsMetadataAttribute(f))
                        {
                            attributes[attributeIndex++] = f;
                        }
                    }
                }
                if (property != null && property.ConstructorArguments.Length != 0 && property.ConstructorArguments[0].Value is string configuredName)
                {
                    displayName = configuredName;
                }
            }
            members.Add(new EnumMemberModel(field, SourceGenUtilities.EnumRawValue(field), displayName, attributes));
        }
        return new EnumModel(type, type.EnumUnderlyingType!, full, members);
    }

    private static bool IsMetadataAttribute(AttributeData attribute)
        => attribute.AttributeClass is { } attributeClass && (SourceGenUtilities.HasMetadataName(attributeClass, PropertyDisplayMetadataName)
                || SourceGenUtilities.HasMetadataName(attributeClass, NMAttributeMetadataName)
                || SourceGenUtilities.HasMetadataName(attributeClass, DuelAttributeMetadataName));

    private static string Render(IReadOnlyList<EnumModel> enums)
    {
        var memberCount = 0;
        var count = enums.Count;
        for (var i = 0; i < count; ++i)
        {
            memberCount += enums[i].Members.Count;
        }
        var sb = new StringBuilder(Math.Max(1024, memberCount * 112));
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace BossMod;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class GeneratedEnumMetadata");
        sb.AppendLine("{");
        sb.AppendLine("    private static partial void Register(global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Lazy<EnumMetadata>> metadata)");
        sb.AppendLine("    {");
        var chunks = (count + ChunkSize - 1) / ChunkSize;
        for (var chunk = 0; chunk < chunks; ++chunk)
        {
            sb.Append("        RegisterChunk").Append(chunk).AppendLine("(metadata);");
        }
        sb.AppendLine("    }");

        for (var chunk = 0; chunk < chunks; ++chunk)
        {
            sb.AppendLine();
            sb.Append("    private static void RegisterChunk").Append(chunk).AppendLine("(global::System.Collections.Generic.Dictionary<global::System.Type, global::System.Lazy<EnumMetadata>> metadata)");
            sb.AppendLine("    {");
            var end = Math.Min(count, (chunk + 1) * ChunkSize);
            for (var i = chunk * ChunkSize; i < end; ++i)
            {
                var typeName = SourceGenUtilities.TypeName(enums[i].Type);
                sb.Append("        metadata.Add(typeof(").Append(typeName).Append("), new global::System.Lazy<EnumMetadata>(Create").Append(i).AppendLine("));");
            }
            sb.AppendLine("    }");
        }

        for (var i = 0; i < count; ++i)
        {
            RenderCreate(sb, i, enums[i]);
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void RenderCreate(StringBuilder sb, int index, EnumModel model)
    {
        var typeName = SourceGenUtilities.TypeName(model.Type);
        var underlying = SourceGenUtilities.TypeName(model.UnderlyingType);
        var hasDisplayNames = false;
        var hasAttributes = false;
        var count = model.Members.Count;
        for (var i = 0; i < count; ++i)
        {
            var m = model.Members[i];
            hasDisplayNames |= m.DisplayName != m.Field.Name;
            hasAttributes |= m.Attributes.Length != 0;
        }
        hasAttributes &= model.Full;
        sb.AppendLine();
        sb.Append("    private static EnumMetadata Create").Append(index).AppendLine("()");
        sb.AppendLine("        => new EnumMetadata(");
        sb.Append("            new string[] { ");
        for (var i = 0; i < count; ++i)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append(SourceGenUtilities.PrimitiveLiteral(model.Members[i].Field.Name));
        }
        sb.AppendLine(" },");
        sb.Append("            new ulong[] { ");
        for (var i = 0; i < count; ++i)
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append(model.Members[i].RawValue.ToString(CultureInfo.InvariantCulture)).Append("UL");
        }
        sb.AppendLine(" },");
        if (model.Full)
        {
            sb.Append("            new ").Append(typeName).Append("[] { ");
            for (var i = 0; i < count; ++i)
            {
                if (i != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(typeName).Append('.').Append(SourceGenUtilities.EscapeIdentifier(model.Members[i].Field.Name));
            }
            sb.AppendLine(" },");
        }
        else
        {
            sb.AppendLine("            null,");
        }
        if (model.Full && hasDisplayNames)
        {
            sb.Append("            new string[] { ");
            for (var i = 0; i < count; ++i)
            {
                if (i != 0)
                {
                    sb.Append(", ");
                }
                sb.Append(SourceGenUtilities.PrimitiveLiteral(model.Members[i].DisplayName));
            }
            sb.AppendLine(" },");
        }
        else
        {
            sb.AppendLine("            null,");
        }
        if (hasAttributes)
        {
            sb.AppendLine("            new global::System.Attribute[][]");
            sb.AppendLine("            {");
            for (var memberIndex = 0; memberIndex < count; ++memberIndex)
            {
                var member = model.Members[memberIndex];
                var ats = member.Attributes;
                var lenA = ats.Length;
                if (lenA == 0)
                {
                    sb.AppendLine("                global::System.Array.Empty<global::System.Attribute>(),");
                }
                else
                {
                    sb.Append("                new global::System.Attribute[] { ");
                    for (var attributeIndex = 0; attributeIndex < lenA; ++attributeIndex)
                    {
                        if (attributeIndex != 0)
                        {
                            sb.Append(", ");
                        }
                        SourceGenUtilities.AppendAttributeExpression(sb, ats[attributeIndex]);
                    }
                    sb.AppendLine(" },");
                }
            }
            sb.AppendLine("            },");
        }
        else
        {
            sb.AppendLine("            null,");
        }
        sb.Append("            static value => (ulong)(").Append(underlying).Append(")(").Append(typeName).AppendLine(")(object)value,");
        sb.Append("            static raw => (global::System.Enum)(object)(").Append(typeName).Append(")(").Append(underlying).AppendLine(")raw);");
    }

    private sealed class EnumModel(INamedTypeSymbol type, INamedTypeSymbol underlyingType, bool full, List<EnumMemberModel> members)
    {
        public readonly INamedTypeSymbol Type = type;
        public readonly INamedTypeSymbol UnderlyingType = underlyingType;
        public readonly bool Full = full;
        public readonly List<EnumMemberModel> Members = members;
    }

    private sealed class EnumMemberModel(IFieldSymbol field, ulong rawValue, string displayName, AttributeData[] attributes)
    {
        public readonly IFieldSymbol Field = field;
        public readonly ulong RawValue = rawValue;
        public readonly string DisplayName = displayName;
        public readonly AttributeData[] Attributes = attributes;
    }
}
