using CoverageReporter.Core;
using CoverageReporter.Html;

namespace CoverageReporter.Html.Tests;

public class HtmlCoverageReportRendererTests
{
    [Test]
    public async Task RendersMetricsSourceAndEscapesReportContent()
    {
        var report = new CoverageReport(
            "<unsafe report>",
            new CoverageMetrics(1, 2, 1, 2),
            [new CoverageClass("A&B", "Source.cs", new CoverageMetrics(1, 2, 1, 2),
                [new CoverageLine(1, 1, 0, 0), new CoverageLine(2, 0, 1, 2)], "if (x < 3) & ready")]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Contains("&lt;unsafe report&gt;")).IsTrue();
        await Assert.That(html.Contains("A&amp;B")).IsTrue();
        await Assert.That(html.Contains("if (x &lt; 3) &amp; ready")).IsTrue();
        await Assert.That(html.Contains("50%")).IsTrue();
        await Assert.That(html.Contains("class-search")).IsTrue();
        await Assert.That(html.Contains("data-theme=\"dark\"")).IsTrue();
        await Assert.That(html.Contains("theme-toggle")).IsTrue();
        await Assert.That(html.Contains(".brand-mark { display: grid;")).IsTrue();
        await Assert.That(html.Contains(".brand strong, .brand div > span { display: block; }")).IsTrue();
        await Assert.That(html.Contains(".brand strong, .brand span { display: block; }")).IsFalse();
    }

    [Test]
    public async Task KeepsControlsUsableAndLongSourceLinesInsideTheScrollableViewer()
    {
        var longSourceLine = new string('x', 4096);
        var report = new CoverageReport("long-source", new CoverageMetrics(1, 1, 0, 0),
            [new CoverageClass("Demo.LongLine", "LongLine.cs", new CoverageMetrics(1, 1, 0, 0),
                [new CoverageLine(1, 1, 0, 0)], longSourceLine)]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Contains(longSourceLine)).IsTrue();
        await Assert.That(html.Contains("table { width: 100%; table-layout: fixed;")).IsTrue();
        await Assert.That(html.Contains(".source-code { width: 100%; max-width: 100%; min-width: 0; max-height: 470px; overflow: auto;")).IsTrue();
        await Assert.That(html.Contains(".source-code ol { width: max-content; min-width: 100%;")).IsTrue();
        await Assert.That(html.Contains(".search { display: flex; min-width: 180px; flex: 1 1 220px;")).IsTrue();
        await Assert.That(html.Contains(".show-generated input { width: 16px; height: 16px;")).IsTrue();
        await Assert.That(html.Contains("@media (max-width: 760px)")).IsTrue();
        await Assert.That(html.Contains(".search { width: 100%; min-width: 0; flex: none; }")).IsTrue();
    }

    [Test]
    public async Task MissingSourcesAreReportedWithoutBreakingTheCoverageSummary()
    {
        var report = new CoverageReport("empty", new CoverageMetrics(0, 0, 0, 0),
            [new CoverageClass("NoSource", "Missing.cs", new CoverageMetrics(0, 0, 0, 0), [], null)]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Contains("N/A")).IsTrue();
        await Assert.That(html.Contains("Source file was not found")).IsTrue();
        await Assert.That(html.Contains("NoSource")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-line-coverage\"><span class=\"coverage-value\">N/A</span> lines</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-branch-coverage\"><span class=\"coverage-value\">N/A</span> branches</span>")).IsTrue();
    }

    [Test]
    public async Task ColorCodesCoveragePercentagesUsingDisplayedThresholds()
    {
        var boundaries = new[]
        {
            ("Zero", 0, 100, "0", "coverage-low"),
            ("BelowAmber", 4999, 10000, "49.99", "coverage-low"),
            ("RoundedAmber", 49999, 100000, "50", "coverage-medium"),
            ("Amber", 50, 100, "50", "coverage-medium"),
            ("BelowGreen", 7999, 10000, "79.99", "coverage-medium"),
            ("RoundedGreen", 79999, 100000, "80", "coverage-high"),
            ("Green", 80, 100, "80", "coverage-high"),
            ("Complete", 100, 100, "100", "coverage-high")
        };
        var classes = boundaries.Select(boundary => new CoverageClass(boundary.Item1, boundary.Item1 + ".cs",
            new CoverageMetrics(boundary.Item2, boundary.Item3, boundary.Item2, boundary.Item3), [], null)).ToArray();
        var report = new CoverageReport("thresholds", new CoverageMetrics(79999, 100000, 49999, 100000), classes);

        var html = new HtmlCoverageReportRenderer().Render(report);

        foreach (var (_, _, _, displayedPercentage, colorClass) in boundaries)
        {
            await Assert.That(html.Contains($"<span class=\"coverage-value {colorClass}\">{displayedPercentage}%</span>")).IsTrue();
        }

        await Assert.That(html.Contains("<div class=\"metric-value coverage-high\">80%</div>")).IsTrue();
        await Assert.That(html.Contains("<div class=\"metric-value coverage-medium\">50%</div>")).IsTrue();
        await Assert.That(html.Contains("class=\"metric-track\"><span class=\"coverage-high\" style=\"width:80%\"")).IsTrue();
        await Assert.That(html.Contains("class=\"metric-track\"><span class=\"coverage-medium\" style=\"width:50%\"")).IsTrue();
        await Assert.That(html.Contains("Coverage colors")).IsTrue();
        await Assert.That(html.Contains("amber 50% to &lt;80%")).IsTrue();
    }

    [Test]
    public async Task UnavailableCoverageRemainsNeutral()
    {
        var report = new CoverageReport("unavailable", new CoverageMetrics(0, 0, 0, 0),
            [new CoverageClass("NoMetrics", "NoMetrics.cs", new CoverageMetrics(0, 0, 0, 0), [], null)]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Contains("<div class=\"metric-value\">N/A</div>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"row-line-coverage\"><span class=\"coverage-value\">N/A</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"row-branch-coverage\"><span class=\"coverage-value\">N/A</span>")).IsTrue();
    }

    [Test]
    public async Task GroupsClassesByInferredNamespaceAndAggregatesCoverage()
    {
        var report = new CoverageReport("namespaces", new CoverageMetrics(0, 0, 0, 0),
        [
            new CoverageClass("Demo.Core.Worker", "Worker.cs", new CoverageMetrics(4, 5, 1, 2), [], null),
            new CoverageClass("Demo.Core.Other", "Other.cs", new CoverageMetrics(1, 5, 0, 0), [], null),
            new CoverageClass("Demo.Api.Controller", "Controller.cs", new CoverageMetrics(8, 10, 4, 5), [], null),
            new CoverageClass("GlobalType", "GlobalType.cs", new CoverageMetrics(2, 2, 0, 0), [], null),
            new CoverageClass("Demo.Core.Worker.Child", "Child.cs", new CoverageMetrics(2, 2, 1, 2), [], null),
            new CoverageClass("Demo.Core.Worker.<>c", "Worker.cs", new CoverageMetrics(1, 3, 0, 0), [], null)
        ]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Split("<tbody class=\"namespace-group\"", StringSplitOptions.None).Length - 1).IsEqualTo(3);
        await Assert.That(html.Contains("<span class=\"namespace-name\">Demo.Core</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-name\">Demo.Api</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-name\">(Global namespace)</span>")).IsTrue();
        await Assert.That(html.Contains("class=\"namespace-toggle\" type=\"button\" aria-expanded=\"true\"")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-count\">4 classes</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"coverage-value coverage-medium\">53.33%</span> lines")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-branch-coverage\"><span class=\"coverage-value coverage-medium\">50%</span> branches</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"class-name\">Demo.Core.Worker.Child</span>")).IsTrue();
        await Assert.That(html.Contains("<span class=\"class-name\">Demo.Core.Worker.&lt;&gt;c</span>")).IsTrue();
        await Assert.That(html.Contains("group and summary metrics include them")).IsTrue();
        await Assert.That(html.Contains("group.querySelectorAll('.class-row')")).IsTrue();
        await Assert.That(html.Contains("group.dataset.collapsed = String(isExpanded)")).IsTrue();
        await Assert.That(html.Contains("matchesNamespace || row.dataset.search.toLocaleLowerCase().includes(query)")).IsTrue();
    }

    [Test]
    public async Task EscapesNamespaceNamesAndHandlesReportsWithoutClasses()
    {
        var report = new CoverageReport("namespaces", new CoverageMetrics(0, 0, 0, 0),
            [new CoverageClass("A&B.Widget", "Widget.cs", new CoverageMetrics(1, 1, 0, 0), [], null)]);
        var html = new HtmlCoverageReportRenderer().Render(report);
        var emptyHtml = new HtmlCoverageReportRenderer().Render(new CoverageReport("empty", new CoverageMetrics(0, 0, 0, 0), []));

        await Assert.That(html.Contains("data-namespace=\"A&amp;B\"")).IsTrue();
        await Assert.That(html.Contains("<span class=\"namespace-name\">A&amp;B</span>")).IsTrue();
        await Assert.That(emptyHtml.Contains("No class coverage data was found in this report.")).IsTrue();
        await Assert.That(emptyHtml.Contains("<tbody class=\"namespace-group\"")).IsFalse();
        await Assert.That(emptyHtml.Contains("id=\"empty-results\" hidden")).IsTrue();
    }

    [Test]
    public async Task GeneratedClosureWithoutAnEnclosingClassUsesTheOwningTypeNamespace()
    {
        var report = new CoverageReport("generated", new CoverageMetrics(0, 0, 0, 0),
            [new CoverageClass("Orphan.Namespace.Owner.<>c__DisplayClass0_0", "Owner.cs", new CoverageMetrics(1, 1, 0, 0), [], null)]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Contains("data-namespace=\"Orphan.Namespace\"")).IsTrue();
    }

    [Test]
    public async Task HidesCompilerGeneratedClassesByDefaultWithoutDroppingCoverage()
    {
        var report = new CoverageReport("generated", new CoverageMetrics(4, 10, 1, 3),
        [
            new CoverageClass("AdventOfCode._2015.Day1", "Day1.cs", new CoverageMetrics(1, 2, 0, 0), [], null),
            new CoverageClass("AdventOfCode._2024.Day6.Point", "Day6.cs", new CoverageMetrics(1, 2, 0, 0), [], null),
            new CoverageClass("AdventOfCode._2015.Day1.<>c", "Day1.cs", new CoverageMetrics(1, 2, 0, 0), [], null),
            new CoverageClass("AdventOfCode._2015.Day1.<>c__DisplayClass5_0", "Day1.cs", new CoverageMetrics(1, 2, 0, 0), [], null),
            new CoverageClass("System.Text.RegularExpressions.Generated.<RegexGenerator_g>ABC__SampleRegex_0", "Regex.cs", new CoverageMetrics(1, 2, 1, 3), [], null),
            new CoverageClass("System.Text.RegularExpressions.Generated.RegexHelper", "RegexHelper.cs", new CoverageMetrics(1, 2, 0, 0), [], null)
        ]);

        var html = new HtmlCoverageReportRenderer().Render(report);

        await Assert.That(html.Split("class=\"class-row generated-class\" hidden data-generated=\"true\"", StringSplitOptions.None).Length - 1).IsEqualTo(3);
        await Assert.That(html.Contains("<span class=\"class-name\">AdventOfCode._2015.Day1</span>")).IsTrue();
        await Assert.That(html.Contains("AdventOfCode._2024.Day6.Point")).IsTrue();
        await Assert.That(html.Contains("System.Text.RegularExpressions.Generated.RegexHelper")).IsTrue();
        await Assert.That(html.Contains("System.Text.RegularExpressions.Generated.&lt;RegexGenerator_g&gt;ABC__SampleRegex_0")).IsTrue();
        await Assert.That(html.Contains("AdventOfCode._2015.Day1.&lt;&gt;c")).IsTrue();
        await Assert.That(html.Contains("AdventOfCode._2015.Day1.&lt;&gt;c__DisplayClass5_0")).IsTrue();
        await Assert.That(html.Contains("4</strong> of 10 lines covered")).IsTrue();
        await Assert.That(html.Contains("Show generated classes")).IsTrue();
        await Assert.That(html.Contains("summary metrics include them")).IsTrue();
        await Assert.That(html.Contains("showGenerated.addEventListener('change', updateRows)")).IsTrue();
        await Assert.That(html.Contains("matchesSearch && (!isGenerated || showGenerated?.checked)")).IsTrue();
        await Assert.That(html.Contains("No classes match the current filters.")).IsTrue();
    }
}