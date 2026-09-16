using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace BossMod.SourceGen;

public sealed partial class RegistryGenerator
{
    // Symbol-free registry pipeline. All semantic questions are answered while a single declaration's symbol is available; Collect() retains only value data.
    private static void GenerateSnapshots(SourceProductionContext context, ImmutableArray<RegistryTypeSnapshot> snapshots, RegistryFrameworkState frameworkState)
    {
        var sourceOrder = frameworkState.CreateSourcePathOrder();
        var len = snapshots.Length;
        var all = new List<RegistryTypeSnapshot>(len);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < len; ++i)
        {
            var snapshot = snapshots[i];
            if (seen.Add(snapshot.TypeName))
            {
                all.Add(snapshot);
            }
        }
        all.Sort((left, right) => CompareSnapshotSource(left, right, sourceOrder));

        var bossModules = new List<BossSnapshot>(1024);
        var zoneModules = new List<ZoneSnapshot>(128);
        var rotationModules = new List<SimpleTypeSnapshot>(64);
        var actionDefinitions = new List<SimpleTypeSnapshot>(512);
        var configNodes = new List<SimpleTypeSnapshot>(256);
        var components = new List<ComponentSnapshot>(16384);

        var count = all.Count;
        for (var i = 0; i < count; ++i)
        {
            var type = all[i];
            var diagnostics = type.Diagnostics;
            var lenD = diagnostics.Length;
            for (var j = 0; j < lenD; ++j)
            {
                var diagnostic = diagnostics[j];
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptorFor(diagnostic.Kind), diagnostic.Location.ToLocation(), diagnostic.Arguments));
            }

            if (type.Boss is { } boss)
            {
                var resolved = boss.Resolve(frameworkState);
                bossModules.Add(resolved);
                if (resolved.ExpansionFallback)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InferredMetadataFallback, type.Location.ToLocation(), type.TypeName, "expansion", "Expansion.Global"));
                }
                if (resolved.CategoryFallback)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InferredMetadataFallback, type.Location.ToLocation(), type.TypeName, "category", "Category.Uncategorized"));
                }
            }
            else if (type.Zone is { } zone)
            {
                zoneModules.Add(zone);
            }
            else if (type.Rotation is { } rotation)
            {
                rotationModules.Add(rotation);
            }
            else if (type.ActionDefinition is { } actionDefinition)
            {
                actionDefinitions.Add(actionDefinition);
            }
            else if (type.ConfigNode is { } configNode)
            {
                configNodes.Add(configNode);
            }
            else if (type.Component is { } component)
            {
                components.Add(component);
            }
        }

        ReportDuplicateBossSnapshots(context, bossModules);
        ReportDuplicateZoneSnapshots(context, zoneModules);

        AddSource(context, "GeneratedRegistries.BossModules.g.cs", InitialCapacity(bossModules.Count, 1024), bossModules, RenderBossSnapshots);
        AddSource(context, "GeneratedRegistries.ZoneModules.g.cs", InitialCapacity(zoneModules.Count, 256), zoneModules, RenderZoneSnapshots);
        AddSource(context, "GeneratedRegistries.RotationModules.g.cs", InitialCapacity(rotationModules.Count, 320), rotationModules, RenderRotationSnapshots);
        AddSource(context, "GeneratedRegistries.ActionDefinitions.g.cs", InitialCapacity(actionDefinitions.Count, 128), actionDefinitions, RenderActionDefinitionSnapshots);
        AddSource(context, "GeneratedRegistries.ConfigNodes.g.cs", InitialCapacity(configNodes.Count, 160), configNodes, RenderConfigNodeSnapshots);
        AddSource(context, "GeneratedRegistries.Components.g.cs", InitialCapacity(components.Count, 512), components,
            (source, items) => RenderComponentSnapshots(source, items, bossModules));
    }

    private static DiagnosticDescriptor DiagnosticDescriptorFor(RegistryDiagnosticKind kind) => kind switch
    {
        RegistryDiagnosticKind.InvalidBossModule => InvalidBossModule,
        RegistryDiagnosticKind.InvalidZoneModule => InvalidZoneModule,
        RegistryDiagnosticKind.InvalidRotationModule => InvalidRotationModule,
        RegistryDiagnosticKind.InvalidRegistration => InvalidRegistration,
        RegistryDiagnosticKind.InvalidBossComponent => InvalidBossComponent,
        _ => InvalidRegistration,
    };

    private static int CompareSnapshotSource(RegistryTypeSnapshot left, RegistryTypeSnapshot right, IReadOnlyDictionary<string, int> sourceOrder)
    {
        var lo = sourceOrder.TryGetValue(left.SourceTreeKey, out var l) ? l : int.MaxValue;
        var ro = sourceOrder.TryGetValue(right.SourceTreeKey, out var r) ? r : int.MaxValue;
        var byTree = lo.CompareTo(ro);
        if (byTree != 0)
        {
            return byTree;
        }
        var bySpan = left.SourceSpanStart.CompareTo(right.SourceSpanStart);
        return bySpan != 0 ? bySpan : StringComparer.Ordinal.Compare(left.TypeName, right.TypeName);
    }

    private static void ReportDuplicateBossSnapshots(SourceProductionContext context, IReadOnlyList<BossSnapshot> modules)
    {
        var count = modules.Count;
        var firstByOid = new Dictionary<uint, BossSnapshot>(count);
        for (var i = 0; i < count; ++i)
        {
            var module = modules[i];
            var oid = module.PrimaryActorOid;
            if (!firstByOid.TryGetValue(oid, out var first))
            {
                firstByOid.Add(oid, module);
                continue;
            }
            context.ReportDiagnostic(Diagnostic.Create(DuplicateRegistration, module.Location.ToLocation(), "Boss modules", first.ModuleTypeName, module.ModuleTypeName, oid.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static void ReportDuplicateZoneSnapshots(SourceProductionContext context, IReadOnlyList<ZoneSnapshot> modules)
    {
        var count = modules.Count;
        var firstByCfc = new Dictionary<uint, ZoneSnapshot>(count);
        for (var i = 0; i < count; ++i)
        {
            var module = modules[i];
            var id = module.CfcId;
            if (!firstByCfc.TryGetValue(id, out var first))
            {
                firstByCfc.Add(id, module);
                continue;
            }
            context.ReportDiagnostic(Diagnostic.Create(DuplicateRegistration, module.Location.ToLocation(), "Zone modules", first.ModuleTypeName, module.ModuleTypeName, id.ToString(CultureInfo.InvariantCulture)));
        }
    }

    private static void RenderBossSnapshots(StringBuilder source, IReadOnlyList<BossSnapshot> modules)
    {
        var count = modules.Count;
        RenderChunkDispatcher(source, "RegisterBossModules", "global::System.Action<global::BossMod.BossModuleRegistry.Info> register", "register", count, BossChunkSize);
        for (var offset = 0; offset < count; offset += BossChunkSize)
        {
            var chunk = offset / BossChunkSize;
            source.Append("    private static void RegisterBossModules").Append(chunk).AppendLine("(global::System.Action<global::BossMod.BossModuleRegistry.Info> register)");
            source.AppendLine("    {");
            var end = Math.Min(offset + BossChunkSize, count);
            for (var index = offset; index < end; ++index)
            {
                var module = modules[index];
                source.AppendLine("        register(new global::BossMod.BossModuleRegistry.Info(");
                source.Append("            typeof(").Append(module.ModuleTypeName).AppendLine("),");
                source.Append("            typeof(").Append(module.StatesTypeName).AppendLine("),");
                source.Append("            ").Append(TypeOfSnapshot(module.ConfigTypeName)).AppendLine(",");
                source.Append("            ").Append(TypeOfSnapshot(module.ObjectIdTypeName)).AppendLine(",");
                source.Append("            ").Append(TypeOfSnapshot(module.ActionIdTypeName)).AppendLine(",");
                source.Append("            ").Append(TypeOfSnapshot(module.StatusIdTypeName)).AppendLine(",");
                source.Append("            ").Append(TypeOfSnapshot(module.TetherIdTypeName)).AppendLine(",");
                source.Append("            ").Append(TypeOfSnapshot(module.IconIdTypeName)).AppendLine(",");
                source.Append("            ").Append(UIntLiteral(module.PrimaryActorOid)).AppendLine(",");
                source.Append("            ").Append(module.HasPrePullHints ? "true" : "false").AppendLine(",");
                source.Append("            static (worldState, primaryActor) => new ").Append(module.ModuleTypeName).AppendLine("(worldState, primaryActor),");
                source.Append("            static module => new ").Append(module.StatesTypeName).Append("((").Append(module.ModuleTypeName).AppendLine(")module).Build(),");
                source.Append("            (global::BossMod.BossModuleInfo.Maturity)").Append(module.Maturity.ToString(CultureInfo.InvariantCulture)).AppendLine(",");
                source.Append("            ").Append(StringLiteral(module.Contributors)).AppendLine(",");
                source.Append("            (global::BossMod.BossModuleInfo.Expansion)").Append(module.Expansion.ToString(CultureInfo.InvariantCulture)).AppendLine(",");
                source.Append("            (global::BossMod.BossModuleInfo.Category)").Append(module.Category.ToString(CultureInfo.InvariantCulture)).AppendLine(",");
                source.Append("            (global::BossMod.BossModuleInfo.GroupType)").Append(module.GroupType.ToString(CultureInfo.InvariantCulture)).AppendLine(",");
                source.Append("            ").Append(UIntLiteral(module.GroupId)).AppendLine(",");
                source.Append("            ").Append(UIntLiteral(module.NameId)).AppendLine(",");
                source.Append("            ").Append(module.SortOrder.ToString(CultureInfo.InvariantCulture)).AppendLine(",");
                source.Append("            ").Append(module.PlanLevel.ToString(CultureInfo.InvariantCulture)).AppendLine("));");
            }
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    private static void RenderZoneSnapshots(StringBuilder source, IReadOnlyList<ZoneSnapshot> modules)
    {
        var count = modules.Count;
        RenderChunkDispatcher(source, "RegisterZoneModules", "global::System.Action<global::BossMod.ZoneModuleRegistry.Info> register", "register", count, DefaultChunkSize);
        for (var offset = 0; offset < count; offset += DefaultChunkSize)
        {
            var chunk = offset / DefaultChunkSize;
            source.Append("    private static void RegisterZoneModules").Append(chunk).AppendLine("(global::System.Action<global::BossMod.ZoneModuleRegistry.Info> register)");
            source.AppendLine("    {");
            var end = Math.Min(offset + DefaultChunkSize, count);
            for (var i = offset; i < end; ++i)
            {
                var module = modules[i];
                source.Append("        register(new global::BossMod.ZoneModuleRegistry.Info(typeof(").Append(module.ModuleTypeName)
                    .Append("), new global::BossMod.ZoneModuleInfoAttribute((global::BossMod.BossModuleInfo.Maturity)").Append(module.Maturity.ToString(CultureInfo.InvariantCulture))
                    .Append(", ").Append(UIntLiteral(module.CfcId)).Append(", ").Append(UIntLiteral(module.TerritoryId))
                    .Append("), static worldState => new ").Append(module.ModuleTypeName).AppendLine("(worldState)));");
            }
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    private static void RenderRotationSnapshots(StringBuilder source, IReadOnlyList<SimpleTypeSnapshot> modules)
    {
        var count = modules.Count;
        const string parameters = "global::System.Collections.Generic.Dictionary<global::System.Type, global::BossMod.Autorotation.RotationModuleRegistry.Entry> modules, global::System.Collections.Generic.Dictionary<string, global::System.Type> modulesByName";
        RenderChunkDispatcher(source, "RegisterRotationModules", parameters, "modules, modulesByName", count, DefaultChunkSize);
        for (var offset = 0; offset < count; offset += DefaultChunkSize)
        {
            var chunk = offset / DefaultChunkSize;
            source.Append("    private static void RegisterRotationModules").Append(chunk).Append('(').Append(parameters).AppendLine(")");
            source.AppendLine("    {");
            var end = Math.Min(offset + DefaultChunkSize, count);
            for (var i = offset; i < end; ++i)
            {
                var module = modules[i];
                source.Append("        modules[typeof(").Append(module.TypeName).Append(")] = new global::BossMod.Autorotation.RotationModuleRegistry.Entry(")
                    .Append(module.TypeName).Append(".Definition(), static (manager, player) => new ").Append(module.TypeName).AppendLine("(manager, player));");
                source.Append("        modulesByName[").Append(StringLiteral(module.RuntimeFullName)).Append("] = typeof(").Append(module.TypeName).AppendLine(");");
            }
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    private static void RenderActionDefinitionSnapshots(StringBuilder source, IReadOnlyList<SimpleTypeSnapshot> definitions)
    {
        const string parameter = "global::BossMod.ActionDefinitions definitions";
        var count = definitions.Count;
        RenderChunkDispatcher(source, "RegisterActionDefinitions", parameter, "definitions", count, DefaultChunkSize);
        for (var offset = 0; offset < count; offset += DefaultChunkSize)
        {
            var chunk = offset / DefaultChunkSize;
            source.Append("    private static void RegisterActionDefinitions").Append(chunk).Append('(').Append(parameter).AppendLine(")");
            source.AppendLine("    {");
            var end = Math.Min(offset + DefaultChunkSize, count);
            for (var i = offset; i < end; ++i)
            {
                source.Append("        new ").Append(definitions[i].TypeName).AppendLine("().Define(definitions);");
            }
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    private static void RenderConfigNodeSnapshots(StringBuilder source, IReadOnlyList<SimpleTypeSnapshot> nodes)
    {
        const string parameter = "global::System.Action<global::System.Type, global::BossMod.ConfigNode> register";
        var count = nodes.Count;
        RenderChunkDispatcher(source, "RegisterConfigNodes", parameter, "register", count, DefaultChunkSize);
        for (var offset = 0; offset < count; offset += DefaultChunkSize)
        {
            var chunk = offset / DefaultChunkSize;
            source.Append("    private static void RegisterConfigNodes").Append(chunk).Append('(').Append(parameter).AppendLine(")");
            source.AppendLine("    {");
            var end = Math.Min(offset + DefaultChunkSize, count);
            for (var i = offset; i < end; ++i)
            {
                source.Append("        register(typeof(").Append(nodes[i].TypeName).Append("), new ").Append(nodes[i].TypeName).AppendLine("());");
            }
            source.AppendLine("    }");
            source.AppendLine();
        }
    }

    private static void RenderComponentSnapshots(StringBuilder source, IReadOnlyList<ComponentSnapshot> components, IReadOnlyList<BossSnapshot> bossModules)
    {
        source.AppendLine("    internal static partial bool TryCreateBossComponent<T>(global::BossMod.BossModule module, out T component) where T : global::BossMod.BossComponent");
        source.AppendLine("    {");
        source.AppendLine("        var factory = ComponentFactory<T>.Factory;");
        source.AppendLine("        if (factory is not null)");
        source.AppendLine("        {");
        // ResolveComponentFactory(typeof(T)) only returns a factory for the exact requested T
        source.AppendLine("            component = global::System.Runtime.CompilerServices.Unsafe.As<T>(factory(module));");
        source.AppendLine("            return true;");
        source.AppendLine("        }");
        source.AppendLine();
        source.AppendLine("        component = default!;");
        source.AppendLine("        return false;");
        source.AppendLine("    }");
        source.AppendLine();
        source.AppendLine("    private static class ComponentFactory<T> where T : global::BossMod.BossComponent");
        source.AppendLine("    {");
        source.AppendLine("        internal static readonly global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? Factory =");
        source.AppendLine("            ResolveComponentFactory(typeof(T));");
        source.AppendLine("    }");
        source.AppendLine();

        // BossMod convention is one encounter module plus its components per namespace.
        // Exploit that locality only when the namespace has exactly one generated BossModule
        // and the local component set is small. Ambiguous, framework-wide, and unusually
        // large namespaces stay in the adaptive global hash table.
        var ownerByNamespace = new Dictionary<string, string?>(StringComparer.Ordinal);
        var countM = bossModules.Count;
        for (var i = 0; i < countM; ++i)
        {
            var moduleNamespace = bossModules[i].NamespaceName;
            if (string.IsNullOrEmpty(moduleNamespace))
            {
                continue;
            }
            if (ownerByNamespace.ContainsKey(moduleNamespace))
            {
                ownerByNamespace[moduleNamespace] = null;
            }
            else
            {
                ownerByNamespace.Add(moduleNamespace, bossModules[i].ModuleTypeName);
            }
        }

        var candidateGroups = new Dictionary<string, ComponentModuleGroupSnapshot>(StringComparer.Ordinal);
        var sharedComponents = new List<ComponentSnapshot>();
        var countComp = components.Count;
        for (var i = 0; i < countComp; ++i)
        {
            var component = components[i];
            if (!string.IsNullOrEmpty(component.NamespaceName) && ownerByNamespace.TryGetValue(component.NamespaceName, out var ownerModule) && ownerModule is not null)
            {
                if (!candidateGroups.TryGetValue(component.NamespaceName, out var group))
                {
                    group = new ComponentModuleGroupSnapshot(component.NamespaceName, ownerModule);
                    candidateGroups.Add(component.NamespaceName, group);
                }
                group.Components.Add(component);
            }
            else
            {
                sharedComponents.Add(component);
            }
        }

        var localGroups = new List<ComponentModuleGroupSnapshot>(candidateGroups.Count);

        foreach (var pair in candidateGroups)
        {
            if (pair.Value.Components.Count <= ComponentFactoryMaxModuleLocalComponents)
            {
                localGroups.Add(pair.Value);
            }
            else
            {
                sharedComponents.AddRange(pair.Value.Components);
            }
        }
        localGroups.Sort(static (a, b) => StringComparer.Ordinal.Compare(a.NamespaceName, b.NamespaceName));
        SortComponentSnapshotsByHash(sharedComponents);

        var countLG = localGroups.Count;
        source.Append("    // Component factory routing: ").Append(countLG.ToString(CultureInfo.InvariantCulture))
            .Append(" module-local namespace groups, ");
        var localComponentCount = 0;

        for (var i = 0; i < countLG; ++i)
        {
            localComponentCount += localGroups[i].Components.Count;
        }
        source.Append(localComponentCount.ToString(CultureInfo.InvariantCulture)).Append(" local components, ")
            .Append(sharedComponents.Count.ToString(CultureInfo.InvariantCulture)).AppendLine(" shared/global components.");

        var namespaceBucketCount = ComponentFactoryModuleNamespaceBucketCount(countLG);
        var namespaceBucketMask = (uint)(namespaceBucketCount - 1);
        var namespaceBuckets = new List<int>?[namespaceBucketCount];
        for (var i = 0; i < countLG; ++i)
        {
            var bucketIndex = (int)(ComponentNameHash(localGroups[i].NamespaceName) & namespaceBucketMask);
            (namespaceBuckets[bucketIndex] ??= []).Add(i);
        }
        var lenN = namespaceBuckets.Length;
        for (var i = 0; i < lenN; ++i)
        {
            namespaceBuckets[i]?.Sort((left, right) =>
            {
                var leftN = localGroups[left].NamespaceName;
                var rightN = localGroups[right].NamespaceName;
                var leftHash = ComponentNameHash(leftN);
                var rightHash = ComponentNameHash(rightN);
                var byHash = leftHash.CompareTo(rightHash);
                return byHash != 0 ? byHash : StringComparer.Ordinal.Compare(leftN, rightN);
            });
        }

        source.AppendLine("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ResolveComponentFactory(global::System.Type type)");
        source.AppendLine("    {");
        if (countLG != 0)
        {
            source.AppendLine("        if (type.Namespace is { } componentNamespace)");
            source.AppendLine("        {");
            source.AppendLine("            var namespaceHash = ComponentNameHash(componentNamespace);");
            source.Append("            var localFactory = (namespaceHash & ").Append(UIntLiteral(namespaceBucketMask)).AppendLine(") switch");
            source.AppendLine("            {");
            for (var i = 0; i < lenN; ++i)
            {
                if (namespaceBuckets[i] != null)
                {
                    source.Append("                ").Append(i.ToString(CultureInfo.InvariantCulture)).Append("u => ResolveComponentNamespaceBucket")
                        .Append(i.ToString(CultureInfo.InvariantCulture)).AppendLine("(type, namespaceHash),");
                }
            }
            source.AppendLine("                _ => null,");
            source.AppendLine("            };");
            source.AppendLine("            if (localFactory != null)");
            source.AppendLine("                return localFactory;");
            source.AppendLine("        }");
        }
        source.AppendLine("        if (type.FullName is not { } fullName)");
        source.AppendLine("            return null;");
        source.AppendLine("        return ResolveSharedComponentFactory(type, ComponentNameHash(fullName));");
        source.AppendLine("    }");
        source.AppendLine();
        source.AppendLine("    private static uint ComponentNameHash(string value)");
        source.AppendLine("    {");
        source.AppendLine("        var hash = 2166136261u;");
        source.AppendLine("        var len = value.Length;");
        source.AppendLine("        for (var i = 0; i < len; ++i)");
        source.AppendLine("            hash = (hash ^ value[i]) * 16777619u;");
        source.AppendLine("        return hash;");
        source.AppendLine("    }");
        source.AppendLine();

        // First level: hash the component namespace to a small set of module-local groups.
        // The full namespace hash then selects the exact encounter namespace; only that
        // encounter's tiny component resolver is touched/JITed on the cold resolution path
        for (var bucketIndex = 0; bucketIndex < lenN; ++bucketIndex)
        {
            var bucket = namespaceBuckets[bucketIndex];
            if (bucket == null)
            {
                continue;
            }
            var bucketName = bucketIndex.ToString(CultureInfo.InvariantCulture);
            source.Append("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ResolveComponentNamespaceBucket")
                .Append(bucketName).AppendLine("(global::System.Type type, uint namespaceHash)");
            source.AppendLine("    {");
            source.AppendLine("        switch (namespaceHash)");
            source.AppendLine("        {");
            var groupStart = 0;
            var countB = bucket.Count;
            while (groupStart < countB)
            {
                var namespaceHash = ComponentNameHash(localGroups[bucket[groupStart]].NamespaceName);
                var groupEnd = groupStart + 1;
                while (groupEnd < countB && ComponentNameHash(localGroups[bucket[groupEnd]].NamespaceName) == namespaceHash)
                {
                    ++groupEnd;
                }
                source.Append("            case ").Append(UIntLiteral(namespaceHash)).AppendLine(":");
                for (var groupIndex = groupStart; groupIndex < groupEnd; ++groupIndex)
                {
                    var localGroupIndex = bucket[groupIndex];
                    source.Append("                {");
                    source.AppendLine();
                    source.Append("                    var factory = ResolveComponentModule").Append(localGroupIndex.ToString(CultureInfo.InvariantCulture)).AppendLine("(type);");
                    source.AppendLine("                    if (factory != null)");
                    source.AppendLine("                        return factory;");
                    source.AppendLine("                }");
                }
                source.AppendLine("                break;");
                groupStart = groupEnd;
            }
            source.AppendLine("        }");
            source.AppendLine("        return null;");
            source.AppendLine("    }");
            source.AppendLine();
        }

        // Second level: encounter-local groups are deliberately capped at 16 components.
        // A short RuntimeType pointer-comparison chain avoids hashing the full component
        // name on the common module-local path, and all components for an encounter share
        // the same small resolver method/JIT body.
        for (var i = 0; i < countLG; ++i)
        {
            var group = localGroups[i];
            source.Append("    // ").Append(group.ModuleTypeName).AppendLine();
            RenderLocalComponentSnapshotResolver(source, "ResolveComponentModule" + i.ToString(CultureInfo.InvariantCulture), group.Components);
        }

        RenderSharedComponentSnapshotResolver(source, sharedComponents);
    }

    private static void SortComponentSnapshotsByHash(List<ComponentSnapshot> components)
    {
        components.Sort(static (a, b) =>
        {
            var leftHash = ComponentNameHash(a.RuntimeFullName);
            var rightHash = ComponentNameHash(b.RuntimeFullName);
            var byHash = leftHash.CompareTo(rightHash);
            return byHash != 0 ? byHash : StringComparer.Ordinal.Compare(a.TypeName, b.TypeName);
        });
    }

    private static void RenderLocalComponentSnapshotResolver(StringBuilder source, string methodName, IReadOnlyList<ComponentSnapshot> components)
    {
        source.Append("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ").Append(methodName)
            .AppendLine("(global::System.Type type)");
        source.AppendLine("    {");
        var count = components.Count;
        for (var i = 0; i < count; ++i)
        {
            RenderComponentSnapshotFactory(source, components[i], "        ");
        }
        source.AppendLine("        return null;");
        source.AppendLine("    }");
        source.AppendLine();
    }

    private static void RenderComponentSnapshotResolver(StringBuilder source, string methodName, IReadOnlyList<ComponentSnapshot> components)
    {
        source.Append("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ").Append(methodName)
            .AppendLine("(global::System.Type type, uint hash)");
        source.AppendLine("    {");
        source.AppendLine("        switch (hash)");
        source.AppendLine("        {");
        var start = 0;
        var count = components.Count;
        while (start < count)
        {
            var hash = ComponentNameHash(components[start].RuntimeFullName);
            var end = start + 1;
            while (end < count && ComponentNameHash(components[end].RuntimeFullName) == hash)
            {
                ++end;
            }
            source.Append("            case ").Append(UIntLiteral(hash)).AppendLine(":");
            for (var i = start; i < end; ++i)
            {
                RenderComponentSnapshotFactory(source, components[i], "                ");
            }
            source.AppendLine("                break;");
            start = end;
        }
        source.AppendLine("        }");
        source.AppendLine("        return null;");
        source.AppendLine("    }");
        source.AppendLine();
    }

    private static void RenderSharedComponentSnapshotResolver(StringBuilder source, List<ComponentSnapshot> components)
    {
        var count = components.Count;
        if (count == 0)
        {
            source.AppendLine("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ResolveSharedComponentFactory(global::System.Type type, uint hash) => null;");
            source.AppendLine();
            return;
        }

        var bucketCount = ComponentFactoryBucketCount(count);
        var bucketMask = (uint)(bucketCount - 1);
        var buckets = new List<ComponentSnapshot>?[bucketCount];
        for (var i = 0; i < count; ++i)
        {
            var comp = components[i];
            var bucketIndex = (int)(ComponentNameHash(comp.RuntimeFullName) & bucketMask);
            (buckets[bucketIndex] ??= []).Add(comp);
        }
        for (var i = 0; i < bucketCount; ++i)
        {
            if (buckets[i] is { } bucket)
            {
                SortComponentSnapshotsByHash(bucket);
            }
        }

        source.AppendLine("    private static global::System.Func<global::BossMod.BossModule, global::BossMod.BossComponent>? ResolveSharedComponentFactory(global::System.Type type, uint hash)");
        source.AppendLine("    {");
        source.Append("        return (hash & ").Append(UIntLiteral(bucketMask)).AppendLine(") switch");
        source.AppendLine("        {");
        for (var i = 0; i < bucketCount; ++i)
        {
            if (buckets[i] != null)
            {
                source.Append("            ").Append(i.ToString(CultureInfo.InvariantCulture)).Append("u => ResolveSharedComponentFactoryBucket")
                    .Append(i.ToString(CultureInfo.InvariantCulture)).AppendLine("(type, hash),");
            }
        }
        source.AppendLine("            _ => null,");
        source.AppendLine("        };");
        source.AppendLine("    }");
        source.AppendLine();

        for (var i = 0; i < bucketCount; ++i)
        {
            if (buckets[i] is { } bucket)
            {
                RenderComponentSnapshotResolver(source, "ResolveSharedComponentFactoryBucket" + i.ToString(CultureInfo.InvariantCulture), bucket);
            }
        }
    }

    private static void RenderComponentSnapshotFactory(StringBuilder source, ComponentSnapshot component, string indent)
    {
        source.Append(indent).Append("if (type == typeof(").Append(component.TypeName).AppendLine("))");
        if (component.AcceptsAnyBossModule)
        {
            source.Append(indent).Append("    return static module => new ").Append(component.TypeName).AppendLine("(module);");
            return;
        }

        source.Append(indent).AppendLine("    return static module =>");
        source.Append(indent).AppendLine("    {");
        var len = component.ModuleTypeNames.Length;
        for (var i = 0; i < len; ++i)
        {
            source.Append(indent).Append("        if (module is ").Append(component.ModuleTypeNames[i]).Append(" typedModule")
                .Append(i.ToString(CultureInfo.InvariantCulture)).AppendLine(")");
            source.Append(indent).Append("            return new ").Append(component.TypeName).Append("(typedModule")
                .Append(i.ToString(CultureInfo.InvariantCulture)).AppendLine(");");
        }
        source.Append(indent).Append("        throw new global::System.InvalidOperationException($\"Boss component {typeof(").Append(component.TypeName)
            .AppendLine(").FullName} has no constructor compatible with module {module.GetType().FullName}\");");
        source.Append(indent).AppendLine("    };");
    }

    private sealed class ComponentModuleGroupSnapshot(string namespaceName, string moduleTypeName)
    {
        public readonly string NamespaceName = namespaceName;
        public readonly string ModuleTypeName = moduleTypeName;
        public readonly List<ComponentSnapshot> Components = [];
    }

    private static string TypeOfSnapshot(string? typeName) => typeName is null ? "null" : "typeof(" + typeName + ")";

    private sealed class RegistryTypeSnapshot : IEquatable<RegistryTypeSnapshot>
    {
        private RegistryTypeSnapshot(string typeName, string sourceTreeKey, int sourceSpanStart, in RegistryLocation location, BossCandidateSnapshot? boss, ZoneSnapshot? zone,
            SimpleTypeSnapshot? rotation, SimpleTypeSnapshot? actionDefinition, SimpleTypeSnapshot? configNode, ComponentSnapshot? component, RegistryDiagnosticSnapshot[] diagnostics, string fingerprint)
        {
            TypeName = typeName;
            SourceTreeKey = sourceTreeKey;
            SourceSpanStart = sourceSpanStart;
            Location = location;
            Boss = boss;
            Zone = zone;
            Rotation = rotation;
            ActionDefinition = actionDefinition;
            ConfigNode = configNode;
            Component = component;
            Diagnostics = diagnostics;
            Fingerprint = fingerprint;
        }
        public readonly string TypeName;
        public readonly string SourceTreeKey;
        public readonly int SourceSpanStart;
        public readonly RegistryLocation Location;
        public readonly BossCandidateSnapshot? Boss;
        public readonly ZoneSnapshot? Zone;
        public readonly SimpleTypeSnapshot? Rotation;
        public readonly SimpleTypeSnapshot? ActionDefinition;
        public readonly SimpleTypeSnapshot? ConfigNode;
        public readonly ComponentSnapshot? Component;
        public readonly RegistryDiagnosticSnapshot[] Diagnostics;
        private readonly string Fingerprint;

        public static RegistryTypeSnapshot Create(INamedTypeSymbol type)
        {
            var typeName = TypeName(type);
            var runtime = SourceGenUtilities.RuntimeFullName(type);
            var loc = SourceGenUtilities.FirstSourceLocation(type);
            var rloc = RegistryLocation.From(loc);
            var treeKey = loc?.SourceTree is { } tree ? SourceGenUtilities.SourceTreeKey(tree) : string.Empty;
            var span = loc?.IsInSource == true ? loc.SourceSpan.Start : int.MaxValue;
            var diagnostics = new List<RegistryDiagnosticSnapshot>();
            BossCandidateSnapshot? boss = null;
            ZoneSnapshot? zone = null;
            SimpleTypeSnapshot? rotation = null;

            if (!type.IsAbstract && InheritsFrom(type, BossModuleMetadataName) && !SourceGenUtilities.HasMetadataName(type, DemoModuleMetadataName))
            {
                string? error = null;
                if (!CanReference(type))
                {
                    error = "the type is not accessible from generated code";
                }
                else if (!SourceGenUtilities.HasConstructor(type, WorldStateMetadataName, ActorMetadataName))
                {
                    error = "it needs an accessible constructor accepting (WorldState, Actor)";
                }
                var attribute = SourceGenUtilities.Attribute(type, ModuleInfoAttributeMetadataName);
                var states = NamedTypeArgument(attribute, "StatesType") ?? ConventionSibling(type, type.Name + "States");
                if (error == null && (states == null || states.IsAbstract || !InheritsFrom(states, StateMachineBuilderMetadataName)))
                {
                    error = "the associated states type is missing, abstract, or does not derive from StateMachineBuilder";
                }
                if (error == null && states != null && !CanReference(states))
                {
                    error = $"states type '{TypeName(states)}' is not accessible from generated code";
                }
                if (error == null && states != null && !HasApplicableSingleArgumentConstructor(states, type))
                {
                    error = $"states type '{TypeName(states)}' needs an accessible constructor accepting the module (or one of its base types)";
                }

                var cfg = NamedTypeArgument(attribute, "ConfigType") ?? ConventionSibling(type, type.Name + "Config");
                var oid = NamedTypeArgument(attribute, "ObjectIDType") ?? NamespaceType(type, "OID");
                var aid = NamedTypeArgument(attribute, "ActionIDType") ?? NamespaceType(type, "AID");
                var sid = NamedTypeArgument(attribute, "StatusIDType") ?? NamespaceType(type, "SID");
                var tid = NamedTypeArgument(attribute, "TetherIDType") ?? NamespaceType(type, "TetherID");
                var iid = NamedTypeArgument(attribute, "IconIDType") ?? NamespaceType(type, "IconID");
                if (error == null)
                {
                    var a = new[] { cfg, oid, aid, sid, tid, iid };
                    for (var i = 0; i < 6; ++i)
                    {
                        var mt = a[i];
                        if (mt != null && !CanReference(mt))
                        {
                            error = $"metadata type '{TypeName(mt)}' is not accessible from generated code";
                            break;
                        }
                    }
                }
                if (error is not null)
                {
                    diagnostics.Add(RegistryDiagnosticSnapshot.InvalidBoss(rloc, typeName, error));
                }
                else if (states is not null)
                {
                    var primary = NamedUInt32(attribute, "PrimaryActorOID", 0u);
                    if (primary == 0 && oid is not null)
                    {
                        primary = EnumUInt32Value(oid, "Boss") ?? 0u;
                    }
                    long? maturity = attribute is not null && attribute.ConstructorArguments.Length > 0 ? Int64Value(attribute.ConstructorArguments[0], 0L) : null;
                    boss = new BossCandidateSnapshot(typeName, TypeName(states), NameOrNull(cfg), NameOrNull(oid), NameOrNull(aid), NameOrNull(sid), NameOrNull(tid), NameOrNull(iid), primary, HasPrePullHints(type), maturity,
                        NamedString(attribute, "Contributors", string.Empty), TryNamedInt64(attribute, "Expansion"), TryNamedInt64(attribute, "Category"), TryNamedInt64(attribute, "GroupType"),
                        NamedUInt32(attribute, "GroupID", 0u), NamedUInt32(attribute, "NameID", 0u), NamedInt32(attribute, "SortOrder", 0), NamedInt32(attribute, "PlanLevel", 0), type.Name, type.ContainingNamespace.ToDisplayString(), rloc);
                }
            }

            if (!type.IsAbstract && InheritsFrom(type, ZoneModuleMetadataName))
            {
                var attr = SourceGenUtilities.Attribute(type, ZoneModuleInfoAttributeMetadataName);
                string? error = null;
                if (!CanReference(type))
                {
                    error = "the type is not accessible from generated code";
                }
                else if (attr is null || attr.ConstructorArguments.Length < 2)
                {
                    error = "it needs a ZoneModuleInfo attribute";
                }
                else if (!SourceGenUtilities.HasConstructor(type, WorldStateMetadataName))
                {
                    error = "it needs an accessible constructor accepting WorldState";
                }
                if (error is not null)
                {
                    diagnostics.Add(RegistryDiagnosticSnapshot.InvalidZone(rloc, typeName, error));
                }
                else
                {
                    zone = new ZoneSnapshot(typeName, Int64Value(attr!.ConstructorArguments[0], 0), UInt32Value(attr.ConstructorArguments[1], 0), attr.ConstructorArguments.Length > 2 ? UInt32Value(attr.ConstructorArguments[2], 0) : 0, rloc);
                }
            }

            if (!type.IsAbstract && InheritsFrom(type, RotationModuleMetadataName))
            {
                string? error = null;
                if (!CanReference(type))
                {
                    error = "the type is not accessible from generated code";
                }
                else if (!SourceGenUtilities.HasConstructor(type, RotationModuleManagerMetadataName, ActorMetadataName))
                {
                    error = "it needs an accessible constructor accepting (RotationModuleManager, Actor)";
                }
                else if (!HasDefinitionMethod(type, RotationModuleDefinitionMetadataName))
                {
                    error = "it needs a public static Definition() method returning RotationModuleDefinition";
                }
                if (error != null)
                {
                    diagnostics.Add(RegistryDiagnosticSnapshot.InvalidRotation(rloc, typeName, error));
                }
                else
                {
                    rotation = new SimpleTypeSnapshot(typeName, runtime);
                }
            }

            var action = CreateSimple(type, typeName, runtime, ActionDefinitionsMetadataName, "action-definition registry", rloc, diagnostics);

            var config = CreateSimple(type, typeName, runtime, ConfigNodeMetadataName, "config registry", rloc, diagnostics);

            var component = CreateComponent(type, typeName, runtime, rloc, diagnostics);
            var fp = BuildFingerprint(typeName, treeKey, span, boss, zone, rotation, action, config, component, diagnostics);
            return new RegistryTypeSnapshot(typeName, treeKey, span, rloc, boss, zone, rotation, action, config, component, [.. diagnostics], fp);
        }

        private static SimpleTypeSnapshot? CreateSimple(INamedTypeSymbol type, string typeName, string runtime, string baseName, string registryName, in RegistryLocation loc, List<RegistryDiagnosticSnapshot> diagnostics)
        {
            if (type.IsAbstract || IsOpenGeneric(type) || !InheritsFrom(type, baseName))
            {
                return null;
            }
            string? error = null;
            if (!CanReference(type))
            {
                error = "the type is not accessible from generated code";
            }
            else if (!SourceGenUtilities.HasConstructor(type))
            {
                error = "it needs a parameterless constructor";
            }
            if (error != null)
            {
                diagnostics.Add(RegistryDiagnosticSnapshot.InvalidRegistration(loc, typeName, registryName, error));
                return null;
            }
            return new SimpleTypeSnapshot(typeName, runtime);
        }

        private static ComponentSnapshot? CreateComponent(INamedTypeSymbol type, string typeName, string runtime, in RegistryLocation loc, List<RegistryDiagnosticSnapshot> diagnostics)
        {
            if (type.IsAbstract || IsOpenGeneric(type) || !InheritsFrom(type, BossComponentMetadataName))
            {
                return null;
            }
            var ctors = type.InstanceConstructors;
            var compatible = false;
            var len = ctors.Length;
            for (var i = 0; i < len; ++i)
            {
                var ctor = ctors[i];
                if (ctor.Parameters.Length == 1 && ctor.Parameters[0].RefKind == RefKind.None && ctor.Parameters[0].Type is INamedTypeSymbol mt && IsAssignableTo(mt, BossModuleMetadataName))
                {
                    compatible = true;
                    break;
                }
            }
            if (!compatible)
            {
                return null;
            }
            if (!CanReference(type))
            {
                diagnostics.Add(RegistryDiagnosticSnapshot.InvalidComponent(loc, typeName, "the type is not accessible from generated code"));
                return null;
            }
            var usable = new List<INamedTypeSymbol>();
            INamedTypeSymbol? exact = null;
            for (var i = 0; i < len; ++i)
            {
                var c = ctors[i];
                if (c.Parameters.Length != 1 || c.Parameters[0].RefKind != RefKind.None || c.Parameters[0].Type is not INamedTypeSymbol mt || !IsAssignableTo(mt, BossModuleMetadataName) || !IsAccessibleMember(c.DeclaredAccessibility) || !CanReference(mt))
                {
                    continue;
                }
                usable.Add(mt);
                if (SourceGenUtilities.HasMetadataName(mt, BossModuleMetadataName))
                {
                    exact = mt;
                }
            }
            var countU = usable.Count;
            if (countU == 0)
            {
                diagnostics.Add(RegistryDiagnosticSnapshot.InvalidComponent(loc, typeName, "none of its BossModule-compatible constructors are accessible from generated code"));
                return null;
            }
            if (exact != null)
            {
                return new ComponentSnapshot(typeName, runtime, type.ContainingNamespace.ToDisplayString(), [TypeName(exact)], true);
            }
            StableSortModuleTypes(usable, BossModuleMetadataName);
            var names = new string[countU];
            for (var i = 0; i < countU; ++i)
            {
                names[i] = TypeName(usable[i]);
            }
            return new ComponentSnapshot(typeName, runtime, type.ContainingNamespace.ToDisplayString(), names, false);
        }

        private static string? NameOrNull(INamedTypeSymbol? type) => type is null ? null : TypeName(type);
        private static long? TryNamedInt64(AttributeData? attribute, string name) => TryNamedArgument(attribute, name, out var value) ? Int64Value(value, 0) : null;
        private static string BuildFingerprint(string typeName, string tree, int span, BossCandidateSnapshot? boss, ZoneSnapshot? zone, SimpleTypeSnapshot? rotation, SimpleTypeSnapshot? action, SimpleTypeSnapshot? config, ComponentSnapshot? component, List<RegistryDiagnosticSnapshot> diagnostics)
        {
            var sb = new StringBuilder(512).Append(typeName).Append('|').Append(tree).Append('|').Append(span).Append('|').Append(boss?.Fingerprint).Append('|').Append(zone?.Fingerprint).Append('|').Append(rotation?.Fingerprint).Append('|').Append(action?.Fingerprint).Append('|').Append(config?.Fingerprint).Append('|').Append(component?.Fingerprint);

            var count = diagnostics.Count;
            for (var i = 0; i < count; ++i)
            {
                sb.Append('|').Append(diagnostics[i].Fingerprint);
            }
            return sb.ToString();
        }
        public bool Equals(RegistryTypeSnapshot? other) => other is not null && Fingerprint == other.Fingerprint;
        public override bool Equals(object? obj) => obj is RegistryTypeSnapshot other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Fingerprint);
    }

    private sealed class BossCandidateSnapshot(string module, string states, string? config, string? oid, string? aid, string? sid, string? tid, string? iid, uint primary, bool hasPrePullHints, long? maturity, string contributors, long? expansion, long? category, long? groupType, uint groupId, uint nameId, int sortOrder, int planLevel, string simpleName, string ns, in RegistryLocation loc)
    {
        public readonly string ModuleTypeName = module;
        public readonly string StatesTypeName = states;
        public readonly string? ConfigTypeName = config;
        public readonly string? ObjectIdTypeName = oid;
        public readonly string? ActionIdTypeName = aid;
        public readonly string? StatusIdTypeName = sid;
        public readonly string? TetherIdTypeName = tid;
        public readonly string? IconIdTypeName = iid;
        public readonly uint PrimaryActorOid = primary;
        public readonly bool HasPrePullHints = hasPrePullHints;
        public readonly long? Maturity = maturity;
        public readonly string Contributors = contributors;
        public readonly long? Expansion = expansion;
        public readonly long? Category = category;
        public readonly long? GroupType = groupType;
        public readonly uint GroupId = groupId;
        public readonly uint NameId = nameId;
        public readonly int SortOrder = sortOrder;
        public readonly int PlanLevel = planLevel;
        private readonly string SimpleName = simpleName;
        private readonly string NamespaceName = ns;
        private readonly RegistryLocation Location = loc;
        public string Fingerprint => $"{ModuleTypeName}|{StatesTypeName}|{ConfigTypeName}|{ObjectIdTypeName}|{ActionIdTypeName}|{StatusIdTypeName}|{TetherIdTypeName}|{IconIdTypeName}|{PrimaryActorOid}|{HasPrePullHints}|{Maturity}|{Contributors}|{Expansion}|{Category}|{GroupType}|{GroupId}|{NameId}|{SortOrder}|{PlanLevel}";
        public BossSnapshot Resolve(RegistryFrameworkState fw)
        {
            var maturity = Maturity ?? fw.MaturityWip;
            var expansion = Expansion ?? fw.ExpansionCount;
            var ef = false;
            if (expansion == fw.ExpansionCount)
            {
                var n = DottedPart(NamespaceName, 1);
                var v = n == null ? null : fw.ExpansionValues.TryGetValue(n);
                ef = v == null;
                expansion = v ?? fw.ExpansionGlobal;
            }
            var category = Category ?? fw.CategoryCount;
            var cf = false;
            if (category == fw.CategoryCount)
            {
                var n = DottedPart(NamespaceName, 2);
                var v = n == null ? null : fw.CategoryValues.TryGetValue(n);
                cf = v == null;
                category = v ?? fw.CategoryUncategorized;
            }
            var sort = SortOrder;
            if (sort == 0)
            {
                sort = FirstNumber(SimpleName);
            }
            if (sort == 0)
            {
                sort = (int)PrimaryActorOid;
            }
            return new BossSnapshot(ModuleTypeName, StatesTypeName, ConfigTypeName, ObjectIdTypeName, ActionIdTypeName, StatusIdTypeName, TetherIdTypeName, IconIdTypeName, PrimaryActorOid, HasPrePullHints, maturity, Contributors, expansion, category, GroupType ?? fw.GroupNone, GroupId, NameId, sort, PlanLevel, NamespaceName, Location, ef, cf);
        }
    }

    private sealed class BossSnapshot(string module, string states, string? config, string? oid, string? aid, string? sid, string? tid, string? iid, uint primary, bool hasPrePullHints, long maturity, string contributors, long expansion, long category, long groupType, uint groupId, uint nameId, int sortOrder, int planLevel, string ns, in RegistryLocation loc, bool ef, bool cf)
    {
        public readonly string ModuleTypeName = module;
        public readonly string StatesTypeName = states;
        public readonly string? ConfigTypeName = config;
        public readonly string? ObjectIdTypeName = oid;
        public readonly string? ActionIdTypeName = aid;
        public readonly string? StatusIdTypeName = sid;
        public readonly string? TetherIdTypeName = tid;
        public readonly string? IconIdTypeName = iid;
        public readonly uint PrimaryActorOid = primary;
        public readonly bool HasPrePullHints = hasPrePullHints;
        public readonly long Maturity = maturity;
        public readonly string Contributors = contributors;
        public readonly long Expansion = expansion;
        public readonly long Category = category;
        public readonly long GroupType = groupType;
        public readonly uint GroupId = groupId;
        public readonly uint NameId = nameId;
        public readonly int SortOrder = sortOrder;
        public readonly int PlanLevel = planLevel;
        public readonly string NamespaceName = ns;
        public readonly RegistryLocation Location = loc;
        public readonly bool ExpansionFallback = ef;
        public readonly bool CategoryFallback = cf;
    }

    private sealed class ZoneSnapshot(string type, long maturity, uint cfc, uint territory, in RegistryLocation loc)
    {

        public readonly string ModuleTypeName = type;
        public readonly long Maturity = maturity;
        public readonly uint CfcId = cfc;
        public readonly uint TerritoryId = territory;
        public readonly RegistryLocation Location = loc;
        public string Fingerprint => $"{ModuleTypeName}|{Maturity}|{CfcId}|{TerritoryId}";
    }

    private sealed class SimpleTypeSnapshot(string type, string runtime)
    {
        public readonly string TypeName = type;
        public readonly string RuntimeFullName = runtime;
        public string Fingerprint => TypeName + "|" + RuntimeFullName;
    }

    private sealed class ComponentSnapshot(string type, string runtime, string ns, string[] modules, bool any)
    {

        public readonly string TypeName = type;
        public readonly string RuntimeFullName = runtime;
        public readonly string NamespaceName = ns;
        public readonly string[] ModuleTypeNames = modules;
        public readonly bool AcceptsAnyBossModule = any;
        public string Fingerprint => TypeName + "|" + RuntimeFullName + "|" + NamespaceName + "|" + AcceptsAnyBossModule + "|" + string.Join(";", ModuleTypeNames);
    }

    private enum RegistryDiagnosticKind { InvalidBossModule, InvalidZoneModule, InvalidRotationModule, InvalidRegistration, InvalidBossComponent }
    private sealed class RegistryDiagnosticSnapshot
    {
        private RegistryDiagnosticSnapshot(RegistryDiagnosticKind kind, in RegistryLocation loc, object[] args)
        {
            Kind = kind;
            Location = loc;
            Arguments = args;
            Fingerprint = kind + "|" + string.Join("|", args);
        }
        public readonly RegistryDiagnosticKind Kind;
        public readonly RegistryLocation Location;
        public readonly object[] Arguments;
        public readonly string Fingerprint;
        public static RegistryDiagnosticSnapshot InvalidBoss(in RegistryLocation l, string t, string r) => new(RegistryDiagnosticKind.InvalidBossModule, l, [t, r]);
        public static RegistryDiagnosticSnapshot InvalidZone(in RegistryLocation l, string t, string r) => new(RegistryDiagnosticKind.InvalidZoneModule, l, [t, r]);
        public static RegistryDiagnosticSnapshot InvalidRotation(in RegistryLocation l, string t, string r) => new(RegistryDiagnosticKind.InvalidRotationModule, l, [t, r]);
        public static RegistryDiagnosticSnapshot InvalidRegistration(in RegistryLocation l, string t, string registry, string r) => new(RegistryDiagnosticKind.InvalidRegistration, l, [t, registry, r]);
        public static RegistryDiagnosticSnapshot InvalidComponent(in RegistryLocation l, string t, string r) => new(RegistryDiagnosticKind.InvalidBossComponent, l, [t, r]);
    }

    private readonly struct RegistryLocation : IEquatable<RegistryLocation>
    {
        private RegistryLocation(string path, TextSpan span, LinePositionSpan line, bool source)
        {
            Path = path;
            Span = span;
            LineSpan = line;
            HasSource = source;
        }
        private readonly string Path;
        private readonly TextSpan Span;
        private readonly LinePositionSpan LineSpan;
        private readonly bool HasSource;
        public static RegistryLocation From(Location? location)
        {
            if (location == null || !location.IsInSource)
            {
                return default;
            }
            var l = location.GetLineSpan();
            return new RegistryLocation(l.Path ?? string.Empty, location.SourceSpan, l.Span, true);
        }
        public Location ToLocation() => HasSource ? Location.Create(Path, Span, LineSpan) : Location.None;
        public readonly bool Equals(RegistryLocation other) => HasSource == other.HasSource && Path == other.Path && Span.Equals(other.Span) && LineSpan.Equals(other.LineSpan);
        public override readonly bool Equals(object? o) => o is RegistryLocation other && Equals(other);
        public override readonly int GetHashCode() => (((HasSource ? 1 : 0) * 31 + StringComparer.Ordinal.GetHashCode(Path ?? string.Empty)) * 31 + Span.GetHashCode()) * 31 + LineSpan.GetHashCode();
    }
}
