namespace CoverageReporter.Core;

public sealed record CoverageReport(
    string Name,
    CoverageMetrics Summary,
    IReadOnlyList<CoverageClass> Classes,
    int? PackageCount = null,
    DateTimeOffset? CoverageTimestamp = null);

public sealed record CoverageClass(
    string Name,
    string? Filename,
    CoverageMetrics Metrics,
    IReadOnlyList<CoverageLine> Lines,
    string? SourceCode);

public sealed record CoverageLine(int Number, int Hits, int BranchesCovered, int BranchesValid);