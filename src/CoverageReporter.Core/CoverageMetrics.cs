namespace CoverageReporter.Core;

public sealed record CoverageMetrics(int LinesCovered, int LinesValid, int BranchesCovered, int BranchesValid)
{
    public decimal? LineCoveragePercentage => GetPercentage(LinesCovered, LinesValid);

    public decimal? BranchCoveragePercentage => GetPercentage(BranchesCovered, BranchesValid);

    private static decimal? GetPercentage(int covered, int total) =>
        total == 0 ? null : (decimal)covered * 100 / total;
}