using CoverageReporter.Core;

namespace CoverageReporter.Core.Tests;

public class CoverageMetricsTests
{
    [Test]
    public void PercentagesAreCalculatedFromCoveredAndTotalCounts()
    {
        var metrics = new CoverageMetrics(2, 3, 1, 4);

        Assert.Equals(metrics.LineCoveragePercentage, 200m / 3);
        Assert.Equals(metrics.BranchCoveragePercentage, 25m);
    }

    [Test]
    public void EmptyCountsHaveNoPercentage()
    {
        var metrics = new CoverageMetrics(0, 0, 0, 0);

        Assert.Equals(metrics.LineCoveragePercentage, null);
        Assert.Equals(metrics.BranchCoveragePercentage, null);
    }
}