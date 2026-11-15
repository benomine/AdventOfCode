using CoverageReporter.Core;
using CoverageReporter.Markdown;

namespace CoverageReporter.Cli.Tests;

public class MarkdownCoverageReportRendererTests
{
    [Test]
    public async Task RendersExpandableSummaryAndWeightedNamespaceGroupsIncludingGeneratedClasses()
    {
        var report = new CoverageReport("Sample | Report", new CoverageMetrics(9, 17, 3, 7),
        [
            new CoverageClass("Demo.Core.Worker", "Worker.cs", new CoverageMetrics(4, 5, 1, 2), [], null),
            new CoverageClass("Demo.Core.Worker.Child", "Child.cs", new CoverageMetrics(2, 2, 1, 2), [], null),
            new CoverageClass("Demo.Core.Other", "Other.cs", new CoverageMetrics(1, 5, 0, 0), [], null),
            new CoverageClass("Demo.Core.Worker.<>c", "Worker.cs", new CoverageMetrics(1, 3, 0, 0), [], null),
            new CoverageClass("System.Text.RegularExpressions.Generated.<RegexGenerator_g>Regex_0", "Regex.cs",
                new CoverageMetrics(1, 2, 1, 3), [], null),
            new CoverageClass("GlobalType", "Global.cs", new CoverageMetrics(1, 1, 0, 0), [], null)
        ], 2, DateTimeOffset.FromUnixTimeSeconds(1794708667));

        var generatedAt = new DateTimeOffset(2026, 11, 14, 20, 0, 0, TimeSpan.FromHours(-5));
        var markdown = new MarkdownCoverageReportRenderer().Render(report, generatedAt);

        await Assert.That(markdown.Contains("# Coverage report: Sample \\| Report")).IsTrue();
        await Assert.That(markdown.Contains("<details open><summary>Coverage summary</summary>")).IsTrue();
        await Assert.That(markdown.Contains("| Lines covered | 9 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Lines uncovered | 8 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Coverable lines | 17 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Branches covered | 3 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Branches uncovered | 4 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Coverable branches | 7 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Packages | 2 |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Classes | 6 |")).IsTrue();
        await Assert.That(markdown.Contains("| Files | 5 |")).IsTrue();
        await Assert.That(markdown.Contains("| Coverage timestamp (UTC) | 2026-11-15 02:11:07 UTC |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("| Report generated (UTC) | 2026-11-15 01:00:00 UTC |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("<summary>Demo.Core — 🟡 53.33% (8/15) lines · 🟡 50% (2/4) branches (4 classes)</summary>")).IsTrue();
        await Assert.That(markdown.Contains("<summary>Generated classes (1)</summary>")).IsTrue();
        await Assert.That(markdown.Contains("Demo.Core.Worker.Child")).IsTrue();
        await Assert.That(markdown.Contains("System.Text.RegularExpressions.Generated.<RegexGenerator_g>Regex_0")).IsFalse();
        await Assert.That(markdown.Contains("System.Text.RegularExpressions.Generated.&lt;RegexGenerator\\_g&gt;Regex\\_0")).IsTrue();
        await Assert.That(markdown.Contains("(Global namespace)")).IsTrue();
        await Assert.That(markdown.Contains("1/3")).IsTrue();
    }

    [Test]
    public async Task ColorCodesDisplayedLineAndBranchPercentagesAtThresholds()
    {
        var boundaries = new[]
        {
            ("Zero", 0, 100, "🔴 0%"),
            ("BelowAmber", 4999, 10000, "🔴 49.99%"),
            ("RoundedAmber", 49999, 100000, "🟡 50%"),
            ("Amber", 50, 100, "🟡 50%"),
            ("BelowGreen", 7999, 10000, "🟡 79.99%"),
            ("RoundedGreen", 79999, 100000, "🟢 80%"),
            ("Green", 80, 100, "🟢 80%"),
            ("Complete", 100, 100, "🟢 100%")
        };
        var classes = boundaries.Select(boundary => new CoverageClass(boundary.Item1, boundary.Item1 + ".cs",
            new CoverageMetrics(boundary.Item2, boundary.Item3, 1, 10), [], null)).ToArray();
        var report = new CoverageReport("thresholds", new CoverageMetrics(79999, 100000, 1, 2), classes);

        var markdown = new MarkdownCoverageReportRenderer().Render(report);

        foreach (var (name, _, _, displayedCoverage) in boundaries)
        {
            await Assert.That(markdown.Contains($"| {name} | {name}.cs | {displayedCoverage} (", StringComparison.Ordinal)).IsTrue();
        }

        await Assert.That(markdown.Contains("🔴 10% (1/10)", StringComparison.Ordinal)).IsTrue();
        await Assert.That(markdown.Contains("🟢 80% (79999/100000)")).IsTrue();
    }

    [Test]
    public async Task EscapesTableContentAndRendersEmptyAndUnavailableCoverageClearly()
    {
        var unsafeReport = new CoverageReport("unsafe", new CoverageMetrics(1, 1, 0, 0),
            [new CoverageClass("Demo.Widget|`*_[x]&<script>", "src/File|Name.cs", new CoverageMetrics(0, 0, 0, 0), [], null)]);
        var emptyReport = new CoverageReport("empty", new CoverageMetrics(0, 0, 0, 0), []);

        var markdown = new MarkdownCoverageReportRenderer().Render(unsafeReport);
        var emptyMarkdown = new MarkdownCoverageReportRenderer().Render(emptyReport, DateTimeOffset.UnixEpoch);

        await Assert.That(markdown.Contains("Widget\\|\\`\\*\\_\\[x\\]&amp;&lt;script&gt;")).IsTrue();
        await Assert.That(markdown.Contains("src/File\\|Name.cs")).IsTrue();
        await Assert.That(markdown.Contains("| Classes | 1 |")).IsTrue();
        await Assert.That(markdown.Contains("| Files | 1 |")).IsTrue();
        await Assert.That(markdown.Contains("N/A (0/0)")).IsTrue();
        await Assert.That(emptyMarkdown.Contains("| Classes | 0 |")).IsTrue();
        await Assert.That(emptyMarkdown.Contains("| Files | 0 |")).IsTrue();
        await Assert.That(emptyMarkdown.Contains("| Packages | N/A |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(emptyMarkdown.Contains("| Coverage timestamp (UTC) | N/A |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(emptyMarkdown.Contains("| Report generated (UTC) | 1970-01-01 00:00:00 UTC |", StringComparison.Ordinal)).IsTrue();
        await Assert.That(emptyMarkdown.Contains("No class coverage data was found in this report.")).IsTrue();
        await Assert.That(emptyMarkdown.Contains("<details><summary>")).IsFalse();
        await Assert.That(emptyMarkdown.Contains("N/A")).IsTrue();
    }
}