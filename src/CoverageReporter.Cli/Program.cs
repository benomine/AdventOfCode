using System.CommandLine;
using System.Globalization;
using CoverageReporter.Html;
using CoverageReporter.Markdown;
using CoverageReporter.MicrosoftCodeCoverage;
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

    private static void WriteSuccess(CoverageReporter.Core.CoverageReport report, string outputPath)
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