using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BossMod.SourceGen;

internal static class SourceGenUtilities
{
    private static readonly SymbolDisplayFormat FullyQualifiedTypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            & ~SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static IncrementalValuesProvider<INamedTypeSymbol> DeclaredTypes(IncrementalGeneratorInitializationContext context)
        => context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax,
                static (syntaxContext, cancellationToken) =>
                {
                    if (syntaxContext.SemanticModel.GetDeclaredSymbol(syntaxContext.Node, cancellationToken) is not INamedTypeSymbol type || type.DeclaringSyntaxReferences.Length == 0)
                    {
                        return null;
                    }

                    // A partial type is one symbol; publish it only for its first declaration to keep the shared pipeline compact
                    var first = type.DeclaringSyntaxReferences[0];
                    return ReferenceEquals(first.SyntaxTree, syntaxContext.Node.SyntaxTree) && first.Span == syntaxContext.Node.Span
                        ? type
                        : null;
                })
            .Where(static type => type is not null)
            .Select(static (type, _) => type!);

    public static List<INamedTypeSymbol> DistinctTypes(ImmutableArray<INamedTypeSymbol> types)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var len = types.Length;
        var result = new List<INamedTypeSymbol>(len);
        for (var i = 0; i < len; ++i)
        {
            var type = types[i];
            if (seen.Add(type))
            {
                result.Add(type);
            }
        }
        return result;
    }

    public static void SortTypesByName(List<INamedTypeSymbol> types)
    {
        if (types.Count < 2)
        {
            return;
        }

        // Cache display names
        var count = types.Count;
        var entries = new NamedTypeSortEntry[count];
        for (var i = 0; i < count; ++i)
        {
            var t = types[i];
            entries[i] = new NamedTypeSortEntry(t, TypeName(t), i);
        }
        Array.Sort(entries, static (left, right) =>
        {
            var byName = StringComparer.Ordinal.Compare(left.Name, right.Name);
            return byName != 0 ? byName : left.OriginalIndex.CompareTo(right.OriginalIndex);
        });
        var len = entries.Length;
        for (var i = 0; i < len; ++i)
        {
            types[i] = entries[i].Type;
        }
    }

    public static void StableSortFieldsBySource(List<IFieldSymbol> fields, IReadOnlyDictionary<SyntaxTree, int> syntaxTreeOrder)
    {
        var count = fields.Count;
        for (var i = 1; i < count; ++i)
        {
            var field = fields[i];
            var j = i - 1;
            while (j >= 0 && CompareSourcePosition(field, fields[j], syntaxTreeOrder) < 0)
            {
                fields[j + 1] = fields[j];
                --j;
            }
            fields[j + 1] = field;
        }
    }

    public static void StableSortFieldsBySourcePath(List<IFieldSymbol> fields, IReadOnlyDictionary<string, int> sourcePathOrder)
    {
        var count = fields.Count;
        for (var i = 1; i < count; ++i)
        {
            var field = fields[i];
            var j = i - 1;
            while (j >= 0 && CompareSourcePositionByPath(field, fields[j], sourcePathOrder) < 0)
            {
                fields[j + 1] = fields[j];
                --j;
            }
            fields[j + 1] = field;
        }
    }

    private static int CompareSourcePosition(ISymbol left, ISymbol right, IReadOnlyDictionary<SyntaxTree, int> syntaxTreeOrder)
    {
        var byTree = SourceOrder(left, syntaxTreeOrder).CompareTo(SourceOrder(right, syntaxTreeOrder));
        return byTree != 0 ? byTree : SourceSpanStart(left).CompareTo(SourceSpanStart(right));
    }

    private static int CompareSourcePositionByPath(ISymbol left, ISymbol right, IReadOnlyDictionary<string, int> sourcePathOrder)
    {
        var byTree = SourceOrderByPath(left, sourcePathOrder).CompareTo(SourceOrderByPath(right, sourcePathOrder));
        return byTree != 0 ? byTree : SourceSpanStart(left).CompareTo(SourceSpanStart(right));
    }

    public static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, baseType))
            {
                return true;
            }
        }
        return false;
    }

    public static bool InheritsFrom(INamedTypeSymbol type, string baseTypeMetadataName)
    {
        for (var current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (HasMetadataName(current.OriginalDefinition, baseTypeMetadataName))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Implements(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
    {
        var interfaces = type.AllInterfaces;
        var len = interfaces.Length;
        for (var i = 0; i < len; ++i)
        {
            if (SymbolEqualityComparer.Default.Equals(interfaces[i].OriginalDefinition, interfaceType))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Implements(INamedTypeSymbol type, string interfaceMetadataName)
    {
        var interfaces = type.AllInterfaces;
        var len = interfaces.Length;
        for (var i = 0; i < len; ++i)
        {
            if (HasMetadataName(interfaces[i], interfaceMetadataName))
            {
                return true;
            }
        }
        return false;
    }

    public static bool CanReference(ISymbol symbol)
    {
        for (var current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingType)
        {
            if (current is INamedTypeSymbol namedType && (namedType.IsFileLocal || namedType.IsExtension)
                || current.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
            {
                return false;
            }
        }
        return true;
    }

    public static bool CanEmitClosedType(INamedTypeSymbol type)
    {
        if (!CanReference(type))
        {
            return false;
        }
        for (var current = type; current is not null; current = current.ContainingType)
        {
            if (current.Arity != 0)
            {
                return false;
            }
        }
        return true;
    }

    public static bool CanAssign(IFieldSymbol field)
        => !field.IsReadOnly && !field.IsConst && CanReference(field);

    public static bool HasConstructor(INamedTypeSymbol type) => HasConstructor(type, 0, null, null);

    public static bool HasConstructor(INamedTypeSymbol type, ITypeSymbol parameter) => HasConstructor(type, 1, parameter, null);

    public static bool HasConstructor(INamedTypeSymbol type, ITypeSymbol firstParameter, ITypeSymbol secondParameter) => HasConstructor(type, 2, firstParameter, secondParameter);

    public static bool HasConstructor(INamedTypeSymbol type, string parameterMetadataName) => HasConstructorByMetadataName(type, parameterMetadataName, null);

    public static bool HasConstructor(INamedTypeSymbol type, string firstParameterMetadataName, string secondParameterMetadataName) => HasConstructorByMetadataName(type, firstParameterMetadataName, secondParameterMetadataName);

    private static bool HasConstructorByMetadataName(INamedTypeSymbol type, string firstParameterMetadataName, string? secondParameterMetadataName)
    {
        var parameterCount = secondParameterMetadataName is null ? 1 : 2;
        var consts = type.InstanceConstructors;
        var len = consts.Length;
        for (var i = 0; i < len; ++i)
        {
            var ctor = consts[i];
            if (ctor.IsStatic || ctor.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal || ctor.Parameters.Length != parameterCount)
            {
                continue;
            }
            if (ctor.Parameters[0].RefKind != RefKind.None || ctor.Parameters[0].Type is not INamedTypeSymbol firstParameter || !HasMetadataName(firstParameter, firstParameterMetadataName))
            {
                continue;
            }
            if (parameterCount > 1 && (ctor.Parameters[1].RefKind != RefKind.None || ctor.Parameters[1].Type is not INamedTypeSymbol secondParameter || !HasMetadataName(secondParameter, secondParameterMetadataName!)))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    private static bool HasConstructor(INamedTypeSymbol type, int parameterCount, ITypeSymbol? firstParameter, ITypeSymbol? secondParameter)
    {
        var consts = type.InstanceConstructors;
        var len = consts.Length;
        for (var i = 0; i < len; ++i)
        {
            var ctor = consts[i];
            if (ctor.IsStatic || ctor.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal || ctor.Parameters.Length != parameterCount)
            {
                continue;
            }
            if (parameterCount > 0 && (ctor.Parameters[0].RefKind != RefKind.None || !SymbolEqualityComparer.Default.Equals(ctor.Parameters[0].Type, firstParameter)))
            {
                continue;
            }
            if (parameterCount > 1 && (ctor.Parameters[1].RefKind != RefKind.None || !SymbolEqualityComparer.Default.Equals(ctor.Parameters[1].Type, secondParameter)))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    public static AttributeData? Attribute(ISymbol symbol, INamedTypeSymbol? attributeType) => attributeType is null ? null : Attribute(symbol.GetAttributes(), attributeType);

    public static AttributeData? Attribute(ISymbol symbol, string attributeMetadataName) => Attribute(symbol.GetAttributes(), attributeMetadataName);

    public static AttributeData? Attribute(ImmutableArray<AttributeData> attributes, string attributeMetadataName)
    {
        var ats = attributes;
        var len = ats.Length;
        for (var i = 0; i < len; ++i)
        {
            var attribute = ats[i];
            if (attribute.AttributeClass is { } attributeClass && HasMetadataName(attributeClass, attributeMetadataName))
            {
                return attribute;
            }
        }
        return null;
    }

    public static AttributeData? Attribute(ImmutableArray<AttributeData> attributes, INamedTypeSymbol? attributeType)
    {
        if (attributeType is null)
        {
            return null;
        }
        var ats = attributes;
        var len = ats.Length;
        for (var i = 0; i < len; ++i)
        {
            var attribute = ats[i];
            if (SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType))
            {
                return attribute;
            }
        }
        return null;
    }

    public static void AppendAttributeExpression(StringBuilder result, AttributeData? attribute)
    {
        if (attribute?.AttributeClass is null)
        {
            result.Append("null");
            return;
        }

        result.Append("new ").Append(TypeName(attribute.AttributeClass)).Append('(');
        var lenCArgs = attribute.ConstructorArguments.Length;
        for (var i = 0; i < lenCArgs; ++i)
        {
            if (i != 0)
            {
                result.Append(", ");
            }
            AppendTypedConstant(result, attribute.ConstructorArguments[i]);
        }
        result.Append(')');
        var lenNArgs = attribute.NamedArguments.Length;
        if (lenNArgs > 0)
        {
            result.Append(" { ");
            for (var i = 0; i < lenNArgs; ++i)
            {
                if (i != 0)
                {
                    result.Append(", ");
                }
                var arg = attribute.NamedArguments[i];
                result.Append(EscapeIdentifier(arg.Key)).Append(" = ");
                AppendTypedConstant(result, arg.Value);
            }
            result.Append(" }");
        }
    }

    private static void AppendTypedConstant(StringBuilder result, TypedConstant value)
    {
        if (value.IsNull)
        {
            result.Append("null");
            return;
        }

        if (value.Kind == TypedConstantKind.Array)
        {
            var arrayType = value.Type is IArrayTypeSymbol ats ? TypeName(ats.ElementType) : "object";
            result.Append("new ").Append(arrayType).Append("[] { ");
            var len = value.Values.Length;
            for (var i = 0; i < len; ++i)
            {
                if (i != 0)
                {
                    result.Append(", ");
                }
                AppendTypedConstant(result, value.Values[i]);
            }
            result.Append(" }");
            return;
        }

        if (value.Kind == TypedConstantKind.Type && value.Value is ITypeSymbol typeValue)
        {
            result.Append("typeof(").Append(TypeName(typeValue)).Append(')');
            return;
        }

        if (value.Kind == TypedConstantKind.Enum && value.Type is not null)
        {
            result.Append('(').Append(TypeName(value.Type)).Append(')').Append(PrimitiveLiteral(value.Value));
            return;
        }

        result.Append(PrimitiveLiteral(value.Value));
    }

    public static string PrimitiveLiteral(object? value)
        => value switch
        {
            null => "null",
            string stringValue => SymbolDisplay.FormatLiteral(stringValue, true),
            char charValue => SymbolDisplay.FormatLiteral(charValue, true),
            bool boolValue => boolValue ? "true" : "false",
            byte byteValue => byteValue.ToString(CultureInfo.InvariantCulture),
            sbyte sbyteValue => sbyteValue.ToString(CultureInfo.InvariantCulture),
            short shortValue => shortValue.ToString(CultureInfo.InvariantCulture),
            ushort ushortValue => ushortValue.ToString(CultureInfo.InvariantCulture),
            int intValue => intValue == int.MinValue ? "int.MinValue" : intValue.ToString(CultureInfo.InvariantCulture),
            uint uintValue => uintValue.ToString(CultureInfo.InvariantCulture) + "u",
            long longValue => longValue == long.MinValue ? "long.MinValue" : longValue.ToString(CultureInfo.InvariantCulture) + "L",
            ulong ulongValue => ulongValue.ToString(CultureInfo.InvariantCulture) + "UL",
            float floatValue when float.IsNaN(floatValue) => "float.NaN",
            float floatValue when float.IsPositiveInfinity(floatValue) => "float.PositiveInfinity",
            float floatValue when float.IsNegativeInfinity(floatValue) => "float.NegativeInfinity",
            float floatValue => floatValue.ToString("R", CultureInfo.InvariantCulture) + "f",
            double doubleValue when double.IsNaN(doubleValue) => "double.NaN",
            double doubleValue when double.IsPositiveInfinity(doubleValue) => "double.PositiveInfinity",
            double doubleValue when double.IsNegativeInfinity(doubleValue) => "double.NegativeInfinity",
            double doubleValue => doubleValue.ToString("R", CultureInfo.InvariantCulture) + "d",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
        };

    public static string TypeName(ITypeSymbol type) => type.ToDisplayString(FullyQualifiedTypeFormat);

    public static bool HasMetadataName(INamedTypeSymbol type, string metadataName) => StringComparer.Ordinal.Equals(RuntimeFullName(type.OriginalDefinition), metadataName);

    public static string RuntimeFullName(INamedTypeSymbol type)
    {
        var result = new StringBuilder(64);
        if (!type.ContainingNamespace.IsGlobalNamespace)
        {
            result.Append(type.ContainingNamespace.ToDisplayString()).Append('.');
        }
        AppendContainingTypeNames(result, type);
        return result.ToString();
    }

    private static void AppendContainingTypeNames(StringBuilder result, INamedTypeSymbol type)
    {
        if (type.ContainingType is { } containingType)
        {
            AppendContainingTypeNames(result, containingType);
            result.Append('+');
        }
        result.Append(type.MetadataName);
    }

    public static string EscapeIdentifier(string name) => SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ? "@" + name : name;

    public static int SourceOrder(ISymbol symbol, IReadOnlyDictionary<SyntaxTree, int> syntaxTreeOrder)
    {
        if (FirstSourceLocation(symbol) is not { SourceTree: { } tree })
        {
            return int.MaxValue;
        }
        return syntaxTreeOrder.TryGetValue(tree, out var order) ? order : int.MaxValue;
    }

    public static int SourceOrderByPath(ISymbol symbol, IReadOnlyDictionary<string, int> sourcePathOrder)
    {
        if (FirstSourceLocation(symbol) is not { SourceTree: { } tree })
        {
            return int.MaxValue;
        }
        return sourcePathOrder.TryGetValue(SourceTreeKey(tree), out var order) ? order : int.MaxValue;
    }

    public static string SourceTreeKey(SyntaxTree tree)
    {
        if (!string.IsNullOrEmpty(tree.FilePath))
        {
            return tree.FilePath;
        }

        // Pathless trees are common in generator tests. Use the source checksum as a stable fallback so separate trees do not collapse to the same order key
        var checksum = tree.GetText().GetChecksum();
        var len = checksum.Length;
        var result = new StringBuilder(1 + len * 2);
        result.Append('\0');
        for (var i = 0; i < len; ++i)
        {
            result.Append(checksum[i].ToString("x2", CultureInfo.InvariantCulture));
        }
        return result.ToString();
    }

    public static int SourceSpanStart(ISymbol symbol) => FirstSourceLocation(symbol)?.SourceSpan.Start ?? int.MaxValue;

    public static Location? FirstSourceLocation(ISymbol symbol)
    {
        var locations = symbol.Locations;
        var len = locations.Length;
        for (var i = 0; i < len; ++i)
        {
            if (locations[i].IsInSource)
            {
                return locations[i];
            }
        }
        return null;
    }

    public static ulong EnumRawValue(IFieldSymbol field)
    {
        var value = field.ConstantValue;
        return value switch
        {
            sbyte v => (ulong)v,
            byte v => v,
            short v => (ulong)v,
            ushort v => v,
            int v => (ulong)v,
            uint v => v,
            long v => (ulong)v,
            ulong v => v,
            _ => 0
        };
    }

    private readonly struct NamedTypeSortEntry(INamedTypeSymbol type, string name, int originalIndex)
    {
        public readonly INamedTypeSymbol Type = type;
        public readonly string Name = name;
        public readonly int OriginalIndex = originalIndex;
    }
}
