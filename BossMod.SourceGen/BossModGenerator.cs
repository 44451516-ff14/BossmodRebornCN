using Microsoft.CodeAnalysis;

namespace BossMod.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class BossModGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declaredTypes = SourceGenUtilities.DeclaredTypes(context);
        var allDeclaredTypes = declaredTypes.Collect();
        RegistryGenerator.Register(context, declaredTypes);
        StrategyGenerator.Register(context, declaredTypes);
        ConfigGenerator.Register(context, declaredTypes);
        EnumMetadataGenerator.Register(context, declaredTypes);
        FactoryGenerator.Register(context, allDeclaredTypes);
    }
}
