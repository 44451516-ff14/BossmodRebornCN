; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
BMSG000 | BossMod.SourceGen | Error | MissingFrameworkSymbol
BMSG001 | BossMod.SourceGen | Error | InvalidBossModule
BMSG002 | BossMod.SourceGen | Error | InvalidZoneModule
BMSG003 | BossMod.SourceGen | Error | InvalidRotationModule
BMSG004 | BossMod.SourceGen | Error | InvalidRegistration
BMSG005 | BossMod.SourceGen | Warning | DuplicateRegistration
BMSG006 | BossMod.SourceGen | Info | InferredMetadataFallback
BMSG007 | BossMod.SourceGen | Error | InvalidBossComponent
BMSG100 | BossMod.SourceGen | Error | MissingStrategySymbol
BMSG101 | BossMod.SourceGen | Error | InvalidStrategy
BMSG200 | BossMod.SourceGen | Error | MissingConfigSymbol
BMSG201 | BossMod.SourceGen | Error | InvalidConfig
BMSG300 | BossMod.SourceGen | Error | MissingEnumSymbol
BMSG400 | BossMod.SourceGen | Error | MissingFactorySymbol
BMSG401 | BossMod.SourceGen | Error | InvalidFactory
