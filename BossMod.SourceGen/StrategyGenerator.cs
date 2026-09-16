using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed class StrategyGenerator : IIncrementalGenerator
{
    private const string Category = "BossMod.SourceGen";
    private const string TrackMetadataName = "BossMod.Autorotation.Track`1";
    private const string TrackAttributeMetadataName = "BossMod.Autorotation.TrackAttribute";
    private const string NumberAttributeMetadataName = "BossMod.Autorotation.NumberAttribute";
    private const string OptionAttributeMetadataName = "BossMod.Autorotation.OptionAttribute";
    private const string RendererAttributeMetadataName = "BossMod.Autorotation.RendererAttribute";
    private const string StrategyValueTrackMetadataName = "BossMod.Autorotation.StrategyValueTrack";

    private static readonly DiagnosticDescriptor MissingSymbol = new(
        "BMSG100", "Strategy source generation could not start", "Required symbol '{0}' was not found",
        Category, DiagnosticSeverity.Error, true);

    private static readonly DiagnosticDescriptor InvalidStrategy = new(
        "BMSG101", "Strategy cannot be source generated", "Strategy '{0}' cannot be source generated: {1}",
        Category, DiagnosticSeverity.Error, true);

    public void Initialize(IncrementalGeneratorInitializationContext context) => Register(context, SourceGenUtilities.DeclaredTypes(context));

    internal static void Register(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<INamedTypeSymbol> declaredTypes)
    {
        // Keep the hot source-emission path independent from the full Compilation.
        // Only structs that can actually be strategy schemas flow into Collect().
        var strategyTypes = declaredTypes.Where(static type => IsPotentialStrategy(type)).Collect();

        // We still validate required framework symbols, but collapse Compilation down
        // to a tiny value-equatable bit mask before combining it with source models.
        // Ordinary compilation changes therefore do not invalidate strategy emission.
        var missingRequiredSymbols = context.CompilationProvider.Select(static (compilation, _) => GetMissingRequiredSymbols(compilation));

        context.RegisterImplementationSourceOutput(strategyTypes.Combine(missingRequiredSymbols), static (productionContext, value) =>
        {
            if (value.Right != 0)
            {
                ReportMissingSymbols(productionContext, value.Right);
                return;
            }
            Generate(productionContext, value.Left);
        });
    }

    private static bool IsPotentialStrategy(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Struct || type.IsRefLikeType)
        {
            return false;
        }

        var members = type.GetMembers();
        var len = members.Length;
        for (var i = 0; i < len; ++i)
        {
            if (members[i] is IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false, Type: INamedTypeSymbol fieldType }
                && SourceGenUtilities.HasMetadataName(fieldType, TrackMetadataName))
            {
                return true;
            }
        }
        return false;
    }

    private static int GetMissingRequiredSymbols(Compilation compilation)
    {
        var result = 0;
        if (compilation.GetTypeByMetadataName(TrackMetadataName) is null)
        {
            result |= 1 << 0;
        }
        if (compilation.GetTypeByMetadataName(TrackAttributeMetadataName) is null)
        {
            result |= 1 << 1;
        }
        if (compilation.GetTypeByMetadataName(NumberAttributeMetadataName) is null)
        {
            result |= 1 << 2;
        }
        if (compilation.GetTypeByMetadataName(OptionAttributeMetadataName) is null)
        {
            result |= 1 << 3;
        }
        if (compilation.GetTypeByMetadataName(RendererAttributeMetadataName) is null)
        {
            result |= 1 << 4;
        }
        if (compilation.GetTypeByMetadataName(StrategyValueTrackMetadataName) is null)
        {
            result |= 1 << 5;
        }
        return result;
    }

    private static void ReportMissingSymbols(SourceProductionContext context, int missing)
    {
        if ((missing & (1 << 0)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, TrackMetadataName));
        }
        if ((missing & (1 << 1)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, TrackAttributeMetadataName));
        }
        if ((missing & (1 << 2)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, NumberAttributeMetadataName));
        }
        if ((missing & (1 << 3)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, OptionAttributeMetadataName));
        }
        if ((missing & (1 << 4)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, RendererAttributeMetadataName));
        }
        if ((missing & (1 << 5)) != 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(MissingSymbol, Location.None, StrategyValueTrackMetadataName));
        }
    }

    private static void Generate(SourceProductionContext context, System.Collections.Immutable.ImmutableArray<INamedTypeSymbol> declaredTypes)
    {
        var strategies = new List<StrategyModel>(Math.Min(declaredTypes.Length, 64));
        var types = SourceGenUtilities.DistinctTypes(declaredTypes);
        var count = types.Count;
        for (var i = 0; i < count; ++i)
        {
            var type = types[i];
            // IsPotentialStrategy already applies these checks, but keep Generate defensive in case this method is reused by a future pipeline
            if (type.TypeKind != TypeKind.Struct || type.IsRefLikeType)
            {
                continue;
            }

            var members = type.GetMembers();
            var len = members.Length;
            var fields = new List<IFieldSymbol>(len);
            var hasTrack = false;
            for (var j = 0; j < len; ++j)
            {
                if (members[j] is not IFieldSymbol field || field.IsStatic || field.IsImplicitlyDeclared)
                {
                    continue;
                }
                fields.Add(field);
                if (field.Type is INamedTypeSymbol namedFieldType && SourceGenUtilities.HasMetadataName(namedFieldType, TrackMetadataName))
                {
                    hasTrack = true;
                }
            }
            var countF = fields.Count;
            if (countF == 0 || !hasTrack)
            {
                continue;
            }

            // DeclaringSyntaxReferences are ordered consistently with the type's partial declarations, which is all we need to retain field order
            var treeOrder = new Dictionary<SyntaxTree, int>();
            var declaringSyntax = type.DeclaringSyntaxReferences;
            var lenS = declaringSyntax.Length;
            for (var j = 0; j < lenS; ++j)
            {
                var s = declaringSyntax[j].SyntaxTree;
                if (!treeOrder.ContainsKey(s))
                {
                    treeOrder.Add(s, treeOrder.Count);
                }
            }
            SourceGenUtilities.StableSortFieldsBySource(fields, treeOrder);

            if (!SourceGenUtilities.CanEmitClosedType(type))
            {
                Report(context, type, "the type is inaccessible or contains unbound generic parameters");
                continue;
            }

            var modelFields = new List<StrategyFieldModel>(countF);
            var valid = true;

            for (var j = 0; j < countF; ++j)
            {
                var field = fields[j];
                if (field.Type is not INamedTypeSymbol fieldType || !SourceGenUtilities.HasMetadataName(fieldType, TrackMetadataName) || fieldType.TypeArguments.Length != 1)
                {
                    Report(context, field, $"field '{field.Name}' is not a Track<T>");
                    valid = false;
                    continue;
                }
                if (!SourceGenUtilities.CanAssign(field))
                {
                    Report(context, field, $"field '{field.Name}' is not accessible and writable from generated code");
                    valid = false;
                    continue;
                }

                var valueType = fieldType.TypeArguments[0];
                var kind = valueType.TypeKind == TypeKind.Enum ? StrategyFieldKind.Enum
                    : valueType.SpecialType == SpecialType.System_Single ? StrategyFieldKind.Float
                    : valueType.SpecialType == SpecialType.System_Int64 ? StrategyFieldKind.Int
                    : StrategyFieldKind.Unsupported;
                if (kind == StrategyFieldKind.Unsupported)
                {
                    Report(context, field, $"field '{field.Name}' uses unsupported value type '{SourceGenUtilities.TypeName(valueType)}'");
                    valid = false;
                    continue;
                }

                modelFields.Add(new StrategyFieldModel(field, valueType, kind));
            }

            if (valid)
            {
                strategies.Add(new StrategyModel(type, modelFields));
            }
        }

        strategies.Sort(static (left, right) => StringComparer.Ordinal.Compare(left.TypeName, right.TypeName));
        context.AddSource("GeneratedStrategies.g.cs", SourceText.From(Render(strategies), Encoding.UTF8));
    }

    private static string Render(IReadOnlyList<StrategyModel> strategies)
    {
        var fieldCount = 0;
        var count = strategies.Count;
        for (var i = 0; i < count; ++i)
        {
            fieldCount += strategies[i].Fields.Count;
        }
        var sb = new StringBuilder(Math.Max(1024, fieldCount * 900));
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("namespace BossMod.Autorotation;");
        sb.AppendLine();
        sb.AppendLine("internal static partial class GeneratedStrategies");
        sb.AppendLine("{");
        sb.AppendLine("    internal static partial void AddStrategies<S>(RotationModuleDefinition definition) where S : struct");
        sb.AppendLine("    {");
        for (var i = 0; i < count; ++i)
        {
            sb.Append("        if (typeof(S) == typeof(").Append(strategies[i].TypeName).AppendLine("))");
            sb.AppendLine("        {");
            sb.Append("            Add").Append(i).AppendLine("(definition);");
            sb.AppendLine("            return;");
            sb.AppendLine("        }");
        }
        sb.AppendLine("        throw new global::System.ArgumentException($\"No generated strategy schema for {typeof(S).FullName}\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    internal static partial T ConvertValues<T>(StrategyValues values) where T : struct");
        sb.AppendLine("    {");
        for (var i = 0; i < count; ++i)
        {
            var strategy = strategies[i];
            var strategyType = strategy.TypeName;
            sb.Append("        if (typeof(T) == typeof(").Append(strategyType).AppendLine("))");
            sb.AppendLine("        {");
            sb.Append("            if (values.Values.Length != ").Append(strategy.Fields.Count).AppendLine(")");
            sb.Append("                throw new global::System.ArgumentException($\"Strategy value count mismatch for ").Append(strategyType).AppendLine(": {values.Values.Length}\");");
            sb.Append("            var converted = new ").Append(strategyType).AppendLine("();");
            var countF = strategy.Fields.Count;
            for (var f = 0; f < countF; ++f)
            {
                var field = strategy.Fields[f];
                var fieldName = SourceGenUtilities.EscapeIdentifier(field.Field.Name);
                if (field.Kind == StrategyFieldKind.Enum)
                {
                    var valueType = SourceGenUtilities.TypeName(field.ValueType);
                    sb.Append("            var track").Append(f).Append(" = (StrategyValueTrack)values.Values[").Append(f).AppendLine("]; ");
                    sb.Append("            converted.").Append(fieldName).Append(" = new Track<").Append(valueType).Append(">((").Append(valueType).Append(")track").Append(f)
                        .Append(".Option, track").Append(f).Append(", ((StrategyConfigTrack)values.Configs[").Append(f).Append("]).Options[track").Append(f).AppendLine(".Option].DefaultPriority);");
                }
                else if (field.Kind == StrategyFieldKind.Float)
                {
                    sb.Append("            var number").Append(f).Append(" = (StrategyValueFloat)values.Values[").Append(f).AppendLine("]; ");
                    sb.Append("            converted.").Append(fieldName).Append(" = new Track<float>(number").Append(f).Append(".Value, number").Append(f).AppendLine(", float.NaN);");
                }
                else
                {
                    sb.Append("            var number").Append(f).Append(" = (StrategyValueInt)values.Values[").Append(f).AppendLine("]; ");
                    sb.Append("            converted.").Append(fieldName).Append(" = new Track<long>(number").Append(f).Append(".Value, number").Append(f).AppendLine(", float.NaN);");
                }
            }
            sb.Append("            return global::System.Runtime.CompilerServices.Unsafe.BitCast<").Append(strategyType).AppendLine(", T>(converted);");
            sb.AppendLine("        }");
        }
        sb.AppendLine("        throw new global::System.ArgumentException($\"No generated strategy converter for {typeof(T).FullName}\");");
        sb.AppendLine("    }");

        for (var i = 0; i < count; ++i)
        {
            sb.AppendLine();
            sb.Append("    private static void Add").Append(i).AppendLine("(RotationModuleDefinition definition)");
            sb.AppendLine("    {");
            var fields = strategies[i].Fields;
            var countSF = fields.Count;
            for (var fieldIndex = 0; fieldIndex < countSF; ++fieldIndex)
            {
                RenderField(sb, fields[fieldIndex], fieldIndex);
            }
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void RenderField(StringBuilder sb, StrategyFieldModel field, int fieldIndex)
    {
        var localSuffix = fieldIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var nameLiteral = SourceGenUtilities.PrimitiveLiteral(field.Field.Name);
        if (field.Kind == StrategyFieldKind.Enum && field.ValueType is INamedTypeSymbol enumType)
        {
            var trackAttr = SourceGenUtilities.Attribute(field.Field, TrackAttributeMetadataName);
            sb.Append("        var trackInfo_").Append(localSuffix).Append(" = ");
            if (trackAttr is null)
            {
                sb.Append("new TrackAttribute()");
            }
            else
            {
                SourceGenUtilities.AppendAttributeExpression(sb, trackAttr);
            }
            sb.AppendLine(";");
            var rendererAttr = SourceGenUtilities.Attribute(enumType, RendererAttributeMetadataName);
            sb.Append("        var trackCfg_").Append(localSuffix).Append(" = new StrategyConfigTrack(typeof(").Append(SourceGenUtilities.TypeName(enumType)).Append("), trackInfo_").Append(localSuffix)
                .Append(".InternalName ?? ").Append(nameLiteral).Append(", trackInfo_").Append(localSuffix).Append(".DisplayName ?? ").Append(nameLiteral).Append(", trackInfo_").Append(localSuffix)
                .Append(".UiPriority, trackInfo_").Append(localSuffix).Append(".Renderer ?? ");
            if (rendererAttr is null)
            {
                sb.Append("typeof(TrackRenderer)");
            }
            else
            {
                SourceGenUtilities.AppendAttributeExpression(sb, rendererAttr);
                sb.Append(".Type");
            }
            sb.AppendLine(");");
            sb.Append("        trackCfg_").Append(localSuffix).Append(".AssociatedActions.AddRange(trackInfo_").Append(localSuffix).AppendLine(".ActionIDs);");

            var symbols = enumType.GetMembers();
            var len = symbols.Length;
            var members = new List<IFieldSymbol>(len);
            for (var i = 0; i < len; ++i)
            {
                if (symbols[i] is IFieldSymbol member && member.HasConstantValue)
                {
                    members.Add(member);
                }
            }
            members.Sort(static (left, right) =>
            {
                var byValue = SourceGenUtilities.EnumRawValue(left).CompareTo(SourceGenUtilities.EnumRawValue(right));
                return byValue != 0 ? byValue : StringComparer.Ordinal.Compare(left.Name, right.Name);
            });
            var optionIndex = 0;
            var countM = members.Count;
            for (var i = 0; i < countM; ++i)
            {
                var member = members[i];
                var optionAttr = SourceGenUtilities.Attribute(member, OptionAttributeMetadataName);
                sb.Append("        var option_").Append(localSuffix).Append('_').Append(optionIndex).Append(" = ");
                if (optionAttr is null)
                {
                    sb.Append("new OptionAttribute()");
                }
                else
                {
                    SourceGenUtilities.AppendAttributeExpression(sb, optionAttr);
                }
                sb.AppendLine(";");
                sb.Append("        trackCfg_").Append(localSuffix).Append(".Options.Add(new(").Append(SourceGenUtilities.PrimitiveLiteral(member.Name)).Append(", option_").Append(localSuffix).Append('_').Append(optionIndex).AppendLine(".DisplayName ?? \"\")");
                sb.AppendLine("        {");
                sb.Append("            Cooldown = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".Cooldown, trackInfo_").Append(localSuffix).AppendLine(".Cooldown, 0),");
                sb.Append("            Effect = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".Effect, trackInfo_").Append(localSuffix).AppendLine(".Effect, 0),");
                sb.Append("            SupportedTargets = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".Targets, trackInfo_").Append(localSuffix).AppendLine(".Targets, ActionTargets.None),");
                sb.Append("            MinLevel = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".MinLevel, trackInfo_").Append(localSuffix).AppendLine(".MinLevel, 1),");
                sb.Append("            MaxLevel = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".MaxLevel, trackInfo_").Append(localSuffix).AppendLine(".MaxLevel, int.MaxValue),");
                sb.Append("            DefaultPriority = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).Append(".DefaultPriority, trackInfo_").Append(localSuffix).AppendLine(".DefaultPriority, ActionQueue.Priority.Medium),");
                sb.Append("            Context = definition.NonDefault(option_").Append(localSuffix).Append('_').Append(optionIndex).AppendLine(".Context, StrategyContext.All),");
                sb.Append("            Color = option_").Append(localSuffix).Append('_').Append(optionIndex).AppendLine(".Color");
                sb.AppendLine("        });");
                ++optionIndex;
            }
            sb.Append("        definition.Configs.Add(trackCfg_").Append(localSuffix).AppendLine(");");
        }
        else
        {
            var numberAttr = SourceGenUtilities.Attribute(field.Field, NumberAttributeMetadataName);
            sb.Append("        var numberInfo_").Append(localSuffix).Append(" = ");
            if (numberAttr is null)
            {
                sb.Append("new NumberAttribute()");
            }
            else
            {
                SourceGenUtilities.AppendAttributeExpression(sb, numberAttr);
            }
            sb.AppendLine(";");
            if (field.Kind == StrategyFieldKind.Float)
            {
                sb.Append("        definition.Configs.Add(new StrategyConfigFloat(").Append(nameLiteral).Append(", numberInfo_").Append(localSuffix).Append(".DisplayName, numberInfo_").Append(localSuffix)
                    .Append(".MinValue, numberInfo_").Append(localSuffix).Append(".MaxValue, numberInfo_").Append(localSuffix).Append(".UiPriority, numberInfo_").Append(localSuffix)
                    .Append(".Renderer ?? typeof(FloatRenderer), numberInfo_").Append(localSuffix).Append(".Slider, numberInfo_").Append(localSuffix).AppendLine(".Speed));");
            }
            else
            {
                sb.Append("        definition.Configs.Add(new StrategyConfigInt(").Append(nameLiteral).Append(", numberInfo_").Append(localSuffix).Append(".DisplayName, (long)numberInfo_").Append(localSuffix)
                    .Append(".MinValue, (long)numberInfo_").Append(localSuffix).Append(".MaxValue, numberInfo_").Append(localSuffix).Append(".UiPriority, numberInfo_").Append(localSuffix)
                    .Append(".Renderer ?? typeof(IntRenderer), numberInfo_").Append(localSuffix).Append(".Slider, numberInfo_").Append(localSuffix).AppendLine(".Speed));");
            }
        }
    }

    private static void Report(SourceProductionContext context, ISymbol symbol, string reason)
    {
        var type = symbol as ITypeSymbol ?? symbol.ContainingType!;
        context.ReportDiagnostic(Diagnostic.Create(InvalidStrategy, SourceGenUtilities.FirstSourceLocation(symbol), SourceGenUtilities.TypeName(type), reason));
    }

    private sealed class StrategyModel(INamedTypeSymbol type, List<StrategyFieldModel> fields)
    {
        public readonly INamedTypeSymbol Type = type;
        public readonly string TypeName = SourceGenUtilities.TypeName(type);
        public readonly List<StrategyFieldModel> Fields = fields;
    }

    private sealed class StrategyFieldModel(IFieldSymbol field, ITypeSymbol valueType, StrategyFieldKind kind)
    {
        public readonly IFieldSymbol Field = field;
        public readonly ITypeSymbol ValueType = valueType;
        public readonly StrategyFieldKind Kind = kind;
    }

    private enum StrategyFieldKind { Unsupported, Enum, Float, Int }
}
