# CoverageReporter

CoverageReporter creates an easy-to-browse HTML or Markdown summary from Cobertura XML, including output from `Microsoft.Testing.Extensions.CodeCoverage`. It uses a compact overview, searchable class list, expandable source view, and responsive light/dark styling inspired by the Microsoft.Testing.Extensions.HtmlReport.

## Generate a coverage report

Ask the Microsoft coverage extension for Cobertura XML, then pass that file to CoverageReporter:

```powershell
dotnet test -- --coverage --coverage-output-format cobertura
$coverage = Get-ChildItem .\TestResults -Filter *.cobertura.xml -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
dotnet run --project .\src\CoverageReporter.Cli -- $coverage
```

HTML is the default output and is written beside the input using the same base filename. Use `--format markdown` (or `-f markdown`) to write Markdown, `--output path` (or `-o path`) to choose an output file, and `--source-root path` (or `-s path`) when the source paths in the Cobertura document need a repository-root prefix. The CLI provides `--help`/`-h` and displays a line/branch summary after writing a report. Source files are optional; the report still displays class metrics when a file is unavailable. Both formats group classes by inferred namespace; because Cobertura provides qualified class names rather than namespace metadata, nested types are assigned to a known enclosing class's namespace when that enclosing class is present. In HTML, expand or collapse namespaces, or search for a namespace, file, or class. Namespace and overall summary metrics include all classes, even while filtering or when generated rows are hidden. The HTML class list hides C# compiler-generated lambda/closure and generated-regex classes by default; enable **Show generated classes** to include them. HTML percentages are red below 50%, amber from 50% to below 80%, and green at 80% or above; unavailable percentages remain neutral.

## Run as a single file (.NET 10)

`CoverageReporter.cs` is a self-contained .NET 10 file-based app for when you want the tool without building or referencing its project structure. It includes the parser, both report formats, and the HTML assets; the existing modular projects and CLI remain available. The .NET 10 SDK restores the two package dependencies on first run.

```powershell
dotnet run --file .\CoverageReporter.cs -- $coverage
dotnet run --file .\CoverageReporter.cs -- $coverage --format markdown --output .\coverage-summary.md
```

Markdown output uses GitHub-flavored `<details>` sections for an expandable summary and per-namespace class tables. Group metrics are weighted from the classes in each namespace; generated classes are collected in their own collapsed subsection but remain included in namespace and overall totals. Line and branch cells include covered/total counts and color indicators using the same 50% and 80% thresholds as HTML; unavailable percentages display as `N/A`. The summary includes covered, uncovered, and coverable line and branch counts, class and distinct source-file counts, and the number of Cobertura packages when present. It displays the Cobertura timestamp and report-generation time in UTC; Cobertura package counts are not presented as assembly counts, and physical source-line and method totals are not available in the Cobertura summary.

```powershell
dotnet run --project .\src\CoverageReporter.Cli -- $coverage --format markdown --output .\coverage-summary.md
```

The CLI consumes Cobertura XML. Binary `.coverage` artifacts must first be converted to Cobertura with the Microsoft coverage tools.

## Publish a native executable

CoverageReporter.Cli supports Native AOT. Publish for the target runtime identifier (RID); for example, on Windows x64 with the .NET SDK and the Visual C++ build tools installed:

```powershell
dotnet publish .\src\CoverageReporter.Cli\CoverageReporter.Cli.csproj -c Release -r win-x64
```

Run the published executable directly:

```powershell
.\src\CoverageReporter.Cli\bin\Release\net11.0\win-x64\publish\CoverageReporter.Cli.exe $coverage
```

Native AOT output is platform-specific, so publish separately for each target RID. To run the CLI integration tests against the native executable:

```powershell
$env:COVERAGE_REPORTER_CLI_PATH = (Resolve-Path .\src\CoverageReporter.Cli\bin\Release\net11.0\win-x64\publish\CoverageReporter.Cli.exe).Path
dotnet test .\tests\CoverageReporter.Cli.Tests\CoverageReporter.Cli.Tests.csproj
```