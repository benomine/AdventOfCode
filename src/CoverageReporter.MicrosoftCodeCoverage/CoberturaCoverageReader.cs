using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using CoverageReporter.Core;

namespace CoverageReporter.MicrosoftCodeCoverage;

public sealed class CoberturaCoverageReader
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