using System.Text;
using CoverageReporter.MicrosoftCodeCoverage;

namespace CoverageReporter.MicrosoftCodeCoverage.Tests;

public class CoberturaCoverageReaderTests
{
    private const string Cobertura = """
        <?xml version="1.0"?>
        <coverage name="sample" timestamp="1794708667" lines-covered="2" lines-valid="3" branches-covered="1" branches-valid="2">
          <sources><source>src</source></sources>
          <packages><package name="Sample.Package"><classes>
            <class name="Sample.Worker" filename="Worker.cs">
              <methods>
                <method name="First"><lines><line number="1" hits="1"/><line number="2" hits="0" branch="true" condition-coverage="50% (1/2)"/></lines></method>
                <method name="Second"><lines><line number="1" hits="1"/></lines></method>
              </methods>
              <lines><line number="1" hits="1"/><line number="2" hits="0" branch="true" condition-coverage="50% (1/2)"/><line number="3" hits="2"/></lines>
            </class>
          </classes></package></packages>
        </coverage>
        """;

    [Test]
    public void ReadsRootMetricsAndAggregatesDuplicateMethodLines()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Cobertura));

        var report = new CoberturaCoverageReader().Read(stream, reportName: "fallback");

        Assert.Equals(report.Name, "sample");
        Assert.Equals(report.Summary.LinesCovered, 2);
        Assert.Equals(report.Summary.LinesValid, 3);
        Assert.Equals(report.Summary.BranchesCovered, 1);
        Assert.Equals(report.Classes[0].Metrics.LinesValid, 3);
        Assert.Equals(report.Classes[0].Lines.Count, 3);
        Assert.Equals(report.PackageCount, 1);
        Assert.Equals(report.CoverageTimestamp, DateTimeOffset.FromUnixTimeSeconds(1794708667));
    }

    [Test]
    public void MissingPackageAndInvalidOrOutOfRangeTimestampMetadataRemainUnavailable()
    {
        const string xml = "<coverage timestamp=\"invalid\"><classes /></coverage>";
        const string missingTimestampXml = "<coverage><packages /></coverage>";
        const string outOfRangeTimestampXml = "<coverage timestamp=\"9223372036854775807\"><packages /></coverage>";
        using var missingMetadataStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var missingTimestampStream = new MemoryStream(Encoding.UTF8.GetBytes(missingTimestampXml));
        using var outOfRangeStream = new MemoryStream(Encoding.UTF8.GetBytes(outOfRangeTimestampXml));

        var missingMetadataReport = new CoberturaCoverageReader().Read(missingMetadataStream);
        var missingTimestampReport = new CoberturaCoverageReader().Read(missingTimestampStream);
        var outOfRangeReport = new CoberturaCoverageReader().Read(outOfRangeStream);

        Assert.Equals(missingMetadataReport.PackageCount, null);
        Assert.Equals(missingMetadataReport.CoverageTimestamp, null);
        Assert.Equals(missingTimestampReport.PackageCount, 0);
        Assert.Equals(missingTimestampReport.CoverageTimestamp, null);
        Assert.Equals(outOfRangeReport.PackageCount, 0);
        Assert.Equals(outOfRangeReport.CoverageTimestamp, null);
    }

    [Test]
    public void IgnoresMethodLinesWhenClassLineDataIsPresent()
    {
        const string xml = """
            <coverage><packages><package><classes>
              <class name="Sample.Worker" filename="Worker.cs">
                <methods><method name="Run"><lines><line number="99" hits="1"/></lines></method></methods>
                <lines><line number="1" hits="0"/></lines>
              </class>
            </classes></package></packages></coverage>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var report = new CoberturaCoverageReader().Read(stream);

        Assert.Equals(report.Classes[0].Metrics.LinesValid, 1);
        Assert.Equals(report.Classes[0].Lines[0].Number, 1);
        Assert.Equals(report.Classes[0].Metrics.LinesCovered, 0);
    }

    [Test]
    public void ReadsSourceAndConditionCoverageFromCoberturaFile()
    {
        var sourceRoot = Path.Combine(Path.GetTempPath(), $"coverage-reporter-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(sourceRoot, "src"));
        File.WriteAllText(Path.Combine(sourceRoot, "src", "Worker.cs"), "first\nsecond\nthird");

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Cobertura));
            var report = new CoberturaCoverageReader().Read(stream, sourceRoot);

            Assert.Equals(report.Classes[0].SourceCode, "first\nsecond\nthird");
            Assert.Equals(report.Classes[0].Metrics.BranchesCovered, 1);
            Assert.Equals(report.Classes[0].Metrics.BranchesValid, 2);
        }
        finally
        {
            Directory.Delete(sourceRoot, recursive: true);
        }
    }

    [Test]
    public void RejectsMalformedXmlWithAnActionableDataError()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<coverage>"));
        var threwExpectedException = false;

        try
        {
            new CoberturaCoverageReader().Read(stream);
        }
        catch (InvalidDataException)
        {
            threwExpectedException = true;
        }

        Assert.Equals(threwExpectedException, true);
    }
}