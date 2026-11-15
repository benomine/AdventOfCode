using System.Diagnostics;

namespace CoverageReporter.Cli.Tests;

public class CoverageReporterCliTests
{
    private const string Cobertura = """
        <coverage name="[Worker]" lines-covered="1" lines-valid="2">
          <sources><source>src</source></sources>
          <packages><package><classes>
            <class name="Sample.Worker" filename="Worker.cs">
              <lines><line number="1" hits="1"/><line number="2" hits="0"/></lines>
            </class>
          </classes></package></packages>
        </coverage>
        """;

    [Test]
    public async Task HelpShowsArgumentsAndOptions()
    {
        var result = await RunCliAsync("--help");

        Assert.Equals(result.ExitCode, 0);
        Assert.Equals(result.StandardOutput.Contains("--format", StringComparison.Ordinal), true);
        Assert.Equals(result.StandardOutput.Contains("--source-root", StringComparison.Ordinal), true);
    }

    [Test]
    public async Task DefaultFormatWritesHtmlSummaryAndEscapesMarkupSensitivePath()
    {
        var directory = CreateTempDirectory();
        try
        {
            var input = Path.Combine(directory, "coverage.xml");
            var output = Path.ChangeExtension(input, ".html");
            File.WriteAllText(input, Cobertura);

            var result = await RunCliAsync(input);

            Assert.Equals(result.ExitCode, 0);
            Assert.Equals(File.Exists(output), true);
            var html = File.ReadAllText(output);
            Assert.Equals(html.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase), true);
            Assert.Equals(html.Contains(".theme-toggle", StringComparison.Ordinal), true);
            Assert.Equals(html.Contains("const themeToggle", StringComparison.Ordinal), true);
            Assert.Equals(result.StandardOutput.Contains(directory, StringComparison.Ordinal), true);
            Assert.Equals(result.StandardOutput.Contains("N/A", StringComparison.Ordinal), true);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task MarkdownAliasesAndSourceRootGenerateMarkdownAtRequestedPath()
    {
        var directory = CreateTempDirectory();
        try
        {
            var input = Path.Combine(directory, "coverage.xml");
            var sourceRoot = Path.Combine(directory, "source");
            var output = Path.Combine(directory, "nested", "summary.md");
            Directory.CreateDirectory(Path.Combine(sourceRoot, "src"));
            File.WriteAllText(Path.Combine(sourceRoot, "src", "Worker.cs"), "public class Worker { }");
            File.WriteAllText(input, Cobertura);

            var result = await RunCliAsync(input, "-f", "MaRkDoWn", "-o", output, "-s", sourceRoot);

            Assert.Equals(result.ExitCode, 0);
            Assert.Equals(File.Exists(output), true);
            var markdown = File.ReadAllText(output);
            Assert.Equals(markdown.Contains("<details open><summary>Coverage summary</summary>", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("Sample — 🟡 50% (1/2) lines", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("1/2", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Lines uncovered | 1 |", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Coverable lines | 2 |", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Packages | 1 |", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Coverage timestamp (UTC) | N/A |", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Report generated (UTC) | ", StringComparison.Ordinal), true);
            Assert.Equals(result.StandardOutput.Contains("Report written", StringComparison.Ordinal), true);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task StandaloneFileAppRunsInIsolationAndMatchesProjectReports()
    {
        var directory = CreateTempDirectory();
        try
        {
            var repositoryRoot = FindRepositoryRoot();
            var standaloneFile = Path.Combine(directory, "CoverageReporter.cs");
            var input = Path.Combine(directory, "coverage.xml");
            var sourceRoot = Path.Combine(directory, "source");
            var standaloneHtml = Path.ChangeExtension(input, ".html");
            var standaloneMarkdown = Path.Combine(directory, "nested", "summary.md");
            var projectHtml = Path.Combine(directory, "project.html");
            var projectMarkdown = Path.Combine(directory, "project.md");
            File.Copy(Path.Combine(repositoryRoot, "CoverageReporter.cs"), standaloneFile);
            Directory.CreateDirectory(Path.Combine(sourceRoot, "src"));
            File.WriteAllText(Path.Combine(sourceRoot, "src", "Worker.cs"), "public class Worker { }");
            File.WriteAllText(input, Cobertura);

            var help = await RunStandaloneAsync(standaloneFile, repositoryRoot, "--help");
            var htmlResult = await RunStandaloneAsync(standaloneFile, repositoryRoot, input, "--source-root", sourceRoot);
            var markdownResult = await RunStandaloneAsync(standaloneFile, repositoryRoot, input,
                "-f", "MaRkDoWn", "-o", standaloneMarkdown, "-s", sourceRoot);
            var invalidFormat = await RunStandaloneAsync(standaloneFile, repositoryRoot, input, "--format", "json");
            var projectHtmlResult = await RunCliAsync(input, "-o", projectHtml, "-s", sourceRoot);
            var projectMarkdownResult = await RunCliAsync(input, "-f", "markdown", "-o", projectMarkdown, "-s", sourceRoot);

            Assert.Equals(help.ExitCode, 0);
            Assert.Equals(help.StandardOutput.Contains("--format", StringComparison.Ordinal), true);
            Assert.Equals(help.StandardOutput.Contains("--source-root", StringComparison.Ordinal), true);
            Assert.Equals(htmlResult.ExitCode, 0);
            Assert.Equals(markdownResult.ExitCode, 0);
            Assert.Equals(invalidFormat.ExitCode != 0, true);
            Assert.Equals(invalidFormat.StandardError.Contains("html", StringComparison.OrdinalIgnoreCase), true);
            Assert.Equals(projectHtmlResult.ExitCode, 0);
            Assert.Equals(projectMarkdownResult.ExitCode, 0);

            var html = File.ReadAllText(standaloneHtml);
            var projectHtmlContents = File.ReadAllText(projectHtml);
            var markdown = File.ReadAllText(standaloneMarkdown);
            var projectMarkdownContents = File.ReadAllText(projectMarkdown);
            Assert.Equals(html.Contains(".coverage-low", StringComparison.Ordinal), true);
            Assert.Equals(html.Contains("namespace-group", StringComparison.Ordinal), true);
            Assert.Equals(html.Contains("public class Worker { }", StringComparison.Ordinal), true);
            Assert.Equals(html, projectHtmlContents);
            Assert.Equals(NormalizeGeneratedTimestamp(markdown), NormalizeGeneratedTimestamp(projectMarkdownContents));
            Assert.Equals(markdown.Contains("| Lines uncovered | 1 |", StringComparison.Ordinal), true);
            Assert.Equals(markdown.Contains("| Packages | 1 |", StringComparison.Ordinal), true);
        }
        finally
        {
            await ShutdownBuildServersAsync(FindRepositoryRoot());
            await DeleteTempDirectoryAsync(directory);
        }
    }

    [Test]
    public async Task InvalidFormatAndMissingInputReturnErrors()
    {
        var directory = CreateTempDirectory();
        try
        {
            var input = Path.Combine(directory, "coverage.xml");
            File.WriteAllText(input, Cobertura);

            var invalidFormat = await RunCliAsync(input, "--format", "json");
            var missingInput = await RunCliAsync(Path.Combine(directory, "missing.xml"));

            Assert.Equals(invalidFormat.ExitCode != 0, true);
            Assert.Equals(invalidFormat.StandardError.Contains("html", StringComparison.OrdinalIgnoreCase), true);
            Assert.Equals(missingInput.ExitCode != 0, true);
            Assert.Equals(missingInput.StandardError.Contains("missing.xml", StringComparison.OrdinalIgnoreCase), true);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task MissingRequiredInputIsRejected()
    {
        var result = await RunCliAsync();

        Assert.Equals(result.ExitCode != 0, true);
        Assert.Equals(result.StandardError.Contains("input", StringComparison.OrdinalIgnoreCase), true);
    }

    [Test]
    public async Task MalformedInputReturnsActionableErrorOnStandardError()
    {
        var directory = CreateTempDirectory();
        try
        {
            var input = Path.Combine(directory, "broken.xml");
            File.WriteAllText(input, "<coverage>");

            var result = await RunCliAsync(input);

            Assert.Equals(result.ExitCode, 1);
            Assert.Equals(result.StandardError.Contains("valid Cobertura XML", StringComparison.Ordinal), true);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static async Task<CommandResult> RunCliAsync(params string[] arguments)
    {
        var repositoryRoot = FindRepositoryRoot();
        var nativeCliPath = Environment.GetEnvironmentVariable("COVERAGE_REPORTER_CLI_PATH");
        var startInfo = new ProcessStartInfo(string.IsNullOrWhiteSpace(nativeCliPath) ? "dotnet" : nativeCliPath)
        {
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        if (string.IsNullOrWhiteSpace(nativeCliPath))
        {
            startInfo.ArgumentList.Add("run");
            startInfo.ArgumentList.Add("--no-build");
            startInfo.ArgumentList.Add("--project");
            startInfo.ArgumentList.Add(Path.Combine(repositoryRoot, "src", "CoverageReporter.Cli", "CoverageReporter.Cli.csproj"));
            startInfo.ArgumentList.Add("--");
        }

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(60));
        return new CommandResult(process.ExitCode, await standardOutput, await standardError);
    }

    private static async Task<CommandResult> RunStandaloneAsync(string standaloneFile, string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        startInfo.Environment["DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER"] = "1";
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--disable-build-servers");
        startInfo.ArgumentList.Add("--file");
        startInfo.ArgumentList.Add(standaloneFile);
        startInfo.ArgumentList.Add("--");
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(120));
        return new CommandResult(process.ExitCode, await standardOutput, await standardError);
    }

    private static string NormalizeGeneratedTimestamp(string markdown)
    {
        const string prefix = "| Report generated (UTC) |";
        return string.Join('\n', markdown.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Select(line => line.StartsWith(prefix, StringComparison.Ordinal)
                ? "| Report generated (UTC) | <timestamp> |"
                : line));
    }

    private static async Task ShutdownBuildServersAsync(string workingDirectory)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("build-server");
        startInfo.ArgumentList.Add("shutdown");
        using var process = Process.Start(startInfo)!;
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        await standardOutput;
        await standardError;
    }

    private static async Task DeleteTempDirectoryAsync(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        for (var attempt = 0; ; attempt++)
        {
            try
            {
                Directory.Delete(directory, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 20)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AdventOfCode.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not find the repository root for the CLI integration tests.");
    }

    private static string CreateTempDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"CoverageReporter [{Guid.NewGuid():N}]");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private sealed record CommandResult(int ExitCode, string StandardOutput, string StandardError);
}