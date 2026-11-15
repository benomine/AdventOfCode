#:property TargetFramework=net10.0
#:property PublishAot=false
#:property ManagePackageVersionsCentrally=false
#:package System.CommandLine@3.0.0
#:package Spectre.Console@0.57.2

using System.Globalization;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.CommandLine;
using Spectre.Console;

return await CoverageReporterCommand.RunAsync(args);

internal static class CoverageReporterCommand
{
    private static readonly Argument<string> InputArgument = new("input")
    {
        Description = "Cobertura XML file to read, including reports produced by Microsoft.Testing.Extensions.CodeCoverage."
    };

    private static readonly Option<string> FormatOption = new("--format")
    {
        Description = "Report format: html or markdown.",
        DefaultValueFactory = _ => "html"
    };

    private static readonly Option<string?> OutputOption = new("--output")
    {
        Description = "Output file path. Defaults to the input file with an .html or .md extension."
    };

    private static readonly Option<string?> SourceRootOption = new("--source-root")
    {
        Description = "Repository root used to resolve source files referenced by the coverage report."
    };

    static CoverageReporterCommand()
    {
        FormatOption.Aliases.Add("-f");
        OutputOption.Aliases.Add("-o");
        SourceRootOption.Aliases.Add("-s");
        FormatOption.Validators.Add(result =>
        {
            var format = result.GetValue(FormatOption);
            if (format is not null && !format.Equals("html", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("markdown", StringComparison.OrdinalIgnoreCase))
            {
                result.AddError($"Unsupported format '{format}'. Use 'html' or 'markdown'.");
            }
        });
        InputArgument.Validators.Add(result =>
        {
            if (string.IsNullOrWhiteSpace(result.GetValue(InputArgument)))
            {
                result.AddError("A Cobertura XML input file is required.");
            }
        });
    }

    public static Task<int> RunAsync(string[] args)
    {
        var rootCommand = new RootCommand("Generate an HTML or Markdown report from Cobertura XML.");
        rootCommand.Arguments.Add(InputArgument);
        rootCommand.Options.Add(FormatOption);
        rootCommand.Options.Add(OutputOption);
        rootCommand.Options.Add(SourceRootOption);
        rootCommand.SetAction(ExecuteAsync);
        return rootCommand.Parse(args).InvokeAsync();
    }

    private static async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var input = parseResult.GetValue(InputArgument)!;
        var format = parseResult.GetValue(FormatOption)!.ToLowerInvariant();
        var output = parseResult.GetValue(OutputOption);
        var sourceRoot = parseResult.GetValue(SourceRootOption);

        try
        {
            var report = new CoberturaCoverageReader().Read(input, sourceRoot);
            var contents = format switch
            {
                "html" => new HtmlCoverageReportRenderer().Render(report),
                "markdown" => new MarkdownCoverageReportRenderer().Render(report),
                _ => throw new ArgumentException($"Unsupported format '{format}'. Use 'html' or 'markdown'.")
            };
            var outputPath = Path.GetFullPath(output ?? Path.ChangeExtension(input,
                format == "html" ? ".html" : ".md"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, contents, cancellationToken);
            WriteSuccess(report, outputPath);
            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidDataException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"CoverageReporter: {exception.Message}");
            return 1;
        }
    }

    private static void WriteSuccess(CoverageReport report, string outputPath)
    {
        var summary = report.Summary;
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold]Metric[/]");
        table.AddColumn("[bold]Covered[/]");
        table.AddColumn("[bold]Total[/]");
        table.AddColumn("[bold]Coverage[/]");
        table.AddRow("Lines", FormatCount(summary.LinesCovered), FormatCount(summary.LinesValid),
            FormatPercentage(summary.LineCoveragePercentage));
        table.AddRow("Branches", FormatCount(summary.BranchesCovered), FormatCount(summary.BranchesValid),
            FormatPercentage(summary.BranchCoveragePercentage));
        AnsiConsole.Write(new Rule($"[cyan]{Markup.Escape(report.Name)}[/]"));
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[green]Report written[/] [grey]{Markup.Escape(outputPath)}[/]");
    }

    private static string FormatCount(int count) => count.ToString("N0", CultureInfo.InvariantCulture);

    private static string FormatPercentage(decimal? percentage) => percentage is null
        ? "N/A"
        : $"{percentage.Value.ToString("F1", CultureInfo.InvariantCulture)}%";
}

internal sealed record CoverageReport(
    string Name,
    CoverageMetrics Summary,
    IReadOnlyList<CoverageClass> Classes,
    int? PackageCount = null,
    DateTimeOffset? CoverageTimestamp = null);

internal sealed record CoverageClass(
    string Name,
    string? Filename,
    CoverageMetrics Metrics,
    IReadOnlyList<CoverageLine> Lines,
    string? SourceCode);

internal sealed record CoverageLine(int Number, int Hits, int BranchesCovered, int BranchesValid);

internal sealed record CoverageMetrics(int LinesCovered, int LinesValid, int BranchesCovered, int BranchesValid)
{
    public decimal? LineCoveragePercentage => GetPercentage(LinesCovered, LinesValid);
    public decimal? BranchCoveragePercentage => GetPercentage(BranchesCovered, BranchesValid);

    private static decimal? GetPercentage(int covered, int total) =>
        total == 0 ? null : (decimal)covered * 100 / total;
}

internal static class CoverageClassGrouping
{
    public static string GetNamespace(string className, IReadOnlySet<string> knownClassNames)
    {
        ArgumentNullException.ThrowIfNull(className);
        ArgumentNullException.ThrowIfNull(knownClassNames);
        var enclosingClass = knownClassNames
            .Where(name => !string.Equals(name, className, StringComparison.Ordinal) &&
                           className.StartsWith(name + ".", StringComparison.Ordinal))
            .MaxBy(name => name.Length);
        if (enclosingClass is not null)
        {
            return GetNamespace(enclosingClass, knownClassNames);
        }

        var closureSuffix = className.IndexOf(".<>c", StringComparison.Ordinal);
        if (closureSuffix >= 0)
        {
            className = className[..closureSuffix];
        }

        var separator = className.LastIndexOf('.');
        return separator < 0 ? "(Global namespace)" : className[..separator];
    }

    public static bool IsCompilerGenerated(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.StartsWith("<>c", StringComparison.Ordinal) ||
               name.Contains(".<>c", StringComparison.Ordinal) ||
               name.StartsWith("System.Text.RegularExpressions.Generated.<RegexGenerator_g>", StringComparison.Ordinal);
    }
}

internal sealed class CoberturaCoverageReader
{
    public CoverageReport Read(string path, string? sourceRoot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var stream = File.OpenRead(path);
        return Read(stream, sourceRoot, Path.GetFileNameWithoutExtension(path));
    }

    public CoverageReport Read(Stream stream, string? sourceRoot = null, string? reportName = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        XDocument document;
        try
        {
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            document = XDocument.Load(reader);
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException("The input is not valid Cobertura XML.", exception);
        }

        var root = document.Root;
        if (root is null || root.Name.LocalName != "coverage")
        {
            throw new InvalidDataException("The input XML does not contain a Cobertura coverage element.");
        }

        var baseDirectory = Path.GetFullPath(sourceRoot ?? Environment.CurrentDirectory);
        var sources = root.Descendants().Where(element => element.Name.LocalName == "source")
            .Select(element => element.Value.Trim())
            .Where(value => value.Length > 0)
            .ToArray();
        var classes = root.Descendants().Where(element => element.Name.LocalName == "class")
            .Select(element => ReadClass(element, sources, baseDirectory))
            .ToArray();
        var classSummary = new CoverageMetrics(
            classes.Sum(coverageClass => coverageClass.Metrics.LinesCovered),
            classes.Sum(coverageClass => coverageClass.Metrics.LinesValid),
            classes.Sum(coverageClass => coverageClass.Metrics.BranchesCovered),
            classes.Sum(coverageClass => coverageClass.Metrics.BranchesValid));
        var summary = new CoverageMetrics(
            ReadInteger(root, "lines-covered") ?? classSummary.LinesCovered,
            ReadInteger(root, "lines-valid") ?? classSummary.LinesValid,
            ReadInteger(root, "branches-covered") ?? classSummary.BranchesCovered,
            ReadInteger(root, "branches-valid") ?? classSummary.BranchesValid);
        var packagesElement = root.Elements().FirstOrDefault(element => element.Name.LocalName == "packages");
        return new CoverageReport(
            Attribute(root, "name") ?? reportName ?? "Code coverage",
            summary,
            classes,
            packagesElement is null
                ? null
                : packagesElement.Descendants().Count(element => element.Name.LocalName == "package"),
            ReadTimestamp(root));
    }

    private static CoverageClass ReadClass(XElement element, IReadOnlyList<string> sources, string sourceRoot)
    {
        var lineElements = element.Elements()
            .Where(child => child.Name.LocalName == "lines")
            .SelectMany(linesElement => linesElement.Elements())
            .Where(child => child.Name.LocalName == "line");
        var lines = lineElements
            .Select(line => new CoverageLine(
                ReadInteger(line, "number") ?? 0,
                ReadInteger(line, "hits") ?? 0,
                ReadConditionCount(line, covered: true),
                ReadConditionCount(line, covered: false)))
            .GroupBy(line => line.Number)
            .Select(group => new CoverageLine(
                group.Key,
                group.Max(line => line.Hits),
                group.Max(line => line.BranchesCovered),
                group.Max(line => line.BranchesValid)))
            .OrderBy(line => line.Number)
            .ToArray();
        var metrics = new CoverageMetrics(
            lines.Count(line => line.Hits > 0),
            lines.Length,
            lines.Sum(line => line.BranchesCovered),
            lines.Sum(line => line.BranchesValid));
        var filename = Attribute(element, "filename");
        return new CoverageClass(
            Attribute(element, "name") ?? filename ?? "Unknown class",
            filename,
            metrics,
            lines,
            filename is null ? null : TryReadSource(filename, sources, sourceRoot));
    }

    private static string? TryReadSource(string filename, IReadOnlyList<string> sources, string sourceRoot)
    {
        foreach (var candidate in GetSourceCandidates(filename, sources, sourceRoot))
        {
            try
            {
                if (File.Exists(candidate))
                {
                    return File.ReadAllText(candidate);
                }
            }
            catch (IOException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetSourceCandidates(string filename, IReadOnlyList<string> sources, string sourceRoot)
    {
        if (Path.IsPathRooted(filename))
        {
            yield return filename;
        }

        yield return Path.Combine(sourceRoot, filename.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        foreach (var source in sources)
        {
            if (Path.IsPathRooted(source))
            {
                yield return Path.Combine(source, filename);
            }

            yield return Path.Combine(sourceRoot,
                source.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), filename);
        }
    }

    private static int ReadConditionCount(XElement line, bool covered)
    {
        var conditionCoverage = Attribute(line, "condition-coverage");
        if (conditionCoverage is null)
        {
            return 0;
        }

        var openParenthesis = conditionCoverage.LastIndexOf('(');
        var closeParenthesis = conditionCoverage.LastIndexOf(')');
        if (openParenthesis < 0 || closeParenthesis <= openParenthesis)
        {
            return 0;
        }

        var counts = conditionCoverage[(openParenthesis + 1)..closeParenthesis].Split('/');
        if (counts.Length != 2)
        {
            return 0;
        }

        var index = covered ? 0 : 1;
        return int.TryParse(counts[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    private static int? ReadInteger(XElement element, string name) =>
        int.TryParse(Attribute(element, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static DateTimeOffset? ReadTimestamp(XElement root)
    {
        if (!long.TryParse(Attribute(root, "timestamp"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp))
        {
            return null;
        }

        try
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string? Attribute(XElement element, string name) =>
        element.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == name)?.Value;
}

internal sealed class HtmlCoverageReportRenderer
{
    public string Render(CoverageReport report)
    {
        ArgumentNullException.ThrowIfNull(report);
        var builder = new System.Text.StringBuilder();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html lang=\"en\" data-theme=\"dark\"><head><meta charset=\"utf-8\">");
        builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        builder.Append("<title>").Append(Encode(report.Name)).AppendLine(" · Coverage report</title>");
        builder.AppendLine("<style>").AppendLine(ReportAssets.Css).AppendLine("</style></head><body>");
        builder.AppendLine("<header class=\"topbar\"><div class=\"brand\"><span class=\"brand-mark\">C</span><div><strong>Coverage</strong><span>Report</span></div></div><div class=\"topbar-right\"><div class=\"report-name\">")
            .Append(Encode(report.Name)).AppendLine("</div><button id=\"theme-toggle\" class=\"theme-toggle\" type=\"button\" aria-label=\"Switch to light theme\">Light theme</button></div></header>");
        builder.AppendLine("<main class=\"page\"><section class=\"intro\"><div><p class=\"eyebrow\">CODE QUALITY · COVERAGE</p><h1>Coverage overview</h1><p class=\"subtitle\">Explore your covered code, line by line.</p></div><div class=\"status-pill\"><span></span> Report generated</div></section>");
        builder.AppendLine("<section class=\"summary-grid\" aria-label=\"Coverage summary\">");
        AppendMetric(builder, "Line coverage", report.Summary.LinesCovered, report.Summary.LinesValid,
            report.Summary.LineCoveragePercentage, "lines");
        AppendMetric(builder, "Branch coverage", report.Summary.BranchesCovered, report.Summary.BranchesValid,
            report.Summary.BranchCoveragePercentage, "branches");
        builder.AppendLine("</section><section class=\"results\"><div class=\"section-heading\"><div><h2>Files and classes</h2><p>Classes are grouped by inferred namespace. Choose a class to inspect its source coverage. Compiler-generated classes are hidden by default; group and summary metrics include them.</p><p class=\"coverage-legend\">Coverage colors: <span class=\"coverage-low\">red &lt;50%</span>, <span class=\"coverage-medium\">amber 50% to &lt;80%</span>, <span class=\"coverage-high\">green ≥80%</span>.</p></div><div class=\"class-controls\"><label class=\"show-generated\"><input id=\"show-generated\" type=\"checkbox\"> Show generated classes</label><label class=\"search\"><span aria-hidden=\"true\">⌕</span><input id=\"class-search\" type=\"search\" placeholder=\"Filter namespaces, files or classes\" aria-label=\"Filter namespaces, files or classes\"></label></div></div>");
        builder.AppendLine("<div class=\"table-wrap\"><table><thead><tr><th>Class / file</th><th>Line coverage</th><th>Branches</th></tr></thead>");
        var knownClassNames = report.Classes.Select(item => item.Name).ToHashSet(StringComparer.Ordinal);
        var namespaceGroups = report.Classes
            .GroupBy(item => CoverageClassGrouping.GetNamespace(item.Name, knownClassNames))
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase);
        foreach (var namespaceGroup in namespaceGroups)
        {
            var classes = namespaceGroup
                .OrderBy(item => item.Filename ?? item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            AppendNamespaceGroup(builder, namespaceGroup.Key, classes);
        }

        if (report.Classes.Count == 0)
        {
            builder.AppendLine("<tbody><tr><td class=\"empty\" colspan=\"3\">No class coverage data was found in this report.</td></tr></tbody>");
        }

        builder.AppendLine("<tbody><tr id=\"empty-results\" hidden><td class=\"empty\" colspan=\"3\">No classes match the current filters.</td></tr></tbody></table></div></section><footer>Generated by <strong>CoverageReporter</strong></footer></main>");
        builder.AppendLine("<script>").AppendLine(ReportAssets.JavaScript).AppendLine("</script></body></html>");
        return builder.ToString();
    }

    private static void AppendMetric(System.Text.StringBuilder builder, string label, int covered, int total, decimal? percentage, string unit)
    {
        var colorClass = GetCoverageColorClass(percentage);
        builder.Append("<article class=\"metric-card\"><div class=\"metric-label\">").Append(label)
            .Append("</div><div class=\"metric-value");
        AppendColorClass(builder, colorClass);
        builder.Append("\">").Append(FormatPercentage(percentage))
            .Append("</div><div class=\"metric-detail\"><strong>").Append(covered.ToString(CultureInfo.InvariantCulture))
            .Append("</strong> of ").Append(total.ToString(CultureInfo.InvariantCulture)).Append(' ').Append(unit)
            .Append(" covered</div><div class=\"metric-track\"><span");
        if (colorClass is not null)
        {
            builder.Append(" class=\"").Append(colorClass).Append('"');
        }

        builder.Append(" style=\"width:")
            .Append((percentage ?? 0).ToString("0.##", CultureInfo.InvariantCulture)).AppendLine("%\"></span></div></article>");
    }

    private static void AppendClass(System.Text.StringBuilder builder, CoverageClass coverageClass)
    {
        var searchable = $"{coverageClass.Name} {coverageClass.Filename}";
        var isCompilerGenerated = CoverageClassGrouping.IsCompilerGenerated(coverageClass.Name);
        builder.Append("<tr class=\"class-row");
        if (isCompilerGenerated)
        {
            builder.Append(" generated-class\" hidden data-generated=\"true");
        }
        else
        {
            builder.Append("\" data-generated=\"false");
        }

        builder.Append("\" data-search=\"").Append(Encode(searchable)).AppendLine("\"><td colspan=\"3\"><details>");
        builder.Append("<summary><span class=\"chevron\" aria-hidden=\"true\">›</span><span class=\"class-name\">")
            .Append(Encode(coverageClass.Name)).Append("</span><span class=\"file-name\">")
            .Append(Encode(coverageClass.Filename ?? "Source file unavailable"))
            .Append("</span><span class=\"row-line-coverage\"><span class=\"coverage-value");
        AppendColorClass(builder, GetCoverageColorClass(coverageClass.Metrics.LineCoveragePercentage));
        builder.Append("\">").Append(FormatPercentage(coverageClass.Metrics.LineCoveragePercentage)).Append("</span> <small>")
            .Append(coverageClass.Metrics.LinesCovered.ToString(CultureInfo.InvariantCulture)).Append('/')
            .Append(coverageClass.Metrics.LinesValid.ToString(CultureInfo.InvariantCulture)).Append(" lines</small></span><span class=\"row-branch-coverage\"><span class=\"coverage-value");
        AppendColorClass(builder, GetCoverageColorClass(coverageClass.Metrics.BranchCoveragePercentage));
        builder.Append("\">").Append(FormatPercentage(coverageClass.Metrics.BranchCoveragePercentage)).AppendLine("</span></span></summary>");
        if (coverageClass.SourceCode is null)
        {
            builder.AppendLine("<div class=\"source-unavailable\">Source file was not found. Coverage totals are still available.</div>");
        }
        else
        {
            AppendSource(builder, coverageClass);
        }

        builder.AppendLine("</details></td></tr>");
    }

    private static void AppendNamespaceGroup(System.Text.StringBuilder builder, string namespaceName, IReadOnlyList<CoverageClass> classes)
    {
        var linesCovered = classes.Sum(item => (long)item.Metrics.LinesCovered);
        var linesValid = classes.Sum(item => (long)item.Metrics.LinesValid);
        var branchesCovered = classes.Sum(item => (long)item.Metrics.BranchesCovered);
        var branchesValid = classes.Sum(item => (long)item.Metrics.BranchesValid);
        builder.Append("<tbody class=\"namespace-group\" data-namespace=\"").Append(Encode(namespaceName)).AppendLine("\">");
        builder.Append("<tr class=\"namespace-row\"><td colspan=\"3\"><button class=\"namespace-toggle\" type=\"button\" aria-expanded=\"true\"><span class=\"chevron\" aria-hidden=\"true\">›</span><span class=\"namespace-name\">")
            .Append(Encode(namespaceName)).Append("</span><span class=\"namespace-count\">")
            .Append(classes.Count.ToString(CultureInfo.InvariantCulture)).Append(classes.Count == 1 ? " class" : " classes")
            .Append("</span><span class=\"namespace-line-coverage\"><span class=\"coverage-value");
        AppendColorClass(builder, GetCoverageColorClass(GetPercentage(linesCovered, linesValid)));
        builder.Append("\">").Append(FormatPercentage(GetPercentage(linesCovered, linesValid)))
            .Append("</span> lines</span><span class=\"namespace-branch-coverage\"><span class=\"coverage-value");
        AppendColorClass(builder, GetCoverageColorClass(GetPercentage(branchesCovered, branchesValid)));
        builder.Append("\">").Append(FormatPercentage(GetPercentage(branchesCovered, branchesValid)))
            .AppendLine("</span> branches</span></button></td></tr>");
        foreach (var coverageClass in classes)
        {
            AppendClass(builder, coverageClass);
        }

        builder.AppendLine("</tbody>");
    }

    private static void AppendSource(System.Text.StringBuilder builder, CoverageClass coverageClass)
    {
        var coverageLines = coverageClass.Lines.ToDictionary(line => line.Number);
        builder.AppendLine("<div class=\"source-code\" role=\"region\" aria-label=\"Source coverage\"><ol>");
        using var reader = new StringReader(coverageClass.SourceCode!);
        var number = 1;
        while (reader.ReadLine() is { } sourceLine)
        {
            coverageLines.TryGetValue(number, out var coverageLine);
            var state = coverageLine is null ? "not-measured" : coverageLine.Hits > 0 ? "covered" : "uncovered";
            var hits = coverageLine is null ? "" : $" title=\"{coverageLine.Hits.ToString(CultureInfo.InvariantCulture)} hits\"";
            builder.Append("<li class=\"").Append(state).Append('"').Append(hits).Append("><span class=\"line-number\">")
                .Append(number.ToString(CultureInfo.InvariantCulture)).Append("</span><code>").Append(Encode(sourceLine))
                .AppendLine("</code></li>");
            number++;
        }

        builder.AppendLine("</ol></div>");
    }

    private static string FormatPercentage(decimal? percentage) =>
        percentage is null ? "N/A" : $"{FormatPercentageValue(percentage.Value)}%";

    private static decimal? GetPercentage(long covered, long total) =>
        total == 0 ? null : (decimal)covered * 100 / total;

    private static string FormatPercentageValue(decimal percentage) =>
        percentage.ToString("0.##", CultureInfo.InvariantCulture);

    private static string? GetCoverageColorClass(decimal? percentage)
    {
        if (percentage is null)
        {
            return null;
        }

        var displayedPercentage = decimal.Parse(FormatPercentageValue(percentage.Value), CultureInfo.InvariantCulture);
        return displayedPercentage < 50 ? "coverage-low" : displayedPercentage < 80 ? "coverage-medium" : "coverage-high";
    }

    private static void AppendColorClass(System.Text.StringBuilder builder, string? colorClass)
    {
        if (colorClass is not null)
        {
            builder.Append(' ').Append(colorClass);
        }
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}

internal sealed class MarkdownCoverageReportRenderer
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

internal static class ReportAssets
{
    public const string Css = """
:root {
    color-scheme: dark;
    --page: #0a0a0a;
    --surface: #111111;
    --surface-soft: #161616;
    --surface-hover: #181818;
    --text: #ededed;
    --muted: #a3a3a3;
    --dim: #737373;
    --border: #262626;
    --border-strong: #333333;
    --accent: oklch(0.72 0.18 270);
    --accent-soft: oklch(0.72 0.18 270 / 0.16);
    --amber: oklch(0.82 0.17 85);
    --green: oklch(0.78 0.18 150);
    --green-soft: oklch(0.78 0.18 150 / 0.12);
    --red: oklch(0.72 0.22 27);
    --red-soft: oklch(0.72 0.22 27 / 0.14);
    --shadow: 0 1px 2px rgb(0 0 0 / 40%), 0 0 0 1px rgb(255 255 255 / 2%);
    font-family: Inter, -apple-system, BlinkMacSystemFont, system-ui, sans-serif;
}

:root[data-theme="light"] {
    color-scheme: light;
    --page: #fafaf9;
    --surface: #ffffff;
    --surface-soft: #f5f5f4;
    --surface-hover: #f0efee;
    --text: #1c1917;
    --muted: #57534e;
    --dim: #a8a29e;
    --border: #e7e5e4;
    --border-strong: #d6d3d1;
    --accent: oklch(0.52 0.20 270);
    --accent-soft: oklch(0.52 0.20 270 / 0.10);
    --amber: oklch(0.62 0.15 75);
    --green: oklch(0.55 0.15 150);
    --green-soft: oklch(0.55 0.15 150 / 0.10);
    --red: oklch(0.55 0.22 27);
    --red-soft: oklch(0.55 0.22 27 / 0.10);
    --shadow: 0 1px 2px rgb(15 15 15 / 4%), 0 1px 4px rgb(15 15 15 / 4%);
}

* { box-sizing: border-box; }
body { margin: 0; background: var(--page); color: var(--text); font-size: 13px; line-height: 1.5; -webkit-font-smoothing: antialiased; text-rendering: optimizeLegibility; }
.topbar { position: sticky; top: 0; z-index: 20; height: 60px; padding: 0 max(20px, calc((100% - 1180px) / 2)); display: flex; align-items: center; justify-content: space-between; background: color-mix(in srgb, var(--surface) 94%, transparent); border-bottom: 1px solid var(--border); backdrop-filter: blur(14px); -webkit-backdrop-filter: blur(14px); }
.brand { display: flex; align-items: center; gap: 10px; }
.brand-mark { display: grid; width: 30px; height: 30px; place-items: center; border-radius: 7px; color: white; background: linear-gradient(135deg, oklch(0.62 0.20 270), oklch(0.45 0.20 270)); box-shadow: 0 2px 8px oklch(0.52 0.20 270 / 0.30); font-weight: 700; }
.brand strong, .brand div > span { display: block; }
.brand strong { font-size: 14px; font-weight: 600; letter-spacing: -.01em; }
.brand div > span { color: var(--dim); font-size: 10px; letter-spacing: .1em; text-transform: uppercase; }
.topbar-right { display: flex; align-items: center; gap: 14px; }
.report-name { max-width: 45vw; overflow: hidden; color: var(--muted); text-overflow: ellipsis; white-space: nowrap; }
.theme-toggle { padding: 5px 10px; border: 1px solid var(--border); border-radius: 7px; background: var(--surface-soft); color: var(--muted); cursor: pointer; font: inherit; font-size: 11px; }
.theme-toggle:hover { border-color: var(--border-strong); background: var(--surface-hover); color: var(--text); }
.page { width: min(1180px, calc(100% - 40px)); margin: 32px auto 24px; }
.intro { display: flex; align-items: center; justify-content: space-between; gap: 20px; margin-bottom: 25px; }
.eyebrow { margin: 0 0 8px; color: var(--accent); font-size: 10px; font-weight: 650; letter-spacing: .12em; }
h1 { margin: 0; font-size: clamp(24px, 4vw, 31px); font-weight: 620; letter-spacing: -.035em; }
.subtitle { margin: 8px 0 0; color: var(--muted); }
.status-pill { display: flex; align-items: center; gap: 8px; padding: 7px 11px; border: 1px solid color-mix(in srgb, var(--green) 32%, transparent); border-radius: 30px; background: var(--green-soft); color: var(--green); font-size: 11px; white-space: nowrap; }
.status-pill span { width: 7px; height: 7px; border-radius: 50%; background: var(--green); }
.summary-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px; margin-bottom: 22px; }
.metric-card, .results { border: 1px solid var(--border); border-radius: 12px; background: var(--surface); box-shadow: var(--shadow); }
.metric-card { padding: 18px 20px 16px; }
.metric-label { color: var(--muted); font-size: 12px; font-weight: 550; }
.metric-value { margin-top: 9px; color: var(--text); font-size: 31px; font-weight: 650; letter-spacing: -.04em; font-variant-numeric: tabular-nums; }
.coverage-low { color: var(--red); }
.coverage-medium { color: var(--amber); }
.coverage-high { color: var(--green); }
.metric-detail { margin-top: 4px; color: var(--muted); font-size: 12px; }
.metric-detail strong { color: var(--text); }
.metric-track { height: 4px; margin-top: 15px; overflow: hidden; border-radius: 20px; background: var(--surface-soft); }
.metric-track span { display: block; height: 100%; border-radius: inherit; background: var(--dim); }
.metric-track span.coverage-low { background: var(--red); }
.metric-track span.coverage-medium { background: var(--amber); }
.metric-track span.coverage-high { background: var(--green); }
.section-heading .coverage-legend { font-size: 11px; }
.results { overflow: hidden; }
.section-heading { display: flex; align-items: center; justify-content: space-between; gap: 20px; padding: 18px 20px; border-bottom: 1px solid var(--border); }
.section-heading > div:first-child { min-width: 0; flex: 1 1 auto; }
h2 { margin: 0; font-size: 15px; font-weight: 600; letter-spacing: -.015em; }
.section-heading p { margin: 5px 0 0; color: var(--muted); font-size: 12px; }
.class-controls { display: flex; flex: 0 1 430px; align-items: center; justify-content: flex-end; gap: 12px; min-width: 0; }
.show-generated { display: flex; min-height: 36px; flex: none; align-items: center; gap: 7px; color: var(--muted); font-size: 11px; white-space: nowrap; cursor: pointer; }
.show-generated input { width: 16px; height: 16px; margin: 0; flex: none; accent-color: var(--accent); }
.show-generated input:focus-visible { outline: 2px solid var(--accent); outline-offset: 3px; }
.search { display: flex; min-width: 180px; flex: 1 1 220px; align-items: center; gap: 8px; width: min(285px, 100%); padding: 7px 10px; border: 1px solid var(--border); border-radius: 7px; background: var(--surface-soft); color: var(--dim); }
.search input { width: 100%; min-width: 0; flex: 1; border: 0; outline: 0; background: transparent; color: var(--text); font: inherit; font-size: 11px; }
.search:focus-within { border-color: color-mix(in srgb, var(--accent) 45%, transparent); box-shadow: 0 0 0 3px var(--accent-soft); }
.table-wrap { min-width: 0; overflow-x: auto; }
table { width: 100%; table-layout: fixed; border-collapse: collapse; text-align: left; }
thead { background: var(--surface-soft); color: var(--dim); font-size: 10px; letter-spacing: .08em; text-transform: uppercase; }
th { padding: 12px 20px; font-weight: 700; }
th:nth-child(1) { width: 66%; }
th:nth-child(2) { width: 19%; }
th:nth-child(3) { width: 15%; }
th:nth-child(2), th:nth-child(3) { text-align: right; }
.class-row > td { min-width: 0; padding: 0; border-top: 1px solid var(--border); }
.namespace-group[hidden] { display: none; }
.namespace-row > td { padding: 0; border-top: 1px solid var(--border); background: var(--surface-soft); }
.namespace-toggle { width: 100%; display: grid; grid-template-columns: 20px minmax(145px, 1fr) auto minmax(100px, .7fr) minmax(80px, .55fr); align-items: center; gap: 12px; min-height: 48px; padding: 9px 20px; border: 0; background: transparent; color: var(--text); text-align: left; cursor: pointer; font: inherit; }
.namespace-toggle:hover { background: var(--surface-hover); }
.namespace-toggle .chevron { transform: rotate(90deg); }
.namespace-toggle[aria-expanded="false"] .chevron { transform: rotate(0); }
.namespace-name { overflow: hidden; font-weight: 650; text-overflow: ellipsis; white-space: nowrap; }
.namespace-count { color: var(--muted); font-size: 11px; white-space: nowrap; }
.namespace-line-coverage, .namespace-branch-coverage { text-align: right; font-size: 11px; font-weight: 650; font-variant-numeric: tabular-nums; white-space: nowrap; }
.class-row details > summary { display: grid; min-width: 0; grid-template-columns: 20px minmax(0, 1.2fr) minmax(0, 1.2fr) minmax(100px, .7fr) minmax(80px, .55fr); align-items: center; gap: 12px; min-height: 59px; padding: 10px 20px; cursor: pointer; list-style: none; }
.class-row details > summary::-webkit-details-marker { display: none; }
.class-row details > summary:hover { background: var(--surface-hover); }
.chevron { color: var(--dim); font-size: 22px; transition: transform .15s ease; }
details[open] .chevron { transform: rotate(90deg); }
.class-name { overflow: hidden; font-weight: 650; text-overflow: ellipsis; white-space: nowrap; }
.file-name { overflow: hidden; color: var(--muted); font-family: ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 11px; text-overflow: ellipsis; white-space: nowrap; }
.row-line-coverage, .row-branch-coverage { text-align: right; font-weight: 650; font-variant-numeric: tabular-nums; }
.row-line-coverage small { display: block; margin-top: 3px; color: var(--muted); font-size: 10px; font-weight: 400; }
.source-code { width: 100%; max-width: 100%; min-width: 0; max-height: 470px; overflow: auto; border-top: 1px solid var(--border); background: var(--page); font-family: "JetBrains Mono", ui-monospace, SFMono-Regular, Consolas, monospace; font-size: 11px; line-height: 1.7; }
.source-code ol { width: max-content; min-width: 100%; margin: 0; padding: 10px 0; list-style: none; }
.source-code li { display: flex; min-width: max-content; padding: 0 18px 0 0; white-space: pre; }
.source-code li::before { width: 3px; margin-right: 12px; content: ""; background: transparent; }
.source-code li.covered::before { background: var(--green); }
.source-code li.uncovered::before { background: var(--red); }
.line-number { width: 48px; padding-right: 12px; color: var(--dim); text-align: right; user-select: none; }
.source-code code { color: var(--text); }
.source-code li.uncovered { background: var(--red-soft); }
.source-unavailable { padding: 17px 22px 18px 52px; border-top: 1px solid var(--border); background: var(--surface-soft); color: var(--muted); font-size: 12px; }
.empty { padding: 35px 20px; color: var(--muted); text-align: center; }
footer { padding: 22px 2px; color: var(--muted); font-size: 11px; text-align: right; }
footer strong { color: var(--muted); }

@media (max-width: 760px) {
    .topbar { padding: 0 14px; }
    .page { width: calc(100% - 32px); margin-top: 26px; }
    .intro { align-items: flex-start; }
    .status-pill { display: none; }
    .topbar-right { gap: 8px; }
    .theme-toggle { padding: 5px 8px; }
    .summary-grid { gap: 11px; }
    .metric-card { padding: 17px; }
    .metric-value { font-size: 29px; }
    .section-heading { align-items: flex-start; flex-direction: column; padding: 18px; }
    .class-controls { width: 100%; flex: none; align-items: stretch; flex-direction: column; }
    .search { width: 100%; min-width: 0; flex: none; }
    .namespace-toggle { grid-template-columns: 16px minmax(0, 1fr) auto; gap: 8px; padding: 10px 13px; }
    .namespace-count { grid-column: 2; grid-row: 2; }
    .namespace-line-coverage { grid-column: 3; grid-row: 1; }
    .namespace-branch-coverage { display: none; }
    .class-row details > summary { grid-template-columns: 16px minmax(120px, 1fr) minmax(82px, .6fr); gap: 8px; padding: 10px 13px; }
    .file-name { grid-column: 2 / 3; grid-row: 2; }
    .row-line-coverage { grid-column: 3; grid-row: 1; }
    .row-branch-coverage { display: none; }
    th { padding: 11px 13px; }
    th:nth-child(1) { width: 67%; }
    th:nth-child(2) { width: 33%; }
    th:nth-child(3) { display: none; }
}

@media (max-width: 430px) {
    .report-name { max-width: 42vw; font-size: 10px; }
    .summary-grid { grid-template-columns: 1fr; }
    .metric-card { padding: 15px 17px; }
    .metric-value { margin-top: 5px; }
}
""";

    public const string JavaScript = """
(() => {
    const search = document.getElementById('class-search');
    const rows = [...document.querySelectorAll('.class-row')];
    const groups = [...document.querySelectorAll('.namespace-group')];
    const showGenerated = document.getElementById('show-generated');
    const emptyResults = document.getElementById('empty-results');
    const themeToggle = document.getElementById('theme-toggle');

    const updateRows = () => {
        const query = search?.value.trim().toLocaleLowerCase() ?? '';
        let visibleCount = 0;
        groups.forEach(group => {
            const matchesNamespace = query.length > 0 && group.dataset.namespace.toLocaleLowerCase().includes(query);
            const isCollapsed = group.dataset.collapsed === 'true';
            let groupVisibleCount = 0;
            group.querySelectorAll('.class-row').forEach(row => {
                const matchesSearch = query.length === 0 || matchesNamespace || row.dataset.search.toLocaleLowerCase().includes(query);
                const isGenerated = row.dataset.generated === 'true';
                const visible = matchesSearch && (!isGenerated || showGenerated?.checked);
                row.hidden = !visible || isCollapsed;
                if (visible) {
                    visibleCount++;
                    groupVisibleCount++;
                }
            });
            group.hidden = groupVisibleCount === 0;
        });
        if (emptyResults) emptyResults.hidden = rows.length === 0 || visibleCount > 0;
    };

    groups.forEach(group => {
        const toggle = group.querySelector('.namespace-toggle');
        if (toggle) {
            toggle.addEventListener('click', () => {
                const isExpanded = toggle.getAttribute('aria-expanded') === 'true';
                toggle.setAttribute('aria-expanded', String(!isExpanded));
                group.dataset.collapsed = String(isExpanded);
                updateRows();
            });
        }
    });

    if (search) {
        search.addEventListener('input', updateRows);
    }

    if (showGenerated) {
        showGenerated.addEventListener('change', updateRows);
    }

    updateRows();

    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const nextTheme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
            document.documentElement.dataset.theme = nextTheme;
            const targetTheme = nextTheme === 'dark' ? 'light' : 'dark';
            themeToggle.textContent = `${targetTheme === 'light' ? 'Light' : 'Dark'} theme`;
            themeToggle.setAttribute('aria-label', `Switch to ${targetTheme} theme`);
        });
    }
})();
""";
}