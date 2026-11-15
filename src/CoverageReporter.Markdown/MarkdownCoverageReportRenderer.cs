using System.Globalization;
using System.Net;
using CoverageReporter.Core;

namespace CoverageReporter.Markdown;

public sealed class MarkdownCoverageReportRenderer
{
    public string Render(CoverageReport report, DateTimeOffset? generatedAt = null)
    {
        ArgumentNullException.ThrowIfNull(report);

        var generationTimestamp = generatedAt ?? DateTimeOffset.UtcNow;
        var lines = new List<string>
        {
            $"# Coverage report: {EscapeMarkdown(report.Name)}",
            "",
            "<details open><summary>Coverage summary</summary>",
            "",
            "| Metric | Result |",
            "|:---|---:|",
            $"| Line coverage | {FormatCoverage(report.Summary.LineCoveragePercentage, report.Summary.LinesCovered, report.Summary.LinesValid)} |",
            $"| Lines covered | {FormatCount(report.Summary.LinesCovered)} |",
            $"| Lines uncovered | {FormatCount((long)report.Summary.LinesValid - report.Summary.LinesCovered)} |",
            $"| Coverable lines | {FormatCount(report.Summary.LinesValid)} |",
            $"| Branch coverage | {FormatCoverage(report.Summary.BranchCoveragePercentage, report.Summary.BranchesCovered, report.Summary.BranchesValid)} |",
            $"| Branches covered | {FormatCount(report.Summary.BranchesCovered)} |",
            $"| Branches uncovered | {FormatCount((long)report.Summary.BranchesValid - report.Summary.BranchesCovered)} |",
            $"| Coverable branches | {FormatCount(report.Summary.BranchesValid)} |",
            $"| Packages | {FormatOptionalCount(report.PackageCount)} |",
            $"| Classes | {report.Classes.Count.ToString(CultureInfo.InvariantCulture)} |",
            $"| Files | {GetFileCount(report.Classes).ToString(CultureInfo.InvariantCulture)} |",
            $"| Generated classes | {report.Classes.Count(item => CoverageClassGrouping.IsCompilerGenerated(item.Name)).ToString(CultureInfo.InvariantCulture)} |",
            $"| Coverage timestamp (UTC) | {FormatTimestamp(report.CoverageTimestamp)} |",
            $"| Report generated (UTC) | {FormatTimestamp(generationTimestamp)} |",
            "",
            "</details>",
            "",
            "Coverage colors: 🔴 below 50%, 🟡 50% to below 80%, 🟢 80% or above; N/A means no coverable items.",
            "",
            "## Coverage",
            ""
        };

        if (report.Classes.Count == 0)
        {
            lines.Add("No class coverage data was found in this report.");
            return string.Join(Environment.NewLine, lines) + Environment.NewLine;
        }

        var knownClassNames = report.Classes.Select(item => item.Name).ToHashSet(StringComparer.Ordinal);
        var namespaceGroups = report.Classes
            .GroupBy(item => CoverageClassGrouping.GetNamespace(item.Name, knownClassNames))
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var namespaceGroup in namespaceGroups)
        {
            var classes = namespaceGroup
                .OrderBy(item => item.Filename ?? item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            AppendNamespaceGroup(lines, namespaceGroup.Key, classes);
        }

        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static void AppendNamespaceGroup(List<string> lines, string namespaceName, IReadOnlyList<CoverageClass> classes)
    {
        var linesCovered = classes.Sum(item => (long)item.Metrics.LinesCovered);
        var linesValid = classes.Sum(item => (long)item.Metrics.LinesValid);
        var branchesCovered = classes.Sum(item => (long)item.Metrics.BranchesCovered);
        var branchesValid = classes.Sum(item => (long)item.Metrics.BranchesValid);
        var generatedClasses = classes.Where(item => CoverageClassGrouping.IsCompilerGenerated(item.Name)).ToArray();
        var visibleClasses = classes.Where(item => !CoverageClassGrouping.IsCompilerGenerated(item.Name)).ToArray();

        lines.Add($"<details><summary>{EncodeHtml(namespaceName)} — {FormatCoverage(GetPercentage(linesCovered, linesValid), linesCovered, linesValid)} lines · {FormatCoverage(GetPercentage(branchesCovered, branchesValid), branchesCovered, branchesValid)} branches ({classes.Count.ToString(CultureInfo.InvariantCulture)} classes)</summary>");
        lines.Add("");
        if (visibleClasses.Length > 0)
        {
            AppendClassTable(lines, visibleClasses);
        }
        else
        {
            lines.Add("_No non-generated classes in this namespace._");
        }

        if (generatedClasses.Length > 0)
        {
            lines.Add("");
            lines.Add($"<details><summary>Generated classes ({generatedClasses.Length.ToString(CultureInfo.InvariantCulture)})</summary>");
            lines.Add("");
            AppendClassTable(lines, generatedClasses);
            lines.Add("");
            lines.Add("</details>");
        }

        lines.Add("");
        lines.Add("</details>");
        lines.Add("");
    }

    private static void AppendClassTable(List<string> lines, IReadOnlyList<CoverageClass> classes)
    {
        lines.Add("| Class | File | Lines | Branches |");
        lines.Add("|:---|:---|---:|---:|");
        foreach (var coverageClass in classes)
        {
            lines.Add($"| {EscapeMarkdown(coverageClass.Name)} | {EscapeMarkdown(coverageClass.Filename ?? "—")} | " +
                      $"{FormatCoverage(coverageClass.Metrics.LineCoveragePercentage, coverageClass.Metrics.LinesCovered, coverageClass.Metrics.LinesValid)} | " +
                      $"{FormatCoverage(coverageClass.Metrics.BranchCoveragePercentage, coverageClass.Metrics.BranchesCovered, coverageClass.Metrics.BranchesValid)} |");
        }
    }

    private static int GetFileCount(IReadOnlyList<CoverageClass> classes) => classes
        .Where(item => item.Filename is not null)
        .Select(item => item.Filename!)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Count();

    private static decimal? GetPercentage(long covered, long total) =>
        total == 0 ? null : (decimal)covered * 100 / total;

    private static string FormatCoverage(decimal? percentage, long covered, long total) =>
        percentage is null
            ? $"N/A ({FormatCount(covered)}/{FormatCount(total)})"
            : $"{GetCoverageIndicator(percentage.Value)} {FormatPercentage(percentage)} ({FormatCount(covered)}/{FormatCount(total)})";

    private static string GetCoverageIndicator(decimal percentage)
    {
        var displayedPercentage = decimal.Parse(FormatPercentageValue(percentage), CultureInfo.InvariantCulture);
        return displayedPercentage < 50 ? "🔴" : displayedPercentage < 80 ? "🟡" : "🟢";
    }

    private static string FormatPercentage(decimal? percentage) =>
        percentage is null ? "N/A" : $"{FormatPercentageValue(percentage.Value)}%";

    private static string FormatPercentageValue(decimal percentage) =>
        percentage.ToString("0.##", CultureInfo.InvariantCulture);

    private static string FormatCount(long count) => count.ToString(CultureInfo.InvariantCulture);

    private static string FormatOptionalCount(int? count) => count?.ToString(CultureInfo.InvariantCulture) ?? "N/A";

    private static string FormatTimestamp(DateTimeOffset? timestamp) => timestamp is null
        ? "N/A"
        : timestamp.Value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

    private static string EscapeMarkdown(string value)
    {
        var encoded = EncodeHtml(value.Replace("\r", " ").Replace("\n", " "));
        return encoded.Replace("\\", "\\\\").Replace("|", "\\|").Replace("`", "\\`")
            .Replace("*", "\\*").Replace("_", "\\_").Replace("[", "\\[").Replace("]", "\\]")
            .Replace("!", "\\!").Replace("(", "\\(").Replace(")", "\\)").Replace("~", "\\~");
    }

    private static string EncodeHtml(string value) => WebUtility.HtmlEncode(value);
}